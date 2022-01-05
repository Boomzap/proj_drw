using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using Boomzap.Character;

namespace Boomzap.Conversation
{
    public class ConversationNode : Node
    {
        public enum VideoMode
        {
            Background,
            LoopingBackground,
            Exclusive,
            FadeExclusive
        }

        public enum NodeStyle
        {
            Dialog,
            Narration,
            Monologue
        }

        [Serializable]
        public class CharacterStateDefinition
        {
            public Character.CharacterInfo character;
            
            public string state;
            public string eyes;
            public string emotion;

            public bool faceLeft;
            public bool lookBack;

            public List<CharacterSpineSlot.SlotToggle> spineSlots;

            // if this is true, we ignore everything else set in here and use it only for reference purposes
            public bool noChangeFromParent = true;

            public CharacterStateDefinition Clone()
            {
                CharacterStateDefinition clone = new CharacterStateDefinition();

                clone.state = state;
                clone.character = character;
                clone.eyes = eyes;
                clone.faceLeft = faceLeft;
                clone.lookBack = lookBack;
                clone.emotion = emotion;
                clone.spineSlots = spineSlots;

                return clone;
            }

            public void CloneFrom(CharacterStateDefinition other)
            {
                this.state = other.state;
                this.character = other.character;
                this.emotion = other.emotion;
                this.lookBack = other.lookBack;
                this.faceLeft = other.faceLeft;
                this.eyes = other.eyes;
                this.spineSlots = other.spineSlots;
            }
        }

        [SerializeField, HideInInspector] protected List<ConversationNode> linkParents = new List<ConversationNode>();

        [HideInInspector] public NodeStyle nodeStyle = NodeStyle.Dialog;
        [HideInInspector] public string emotionPrefix = "";
        [HideInInspector] public Color emotionColor;
        [HideInInspector] public AudioClip triggerSoundEffect;

        //[SerializeField, HideInInspector] protected SpineAnim fullscreenSpine = null;
        [HideInInspector] public Texture2D fullScreenImage = null;
        [HideInInspector] public Texture2D forceBG = null;
        //[SerializeField, HideInInspector] protected VideoClip fullScreenVideo = null;
        [HideInInspector] public bool fullScreenLandscape = false;
        [HideInInspector] public VideoMode applyVideoMode = VideoMode.Background;

        //[SerializeField, HideInInspector] protected GroupCharacter groupCharacter;
        [HideInInspector] public int groupCharacterSlot;
        [HideInInspector] public string groupCharacterState;
        [HideInInspector] public bool flipGroupCharacter;

        [HideInInspector] public bool repeatable = true;

        [HideInInspector] public string[] setFlags = new string[0];
        [HideInInspector] public string flagCondition = "";

        [HideInInspector] public string background;
        [HideInInspector] public TransitionStyle transitionStyle = TransitionStyle.None;

        public Character.CharacterInfo speakingCharacter;
        public string speakingCharacterName;
        public string overrideSpeakingCharacter;

        public CharacterStateDefinition[] characters = new CharacterStateDefinition[ConversationManager.MaxCharacterSlots];

        public bool IsUsedAsLink => linkParents.Count > 0;
        public List<ConversationNode> LinkParents { get => linkParents; }

        #if UNITY_EDITOR
        [HideInInspector] public string comments;
        #endif

        public static ConversationNode Create(Conversation owner)
        {
            ConversationNode result = ScriptableObject.CreateInstance<ConversationNode>();
            result.Initialize(owner);
            return result;
        }

        protected override void Initialize(Conversation owner)
        {
            base.Initialize(owner);

            //text = "---";
            //Debug.Log($"New ConversationNode: {guid.ToStringHex()}");
        }

        public void GetAllNodes(List<ConversationNode> outList)
        {
            outList.Add(this);

            for (int i = 0; i < NumChildren(); i++)
            {
                if (IsLink(i)) continue;

                var c = GetChild(i) as ConversationNode;

                if (outList.Any(x => x.guid == c.guid)) continue;

                c.GetAllNodes(outList);
            }
        }

        protected override bool IsChildTypeValid(Node n)
        {
            return true;
        }

        protected virtual void CopyFrom(ConversationNode source)
        {
        #if UNITY_EDITOR
            this.comments = source.comments;
        #endif
            this.nodeStyle = source.nodeStyle;
            this.emotionColor = source.emotionColor;
            this.emotionPrefix = source.emotionPrefix;
            this.triggerSoundEffect = source.triggerSoundEffect;
            this._guid = new SerializableGUID();
            this.speakingCharacter = source.speakingCharacter;
            this.speakingCharacterName = source.speakingCharacterName;
            this.overrideSpeakingCharacter = source.overrideSpeakingCharacter;
            this.fullScreenImage = source.fullScreenImage;
            this.fullScreenLandscape = source.fullScreenLandscape;
            //this.fullScreenVideo = source.fullScreenVideo;
            //this.fullScreenSpine = source.fullScreenSpine;
            //this.groupCharacter = source.groupCharacter;
            this.groupCharacterSlot = source.groupCharacterSlot;
            this.groupCharacterState = source.groupCharacterState;
            this.flipGroupCharacter = source.flipGroupCharacter;
            this.setFlags = new string[source.setFlags.Length];
            Array.Copy(source.setFlags, this.setFlags, source.setFlags.Length);
            this.flagCondition = source.flagCondition;

            this.characters = source.characters.Select(x => x.Clone()).ToArray();
            this.background = source.background;

            text = source.text;
        }

        static ConversationNode CreateCopy(ConversationNode source)
        {
            ConversationNode result = Create(source.owner);
            result.CopyFrom(source);

            return result;
        }

        static ConversationNode CreateDeepCopy(ConversationNode source)
        {
            ConversationNode result = CreateCopy(source);

            Stack<ConversationNode> originals   = new Stack<ConversationNode>();
            Stack<ConversationNode> copies      = new Stack<ConversationNode>();

            originals.Push(source);
            copies.Push(source);

            while (copies.Count > 0)
            {
            ConversationNode currentOriginal = originals.Pop();
            ConversationNode currentCopy     = copies.Pop();

            for (int i = 0; i < currentOriginal.NumChildren(); ++i)
            {
                bool isLink = currentOriginal.IsLink(i);

                ConversationNode childOriginal = currentOriginal.GetChild(i) as ConversationNode;

                if (!isLink)
                {
                    ConversationNode childCopy = CreateCopy(childOriginal);

                    currentCopy.AddChild(childCopy);

                    originals.Push(childOriginal);
                    copies.Push(childCopy);
                }
                else currentCopy.AddLink(childOriginal);
            }
            }

            return result;
        }

        public void AddLink(Node _child)
        {
            if (!IsChildTypeValid(_child)) return;
            var child = _child as ConversationNode;

            child.linkParents.Add(this);
            connections.Add(new Connection(child, true));
        }

        public void RemoveLink(Node _child)
        {
            if (!IsChildTypeValid(_child)) return;
            var child = _child as ConversationNode;

            int index = connections.FindIndex(x => x.Target == child && x.IsLink);
            if (index < connections.Count && index >= 0)
                connections.RemoveAt(index);
            child.linkParents.Remove(this);
        }

        public void AddLinkAt(Node _child, int idx)
        {
            if (!IsChildTypeValid(_child)) return;
            var child = _child as ConversationNode;

            child.linkParents.Add(this);
            connections.Insert(idx, new Connection(child, true));
        }
        public string text
        {
            get {
                if (string.IsNullOrEmpty(flagCondition) == false)
                    return ConversationManager.instance.textProvider.GetText("Conversation/" + owner.name + "/" + flagCondition, this, !Application.isPlaying);

                return ConversationManager.instance.textProvider.GetText("Conversation/" + owner.name + "/" + guid.ToStringHex(), this, !Application.isPlaying); 
            }
            set { 
                //NOTE: Add Flag Condition as Key Name for Special Item Conversations
                //----  Also this only works for linear storyline. 
                //----  if Code below if storyline is dynamic.
                if(string.IsNullOrEmpty(flagCondition) == false)
                    ConversationManager.instance.textProvider.SetText("Conversation/" + owner.name + "/" + flagCondition, value); 
                else
                    ConversationManager.instance.textProvider.SetText("Conversation/" + owner.name + "/" + guid.ToStringHex(), value);
            }
        }
    }
}

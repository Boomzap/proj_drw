using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boomzap.Conversation
{
    public enum TransitionStyle
    {
        None,
        CrossFade
    }


    [ExecuteInEditMode]
    public class ConversationManager : SimpleSingleton<ConversationManager>, ITextProvider
    {
        public const int MaxCharacterSlots = 5;

        public delegate void OnOptionSelectedHandler();
        public event OnOptionSelectedHandler OnOptionSelected;

        public delegate void OnConversationStartedHandler();
        public event OnConversationStartedHandler OnConversationStarted;

        public delegate void OnConversationEndedHandler();
        public event OnConversationEndedHandler OnConversationEnded;

        public delegate void OnConversationDisposedHandler();
        public event OnConversationDisposedHandler OnConversationDisposed;

        public delegate void OnMarkConversationUsedHandler(SerializableGUID guid);
        public event OnMarkConversationUsedHandler OnMarkConversationUsed;

        public delegate void OnMarkNodeUsedHandler(SerializableGUID guid);
        public event OnMarkNodeUsedHandler OnMarkNodeUsed;

        public delegate void OnSetFlagHandler(string flag);
        public event OnSetFlagHandler OnSetFlag;

        internal ValueDropdownList<string>    TextProviders
        {
            get
            {
                ValueDropdownList<string> _textProviders = new ValueDropdownList<string>();

                var assembly = typeof(ITextProvider).Assembly;

                foreach (var tn in assembly.GetTypes().Where(x => x.IsClass && x.GetInterface("ITextProvider") != null))
                {
                    if (tn == typeof(ConversationManager))
                    {
                        continue;
                    } else
                    {
                        _textProviders.Add(new ValueDropdownItem<string>(tn.FullName, tn.FullName));
                    }
                }                

                return _textProviders;
            }
        }

        public ITextProvider   textProvider = null;

        [ReadOnly]  public Conversation[] conversations = new Conversation[0];

        [ValueDropdown("TextProviders"), LabelText("Localization Provider")]
        public string textProviderClassName;

        public Conversation CurrentConversation { get; private set; }
        public ConversationProgress CurrentConversationProgress { get; private set; }
        public bool CanClose { get; set; } = true;
        public bool InConversation => CurrentConversation != null;

        ConversationNode    currentNode = null;
        public ConversationNode CurrentNode => CurrentConversationProgress?.currentNode ?? null;
        public void ClearCurrentNode() { currentNode = null; }

        Func<ConversationNode, bool> IsNodeValid;

        static public bool dontSetUsedFlags = false;

        public bool StartConversation(Conversation conversation, Func<ConversationNode, bool> nodeValidityChecker)
        {
            if (InConversation)
            {
                Debug.Log($"Attempted to start conversation {conversation.name} while a conversation is already active ({CurrentConversation.name})");
                return false;
            }

            IsNodeValid = nodeValidityChecker;

            if (!dontSetUsedFlags)
                OnMarkConversationUsed?.Invoke(conversation.guid);
            CurrentConversation = conversation;
            CurrentConversationProgress = Advance(CurrentConversation.root);
            ClearCurrentNode();

            OnConversationStarted?.Invoke();
            OnOptionSelected?.Invoke();

            return true;
        }

        public void EndConversation()
        {
            if (!InConversation)
            {
                Debug.Log("Attempted to end conversation while no conversation is active");
                return;
            }

            if (CanClose)
            {
                OnConversationEnded?.Invoke();
                DisposeConversation();
            }
        }

        

        public void DisposeConversation()
        {
            dontSetUsedFlags = false;

            CurrentConversation = null;
            CurrentConversationProgress = null;
            IsNodeValid = null;

            OnConversationDisposed?.Invoke();
        }

        public bool WouldConversationPlay(Conversation c, Func<ConversationNode, bool> nodeValidityChecker)
        {
            //Evaluate Each Conversation Node 
            for (int i = 0; i < c.root.NumChildren(); i++)
            {
                ConversationNode node = c.root.GetChild(i) as ConversationNode;

                //Runs GameController.instance.EvaluateConversationNode
                if (nodeValidityChecker(node))
                {
                    Debug.Log($" Valid Node Text:{node.text}");
                    return true;
                }
                    
            }

            return false;
        }

        ConversationProgress ProcessOptions(Node node)
        {
            ConversationNode tNode = null;
            
            List<ConversationNode> validNodes = new List<ConversationNode>();
            List<string> processedTexts = new List<string>();
            string tNodeText = "";
            currentNode = node as ConversationNode;

            for (int i = 0; i < node.NumChildren(); i++)
            {
                ConversationNode child = node.GetChild(i) as ConversationNode;

                if (IsNodeValid(child))
                {
                    tNode = child;
                    tNodeText = child.text;
                // tNode.TriggerAction( )

                    break;
                }
            }

            if (tNode == null) return null;

            if (!dontSetUsedFlags)
            {
                OnMarkNodeUsed?.Invoke(tNode.guid);
                foreach (string flag in tNode.setFlags)
                    OnSetFlag?.Invoke(flag);
            }

            for (int i = 0; i < tNode.NumChildren(); i++)
            {
                ConversationNode child = tNode.GetChild(i) as ConversationNode;

                // check if child is valid..
                if (!IsNodeValid(child)) continue;

                if (!child.isOption)
                {
                    // if we came across a valid dialog instead of options.. just do that instead
                    validNodes.Clear();
                    processedTexts.Clear();
                }

                validNodes.Add(child);
                processedTexts.Add(child.text);

                // add rest of options if these are actually options, otherwise we stop as soon as we have a valid succeeding line
                if (!child.isOption)
                    break;
            }

            return new ConversationProgress(tNode, validNodes, tNodeText, processedTexts);
        }
        
        public ConversationProgress Advance(Node node)
        {
            if (node is RootNode || node.isOption)
                return ProcessOptions(node);

            ConversationNode tNode = node as ConversationNode;

            // trigger Narration actions here
            if (tNode && tNode.nodeStyle == ConversationNode.NodeStyle.Narration)
            {
                // tNode.TriggerAction( )
            }

            List<ConversationNode> validNodes = new List<ConversationNode>();
            List<string> processedTexts = new List<string>();

            for (int i = 0; i < node.NumChildren(); i++)
            {
                ConversationNode child = node.GetChild(i) as ConversationNode;
                
                if (IsNodeValid(child))
                {
                    validNodes.Add(child);
                    processedTexts.Add(child.text);
                    if (!child.isOption) break; // break on first valid dialog 
                }
            }

            return new ConversationProgress(tNode, validNodes, tNode.text, processedTexts);
        }

        public void SelectOption(int optionIndex)
        {
            if (CurrentConversationProgress == null || CurrentConversationProgress == null) return;

            if (optionIndex < 0 || optionIndex >= CurrentConversationProgress.validChildNodes.Count)
            {
                Debug.LogWarning("Invalid option index passed to conversation");
                return;
            }

            ConversationNode option = CurrentConversationProgress.validChildNodes[optionIndex];

            if (!dontSetUsedFlags)
            {
                OnMarkNodeUsed?.Invoke(option.guid);
                foreach (string flag in option.setFlags)
                    OnSetFlag?.Invoke(flag);
            }

            if (option.nodeStyle != ConversationNode.NodeStyle.Narration)
            {
                //option.TriggerAction( )
            }

            CurrentConversationProgress = Advance(option);

            if (CurrentConversationProgress != null)
            {
                OnOptionSelected?.Invoke();
            } else
            {
                EndConversation();
            }
        }

        [Button]
        void RefreshConversationList()
        {
            #if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:"+ typeof(Conversation).Name); 

            conversations = guids.Select(x => AssetDatabase.LoadAssetAtPath<Conversation>(AssetDatabase.GUIDToAssetPath(x))).ToArray();
            #endif
        }

        private void OnEnable()
        {
            RefreshConversationList();
        }

        private void Awake()
        {
            InitTextProvider();
        }

        public void InitTextProvider(string providerName = "")
        {
            if (!string.IsNullOrEmpty(providerName))
                textProviderClassName = providerName;

            if (string.IsNullOrEmpty(textProviderClassName))
            {
                Debug.Log("No ITextProvider has been set, conversations will not have valid text");
                textProvider = null;
            } else
            {
                textProvider = (ITextProvider)Activator.CreateInstance(Type.GetType(textProviderClassName));
                
                if (textProvider == null)
                {
                    Debug.LogError($"Specified ITextProvider '{textProviderClassName}' failed to instantiate");
                }
            }

            if (textProvider != null)
            {
                textProvider.Init();
            } else
            {
                textProvider = this;
            }
        }

        void ITextProvider.Init()
        {
            
        }

        void ITextProvider.SetText(string key, string text)
        {
            
        }

        string ITextProvider.GetText(string key, ConversationNode forNode, bool inEditorMode)
        {
            return key;
        }
    }
}

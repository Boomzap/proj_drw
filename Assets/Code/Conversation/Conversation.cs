using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Boomzap.Conversation
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Boomzap/Conversation", order = 1)]
    public class Conversation : ScriptableObject
    {
        [SerializeField, HideInInspector] RootNode  _root;
        public RootNode root
        {
            get
            {
                #if UNITY_EDITOR
                if (_root == null && UnityEditor.AssetDatabase.Contains(this)) _root = RootNode.Create(this);
                #endif

                return _root;
            }
        }


        [SerializeField, HideInInspector] CharacterInfo _speaker;
        public CharacterInfo speaker { get { return _speaker; } }

        [SerializeField] SerializableGUID _guid = new SerializableGUID();
        public SerializableGUID guid { get { return _guid; } }

        [HideInInspector]
        public bool repeatable = false;

        public int rootOptionsCount
        {
            get
            {
                if (root == null) return 0;
                //if (root.NumChildren() == 0) return 0;
                return root.NumChildren();
                
                /*for (int i = 0; i < root.NumChildren(); i++)
                {
                    
                }
                check valid root options
                */
            }
        }

        public bool hasRootOptions => rootOptionsCount > 0;

        public ConversationNode FindNode(System.Guid guid)
        {
            if (root == null) return null;
            List<System.Guid> checkedGuids = new List<System.Guid>();

            for (int i = 0; i < root.NumChildren(); i++)
            {
                ConversationNode n = root.GetChild(i) as ConversationNode;
                if (n == null) continue;
                ConversationNode f = FindNode(n, guid, checkedGuids);
                if (f != null) return f;
            }

            return null;
        }

        public ConversationNode FindNode(ConversationNode inNode, System.Guid guid, List<System.Guid> checkedGuids)
        {
            if (inNode == null) return null;
            if (inNode.guid == guid) return inNode;
            if (checkedGuids.Contains(inNode.guid)) return null;
            checkedGuids.Add(inNode.guid);

            for (int i = 0; i < inNode.NumChildren(); i++)
            {
                ConversationNode n = inNode.GetChild(i) as ConversationNode;
                if (n == null) continue;
                ConversationNode f = FindNode(n, guid, checkedGuids);
                if (f != null) return f;
            }

            return null;
        }

        public List<ConversationNode> GetAllNodes()
        {
            List<ConversationNode> nodes =  new List<ConversationNode>();

            for (int i = 0; i < root.NumChildren(); i++)
            {
                if (root.IsLink(i)) continue;
                var c = root.GetChild(i) as ConversationNode;
                if (nodes.Any(x => x.guid == c.guid)) continue;
                c.GetAllNodes(nodes);
            }

            return nodes;
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button] void RegenerateNewGuid()
        {
            _guid = new SerializableGUID();
        }
#endif
    }
}

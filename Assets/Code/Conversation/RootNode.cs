using System;

using UnityEngine;

namespace Boomzap.Conversation
{
    [Serializable]
    public class RootNode : Node
    {
        public static RootNode Create(Conversation owner)
        {
            RootNode result = ScriptableObject.CreateInstance<RootNode>();
            result.Initialize(owner);
            return result;
        }

        protected override bool IsChildTypeValid(Node node)
        {
            return node is ConversationNode;
        }
    }
}

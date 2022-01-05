using UnityEngine;
using System.Collections.Generic;

namespace Boomzap.Conversation
{
    public class ConversationProgress
    {
        public ConversationNode currentNode;
        public List<ConversationNode> validChildNodes;

        public string processedDialogText;
        public List<string> processedChildTexts;

        public ConversationProgress(ConversationNode node, List<ConversationNode> childNodes, string dialogText, List<string> childTexts)
        {
            currentNode = node;
            validChildNodes = childNodes;
            processedDialogText = dialogText;
            processedChildTexts = childTexts;
        }
    }
}

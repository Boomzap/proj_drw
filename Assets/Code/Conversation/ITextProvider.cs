using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boomzap.Conversation
{
    public interface ITextProvider
    {
        public void Init();
        public string GetText(string key, ConversationNode forNode, bool inEditorMode);
        public void SetText(string key, string text);
    }
}

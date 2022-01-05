using UnityEngine;

namespace ho
{
    public class ConversationTextProvider : Boomzap.Conversation.ITextProvider
    {
        public void Init()
        {

        }

        public string GetText(string key, Boomzap.Conversation.ConversationNode forNode, bool inEditorMode)
        {
            /*if (!string.IsNullOrWhiteSpace(forNode.emotionPrefix))
            {
                text = "<color=#"+ ColorUtility.ToHtmlStringRGBA(forNode.emotionColor) + ">(" + forNode.emotionPrefix + ")</color> " + text;
            }*/

            return LocalizationUtil.FindLocalizationEntry(key, "", false, TableCategory.Conversation);
        }

        public void SetText(string key, string value)
        {
#if UNITY_EDITOR

            LocalizationUtil.UpdateLocalizationEntry(key, value, false, TableCategory.Conversation);
            //LocalizationManager.InitializeIfNeeded();
            //var termData = LocalizationManager.Sources[0].GetTermData(key, false);
            //if (termData == null)
            //{
            //    termData = LocalizationManager.Sources[0].AddTerm(key);

            //    termData.Languages[0] = value;
            //} else
            //{
            //    termData.Languages[0] = value;
            //}

            ////UnityEditor.EditorUtility.SetDirty(LocalizationManager.Sources[0]);
            //LocalizationManager.Sources[0].Editor_SetDirty();
        #endif
        }

    }
}

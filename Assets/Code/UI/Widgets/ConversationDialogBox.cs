using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class ConversationDialogBox : MonoBehaviour
    {
        [SerializeField]
        RectTransform   speakerBox;
        [SerializeField]
        TextMeshProUGUI speakerText;
        [SerializeField]
        RectTransform   dialogBackground;
        [SerializeField]
        TextMeshProUGUI dialogText;

        int maxChars = 0;
        int curChars = 0;
        float showStartTime = 0f;

        [SerializeField]
        float characterRevealRate = 100f;

        public bool IsDoneShowing => curChars >= maxChars;
        public bool IsTalkTimerDone => Time.time >= talkDoneTime;

        public float showDoneTime = 0f;
        public float talkDoneTime = 0f;

        public DialogShow dialogType = DialogShow.Instant;

        public enum DialogShow
        {
            Instant,
            WordPerWord
        }

        [Button]
        void ResizeToText()
        {
            bool showSpeaker = !string.IsNullOrWhiteSpace(speakerText.text);

            speakerBox.gameObject.SetActive(showSpeaker);

            if (showSpeaker)
            {
                speakerText.ForceMeshUpdate();
                float nameWidth = speakerText.textBounds.size.x;
                float nameOffset = speakerText.rectTransform.offsetMin.x + (speakerText.rectTransform.offsetMax.x - speakerText.rectTransform.sizeDelta.x);
                speakerBox.sizeDelta = new Vector2(nameOffset + nameWidth, speakerBox.sizeDelta.y);
            }

            bool showDialog = !string.IsNullOrWhiteSpace(dialogText.text);

            dialogBackground.gameObject.SetActive(showDialog);

            if (showDialog)
            {
                dialogText.maxVisibleCharacters = int.MaxValue;
                dialogText.ForceMeshUpdate();
                float linesHeight = dialogText.textBounds.size.y;
                float linesOffset = dialogText.rectTransform.offsetMin.y + (dialogText.rectTransform.offsetMax.y - dialogText.rectTransform.sizeDelta.y);
                dialogBackground.sizeDelta = new Vector2(dialogBackground.sizeDelta.x, linesOffset + linesHeight);

                if(dialogType == DialogShow.Instant)
                    FinishInstantly();
            } else
            {
                FinishInstantly();
            }
            
        }

        public void FinishInstantly()
        {
            curChars = int.MaxValue;
            showDoneTime = Time.time;
            dialogText.maxVisibleCharacters = curChars;
        }

        public void Setup(string dialog, string speaker)
        {
            dialogText.text = dialog;
            speakerText.text = speaker;

            ResizeToText();

           //dialogText.maxVisibleCharacters = 0;
            maxChars = dialog.Length;
            curChars = 0;

            showStartTime = Time.time;
            talkDoneTime = Time.time + (dialog.Count(x => x == ' ') + 1) * 0.3f;
        }

        private void Update()
        {
            if (dialogType == DialogShow.Instant) return;

            if (!IsDoneShowing)
            {
                float t = Time.time - showStartTime;

                curChars = (int)(t * characterRevealRate);

                if (curChars > maxChars)
                {
                    curChars = maxChars;
                    showDoneTime = Time.time;
                }

                dialogText.maxVisibleCharacters = curChars;
            }

        }
    }
}
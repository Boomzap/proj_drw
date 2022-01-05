using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace ho
{
    public enum PrompType
    {
        Info, //Close Button
        Options //Yes No
    }
    public class GenericPromptPopup : Popup
    {
        [SerializeField] TextMeshProUGUI headerText;
        [SerializeField] TextMeshProUGUI promptText;
        [SerializeField] Button closeButton;
        [SerializeField] GameObject optionsPrompt;
        [SerializeField] Button yesButton;
        [SerializeField] Button noButton;
        
        public bool isConfirmed = true;

        protected override void OnBeginShow(bool instant)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());

            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() => OnConfirm());

            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() => OnClose());
        }

        public void Setup(string header, string prompt, PrompType prompType = PrompType.Info)
        {
            switch (prompType)
            {
                case PrompType.Info:
                    {
                        optionsPrompt.SetActive(false);
                        closeButton.gameObject.SetActive(true);
                        break;
                    }
                case PrompType.Options:
                    {
                        optionsPrompt.SetActive(true);
                        closeButton.gameObject.SetActive(false);
                        break;
                    }
            }
                        
            headerText.text = header;
            promptText.text = prompt;
        }

        void OnConfirm()
        {
            isConfirmed = true;
            Hide();
        }

        void OnClose()
        {
            isConfirmed = false;
            Hide();
        }
    }
}

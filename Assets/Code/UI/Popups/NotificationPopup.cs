using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class NotificationPopup : Popup
    {
        [SerializeField] TextMeshProUGUI headerText;
        [SerializeField] TextMeshProUGUI messageText;
        [SerializeField] Button closeButton;

        protected override void OnBeginShow(bool instant)
        {
            base.OnBeginShow(instant);

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());
        }

        public void SetupPopup(string header, string message)
        {
            headerText.text = header;
            messageText.text = message;
        }
    }
}

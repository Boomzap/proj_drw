using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class CheckoutPopup : Popup
    {
        protected override bool UseBlackout => false;

        [SerializeField] Button yesButton;
        [SerializeField] Button noButton;

        protected override void OnBeginShow(bool instant)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() => Hide());

            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() => Hide());
        }
    }
}

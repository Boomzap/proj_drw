using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class SouvenirEntry : MonoBehaviour
    {
        [SerializeField] Button buyButton;

        private void Awake()
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => OnBuyItem());
        }

        public void OnBuyItem()
        {
            Popup.ShowPopup<CheckoutPopup>();
        }
    }
}

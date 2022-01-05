using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class CreditsUI : BaseUI
    {
        [SerializeField] Button closeButton;
        [SerializeField] Credits credits;

        void Awake()
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(credits.HideCredits);
        }

        protected override void OnBeginShow(bool instant)
        {
            credits.gameObject.SetActive(true);
            base.OnBeginShow(instant);
        }

        public Credits Credits { get { return credits; } }

    }
}


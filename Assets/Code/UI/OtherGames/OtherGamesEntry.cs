using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class OtherGamesEntry : MonoBehaviour
    {
        public string osxLink;
        public string winLink;

        public Button entryButton;
        public Image entryImage;

        private void Awake()
        {
            entryButton.onClick.RemoveAllListeners();
            entryButton.onClick.AddListener(OnClick);
        }


        public void OnClick()
        {
#if UNITY_STANDALONE_OSX
            Application.OpenURL(osxLink);
#elif UNITY_STANDALONE_WIN
            Application.OpenURL(winLink);
#endif
        }
    }

}

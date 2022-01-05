using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class TabletUI : BaseUI
    {
        [SerializeField] Image screenOffImage;
        [SerializeField] float screenOffDelay;

        public void FadeOutScreen()
        {
            screenOffImage.CrossFadeAlpha(0, screenOffDelay, false);
        }
    }
}



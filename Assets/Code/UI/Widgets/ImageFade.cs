using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class ImageFade : MonoBehaviour
    {
        Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }
        public void FadeImage(bool fadeIn = false)
        {
            float targetAlpha = fadeIn ? 1f : 0.3f;
            image.CrossFadeAlpha(targetAlpha, .3f, false);
        }
    }
}


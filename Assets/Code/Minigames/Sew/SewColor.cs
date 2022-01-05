using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ho
{
    public class SewColor : BaseColor
    {
        public Sprite currentColorSprite = null;

        public Sprite sprite => colorImage.sprite;

        public void SetColor(Sprite colorSprite)
        {
            colorImage.sprite = colorSprite;
            currentColorSprite = colorSprite;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ho
{
    public class PaintColor : BaseColor
    {
        [SerializeField] TextMeshProUGUI colorKeyText;

        PaintMG.ColorData currentColor;

        public PaintMG.ColorData GetColorData()
        {
            return currentColor;
        }
       
        [Sirenix.OdinInspector.Button]
        public void SetColorPalette(PaintMG.ColorData color)
        {
            currentColor = color;
            colorImage.color = currentColor.color;
            colorKeyText.text = currentColor.colorKey;

            Vector3 colorValues = new Vector3();
            Color.RGBToHSV(color.color, out colorValues.x, out colorValues.y, out colorValues.z);

            //Debug.Log(colorValues.z);
            colorValues.z = (colorValues.z + 0.5f) % 1f;

            //Debug.Log(colorValues.z);

            Color newColor = Color.HSVToRGB(colorValues.x, colorValues.y, colorValues.z);

            //Debug.Log($"{newColor.r} {color.color.r} ");
            colorKeyText.color = newColor;

            //colorKeyText.color = Color.black;
        }

    }
}

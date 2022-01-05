using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    public class PaintColorHolder : BaseColorHolder
    {
        [SerializeField] GameObject paintColorPrefab;

        List<PaintColor> paintColors = new List<PaintColor>();

        public PaintColor SelectedColor { get { return (PaintColor)selectedColor; } }

        //public Texture2D paintBrushCursor;

        public PaintMG paintMg;
        public void SetupPaintColors(List<PaintMG.ColorData> colors)
        {
            selectedColor = null;

            paintColors.ForEach(x => Destroy(x.gameObject));
            paintColors.Clear();

            foreach(var color in colors)
            {
                PaintColor paintColor = Instantiate(paintColorPrefab, transform).GetComponent<PaintColor>();
                paintColor.SetColorPalette(color);
                paintColors.Add(paintColor);
            }
        }
    }
}

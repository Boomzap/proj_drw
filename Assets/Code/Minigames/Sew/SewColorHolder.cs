using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    public class SewColorHolder : BaseColorHolder
    {
        [SerializeField] GameObject sewColorPrefab;

        List<SewColor> sewColors = new List<SewColor>();
        public SewColor SelectedColor { get { return (SewColor)selectedColor; } }

        public SewMG sewMG;
        public void SetupSewColors(List<Sprite> colors)
        {
            selectedColor = null;

            sewColors.ForEach(x => Destroy(x.gameObject));
            sewColors.Clear();

            foreach (var color in colors)
            {
                SewColor sewColor = Instantiate(sewColorPrefab, transform).GetComponent<SewColor>();
                sewColor.SetColor(color);
                sewColors.Add(sewColor);
            }
        }
    }
}

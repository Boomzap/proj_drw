using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class CarouselViewHolder : MonoBehaviour
    {
        [SerializeField] CarouselView[] carouselViews;

        [Button]
        public void CycleToLeft()
        {
            foreach(var view in carouselViews)
            {
                view.CycleCarousel(true);
                view.SetGraphicsColor();
            }
        }

        [Button]
        public void CycleToRight()
        {
            foreach (var view in carouselViews)
            {
                view.CycleCarousel(false);
                view.SetGraphicsColor();
            }
        }


        public void SetDefaultColors()
        {
            foreach (var view in carouselViews)
            {
                view.SetGraphicsColor();
            }
        }
    }

}

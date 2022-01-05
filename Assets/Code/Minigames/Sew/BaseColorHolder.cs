using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    public class BaseColorHolder : MonoBehaviour
    {
        public Transform selectionMark;
        protected BaseColor selectedColor = null;

        public virtual void SelectColor(BaseColor color)
        {
            Debug.Log($"{color.name}");
            if (selectedColor == null)
            {
                selectedColor = color;
            }
            else if (selectedColor == color)
            {
                selectedColor = null;
            }
            else if (selectedColor != color)
            {
                selectedColor = color;
            }

            if (selectedColor)
            {
                selectionMark.SetParent(selectedColor.transform, false);
                selectionMark.SetAsFirstSibling();
                selectionMark.gameObject.SetActive(true);
            }
            else
                selectionMark.gameObject.SetActive(false);
        }
    }

}

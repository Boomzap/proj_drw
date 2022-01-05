using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOItemHolderList : MonoBehaviour
    {
        public HOItemHolder[]   itemHolders;

        bool    isAnimatingOut = false;
        public bool             shouldAutoClose = false;

        private void OnEnable()
        {
            iTween.Stop(gameObject);
            isAnimatingOut = false;
            transform.localScale = Vector3.one;
            shouldAutoClose = false;
        }

        private void Update()
        {
            if (!isAnimatingOut && shouldAutoClose)
            {
                foreach (var holder in itemHolders)
                {
                    if (!holder.isEmpty)
                        return;
                }

                isAnimatingOut = true;

                iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeInBack"));
            }
        }

        public void ClearHolders()
        {
            foreach (var holder in itemHolders)
            {
                holder.Clear();
            }
        }
    }

}
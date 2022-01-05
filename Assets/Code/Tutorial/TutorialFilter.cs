using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Sirenix.OdinInspector;

namespace ho
{
    public class TutorialFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        [ShowInInspector]
        GameObject[] validTargets = new GameObject[0];

        private void OnEnable()
        {
            SDFHitZoneRegister.s_HitTestFilter = HitTestFilter;
        }

        private void OnDisable()
        {
            SDFHitZoneRegister.s_HitTestFilter = null;
        }

        // return 'false' to block input.
        bool HitTestFilter(GameObject go)
        {
            return validTargets.Contains(go);
        }

        // return 'true' to block input.
        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            foreach (var c in validTargets)
            {
                Graphic g = c.GetComponent<Graphic>();
                if (g == null) 
                    continue;
                if (!g.raycastTarget) 
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(g.rectTransform, sp, eventCamera, g.raycastPadding))
                    continue;

                ICanvasRaycastFilter f = c.GetComponentsInChildren<ICanvasRaycastFilter>().FirstOrDefault();
                if (f == null)
                    return false;
                
                if (f.IsRaycastLocationValid(sp, eventCamera))
                    return false;
            }

            return true;
        }
    }
}

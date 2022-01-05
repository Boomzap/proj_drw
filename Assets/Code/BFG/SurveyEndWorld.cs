using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

namespace ho
{
    public class SurveyEndWorld : MonoBehaviour, IWorldState
    {
        [ShowInInspector]
        SurveyScreenshot zoomedScreenShot;

        public static bool isZooming;

        SurveyScreenshot lastScreenShot;

        void Awake()
        {
            isZooming = false;
        }

        private void Update()
        {
            if(Input.GetMouseButtonUp(0) && isZooming == false)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit)
                {
                    zoomedScreenShot = hit.collider.GetComponent<SurveyScreenshot>();
                    if (zoomedScreenShot)
                    {
                        if (lastScreenShot == null)
                        {
                            lastScreenShot = zoomedScreenShot;
                            lastScreenShot.ZoomIn();
                        }
                        else
                        {
                            lastScreenShot.ZoomOut();
                            lastScreenShot = null;
                        }
                    }
                }
                else
                {
                    if(lastScreenShot)
                    {
                        lastScreenShot.ZoomOut();
                        lastScreenShot = null;
                    }
                }
            }
        }

        public void OnLeave()
        {
            StateCache.instance.UnloadSurveyEnd();
        }

        bool IWorldState.ShouldDestroyOnLeave()
        {
            return true;
        }
    }
}
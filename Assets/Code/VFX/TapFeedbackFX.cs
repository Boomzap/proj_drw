using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ho
{
    public class TapFeedbackFX : SimpleSingleton<TapFeedbackFX>
    {
        [SerializeField]
        ObjectPool pool;

        [CheckList("@ho.WorldStateHelper.WorldStates"), SerializeField]
        List<string> enableOnWorldState = new List<string>();

        IWorldState prevWorldState = null;
        bool enableFX = true;

        private void Update()
        {
            if (prevWorldState != GameController.instance.CurrentWorldState)
            {
                prevWorldState = GameController.instance.CurrentWorldState;


                enableFX = prevWorldState != null && enableOnWorldState.Contains(prevWorldState.GetType().Name);
            }

            if (!enableFX) return;


            if (Input.touchSupported)
            {
                var touches = Input.touches;
                foreach (var touch in touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        GameObject go = pool.Fetch();
                        
                        Vector3 touchPos = GameController.instance.currentCamera.ScreenToWorldPoint(touch.position);
                        go.transform.position = (Vector2)touchPos;
                    }
                }
            } else if (Input.GetMouseButtonDown(0))
            {
                GameObject go = pool.Fetch();

                Vector3 mousePos = GameController.instance.currentCamera.ScreenToWorldPoint(Input.mousePosition);
                go.transform.position = (Vector2)mousePos;
            }
        }

        public void CreateAtWorldPos(Vector3 pos)
        {
            GameObject go = pool.Fetch();
            go.transform.position = pos;
        }
    }
}

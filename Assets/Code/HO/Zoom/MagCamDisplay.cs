using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class MagCamDisplay : MonoBehaviour
    {
        public RawImage rawImage;
        public Canvas canvas => GetCanvas();
        public MagCam cam;

        Canvas canvasRef;
        public bool IsOpen = false;

        bool gameplayEnd = false;

        Canvas GetCanvas()
        {
            if(canvasRef == null)
            {
                canvasRef = GetComponentInParent<Canvas>();
            }

            return canvasRef;
        }

        public void ResetToDefault()
        {
            gameplayEnd = false;
            IsOpen = true;
            cam.gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
        }

        public void Open()
        {
            if (!GetComponent<Animation>().isPlaying)
            {
                cam.gameObject.SetActive(true);
                gameObject.PlayAnimation(this, "mag_in");
            }
        }

        public void Close(bool disableObject = false)
        {
            if (disableObject)
            {
                gameplayEnd = disableObject;

                if (IsOpen)
                    return;

            }
            
            if (!GetComponent<Animation>().isPlaying)
                gameObject.PlayAnimation(this, "mag_out", () =>
                {
                    gameObject.SetActive(!disableObject);
                    cam.gameObject.SetActive(false);
                });
        }

        public void ToggleOpen()
        {
            if (gameplayEnd) return;

            IsOpen = !IsOpen;
            if (IsOpen)
                Close();
            else
                Open();
        }

        private void Update()
        {
            Vector3 p = Input.mousePosition;
            p.z = 0f;
            p.x = Mathf.Clamp(p.x, 0, Screen.width);
            p.y = Mathf.Clamp(p.y, 0, Screen.height);

            transform.position = p;

            if (GetComponent<Animation>().isPlaying) return;

            if (Input.GetMouseButtonUp(1))
            {
                ToggleOpen();
            }
        }

    }
}

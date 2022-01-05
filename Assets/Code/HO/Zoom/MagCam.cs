using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class MagCam : MonoBehaviour
    {
        RectTransform rectTransform;
        Camera baseCamera;
        [SerializeField]
        Camera magCamera;
        [SerializeField]
        MagCamDisplay displaySurface;

        [SerializeField]
        float zoomFactor = 2f;

        RenderTexture magTexture;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            baseCamera = HOGameController.instance.hoCamera;
            ResizeCam();
        }

        void ResizeCam()
        {
            magCamera.aspect = 1f;

            magTexture = RenderTexture.GetTemporary((int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y);
            magCamera.targetTexture = magTexture;
            displaySurface.rawImage.texture = magTexture;
        }

        private void Update()
        {
            Vector3 p = Input.mousePosition;
            p.z = -30f;
            p.x = Mathf.Clamp(p.x, 0, Screen.width);
            p.y = Mathf.Clamp(p.y, 0, Screen.height);

            transform.position = baseCamera.ScreenToWorldPoint(p);

            Vector2 origin = baseCamera.ScreenToWorldPoint(Vector2.zero);
            Vector2 frame = baseCamera.ScreenToWorldPoint(rectTransform.sizeDelta);

            magCamera.orthographicSize = (frame - origin).y * 0.5f * displaySurface.canvas.scaleFactor / zoomFactor;
        }

    }
}

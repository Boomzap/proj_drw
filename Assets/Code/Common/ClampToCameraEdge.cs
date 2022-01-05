using System.Collections;
using UnityEngine;

using Sirenix.OdinInspector;

namespace ho
{
    public class ClampToCameraEdge : MonoBehaviour
    {
        enum CameraHorz
        {
            Left,
            Middle,
            Right
        }

        enum CameraVert
        {
            Top,
            Middle,
            Bottom
        }

        [SerializeField] CameraHorz horizontalPosition = CameraHorz.Left;
        [SerializeField] CameraVert verticalPosition = CameraVert.Top;
        [SerializeField] Vector2 offsetPosition = Vector2.zero;

        // Use this for initialization
        void Start()
        {
            SnapToSelected();
        }

        // Update is called once per frame
        void Update()
        {
            SnapToSelected();
        }

        [Button]
        void SnapToSelected()
        {
            Vector3 relativePos = new Vector3(0f, 0f, 0f);

            switch (horizontalPosition)
            {
                case CameraHorz.Left: relativePos.x = 0f; break;
                case CameraHorz.Right: relativePos.x = Screen.width; break;
                case CameraHorz.Middle: relativePos.x = Screen.width * 0.5f; break;
            }

            switch (verticalPosition)
            {
                case CameraVert.Top: relativePos.y = Screen.height; break;
                case CameraVert.Bottom: relativePos.y = 0f; break;
                case CameraVert.Middle: relativePos.y = Screen.height * 0.5f; break;
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(relativePos + (Vector3)offsetPosition);
            worldPos.z = transform.position.z;

            transform.position = worldPos;
        }
    }
}
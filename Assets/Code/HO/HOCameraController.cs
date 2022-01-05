using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOCameraController : MonoBehaviour
    {
        [BoxGroup("Zoom Properties")]
        [Range(1f, 4f)]
        [SerializeField] private float maxZoomMultiplier = 2f;

        [BoxGroup("Zoom Properties")]
        [SerializeField] private float zoomTime = 0.5f; //
        [BoxGroup("Zoom Properties")]
        [SerializeField] private float zoomSpeed = 100f; //Zoom Speed

        private bool isZoomed = false;

        [BoxGroup("Mouse Properties")]
        [SerializeField]
        float dragThreshholdSqr = 10*10;

        private float lastClickTime = 0f;

        [BoxGroup("Mouse Properties")]
        [SerializeField] private float doubleClickDelay = 0.5f; //Delay time to register double click

       
        public bool isDragging = false;
        public bool isZooming = false;

        public bool isZoomEnabled = false;

        private float dragTimer;

        //Camera Default Values
        private Camera cmDefault;

        private float cmOrigSize;
        private Vector3 cmOrigPosition;

        private float cmMaxBoundsY;
        private float cmMinBoundsY;
        private float cmMaxBoundsX;
        private float cmMinBoundsX;

        private Vector3 lastMousePos; //Saves last reference for zoom in
        private Vector3 dragStartPos = Vector3.zero;
        float dragDistance = 0f;
        Vector3 dragPrevMousePosScreen;

        Vector3 lastTapPos;

        private void Start()
        {
            InitCamera();
        }
        public void InitCamera()
        {
            cmDefault = GetComponent<Camera>();
            cmOrigSize = cmDefault.orthographicSize;
            cmOrigPosition = cmDefault.transform.position;

            //ResetCamera(true);
        }

        private void ComputeMaxBounds()
        {
            //Bounds based on HORoom
            //NOTE* Transform.parent = Gets HO Controller Transform values. 
            //Bounds are assuming all HO Rooms are of same size.
            cmMaxBoundsY = transform.parent.position.y + HOGameController.instance.currentRoomRef.roomPrefab.roomBounds.max.y;
            cmMinBoundsY = transform.parent.position.y - HOGameController.instance.currentRoomRef.roomPrefab.roomBounds.max.y;
            cmMaxBoundsX = transform.parent.position.x + HOGameController.instance.currentRoomRef.roomPrefab.roomBounds.max.x;
            cmMinBoundsX = transform.parent.position.x - HOGameController.instance.currentRoomRef.roomPrefab.roomBounds.max.x;

            //Bounds based on Camera
            //NOTE* Transform.parent = Gets HO Controller Transform values. 
            //cmMaxBoundsY = transform.parent.position.y + cmDefault.orthographicSize;
            //cmMinBoundsY = transform.parent.position.y - cmDefault.orthographicSize;
            //cmMaxBoundsX = transform.parent.position.x + (cmDefault.aspect * cmOrigSize);
            //cmMinBoundsX = transform.parent.position.x - (cmDefault.aspect * cmOrigSize);
        }

        #region Player Behaviour

        void Update()
        {
            if (isZoomEnabled == false || HOGameController.instance.DisableInput) return;

            if (Input.GetMouseButtonDown(0) && UIController.instance.isPointerOverUIObject == false)
            {
                //GameObject clickFX = UIController.instance.clickFXPooler.GetPooledObject();
                //clickFX.transform.position = cmDefault.ScreenToWorldPoint(Input.mousePosition);
                //clickFX.gameObject.SetActive(true);

                OnMousePointerDown();
                dragPrevMousePosScreen = Input.mousePosition;
                dragStartPos = cmDefault.ScreenToWorldPoint(dragPrevMousePosScreen);
                dragDistance = 0f;
            }
            else if (Input.GetMouseButton(0) && isZoomed)
            {
                // mouse down frame -> record drag start position
                // succeeding frames with mouse down -> update drag

                if (isDragging)
                {
                    OnCameraDrag();
                }
                else
                {
                    dragDistance += (Input.mousePosition - dragPrevMousePosScreen).sqrMagnitude;
                    // judge this by distance moved, not time
                    if (dragDistance >= dragThreshholdSqr)
                    {
                        isDragging = true;
                    }
                }
            }

         

            float scrollValue = Input.mouseScrollDelta.y;
            if(scrollValue == 1f)
            {
                OnCameraZoom(true);
            }
            
            if(scrollValue == -1f)
            {
                OnCameraZoom(false);
            }
        }

        private void LateUpdate()
        {
            // it's better to keep this kind of control in the same behaviour.. 
            // i assume it was done in HOGameController previously because of execution order
            // making end of drag fuck with the check there.

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }


        private void OnMousePointerDown()
        {
            float tapDelta = (Input.mousePosition - lastTapPos).sqrMagnitude;
            float maxTapDelta = (Screen.width * 0.02f);
            maxTapDelta *= maxTapDelta;

            if (Time.time - lastClickTime < doubleClickDelay && (tapDelta < maxTapDelta))
            {
                OnCameraZoom();
            }
            
            lastClickTime = Time.time;
            lastTapPos = Input.mousePosition;
        }

        public void OnEndDrag()
        {
            isDragging = false;
        }

        #endregion

        #region Camera Behaviour
        private void OnCameraZoom()
        {
            isZoomed = !isZoomed;
            StopAllCoroutines();
            StartCoroutine(OnAnimateZoom());

        }

        private void OnCameraZoom(bool _isZoomed)
        {
            if(isZoomed!=_isZoomed&&isZooming==false)
            {
                isZoomed = _isZoomed;
                StopAllCoroutines();
                StartCoroutine(OnAnimateZoom());
            }
        }

        private void OnCameraDrag()
        {
            Vector3 currentMousePos = cmDefault.ScreenToWorldPoint(Input.mousePosition); // Current mouse position
            Vector3 difference = dragStartPos - currentMousePos; //NOTE* Difference from start drag pos and current pos. 
            Vector3 targetPosition = ComputeTargetPosition(cmDefault.orthographicSize, cmDefault.transform.position + difference);
            cmDefault.transform.position = targetPosition;

        }

        private IEnumerator OnAnimateZoom()
        {
            isZooming = true;
            lastMousePos = cmDefault.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log("Last Mouse Pos: "+lastMousePos);

            float targetCameraSize = isZoomed ? cmOrigSize / maxZoomMultiplier : cmOrigSize;
            Vector3 targetPosition = isZoomed? ComputeTargetPosition(targetCameraSize,lastMousePos): cmOrigPosition; //Compute Target Position based from boundary
            //Debug.Log("TARGET: "+targetPosition);
            float animateTime = zoomTime;
            while (animateTime > 0)
            {
                cmDefault.orthographicSize = Mathf.Lerp(cmDefault.orthographicSize, targetCameraSize, zoomSpeed*zoomTime*Time.deltaTime);
                cmDefault.transform.position = Vector3.Lerp(cmDefault.transform.position, targetPosition, zoomSpeed*zoomTime*Time.deltaTime);
                animateTime -= Time.deltaTime;   
                yield return new WaitForEndOfFrame();
            }

            isZooming = false;
        }

        #endregion

        private Vector3 ComputeTargetPosition(float _cameraSize, Vector3 _mousePosition)
        {
            //Note* Compute Max Bounds only once when a main room is active
            if(cmMaxBoundsX == 0) ComputeMaxBounds();

            //Click Position
            float xTargetPos = _mousePosition.x;
            float yTargetPos = _mousePosition.y;

            //Compute current camera Height Width
            float halfHeight = _cameraSize;
            float halfWidth = cmDefault.aspect * _cameraSize; 


            if(yTargetPos + halfHeight> cmMaxBoundsY) //if Camera exceeds top boundary
            {
                yTargetPos = cmMaxBoundsY - halfHeight;
            }

            if (yTargetPos - halfHeight < cmMinBoundsY) //if Camera exceeds bottom boundary
            {
                yTargetPos = cmMinBoundsY + halfHeight;
            }

            if (xTargetPos + halfWidth > cmMaxBoundsX) //if Camera exceeds right boundary
            {
                xTargetPos = cmMaxBoundsX - halfWidth;
            }

            if (xTargetPos - halfWidth < cmMinBoundsX) //if Camera exceeds left boundary
            {
                xTargetPos = cmMinBoundsX + halfWidth;
            }

            return new Vector3(xTargetPos,yTargetPos, _mousePosition.z);
        }

        //private float ComputeTargetCameraSize()
        //{
        //    float zoomPercentage = Mathf.Round(cmDefault.orthographicSize - (scrollValue * cmDefault.orthographicSize * zoomPercent / 100f));
        //    zoomPercentage = Mathf.Clamp(zoomPercentage, _minCameraZoom, cmOrigSize);
        //    return zoomPercentage;
        //}


        [Button]
        public void ResetCamera(bool skipAnimation)
        {
            //Note* Reset Camera Can be called before object is started
            if (cmDefault == null) InitCamera();
            if(skipAnimation)
            {
                cmDefault.transform.position = cmOrigPosition;
                cmDefault.orthographicSize = cmOrigSize;
            }
            else
            {
                lastMousePos = cmOrigPosition;
                OnCameraZoom(false);
            }
            isZoomed = false; 
            isDragging = false;
            lastClickTime = 0;
        }
    }

}

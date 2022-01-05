using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Sirenix.OdinInspector;

namespace ho
{
    public class ClickToRotatePiece : ClickToPiece
    {
        public float rotationStep = 90f;

        public int sides = 4;

        int currentSide = 0;

        protected TimedFloatLerp rotation;

        ClickToSwapPiece ctsPiece;

        private void Awake()
        {
            if (isFixedPiece)
                ForceComplete();

            currentSide = 0;
        }

        [Button]
        void RotateTest()
        {
            PolygonCollider2D x = GetComponent<PolygonCollider2D>();
            Vector2 avg = Vector2.zero;

            foreach (var p in x.points)
                avg += p;

            avg /= x.points.Length;
            avg += (Vector2)transform.localPosition;

            transform.RotateAround(avg, new Vector3(0f, 0f, 1f), 60f);
        }

        public override void RandomizeRotation()
        {
            currentSide = Random.Range(0, sides);
            transform.rotation = Quaternion.Euler(0, 0, rotationStep * currentSide);
        }

        public override bool IsCorrect()
        {
            float current = transform.eulerAngles.z;
            float clampedAbs = Mathf.Abs(current % 360.0f);
            return clampedAbs < 4.0f || clampedAbs - 360.0f > -4.0f;
        }

        public override void ForceComplete()
        {
            isComplete = true;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rotation = null;
            sprite.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }


        public override bool IsAnimating() { return rotation != null; }
        public override void OnClick()
        {
            //Debug.Log($"Is complete {IsComplete}");
            if (IsComplete) return;

            if(owner.mgType == ClickToMG.MGType.SwapRotate)
            {
                //Do not update Selected piece when MG Type is Swap Rotate
                //We want the selected piece to be updated by click to swap piece
            }
            else
            {
                owner.SelectedPiece = this;
            }
           

            //int curAngle = Mathf.FloorToInt(transform.eulerAngles.z);

            //Debug.Log(curAngle);
            //int nextAngle = (curAngle + (int)rotationStep);

            currentSide++;

            if (currentSide >= sides)
                currentSide = 0;

            transform.eulerAngles = new Vector3(0f, 0f, rotationStep * currentSide);
            Audio.instance.PlaySound(MinigameController.instance.onPieceRotate.GetClip(null));

            if (owner.LockOnComplete && IsCorrect())
            {
                if (owner.mgType == ClickToMG.MGType.SwapRotate)
                {
                    if (ctsPiece == null)
                        ctsPiece = GetComponent<ClickToSwapPiece>();

                    if (ctsPiece.IsCorrect())
                    {
                        //CTS & CTR Piece are both correct Lock on complete
                        isComplete = true;
                        ctsPiece.ForceComplete();
                    }
                }
                else
                {
                    isComplete = true;
                }
            }
        }

        protected override void UpdatePiece()
        {
            if (rotation != null)
            {
                rotation.Update();
                transform.rotation = Quaternion.Euler(0.0f, 0, rotation.Value);
                if (rotation.IsDone)
                {
                    rotation = null;
                    isActive = false;
                }
            }
        }
    }

}
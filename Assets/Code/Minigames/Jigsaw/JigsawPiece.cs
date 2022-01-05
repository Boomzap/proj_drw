using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class JigsawPiece : MinigamePiece
    {
        Vector3 originalPosition;
        Jigsaw jigsaw;
        bool isHighlighted = false;
        bool isMoving = false;
        float selectedOffset = 0;
        float rotationDuration = 0.5f;
        float rotationTime = 0;
        float rotationDirection = 0;
        Vector2 rotateFromTo;
        bool isSet = false;
        int orgSortingOrder;


        public void SetJigsaw(Jigsaw _jigsaw) { jigsaw = _jigsaw; }
        public bool IsSet { get { return isSet; } }

        Vector2 startDragPos;
        Vector2 startDragPosMouse;

        public void StartDrag(Vector3 mouseWorldPos)
        {
            startDragPosMouse = mouseWorldPos;
            startDragPos = transform.position;
            sprite.sortingOrder = jigsaw.DragLayer;
            sdfRenderer.sortingOrder = jigsaw.DragLayer - 1;
        }

        public void TrackMouse(Vector3 mouseWorldPos)
        {
            isHighlighted = true;
            sprite.sortingOrder = jigsaw.DragLayer;

            Vector2 delta = (Vector2)mouseWorldPos - startDragPosMouse;
            Vector2 newPos = startDragPos + delta;

            transform.position = new Vector3(newPos.x, newPos.y, originalPosition.z + jigsaw.SelectedHeight);
        }

        public void ResetToPosition(Vector3 position)
        {
            transform.position = position;
        }


        IEnumerator MoveFromTo(Vector3 from, Vector3 to, float fromZRotation, float toZRotation, float duration)
        {
            isMoving = true;
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                Vector3 pos = Mathfx.Hermite(from, to, t);
                pos.z = originalPosition.z + selectedOffset + Mathf.Sin(t * Mathf.PI) * 200.0f;
                transform.position = pos;

                float angle = Mathfx.Hermite(fromZRotation, toZRotation, t);
                transform.rotation = Quaternion.Euler(0, 0, angle);
                yield return new WaitForEndOfFrame();
            }
            transform.position = to;
            transform.rotation = Quaternion.Euler(0, 0, toZRotation);
            isMoving = false;
        }
        public void Return(bool immediate)
        {
            if (immediate)
            {
                transform.position = originalPosition;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                StartCoroutine(MoveFromTo(transform.position, originalPosition, transform.localEulerAngles.z, 0, Random.Range(jigsaw.AutoMoveTime.x, jigsaw.AutoMoveTime.y)));
            }
        }
        public void Scatter(Vector3 toPos, bool immediate)
        {
            int targetRotation = Random.Range(0, 4);
            if (!jigsaw.CanRotate) targetRotation = 0;
            float rotation = targetRotation * 90.0f;
            if (immediate)
            {
                transform.position = toPos;
                transform.rotation = Quaternion.Euler(0, 0, rotation);
            }
            else
            {
                StartCoroutine(MoveFromTo(transform.position, toPos, transform.localEulerAngles.z, rotation, Random.Range(jigsaw.AutoMoveTime.x, jigsaw.AutoMoveTime.y)));
            }
        }

        public bool isClose
        {
            get
            {
                if (jigsaw.CanRotate)
                {
                    // 				    float rotation = transform.rotation.eulerAngles.z;
                    // 				    if (Mathf.Abs(rotation) > 4.0f) return false; // they all start out at rotation 0, so this is the wrong rotation.
                    float rotDelta = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, 0);
                    if (rotDelta > 4f) return false;
                }
                float dist = (transform.position - originalPosition).magnitude;
                return dist < jigsaw.SnapThreshold;
            }
        }

        public int sort { get { return sprite.sortingOrder; } }
        public void StopDragging()
        {
            sprite.sortingOrder = jigsaw.NextUnsetLayer;
            sdfRenderer.sortingOrder = sprite.sortingOrder - 1;
        }

        public void Snap()
        {
            // animate this?
            //transform.position = originalPosition;
            iTween.MoveTo(gameObject, iTween.Hash("position", originalPosition, "time", 0.5f, "easetype", iTween.EaseType.easeOutQuart));

            isSet = true;
            sprite.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            sprite.material = MinigameController.instance.DefaultMaterial;
            GetComponent<Collider2D>().enabled = false;
            sdfRenderer.gameObject.SetActive(false);
        }

        public void Rotate(int direction)
        {
            if (!jigsaw.CanRotate) return;
            if (rotationDirection != 0) return; // already rotating
            rotationTime = 0;
            rotationDirection = direction;
            rotateFromTo = new Vector2(transform.rotation.eulerAngles.z, transform.rotation.eulerAngles.z + ((direction == 1) ? 90 : -90));
        }

        public void SetHighlight(bool _isHighlighted)
        {
            if (isHighlighted == _isHighlighted) return;
            if (isSet) return;
            isHighlighted = _isHighlighted;

            sdfRenderer.gameObject.SetActive(true);

            if (isHighlighted)
            {
                sprite.material = MinigameController.instance.JigsawSelectedMaterial;
                sdfRenderer.color = Color.white;
                sprite.sortingOrder = jigsaw.DragLayer;
            }
            else
            {
                selectedOffset = 0; // snapdrop it?
                sprite.sortingOrder = jigsaw.NextUnsetLayer;

                sprite.material = MinigameController.instance.DefaultMaterial;
                sdfRenderer.color = Color.black;
            }

            sdfRenderer.sortingOrder = sprite.sortingOrder - 1;
            /*            sdfRenderer.sortingLayerName = "Foreground";*/
        }


        IEnumerator PlaySuccessCo()
        {
            //var startScale = transform.localScale;

            float t = 0f;
            const float max = 1f;

            sdfRenderer.gameObject.SetActive(true);
            sdfRenderer.color = Color.white;

            while (t < max)
            {
                float a = t / max;
                float b = Mathf.Clamp01(a * 2f);
                a = Mathf.Sin(a * Mathf.PI);

                //transform.localScale = (1f + (a * 0.1f)) * startScale;
                sdfRenderer.material.SetFloat("_GlowAlpha", a);
                sprite.color = new Color(0.9f + b * 0.1f, 0.9f + b * 0.1f, 0.9f + b * 0.1f, 1.0f);

                t += Time.deltaTime;
                yield return null;
            }

            sdfRenderer.material.SetFloat("_GlowAlpha", 0f);
        }
        public void OnSuccess()
        {
            StartCoroutine(PlaySuccessCo());
        }


        // Start is called before the first frame update
        public void Init()
        {
            originalPosition = transform.position;
            sprite = GetComponent<SpriteRenderer>();
            orgSortingOrder = sprite.sortingOrder;
            sprite.sortingOrder = jigsaw.NextUnsetLayer;

            if (sdfRenderer != null)
            {
                sdfRenderer.sortingOrder = sprite.sortingOrder - 1;
                sdfRenderer.material = MinigameController.instance.jigsawPieceBorderSDFMaterial;
                sdfRenderer.gameObject.SetActive(true);
                sdfRenderer.color = Color.black;
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (isSet)
            {

                sprite.sortingOrder = orgSortingOrder;
                return; // don't bother
            }

            if (isHighlighted)
            {
                selectedOffset += Time.deltaTime * jigsaw.SelectedHeight * 5.0f;
            }
            else
            {
                selectedOffset -= Time.deltaTime * jigsaw.SelectedHeight * 5.0f;
            }
            selectedOffset = Mathf.Clamp(selectedOffset, jigsaw.SelectedHeight, 0);

            if (rotationDirection != 0)
            {
                rotationTime += Time.deltaTime;
                float t = Mathf.Clamp01(rotationTime / rotationDuration);

                float height = Mathf.Sin(Mathf.PI * t) * jigsaw.SelectedHeight;

                transform.position = new Vector3(transform.position.x, transform.position.y, originalPosition.z + height);
                float angle = Mathfx.Hermite(rotateFromTo.x, rotateFromTo.y, t);
                transform.rotation = Quaternion.Euler(0, 0, angle);
                if (rotationTime > rotationDuration)
                {
                    // done
                    rotationDirection = 0;
                }
            }
            else
            if (isMoving)
            {
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, originalPosition.z + selectedOffset);
            }
        }
    }
}
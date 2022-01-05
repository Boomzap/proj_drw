using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class ClickToPiece : MinigamePiece
    {
        public int SortValue { get { return sortValue; } }
        public PolygonCollider2D Collider { get { return polyCollider; } }
        public float ActiveHeight { get { return (owner) ? owner.SelectedHeight : -100.0f; } }
        public bool IsComplete { get { return isComplete; } }

        public bool isFixedPiece = false;

        protected bool isActive = false;
        protected Material defaultMaterial;
        protected int sortValue = 0;

        protected PolygonCollider2D polyCollider;
        protected bool isComplete = false;
        protected ClickToMG owner;

        public bool IsSelected { get { return owner.SelectedPiece == this; } }

        protected bool finalAnim = false;
        // Start is called before the first frame update
        void Start()
        {
        }

        IEnumerator PlaySuccessCo()
        {
            finalAnim = true;

            float t = 0f;
            const float max = 0.3f;

            sdfRenderer.gameObject.SetActive(true);

            while (t < max)
            {
                float a = t / max;
                a = Mathf.Sin(a * Mathf.PI);

                sdfRenderer.color = new Color(1f, 1f, 1f, a);

                t += Time.deltaTime;
                yield return null;
            }

            sdfRenderer.gameObject.SetActive(false);
        }

        public void OnSuccess()
        {
            StartCoroutine(PlaySuccessCo());
        }


        public virtual void SetupPiece()
        {
            sprite = GetComponent<SpriteRenderer>();
            sortValue = sprite.sortingOrder;
            polyCollider = GetComponent<PolygonCollider2D>();
            defaultMaterial = sprite.sharedMaterial ? sprite.sharedMaterial : sprite.material;
            owner = GetComponentInParent<ClickToMG>();

            if (sdfRenderer)
                sdfRenderer.material = MinigameController.instance.jigsawPieceBorderSDFMaterial;
        }

        public virtual void ForceComplete()
        {
            isComplete = true;
        }


        public void SetSelected(bool _isSelected)
        {
            if (_isSelected && !IsComplete)
            {
                sprite.material = MinigameController.instance.JigsawSelectedMaterial;
            }
            else
            {
                sprite.material = defaultMaterial;
            }
        }

        public abstract void OnClick();
        public virtual void RandomizeRotation() { }
        public virtual bool IsAnimating() { return false; }
        public abstract bool IsCorrect();

        // Update is called once per frame
        void Update()
        {
            if (finalAnim) return;
            if (sprite == null) return;
            if (isActive || IsAnimating())
            {
                sprite.sortingOrder = 1000 + sortValue;
            }
            else
            {
                sprite.sortingOrder = sortValue;
            }

            if (isActive)
            {
                float z = transform.position.z;
                if (z > ActiveHeight)
                {
                    z += ActiveHeight * Time.deltaTime * owner.SelectedHeightSpeed;
                    if (z < ActiveHeight) z = ActiveHeight;
                    transform.position = new Vector3(transform.position.x, transform.position.y, z);
                }
            }
            else
            {
                float z = transform.position.z;
                if (z < 0)
                {
                    z -= ActiveHeight * Time.deltaTime * owner.SelectedHeightSpeed;
                    if (z < 0) z = 0;
                    transform.position = new Vector3(transform.position.x, transform.position.y, z);
                }
            }

            UpdatePiece();

            if(isFixedPiece == false)
            sprite.color = IsComplete ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
        }
        protected virtual void UpdatePiece() { }

    }

}
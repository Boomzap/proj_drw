using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class PaintablePiece : MinigamePiece
    {
        public string colorKey;

        public PaintMG owner;

        public bool isCorrect = false;

        bool selectedColorMatched => UIController.instance.minigameUI.PaintColorHolder.SelectedColor?.GetColorData().colorKey.Equals(colorKey)?? false;

        [ReadOnly]
        public PolygonCollider2D hitBox;

        [Button]
        public void OnClick()
        {
            if (isCorrect) return;

            if(selectedColorMatched)
            {
                //Debug.Log("Color Matched");
                isCorrect = true;
                StartCoroutine(AnimatePaintPiece());
            }
        }

        public void ForceComplete()
        {
            isCorrect = true;
            TapFeedbackFX.instance.CreateAtWorldPos(transform.position);
            StartCoroutine(AnimatePaintPiece());
        }

        IEnumerator AnimatePaintPiece()
        {
            float animTime = 0.5f;
            float currentAlpha = 1f;
            float time = animTime;

            while(time > 0)
            {
                time -= Time.deltaTime;
                currentAlpha = time / animTime;

                sprite.color = new Color(sprite.color.r, sprite.color.g , sprite.color.g , currentAlpha);
                yield return new WaitForEndOfFrame();
            }
        }

        public bool IsCorrect()
        {
            return isCorrect;
        }

        public void SetupPiece()
        {
            var nameCode = name.Split('_');

            if(nameCode.Length == 4)
            {
                colorKey = int.Parse(nameCode[1]).ToString();
            }

            if(hitBox == null)
            {
               hitBox = gameObject.AddComponent<PolygonCollider2D>();
            }
            
            owner = GetComponentInParent<PaintMG>();

            sprite = GetComponent<SpriteRenderer>();

            sprite.sortingOrder = 500;
        }

        private void Update()
        {
            if (isCorrect) return;

            sprite.color = selectedColorMatched ? owner.selectColor : Color.white;
        }
    }
}


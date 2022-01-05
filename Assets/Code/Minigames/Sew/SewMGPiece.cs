using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class SewMGPiece : MinigamePiece
    {
        [ReadOnly]
        public SewMG owner;

        [ReadOnly]
        public Sprite correctSprite;

        bool selectedColorMatched => UIController.instance.minigameUI.SewColorHolder.SelectedColor?.currentColorSprite == correctSprite;

        bool isCorrect => correctSprite == sprite.sprite;

        [ReadOnly]
        public PolygonCollider2D hitBox;

        public bool IsCorrect()
        {
            return isCorrect;
        }

        public void ForceComplete()
        {
            TapFeedbackFX.instance.CreateAtWorldPos(transform.position);
            sprite.sprite = correctSprite;
        }

        [Button]
        public void OnClick()
        {
            if (UIController.instance.minigameUI.SewColorHolder.SelectedColor)
                sprite.sprite = UIController.instance.minigameUI.SewColorHolder.SelectedColor.sprite;
        }

        public void SetupPiece()
        {
            owner = GetComponentInParent<SewMG>();

            sprite = GetComponent<SpriteRenderer>();

            if (hitBox == null)
            {
                hitBox = gameObject.AddComponent<PolygonCollider2D>();
            }

            correctSprite = sprite.sprite;
        }
    }

}

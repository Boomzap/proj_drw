using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOKeyItemHolder : MonoBehaviour
    {
        public Image                itemImage;

        [ReadOnly]
        public HOKeyItem            keyItem;
        public bool                 isEmpty{ get => keyItem == null; }
        ButtonMaterialController    matController;

        private void OnEnable()
        {
            matController = GetComponent<ButtonMaterialController>();
            if (matController)
                matController.disableVisualChanges = (keyItem == null);
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            itemImage.gameObject.SetActive(false);
            itemImage.sprite = null;
            keyItem = null;
        }

        public void SetObject(HOKeyItem keyItem, bool animate = false)
        {
            this.keyItem = keyItem;

            if (matController)
                matController.disableVisualChanges = (keyItem == null);

            itemImage.gameObject.SetActive(!isEmpty);
           

            if (keyItem)
            {
                var q = keyItem.GetComponent<SpriteRenderer>();
                itemImage.sprite = keyItem.GetComponent<SpriteRenderer>().sprite;
                itemImage.preserveAspect = true;
                gameObject.SetActive(true);
                iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
            }
            else
            {
                iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart, "oncomplete","Clear"));
            }
        }

        public Vector4 GetSpriteQuadWhenAspectCorrected(Sprite sprite)
        {
            var padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
            var size = new Vector2(sprite.rect.width, sprite.rect.height);

            Rect r = itemImage.GetPixelAdjustedRect();

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                    padding.x / spriteW,
                    padding.y / spriteH,
                    (spriteW - padding.z) / spriteW,
                    (spriteH - padding.w) / spriteH);

            if (size.sqrMagnitude > 0.0f)
            {
                var spriteRatio = size.x / size.y;
                var rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio)
                {
                    var oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * itemImage.rectTransform.pivot.y;
                }
                else
                {
                    var oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * itemImage.rectTransform.pivot.x;
                }
            }

            v = new Vector4(
                    r.x + r.width * v.x,
                    r.y + r.height * v.y,
                    r.x + r.width * v.z,
                    r.y + r.height * v.w
                    );

            return v;            
        }

    }
}
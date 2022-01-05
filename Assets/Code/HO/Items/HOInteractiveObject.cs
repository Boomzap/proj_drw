using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ho
{
    [DisallowMultipleComponent]
    public abstract class HOInteractiveObject : MonoBehaviour
    {
        public enum ColliderType
        {
            PolygonCollider,
            BoxCollider,
            CapsuleCollider
        };

        [BoxGroup("Object Settings")]
        [ReadOnly]
        public string displayKey;

        [BoxGroup("Object Settings")]
        public SpriteRenderer       sdfRenderer;
        public SDFHitZone           sdfHitZone;

        #if UNITY_EDITOR
        [Button]
        public void Initialize()
        {
            HORoom room = GetComponentInParent<HORoom>();

            if (room)
            {
                InitializeDefaults(room.name);
            } else
            {
                UnityEditor.EditorUtility.DisplayDialog("No room found", "This object doesn't seem to be the child of an HO Room. What you doing?", "OK");
            }
        }

        #endif


        public void StopAnimateGlow()
        {
            StopCoroutine(AnimateGlowCor());
        }

        IEnumerator AnimateGlowCor(UnityAction andThen = null, float glowTime = 2f)
        {
            Material glowMat = Instantiate<Material>(HOGameController.instance.itemFoundGlowMaterial);
            sdfRenderer.material = glowMat;
            sdfRenderer.gameObject.SetActive(true);

            float glowAlpha = 0f;
            float timeElapsed = 0f;

            while(timeElapsed <= glowTime)
            {
                glowAlpha = Mathf.Lerp(glowAlpha, 1f, timeElapsed / glowTime);

                if(sdfRenderer != null)
                {
                    sdfRenderer.material.SetFloat("_GlowAlpha", glowAlpha);
                }
               
                timeElapsed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            andThen?.Invoke();
        }

        public void AnimateGlow(UnityAction andThen = null, float glowTime = 2f)
        {
            Audio.instance.PlaySound(HOGameController.instance.onItemFoundAudio);
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateGlowCor(andThen, glowTime));
            }
            else
            {
                //Instant Glow
                Material glowMat = Instantiate<Material>(HOGameController.instance.itemFoundGlowMaterial);
                sdfRenderer.material = glowMat;
                sdfRenderer.gameObject.SetActive(true);

                if (sdfRenderer != null)
                {
                    sdfRenderer.material.SetFloat("_GlowAlpha", 1f);
                }
            }
        }

        public virtual void InitializeDefaults(string roomName)
        {
            RegenerateCollision();

            displayKey = HOUtil.GetRoomObjectLocalizedName(roomName, gameObject.name, true);
        }

        public abstract bool OnClick();
        public virtual string GetDisplayText()
        {
            return LocalizationUtil.FindLocalizationEntry(displayKey);
        }

        [Button] 
        public void RegenerateCollision()
        {
            RemoveColliders();
            
            sdfHitZone = gameObject.AddComponent<SDFHitZone>();
            sdfHitZone.sdfSprite = sdfRenderer;
        }

        public void RemoveColliders()
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
		    if (colliders != null) 
            {
                foreach (Collider2D o in colliders) 
                    GameObject.DestroyImmediate(o, true);
            }

            SDFHitZone[] hitZones = GetComponents<SDFHitZone>();
            if (hitZones != null)
            {
                foreach (SDFHitZone z in hitZones)
                    GameObject.DestroyImmediate(z, true);
            }
        }

        public bool IsFullyOnScreen()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		    if (spriteRenderer != null) 
            {
                Vector2 min = spriteRenderer.bounds.center - spriteRenderer.bounds.extents;
                Vector2 max = spriteRenderer.bounds.center + spriteRenderer.bounds.extents;

                min = GameController.instance.currentCamera.WorldToScreenPoint(min);
                max = GameController.instance.currentCamera.WorldToScreenPoint(max);

                return min.x >= 0f && min.y >= 0f && 
                    max.x <= GameController.instance.currentCamera.pixelWidth &&
                    max.y <= GameController.instance.currentCamera.pixelHeight;
            }

            return true;
        }

        // rect should be in screen points
        public bool IsCoveredByRect(Rect rect)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		    if (spriteRenderer != null) 
            {
                Vector2 min = spriteRenderer.bounds.center - spriteRenderer.bounds.extents;
                Vector2 max = spriteRenderer.bounds.center + spriteRenderer.bounds.extents;

                min = GameController.instance.currentCamera.WorldToScreenPoint(min);
                max = GameController.instance.currentCamera.WorldToScreenPoint(max);

                Rect localRect = new Rect((max + min) * 0.5f, max-min);
                return rect.Overlaps(localRect, true);
            }

            return false;           
        }
    }
}

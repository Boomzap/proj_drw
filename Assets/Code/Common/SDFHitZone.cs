using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    public class SDFHitZone : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer  sdfSprite;
        Unity.Collections.NativeArray<byte> pixelData;
        int textureWidth, textureHeight;


#if UNITY_EDITOR || DEVELOPMENT_BUILD || ENABLE_CHEATS
        [NonSerialized]
        public Material hitZoneIndicator;
        #endif

        public float maximumHitDistance = 0.6f;

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            SDFHitZoneRegister reg = GetComponentInParent<SDFHitZoneRegister>();

            if (reg)
            {
                reg.Register(this);
            }            
        }

        private void OnDisable()
        {
            SDFHitZoneRegister reg = GetComponentInParent<SDFHitZoneRegister>();

            if (reg)
            {
                reg.Unregister(this);
            }
        }

        private void Init()
        {
            if (sdfSprite)
            {
                pixelData = sdfSprite.sprite.texture.GetPixelData<byte>(0);

                textureWidth = sdfSprite.sprite.texture.width;
                textureHeight = sdfSprite.sprite.texture.height;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || ENABLE_CHEATS
            hitZoneIndicator = Instantiate(Cheat.instance.CheatOutlineMat);
            #endif            
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (sdfSprite == null)
                return;

            if (hitZoneIndicator == null || !pixelData.IsCreated)
                Init();

            Rect txRect = sdfSprite.sprite.textureRect;
            txRect.height = -txRect.height;
            txRect.position += (Vector2)transform.position - txRect.size * 0.5f;
            Gizmos.DrawGUITexture(txRect, sdfSprite.sprite.texture, hitZoneIndicator);

            var mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;

            hitZoneIndicator.SetFloat("_MaxDistance", maximumHitDistance);
            if (IsInside(mousePos))
            {
                hitZoneIndicator.SetColor("_GlowColor", new Color(0f, 1f, 0f, 1f));
            } else
            {
                hitZoneIndicator.SetColor("_GlowColor", new Color(1f, 0f, 0f, 1f));
            }
        }

#endif

        public bool IsInside(Vector2 worldPos)
        {
            Vector2 localPos = (worldPos - (Vector2)transform.position);
           
            if (localPos.x > sdfSprite.sprite.bounds.min.x && 
                localPos.x < sdfSprite.sprite.bounds.max.x &&
                localPos.y > sdfSprite.sprite.bounds.min.y &&
                localPos.y < sdfSprite.sprite.bounds.max.y)
            {
                if (!pixelData.IsCreated) return true;



                Vector2 normalizedPos = (localPos - (Vector2)sdfSprite.sprite.bounds.min);
                normalizedPos.x /= sdfSprite.sprite.bounds.size.x;
                normalizedPos.y /= sdfSprite.sprite.bounds.size.y;

                int xi = Mathf.FloorToInt(normalizedPos.x * textureWidth);
                int yi = Mathf.FloorToInt(normalizedPos.y * textureHeight);

                #if UNITY_EDITOR
                float distance;
                try
                {
                    distance = pixelData[xi + yi * textureWidth] / 255f;
                } catch(ObjectDisposedException)  // this can be disposed when regenerating sdfs.
                {
                    Init();
                    distance = pixelData[xi + yi * textureWidth] / 255f;
                }
                #else
                float distance = pixelData[xi + yi * textureWidth] / 255f;
                #endif


                return distance <= maximumHitDistance;
            }

            return false;
        }
    }
}

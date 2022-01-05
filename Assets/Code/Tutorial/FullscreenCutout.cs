using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class FullscreenCutout : MonoBehaviour
    {
        [SerializeField]
        RawImage textureHolder;
        [SerializeField]
        Camera maskCamera;
        [SerializeField, Layer]
        int maskLayer;
        [SerializeField]
        GameObject maskPrefab;
        [SerializeField]
        Transform maskRoot;

        public Color CoverColor { get => textureHolder.color; set { textureHolder.color = value; } }

        RenderTexture fullscreenTexture = null;
        int texW = 0;
        int texH = 0;

        List<GameObject> cutouts = new List<GameObject>();
        [SerializeField]
        GameObject ta;

        public void Clear()
        {
            foreach (var go in cutouts)
            {
                Destroy(go);
            }

            cutouts.Clear();
        }

        //Cutout for scene Objects
        public void AddCutout(SpriteRenderer fromRenderer, Camera cam)
        {
            Vector2 sMin = cam.WorldToScreenPoint(fromRenderer.bounds.min);
            Vector2 sMax = cam.WorldToScreenPoint(fromRenderer.bounds.max);

            Vector2 min = maskCamera.ScreenToWorldPoint(sMin);
            Vector2 max = maskCamera.ScreenToWorldPoint(sMax);

            GameObject go = Instantiate(maskPrefab, maskRoot, false);
            go.layer = maskLayer;

            go.transform.position = (min + max) * 0.5f;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();


            //Orig Size
            //sr.size = (max - min) + new Vector2(40f, 40f);
            sr.size = (max - min) + new Vector2(3440 * 80 / Screen.width, 3440 * 40 / Screen.height);

            //Debug.Log($"X: {sr.size.x} + Y: {sr.size.y}");

            cutouts.Add(go);
        }

        //Cutout for UI
        public void AddCutout(Graphic fromGraphic)
        {
            Vector2 min = maskCamera.ScreenToWorldPoint(fromGraphic.rectTransform.rect.min + (Vector2)fromGraphic.transform.position);
            Vector2 max = maskCamera.ScreenToWorldPoint(fromGraphic.rectTransform.rect.max + (Vector2)fromGraphic.transform.position);

            GameObject go = Instantiate(maskPrefab, maskRoot, false);
            go.layer = maskLayer;

            go.transform.position = (min + max) * 0.5f;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

            Vector2 renderExtraSize = Vector2.zero;

            //Screen Based Computation
            renderExtraSize.x = 80f + (Screen.width/ 3440);
            renderExtraSize.y = 40f + (Screen.height / 3440f);

            sr.size = (max - min) + renderExtraSize;
            
            //Debug.Log($"X: {sr.size.x} + Y: {sr.size.y}");
            cutouts.Add(go);
        }

        private void OnEnable()
        {
            CreateTexture();
        }

        void ReleaseTexture()
        {
            textureHolder.texture = null;
            textureHolder.enabled = false;

            if (fullscreenTexture != null)
            {
                RenderTexture.ReleaseTemporary(fullscreenTexture);
                fullscreenTexture = null;
            }
        }

        void CreateTexture()
        {
            ReleaseTexture();

            fullscreenTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 8, RenderTextureFormat.R8);
            texW = Screen.width;
            texH = Screen.height;
            textureHolder.texture = fullscreenTexture;
            textureHolder.enabled = true;
        }

        private void Update()
        {
            if (texW != Screen.width || texH != Screen.height || fullscreenTexture == null)
                CreateTexture();

            maskCamera.targetTexture = fullscreenTexture;
            maskCamera.Render();
            maskCamera.targetTexture = null;
        }

        private void OnDisable()
        {
            ReleaseTexture();
        }
    }

}

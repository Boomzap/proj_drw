using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace ho
{
    public class DarkenWithMask : MonoBehaviour, ICanvasRaycastFilter
    {
        [SerializeField] Image   darkenMaskMaster;
        public Image blackoutImage;
        public GameObject maskHolder;

        List<Image> darkenMaskList = new List<Image>();
        public Canvas canvas;

        public float maxAlpha = 0.3f;
        float currentAlpha = 0f;

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (darkenMaskList.Count == 0) return false;

            foreach (var img in darkenMaskList)
            {
                var rt = img.gameObject.transform as RectTransform;
                var inRect = RectTransformUtility.RectangleContainsScreenPoint(rt, sp, eventCamera);

                if (inRect) 
                    return false;
            }

            return true;
        }

        public void ClearMaskList()
        {
            foreach(var mask in darkenMaskList)
                Destroy(mask.gameObject);

            darkenMaskList.Clear();
        }

        public void AddMaskCircle(Vector2 worldPos, Vector2 worldSize)
        {
            GameObject go = Instantiate(darkenMaskMaster.gameObject);
            Image newImage = go.GetComponent<Image>();

            go.transform.SetParent(maskHolder.transform);

            newImage.rectTransform.localPosition = worldPos;
            newImage.rectTransform.sizeDelta = worldSize;

            //foreach (var animator in GetComponentsInChildren<Animation>())
                //animator.GetClip("s").isLooping

            darkenMaskList.Add(newImage);
            go.SetActive(true);
        }

        //         public void AddMaskImage(Image image, Vector2 worldPos)
        //         {
        //             GameObject go = Instantiate(darkenMaskMaster.gameObject);
        //             Image newImage = go.GetComponent<Image>();
        // 
        //             go.transform.SetParent(maskHolder.transform);
        // 
        //             newImage.sprite = image.sprite;
        //             newImage.rectTransform.localPosition = worldPos;
        //             newImage.rectTransform.sizeDelta = image.rectTransform.sizeDelta;
        //             newImage.transform.localScale = image.transform.localScale;
        //             go.SetActive(true);
        // 
        //             darkenMaskList.Add(newImage);
        //         }

        public void Show()
        {
            darkenMaskMaster.gameObject.SetActive(false);
            ClearMaskList();

            if (gameObject.activeInHierarchy) return;

            gameObject.SetActive(true);

            StartCoroutine(ShowHideCor(currentAlpha, maxAlpha, 0.2f));
        }

        public void Hide()
        {
            if (!gameObject.activeInHierarchy)
                return;
            StartCoroutine(ShowHideCor(currentAlpha, 0f, 0.2f));
        }

        private void Start()
        {

        }

        private void Update()
        {
            
        }

        IEnumerator ShowHideCor(float from, float to, float timer)
        {
            float curTimer = 0f;

            while (curTimer <= timer)
            {
                float a = curTimer / timer;

                currentAlpha = from + (to - from) * a;
                blackoutImage.color = new Color(0f, 0f, 0f, currentAlpha);

                curTimer += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            blackoutImage.color = new Color(0f, 0f, 0f, to);

            currentAlpha = to;

            if (currentAlpha <= 0f)
            {
                ClearMaskList();
                gameObject.SetActive(false);
            }
        }
    }
}

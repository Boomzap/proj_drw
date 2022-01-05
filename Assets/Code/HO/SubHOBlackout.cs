using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace ho
{
    public class SubHOBlackout : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public float maxAlpha = 0.3f;

        float currentAlpha = 0f;

        IEnumerator FadeColorCor(float from, float to, float timer)
        {
            float curTimer = 0f;

            while (curTimer <= timer)
            {
                float a = curTimer / timer;

                currentAlpha = from + (to - from) * a;
                spriteRenderer.color = new Color(0f, 0f, 0f, currentAlpha);

                curTimer += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            spriteRenderer.color = new Color(0f, 0f, 0f, to);

            if (to == 0f)
                gameObject.SetActive(false);
            currentAlpha = to;
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeColorCor(currentAlpha, 0f, 0.3f));                
        }

        public void Show(Transform behind)
        {
            gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(FadeColorCor(currentAlpha, maxAlpha, 0.3f));

            if (behind.parent == transform.parent)
            {
                int bsi = behind.GetSiblingIndex();
                transform.SetSiblingIndex(bsi-1);
            } else
            {
                Debug.LogError("Someone bad is calling me");
            }
        }

        private void Start()
        {
            
        }
    }
}

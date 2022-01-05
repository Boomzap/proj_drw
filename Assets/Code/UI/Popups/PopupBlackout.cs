using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class PopupBlackout : MonoBehaviour
    {
        public Image blackoutImage;
        public float maxAlpha = 0.3f;

        int currentStackDepth = 0;
        float currentAlpha = 0f;

        IEnumerator FadeColorCor(float from, float to, float timer)
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

            if (currentStackDepth == 0)
                gameObject.SetActive(false);
            currentAlpha = to;
        }

        public void OnHidePopup()
        {
            currentStackDepth--;

            if (currentStackDepth == 0)
            {
                StopAllCoroutines();
                StartCoroutine(FadeColorCor(currentAlpha, 0f, 0.2f));
            }
           
        }

        public void OnShowPopup()
        {
            if (currentStackDepth == 0)
            {
                gameObject.SetActive(true);

                StopAllCoroutines();
                StartCoroutine(FadeColorCor(currentAlpha, maxAlpha, 0.2f));
            }

            currentStackDepth++;
        }


    }
}

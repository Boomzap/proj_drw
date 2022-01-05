using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ho
{
    public class SurveyScreenshot : MonoBehaviour
    {
        float orgZoom = 1f;
        Vector3 orgPos = Vector3.one;
        Vector3 origZoomRef;

        enum ZoomState
        {
            ZoomIn,
            ZoomOut
        }

        ZoomState zoomState;

        SpriteRenderer sr;

        MaterialPropertyBlock block;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            orgZoom = transform.parent.localScale.x;
            //orgPos = transform.parent.localPosition;
            origZoomRef = transform.parent.localScale;
            orgPos = transform.parent.position;

            block = new MaterialPropertyBlock();


            block.SetFloat("_Intensity", 0f);
            block.SetTexture("_MainTex", sr.sprite.texture);
            sr.SetPropertyBlock(block);
        }

        private void OnMouseEnter()
        {
            if (sr.sortingOrder == 5) return;
            block.SetFloat("_Intensity", 0.2f);
            sr.SetPropertyBlock(block);
        }

        private void OnMouseExit()
        {
            if (sr.sortingOrder == 5) return;
            block.SetFloat("_Intensity", 0.0f);
            sr.SetPropertyBlock(block);
        }

        float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1 + c3* Mathf.Pow(x - 1f, 3f) + c1*  Mathf.Pow(x - 1f, 2f);
        }

        float EaseInBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return c3 * x * x * x - c1 * x * x;
        }

        IEnumerator ZoomCor(float s, float t, Func<float, float> easing, Vector3 tpos)
        {
            SurveyEndWorld.isZooming = true;
            float time = 0f; 
            float fromZoom = transform.parent.localScale.x;
            Vector3 fromPos = transform.parent.localPosition;

            while (time < t)
            {
                float a = easing(time/t);
                float sl = Mathf.Lerp(fromZoom, s, a);

                transform.parent.localScale = new Vector3(sl, sl, 1f);
                transform.parent.localPosition = Vector3.Slerp(fromPos, tpos, a);

                time += Time.deltaTime;

                yield return null;
            }

            if (s == orgZoom)
                sr.sortingOrder = 3;

            SurveyEndWorld.isZooming = false;
        }

        public void ZoomIn()
        {
            //if (SurveyEndWorld.isZooming) return;
            zoomState = ZoomState.ZoomIn;

            sr.sortingOrder = 6;

            //Sorry not sure how to debug this
            //StartCoroutine(ZoomCor(3f * orgZoom, 0.4f, EaseOutBack, Vector3.zero));

            iTween.ScaleTo(transform.parent.gameObject, iTween.Hash("scale", 3f * origZoomRef , "time", 0.3f));
            iTween.MoveTo(transform.parent.gameObject, iTween.Hash("position", GameController.instance.MenuScenePosRef, "time", 0.3f, "oncomplete", "OnZoomComplete", "oncompletetarget", gameObject));
            block.SetFloat("_Intensity", 0.0f);


            sr.SetPropertyBlock(block);

            Audio.instance.PlaySound(UIController.instance.defaultClickAudio);
        }

        public void ZoomOut()
        {
            //if (SurveyEndWorld.isZooming) return;

            zoomState = ZoomState.ZoomOut;

            sr.sortingOrder = 5;

            Audio.instance.PlaySound(UIController.instance.defaultClickAudio);

            //Sorry not sure how to debug this
            //StartCoroutine(ZoomCor(orgZoom, 0.4f, EaseInBack, orgPos));

            iTween.ScaleTo(transform.parent.gameObject, iTween.Hash("scale", origZoomRef, "time", 0.3f));
            iTween.MoveTo(transform.parent.gameObject, iTween.Hash("position", orgPos, "time", 0.3f, "oncomplete", "OnZoomComplete", "oncompletetarget", gameObject));
        }

        void OnZoomComplete()
        {
            if(zoomState == ZoomState.ZoomOut)
            {
                sr.sortingOrder = 3;
            }
        }
    }
}

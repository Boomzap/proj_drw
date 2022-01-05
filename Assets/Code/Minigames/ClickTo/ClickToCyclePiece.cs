using System.Collections;
using UnityEngine;

namespace ho
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class ClickToCyclePiece : MonoBehaviour
    {
        public bool isCorrect;
        public ClickToCyclePiece next;

        SpriteRenderer pieceRenderer;
        // Use this for initialization
        void Start()
        {
            pieceRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSelected(bool b)
        {
            pieceRenderer.material = b ?
                 MinigameController.instance.JigsawSelectedMaterial :
                 MinigameController.instance.DefaultMaterial;
        }

        public void OnClick()
        {
            gameObject.SetActive(false);

            next.gameObject.SetActive(true);

            Audio.instance.PlaySound(MinigameController.instance.onPieceSelected.GetClip(null));
        }

        IEnumerator OnSuccessCor()
        {
            var startScale = transform.localScale;

            float t = 0f;
            const float max = 0.3f;

            while (t < max)
            {
                float a = t / max;
                a = Mathf.Sin(a * Mathf.PI);

                transform.localScale = (1f + (a * 0.1f)) * startScale;

                t += Time.deltaTime;
                yield return null;
            }

            transform.localScale = startScale;
        }

        public void OnSuccess()
        {
            StartCoroutine(OnSuccessCor());
        }

        [Sirenix.OdinInspector.Button]
        public void OnFadeOut()
        {
            if (pieceRenderer == null) pieceRenderer = GetComponent<SpriteRenderer>();
            StopAllCoroutines();
            StartCoroutine(AnimateFade(false, 0.5f));
        }

        [Sirenix.OdinInspector.Button]
        public void OnFadeIn()
        {
            if (pieceRenderer == null) pieceRenderer = GetComponent<SpriteRenderer>();
            StopAllCoroutines();
            StartCoroutine(AnimateFade(true, 0.5f));
        }

        IEnumerator AnimateFade(bool fadeIn, float time)
        {
            float start = fadeIn ? 0f : 1f;
            float startTime = time;

            Color colorStart = new Color(1f, 1f, 1f, start);

            while (fadeIn && time > 0f)
            {
                colorStart.a = 1f - (time / startTime);
                pieceRenderer.color = colorStart;
                time -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            while (fadeIn == false & time > 0f)
            {
                colorStart.a = time / startTime;
                pieceRenderer.color = colorStart;
                time -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
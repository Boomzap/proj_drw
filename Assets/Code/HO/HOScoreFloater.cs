using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOScoreFloater : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI scoreFloaterText;

        IEnumerator AnimateFloatCor()
        {
            scoreFloaterText.transform.localScale = Vector3.zero;

            //Fade in and Scale Animation
            scoreFloaterText.CrossFadeAlpha(1f, .15f, false);
            iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuart));
            yield return new WaitForSeconds(0.15f);

            //Fade out and Move Animation
            iTween.MoveTo(gameObject, iTween.Hash("position",   transform.position + Vector3.up * 100f, "time", 1.5f, "easetype", iTween.EaseType.easeOutQuart));
            scoreFloaterText.CrossFadeAlpha(0f, 1f, false);

            yield return new WaitForSeconds(2f);

            Destroy(gameObject);
        }

        [Button]
        public void AnimateScore(int score)
        {
            scoreFloaterText.text = "+" + score;
            StopAllCoroutines();
            StartCoroutine(AnimateFloatCor());
        }
    }
}

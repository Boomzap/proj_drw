using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class SuperHintButton : HOHintButton
    {
        //----------------------------------------------------------------------------------
        //----------------------NOTE* FT1 HINT BUTTON CODE BELOW----------------------------
        //----------------------------------------------------------------------------------

        //[SerializeField] Image origSuperHintImage;
        //[SerializeField] Image superHintImage;

        //public override void ResetHintTimer()
        //{
        //    buttonAnimator.enabled = false;
        //    hintTimer = hintCooldown = HOGameController.instance.GetSuperHintCooldown();
        //    hintFillImage.fillAmount = 1;
        //    superHintImage.fillAmount = 1;
        //}

        //IEnumerator AnimateSuperHintCor()
        //{
        //    while (superHintImage.fillAmount > 0)
        //    {
        //        superHintImage.fillAmount -= Time.deltaTime;
        //        yield return new WaitForEndOfFrame();
        //    }
           
        //    iTween.ScaleTo(origSuperHintImage.gameObject,Vector3.one * 1.3f, 0.3f);
        //    yield return new WaitForSeconds(0.35f);
        //    iTween.ScaleTo(origSuperHintImage.gameObject, Vector3.one, 0.15f);

        //    yield return new WaitForSeconds(1.05f);
        //    buttonAnimator.enabled = true;
        //}

        //public override void OnHintReady()
        //{
        //    hintTimer = 0f;
        //    hintFillImage.fillAmount = 0f;
        //    superHintImage.fillAmount = 0f;

        //    //NOTE* Play this animation only once 
        //    if (buttonAnimator.enabled) return;

        //    StartCoroutine(AnimateSuperHintCor());
        //}

        //protected override void OnHintUsed()
        //{
        //    if (HOGameController.isHintPlaying) return;

        //    if (isHintReady == false) return;

        //    HOGameController.isHintPlaying = true;

        //    if (GameController.instance.CurrentWorldState is HOGameController)
        //        HOGameController.instance.OnSuperHintUse();

        //    ResetHintTimer();
        //}

        //protected override void Awake()
        //{
        //    base.Awake();
        //}
    }
}

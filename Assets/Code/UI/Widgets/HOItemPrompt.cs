using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ho
{
    public class HOItemPrompt : MonoBehaviour
    {
        [SerializeField] AnimationClip onShowAnimation;
        [SerializeField] AnimationClip onHideAnimation;

        [SerializeField] TextMeshProUGUI promptText;
        [SerializeField] float timeBeforeExit = 3f;

        float timer = 0f;
        bool isPromptActive => gameObject.activeInHierarchy;

        public void DisablePrompt()
        {
            if (isPromptActive == false) return;

            AnimUtil.PlayAnimation(gameObject, this, onHideAnimation.name, () => gameObject.SetActive(false));
        }

        public void ShowPrompt(string text)
        {
            promptText.text = text;

            if (isPromptActive == false)
            {
                gameObject.SetActive(true);
                //Stop Playing Hide Animation
                AnimUtil.StopAnimation(gameObject, this, onHideAnimation.name);
                //NOTE* Show the animation only when prompt is not active
                AnimUtil.PlayAnimation(gameObject, this, onShowAnimation.name);
            }
            timer = timeBeforeExit;
        }


        private void Update()
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;
                if(timer <= 0)
                {
                    DisablePrompt();
                }
            }
        }
    }
}

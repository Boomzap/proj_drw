using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Jobs;

namespace ho
{
    public static class AnimUtil
    {
        private static IEnumerator PlayAnimationCor(Animation animCtrl, AnimationClip animation, UnityAction onComplete = null, float delay = 0f)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            AnimationState animState = animCtrl.PlayQueued(animation.name);

            if (animState)
            {
                yield return new WaitForSeconds(animation.length);

                while (animState != null && animState.time < animation.length)
                    yield return new WaitForEndOfFrame();
            }

            animation.SampleAnimation(animCtrl.gameObject, animation.length);
            onComplete?.Invoke();
        }

        public static void SetToFirstFrameOfAnimation(this GameObject onObject, string animationName)
        {
            Animation animCtrl = onObject.GetComponent<Animation>();

            if (!animCtrl)
            {
                Debug.LogWarning("Object does not have an Animation component: " + onObject);
                return;
            }

            if (!onObject.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Object is not active when trying to play animation: " + onObject);
                return;
            }


            AnimationClip clip = animCtrl.GetClip(animationName);

            if (clip == null)
            {
                Debug.LogWarning($"Animation not found: {animationName}");
                return;
            }

            clip.SampleAnimation(onObject, 0f);
        }

        public static void SetToLastFrameOfAnimation(this GameObject onObject, string animationName)
        {
            Animation animCtrl = onObject.GetComponent<Animation>();

            if (!animCtrl)
            {
                Debug.LogWarning("Object does not have an Animation component: " + onObject);
                return;
            }

            if (!onObject.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Object is not active when trying to play animation: " + onObject);
                return;
            }


            AnimationClip clip = animCtrl.GetClip(animationName);

            if (clip == null)
            {
                Debug.LogWarning($"Animation not found: {animationName}");
                return;
            }

            clip.SampleAnimation(onObject, clip.length);
        }

        public static void PlayAnimation(this GameObject onObject, MonoBehaviour owner, string animationName, UnityAction onComplete = null, float delay = 0f)
        {
            Animation animCtrl = onObject.GetComponent<Animation>();

            if (!animCtrl)
            {
                Debug.LogWarning("Object does not have an Animation component: " + onObject);
                return;
            }

            if (!onObject.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Object is not active when trying to play animation: " + onObject);
                return;
            }
        
        
            AnimationClip clip = animCtrl.GetClip(animationName);

            if (animCtrl.isPlaying && clip)
            {
                //Debug.LogWarning("Object already has an animation playing: " + onObject);
                animCtrl.Stop();
            }


            if (!clip)
            {
                string anims = "";
                Debug.LogWarning("Invalid animation: " + animationName);
                foreach (var t in animCtrl)
			    {
				    if (t == null)	anims += " <null>"; else
								    anims +=" " + t.ToString();
			    }
			    Debug.LogWarning("Valid animations : " + anims);
                onComplete?.Invoke();
            } else
            {
                owner.StartCoroutine(PlayAnimationCor(animCtrl, clip, onComplete, delay));
            }
        }


        public static void StopAnimation(this GameObject onObject, MonoBehaviour owner, string animationName, UnityAction onComplete = null, float delay = 0f)
        {
            Animation animCtrl = onObject.GetComponent<Animation>();

            if (!animCtrl)
            {
                Debug.LogWarning("Object does not have an Animation component: " + onObject);
                return;
            }

            if (!onObject.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Object is not active when trying to play animation: " + onObject);
                return;
            }


            AnimationClip clip = animCtrl.GetClip(animationName);

            if (animCtrl.isPlaying && clip)
            {
                //Debug.LogWarning("Object already has an animation playing: " + onObject);
                animCtrl.Stop();
            }
        }
    }
}
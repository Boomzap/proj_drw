using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace ho
{
    public class BaseUI : MonoBehaviour
    {
        [Header("Animations (must also exist in Animation component)")]
        public AnimationClip    onShowAnimation;
        public AnimationClip    onHideAnimation;

        public delegate         void ShowHideCallback();

        // will get cleared when executed once
        public ShowHideCallback    onHiddenOneshot = null;
        public ShowHideCallback    onShownOneshot = null;

        protected GraphicRaycaster graphicRaycaster;

        void Start()
        {
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        void Update()
        {

        }

        public virtual void Init()
        {
            
        }

        protected virtual void OnBeginShow(bool instant)
        {
            gameObject.SetActive(true);

            if (onShowAnimation && !instant)
            {
                gameObject.PlayAnimation(this, onShowAnimation.name, () => OnFinishShow());
            } else
            {
                OnFinishShow();
            }
        }

        protected virtual void OnFinishShow()
        {
            onShownOneshot?.Invoke();
            
            onShownOneshot = null;
        }

        protected virtual void OnBeginHide(bool instant)
        {
            if (onHideAnimation && !instant)
            {
                gameObject.PlayAnimation(this, onHideAnimation.name, () => OnFinishHide());
            } else
            {
                OnFinishHide();
            }
        }

        protected virtual void OnFinishHide()
        {
            gameObject.SetActive(false);

            onHiddenOneshot?.Invoke();

            onHiddenOneshot = null;
        }

        public virtual void Hide(bool instant = false)
        {
            //             if (!gameObject.activeInHierarchy)
            //                 return;
            DisableInputForHideAnimation();
            OnBeginHide(instant);
        }

        public virtual void EnableRaycaster()
        {
            if (graphicRaycaster == null)
                graphicRaycaster = GetComponent<GraphicRaycaster>();

            graphicRaycaster.enabled = true;
        }

        public virtual void DisableInputForShowAnimation()
        {
            if (graphicRaycaster == null)
                graphicRaycaster = GetComponent<GraphicRaycaster>();

            graphicRaycaster.enabled = false;
            onShownOneshot += () => graphicRaycaster.enabled = true;
        }

        public virtual void DisableInputForHideAnimation()
        {
            if (graphicRaycaster == null)
                graphicRaycaster = GetComponent<GraphicRaycaster>();

            graphicRaycaster.enabled = false;
            onHiddenOneshot += () => graphicRaycaster.enabled = true;
        }

        public virtual void Show(bool instant = false)
        {
            DisableInputForShowAnimation();

            OnBeginShow(instant);
        }
    }
}
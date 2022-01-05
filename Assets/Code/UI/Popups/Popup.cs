using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    public class Popup : BaseUI
    {
        protected virtual bool UseBlackout => true;
        Vector3 baseScale;

        public static T GetPopup<T>() where T : Popup
        {
            return UIController.instance.GetPopup<T>();
        }

        public static T ShowPopup<T>() where T : Popup
        {
            T popup = UIController.instance.GetPopup<T>();

            if (popup)
            {
                popup.Show();
                return popup;
            }

            return null;
        }
        public static T ShowPopup<T>(ShowHideCallback onHide) where T : Popup
        {
            T popup = UIController.instance.GetPopup<T>();

            if (popup)
            {
                popup.Show();
                popup.onHiddenOneshot += onHide;
                return popup;
            }

            return null;
        }

        public static bool IsPopupActive<T>() where T : Popup
        {
            T popup = UIController.instance.GetPopup<T>();

            if (popup)
            {
                return popup.gameObject.activeInHierarchy;
            }

            return false;
        }


        public static T HidePopup<T>() where T : Popup
        {
            T popup = UIController.instance.GetPopup<T>();

            if (popup)
            {
                popup.Hide();
                return popup;
            }

            return null;
        }

        protected override void OnBeginShow(bool instant)
        {
            //base.OnBeginShow(instant);
        }

        protected override void OnBeginHide(bool instant)
        {
            //base.OnBeginHide(instant);
        }

        protected virtual void Awake()
        {
            baseScale = gameObject.transform.localScale;
        }

        public override void Show(bool instant = false)
        {
            if (gameObject.activeInHierarchy)
                return;

            gameObject.SetActive(true);

            OnBeginShow(instant);

            transform.localScale = baseScale;

            if (onShowAnimation != null)
            {
                gameObject.PlayAnimation(this, onShowAnimation.name, () => OnFinishShow());
            }
            else
            {
                iTween.ScaleFrom(gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeOutBack"));
                this.ExecuteAfterDelay(0.3f, OnFinishShow);
            }


          

            if (UseBlackout)
            {
                UIController.instance.popupBlackout.OnShowPopup();
                UIController.instance.popupBlackout.transform.SetAsLastSibling();
            }

            transform.SetAsLastSibling();
        }

        public override void Hide( bool instant = false)
        {
            if (!gameObject.activeInHierarchy)
                return;

            OnBeginHide(instant);


            if (onHideAnimation != null)
            {
                gameObject.PlayAnimation(this, onHideAnimation.name, () => OnFinishHide());
            }
            else
            {
                iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeInBack"));
                this.ExecuteAfterDelay(0.3f, OnFinishHide);
            }

            if (UseBlackout)
            {
                //transform.SetAsLastSibling();
                //int sibIdx = transform.GetSiblingIndex();
                //UIController.instance.popupBlackout.transform.SetSiblingIndex(System.Math.Max(0,sibIdx - 2));
            }
        }

        protected override void OnFinishHide()
        {
            if (UseBlackout)
            {
                UIController.instance.popupBlackout.OnHidePopup();
                transform.SetAsFirstSibling();
                UIController.instance.popupBlackout.transform.SetSiblingIndex(System.Math.Max(0, transform.parent.childCount - 2));

                //transform.SetAsLastSibling();
                //int sibIdx = transform.GetSiblingIndex();
                //UIController.instance.popupBlackout.transform.SetSiblingIndex(System.Math.Max(0,sibIdx - 2));
            }
            base.OnFinishHide();
        }
    }
}

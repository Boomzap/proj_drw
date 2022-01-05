using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ho
{
    public class BaseColor : MonoBehaviour
    {
        public Image colorImage;

        protected BaseColorHolder holder;

        private void Awake()
        {
            holder = GetComponentInParent<BaseColorHolder>();
        }

        public EventTrigger eventTrigger;

        public virtual void OnPointerEnterDelegate(PointerEventData data)
        {
            SetSelected(true);
        }

        public virtual void OnPointerExitDelegate(PointerEventData data)
        {
            SetSelected(false);
        }

        public virtual void OnPointerDownDelegate(PointerEventData data)
        {
            holder.SelectColor(this);
        }

        public void SetSelected(bool selected)
        {
            if (selected) // Animate Scale Up
                iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
            else // Animate Scale Down
                iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
        }

        void EnableMouseEvents()
        {
            AddEventTrigger((data) => { OnPointerEnterDelegate((PointerEventData)data); }, EventTriggerType.PointerEnter);
            AddEventTrigger((data) => { OnPointerExitDelegate((PointerEventData)data); }, EventTriggerType.PointerExit);
            AddEventTrigger((data) => { OnPointerDownDelegate((PointerEventData)data); }, EventTriggerType.PointerDown);
        }

        void AddEventTrigger(UnityAction<BaseEventData> eventEntry, EventTriggerType triggerType)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = triggerType;
            entry.callback.AddListener(eventEntry);
            eventTrigger.triggers.Add(entry);
        }

        private void OnEnable()
        {
            eventTrigger.triggers.Clear();
            EnableMouseEvents();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    [AddComponentMenu("ho/ChapterPreviewAnimator")]
    [RequireComponent(typeof(Button))]
    public class ChapterPreviewAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
    {
        Button                      button;

        [SerializeField]
        Image                       selectedImage;
        [SerializeField]
        Image                       highlightedImage;

        [Serializable]
        internal class State
        {
            public float        selectedAlpha;
            public float        highlightedAlpha;

            public State Clone()
            {
                State state = new State();

                state.selectedAlpha = selectedAlpha;
                state.highlightedAlpha = highlightedAlpha;

                return state;
            }
        }

        [SerializeField]    float transitionTime = 0.1f;
        [SerializeField]    AudioClip onHoverAudio;
        [SerializeField]    AudioClip onClickAudio;

        State currentState = new State();
        State targetState = new State();
        State sourceState = new State();

        Vector2 baseScale;

        float transitionStartTime = 0f;

        bool _isSelected = false;
        public bool isSelected 
        {
            get { return _isSelected; }
            set 
            { 
                _isSelected = value;
                targetState.selectedAlpha = value ? 1f : 0f; 
                sourceState.selectedAlpha = currentState.selectedAlpha; 
                sourceState.highlightedAlpha = currentState.highlightedAlpha; 
                transitionStartTime = Time.time; 
            }
        }

        void Reset()
        {
            float d;
            onHoverAudio = Audio.instance.audioTracker.GetClip("ui_hover", out d);
            onClickAudio = Audio.instance.audioTracker.GetClip("ui_click", out d);
        }

        void Awake()
        {
            button = GetComponent<Button>();

            baseScale = button.transform.localScale;
        }

        private void Update()
        {
            if (!button.interactable)
            {
                selectedImage.gameObject.SetActive(false);
                highlightedImage.gameObject.SetActive(false);
                return;
            }

            float a = Mathf.Clamp((Time.time - transitionStartTime) / transitionTime, 0f, 1f);

            currentState.highlightedAlpha = Mathf.Lerp(sourceState.highlightedAlpha, targetState.highlightedAlpha, a);
            currentState.selectedAlpha = Mathf.Lerp(sourceState.selectedAlpha, targetState.selectedAlpha, a);

            selectedImage.gameObject.SetActive(currentState.selectedAlpha > 0f);
            selectedImage.color = new Color(1f, 1f, 1f, currentState.selectedAlpha);

            highlightedImage.gameObject.SetActive(currentState.highlightedAlpha > 0f);
            highlightedImage.color = new Color(1f, 1f, 1f, currentState.highlightedAlpha);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
     
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClickAudio && button.interactable)
                Audio.instance.PlaySound(onClickAudio.name);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.interactable)
            {
                sourceState.selectedAlpha = currentState.selectedAlpha;
                sourceState.highlightedAlpha = currentState.highlightedAlpha;
                targetState.highlightedAlpha = 1f;
                transitionStartTime = Time.time;
                if (onHoverAudio)
                    Audio.instance.PlaySound(onHoverAudio.name);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            sourceState.selectedAlpha = currentState.selectedAlpha;
            sourceState.highlightedAlpha = currentState.highlightedAlpha;
            targetState.highlightedAlpha = 0f;
            transitionStartTime = Time.time;
        }

    }
}

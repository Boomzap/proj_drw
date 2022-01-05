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
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    [AddComponentMenu("ho/ButtonAnimator")]
    [RequireComponent(typeof(ButtonMaterialController), typeof(Button))]
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
    {
        ButtonMaterialController    matCtrl;
        Button                      button;

        Sprite currentSprite;

        [BoxGroup("Button Sprites")]
        [SerializeField] Sprite normalSprite;
        [BoxGroup("Button Sprites")]
        [SerializeField] Sprite selectedSprite;
        [BoxGroup("Button Sprites")]
        [SerializeField] Sprite unavailableSprite;
        [BoxGroup("Button Sprites")]
        [SerializeField] Sprite hoverSprite;

        [BoxGroup("Button Triggers")]
        public bool isSelected;
        [BoxGroup("Button Triggers")]
        public bool isAvailable;

        [Serializable]
        internal class State
        {
            public float        lightenFactor;
            public float        desaturateFactor;
            public Vector2      scaleFactor;

            public State Clone()
            {
                State state = new State();

                state.lightenFactor = lightenFactor;
                state.desaturateFactor = desaturateFactor;
                state.scaleFactor = new Vector2(scaleFactor.x, scaleFactor.y);

                return state;
            }
        }

        [InfoBox("If this is set, the button itself won't be scaled by default")]
        [SerializeField]    Transform[] explicitScale = new Transform[0];

        [SerializeField]    State normalState = new State { desaturateFactor = 0f, lightenFactor = 0f, scaleFactor = Vector2.one };
        [SerializeField]    State hoverState = new State { desaturateFactor = 0f, lightenFactor = 0.2f, scaleFactor = Vector2.one * 1.02f };
        [SerializeField]    State disabledState = new State { desaturateFactor = 1f, lightenFactor = 0f, scaleFactor = Vector2.one };
        [SerializeField]    State clickState = new State { desaturateFactor = 0f, lightenFactor = -0.1f, scaleFactor = Vector2.one * 0.99f };
        [SerializeField]    float transitionTime = 0.1f;
        [SerializeField]    AudioClip onHoverAudio;
        [SerializeField]    AudioClip onClickAudio;

        State currentState = new State();
        State _targetState;
        State sourceState;

        State targetState { set { _targetState = value; sourceState = currentState.Clone(); transitionStartTime = Time.time; } get { return _targetState; } }

        Vector2 baseScale;
        Vector2[] explicitBaseScales;

        float transitionStartTime = 0f;

        void Reset()
        {
            float d;
            onHoverAudio = Audio.instance.audioTracker.GetClip("ui_hover", out d);
            onClickAudio = Audio.instance.audioTracker.GetClip("ui_click", out d);
        }

        void Awake()
        {
            button = GetComponent<Button>();
            matCtrl = GetComponent<ButtonMaterialController>();

            currentState = button.interactable ? normalState.Clone() : disabledState.Clone();
            targetState = currentState.Clone();
            sourceState = currentState.Clone();

            baseScale = button.transform.localScale;
            explicitBaseScales = explicitScale.Select(x => (Vector2)x.transform.localScale).ToArray();

            currentSprite = normalSprite;
        }

        private void Update()
        {
            if (!button.interactable)
            {
                matCtrl.lightIntensity = disabledState.lightenFactor;
                matCtrl.desatIntensity = disabledState.desaturateFactor;

                if (explicitScale.Length > 0)
                    for (int i = 0; i < explicitScale.Length; i++)
                        explicitScale[i].localScale = explicitBaseScales[i] * disabledState.scaleFactor;
                else
                    button.transform.localScale = baseScale * disabledState.scaleFactor;
                return;
            }

            float a = Mathf.Clamp((Time.time - transitionStartTime) / transitionTime, 0f, 1f);

            currentState.desaturateFactor = Mathf.Lerp(sourceState.desaturateFactor, targetState.desaturateFactor, a);
            currentState.lightenFactor = Mathf.Lerp(sourceState.lightenFactor, targetState.lightenFactor, a);
            currentState.scaleFactor = sourceState.scaleFactor + (targetState.scaleFactor - sourceState.scaleFactor) * a;

            matCtrl.lightIntensity = currentState.lightenFactor;
            matCtrl.desatIntensity = currentState.desaturateFactor;
            if (explicitScale.Length > 0)
                for (int i = 0; i < explicitScale.Length; i++)
                    explicitScale[i].localScale = explicitBaseScales[i] * currentState.scaleFactor;
            else
                button.transform.localScale = baseScale * currentState.scaleFactor;

            //For Select Animation
            if (currentSprite != null && selectedSprite != null)
            {
                if (isSelected)
                {
                    currentSprite = selectedSprite;
                }
                else
                {
                    currentSprite = isAvailable ? normalSprite : unavailableSprite;
                }

                matCtrl.linkedImage.sprite = currentSprite;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetState = clickState.Clone();

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetState = hoverState.Clone();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClickAudio && button.interactable)
                Audio.instance.PlaySound(onClickAudio.name);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetState = hoverState.Clone();

            if (button.interactable)
            {
                if(hoverSprite != null)
                {
                    currentSprite = matCtrl.linkedImage.sprite = hoverSprite;
                }

                if (onHoverAudio)
                {
                    Audio.instance.PlaySound(onHoverAudio.name);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetState = normalState.Clone();
            if(hoverSprite != null && currentSprite == hoverSprite)
            {
                currentSprite = matCtrl.linkedImage.sprite = normalSprite;
            }
        }
    }
}

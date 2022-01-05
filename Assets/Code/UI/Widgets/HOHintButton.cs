using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

using Boomzap.Character;
using Boomzap.Conversation;

using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace ho
{
    public class LayerAttribute : PropertyAttribute
    {

    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    internal class LayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }

#endif
    //----------------------------------------------------------------------------------
    //----------------------NOTE* FT1 HINT BUTTON CODE BELOW----------------------------
    //----------------------------------------------------------------------------------

    //public class HOHintButton : MonoBehaviour
    //{
    //    [SerializeField] protected Button hintButton;
    //    [SerializeField] protected Image hintFillImage;
    //    [SerializeField] protected ButtonAnimator buttonAnimator;

    //    protected float hintTimer = 0f;
    //    protected float hintCooldown = 0f;

    //    bool isPlayingHOScene => GameController.instance.CurrentWorldState is HOGameController;

    //    bool disableInput => IsInputDisabled();

    //    public bool isHintReady => hintTimer <= 0;


    //    bool IsInputDisabled()
    //    {
    //        if (GameController.instance.CurrentWorldState is HOGameController)
    //            return HOGameController.instance.DisableInput;
    //        else if (GameController.instance.CurrentWorldState is MinigameController)
    //            return MinigameController.instance.ActiveMinigame.disableInput;

    //        return true;
    //    }

    //    public virtual void OnHintReady()
    //    {
    //        hintTimer = 0f;
    //        hintFillImage.fillAmount = 0f;
    //        buttonAnimator.enabled = true;
    //    }

    //    protected virtual void OnHintUsed()
    //    {
    //        if (isPlayingHOScene)
    //        {
    //            if (HOGameController.isHintPlaying)
    //            {
    //                Debug.LogWarning("Hint is still playing!");
    //                return;
    //            }

    //            if (isHintReady == false)
    //            {
    //                Debug.LogWarning("Hint is not ready!");
    //                return;
    //            }

    //            HOGameController.isHintPlaying = true;
    //            HOGameController.instance.OnHintUsed();
    //        }
    //        else
    //        {
    //            if (MinigameController.instance.isHintPlaying)
    //            {
    //                Debug.LogWarning("Hint is still playing!");
    //                return;
    //            }

    //            if(isHintReady == false)
    //            {
    //                Debug.LogWarning("Hint is not ready!");
    //                return;
    //            }

    //            MinigameController.instance.isHintPlaying = true;
    //            UIController.instance.minigameUI.OnHintUsed();
    //        }


    //        ResetHintTimer();
    //    }

    //    [Button]
    //    public virtual void ResetHintTimer()
    //    {
    //        buttonAnimator.enabled = false;
    //        hintTimer = hintCooldown = HOGameController.instance.GetHintCooldown();
    //        hintFillImage.fillAmount = 1;
    //    }

    //    void UpdateHintTimer()
    //    {
    //        hintTimer -= Time.deltaTime;
    //        hintFillImage.fillAmount = hintTimer / hintCooldown;
    //    }

    //    private void Update()
    //    {
    //        if(disableInput == false)
    //        {
    //            if(isHintReady == false)
    //            {
    //                UpdateHintTimer();
    //            }
    //            else
    //            {
    //                OnHintReady();
    //            }    
    //        }
    //    }

    //    protected virtual void Awake()
    //    {
    //        hintButton.onClick.RemoveAllListeners();
    //        hintButton.onClick.AddListener(OnHintUsed);
    //    }
    //}


    //----------------------------------------------------------------------------------
    //----------------------NOTE* HO4 HINT BUTTON CODE BELOW----------------------------
    //----------------------------------------------------------------------------------

    public class HOHintButton : MonoBehaviour
    {
        [SerializeField] Image fillProgressImage;
        [SerializeField] Image fillProgressBacking;
        [SerializeField] Image hintFrameImage;
        [SerializeField] RawImage characterSurface;
        [SerializeField] Camera characterCamera;
        [SerializeField] Vector2 characterOffset;
        [SerializeField] Button button;
        [Layer, SerializeField] int renderLayer;
        [SerializeField] float characterScale = 100f;
        [SerializeField] AudioClip hintReadyAudio;

        ConversationNode.CharacterStateDefinition displayState;
        Character loadedCharacter;
        RenderTexture renderTexture;

        bool isLoading = false;
        float cooldownProgress = 0f;

        float cooldownTimer = 0f;
        float currentTime = 0f;

        public float CooldownProgress
        {
            get => cooldownProgress;
            set
            {
                currentTime = 0;
                UpdateHintFill();
            }
        }

        public bool IsHintReady
        {
            get
            {
                return cooldownProgress >= 1;
            }
        }

        bool isInHOScene => GameController.instance.CurrentWorldState is HOGameController;


        [SerializeField] Canvas mainCanvas;

        // Use this for initialization
        void Start()
        {
            Material mi = GetComponent<ButtonMaterialController>().MaterialInstance;

            fillProgressBacking.material = mi;
            fillProgressImage.material = mi;
            characterSurface.material = mi;
            hintFrameImage.material = mi;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnHintUsed);
        }

        private void OnDisable()
        {
            if (loadedCharacter)
            {
                Addressables.ReleaseInstance(loadedCharacter.gameObject);
                loadedCharacter = null;
            }

            if (renderTexture)
            {
                renderTexture = null;
            }
        }

        private void OnEnable()
        {
            renderTexture = new RenderTexture((int)(characterCamera.orthographicSize * characterCamera.aspect * 4f), (int)(characterCamera.orthographicSize * 4f), 32);
            characterSurface.texture = renderTexture;
        }

        // Update is called once per frame
        void Update()
        {
            characterSurface.gameObject.SetActive(loadedCharacter != null);

            if (loadedCharacter != null)
            {
                UpdateRenderTexture();
            }

            if (isInHOScene && HOGameController.instance.DisableInput) return;

            if (isInHOScene == false && MinigameController.instance.ActiveMinigame != null && MinigameController.instance.ActiveMinigame.disableInput) return;

            UpdateHintFill();
        }

        public void ResetHintCooldown()
        {
            cooldownTimer = HOGameController.instance.GetHintCooldown();
            currentTime = 0;
            cooldownProgress = currentTime / cooldownTimer;
            UpdateHintFill();
        }

        void UpdateHintFill()
        {
            //float mw = fillProgressBacking.rectTransform.sizeDelta.x;
            //float p = Mathf.Clamp01(cooldownProgress);

            //if (button.interactable)
            //    p = 1f;

            //fillProgressImage.rectTransform.sizeDelta = new Vector2(mw * p, fillProgressImage.rectTransform.sizeDelta.y);

            currentTime += Time.deltaTime;

            cooldownProgress = currentTime / cooldownTimer;

            fillProgressImage.fillAmount = cooldownProgress;

            if(button.interactable == false && cooldownProgress >= 1f)
            {
                OnHintReady();
            }
        }

        public void SetCharacter(Boomzap.Character.CharacterInfo character)
        {
            displayState = new ConversationNode.CharacterStateDefinition();

            displayState.character = character;
            displayState.emotion = "normal";
            displayState.eyes = "look_cam";
            displayState.state = "Default.idle";
            displayState.faceLeft = true;
            displayState.lookBack = false;

            SetCharacterState(displayState);
        }

        public void OnHintReady()
        {
            displayState.emotion = "happy";

            SetCharacterState(displayState);

            button.interactable = true;


            Audio.instance.PlaySound(hintReadyAudio);
        }

        public void OnHintUsed()
        {
            if (GameController.instance.CurrentWorldState is HOGameController)
                HOGameController.instance.OnHintUsed();
            else if (GameController.instance.CurrentWorldState is MinigameController )
                UIController.instance.minigameUI.OnHintUsed();

            DisableHint();
        }

        public void DisableHint()
        {
            ResetHintCooldown();

            displayState.emotion = "normal";
            SetCharacterState(displayState);
            button.interactable = false;
        }

        public void SetCharacterState(ConversationNode.CharacterStateDefinition stateDefinition)
        {
            if (loadedCharacter != null && loadedCharacter.characterInfo != stateDefinition.character)
            {
                Addressables.ReleaseInstance(loadedCharacter.gameObject);
                loadedCharacter = null;
            }

            FetchCharacterAsync(stateDefinition.character, (Character c) =>
            {
                c.SetState(stateDefinition.state);

                if (!c.GetCurrentEmotion.allEyes.Contains(stateDefinition.eyes) && stateDefinition.eyes == "look_cam")
                    stateDefinition.eyes = "look_camera";

                c.SetEmotion(stateDefinition.emotion, stateDefinition.eyes);
                c.SetLookBack(stateDefinition.lookBack);
                c.Flip(!stateDefinition.faceLeft);

                UpdateCharacterPosition();
            });
        }

        void UpdateCharacterPosition()
        {
            // we want the head to be here
            Vector3 tPoint = characterCamera.transform.position;

            if (loadedCharacter != null)
            {
                var s = loadedCharacter.FaceBounds.position;

                Vector3 delta = tPoint - s;

                Vector3 newPos = loadedCharacter.transform.position + delta + (Vector3)characterOffset;
                newPos.z = 10f;

                loadedCharacter.transform.position = newPos;
                characterSurface.texture = renderTexture;
            }
        }

        void UpdateRenderTexture()
        {
            if (characterCamera == null) return;

            characterCamera.aspect = 16f / 9f;
            characterCamera.targetTexture = renderTexture;
            characterCamera.Render();
            characterCamera.targetTexture = null;
        }

        void FetchCharacterAsync(Boomzap.Character.CharacterInfo info, UnityAction<Character> andThen)
        {
            if (loadedCharacter != null &&
                loadedCharacter.characterInfo == info)
            {
                andThen?.Invoke(loadedCharacter);
                return;
            }

            if (isLoading)
            {
                return;
            }

            isLoading = true;

            var handle = info.characterRef.InstantiateAsync(characterCamera.transform, false);
            // completed synchronously, i was already loaded
            if (handle.IsDone)
            {
                OnAssetInstantiated(handle, info, andThen);
            }
            else
            {
                handle.Completed += (AsyncOperationHandle<GameObject> handle2) => OnAssetInstantiated(handle2, info, andThen);
            }
        }

        void OnAssetInstantiated(AsyncOperationHandle handle, Boomzap.Character.CharacterInfo info, UnityAction<Character> andThen)
        {
            isLoading = false;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (loadedCharacter)
                {
                    Destroy(loadedCharacter.gameObject);
                }

                loadedCharacter = (handle.Result as GameObject).GetComponent<Character>();


                loadedCharacter.name = info.name;
                loadedCharacter.ForceAlpha(1f);

                foreach (var t in loadedCharacter.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = renderLayer;
                gameObject.layer = renderLayer;

                loadedCharacter.gameObject.SetActive(true);

                if (mainCanvas)
                    loadedCharacter.SetScale(characterScale * (1f / mainCanvas.transform.localScale.x));
                else
                    Debug.Log("Main Canvas not found");

                andThen?.Invoke(loadedCharacter);
            }
            else
            {
                Debug.LogError($"Failed to load a character {info.name}");
            }
        }
    }
}
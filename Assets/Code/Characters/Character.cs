using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Experimental;
using Spine;
using Spine.Unity;
#endif

namespace Boomzap.Character
{
    public class Character : MonoBehaviour
    {
        [InlineEditor(inlineEditorMode: InlineEditorModes.GUIOnly, Expanded = true)]
        public CharacterInfo characterInfo;

        //         [SerializeField]
        //         CharacterEmotion  emotion = null;
        [SerializeField]
        bool canFlip = true;

        [ShowInInspector]
        float currentAlpha = 0f;
        public float CurrentAlpha { get => currentAlpha; }
        CharacterAttachment[] attachments;
        string[] characterStates;
        public CharacterSpine[] characterSpines;
        Vector3 baseScale;
        public void SetScale(float s)
        {
            transform.localScale = baseScale * s;
        }

        CharacterSpine currentSpine;
        CharacterEmotion currentEmotion;
        string currentState = "";
        bool isFlipped = false;
        FlagSetBool enabledAttachments = new FlagSetBool();
        public bool IsFlipped { get { return isFlipped; } set { if (canFlip) isFlipped = value; } }
        CharacterEmote emote;

        public CharacterSpineSlot _characterSlot;

        public RectTransform FaceBounds => currentSpine.faceBounds;

        // Start is called before the first frame update
        void Start()
        {

        }

        void Awake()
        {
            baseScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateGfx();
        }

        public string CurrentState
        {
            get { return $"{currentSpine.name}.{currentState}"; }
        }

        public string LastName { get { return characterInfo.lastName; } }
        public string FirstName { get { return characterInfo.firstName; } }

        [Header("Conversation defaults")]
        [ValueDropdown("GetStates")]
        public string defaultState;
        [ValueDropdown("GetEmotions")]
        public string defaultEmotion;
        [ValueDropdown("GetEyes")]
        public string defaultEyes;

        public List<CharacterSpineSlot.SlotToggle> defaultSlots => GetSlotToggles();

        public bool isLookingBack = false;

        public bool IsTalking { get; set; } = false;

        void ResolveExpression()
        {
            if (currentSpine == null) return;
            CharacterEmotion emotion = currentSpine.GetComponentInChildren<CharacterEmotion>();
            if (currentEmotion != null && emotion != null)
            {
                emotion.SetEmotion(currentEmotion.emotion, currentEmotion.eyes);
            }

            currentEmotion = emotion;
        }

        public void Flip(bool flip)
        {
            if (canFlip)
                isFlipped = flip;
        }

        [Button]
        public void UpdateState()
        {
            ForceUpdateWithoutSaving(defaultState, defaultEmotion, defaultEyes, defaultSlots, false, isLookingBack);
        }

        public void SetDefaultState()
        {
            SetState(defaultState);
        }

        public List<string> GetCharacterStates()
        {
            List<string> states = new List<string>();

            foreach (var spine in characterSpines)
            {
                foreach (var state in spine.states)
                {
                    var name = string.IsNullOrWhiteSpace(state.Name) ? state.skin : state.Name;
                    states.Add($"{spine.name}.{name}");
                }
            }

            return states;
        }

        public void UpdateVisible()
        {
            //             if (ShouldShow() && !gameObject.activeSelf)
            //             {
            //                 gameObject.SetActive(true);
            //                 KillEffects();
            //                 UpdateAttachments();
            //                 //ShutdownOld
            //             }
        }

        public void SetAttached(string itemName, bool attached)
        {
            enabledAttachments.Set(itemName, attached);
            UpdateAttachments();
        }

        public void UpdateAttachments()
        {
            if (currentSpine == null) return;
            if (attachments == null) attachments = currentSpine.GetComponentsInChildren<CharacterAttachment>(true);

            foreach (var t in attachments)
            {
                t.gameObject.SetActive(enabledAttachments.Get(t.name, false));
            }
        }

        private void OnEnable()
        {
            KillEffects();
            UpdateAttachments();
        }

        private void OnDisable()
        {
            currentAlpha = 0f;
        }

        public float CurrentAnimDuration
        {
            get
            {
                return currentSpine.skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration;
            }
        }

        public float AnimTime
        {
            get
            {
                return currentSpine.skeletonAnimation.AnimationState.GetCurrent(0).AnimationTime;
            }
        }

        public void ForceAlpha(float f)
        {
            currentAlpha = f;
            UpdateGfx();
        }

        void UpdateGfx()
        {
            if (currentEmotion)
            {
                // -> decouple this from UI, make it set manually by whatever is controlling the renderer
                //currentEmotion.isTalking = true; /*UIController.instance.conversationUI.IsSpeaking(this)*/
                //var curNode = Conversation.ConversationManager.instance.CurrentNode;
                //currentEmotion.isTalking = curNode != null && curNode.speakingCharacter == this.characterInfo; // && presentation of text is not done
                currentEmotion.isTalking = IsTalking;
            }

            Color baseColor = Color.white;
            baseColor.a = currentAlpha;

            if (currentSpine)
            {
                currentSpine.SetColor(baseColor);
                currentSpine.Flip(isFlipped);
            }
            if (currentEmotion)
            {
                currentEmotion.SetColor(baseColor);
                currentEmotion.Flip(!isLookingBack);
            }
            if (attachments != null)
            {
                foreach (var a in attachments) a.SetColor(baseColor);
            }
        }

        public void TriggerFX(string fx)
        {
            //CharacterFX vfx = 
        }

        public bool TestIntersection(Ray ray)
        {
            if (!gameObject.activeInHierarchy) return false;
            if (currentSpine != null) return currentSpine.TestIntersection(ray);
            return false;
        }


        CharacterState GetState(string stateName)
        {
            if (currentSpine == null) return null;
            stateName = StrReplace.CleanStr(stateName);
            return currentSpine.GetState(stateName);
        }

        public CharacterEmotion GetCurrentEmotion => currentEmotion;

        public void SetLookBack(bool value)
        {
            isLookingBack = value;
            if (currentEmotion)
            {
                currentEmotion.Flip(!isLookingBack);
            }
        }

        public void SetEmotion(string emotion, string eyes)
        {
            if (currentEmotion)
                currentEmotion.SetEmotion(emotion, eyes);
        }

        public void SetSpineSlots(List<CharacterSpineSlot.SlotToggle> slotToggles)
        {
            foreach (CharacterSpineSlot.SlotToggle slotToggle in slotToggles)
            {
                CharacterSpineSlot.UpdateSpineSlot(_characterSlot, slotToggle);
            }
        }

        float lastForcedUpdate = 0;
        float forcedUpdateInterval = 0.1f;
        public void ForceUpdateWithoutSaving(string stateName, string emotion, string eyes, List<CharacterSpineSlot.SlotToggle> slotToggles, bool isFlipped, bool isLookingBack)
        {
            //if (!gameObject.activeInHierarchy) return;
            if (Time.time - lastForcedUpdate < forcedUpdateInterval) return;
            lastForcedUpdate = Time.time;
            string[] set = StrReplace.Tokenize(stateName, '.');
            if (set == null || set.Length != 2)
            {
                //Debug.Log("Invalid state " + stateName);
                return;
            }

            CharacterSpine spine = GetSpine(set[0]);
            if (spine)
            {
                if (spine != currentSpine)
                {
                    currentSpine?.gameObject.SetActive(false);
                    spine.gameObject.SetActive(true);
                    currentSpine = spine;
                }

                spine.ForceState(set[1]);
                spine.Flip(isFlipped);
                this.isFlipped = isFlipped;

                CharacterEmotion ce = spine.GetComponentInChildren<CharacterEmotion>();
                ResolveExpression();
                if (ce)
                {
                    ce.SetEmotion(emotion, eyes);
                    ce.Flip(!isLookingBack);
                    this.isLookingBack = isLookingBack;
                }

                //CharacterSpineSlot cs = spine.GetComponentInChildren<CharacterSpineSlot>();
                //if (cs)
                //{
                //    //Debug.Log($"{gameObject.name}Character Slot Found");
                //    cs.SpineSlots = slotToggles;
                //    cs.UpdateAllSlots(cs);
                //}

                UpdateAttachments();
            }
        }

        public void SetState(string value, UnityEngine.Events.UnityAction<CharacterSpine, string> action = null)
        {
            // parse it. format is spline.state
            string[] set = StrReplace.Tokenize(value, '.');
            if (set.Length < 2)
            {
                return; // junk
            }
            CharacterSpine current = currentSpine;

            CharacterSpine spine = GetSpine(set[0]);
            if (spine)
            {
                if (spine != currentSpine)
                {
                    currentSpine?.gameObject.SetActive(false);
                    spine.gameObject.SetActive(true);
                    currentSpine = spine;
                    attachments = null;
                }

                currentState = set[1];

                currentSpine.SetState(currentState);
                currentSpine.Flip(isFlipped);
                ResolveExpression();

                UpdateAttachments();

                action?.Invoke(currentSpine, set[1]);

            }

        }

        public void KillEffects()
        {
            //             VisualEffect[] effects = GetComponentsInChildren<VisualEffect>(true);
            //             foreach (var e in effects)
            //                 Destroy(e.gameObject);

            if (emote)
            {
                emote.gameObject.SetActive(false);
            }
        }

        public void Emote(string emoteName)
        {
            //             if (emote == null)
            //                 emote = Instantiate<CharacterEmote>(/*emote prototype*/, transform)

            if (emote == null || currentSpine == null) return;
            emote.toCharacter = this;

            Transform face = transform;
            if (face != null)
            {
                emote.transform.parent = face;
                emote.transform.localPosition = Vector3.zero;
                emote.transform.localScale = Vector3.one;
                emote.transform.localRotation = Quaternion.identity;

                emote.TriggerEmote(emoteName);
            }
        }

        CharacterSpine GetSpine(string name)
        {
            return characterSpines.First(x => StrReplace.Equals(name, x.name));
        }

        #region IPuppet impl
        public CharacterEmotion GetEmotion(string forState)
        {
            CharacterEmotion emotion = null;
            string[] set = StrReplace.Tokenize(forState, '.');
            if (set == null || set.Length != 2)
            {
                //Debug.Log("Invalid state " + stateName);
                return null;
            }

            CharacterSpine spine = GetSpine(set[0]);
            if (spine)
            {
                emotion = spine.GetComponentInChildren<CharacterEmotion>();
            }

            return emotion;
        }
        public bool IsSpeaking()
        {
            Validate();

            // return UIController.instance.conversationUI.IsSpeaking(this);
            return false;
        }

        public void ForceEmotion(string state, string emotion, string eyes, bool faceLeft, bool lookingBack)
        {
            Validate();
            ForceUpdateWithoutSaving(state, emotion, eyes, null, faceLeft, lookingBack);
        }

        public string[] GetStates()
        {
            Validate();
            return characterStates;
        }

        public string[] GetEmotions()
        {
            CharacterEmotion emotion = GetEmotion(defaultState);
            return emotion?.allEmotions ?? null;
        }

        public string[] GetEyes()
        {
            CharacterEmotion emotion = GetEmotion(defaultState);
            return emotion?.allEyes ?? null;
        }

        public List<CharacterSpineSlot.SlotToggle> GetSlotToggles()
        {
            if (_characterSlot == null)
            {
                Debug.Log("Character Slot is Empty");
                return null;
            }

            if (_characterSlot.slots == null)
            {
                _characterSlot.FillSlotToggles();
                Debug.Log("Character Spine Slots is Empty");
                return _characterSlot.slots;
            }

            return _characterSlot.slots;
        }

        public string[] GetSkins()
        {
            Validate();

            if (characterStates == null) return null;

            List<string> skins = new List<string>();
            foreach (var t in characterStates)
            {
                string[] data = t.Split('.');
                if (data == null || data.Length != 2) continue;
                if (string.IsNullOrWhiteSpace(data[0])) continue;
                if (!skins.Exists(x => StrReplace.Equals(data[0], x)))
                {
                    skins.Add(data[0]);
                }
            }

            if (skins.Count == 0)
                characterStates = null;

            skins.Sort();
            return skins.ToArray();
        }

        public string[] GetStatesInSkin(string forSkin)
        {
            Validate();

            if (characterStates == null) return null;

            List<string> skins = new List<string>();
            foreach (var t in characterStates)
            {
                string[] data = t.Split('.');
                if (data == null || data.Length != 2) continue;
                if (string.IsNullOrWhiteSpace(data[0])) continue;
                if (StrReplace.Equals(data[1], "default")) continue;
                if (!skins.Exists(x => StrReplace.Equals(data[0], x)))
                {
                    skins.Add(data[1]);
                }
            }

            skins.Sort();
            return skins.ToArray();
        }

        void Validate()
        {
            if (characterStates == null || characterStates.Length == 0)
            {
                characterStates = GetCharacterStates().ToArray();
            }
        }

        #endregion

        private void Reset()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;

            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);


            if (prefabStage == null)
            {
                EditorUtility.DisplayDialog("Setup", "Please create this object as a prefab, and open it in prefab mode before setting up the character in it.", "OK");
                DestroyImmediate(this);
                return;
            }

            string prefabPath = prefabStage.assetPath;
            string fileName = Path.GetFileNameWithoutExtension(prefabPath);
            string defaultInfoPath = Path.GetDirectoryName(prefabPath) + "/" + fileName + ".asset";

            var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(prefabPath);
            if (existing != null)
            {
                characterInfo = existing as CharacterInfo;
            } else
            {
                characterInfo = ScriptableObject.CreateInstance<CharacterInfo>();
                AssetDatabase.CreateAsset(characterInfo, defaultInfoPath);
            }

            var AASettings = AddressableAssetSettingsDefaultObject.Settings;
            if (AASettings)
            {
                var charactersGroup = AASettings.DefaultGroup;
                if (charactersGroup)
                {
                    var guid = AssetDatabase.GUIDFromAssetPath(prefabPath);
                    var entry = AASettings.CreateOrMoveEntry(guid.ToString(), charactersGroup, false, false);
                    entry.SetLabel("Character", true);
                    entry.address = fileName;
                    AASettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);

                    characterInfo.characterRef = new CharacterReference(guid.ToString());
                    EditorUtility.SetDirty(characterInfo);
                    AssetDatabase.SaveAssets();
                } else
                {
                    Debug.LogError("No characters addressables group?");
                }
            }
#endif
        }


#if UNITY_EDITOR
        [Button]
        void UpdateCharacterSlot()
        {
            _characterSlot = GetComponentInChildren<CharacterSpineSlot>();
        }
#endif
    }
}
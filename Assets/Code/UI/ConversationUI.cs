using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;
using System;

using Boomzap.Character;
using Boomzap.Conversation;

namespace ho
{
    public class ConversationUI : BaseUI
    {
        [SerializeField] Transform[] characterSlots;
        [SerializeField] RawImage characterTextureSurface;
        int screenWidth = 0;
        int screenHeight = 0;
        [SerializeField] ConversationDialogBox dialogBox;

        [SerializeField] float delayBeforeNext = 1f;

        [SerializeField] Button skipButton;
        [SerializeField] TextMeshProUGUI clickToContinueText;
        [SerializeField] Image block;
        [SerializeField] Image transitionBlock;

        bool isAnimatingTransition = false;

        CharacterCanvas characterRenderer => CharacterManager.instance.characterCanvas;

        Character currentCharacter;

        ConversationNode speakerNode;

        // Start is called before the first frame update
        void Start()
        {

        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void OnGUI()
        {
            float y = 10f;
            foreach (var f in GameController.save.currentProfile.conversationFlags)
            {
                GUI.Label(new Rect(5f, y, 300f, 20f), f);
                y += 25;
            }
        }

#endif

        public override void Init()
        {
            ConversationManager.instance.OnConversationStarted += OnConversationStarted;
            ConversationManager.instance.OnConversationEnded += OnConversationEnded;
            ConversationManager.instance.OnOptionSelected += SetupLine;

            skipButton.onClick.AddListener(OnSkipButton);
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            //ConversationManager.instance.OnConversationStarted -= OnConversationStarted;
            //ConversationManager.instance.OnConversationEnded -= OnConversationEnded;
            //ConversationManager.instance.OnOptionSelected -= SetupLine;
        }

        void OnConversationStarted()
        {
            block.color = new Color(0f, 0f, 0f, 0f);
            Show();
        }

        void OnConversationEnded()
        {
            HideAllCharacters();

            Hide();
        }

        void OnSkipButton()
        {
            if (ConversationManager.instance.CurrentConversation != null)
            {
                ConversationManager.instance.EndConversation();
            }
        }

        void HideAllCharacters()
        {
            characterRenderer.StartChanges();

            for (int i = 0; i < characterRenderer.SlotCount; i++)
            {
                characterRenderer.UpdateSlot(i, null, "", "", "", null, false, false);
            }

            characterRenderer.ChangesFinished();
        }

        protected override void OnBeginShow(bool instant)
        {
            /*CharacterManager.instance.gameObject.SetActive(false);*/
            base.OnBeginShow(instant);
            RelayoutCharacters();
        }

        protected override void OnFinishShow()
        {
            base.OnFinishShow();
        }
        protected override void OnFinishHide()
        {
            characterRenderer.ResetCharacters();
            characterRenderer.FreeCharacters();

            base.OnFinishHide();
        }

        string ResolveSpeakingCharacterName(ConversationNode fromNode)
        {
            if (!string.IsNullOrEmpty(fromNode.overrideSpeakingCharacter))
                return StrReplace.Parse(fromNode.overrideSpeakingCharacter);
            if (fromNode.speakingCharacter != null)
                return fromNode.speakingCharacter.firstName;
            return string.Empty;
        }

        void RelayoutCharacters()
        {
            RectTransform rt = characterTextureSurface.transform as RectTransform;

            //   rt.sizeDelta = new Vector2(Screen.width / canvas.scaleFactor, 0);

            characterRenderer.SetLayout(characterSlots.Select(x => x.position).ToArray(), transform.position);
            screenWidth = Screen.width;
            screenHeight = Screen.height;
        }
        IEnumerator AnimateTransitionCor()
        {
            characterRenderer.MoveAwayCharacterSlots();
            gameObject.PlayAnimation(this, onHideAnimation.name);
            transitionBlock.CrossFadeAlpha(1, 0.5f, false);
            yield return new WaitForSeconds(.5f);

            HideAllCharacters();
            UpdateBackground();

            yield return new WaitForSeconds(0.1f);
            transitionBlock.CrossFadeAlpha(0f, 0.25f, false);
            yield return new WaitForSeconds(0.25f);
            gameObject.PlayAnimation(this, onShowAnimation.name);
            OnFinishTransition();

            isAnimatingTransition = false;
        }

        void OnFinishTransition()
        {
            var currentNode = ConversationManager.instance.CurrentNode;

            UpdateSpeaking();

            characterRenderer.StartChanges();

            if (currentNode == null)
            {
                Debug.Log("Current Node is null");
                return;
            }

            for (int i = 0; i < currentNode.characters.Length; i++)
            {
                var slot = currentNode.characters[i];
                characterRenderer.UpdateSlot(i, slot.character, slot.state, slot.emotion, slot.eyes, slot.spineSlots, !slot.faceLeft, slot.lookBack);
            }

            characterRenderer.ChangesFinished();

            dialogBox.Setup(currentNode.text, ResolveSpeakingCharacterName(currentNode));
        }

        void SetupLine()
        {
            //Reset BG before convos

            var currentNode = ConversationManager.instance.CurrentNode;

            if (currentNode == null)
            {
                if (ConversationManager.instance.CurrentConversation != null)
                {
                    Debug.Log($"Empty conversation: {ConversationManager.instance.CurrentConversation.name}");
                    ConversationManager.instance.EndConversation();
                }
                return;
            }

            //StopCoroutine(AnimateTransitionCor());

            if (currentNode.transitionStyle == TransitionStyle.CrossFade)
            {
                isAnimatingTransition = true;
                StartCoroutine(AnimateTransitionCor());
            }
            else
            {
                isAnimatingTransition = false;
                transitionBlock.CrossFadeAlpha(0f, 0f, true);
                UpdateBackground();
                OnFinishTransition();
            }
        }

        public void UpdateBackground()
        {
            var currentNode = ConversationManager.instance.CurrentNode;
            
            bool entrySameAsBG = false;

            if (currentNode != null && HOGameController.instance.currentRoomRef != null)
            {
                entrySameAsBG  = currentNode.background.Equals(HOGameController.instance.currentRoomRef.roomName);
            }

            bool isDefaultBG = currentNode.background.Equals("Default") || string.IsNullOrWhiteSpace(currentNode.background) || entrySameAsBG;

            //Debug.Log($"Default bg?  {isDefaultBG} {currentNode.background}");
            if (isDefaultBG == false)
            {
                BGManager.instance.LoadBackground(currentNode.background);
            }
            else
            {
                BGManager.instance.DisableAllBackground();
            }
        }

        void UpdateSpeaking()
        {
            speakerNode = ConversationManager.instance.CurrentNode;
            Boomzap.Character.CharacterInfo currentSpeaker = null;

            if (speakerNode != null)
            {
                currentSpeaker = speakerNode.speakingCharacter;
            }

            //bool isTalkTimerDone = dialogBox.IsTalkTimerDone;
            bool isTalkTimerDone = true;

            foreach (Character c in characterRenderer.Characters)
            {
                if (c == null)
                {
                    continue;
                }
                    
                if (currentSpeaker == null) continue;

                int cidx = Array.IndexOf(characterRenderer.Characters, c);
                

                if (c.characterInfo.name == currentSpeaker.name)
                {
                   if(currentCharacter != null && currentCharacter != c)
                   {
                        characterRenderer.AdjustCharacterToLayerForSlot(cidx, currentCharacter, false);
                        iTween.ScaleTo(currentCharacter.gameObject, Vector3.one * 150f, 0.25f);

                        currentCharacter = c;

                        characterRenderer.AdjustCharacterToLayerForSlot(cidx, c, true);
                        iTween.ScaleTo(c.gameObject, Vector3.one * 155f, 0.25f);

                    }
                    else if(currentCharacter == null)
                    {
                        currentCharacter = c;
                        characterRenderer.AdjustCharacterToLayerForSlot(cidx, c, true);
                        iTween.ScaleTo(c.gameObject, Vector3.one * 160f, 0.25f);
                    }
                }
                else
                    characterRenderer.AdjustCharacterToLayerForSlot(cidx, c, false);

                c.IsTalking = !isTalkTimerDone && currentSpeaker.name == c.characterInfo.name;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(isAnimatingTransition == false)
                RelayoutCharacters();

            //if (screenWidth != Screen.width || screenHeight != Screen.height)
            //{
            //    RelayoutCharacters();
            //}

            characterTextureSurface.texture = characterRenderer.OutputTexture;
            characterTextureSurface.gameObject.SetActive(characterRenderer.OutputTexture != null);

            UpdateSpeaking();

            if(dialogBox.dialogType == ConversationDialogBox.DialogShow.Instant)
            {
                clickToContinueText.gameObject.SetActive(true);
            }
            else
                clickToContinueText.gameObject.SetActive(dialogBox.IsDoneShowing && ((Time.time - dialogBox.showDoneTime) >= delayBeforeNext));

            if (Input.GetMouseButtonDown(0))
            {
                HandleMousePress();
            }
        }

        void SelectOption(int idx)
        {

            // for future compatibility if we add dialog choices

            ConversationManager.instance.SelectOption(idx);
        }

        void HandleMousePress()
        {
            Animation animCtrl = GetComponent<Animation>();

            if (animCtrl && animCtrl.isPlaying) return;

            //Skip Done Check
            if(dialogBox.dialogType != ConversationDialogBox.DialogShow.Instant)
            {
                if (!dialogBox.IsDoneShowing)
                {
                    dialogBox.FinishInstantly();
                    return;
                }

                if ((Time.time - dialogBox.showDoneTime) < delayBeforeNext) return;
            }

            if (ConversationManager.instance.CurrentConversationProgress.validChildNodes.Count > 1) return;

            if (ConversationManager.instance.CurrentConversationProgress.validChildNodes.Count == 0)
            {
                // end of conversation
                ConversationManager.instance.EndConversation();
                return;
            }

            SelectOption(0);   // 'continue' when there are no actual options.
        }
    }
}

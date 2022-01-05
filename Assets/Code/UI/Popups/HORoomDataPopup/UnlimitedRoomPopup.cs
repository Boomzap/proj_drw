using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace ho
{
    public class UnlimitedRoomPopup : Popup
    {
        [BoxGroup("UI Buttons"), SerializeField] Button mainMenuButton;
        [BoxGroup("UI Buttons"), SerializeField] Button playButton;
        [BoxGroup("UI Buttons"), SerializeField] Button backButton;

        [BoxGroup("UI Buttons"), SerializeField] Button highScoreButton;

        [BoxGroup("UI Buttons"), SerializeField] Button nextButton;
        [BoxGroup("UI Buttons"), SerializeField] Button previousButton;

        [BoxGroup("Room Preview"), SerializeField] UnlimitedRoomData mainRoomData;
        [BoxGroup("Room Preview"), SerializeField] UnlimitedRoomData subRoomData;

        [BoxGroup("Animation"), SerializeField] AnimationClip nextAnimation;
        [BoxGroup("Animation"), SerializeField] AnimationClip previousAnimation;

        [SerializeField] CanvasGroup canvasGroup;

        protected override bool UseBlackout => true;

        [SerializeField] ModeSelector modeSelector;

        Chapter.Entry currentEntry = null;

        bool isAnimating = false;

        public void Setup(Chapter.Entry entry)
        {
            currentEntry = entry;
            mainRoomData.SetupEntry(entry);

            ResetMainRoom();
        }

        protected override void OnFinishShow()
        {
            base.OnFinishShow();

        }
        void OnHighScoreButton()
        {
            HighScorePopup popup = Popup.GetPopup<HighScorePopup>();
            popup.SetupEntry(currentEntry);
            onHiddenOneshot += () => popup.Show();
            Hide();
        }

        void OnClose()
        {
            canvasGroup.blocksRaycasts = false;
            onHiddenOneshot += () =>
            {
                UIController.instance.unlimitedUI.Show();
                canvasGroup.blocksRaycasts = true;
            };
            Hide();
        }
        
        void OnPlayButton()
        {
            Chapter.Entry newEntry = new Chapter.Entry();
            newEntry.hoRoom = currentEntry.hoRoom;
            newEntry.hoLogic = modeSelector.selectedLogic.ToString();
            newEntry.objectCount = modeSelector.selectedLogic == HOLogicType.HOLogicPairs ? 9 : 18;

            GameController.instance.isUnlimitedMode = true;
            GameController.instance.LaunchBootEntry(newEntry);

            Hide();
        }

        void OnMainMenu()
        {
            onHiddenOneshot+= () => UIController.instance.mainMenuUI.Show();
            Hide();
        }


        void ResetMainRoom()
        {
            mainRoomData.SetupEntry(currentEntry);
            mainRoomData.transform.localPosition = new Vector3(0, 35f, 0);

            subRoomData.gameObject.SetActive(false);
            subRoomData.transform.localPosition = new Vector3(-2000f, 35f, 0);

            isAnimating = false;
        }

        void OnNextButton()
        {
            if (isAnimating) return;

            Chapter.Entry nextEntry = UIController.instance.unlimitedUI.GetNextRoomUnlocked(currentEntry);

            if(nextEntry != null && currentEntry != nextEntry)
            {
                isAnimating = true;

                currentEntry = nextEntry;
                subRoomData.SetupEntry(currentEntry);

                subRoomData.gameObject.SetActive(true);

                AnimUtil.PlayAnimation(gameObject, this, nextAnimation.name, () => ResetMainRoom());
            }
        }

        void OnPreviousButton()
        {
            if (isAnimating) return;

            Chapter.Entry previousEntry = UIController.instance.unlimitedUI.GetPreviousRoomUnlocked(currentEntry);

            if (previousEntry != null && currentEntry != previousEntry)
            {
                isAnimating = true;

                currentEntry = previousEntry;
                subRoomData.SetupEntry(currentEntry);

                subRoomData.gameObject.SetActive(true);

                AnimUtil.PlayAnimation(gameObject, this, previousAnimation.name, () => ResetMainRoom());
            }
        }

        public override void Init()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenu);

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayButton);

            highScoreButton.onClick.RemoveAllListeners();
            highScoreButton.onClick.AddListener(OnHighScoreButton);

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnClose);

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButton);

            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousButton);
        }

        protected override void Awake()
        {
            Init();
            base.Awake();
        }
    }
}


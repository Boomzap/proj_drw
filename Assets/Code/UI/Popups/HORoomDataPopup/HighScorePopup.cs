using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class HighScorePopup : Popup
    {
        Chapter.Entry currentEntry;

        [BoxGroup("Button"), SerializeField] Button mainMenuButton;
        [BoxGroup("Button"), SerializeField] Button backButton;
        [BoxGroup("Button"), SerializeField] Button unlimitedUIButton;
        [BoxGroup("Button"), SerializeField] Button nextButton;
        [BoxGroup("Button"), SerializeField] Button previousButton;

        [BoxGroup("Animation"), SerializeField] AnimationClip nextAnimation;
        [BoxGroup("Animation"), SerializeField] AnimationClip previousAnimation;

        bool isAnimating = false;

        [BoxGroup("Room Preview"), SerializeField] HighScoreData mainRoomData;
        [BoxGroup("Room Preview"), SerializeField] HighScoreData subRoomData;

        public void SetupEntry(Chapter.Entry entry)
        {
            currentEntry = entry;

            mainRoomData.SetupHighScoreData(entry);

            ResetMainRoom();
        }

        void ResetMainRoom()
        {
            mainRoomData.SetupHighScoreData(currentEntry);
            mainRoomData.transform.localPosition = new Vector3(0, 35f, 0);

            subRoomData.gameObject.SetActive(false);
            subRoomData.transform.localPosition = new Vector3(-2000f, 35f, 0);

            isAnimating = false;
        }

        void OnNextButton()
        {
            if (isAnimating) return;

            Chapter.Entry nextEntry = UIController.instance.unlimitedUI.GetNextRoomUnlocked(currentEntry);

            if (nextEntry != null && currentEntry != nextEntry)
            {
                isAnimating = true;

                currentEntry = nextEntry;
                subRoomData.SetupHighScoreData(currentEntry);

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
                subRoomData.SetupHighScoreData(currentEntry);

                subRoomData.gameObject.SetActive(true);

                AnimUtil.PlayAnimation(gameObject, this, previousAnimation.name, () => ResetMainRoom());
            }
        }

        public void OnMainMenuButton()
        {
            onHiddenOneshot += () => UIController.instance.mainMenuUI.Show();
            Hide();
        }

        public void OnUnlimitedMenuButton()
        {
            onHiddenOneshot += () => UIController.instance.unlimitedUI.Show();
            Hide();
        }

        public void OnUnlimitedPopupButton()
        {
            UnlimitedRoomPopup popup = GetPopup<UnlimitedRoomPopup>();
            popup.Setup(currentEntry);

            onHiddenOneshot += () => popup.Show();
            Hide();
        }

        public override void Init()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => OnMainMenuButton());

            unlimitedUIButton.onClick.RemoveAllListeners();
            unlimitedUIButton.onClick.AddListener(() => OnUnlimitedMenuButton());

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => OnUnlimitedPopupButton());

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => OnNextButton());

            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(() => OnPreviousButton());

        }

        protected override void Awake()
        {
            base.Awake();
            Init();
        }
    }
}

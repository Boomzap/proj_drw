using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ho
{
    public class SouvenirUI : BaseUI
    {
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button photoJournalButton;
        [SerializeField] Button achievementsButton;

        void OnAchievements()
        {
            UIController.instance.achievementUI.Show();
            Hide();
        }

        void OnPhotoJournal()
        {
            UIController.instance.journalUI.Show();
            Hide();
        }

        void OnMainMenu()
        {
            UIController.instance.mainMenuUI.Show();
            Hide();
        }

        public override void Init()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => OnMainMenu());

            photoJournalButton.onClick.RemoveAllListeners();
            photoJournalButton.onClick.AddListener(() => OnPhotoJournal());

            achievementsButton.onClick.RemoveAllListeners();
            achievementsButton.onClick.AddListener(() => OnAchievements());
        }

        private void Awake()
        {
            Init();
        }
    }
}

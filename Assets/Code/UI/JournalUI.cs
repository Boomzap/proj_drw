using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class JournalUI : BaseUI
    {
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button unliPlayButton;
        [SerializeField] Button achievementButton;

        [SerializeField] Button nextButton;
        [SerializeField] Button previousButton;

        [ShowInInspector, ReadOnly]
        List<Chapter.Entry> mgReferences = new List<Chapter.Entry>();

        [SerializeField] JournalEntry[] journalEntries;

        int currPageIdx = 0;

        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => OnMainMenu());

            unliPlayButton.onClick.RemoveAllListeners();
            unliPlayButton.onClick.AddListener(() => OnShowUnliPlay());

            achievementButton.onClick.RemoveAllListeners();
            achievementButton.onClick.AddListener(() => OnShowAchievements());

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => OnNextButton());

            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(() => OnPreviousButton());
        }

        void OnMainMenu()
        {
            onHiddenOneshot += () => UIController.instance.mainMenuUI.Show();
            Hide();
        }

        void OnShowUnliPlay()
        {
            //Show Unli Play UI when ready
            onHiddenOneshot += () => UIController.instance.unlimitedUI.Show();
            Hide();
        }

        void OnShowAchievements()
        {
            //Show Achievements when ready
            onHiddenOneshot += () => UIController.instance.achievementUI.Show();
            Hide();
        }

        void OnNextButton()
        {
            currPageIdx++;
           
            if(currPageIdx >= mgReferences.Count / journalEntries.Length)
            {
                currPageIdx = 0;
            }

            SetJournalPageIndex(currPageIdx);
        }

        void OnPreviousButton()
        {
            currPageIdx--;

            if(currPageIdx < 0)
            {
                currPageIdx = (mgReferences.Count / journalEntries.Length) - 1;
            }

            SetJournalPageIndex(currPageIdx);
        }

        void SetJournalPageIndex(int idx)
        {
            int refPointer = idx * journalEntries.Length;
            for (int i = 0; i < journalEntries.Length; i++)
            {
                bool isValidIndex = refPointer + i < mgReferences.Count;
                if (isValidIndex)
                    journalEntries[i].SetupEntry(mgReferences[refPointer + i]);


                journalEntries[i].gameObject.SetActive(isValidIndex);
            }
        }

        void BuildMGPreview()
        {
            mgReferences.Clear();
            foreach (var chapter in GameController.instance.gameChapters)
            {
                foreach (var sceneEntry in chapter.sceneEntries)
                {
                    //Detail or Odd Mode
                    if(sceneEntry.IsHOScene && sceneEntry.hoRoom.roomName.ToLower().StartsWith("det"))
                    {
                        mgReferences.Add(sceneEntry);
                        continue;
                    }

                    if (sceneEntry.IsMinigame)
                    {
                        if (sceneEntry.minigame == null) continue;

                        mgReferences.Add(sceneEntry);
                    }
                }
            }

            currPageIdx = 0;
            SetJournalPageIndex(currPageIdx);
        }

        protected override void OnBeginShow(bool instant)
        {
            unliPlayButton.interactable = GameController.instance.isFirstHOSceneUnlocked;
            BuildMGPreview();
            base.OnBeginShow(instant);
        }
    }
}

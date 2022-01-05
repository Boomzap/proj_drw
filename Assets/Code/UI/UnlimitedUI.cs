using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using System.Linq;

namespace ho
{
    public class UnlimitedUI : BaseUI
    {
        [BoxGroup("UI Buttons"), SerializeField] Button mainMenuButton;

        //FT1
        //[BoxGroup("UI Buttons"), SerializeField] Button photoJournalButton;
        //[BoxGroup("UI Buttons"), SerializeField] Button achievementsButton;
        //[BoxGroup("UI Buttons"), SerializeField] Button closeButton;

        [BoxGroup("UI Buttons"), SerializeField] Button nextButton;
        [BoxGroup("UI Buttons"), SerializeField] Button previousButton;

        // FT1
        //[BoxGroup("Player Info"), SerializeField] TextMeshProUGUI levelsUnlockedText;
        //[BoxGroup("Player Info"), SerializeField] TextMeshProUGUI totalScoreText;
        //[BoxGroup("Player Info"), SerializeField] TextMeshProUGUI triviasFoundText;

        [ShowInInspector, ReadOnly]
        List<Chapter.Entry> roomReferences = new List<Chapter.Entry>();

        [SerializeField] RoomPreviewHolder[] previewHolders;
        int maxIndex => roomReferences.Count / previewHolders.Length;

        int currentPageIndex = 0;
        protected override void OnFinishShow()
        {
            base.OnFinishShow();

            Tutorial.TriggerTutorial(Tutorial.Trigger.UnlimitedFirstLevel);

            UIController.instance.isUIInputDisabled = false;
        }

        void OnNextButton()
        {
            currentPageIndex++;

            if (currentPageIndex <= maxIndex)
                SetupRoomEntries(currentPageIndex);
            else
            {
                currentPageIndex = 0;
                SetupRoomEntries(0);
            }
        }

        void OnPreviousButton()
        {
            currentPageIndex--;

            if (currentPageIndex >= 0)
            {
                SetupRoomEntries(currentPageIndex);
            }
            else
            {
                currentPageIndex = maxIndex;
                SetupRoomEntries(maxIndex);
            }
        }

        void SetupRoomEntries(int pageIndex)
        {
            int holderCount = previewHolders.Length;
            int roomIndex = holderCount * pageIndex;

            //Debug.Log($"Holder Count: {holderCount}");

            //Start at this index
            for (int i = 0; i < holderCount; i++)
            {
                bool isRoomValid = roomIndex + i < roomReferences.Count;
                //Debug.Log($"Room Valid? {isRoomValid}");
                if (isRoomValid)
                {
                    Chapter.Entry entry = roomReferences[roomIndex + i];
                    string displayName = HOUtil.GetRoomLocalizedName(entry.hoRoom.roomName);
                    //Debug.Log($"Check DisplayName: {displayName}");

                    Savegame.HORoomData roomData = HORoomDataHelper.instance.GetHORoomData(entry.hoRoom.AssetGUID);

                    previewHolders[i].Setup(entry, entry.hoRoom.roomPreviewSprite, displayName, roomData.triviasFound.Count, roomData.modesData.Count);
                    //Debug.Log($"{i} {entry.hoRoom.roomName}");
                }

                previewHolders[i].gameObject.SetActive(isRoomValid);
            }
        }

        void BuildRoomPreviews()
        {
            roomReferences.Clear();

            //Debug.Log("Build Room Previews");

            roomReferences = HORoomAssetManager.instance.GetRoomReferences();

            //Order By Unlock
            //roomReferences = roomReferences.OrderBy(x => x.isEntryUnlocked? 0 : 1).ThenBy(x => LocalizationUtil.FindLocalizationEntry(x.hoRoom.roomLocalizationKey)).ToList();

            SetupRoomEntries(0);

            //FT1
            //List<Savegame.HORoomData> roomsUnlocked = HORoomDataHelper.instance.GetHORoomsUnlockedData(roomReferences);

            //int triviasFound = roomsUnlocked.Select(x => x.triviasFound).Sum(y => y.Count);

            //int totalScore = 0;
            //foreach(var roomData in roomsUnlocked)
            //{
            //    totalScore += roomData.modesData.Select(x => x.highScore).Sum();
            //}

            //levelsUnlockedText.text = $"{roomsUnlocked.Count}/{roomReferences.Count}";
            //triviasFoundText.text = $"{triviasFound}/{roomReferences.Count * 5}";
            //totalScoreText.text = $"{totalScore:N0}";
        }

        protected override void OnBeginShow(bool instant)
        {
            BuildRoomPreviews();
            base.OnBeginShow(instant);
        }

        #region Initialize Buttons
        void OnMainMenu()
        {
            onHiddenOneshot+= () => UIController.instance.mainMenuUI.Show();
            Hide();
        }

        void OnPhotoJournal()
        {
            onHiddenOneshot += () => UIController.instance.journalUI.Show();
            Hide();
        }

        void OnAchievements()
        {
            onHiddenOneshot += () => UIController.instance.achievementUI.Show();
            Hide();
        }


        public override void Init()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenu);

            //FT1
            //photoJournalButton.onClick.RemoveAllListeners();
            //photoJournalButton.onClick.AddListener(OnPhotoJournal);

            //achievementsButton.onClick.RemoveAllListeners();
            //achievementsButton.onClick.AddListener(OnAchievements);

            //closeButton.onClick.RemoveAllListeners();
            //closeButton.onClick.AddListener(OnMainMenu);

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButton);

            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousButton);
        }

        private void Awake()
        {
            Init();
        }

        public Chapter.Entry GetNextRoomUnlocked(Chapter.Entry currrentEntry)
        {
            var roomsUnlocked = roomReferences.Where(x => x.isEntryUnlocked).ToList();

            int index = roomsUnlocked.IndexOf(currrentEntry);

            //Entry is not found on unlocked rooms
            if (index == -1)
            {
                return currrentEntry;
            }

            
            if (index + 1 < roomsUnlocked.Count)
            {
                //If room index is withing rooms unlock count return next room
                return roomsUnlocked[index + 1];
            }
            else
            {
                //If room index is withing rooms unlock count return first unlocked room
                return roomsUnlocked[0];
            }
        }

        public Chapter.Entry GetPreviousRoomUnlocked(Chapter.Entry currrentEntry)
        {
            var roomsUnlocked = roomReferences.Where(x => x.isEntryUnlocked).ToList();

            int index = roomsUnlocked.IndexOf(currrentEntry);

            //Entry is not found on unlocked rooms
            if (index == -1)
            {
                return currrentEntry;
            }

            if (index - 1 >= 0)
            {
                return roomsUnlocked[index - 1];
            }
            else
            {
                return roomsUnlocked[roomsUnlocked.Count - 1];
            }
        }
        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class JournalEntry : MonoBehaviour
    {
        [SerializeField] Image previewImage;
        [SerializeField] TextMeshProUGUI minigameTitleText;
        [SerializeField] TextMeshProUGUI descriptionText;

        [SerializeField] Button playButton;

        Chapter.Entry currentEntry;
        public void SetupEntry(Chapter.Entry entry)
        {
            currentEntry = entry;
            bool isEntryUnlocked = GameController.save.IsChapterEntryUnlocked(entry);

            bool isDetailMode = entry.IsHOScene && entry.hoRoom.roomName.ToLower().StartsWith("det");

            previewImage.material = null;

            playButton.interactable = isEntryUnlocked;

            if (isEntryUnlocked == false)
            {
                if(isDetailMode)
                {
                    previewImage.sprite = HORoomAssetManager.instance.roomTracker.GetPreviewSprite(entry.hoRoom);
                }
                else
                {
                    previewImage.sprite = HORoomAssetManager.instance.mgTracker.GetPreviewSprite(entry.minigame);
                }
                previewImage.material = UIController.instance.grayscale;
                minigameTitleText.text = "???";
                descriptionText.text = "???";
                return;
            }

            //Detail or Odd Mode
            if (isDetailMode)
            {
                previewImage.sprite = HORoomAssetManager.instance.roomTracker.GetPreviewSprite(entry.hoRoom);
                minigameTitleText.text = LocalizationUtil.FindLocalizationEntry($"Minigame/{entry.hoRoom.roomName.ToLower()}_title", string.Empty, false, TableCategory.UI);
                descriptionText.text = LocalizationUtil.FindLocalizationEntry($"Minigame/{entry.hoRoom.roomName.ToLower()}_desc", string.Empty, false, TableCategory.UI);
            }
            else
            {
                previewImage.sprite = HORoomAssetManager.instance.mgTracker.GetPreviewSprite(entry.minigame);
                string roomName = HORoomAssetManager.instance.mgTracker.GetMGRoomName(entry.minigame);
                minigameTitleText.text = LocalizationUtil.FindLocalizationEntry($"Minigame/{roomName.ToLower()}_title", string.Empty, false, TableCategory.UI);
                descriptionText.text = LocalizationUtil.FindLocalizationEntry($"Minigame/{roomName.ToLower()}_desc", string.Empty, false, TableCategory.UI);
            }
        }

        void OnPlayButton()
        {
            Chapter.Entry bootEntry = new Chapter.Entry();
            bootEntry.hoRoom = currentEntry.hoRoom;
            bootEntry.minigame = currentEntry.minigame;
            bootEntry.hoLogic = currentEntry.hoLogic;
            bootEntry.objectCount = 18;
            bootEntry.music = currentEntry.music;
            bootEntry.ambient = currentEntry.ambient;

            Debug.Log("FadeToMinigame");

            if (currentEntry.IsHOScene)
            {
                HOGameController.instance.returnToJournalUI = true;
                GameController.instance.FadeToHOGame(bootEntry);
            }
            else
            {
                MinigameController.instance.returnToJournalUI = true;
                GameController.instance.FadeToMinigame(currentEntry);
            }
                
        }

        private void OnEnable()
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => OnPlayButton());
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;

namespace ho
{
    public class SceneEntry : MonoBehaviour
    {
        [SerializeField] Button entryButton = null;
        [SerializeField] Image previewImage = null;

        [BoxGroup("Animation")] public AnimationClip onPointerEnter;
        [BoxGroup("Animation")] public AnimationClip onPointerExit;
        //bool pointerEntered = false;

        [SerializeField] TextMeshProUGUI entryDisplayName;
        [SerializeField] TextMeshProUGUI entryDescription;

        Chapter.Entry entry = null;

        public Chapter.Entry EntryScene
        {
            get { return entry; }
        }

        private void Awake()
        {
            entryButton.onClick.RemoveAllListeners();
            entryButton.onClick.AddListener(OnLoadScene);
        }

        public void Setup(Chapter.Entry selectedEntry, string description, bool unlocked = false)
        {
            entry = selectedEntry;
            previewImage.sprite = selectedEntry.IsHOScene ? selectedEntry.hoRoom.roomPreviewSprite : selectedEntry.minigame.roomPreviewSprite;

            if(entry != null)
            {
                if(entry.IsHOScene)
                {
                    Savegame.HOSceneState state = GameController.save.GetSceneState(entry);

                    previewImage.material = unlocked ? null : UIController.instance.grayscale;

                    entryDisplayName.text = unlocked? LocalizationUtil.FindLocalizationEntry(entry.hoRoom.roomLocalizationKey, string.Empty, false, TableCategory.Game) : "???" ;
                  
                    //entryButton.interactable = unlocked;
                }

                if(entry.IsMinigame)
                {
                    previewImage.material = unlocked ? null : UIController.instance.grayscale;

                    entryDisplayName.text = unlocked ? LocalizationUtil.FindLocalizationEntry(entry.minigame.roomNameKey, string.Empty, false, TableCategory.Game) : "???";

                    //entryButton.interactable = unlocked;
                }

                entryDescription.text = unlocked? description: "???";
            }
        }

        void OnLoadScene()
        {
            if (entry == null) return;

            UIController.instance.chapterUI.selectedEntry = entry;
        }
    }
}

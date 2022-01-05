using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class RoomPreviewHolder : MonoBehaviour
    {
        [SerializeField] Button holder;

        [SerializeField] Image roomPreview;
        [SerializeField] TextMeshProUGUI roomDisplayText;

        //FT1
        //[SerializeField] TextMeshProUGUI funFactsFoundText;
        //[SerializeField] TextMeshProUGUI modesPlayedText;

        Chapter.Entry entry;

        string funFactsFoundLabel => LocalizationUtil.FindLocalizationEntry("FunFactsFound", string.Empty, false, TableCategory.UI);
        string modesPlayedLabel => LocalizationUtil.FindLocalizationEntry("ModesPlayed", string.Empty, false, TableCategory.UI);

        public void Setup(Chapter.Entry roomEntry, Sprite roomPreviewSprite, string roomDisplayKey, int foundFacts, int modesPlayed)
        {
            entry = roomEntry;
            roomPreview.sprite = roomPreviewSprite;

            //FT1
            //funFactsFoundText.text = $"{funFactsFoundLabel} {foundFacts}/5";
            //modesPlayedText.text = $"{modesPlayedLabel} {modesPlayed}/8 ";

            roomDisplayText.text = roomEntry.isEntryUnlocked? LocalizationUtil.FindLocalizationEntry(roomDisplayKey) : "???";
            holder.interactable = roomEntry.isEntryUnlocked;
            roomPreview.material = roomEntry.isEntryUnlocked? null : UIController.instance.grayscale;
        }

        void OnOpenUnlimitedPopup()
        {
            UIController.instance.unlimitedUI.onHiddenOneshot += () =>
            {
                UnlimitedRoomPopup popup = Popup.GetPopup<UnlimitedRoomPopup>();
                popup.Setup(entry);
                popup.Show();
                UIController.instance.unlimitedUI.DisableInputForHideAnimation();
            };
            UIController.instance.unlimitedUI.Hide();
        }

        private void Awake()
        {
            holder.onClick.RemoveAllListeners();
            holder.onClick.AddListener(OnOpenUnlimitedPopup);
        }
    }

}

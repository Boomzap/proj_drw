using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace ho
{
    public class UnlimitedRoomData : MonoBehaviour
    {
        [BoxGroup("Display"), SerializeField] Image mainPreviewImage;

        [BoxGroup("Panels"), SerializeField] GameObject mainPanelObject;

        [BoxGroup("Room Preview"), SerializeField] TextMeshProUGUI displayRoomText;

        public void SetupEntry(Chapter.Entry entry)
        {
            displayRoomText.text = LocalizationUtil.FindLocalizationEntry(entry.hoRoom.roomLocalizationKey);
            mainPreviewImage.sprite = entry.hoRoom.roomPreviewSprite;

            Popup.GetPopup<HighScorePopup>().SetupEntry(entry);
        }
    }
}

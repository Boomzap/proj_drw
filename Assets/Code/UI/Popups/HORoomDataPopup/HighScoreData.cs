using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class HighScoreData : MonoBehaviour
    {
        [BoxGroup("Display"), SerializeField] Image mainPreviewImage;
        [BoxGroup("Display"), SerializeField] TextMeshProUGUI roomDisplayText;

        [SerializeField] HighScoreEntry[] highScoreEntries;

        public void SetupHighScoreData(Chapter.Entry entry)
        {
            roomDisplayText.text = LocalizationUtil.FindLocalizationEntry(entry.hoRoom.roomLocalizationKey);
            mainPreviewImage.sprite = entry.hoRoom.roomPreviewSprite;

            Savegame.HORoomData roomData = HORoomDataHelper.instance.GetHORoomData(entry.hoRoom.AssetGUID);
            SetupHighScoreEntries(roomData);
        }


        public void SetupHighScoreEntries(Savegame.HORoomData roomData)
        {
            foreach (var entry in highScoreEntries)
            {
                if (roomData.modesData.Any(x => entry.logicType.ToString() == x.modeName))
                {
                    Savegame.ModeData modeData = roomData.modesData.First(x => x.modeName == entry.logicType.ToString());
                    entry.SetupEntry(modeData.fastestGameClear, modeData.highScore, modeData.playedGames);
                }
                else
                {
                    // No Mode data record
                    entry.SetupEntry(0, 0, 0);
                }
            }
        }
    }
}

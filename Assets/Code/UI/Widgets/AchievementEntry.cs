using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steam;
using TMPro;

namespace ho
{
    public class AchievementEntry : MonoBehaviour
    {
        [SerializeField] Image bgImage;
        [SerializeField] Image faceImage;
        [SerializeField] Image headerImage;
        [SerializeField] Image propsImage;

        [SerializeField] TextMeshProUGUI headerText;
        [SerializeField] TextMeshProUGUI descriptionText;


        public void SetupSkinData(AchievementSkin.SkinData skinData, bool isLocked = false)
        {
            if(isLocked == false)
            {
                bgImage.sprite = skinData.bg;
                faceImage.sprite = skinData.face;
                headerImage.sprite = skinData.header;
                propsImage.sprite = skinData.props;

                headerText.text = LocalizationUtil.FindLocalizationEntry(skinData.achievementKey, string.Empty, false, TableCategory.UI);
            }

            descriptionText.text = LocalizationUtil.FindLocalizationEntry(skinData.achievementDescKey, string.Empty, false, TableCategory.UI);
        }
    }
}


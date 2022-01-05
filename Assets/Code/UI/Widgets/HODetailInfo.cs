using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class HODetailInfo : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI instructionsText;
        [SerializeField] TextMeshProUGUI progressLabel;
        [SerializeField] TextMeshProUGUI progressText;

        private void Awake()
        {
            Setup();
        }
        public void Setup()
        {
            instructionsText.text = LocalizationUtil.FindLocalizationEntry("UI/Minigame/Instruction/Detail", string.Empty, false, TableCategory.UI);
            progressLabel.text = LocalizationUtil.FindLocalizationEntry("UI/ItemsFound", string.Empty, false, TableCategory.UI);
        }
    }
}

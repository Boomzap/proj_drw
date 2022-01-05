using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ho
{


    public class HighScoreEntry : MonoBehaviour
    {
        public HOLogicType logicType = HOLogicType.HOLogicStandard;

        [SerializeField] TextMeshProUGUI fastestClearText;
        [SerializeField] TextMeshProUGUI highScoreText;
        //[SerializeField] TextMeshProUGUI timesPlayedText;

        public void SetupEntry(int fastestClear, int highScore, int timesPlayed)
        {
            int mins = fastestClear / 60;
            int secs = fastestClear % 60;

            fastestClearText.text = fastestClear > 0? $"{mins}:{secs:D2}" : "-";
            //highScoreText.text = highScore > 0? $"{highScore:N0}" : "-";

            highScoreText.text = highScore > 0 ? HOUtil.GetCultureNumberSeparator(highScore) : "-";
            //timesPlayedText.text = timesPlayed > 0? timesPlayed.ToString() : "-";
        }
    }
}

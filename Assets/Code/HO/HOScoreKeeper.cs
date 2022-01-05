using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
namespace ho
{
    public class HOScoreKeeper : MonoBehaviour
    {
        HOScoreSettings scoreSettings;

        [BoxGroup("Scores"), ReadOnly] int rawScore = 0;

        [BoxGroup("Score Components"), SerializeField] TextMeshProUGUI scoreText;
        [BoxGroup("Score Components"), SerializeField] HOComboMeter comboMeter;

        [BoxGroup("Score Components")] public GameObject scoreFloaterPrefab;

        Canvas scoreCanvas;

        int totalScore = 0;
        int hintScore = 0;
        int timeScore = 0;

        private void Awake()
        {
            scoreSettings = HOGameController.instance.scoreSettings;
            scoreCanvas = UIController.instance.scoreCanvas;
        }

        public int GetTotalScore()
        {
            //hintScore = GetHintScore();
            hintScore = 0;
            timeScore = GetTimeScore();
            totalScore = rawScore  + timeScore;
            return totalScore;
        }

        public int GetRawScore()
        {
            return rawScore;
        }

        public int GetHintScore()
        {
            hintScore = scoreSettings.hintMaxBonus - (HOGameController.instance.hintUse * scoreSettings.hintScorePenaltyPerUse);
            if (hintScore < 0)
                hintScore = 0;

            return hintScore;
        }

        public int GetTimeScore()
        {
            int time = UIController.instance.hoMainUI.timeKeeper.GetCurrentTimePlayed();

            if (time < scoreSettings.maxBonusTime)
                return scoreSettings.maxSpeedBonus;

            if (time > scoreSettings.timeBonusEnd)
                return 0;

            timeScore = scoreSettings.maxSpeedBonus / (scoreSettings.timeBonusEnd - scoreSettings.maxBonusTime);

            return timeScore;
        }

        public void ResetScore()
        {
            rawScore = 0;
            totalScore = 0;
            hintScore = 0;
            scoreText.text = rawScore.ToString("N0");
            comboMeter.ResetComboFill();
        }

        public void OnItemPicked(Vector3 clickPosition)
        {
            comboMeter.AddCombo();

            if (scoreCanvas.gameObject.activeInHierarchy == false)
                scoreCanvas.gameObject.SetActive(true);

            int scoreToAdd = Mathf.RoundToInt( scoreSettings.scorePerItem * comboMeter.GetScoreMultiplier());

            HOScoreFloater scoreFloater = Instantiate(scoreFloaterPrefab, scoreCanvas.transform, false).GetComponent<HOScoreFloater>();
            scoreFloater.transform.position = clickPosition;
            scoreFloater.gameObject.SetActive(true);
            scoreFloater.AnimateScore(scoreToAdd);

            rawScore += scoreToAdd;
            scoreText.text = rawScore.ToString("N0");

            totalScore += rawScore;
        }

    }
}

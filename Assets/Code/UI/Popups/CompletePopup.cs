using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace ho
{
    public class CompletePopup : Popup
    {
        [SerializeField] TextMeshProUGUI timePlayedText;
        [SerializeField] TextMeshProUGUI rawScoreText;
        [SerializeField] TextMeshProUGUI speedBonusText;
        [SerializeField] TextMeshProUGUI hintsUsedText;
        [SerializeField] TextMeshProUGUI finalScoreText;

        [SerializeField] Button replayButton = null;
        [SerializeField] Button continueButton = null;
        [SerializeField] Button menuButton = null;

        public override void Init()
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() => ReplayRoom());

            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => OnContinue());

            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => OnUnlimitedMenu());
        }

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnFinishShow()
        {
            base.OnFinishShow();
        }

        public void SetupBeforeShow(int timePlayed, int rawScore, int speedBonus, int hintsUsed, int finalScore, HORoomReference roomRef = null, UnityAction andThen = null)
        {
            int timeInMinutes = timePlayed / 60;
            int timeInSeconds = timePlayed % 60;

            timePlayedText.text = $"{timeInMinutes}:{timeInSeconds:D2}";
            rawScoreText.text = rawScore.ToString("N0");
            speedBonusText.text = speedBonus.ToString("N0");
            hintsUsedText.text = hintsUsed.ToString();
            finalScoreText.text = finalScore.ToString("N0");

            bool isUnlimitedMode = GameController.instance.isUnlimitedMode;

            if (isUnlimitedMode)
            {
                continueButton.transform.SetAsFirstSibling();
            }
            else
                menuButton.transform.SetAsFirstSibling();

            if(andThen != null)
            {
                onHiddenOneshot += () => andThen?.Invoke() ;
            }
        }

        void ReplayRoom()
        {
            //This is to clear conversation or achievements added in onHiddenOneshot
            onHiddenOneshot = null;
            onHiddenOneshot += () => HOGameController.instance.ReplayRoom();
            Hide();
        }

        void OnContinue()
        {
            Hide();
        }

        void OnUnlimitedMenu()
        {
            Hide();
        }
    }
}


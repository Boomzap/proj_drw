using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class TutorialSettingPopup : Popup
    {
        [SerializeField] Button neverPlayedBeforeButton;
        [SerializeField] Button playedBeforeButton;
        [SerializeField] Button continueButton;

        [SerializeField] Transform selectionMark;

        bool playTutorial = true;

        void SetSelected(Button selectedButton)
        {
            selectionMark.SetParent(selectedButton.transform, false);

            playTutorial = neverPlayedBeforeButton == selectedButton;
        }

        public void Setup()
        {
            //if (isFullTutorial)
            //    SetSelected(neverPlayedBeforeButton);
            //else
            //    SetSelected(playedBeforeButton);
        }

        protected override void OnBeginShow(bool instant)
        {
            Setup();
            base.OnBeginShow(instant);
        }

        void OnContinue()
        {
            if(playTutorial == false)
            {
                Tutorial.instance.SkipTutorials();
            }
            Hide();
        }

        protected override void Awake()
        {
            base.Awake();

            neverPlayedBeforeButton.onClick.RemoveAllListeners();
            neverPlayedBeforeButton.onClick.AddListener(() => SetSelected(neverPlayedBeforeButton));

            playedBeforeButton.onClick.RemoveAllListeners();
            playedBeforeButton.onClick.AddListener(() => SetSelected(playedBeforeButton));

            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => OnContinue());
        }
    }
}

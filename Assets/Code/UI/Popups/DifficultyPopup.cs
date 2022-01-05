using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ho
{
    public class DifficultyPopup : Popup
    {
        [SerializeField] Button easyButton;
        [SerializeField] Button mediumButton;
        [SerializeField] Button hardButton;

        [SerializeField] Button closeButton;

        //[SerializeField] Image marker;

        [SerializeField] Color selectColor;

        Button selectedButton;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        public override void Init()
        {
            easyButton.onClick.RemoveAllListeners();
            easyButton.onClick.AddListener(() => SwitchDifficulty(HODifficulty.Easy));

            mediumButton.onClick.RemoveAllListeners();
            mediumButton.onClick.AddListener(() => SwitchDifficulty(HODifficulty.Medium));

            hardButton.onClick.RemoveAllListeners();
            hardButton.onClick.AddListener(() => SwitchDifficulty(HODifficulty.Hard));

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());
        }

        protected override void OnBeginShow(bool instant)
        {
            if(GameController.save != null)
            {
                SwitchDifficulty((HODifficulty)GameController.save.currentProfile.hoDifficultyIndex);
            }
            else
            {
                SwitchDifficulty(HODifficulty.Easy);
            }

            base.OnBeginShow(instant);
        }


        void SwitchDifficulty(HODifficulty difficulty)
        {
            HOGameController.instance.currentDifficulty = difficulty;

            if(selectedButton != null)
            {
                selectedButton.targetGraphic.color = Color.white;
            }

            switch(difficulty)
            {
                case HODifficulty.Easy:
                    selectedButton = easyButton;
                    break;
                case HODifficulty.Medium:
                    selectedButton = mediumButton;
                    break;
                case HODifficulty.Hard:
                    selectedButton = hardButton;
                    break;
            }

            selectedButton.targetGraphic.color = selectColor;
        }

        protected override void OnFinishHide()
        {
            GameController.save.currentProfile.hoDifficultyIndex =  (int) HOGameController.instance.currentDifficulty;
            Savegame.SetDirty();

            base.OnFinishHide();
        }

    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class ModeSelector : MonoBehaviour
    {
        [BoxGroup("Mode Buttons"), SerializeField] Button normalButton;
        [BoxGroup("Mode Buttons"), SerializeField] Button findXButton;
        [BoxGroup("Mode Buttons"), SerializeField] Button riddleButton;
        [BoxGroup("Mode Buttons"), SerializeField] Button scrambleButton;
        [BoxGroup("Mode Buttons"), SerializeField] Button silhouetteButton;
        [BoxGroup("Mode Buttons"), SerializeField] Button imageButton;

        //[SerializeField] Transform selectionMark;
        [SerializeField] Color selectColor;

        public HOLogicType selectedLogic;

       

        Button selectedButton;

        private void OnSelectMode(Button button, HOLogicType logicType)
        {
            if (button == selectedButton) return;

            //selectionMark.transform.SetParent(button.transform, false);
            //selectionMark.transform.SetAsFirstSibling();

            selectedButton.targetGraphic.color = Color.white;
            button.targetGraphic.color = selectColor;
            selectedButton = button;

            selectedLogic = logicType;
        }


        void InitializeButtons()
        {
            selectedButton = normalButton;
            selectedButton.targetGraphic.color = selectColor;

            normalButton.onClick.RemoveAllListeners();
            normalButton.onClick.AddListener(() => OnSelectMode(normalButton, HOLogicType.HOLogicStandard));

            //pairsButton.onClick.RemoveAllListeners();
            //pairsButton.onClick.AddListener(() => OnSelectMode(pairsButton, HOLogicType.HOLogicPairs));

            riddleButton.onClick.RemoveAllListeners();
            riddleButton.onClick.AddListener(() => OnSelectMode(riddleButton, HOLogicType.HOLogicRiddle));

            findXButton.onClick.RemoveAllListeners();
            findXButton.onClick.AddListener(() => OnSelectMode(findXButton, HOLogicType.HOLogicFindX));

            silhouetteButton.onClick.RemoveAllListeners();
            silhouetteButton.onClick.AddListener(() => OnSelectMode(silhouetteButton, HOLogicType.HOLogicSilhouette));

            imageButton.onClick.RemoveAllListeners();
            imageButton.onClick.AddListener(() => OnSelectMode(imageButton, HOLogicType.HOLogicPicture));

            //noVowelsButton.onClick.RemoveAllListeners();
            //noVowelsButton.onClick.AddListener(() => OnSelectMode(noVowelsButton, HOLogicType.HOLogicNoVowel));

            scrambleButton.onClick.RemoveAllListeners();
            scrambleButton.onClick.AddListener(() => OnSelectMode(scrambleButton, HOLogicType.HOLogicScramble));
        }

        private void Awake()
        {
            InitializeButtons();
        }
    }
}

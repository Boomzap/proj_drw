using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace ho
{
    public class ChapterPreview : MonoBehaviour
    {
        [SerializeField]    Image completeImage;
        [SerializeField]    Image availableImage;
        [SerializeField]    Image unavailableImage;
        [SerializeField]    Image unavailableCEImage;
        [SerializeField]    Image hoverImage;
        [SerializeField]    Image selectedImage;
        [SerializeField]    Image previewImage;

        [SerializeField] TextMeshProUGUI timeofDayText;

        bool    _isSelected = false;
        bool    _isAvailable = false;
        Chapter _chapter = null;

        [SerializeField]    ButtonAnimator  animator;
        [SerializeField]    Button                  button;

        public Chapter chapter 
        {
            get { return _chapter; }
        }

        public bool isSelected
        { 
            get { return _isSelected; }
            set { 
                _isSelected = value; 
                animator.isSelected = value; 
            }
        }

        public bool isAvailable
        {
            get { return _isAvailable; }
        }

        private void Reset()
        {

        }

        private void Start()
        {

        }

        public void SetChapter(Chapter chapter)
        {
            _chapter = chapter;

            bool chapterAvailable = GameController.save.IsChapterAvailable(chapter);
            bool chapterComplete = GameController.save.IsChapterComplete(chapter);

            //completeImage.gameObject.SetActive(chapterComplete);

            //#if SURVEY_BUILD
            //if ((chapter.isCEContent && !GameController.save.canPlayCEContent) || !chapter.isSurveyContent)

            //#else
            //if (chapter.isCEContent && !GameController.save.canPlayCEContent)
            //#endif
            //{
            //    availableImage.gameObject.SetActive(false);
            //    unavailableImage.gameObject.SetActive(false);
            //    unavailableCEImage.gameObject.SetActive(true);
            //    button.interactable = false;
            //    _isAvailable = false;
            //    return;
            //}

            //button.interactable = chapterAvailable;

            //availableImage.gameObject.SetActive(chapterAvailable);
            //unavailableImage.gameObject.SetActive(!chapterAvailable);
            //unavailableCEImage.gameObject.SetActive(false);

            _isAvailable = chapterAvailable;
            animator.isAvailable = _isAvailable;
            timeofDayText.text = LocalizationUtil.FindLocalizationEntry($"{chapter.timeOfDay.ToString()}", string.Empty, false, TableCategory.UI);

            //previewImage.sprite = chapter.chapterImage;
        }
    }

}
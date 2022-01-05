using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;

namespace ho
{
    public class ChapterUI : BaseUI
    {
        [SerializeField] Button         mainMenuButton;
        [SerializeField] Button         settingsButton;
        [SerializeField] Button         playButton;

        [SerializeField] Button nextButton;
        [SerializeField] Button previousButton;

        [SerializeField] Boomzap.Conversation.Conversation   endOfSurveyConversation;

        [SerializeField] ChapterBuilder[] chapterBuilders;

        [SerializeField] CarouselViewHolder viewHolder;

        Chapter selectedChapter = null;

        int selectedChapterIndex = 1;

        public Chapter.Entry selectedEntry = null;

        // Start is called before the first frame update
        void Start()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuButton);

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => OnPlaySelectedEntry());

            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Popup.ShowPopup<OptionsPopup>());

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButton);

            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousButton);
        }

        void OnNextButton()
        {
            var exitBuilder = chapterBuilders.First(x => x.GetAnimState() == CarouselView.CarouselAnimState.Exit);

            int chapterCount = GameController.instance.gameChapters.Count;

            if(selectedChapterIndex + 2 >= chapterCount)
            {
                exitBuilder.SetupChapter(GameController.instance.gameChapters[selectedChapterIndex + 2 - chapterCount]);
            }
            else
                exitBuilder.SetupChapter(GameController.instance.gameChapters[selectedChapterIndex + 2]);

            selectedChapterIndex++;

            if (selectedChapterIndex >= chapterCount)
                selectedChapterIndex = 0;

            viewHolder.CycleToLeft();
        }

        void OnPreviousButton()
        {
            var exitBuilder = chapterBuilders.First(x => x.GetAnimState() == CarouselView.CarouselAnimState.Exit);

            int chapterCount = GameController.instance.gameChapters.Count;

            if (selectedChapterIndex - 2 < 0)
            {
                exitBuilder.SetupChapter(GameController.instance.gameChapters[selectedChapterIndex - 2 + chapterCount]);
            }
            else
                exitBuilder.SetupChapter(GameController.instance.gameChapters[selectedChapterIndex - 2]);

            selectedChapterIndex--;

            if (selectedChapterIndex < 0)
                selectedChapterIndex = chapterCount - 1;

            viewHolder.CycleToRight();
        }

        void OnPlaySelectedEntry()
        {
            if (selectedEntry == null) return;

            GameController.instance.isUnlimitedMode = false;
            GameController.instance.LaunchBootEntry(selectedEntry);
        }

        void OnMainMenuButton()
        {
            onHiddenOneshot += () => GameController.instance.FadeToGameMenu();
            Hide();
        }
        protected override void OnFinishShow()
        {
            base.OnFinishShow();
            Tutorial.TriggerTutorial(Tutorial.Trigger.ChapterMenu);

            Audio.instance.PlayMusic(UIController.instance.mainMenuUI.MainMenuMusic);
            Audio.instance.PlayAmbient(null);
        }


#if SURVEY_BUILD
        void EndOfSurveyContent()
        {
            GameController.instance.PlayConversation(endOfSurveyConversation, (_) => GameController.instance.FadeToSurveyEnd());
        }
#endif

        public void SetupBeforeShow()
        {
            Chapter.Entry nextIncomplete = GameController.save.GetNextIncomplete();
            List<Chapter> chaptersToLoad = GameController.instance.gameChapters;

            if (nextIncomplete != null)
                selectedChapter = chaptersToLoad.First(x => x.sceneEntries.Contains(nextIncomplete));
            else
                selectedChapter = chaptersToLoad.Last();

            int chapterIndex = chaptersToLoad.IndexOf(selectedChapter);

            var centerBuilder = chapterBuilders.First(x => x.GetAnimState() == CarouselView.CarouselAnimState.Center);
            centerBuilder.SetupChapter(chaptersToLoad[chapterIndex]);

            var leftBuilder = chapterBuilders.First(x => x.GetAnimState() == CarouselView.CarouselAnimState.Left);

            if (chapterIndex - 1 < 0)
            {
                leftBuilder.SetupChapter(chaptersToLoad[chaptersToLoad.Count - 1]);
            }
            else
                leftBuilder.SetupChapter(chaptersToLoad[chapterIndex - 1]);

            var rightBuilder = chapterBuilders.First(x => x.GetAnimState() == CarouselView.CarouselAnimState.Right);

            if (chapterIndex + 1 >= chaptersToLoad.Count)
            {
                rightBuilder.SetupChapter(chaptersToLoad[0]);
            }
            else
            {
                rightBuilder.SetupChapter(chaptersToLoad[chapterIndex + 1]);
            }

            selectedChapterIndex = chapterIndex;

            viewHolder.SetDefaultColors();
        }
    }
}

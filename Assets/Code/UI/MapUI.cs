using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace ho
{
    public class MapUI : BaseUI
    {
        [SerializeField] Button         mainMenuButton;
        [SerializeField] Button         chapterListButton;
        [SerializeField] Button         settingsButton;

        // Start is called before the first frame update
        void Start()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => GameController.instance.FadeToGameMenu());

            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Popup.ShowPopup<OptionsPopup>());

            chapterListButton.onClick.RemoveAllListeners();
            chapterListButton.onClick.AddListener(() => GameController.instance.FadeToChapterMenu());
        }


        // Update is called once per frame
        void Update()
        {

        }

        protected override void OnFinishShow()
        {
            base.OnFinishShow();

            if(GameController.instance.currentChapter && GameController.instance.isUnlimitedMode == false)
            {
                CheckConversationsForChapter(GameController.instance.currentChapter);
            }

            Audio.instance.PlayMusic(UIController.instance.mainMenuUI.MainMenuMusic);

            UIController.instance.eventSystem.gameObject.SetActive(true);
        }

        void CheckTutorials(bool isPreBoss)
        {
            //if (isPreBoss)
            //{
            //    Tutorial.TriggerTutorial(Tutorial.Trigger.MapBoss);
            //} else if (GameController.instance.currentChapter.sceneEntries.Any(x => x.IsMinigame))
            //{
            //    Tutorial.TriggerTutorial(Tutorial.Trigger.MapPuzzle);
            //} else
            //{
            //    //Tutorial.TriggerTutorial(Tutorial.Trigger.Map);
            //}
        }

        void CheckConversationsForChapter(Chapter forChapter)
        {
            if (forChapter.sceneEntries.All(x => GameController.save.IsChapterEntryComplete(x)))
            {
                //Finish Chapter Conversation
                UIController.instance.isShowEndCredits = true;
                //GameController.instance.PlayConversation(forChapter.finishChapterConversation, (bool _) => GameController.instance.FadeToChapterMenu());
            } 
            else if (!forChapter.sceneEntries.Any(x => GameController.save.IsChapterEntryComplete(x)))
            {
                //Open Chapter Conversation
                //GameController.instance.PlayConversation(forChapter.openChapterConversation, (bool _) => CheckTutorials(false));
            }
        }
    }
}

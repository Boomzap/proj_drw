using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class Cheat : SimpleSingleton<Cheat>
    {
        public Material CheatOutlineMat;

        [SerializeField] GameObject cheatsPanel;
        [SerializeField] GameObject mainMenuPanel;

        public bool cheatsEnabled = false;

        [SerializeField] Button unlockChapters;
        [SerializeField] Button unlockScenes;
        [SerializeField] Button skipTutorial;
        [SerializeField] Button clearSave;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool devConsoleOpen = false;
#endif

    private void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || ENABLE_CHEATS
            unlockChapters.onClick.RemoveAllListeners();
            unlockChapters.onClick.AddListener(() => UnlockChapters());

            unlockScenes.onClick.RemoveAllListeners();
            unlockScenes.onClick.AddListener(() => UnlockScenes());

            skipTutorial.onClick.RemoveAllListeners();
            skipTutorial.onClick.AddListener(() => SkipTutorial());

            clearSave.onClick.RemoveAllListeners();
            clearSave.onClick.AddListener(() => ClearSave());

            cheatsEnabled = cheatsPanel.activeInHierarchy;
#endif
        }
        private void Update()
        {
#if ENABLE_CHEATS || UNITY_EDITOR || DEVELOPMENT_BUILD
            if (UIController.instance.mainMenuUI.gameObject.activeInHierarchy)
                EnablePanel(mainMenuPanel);
            else if(UIController.instance.hoMainUI.gameObject.activeInHierarchy)
                EnablePanel(null);
            else
                EnablePanel(null);

            if (Input.GetKeyDown(KeyCode.F2))
            {
                cheatsEnabled = !cheatsPanel.activeInHierarchy;
                cheatsPanel.gameObject.SetActive(cheatsEnabled);
            }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F3))
            {
                devConsoleOpen = !devConsoleOpen;
                Debug.developerConsoleVisible = devConsoleOpen;
            }
#endif
        }

#if ENABLE_CHEATS || DEVELOPMENT_BUILD || UNITY_EDITOR
        public void EnablePanel(GameObject panel)
        {
            mainMenuPanel.gameObject.SetActive(panel == mainMenuPanel);
        }
        public void UnlockChapters()
        {
            GameController.save.currentProfile.flags.SetFlag("story_complete", true);
            UIController.instance.mainMenuUI.bonusButton.interactable = true;
            foreach (var c in GameController.instance.gameChapters)
            {
                GameController.save.SetChapterAvailable(c, true);

                foreach (var entry in c.sceneEntries)
                {
                    GameController.save.GetSceneState(entry);
                }
            }
        }

        public void UnlockScenes()
        {
            UIController.instance.mainMenuUI.unlimitedButton.interactable = true;
            for (int cidx = 0; cidx < GameController.instance.gameChapters.Count; cidx++)
            {
                Chapter chapter = GameController.instance.gameChapters[cidx];
                //Unlock SceneEntries
                for (int i = 0; i < chapter.sceneEntries.Length; i++)
                {
                    Chapter.Entry entry = chapter.sceneEntries[i];

                    if (entry.IsHOScene)
                    {
                        Savegame.HOSceneState state = GameController.save.GetSceneState(entry);

                        state.unlocked = true;

                    }
                    if (entry.IsMinigame)
                        GameController.save.SetMinigameComplete(entry);
                }
            }
        }

        public void SkipTutorial()
        {
            Tutorial.instance.SkipTutorials();
        }

        public void ClearSave()
        {
            GameController.instance.ClearSave();
            Popup.GetPopup<ProfilePopup>().Reset();
        }
#endif
    }
}
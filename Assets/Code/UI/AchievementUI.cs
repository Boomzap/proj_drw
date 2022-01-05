using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Steam;
using TMPro;
using System.Linq;

namespace ho
{
    public class AchievementUI : BaseUI
    {
        [BoxGroup("UI Buttons"), SerializeField] Button mainMenuButton;
        [BoxGroup("UI Buttons"), SerializeField] Button journalButton;
        [BoxGroup("UI Buttons"), SerializeField] Button unlimitedButton;

        [SerializeField] TextMeshProUGUI achievementsUnlockedText;

        [SerializeField] Transform content;
        [SerializeField] GameObject lockedPrefab;
        [SerializeField] GameObject achievementPrefab;

        List<GameObject> achievementObjects = new List<GameObject>();

        void OnUnlimitedMenu()
        {
            onHiddenOneshot += () => UIController.instance.unlimitedUI.Show();
            Hide();
        }

        void OnJournalButton()
        {
            onHiddenOneshot += () => UIController.instance.journalUI.Show();
            Hide();
        }

        void OnMainMenu()
        {
            onHiddenOneshot += () => UIController.instance.mainMenuUI.Show();
            Hide();
        }

        public override void Init()
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenu);

            journalButton.onClick.RemoveAllListeners();
            journalButton.onClick.AddListener(OnJournalButton);

            unlimitedButton.onClick.RemoveAllListeners();
            unlimitedButton.onClick.AddListener(OnUnlimitedMenu);
        }

        void BuildAchievements()
        {
            achievementObjects.ForEach(x => Destroy(x));
            achievementObjects.Clear();

            var achievementList = SteamAchievements.GetAchievements();

            int achievementsUnlocked = 0;

            //Create Objects for achievements first;

            List<SteamAchievements.Achievement> achievementsLocked = new List<SteamAchievements.Achievement>();

            foreach(var achievement in achievementList)
            {
                SteamAchievements.Achievement setAchievement = (SteamAchievements.Achievement)achievement;

                if (setAchievement == SteamAchievements.Achievement.NONE) continue;


                bool isAchievementAquired = SteamAchievements.HasAchievement(setAchievement);

                if (isAchievementAquired)
                {
                    achievementsUnlocked++;
                    GameObject newObject = Instantiate(achievementPrefab, content);
                    AchievementEntry entry = newObject.GetComponent<AchievementEntry>();
                    entry.SetupSkinData(SteamAchievements.instance.achievementSkins.GetSkinData(setAchievement));
                    newObject.SetActive(true);
                    achievementObjects.Add(newObject);
                }
                else
                    achievementsLocked.Add(setAchievement);
            }


            //NOTE: There's an extra achievement item "NONE" which is the default option.
            //We want to not include that in achievements count
            achievementsUnlockedText.text = $"{achievementsUnlocked}/{achievementList.Length - 1}";

            achievementsLocked.ForEach(x => 
            {
                GameObject newObject = Instantiate(lockedPrefab, content);
                AchievementEntry entry = newObject.GetComponent<AchievementEntry>();
                entry.SetupSkinData(SteamAchievements.instance.achievementSkins.GetSkinData(x), true);
                newObject.SetActive(true);
                achievementObjects.Add(newObject);
            });
        }

        protected override void OnBeginShow(bool instant)
        {
            base.OnBeginShow(instant);
            BuildAchievements();

            unlimitedButton.interactable = GameController.instance.isFirstHOSceneUnlocked;
        }

    }
}


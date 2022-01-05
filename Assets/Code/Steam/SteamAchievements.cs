using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif


namespace Steam
{
    public class SteamAchievements : SimpleSingleton<SteamAchievements>
    {
        public GameObject steamManagerPrefab;

        static SteamManager steamManager = null;

        static List<Achievement> achievementsToLoad = new List<Achievement>();

        public static int achievementsToLoadCount => achievementsToLoad.Count;

        public ho.AchievementSkin achievementSkins;

        //List down below Achievement keys you'll be using
        public enum Achievement
        {
            NONE,
            ACH_CHAPTER_1_FIN, //Done
            ACH_CHAPTER_14_FIN, //Done
            ACH_CHAPTER_20_FIN, // Done
            ACH_TRIVIA_FOUND_ALL, //Done
            ACH_SUBGAME_FIN_10,
            ACH_SCENE_FIN_90, //Done
            ACH_SCENE_FIN_180, //Done
            ACH_SCENE_FIN_360, //Done
            ACH_PLAY_UNLIMITED, // Done
            ACH_PLAY_SILHOUETTE, // Done
            ACH_PLAY_IMAGE, // Done
            ACH_PLAY_RIDDLE, // Done
            ACH_PLAY_COLLECTION, // Done
            ACH_PLAY_SCRAMBLE, // Done
            ACH_PLAY_NOVOWEL, // Done
            ACH_PLAY_PAIRS, // Done
        }

        //List down achievement stats you'll be using
        public enum Stat
        {
            NONE,
            ACH_STAT_SUBGAME_NO_HINT
        }

        private void Awake()
        {
#if !DISABLESTEAMWORKS
            //if (ho.SystemSaveContainer.instance.Vendor.Contains("bz"))
            //{
            //    steamManager = Instantiate(steamManagerPrefab).GetComponent<SteamManager>();
            //}
#endif
        }

        public static Array GetAchievements()
        {
            var achievements = Enum.GetValues(typeof(Achievement));

            return achievements;
        }

        public static void ClearAchievementsList()
        {
            achievementsToLoad.Clear();
        }

        public static void SaveAchievements()
        {
            foreach(var achievement in achievementsToLoad)
            {
                if (ho.GameController.save.IsFlagSet("achievement_" + achievement.ToString()) == false)
                    ho.GameController.save.currentProfile.flags.SetFlag("achievement_" + achievement.ToString(), true);
            }
            ClearAchievementsList();
            ho.Savegame.SetDirty();
        }

        public static bool HasAchievement(Achievement achievement)
        {
            return ho.GameController.save.IsFlagSet("achievement_" + achievement.ToString());
        }

        public static List<Achievement> GetAchievementsAcquired()
        {
            return achievementsToLoad;
        }

        public static void SetAchievement(Achievement achievement)
        {
            return;
            //Check if achievement flag is set already and If added to achievement loading list
            if (ho.GameController.save.IsFlagSet("achievement_" + achievement.ToString()) == false && achievementsToLoad.Contains(achievement) == false)
                achievementsToLoad.Add(achievement);

#if !DISABLESTEAMWORKS
            if (steamManager == null) return;

            if (SteamManager.Initialized == false)
            {
                Debug.Log("SteamManager not initialized!");
                return;
            }

            SteamUserStats.SetAchievement(achievement.ToString());
            SteamUserStats.StoreStats();
#endif
        }

        public static void SetAchievementStat(Stat statName, int stat)
        {
            if(statName == Stat.ACH_STAT_SUBGAME_NO_HINT && stat > 10)
            {
                SetAchievement(Achievement.ACH_SUBGAME_FIN_10);
            }


#if !DISABLESTEAMWORKS
            if (steamManager == null) return;

            if (SteamManager.Initialized == false)
            {
                Debug.Log("SteamManager not initialized!");
                return;
            }

            SteamUserStats.SetStat(statName.ToString(), stat);
            SteamUserStats.StoreStats();
#endif
        }

        #region Editor
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !DISABLESTEAMWORKS
        public static void ResetAchievements()
        {
            Debug.Log("Steam Achievements Reset!");
            SteamUserStats.ResetAllStats(true);
        }
        private void OnGUI()
        {
            if (steamManager == null) return;

            if (SceneManager.GetActiveScene().name == "Splash") return;

            if (SteamManager.Initialized == false)
            {
                Debug.Log("SteamManager not initialized!");
                return;
            }
               

            if (ho.UIController.instance.mainMenuUI.gameObject.activeInHierarchy == false) return;

            if (GUI.Button(new Rect(20f, 100f, 150f, 30), "Reset Achievements"))
            {
                ResetAchievements();
            }
        }
#endif
        #endregion
    }
}


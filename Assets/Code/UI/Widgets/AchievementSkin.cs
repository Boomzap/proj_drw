using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Steam;
using System.Linq;

#if UNITY_EDITOR
using System;
#endif

namespace ho
{
    [CreateAssetMenu(fileName = "AchievementSkin", menuName = "HO/Achievements", order = 1)]
    public class AchievementSkin : ScriptableObject
    {
        [System.Serializable]
        public class SkinData
        {
            public SteamAchievements.Achievement achievement;

            public Sprite bg;
            public Sprite face;
            public Sprite props;
            public Sprite header;

            [ReadOnly]
            public string achievementDescKey;

            [ReadOnly]
            public string achievementKey => achievement.ToString();
#if UNITY_EDITOR
            [ReadOnly]
            public string achevementKeyText;
#endif

        }

        [InlineProperty]
        public SkinData[] achievementSkins;

        public SkinData GetSkinData(SteamAchievements.Achievement achievement)
        {
            if (achievementSkins.Any(x => x.achievement == achievement))
                return achievementSkins.FirstOrDefault(x => x.achievement == achievement);

            return null;
        }


#if UNITY_EDITOR
        [Button]
        void ReloadAchievements()
        {
            var achievementList = Enum.GetValues(typeof(SteamAchievements.Achievement));

            List<SkinData> achievementSkinData = new List<SkinData>();

            foreach(var achievement in achievementList)
            {
                if (achievement.ToString() == "NONE") continue;
                SteamAchievements.Achievement setAchievement = (SteamAchievements.Achievement)achievement;
                SkinData skinData = new SkinData();

                skinData.achievement = setAchievement;

                //Add Key for Achievement Title
                LocalizationUtil.FindLocalizationEntry($"{achievement.ToString()}", string.Empty, true, TableCategory.UI);

                skinData.achievementDescKey = LocalizationUtil.FindLocalizationEntry($"{achievement.ToString()}_desc", string.Empty, true, TableCategory.UI);

                skinData.achevementKeyText = LocalizationUtil.FindLocalizationEntry(skinData.achievementKey, string.Empty, false, TableCategory.UI);

                achievementSkinData.Add(skinData);
            }

            achievementSkins = achievementSkinData.ToArray();
        }
#endif

    }
}



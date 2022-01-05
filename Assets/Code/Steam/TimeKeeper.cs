using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steam
{
    public class TimeKeeper : MonoBehaviour
    {
        [Sirenix.OdinInspector.ShowInInspector]
        float timePlayed = 0f;

        bool recordTime => (ho.UIController.instance.isPointerOverUIObject || ho.HOGameController.instance.DisableInput) == false;
        // Update is called once per frame
        void Update()
        {
            if(recordTime)
            {
                timePlayed += Time.deltaTime;
            }
        }

        public void BeginRecordTime()
        {
            timePlayed = 0f;
        }

        public void OnSceneEnded()
        {
            if(timePlayed < 90f)
            {
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_90);
            }

            if (timePlayed < 180f)
            {
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_180);
            }

            if (timePlayed < 360f)
            {
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_360);
            }
        }
    }

}

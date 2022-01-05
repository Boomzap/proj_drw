using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Steam;
using System.Linq;
using UnityEngine.UI;

namespace ho
{
    public class AchievementPopup : Popup
    {
        [SerializeField] Button closeButton;

        public AchievementEntry[] achievementEntries;

        AchievementSkin.SkinData[] skinDatas => SteamAchievements.instance.achievementSkins.achievementSkins;

        int achievementCount = 0;

        public override void Init()
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());
        }


        IEnumerator AnimateAchievementsCor()
        {
            foreach (var entry in achievementEntries)
                entry.gameObject.SetActive(false);

            for(int i = 0; i < achievementCount; i++)
            {
                //Animation for entries is set to play on awake
                //Show Achievements one by one
                achievementEntries[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);
            }

            SteamAchievements.SaveAchievements();

            //closeButton.transform.localScale = Vector3.one;
            //iTween.ScaleFrom(closeButton.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.3f, "easetype", "easeOutBack"));

            closeButton.gameObject.SetActive(true);
            gameObject.PlayAnimation(this, "achievement_button_in", () => closeButton.transform.localScale = Vector3.one);
        }

        public override void Show(bool instant = false)
        {
            if (gameObject.activeInHierarchy)
                return;

            gameObject.SetActive(true);
            closeButton.transform.localScale = Vector3.zero;
            closeButton.gameObject.SetActive(false);

            OnBeginShow(instant);


            if (onShowAnimation != null)
            {
                gameObject.PlayAnimation(this, onShowAnimation.name, () =>
                {
                    StartCoroutine(AnimateAchievementsCor());
                });
            }

            if (UseBlackout)
            {
                UIController.instance.popupBlackout.OnShowPopup();
                UIController.instance.popupBlackout.transform.SetAsLastSibling();
            }

            transform.SetAsLastSibling();
        }


        public void SetUpAchievements()
        {
            var achievements = SteamAchievements.GetAchievementsAcquired();

            achievementCount = achievements.Count;

            //Debug.Log(achievements.Count);

            for (int i = 0; i < achievementEntries.Length; i++)
            {
                achievementEntries[i].gameObject.SetActive(false);

                if (i < achievementCount)
                {
                    AchievementSkin.SkinData skinData = skinDatas.FirstOrDefault(x => x.achievement == achievements[i]);

                    achievementEntries[i].SetupSkinData(skinData);
                }
            }
        }


#if UNITY_EDITOR
        public int testIndexStart;
        [Button]
        void TestFillAchievementEntries()
        {
            for(int i = 0; i < achievementEntries.Length; i++)
            {
                if(testIndexStart + i < skinDatas.Length)
                {
                    achievementEntries[i].SetupSkinData(skinDatas[testIndexStart + i]);
                }
            }
        }
#endif
    }
}


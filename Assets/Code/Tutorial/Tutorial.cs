using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    public class Tutorial : SimpleSingleton<Tutorial>
    {
        public enum Trigger
        {
            ChapterMenu, //
            DetailMode, //
            FindObject, //
            FindXMode, //
            //HOStandardMode,
            ImageMode, //
            OddMode, //
            PaintMG, // 
            RepairMode, // 
            ReverseHOMode,//
            RiddleMode, // 
            SceneButtons,
            ScoreMultiplier,
            SewMG,
            Silhouette,
            SpecialRiddle,
            SudokuMG,
            UnlimitedFirstLevel,
            UnlimitedRoomPlay,
            UnlimitedUnlocked,
            UnlockRoom,
            UseHint,
            Zoom
        }

        [SerializeField]
        List<TutorialEntry> tutorials = new List<TutorialEntry>();

        private void Reset()
        {
            ReloadTutorials();
        }

        public void SkipTutorials()
        {
            foreach (var tutorial in Enum.GetNames(typeof(Trigger)))
            {
                GameController.save.currentProfile.flags.SetFlag("tutorial_" + tutorial, true);
            }
            Savegame.SetDirty();
        }

        [Button]
        void ReloadTutorials()
        {
#if UNITY_EDITOR
            foreach (var e in Enum.GetNames(typeof(Trigger)))
            {
                var existing = tutorials.FirstOrDefault(x => x.name.ToLower().StartsWith(e.ToLower()));

                if (existing != null)
                {
                    //
                }
                else
                {
                    var newEntry = new TutorialEntry();
                    newEntry.name = e;

                    tutorials.Add(newEntry);
                }

            }
#endif
        }

        public static void TriggerTutorial(string tutorial)
        {
            if (Popup.IsPopupActive<TutorialUI>()) return;

            if (GameController.save.currentProfile.flags.HasFlag("tutorial_" + tutorial))
                return;

            var entry = instance.tutorials.FirstOrDefault(x => x.name == tutorial);
            if (entry == null) return;

            TutorialUI.RunTutorial(entry);
        }

        public static void TriggerTutorial(Trigger tutorial)
        {
            TriggerTutorial(tutorial.ToString());
        }
    }
}

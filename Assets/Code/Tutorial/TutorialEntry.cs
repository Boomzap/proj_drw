using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace ho
{
    [Serializable]
    public class TutorialEntry
    {
        [ReadOnly]
        public string name;
        public string next = "";
        public float delayBeforeShowingNext = 0f;

        [ReadOnly]
        public string header;
        [ReadOnly]
        public string body;

        //public bool isShortTutorial => Tutorial.Mode.Short == modeAvailable;

        public GameObject[] highlightUIObjects = new GameObject[0];

        public Vector3 popupPosition = Vector3.zero;

        //public GameObject mainMaskObject;

        //public Tutorial.Mode modeAvailable = Tutorial.Mode.Full;

        [BoxGroup("Additional highlights")]
        public bool highlightDoorObject = false;
        [BoxGroup("Additional highlights")]
        public bool highlightAnyFindableObject = false;
        [BoxGroup("Additional highlights")]
        public bool highlightTriviaObject = false;
        [BoxGroup("Additional highlights")]
        public bool highlightUIEntriesForSubsceneObjects = false;
        //[BoxGroup("Additional highlights")]
        //public bool highlightActiveMapEntries = false;
        //[BoxGroup("Additional highlights")]
        //public bool highlightMapBossEntry = false;
        //[BoxGroup("Additional highlights")]
        //public bool highlightMapPuzzleEntries = false;

        [Button, InfoBox("There is no 'rename fixing' - just name the tutorial correctly before you press this please.")]
        void CreateLocEntries()
        {
            header = LocalizationUtil.FindLocalizationEntry($"UI/Tutorial/{name}_header", string.Empty, true, TableCategory.UI);
            body = LocalizationUtil.FindLocalizationEntry($"UI/Tutorial/{name}_body", string.Empty, true, TableCategory.UI);
        }
    }
}

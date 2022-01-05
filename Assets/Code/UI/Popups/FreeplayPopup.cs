using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.UI;

namespace ho
{
    public class FreeplayPopup : Popup
    {
        [ReadOnly]
        public Button closeButton;

        [ReadOnly]
        public List<Button> gameModeButtons = new List<Button>();

        const string hoLogicStandard = "HOLogicStandard";
        const string hoLogicSilhouette = "HOLogicSilhouette";
        const string hoLogicPicture = "HOLogicPicture";
        const string hoLogicRiddle = "HOLogicRiddle";
        const string hoLogicCollection = "HOLogicFindX";


        MapNode currentNode;
        Chapter.Entry bootEntry;

        protected override void OnBeginShow(bool instant)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClose);

            foreach(Button button in gameModeButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnStartGame(button.name));
            }

            base.OnBeginShow(instant);
        }

        void OnClose()
        {
            currentNode = null;
            bootEntry = null;
            GameController.instance.GetWorldState<MapController>().launchingGameplay = false;
            Hide();
        }

        void OnStartGame(string mode)
        {
            if(mode.Contains("Normal"))
            {
                bootEntry.hoLogic = hoLogicStandard;
            }

            if (mode.Contains("Silhouette"))
            {
                bootEntry.hoLogic = hoLogicSilhouette;
            }
            if (mode.Contains("Picture"))
            {
                bootEntry.hoLogic = hoLogicPicture;
            }
            if (mode.Contains("Riddle"))
            {
                bootEntry.hoLogic = hoLogicRiddle;
            }
            if (mode.Contains("Collection"))
            {
                bootEntry.hoLogic = hoLogicCollection;
            }

            GameController.instance.LaunchBootEntry(bootEntry);

            currentNode = null;
            bootEntry = null;

            Hide();
        }



        public void Setup(MapNode selectedNode)
        {
            if(currentNode != selectedNode)
            {
                currentNode = selectedNode;
                bootEntry = new Chapter.Entry();
                bootEntry.hoRoom = currentNode.roomReferences[0];
                bootEntry.type = Chapter.EntryType.Scene;
                bootEntry.itemDifficulty = HODifficulty.Medium;
                bootEntry.objectCount = 16;
            }
        }


#if UNITY_EDITOR
        [Button]
        public void SetupButtons()
        {
            closeButton = GetComponentsInChildren<Button>().Single(x => x.name.Contains("Close"));
            gameModeButtons.Clear();
            gameModeButtons = GetComponentsInChildren<Button>().Where(x => x.name.Contains("Close") == false).ToList();
        }
#endif
    }
}


#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public partial class MainMenuUI : BaseUI
    {

        HORoomReference bootRoomRef = null;
        bool showBootWindow = false;
        HORoom bootRoom = null;
        Rect sceneBootWindowRect = new Rect(100, 100, 500, 500);
        bool showDebugWindow = false;
        Rect sceneDebugWindowRect = new Rect(50, 50, 500, 500);
        bool showMinigameWindow = false;
        Rect minigameWindowRect = new Rect(50, 50, 500, 500);
        bool showConversationWindow = false;
        Rect conversationWindowRect = new Rect(50, 50, 500, 500);

        Vector2 minigameListPos = Vector2.zero;
        Vector2 sceneListPos = Vector2.zero;
        Vector2 logicListPos = Vector2.zero;
        Vector2 convoListPos = Vector2.zero;
        string selectedRoomName;
        string selectedLogic;
        Chapter.Entry bootEntry;

        string convoFlags = "";

        List<string> conversationFlags = new List<string>();
        List<SerializableGUID> conversationNodes = new List<SerializableGUID>();
        List<SerializableGUID> conversationGUIDs = new List<SerializableGUID>();

        void OnGUI()
        {
            if (Cheat.instance.cheatsEnabled) return;

            if (GUI.Button(new Rect(20, 20, 100, 30), new GUIContent("Scenes")))
            {
                showDebugWindow = true;
            }

            if (GUI.Button(new Rect(140, 20, 100, 30), new GUIContent("Minigames")))
            {
                showMinigameWindow = true;
            }

            if (GUI.Button(new Rect(260, 20, 100, 30), new GUIContent("Clear Save")))
            {
                Cheat.instance.ClearSave();
            }

            if (GUI.Button(new Rect(380, 20, 100, 30), new GUIContent("Conversation")))
            {
                showConversationWindow = true;
            }

            //if (GUI.Button(new Rect(20, 60, 100, 30), new GUIContent("Freeplay")))
            //{
            //    GameController.save.currentProfile.flags.SetFlag("freeplay", true);
            //    //freePlayBtn.interactable = true;
            //}

            if (GUI.Button(new Rect(140, 60, 100, 30), new GUIContent("Unlock Chp")))
            {
                Cheat.instance.UnlockChapters();
            }

            if (GUI.Button(new Rect(260f, 60f, 100f, 30f), "Unlock Scenes"))
            {
                Cheat.instance.UnlockScenes();
            }

            if (GUI.Button(new Rect(380f, 60f, 100f, 30f), "Skip Tutorial"))
            {
                Cheat.instance.SkipTutorial();
            }

#if SURVEY_BUILD
            if (GUI.Button(new Rect(500, 20, 100, 30), new GUIContent("Survey Screen")))
            {
                GameController.instance.FadeToSurveyEnd();
            }
#endif

            if (showDebugWindow)
            {
                sceneDebugWindowRect = GUILayout.Window(0, sceneDebugWindowRect, OnSceneDebugWindow, new GUIContent("Scenes Debug"));
            }

            if (showBootWindow)
            {
                sceneBootWindowRect = GUILayout.Window(1, sceneBootWindowRect, OnSceneBootWindow, new GUIContent("Scene Boot"));
            }

            if (showMinigameWindow)
            {
                minigameWindowRect = GUILayout.Window(2, minigameWindowRect, OnMinigameWindow, new GUIContent("Minigame Debug"));
            }

            if (showConversationWindow)
            {
                conversationWindowRect = GUILayout.Window(3, conversationWindowRect, OnConversationWindow, new GUIContent("Conversation Debug"));
            }

        }

        void OnDebugConversationEnd(bool wasRun)
        {
            UIController.instance.mainMenuUI.Show(true);
            GameController.save.currentProfile.conversationFlags = new List<string>(conversationFlags);
            GameController.save.currentProfile.usedConversations = new List<SerializableGUID>(conversationGUIDs);
            GameController.save.currentProfile.usedConversationNodes = new List<SerializableGUID>(conversationNodes);
        }

        void OnConversationWindow(int windowid)
        {
            GUILayout.BeginVertical();
            convoListPos = GUILayout.BeginScrollView(convoListPos);

            for (int i = 0; i < Boomzap.Conversation.ConversationManager.instance.conversations.Length; i++)
            {
                if (GUILayout.Button(new GUIContent(Boomzap.Conversation.ConversationManager.instance.conversations[i].name)))
                {
                    conversationFlags = new List<string>(GameController.save.currentProfile.conversationFlags);
                    conversationGUIDs = new List<SerializableGUID>(GameController.save.currentProfile.usedConversations);
                    conversationNodes = new List<SerializableGUID>(GameController.save.currentProfile.usedConversationNodes);

                    foreach (var s in convoFlags.Split(','))
                    {
                        var f = s.Trim();
                        if (string.IsNullOrWhiteSpace(f)) continue;
                        if (GameController.save.currentProfile.conversationFlags.Contains(f)) continue;

                        GameController.save.currentProfile.conversationFlags.Add(f);
                    }

                    //Boomzap.Conversation.ConversationManager.dontSetUsedFlags = true;
                    UIController.instance.HideAll(true);
                    BGManager.instance.PreloadBackgrounds(Boomzap.Conversation.ConversationManager.instance.conversations[i]);
                    GameController.instance.PlayConversation(Boomzap.Conversation.ConversationManager.instance.conversations[i], OnDebugConversationEnd, true);

                    showConversationWindow = false;
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(1);

            GUILayout.Label("Flags to set for this conversation (comma separated)");
            convoFlags = GUILayout.TextField(convoFlags);


            if (GUILayout.Button("Close"))
                showConversationWindow = false;
            GUILayout.EndVertical();


            GUI.DragWindow();
        }

        void OnMinigameWindow(int windowid)
        {
            GUILayout.BeginVertical();
            minigameListPos = GUILayout.BeginScrollView(minigameListPos);

            for (int i = 0; i < MinigameController.instance.minigamePrefabNames.Length; i++)
            {
                if (GUILayout.Button(new GUIContent(MinigameController.instance.minigamePrefabNames[i])))
                {
                    //start debug room
                    Chapter.Entry bootEntry = new Chapter.Entry();

                    bootEntry.type = Chapter.EntryType.Minigame;
                    bootEntry.minigame = MinigameController.instance.minigamePrefabs[i];

                    MinigameController.instance.returnToMainMenu = true;
                    GameController.instance.FadeToMinigame(bootEntry);

                    showBootWindow = showDebugWindow = showMinigameWindow = false;
                }
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
                showMinigameWindow = false;
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void OnSceneDebugWindow(int windowid)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            sceneListPos = GUILayout.BeginScrollView(sceneListPos);

            foreach (var room in HORoomAssetManager.instance.roomTracker.roomEntries)
            {
                if (room.roomName.Contains("sub"))
                    continue;

                if (GUILayout.Button(new GUIContent(room.roomName)) || string.IsNullOrEmpty(selectedRoomName))
                {
                    selectedRoomName = room.roomName;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            logicListPos = GUILayout.BeginScrollView(logicListPos);

            for (int i = 0; i < HOFindableLogicValidity.logicTypes.Length; i++)
            {
                if (GUILayout.Button(new GUIContent(HOFindableLogicValidity.friendlyTypes[i])) || string.IsNullOrEmpty(selectedLogic))
                {
                    selectedLogic = HOFindableLogicValidity.logicTypes[i];
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Label(new GUIContent($"Room: {selectedRoomName}, Logic: {selectedLogic}"));
            if (GUILayout.Button("Play"))
            {
                DebugPlayRoom(selectedRoomName, selectedLogic);
                showDebugWindow = false;
            }

            if (GUILayout.Button("Close"))
                showDebugWindow = false;

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void OnSceneBootWindow(int windowid)
        {
            if (bootRoom == null)
            {
                showBootWindow = false;
                return;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label("Unlocked: ");
            foreach (var sub in bootRoom.subHO)
            {
                bool unlocked = bootEntry.unlockedSubHO.Contains(sub);
                bool want = GUILayout.Toggle(unlocked, sub.roomName);
                if (unlocked != want)
                {
                    if (want)
                        bootEntry.unlockedSubHO.Add(sub);
                    else
                        bootEntry.unlockedSubHO.Remove(sub);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Locked: ");
            foreach (var sub in bootRoom.subHO)
            {
                bool unlocked = bootEntry.unlockableSubHO.Contains(sub);
                bool want = GUILayout.Toggle(unlocked, sub.roomName);
                if (unlocked != want)
                {
                    if (want)
                        bootEntry.unlockableSubHO.Add(sub);
                    else
                        bootEntry.unlockableSubHO.Remove(sub);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Special items: ");
            foreach (var special in bootRoom.GetComponentsInChildren<HOFindableObject>().Where(x => x.isSpecialStoryItem))
            {
                bool unlocked = bootEntry.specialItems.Contains(special.name);
                bool want = GUILayout.Toggle(unlocked, special.name);
                if (unlocked != want)
                {
                    if (want)
                        bootEntry.specialItems.Add(special.name);
                    else
                        bootEntry.specialItems.Remove(special.name);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            bool easy = GUILayout.Toggle(bootEntry.itemDifficulty == HODifficulty.Easy, "Easy");
            if (easy) bootEntry.itemDifficulty = HODifficulty.Easy;

            bool med = GUILayout.Toggle(bootEntry.itemDifficulty == HODifficulty.Medium, "Medium");
            if (med) bootEntry.itemDifficulty = HODifficulty.Medium;

            bool hard = GUILayout.Toggle(bootEntry.itemDifficulty == HODifficulty.Hard, "Hard");
            if (hard) bootEntry.itemDifficulty = HODifficulty.Hard;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Item count: {bootEntry.objectCount}: ");
            bootEntry.objectCount = (int)GUILayout.HorizontalSlider((float)bootEntry.objectCount, 1, 30);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Cancel"))
            {
                showBootWindow = false;
                if (bootRoom != null || bootRoomRef != null)
                {
                    HORoomAssetManager.instance.UnloadRoom(bootRoomRef);
                    bootRoom = null;
                    bootRoomRef = null;
                }
            }

            if (GUILayout.Button("Play"))
            {
                HOGameController.instance.returnToMainMenu = true;
                GameController.instance.FadeToHOGame(bootEntry);
                bootRoom = null;
                bootRoomRef = null;
                bootEntry = null;
                showBootWindow = showDebugWindow = showMinigameWindow = false;
            }

            GUI.DragWindow();
        }

        void SceneStartDebugBoot(GameObject go)
        {
            HORoom rm = go.GetComponent<HORoom>();
            bootRoom = rm;

            if (selectedLogic == "HOLogicDebug")
            {
                GameController.instance.FadeToHOGame(bootEntry);
                HOGameController.instance.returnToMainMenu = true;
                bootRoom = null;
                bootRoomRef = null;
                bootEntry = null;
                showBootWindow = showDebugWindow = showMinigameWindow = false;
            }
            else
            {
                showBootWindow = true;
            }
        }

        void DebugPlayRoom(string room, string logic)
        {
            if (bootRoom != null || bootRoomRef != null)
            {
                HORoomAssetManager.instance.UnloadRoom(bootRoomRef);
                bootRoom = null;
                bootRoomRef = null;
            }
            bootEntry = new Chapter.Entry();

            bootRoomRef = HORoomAssetManager.instance.roomTracker.GetItemByName(room);
            bootEntry.hoRoom = bootRoomRef;
            bootEntry.hoLogic = selectedLogic;
            HORoomAssetManager.instance.LoadRoomAsync(bootRoomRef, SceneStartDebugBoot);
        }

    }
}

#endif

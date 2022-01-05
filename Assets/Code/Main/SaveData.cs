using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using System.Linq;

using System;


namespace ho
{
    [Serializable]
    public class Savegame
    {
        [Serializable]
        public class HOSceneState
        {
            // <> chapter.Entry.saveGUID
            public SerializableGUID     saveGUID;

            public List<string>         currentObjects = new List<string>();
            public List<string>         futureObjects = new List<string>();

            public List<string>         inactiveObjects = new List<string>();

            public List<string>         closedDoors = new List<string>();
            public List<string>         openDoors = new List<string>();

            public List<string>         heldKeyItems = new List<string>();

            public bool                 hasSaveState = false;

            public bool                 unlocked = false;
            public bool                 completed { get { return hasSaveState && futureObjects.Count == 0 && currentObjects.Count == 0; } }
        };


        //Data for each logic
        [Serializable]
        public class ModeData
        {
            public string modeName;
            public int highScore = 0;
            public int playedGames = 0;
            public int fastestGameClear = int.MaxValue;
        }

        //Container for Int based achievements
        [Serializable]
        public class IntContainers
        {
            public List<IntContainer> containers = new List<IntContainer>();
            [Serializable]
            public class IntContainer
            {
                public string keyName;
                public int value;
            }

            public bool HasContainer(string name)
            {
                return containers.Any(x => x.keyName.Equals(name));
            }

            public void SetValueByName(string name, int newValue)
            {
                if (containers.Any(x => x.keyName.Equals(name)))
                {
                    var container = containers.FirstOrDefault(x => x.keyName.Equals(name));
                    container.value = newValue;
                }
                else
                {
                    //Create New Container
                    IntContainer newContainer = new IntContainer()
                    {
                        keyName = name,
                        value = newValue
                    };

                    containers.Add(newContainer);
                    SetDirty();
                }
            }
            public IntContainer GetContainerByName(string name)
            {
                if (containers.Any(x => x.keyName.Equals(name)))
                {
                    return containers.FirstOrDefault(x => x.keyName.Equals(name));
                }

                IntContainer newContainer = new IntContainer()
                {
                    keyName = name,
                    value = 0
                };

                containers.Add(newContainer);
                SetDirty();

                return newContainer;
            }

            public int GetValueByName(string name)
            {
                if(containers.Any(x => x.keyName.Equals(name)))
                {
                    return containers.FirstOrDefault(x => x.keyName.Equals(name)).value;
                }

                IntContainer newContainer = new IntContainer()
                {
                    keyName = name,
                    value = 0
                };

                containers.Add(newContainer);
                SetDirty();

                return newContainer.value;
            }

            public int IncreaseValueByName(string name, int increase = 1)
            {
                int value = GetValueByName(name);
                value += increase;

                SetValueByName(name, value);

                return value;
            }
        }


        //Combined HO Room Data
        [Serializable]
        public class HORoomData
        {
            public string assetGUID;
            public List<string> triviasFound = new List<string>();
            public List<ModeData> modesData = new List<ModeData>();
        }

        [Serializable]
        public class MinigameState
        {
            // <> chapter.Entry.saveGUID
            public SerializableGUID     saveGUID;
            public bool                 completed = false;
        }

        [Serializable]
        public class Profile
        {
            public string                       playerName;
            public List<HOSceneState>           hoSceneStates = new List<HOSceneState>();
            public List<MinigameState>          minigameStates = new List<MinigameState>();
	        public List<SerializableGUID>       usedConversations = new List<SerializableGUID>();
	        public List<SerializableGUID>		usedConversationNodes = new List<SerializableGUID>();
	        public Flags			            flags = new Flags();
            public FlagSetGUIDBool              availableChapters = new FlagSetGUIDBool();
            public FlagSetGUIDBool              completedChapters = new FlagSetGUIDBool();
            public IntContainers                intContainers = new IntContainers();
            public SerializableGUID             lastPlayedChapter;
            public SerializableGUID             saveGUID = new SerializableGUID();
            public List<string>                 conversationFlags = new List<string>();
            public List<HORoomData>             hoRoomDatas = new List<HORoomData>();
            public int                          hoDifficultyIndex = 0;
            public long                         timePlayedSeconds = 0;

        }

        [NonSerialized]
        public bool                         isDirty = false;

        [NonSerialized]
        public Profile                      currentProfile = null;

        public List<Profile>                profiles = new List<Profile>();
        [SerializeField, HideInInspector]   SerializableGUID currentProfileGUID;

        public void ResetHOProfile()
        {
            if(currentProfile != null && currentProfile.hoSceneStates.Any(x => x.completed == false && x.hasSaveState))
            {
                var incompleteScenes = currentProfile.hoSceneStates.Where(x => x.completed == false && x.hasSaveState).ToList();
                foreach(var incompleteScene in incompleteScenes)
                {
                    currentProfile.hoSceneStates.Remove(incompleteScene);
                }
            }
        }

        public Profile CreateProfile(string playerName = "")
        {
            Profile p = new Profile();
            p.playerName = playerName;

            //if (string.IsNullOrEmpty(playerName))
            //{
            //    p.playerName = (I2.Loc.LocalizedString)"UI/DefaultPlayerName";
            //}
             
            p.availableChapters.Set(GameController.instance.gameChapters[0].saveGUID, true);

            profiles.Add(p);
            SetProfile(p);

            return p;
        }

        public void SetProfile(Profile profile)
        {
            currentProfile = profile;
            currentProfileGUID = profile == null ? new SerializableGUID(Guid.Empty) : profile.saveGUID;
            SetDirty();
        }

        public Profile DeleteProfile(Profile profile)
        {
            profiles.Remove(profile);

            //Update Current Profile
            if (profiles.Count > 0) SetProfile(profiles.First());
            else SetProfile(null);

            return currentProfile;
        }

        public string ValidateProfileName(string name)
        {
            bool isNameAvailable = profiles.Where(x => x.playerName.Equals(name)).Select(x => x.playerName).Count() == 0;

            if (isNameAvailable == false)
                //return "A profile with this name already exists.";
                return LocalizationUtil.FindLocalizationEntry("UI/Prompt/PlayerNameExists_body", "", false, TableCategory.UI);
            if (string.IsNullOrEmpty(name))
                //return "Please enter a valid character.";
                return LocalizationUtil.FindLocalizationEntry("UI/Prompt/EnterValidCharacter_body", "", false, TableCategory.UI);

            //NOTE* From here on Profile Name is Valid
            return "valid";
        }


        public void Init()
        {
            profiles.Clear();
            currentProfile = null;
            currentProfileGUID = new SerializableGUID(Guid.Empty);

            //currentProfile = CreateProfile();
            //profiles.Add(currentProfile);
            //currentProfileGUID = currentProfile.saveGUID;
        }

        #if CE_BUILD
        public bool                         canPlayCEContent => true;
        #else
        public bool                         canPlayCEContent => false;
        #endif

        public bool                         canPlayFreePlay  { get { return currentProfile?.flags.HasFlag("freeplay") ?? false; } }

        public Chapter                      GetNextChapter(Chapter curChapter)
        {
            int curIndex = GameController.instance.gameChapters.IndexOf(curChapter);
            if (curIndex >= 0 && (curIndex+1 < GameController.instance.gameChapters.Count))
            {
                var nextChapter = GameController.instance.gameChapters[curIndex + 1];
                if (IsChapterAvailable(nextChapter))
                    return nextChapter;
            }

            return curChapter;
        }

        public bool                         IsChapterAvailable(Chapter chapter)
        {
            bool unlocked = currentProfile.availableChapters.Get(chapter.saveGUID);
            bool ceSatisfied = !chapter.isCEContent || canPlayCEContent;

            #if SURVEY_BUILD
            unlocked &= chapter.isSurveyContent;
            #endif

            return unlocked && ceSatisfied;
        }

        public void                         SetChapterAvailable(Chapter chapter, bool available)
        {
            currentProfile.availableChapters.Set(chapter.saveGUID, available);
            SetDirty();
        }

        public bool                         IsChapterComplete(Chapter chapter)
        {
            return currentProfile.completedChapters.Get(chapter.saveGUID);
        }

        public bool                         IsChapterReplayComplete(Chapter chapter)
        {
            return chapter.sceneEntries.All(x => IsChapterEntryComplete(x));
        }

        public void                         SetChapterComplete(Chapter chapter, bool complete)
        {
            currentProfile.completedChapters.Set(chapter.saveGUID, complete);
            SetDirty();
        }

        static public void SetDirty()
        {
            GameController.save.isDirty = true;
        }

        public static string GetPath(string toFile)
	    {
#if SURVEY_BUILD
                var surveyPath = Path.Combine(Application.persistentDataPath, "survey");
    		    return Path.Combine(surveyPath, toFile);
#else
            return Path.Combine(Application.persistentDataPath, toFile);
            #endif
        }

        #region Saving and Loading
        // check for null pointers introduced by new data fields here.
        public bool Validate()
	    {
            currentProfile = null;

            foreach (var profile in profiles)
            {
                if (profile.saveGUID == currentProfileGUID)
                {
                    currentProfile = profile;
                    break;
                }
            }

            if (currentProfile == null)
            {
                Debug.LogWarning("Invalid guid for currentProfile");
                if (profiles.Count > 0)
                    currentProfile = profiles[0];
                else
                    return false;
            }

            foreach (var p in profiles)
            {
                if (p.conversationFlags == null)
                    p.conversationFlags = new List<string>();
            }
// 
//     #if !DISABLESTEAMWORKS
// 		    currentProfile.flags.SetFlag("steam", true);
//     #else
// 		    currentProfile.flags.DeleteFlag("steam");
//     #endif

            return true;
	    }

        public bool SerializeBinary(Stream toStream)
        {
            try
		    {
			    BinaryFormatter bf = new BinaryFormatter();

                if (currentProfile != null)
                    currentProfileGUID = currentProfile.saveGUID;

                bf.Serialize(toStream, this);

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Exception " + e.ToString() + " saving SaveData");
                
                return false;
            }
        }

        public static Savegame LoadBinary(FileStream fromStream)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                Savegame loadedState = bf.Deserialize(fromStream) as Savegame;

                if (loadedState == null) return null;

                if (!loadedState.Validate()) return null;

                return loadedState;
            } catch (Exception e)
            {
                // .. on second thought, just throw it up?
                throw e;
            }
        }

        #endregion

        #region Conversation
       
        public bool			    IsConversationTriggered(SerializableGUID conversationGUID)	
        { 
            return currentProfile.usedConversations.Contains(conversationGUID);
        }

        public void             SetConversationTriggered(SerializableGUID conversationGUID)
        {
            if (currentProfile.usedConversations.Contains(conversationGUID)) return;

            currentProfile.usedConversations.Add(conversationGUID);
            SetDirty();
        }

        public bool             IsConversationNodeUsed(SerializableGUID nodeGUID)
        {
            return currentProfile.usedConversationNodes.Contains(nodeGUID);
        }

        public void             SetConversationNodeUsed(SerializableGUID nodeGUID)
        {
            if (IsConversationNodeUsed(nodeGUID)) return;

            currentProfile.usedConversationNodes.Add(nodeGUID);
            SetDirty();
        }

        public bool IsConversationFlagSet(string flag)
        {
            flag = flag.ToLowerInvariant();
            return currentProfile.conversationFlags.Contains(flag);
        }

        public bool IsFlagSet(string flag)
        {
            return currentProfile.flags.HasFlag(flag);
        }

        public void SetConversationFlag(string flag, bool set = true)
        {
            if (IsConversationFlagSet(flag) == set) return;

            if (set)
                currentProfile.conversationFlags.Add(flag);
            else
                currentProfile.conversationFlags.Remove(flag);
            SetDirty();
        }


        #endregion

        #region Scenes

        public bool IsChapterEntryComplete(Chapter.Entry entry)
        {
            if (entry.IsHOScene)
                return IsSceneComplete(entry);
            return IsMinigameComplete(entry);
        }

        public bool IsChapterEntryUnlocked(Chapter.Entry entry)
        {
            if (entry.IsHOScene)
                return IsSceneUnlocked(entry);
            return IsMinigameComplete(entry);
        }

        public HOSceneState GetSceneState(Chapter.Entry forEntry)
        {
            return GetSceneStateByGUID(forEntry.saveGUID);
        }

        public HOSceneState GetSceneStateByGUID(SerializableGUID saveGuid)
        {
            HOSceneState state = currentProfile.hoSceneStates.Find(x => x.saveGUID == saveGuid);

            if (state != null)
                return state;

            state = new HOSceneState { saveGUID = saveGuid };

            currentProfile.hoSceneStates.Add(state);
            SetDirty();

            return state;
        }

        public bool IsSceneComplete(Chapter.Entry forEntry)
        {
            HOSceneState state = GetSceneState(forEntry);

            return state.completed;
        }

        public bool IsSceneUnlocked(Chapter.Entry forEntry)
        {
            HOSceneState state = GetSceneState(forEntry);

            return state.unlocked;
        }

        public MinigameState GetMinigameState(Chapter.Entry forEntry)
        {
            return GetMinigameStateByGUID(forEntry.saveGUID);
        }

        public MinigameState GetMinigameStateByGUID(SerializableGUID saveGuid)
        {
            MinigameState state = currentProfile.minigameStates.Find(x => x.saveGUID == saveGuid);

            if (state != null)
                return state;

            state = new MinigameState { saveGUID = saveGuid };

            currentProfile.minigameStates.Add(state);
            SetDirty();

            return state;            
        }

        public bool IsMinigameComplete(Chapter.Entry forEntry)
        {
            MinigameState state = GetMinigameState(forEntry);

            return state.completed;
        }

        public void SetMinigameComplete(Chapter.Entry forEntry)
        {
            MinigameState state = GetMinigameState(forEntry);

            state.completed = true;

            SetDirty();

            CheckChapterCompletion();
        }

        void UnsetSceneEntryCompletionForReplay(Chapter.Entry entry)
        {
            if (entry.IsHOScene)
            {
                HOSceneState state = GetSceneState(entry);
                state.hasSaveState = false;
            } else
            {
                MinigameState state = GetMinigameState(entry);
                state.completed = false;
            }
        }

        void UnsetConversationCompletionForReplay(Boomzap.Conversation.Conversation conversation)
        {
            if (conversation == null) return;

            currentProfile.usedConversations.Remove(conversation.guid);

            var allNodes = conversation.GetAllNodes();
            foreach (var n in allNodes)
                currentProfile.usedConversationNodes.Remove(n.guid);
        }

        public void SetupChapterForReplay(Chapter ch)
        {
            //UnsetConversationCompletionForReplay(ch.finishChapterConversation);
            //UnsetConversationCompletionForReplay(ch.openChapterConversation);
            //UnsetConversationCompletionForReplay(ch.preBossConversation);

            foreach (var e in ch.sceneEntries)
            {
                UnsetConversationCompletionForReplay(e.onStartConversation);
                UnsetConversationCompletionForReplay(e.onEndConversation);

                UnsetSceneEntryCompletionForReplay(e);
            }

            SetDirty();
        }

        public Chapter.Entry GetNextRoom(Chapter.Entry currentEntry)
        {
            for (int i = 0; i < GameController.instance.gameChapters.Count; i++)
            {
                Chapter chapter = GameController.instance.gameChapters[i];

                if (chapter.sceneEntries.Contains(currentEntry))
                {
                    int entryIndex = chapter.sceneEntries.ToList().IndexOf(currentEntry);

                    if(entryIndex < 0)
                    {
                        Debug.LogError("Current entry does not exist");
                        return null;
                    }

                    entryIndex++;

                    //Get next entry index

                    if (entryIndex < chapter.sceneEntries.Length)
                    {
                        return chapter.sceneEntries[entryIndex];
                    }
                    else
                    {
                        //Get Next Chapter
                        i++;

                        if (i < GameController.instance.gameChapters.Count)
                        {
                            Chapter nextChapter = GameController.instance.gameChapters[i];

                            if (nextChapter.isCEContent)
                            {
#if CE_BUILD
                                //Return first scene of next chapter
                                return nextChapter.sceneEntries[0];
#else
                                return null;
#endif
                            }
                            else
                            {
                                return nextChapter.sceneEntries[0];
                            }
                        }
                        else
                            return null;
                    }
                }
                else
                    continue;
            }

            return null;
        }

        public Chapter.Entry GetNextIncomplete()
        {
            for (int i = 0; i < GameController.instance.gameChapters.Count; i++)
            {
                Chapter chapter = GameController.instance.gameChapters[i];

                if (IsChapterAvailable(chapter) == false || IsChapterComplete(chapter)) continue;

                foreach(var entry in chapter.sceneEntries)
                {
                    if(GameController.save.IsChapterEntryUnlocked(entry) == false)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        public void CheckChapterCompletion()
        {
            for (int i = 0; i < GameController.instance.gameChapters.Count; i++)
            {
                Chapter c = GameController.instance.gameChapters[i];

                if (!IsChapterAvailable(c) || IsChapterComplete(c)) continue;

                if (c.sceneEntries.All(x => IsChapterEntryUnlocked(x)))
                {
                    SetChapterComplete(c, true);

                    if (i + 1 < GameController.instance.gameChapters.Count)
                    {
                        if(GameController.instance.gameChapters[i + 1].isCEContent)
                        {
                            currentProfile.flags.SetFlag("story_complete", true);
                        }
                        SetChapterAvailable(GameController.instance.gameChapters[i+1], true);

                        #if SE_BUILD
                        if (GameController.instance.gameChapters[i+1].isCEContent)
                        {
                            currentProfile.flags.SetFlag("freeplay", true);
                            SetDirty();
                        }
                        #endif
                    } else
                    {
                        currentProfile.flags.SetFlag("", true);
                        SetDirty();
                    }
                }
            }
        }

        #endregion

        
    }

}
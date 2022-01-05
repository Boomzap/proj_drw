using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Chapter", menuName = "HO/Chapter", order = 1)]
    public class Chapter : ScriptableObject
    {
        public enum EntryType
        {
            Minigame,
            Scene
        };

        public enum TimeOfDay
        {
            Morning,
            Evening
        }

        [Serializable] 
        public class Entry
        {
            [HideInInspector]
            public SerializableGUID     saveGUID = new SerializableGUID();
            static internal ValueDropdownList<string>    validTypes;

            [BoxGroup("Other Settings", order: 2)]
            public bool forceShowAsMinigameInMap = false;
            [BoxGroup("Entry Settings", order:0)]
            public EntryType            type = EntryType.Scene;

            [BoxGroup("SFX")]
            public AudioClip            music;
            [BoxGroup("SFX")]
            public AudioClip            ambient;

            [ShowIf("IsHOScene"), BoxGroup("Other Settings", order: 2)]
            public Boomzap.Character.CharacterInfo hintCharacter;

            [BoxGroup("Conversation")]
            public Boomzap.Conversation.Conversation onStartConversation;
            [BoxGroup("Conversation")]
            public Boomzap.Conversation.Conversation onEndConversation;

            [ShowIf("IsMinigame"), BoxGroup("Minigame Settings")]
            public MinigameReference    minigame;

            [ShowIf("IsHOScene"), BoxGroup("HO Settings")]
            public HORoomReference      hoRoom;
            [ShowIf("IsHOScene"), BoxGroup("HO Settings")]
            [ValueDropdown("validTypes")]
            public string               hoLogic;
            [ShowIf("IsHOScene"), BoxGroup("HO Settings")]
            public int                  objectCount = 10;   
            [ShowIf("IsHOScene"), BoxGroup("HO Settings")]
            public HODifficulty         itemDifficulty = HODifficulty.Easy;
            [ShowIf("IsHOScene"), CheckList("childHOs")]
            public List<HORoomReference>    unlockableSubHO = new List<HORoomReference>();
            [ShowIf("IsHOScene"), CheckList("childHOs")]
            public List<HORoomReference>    unlockedSubHO = new List<HORoomReference>();
            [ShowIf("IsHOScene"), CheckList("childSpecialObjects"), BoxGroup("Other Settings", order:2)]
            public List<string>         specialItems = new List<string>();

            [BoxGroup("Achievement")]
            public Steam.SteamAchievements.Achievement onFinishAchievement;

            public bool isEntryUnlocked => GameController.save.GetSceneState(this).unlocked;

            public bool IsHOScene { get { return type == EntryType.Scene; } }
            public bool IsMinigame { get { return type == EntryType.Minigame; } }

            #if UNITY_EDITOR
            internal IEnumerable<HORoomReference> childHOs
            {
                get
                {
                    if (hoRoom != null && hoRoom.editorAsset != null)
                    {
                        HORoom r = hoRoom.editorAsset.GetComponent<HORoom>();
                        return r.subHO;
                    }
                    return null;
                }
            }

            internal IEnumerable<string> childSpecialObjects
            {
                get
                {
                    if (hoRoom != null && hoRoom.editorAsset != null)
                    {
                        HORoom r = hoRoom.editorAsset.GetComponent<HORoom>();
                        return r.gameObject.GetComponentsInChildren<HOFindableObject>(true).Where(x => x.isSpecialStoryItem).Select(x => x.name).ToList();
                    }

                    return null;
                }
            }
            #endif

            public Entry()
            {
                #if UNITY_EDITOR
                if (validTypes == null)
                {
//                     validTypes = .ToArray();
                    validTypes = new ValueDropdownList<string>();

                    foreach (var tn in typeof(HOLogic).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(HOLogic))))
                    {
                        object[] fna = tn.GetCustomAttributes(typeof(FriendlyNameAttribute), false);
                        if (fna.Length > 0)
                            validTypes.Add(new ValueDropdownItem<string>((fna[0] as FriendlyNameAttribute).friendlyName, tn.Name));
                        else
                            validTypes.Add(tn.Name);
                    }
                                       
                }

                hoLogic = validTypes[0].Value;
                #endif
            }
        }
        
               
                

        public string chapterDisplayName => LocalizationUtil.FindLocalizationEntry($"UI/Chapter/{name}/header", string.Empty, false, TableCategory.UI);

        public string chapterInfoHeader => LocalizationUtil.FindLocalizationEntry($"UI/{name}/chapterHeader", string.Empty, false, TableCategory.UI);
        public string chapterInfoText => LocalizationUtil.FindLocalizationEntry($"UI/{name}/chapterBlurb", string.Empty, false, TableCategory.UI);

        [Header("Chapter Settings")]
        public Sprite           chapterImage;
        public bool             isCEContent = false;
        public bool             isSurveyContent = false;

        public TimeOfDay        timeOfDay;

        //public Boomzap.Conversation.Conversation    openChapterConversation;
        //public Boomzap.Conversation.Conversation    preBossConversation;
        //public Boomzap.Conversation.Conversation    finishChapterConversation;

        [PropertySpace, ListDrawerSettings(ShowIndexLabels = true)]
        public Entry[]  sceneEntries = new Entry[0];

        [HideInInspector]
        public SerializableGUID saveGUID = new SerializableGUID();

        public bool IsEntryUnlocked(Entry matchingEntry)
        {
            int entryIndex = sceneEntries.ToList().IndexOf(matchingEntry);

            //Unlocks first chapter entry
            if (entryIndex == 0)
            {
                //var state = GameController.save.GetSceneState(sceneEntries[0]);
                //state.unlocked = true;
                //Savegame.SetDirty();
                return true;
            }


            if (entryIndex > 0)
            {
                Chapter.Entry previousEntry = sceneEntries[entryIndex - 1];
                //Debug.Log($"Entry Index {entryIndex} for {matchingEntry} ");
                if(previousEntry.IsHOScene)
                {
                   var state = GameController.save.GetSceneState(previousEntry);
                   return state.unlocked;
                }
                   
                if(previousEntry.IsMinigame)
                {
                    var state = GameController.save.GetMinigameState(previousEntry);
                    return state.completed;
                }
            }

            return false;
        }
        public Entry FindMatchingBootEntry(HORoomReference[] rooms, MinigameReference[] mgs, bool boss)
        {
            foreach (var entry in sceneEntries)
            {
                if (entry.IsMinigame && mgs.Contains(entry.minigame)) return entry;
                if (entry.IsHOScene && rooms.Contains(entry.hoRoom)) return entry;
            }

            return null;
        }
    }
}

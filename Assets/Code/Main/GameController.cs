using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System;
using UnityEngine.Events;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

using UnityEngine.Localization.Settings;

namespace ho
{
    public partial class GameController : SimpleSingleton<GameController>
    {
        public Camera                           currentCamera;
        public Camera                           defaultCamera;

        public List<Chapter>                    gameChapters;
        public Chapter                          currentChapter { get; set; } = null;

        Vector3                                 cameraOffset = new Vector3(0f, 0f, -10f);

        Savegame                                _save = new Savegame();


        Chapter.Entry                           replayEntry = null;

	    public static  Savegame					save 
        { 
            get { return instance._save; } 
        }

	    public static  SystemSave				systemSave 
        { 
            get { return SystemSaveContainer.instance.systemSave; } 
        }

        // basically, we don't want to save conversations until they're complete,
        //  and similarly with HO scenes, we don't want to be spam saving until we're done.
        public bool                             isSafeToSave
        {
            get { return !inConversation; }
        }

        public bool                             inGameplay
        {
            get { return HOGameController.instance.gameObject.activeInHierarchy || MinigameController.instance.ActiveMinigame != null; }
        }

        public bool                             canChangeResolution => !inGameplay;

        public bool                             isUnlimitedMode;

        public bool                             isReplayingRoom = false;
        public bool                             inConversation => Boomzap.Conversation.ConversationManager.instance.InConversation;

        public bool                             isFirstHOSceneUnlocked => GameController.save.IsChapterEntryUnlocked(GameController.instance.gameChapters[0].sceneEntries.First(x => x.IsHOScene));

        bool                                    didEnterGame = false;

        [SerializeField]                        Transform chapterScenePos;
        [SerializeField]                        Transform menuScenePos;
        [SerializeField]                        Transform mapScenePos;
        [SerializeField]                        AudioClip mainMenuMusic;
        GameObject                              currentWorldStateObject = null;
        IWorldState                             currentWorldState = null;

        public Vector3 MenuScenePosRef { get{ return menuScenePos.position; }}

        public IWorldState                      CurrentWorldState => currentWorldState;

        UnityAction<bool>                       onConversationEndedCallback;

        public bool storyModeOpened = false;

        private void Awake()
        {
#if UNITY_EDITOR
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
#elif UNITY_STANDALONE
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
#endif
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);

            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

#if SURVEY_BUILD
            var surveyPath = Path.Combine(Application.persistentDataPath, "survey");
            if (!Directory.Exists(surveyPath))
            {
                Directory.CreateDirectory(surveyPath);
            }
#endif

            StartCoroutine(ProfileTimeUpCor());
        }
        IEnumerator ProfileTimeUpCor()
        {
            do
            {
                if (save.currentProfile != null)
                {
                    save.currentProfile.timePlayedSeconds++;

                    // not important that this is saved perfectly, let it be written wtih other changes
                    //Savegame.SetDirty();
                }

                yield return new WaitForSecondsRealtime(1f);
            } while (true);
        }

        private void Start()
        {
            //I2.Loc.LocalizationManager.InitializeIfNeeded();

            SetActiveCamera(Camera.main);

            HOGameController.instance.gameObject.SetActive(false);
            MinigameController.instance.gameObject.SetActive(false);

            defaultCamera = Camera.main;

            // this should already be done by our StateCache in the splash screen, but when we're debugging
            // it's not likely we're gonna bother going through that, so..
            StateCache.instance.PreloadAll(OnAssetsReady);

            Boomzap.Conversation.ConversationManager.instance.OnMarkConversationUsed += SetConversationTriggered;
            Boomzap.Conversation.ConversationManager.instance.OnMarkNodeUsed += SetConversationNodeTriggered;
            Boomzap.Conversation.ConversationManager.instance.OnConversationEnded += OnConversationEnded;
            Boomzap.Conversation.ConversationManager.instance.OnSetFlag += SetConversationFlag;
        }

        void SetConversationTriggered(SerializableGUID conversationGuid)
        {
            save.SetConversationTriggered(conversationGuid);
        }

        void SetConversationNodeTriggered(SerializableGUID nodeGuid)
        {
            save.SetConversationNodeUsed(nodeGuid);
        }

        void SetConversationFlag(string flag)
        {
            save.SetConversationFlag(flag);
        }

        //         // Start is called before the first frame update
        //         void Start()
        //         {
        //             currentCamera.gameObject.SetActive(true);
        // 
        //             //FadeToHOGame(gameChapters[0].sceneEntries[0]);
        //         }

        void OnAssetsReady()
        {
            if (!Load(Savegame.GetPath("game.sav")))
            {
                InitNewSave();
            }

            didEnterGame = true;

            FadeToGameMenu();
        }

        // Update is called once per frame
        void Update()
        {
            if (didEnterGame && save.isDirty && isSafeToSave)
            {
                Save(Savegame.GetPath("game.sav"));
            }

            if (systemSave.isDirty) 
		    {
			    SystemSave.Save(systemSave, Savegame.GetPath("system.sav"));
			    systemSave.isDirty = false;
		    }
        }

#region Save/Load
        
        public void ClearSave()
        {
            InitNewSave();
            UIController.instance.mainMenuUI.Hide(true);
            Popup.ShowPopup<EnterNamePopup>();
        }

        private void InitNewSave()
        {
            _save = new Savegame();

            save.Init();
            save.isDirty = true;
        }

        private bool Save(string toPath)
        {
            if (save == null) return false;

            save.isDirty = false;

            using (FileStream file = File.Create(toPath))
            {
                return save.SerializeBinary(file);
            }
        }

        private bool Load(string fromPath)
        {
            bool loadedOK = false;

            try
            {
                using (FileStream file = File.Open(fromPath, FileMode.Open))
                {
                    Savegame loadedState = Savegame.LoadBinary(file);
                    if (loadedState != null)
                    {
                        _save = loadedState;
                        loadedOK = true;
                    }
                    
                }
            } catch (Exception e)
            {
                Debug.Log("Exception " + e.ToString() + " loading core save. Resetting");                        
            }

            return loadedOK;
        }
#endregion

        public void SetActiveCamera(Camera camera)
        {
            if (currentCamera == camera) return;

            currentCamera.gameObject.SetActive(false);
            camera.gameObject.SetActive(true);

            currentCamera = camera;
        }

        public void FadeToChapterMenu(bool isStorymode = true, UnityEngine.Events.UnityAction onFadeOutComplete = null)
        {
            if (InWorldState<ChapterScreen>()) return;

            storyModeOpened = isStorymode;

            BGManager.instance.UnloadBGs();

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                StateCache.instance.PreloadChapterScreen(() =>     
                {
                    onFadeOutComplete?.Invoke();

                    currentWorldStateObject = Instantiate(StateCache.instance.ChapterScreen, chapterScenePos);
                    currentWorldState = currentWorldStateObject.GetComponent<ChapterScreen>();
                    currentCamera.transform.position = chapterScenePos.position + cameraOffset;

                    UIController.instance.HideAll(true);
                    UIController.instance.chapterUI.SetupBeforeShow();

                    UIController.instance.FadeIn(() =>
                    {
                        UIController.instance.chapterUI.Show();
                    });
                });
            });
        }

        public void FadeToMinigame(Chapter.Entry entry)
        {
            //Disable Click Events
            UIController.instance.eventSystem.gameObject.SetActive(false);

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                BGManager.instance.UnloadBGs(() =>
                {
                    BGManager.instance.PreloadBackgrounds(entry, false);
                    MinigameController.instance.gameObject.SetActive(true);

                    MinigameController.instance.LoadMinigame(entry, () =>
                    {
                        UIController.instance.HideAll(true);

                        currentCamera.transform.position = MinigameController.instance.transform.position + cameraOffset;
                        currentWorldState = MinigameController.instance;
                        currentWorldStateObject = MinigameController.instance.gameObject;
                        MinigameController.instance.StartCurrentMinigame();

                        UIController.instance.FadeIn(() =>
                        {

                        });
                    });
                });
            });            
        }

        public void FadeToHOGame(Chapter.Entry entry)
        {
            //if (InWorldState<HOGameController>()) return;

            //Disable Click Events
            UIController.instance.isUIInputDisabled = true;
           

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                HORoomAssetManager.instance.UnloadRoom(entry.hoRoom);

                BGManager.instance.UnloadBGs(() =>
                {
                    BGManager.instance.PreloadBackgrounds(entry, true);

                    HORoomAssetManager.instance.LoadRoomAsync(entry.hoRoom, (GameObject go) =>
                    {
                        if (go)
                        {
                            UIController.instance.HideAll(true);

                            HOGameController.instance.SetRoom(entry.hoRoom, entry);
                            HOGameController.instance.OnBeginFadeTo();
                            currentWorldState = HOGameController.instance;
                            currentWorldStateObject = HOGameController.instance.gameObject;

                            //Check if Start Conversation is already Triggered
                            if (entry.onStartConversation == null || entry.onStartConversation != null && (save.IsConversationTriggered(entry.onStartConversation.guid) && !entry.onStartConversation.repeatable))
                                HOGameController.instance.EnableRoomObjects(true);
                            else
                                HOGameController.instance.EnableRoomObjects(false);


                            UIController.instance.FadeIn(() =>
                            {
                                HOGameController.instance.OnFadeToComplete();
                            });
                        }
                        else
                        {
                            Debug.LogError($"Room load was not valid - guid: {entry.hoRoom.AssetGUID}");

                            FadeToGameMenu();
                        }
                    });
                });
            });
        }

        public void FadeToGameMenu(UnityEngine.Events.UnityAction onFadeOutComplete = null)
        {
            if(UIController.instance.eventSystem.gameObject.activeInHierarchy == false)
            {
                //Enable Click Events here in case it is inactive
                UIController.instance.eventSystem.gameObject.SetActive(true);
            }

            if (InWorldState<MainMenuWorld>()) return;

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();
                BGManager.instance.UnloadBGs();

                StateCache.instance.PreloadMainMenu(() =>     
                {
                    onFadeOutComplete?.Invoke();

                    currentWorldStateObject = Instantiate(StateCache.instance.MainMenu, menuScenePos);
                    currentWorldState = currentWorldStateObject.GetComponent<MainMenuWorld>();
                    currentCamera.transform.position = menuScenePos.position + cameraOffset;

                    UIController.instance.HideAll(true);

                    UIController.instance.FadeIn(() =>
                    {
                        //NOTE* Experiencing Null Exception when placed on Awake
                        if (GameController.save.currentProfile != null)
                        {
                            UIController.instance.mainMenuUI.Show();
                        }
                        else
                        {
                            Popup.ShowPopup<EnterNamePopup>();
                        }
                    });
                });
            });
        }

        public void FadeToUnlimitedMenu(UnityEngine.Events.UnityAction onFadeOutComplete = null)
        {
            if (UIController.instance.eventSystem.gameObject.activeInHierarchy == false)
            {
                //Enable Click Events here in case it is inactive
                UIController.instance.eventSystem.gameObject.SetActive(true);
            }

            if (InWorldState<MainMenuWorld>()) return;

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                StateCache.instance.PreloadMainMenu(() =>
                {
                    onFadeOutComplete?.Invoke();

                    currentWorldStateObject = Instantiate(StateCache.instance.MainMenu, menuScenePos);
                    currentWorldState = currentWorldStateObject.GetComponent<MainMenuWorld>();
                    currentCamera.transform.position = menuScenePos.position + cameraOffset;

                    UIController.instance.HideAll(true);

                    UIController.instance.FadeIn(() =>
                    {
                        UnlimitedRoomPopup popup = Popup.GetPopup<UnlimitedRoomPopup>();
                        //popup.Setup(popup.currentEntry);
                        popup.Show();

                    });
                });
            });
        }


        public void FadeToJournalMenu(UnityEngine.Events.UnityAction onFadeOutComplete = null)
        {
            if (UIController.instance.eventSystem.gameObject.activeInHierarchy == false)
            {
                //Enable Click Events here in case it is inactive
                UIController.instance.eventSystem.gameObject.SetActive(true);
            }

            if (InWorldState<MainMenuWorld>()) return;

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                StateCache.instance.PreloadMainMenu(() =>
                {
                    onFadeOutComplete?.Invoke();

                    currentWorldStateObject = Instantiate(StateCache.instance.MainMenu, menuScenePos);
                    currentWorldState = currentWorldStateObject.GetComponent<MainMenuWorld>();
                    currentCamera.transform.position = menuScenePos.position + cameraOffset;

                    UIController.instance.HideAll(true);

                    UIController.instance.FadeIn(() =>
                    {
                        UIController.instance.journalUI.Show();
                    });
                });
            });
        }




        public void FadeToCredits(UnityEngine.Events.UnityAction onFadeOutComplete = null)
        {
            if (InWorldState<MainMenuWorld>() && UIController.instance.mainMenuUI.gameObject.activeInHierarchy)
            {
                UIController.instance.mainMenuUI.Hide();
                Popup.HidePopup<OptionsPopup>();
                UIController.instance.creditsUI.Show();
                return;
            }

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                StateCache.instance.PreloadMainMenu(() =>
                {
                    onFadeOutComplete?.Invoke();

                    currentWorldStateObject = Instantiate(StateCache.instance.MainMenu, menuScenePos);
                    currentWorldState = currentWorldStateObject.GetComponent<MainMenuWorld>();
                    currentCamera.transform.position = menuScenePos.position + cameraOffset;

                    UIController.instance.HideAll(true);

                    UIController.instance.FadeIn(() =>
                    {
                        UIController.instance.creditsUI.Show();
                    });
                });
            });
        }


        //public void FadeToMap(UnityEngine.Events.UnityAction onFadeOutComplete = null)
        //{
        //    if (InWorldState<MapController>()) return;

        //    //Disable Click Events
        //    UIController.instance.eventSystem.gameObject.SetActive(false);

        //    UIController.instance.FadeOut(() =>
        //    {
        //        WorldStateCleanup();

        //        StateCache.instance.PreloadMapScreen(() =>     
        //        {
        //            onFadeOutComplete?.Invoke();

        //            currentWorldStateObject = Instantiate(StateCache.instance.MapScreen, mapScenePos);
        //            currentWorldState = currentWorldStateObject.GetComponent<MapController>();
        //            currentCamera.transform.position = mapScenePos.position + cameraOffset;

        //            UIController.instance.HideAll(true);

        //            UIController.instance.FadeIn(() =>
        //            {
        //                //UIController.instance.mapUI.Show();
        //            });
        //        });
        //    });

           
        //}

        void OnConversationEnded()
        {
            UIController.instance.FadeOut(() =>
               {
                   BGManager.instance.DisableAllBackground();
                   HOGameController.instance.EnableRoomObjects(true);
                   UIController.instance.FadeIn(() =>
                   {
                       onConversationEndedCallback?.Invoke(true);
                       onConversationEndedCallback = null;
                   });
               });
        }

        public void PlayConversation(Boomzap.Conversation.Conversation conversation, UnityAction<bool> andThen = null, bool forcePlay = false)
        {
            if (conversation == null)
            {
                Debug.LogWarning("Conversation empty");
                return;
            }
            
            if (Boomzap.Conversation.ConversationManager.instance.InConversation)
            {
                Debug.Log($"Tried to start conversation {conversation.name} while already playing one.");
                andThen?.Invoke(false);
                return;
            }

            if ((save.IsConversationTriggered(conversation.guid) && !conversation.repeatable) && !forcePlay)
            {
                Debug.Log($"Conversation {conversation.name} with guid {conversation.guid} has already been played!");
                andThen?.Invoke(false);
                return;
            }

            Boomzap.Conversation.ConversationManager.instance.StartConversation(conversation, EvaluateConversationNode);
            onConversationEndedCallback = andThen;
        }

        bool EvaluateSubCond(string cond, out string repl)
        {
            Debug.Log($"Condition to Evaluate: {cond}");
         
            cond = cond.Replace("&&", "&").Replace("||", "|");

            int openC = cond.Count(x => x == '(');
            int closeC = cond.Count(x => x == ')');


            if (openC != closeC)
            {
                Debug.LogError($"Condition: {cond} - open and close brackets don't match");

                repl = "";
                return true;
            }
// 
//             bool rev = true;

            int outer = 0;
            do
            {
                outer = cond.IndexOf('(');

                if (outer >= 0)
                {
                    int outerEnd = outer+1;
                    int depth = 1;
                    do
                    {
                        if (cond[outerEnd] == ')') depth--;
                        else if (cond[outerEnd] == '(') depth++;
                        outerEnd++;
                    } while (depth > 0);

                    string sub = cond.Substring(outer+1, outerEnd-outer-2);
                    string newSub;
                    EvaluateSubCond(sub, out newSub);

                    cond = cond.Replace(cond.Substring(outer, outerEnd - outer), newSub);

                    Debug.Log($"After 2nd Evaluation: {cond}");
                }
            } while (outer >= 0);

            repl = "";
            string tok = "";
            bool inverse = false;
            bool running = true;
            bool fvar = true;
            char prevOp = '-';

            foreach(var c in cond)
            {
                if (c == '&' || c == '|')
                {
                    bool tval;

                    if (tok == "@" || tok == "~")
                        tval = tok == "@";
                    else
                    {
                        tval = save.IsConversationFlagSet(tok);
                        if (inverse) tval = !tval;
                    }

                    inverse = false;

                    if (fvar)
                    {
                        running = tval;
                        fvar = false;
                    }
                    else
                    {
                        if (prevOp == '&')
                            running &= tval;
                        else
                            running |= tval;
                    }

                    prevOp = c;
                    tok = "";

                } else if (c == '!')
                {
                    inverse = true;
                } else if (c == ' ')
                {
                    continue;
                } else
                {
                    tok += c;
                }
            }

            Debug.Log($"Condition: {tok}");

            bool tvale;

            if (tok == "@" || tok == "~")
                tvale = tok == "@";
            else
            {
                tvale = save.IsConversationFlagSet(tok);
                if (inverse) tvale = !tvale;
            }

            Debug.Log($"Conversation Flag Set: {tvale}");

            inverse = false;

            if (fvar || prevOp == '-')
            {
                running = tvale;
                fvar = false;
            }
            else 
            {
                if (prevOp == '&')
                    running &= tvale;
                else
                    running |= tvale;
            }

            repl = running ? "@" : "~";

            return running;
        }

        public bool EvaluateConversationNode(Boomzap.Conversation.ConversationNode node)
        {
            //Checks if Profile has played this conversation and is not repeatable
            if (save.IsConversationNodeUsed(node.guid) && !node.repeatable)
                return false;

            //Flag Condition to play the conversation
            string cond = node.flagCondition.Trim();


            //If there's no condition just play it.
            if (string.IsNullOrEmpty(cond)) return true;


            //Evaluate condition
            return EvaluateSubCond(cond, out string _);
        }

        public void LaunchBootEntry(Chapter.Entry bootEntry)
        {
            //if (InWorldState<HOGameController>() || InWorldState<MinigameController>()) return;

            if (replayEntry == null)
            {
                replayEntry = bootEntry;
            }

            if (bootEntry.IsHOScene)
            {
                Debug.Log("Loading Ho Scene...");
                FadeToHOGame(bootEntry);
            } else
            {
                Debug.Log("Loading MG Scene...");
                FadeToMinigame(bootEntry);
            }
        }

        bool InWorldState<T>() where T : IWorldState
        {
            if (currentWorldStateObject == null) return false;
            return (currentWorldStateObject.GetComponent<T>() != null);
        }

        void WorldStateCleanup()
        {
            if (currentWorldState != null)
            {
                currentWorldState.OnLeave();
                if (currentWorldState.ShouldDestroyOnLeave())
                {
                    Destroy(currentWorldStateObject);
                }
            }

            currentWorldState = null;
        }

        public T GetWorldState<T>() where T : IWorldState
        {
            if (currentWorldStateObject == null) return default(T);
            return (currentWorldStateObject.GetComponent<T>());            
        }

        public void LoadReplayEntry()
        {
            isReplayingRoom = true;
            LaunchBootEntry(replayEntry);
        }

        public void ClearReplayEntry()
        {
            replayEntry = null;
        }

        public void PlayNextRoom(Chapter.Entry lastPlayedEntry)
        {
            Chapter.Entry continueRoom = GameController.save.GetNextRoom(lastPlayedEntry);
            if (continueRoom == null)
            {
                if (GameController.save.IsFlagSet("credits_played") == false)
                {
                    GameController.save.currentProfile.flags.SetFlag("credits_played", true);
                    Savegame.SetDirty();
                    GameController.instance.FadeToCredits();
                }
                else
                {
#if CE_BUILD
                    GameController.instance.FadeToChapterMenu(true);
#else
                    GameController.instance.FadeToChapterMenu();
#endif
                }

                return;
            }

            GameController.instance.LaunchBootEntry(continueRoom);
        }

#if UNITY_EDITOR
        [Button]
        public void GenerateChapterLocEntries()
        {
            gameChapters.ForEach(x =>
            {
                LocalizationUtil.FindLocalizationEntry($"UI/Chapter/{x.name}/header", "", true, TableCategory.UI);
                //LocalizationUtil.FindLocalizationEntry($"{x.name}/chapterHeader", "", true, TableCategory.UI);
                //LocalizationUtil.FindLocalizationEntry($"{x.name}/chapterBlurb", "", true, TableCategory.UI);
            }
          );
        }

        [Button]
        public void GenerateSceneDescriptionEntries()
        {
           gameChapters.ForEach(x =>
           {
               for(int i = 0; i < x.sceneEntries.Length; i++)
               {
                   LocalizationUtil.FindLocalizationEntry($"UI/Chapter/{x.name}/scene_{i}_desc", string.Empty, false, TableCategory.UI);
               }
           });
        }

        public void AddAddressableTagByGUID(string tag, string guid, string assetField, string chapterName)
        {
            AddressableAssetEntry entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);

            if (entry == null)
            {
                Debug.LogWarning($"Chapter {chapterName} missing a valid addressable asset for {assetField}");
            }

            entry.SetLabel(tag, true, true);
        }
#endif
    }
}
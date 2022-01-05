using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
#endif

namespace ho
{
    public class MinigameController : SimpleSingleton<MinigameController>, IWorldState
    {
        [SerializeField, HideInInspector]   public MinigameReference[]      minigamePrefabs;
        [SerializeField, ReadOnly]          public string[]          minigamePrefabNames;

        [SerializeField] RectTransform      safeZone;


        [Header("Used generically")]
        [SerializeField] Material           defaultMaterial;
        public Material                     DefaultMaterial { get { return defaultMaterial; } }
        public AudioClip                    defaultClickAudio;
        public float                        skipTimer = 30f;
        public Material                     InactiveObjectMaterial;
        public float                        InactiveDesatFactor = 0.3f;
        public float                        InactiveBrightenFactor = -0.3f;

        [Header("Jigsaw")]
        [SerializeField] Material jigsawSelectedMaterial;
        public Material JigsawSelectedMaterial { get { return jigsawSelectedMaterial; } }
        public Material jigsawPieceBorderSDFMaterial;

        [Header("FTD")]
        public Material FTDSDFGlowMaterial;
        public Material FTDSDFCollectibleMaterial;
        public AudioClip FTDFoundAudio;
        public GameObject FTDSeparatorPrefab;

        [Header("Silhouette")]
        public Material silhouetteMaterial;

        [Header("Sorting")]
        public Material SortingSelectedObjectOutlineMaterial;
        public float SortingHintDuration = 5f;

        [Header("Memory")]
        public Material MemoryMouseoverMaterial;

        [Header("Audio")]
        public RandomAudio onPieceSelected;
        public RandomAudio onPieceSwap;
        public RandomAudio onPieceRotate;
        public RandomAudio onPieceCorrect;
        public AudioClip   minigameCompleteClip;

        float skipTimerReady = 0f;

        MinigameUI minigameUI => UIController.instance.minigameUI;

        MinigameBase activeMinigame;
        public MinigameBase ActiveMinigame { get { return activeMinigame; } }
        public T ActiveMinigameAsType<T>() where T : MinigameBase
        {
            return activeMinigame as T;
        }

        public bool isHintPlaying = false;

        AsyncOperationHandle<GameObject> minigameLoadOperationHandle;
        bool isLoadingMinigame = false;

        UnityEngine.Events.UnityAction onMinigameLoadedOneShot;

        [NonSerialized]
        public bool returnToMainMenu = false;

        public bool returnToJournalUI = false;

        MinigameReference currentMinigameRef;
        bool wasComplete = false;

        [HideInInspector]
        Chapter.Entry   launchedBootEntry;
        public Chapter.Entry ChapterEntry => launchedBootEntry;

        //For Achievement
        public bool isHintUsedOnce = false;

        public bool ShouldDestroyOnLeave()
        {
            return false;
        }

        public void OnLeave()
        {
            Cleanup();
            gameObject.SetActive(false);
        }

        public MinigameReference GetMinigameReference(string name)
        {
            for (int i = 0; i < minigamePrefabs.Length; i++)
            {
                if (name.Equals(minigamePrefabNames[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    return minigamePrefabs[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            RefreshEditorMinigameList();   
        }



        [Button]
        public void RefreshEditorMinigameList()
        {
            List<MinigameReference> output = new List<MinigameReference>();
            List<string> outputNames = new List<string>();

            List<AddressableAssetEntry> assets = new List<AddressableAssetEntry>();

            if (AddressableAssetSettingsDefaultObject.Settings == null)
                return;


            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(assets, false, null, (AddressableAssetEntry e) =>
            {
                return e.labels.Contains("Minigame");
            });

          

            foreach (var t in assets)
            {
                output.Add(new MinigameReference(t.guid));
                outputNames.Add(t.address);
            }

            minigamePrefabs = output.ToArray();
            minigamePrefabNames = outputNames.ToArray();
        }
#endif

        // Update is called once per frame
        void Update()
        {
            if (activeMinigame && activeMinigame.IsComplete() && !wasComplete)
            {
                OnComplete();
            }

            if (activeMinigame && !activeMinigame.disableInput)
            {
                UpdateSkipTimer();
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR || ENABLE_CHEATS
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    OnComplete();
            //}
            if (Input.GetKeyDown(KeyCode.F12))
            {
                minigameUI.OnHintReady();
                skipTimerReady = 0f;
            }
#endif
        }

        public void OnSkip()
        {
            skipTimerReady = 9999f;
            activeMinigame.Skip();
        }

        void UpdateSkipTimer()
        {
            skipTimerReady -= Time.deltaTime;
            float a = Mathf.Clamp01(1f - (skipTimerReady / skipTimer));
            minigameUI.UpdateSkip(a);
        }

        void Cleanup()
        {
            if (minigameLoadOperationHandle.IsValid())
                Addressables.Release(minigameLoadOperationHandle);

		    if (activeMinigame)
		    {
			    Destroy(activeMinigame.gameObject);
                activeMinigame = null;
		    }	            
        }

        void PostConversation()
        {
            //BGManager.instance.UnloadBGs();
            CheckAchievements();
        }

        void CheckAchievements()
        {
            if (isHintUsedOnce == false)
            {
                int noHintClear = GameController.save.currentProfile.intContainers.IncreaseValueByName("mg_no_hint_clear");

                Steam.SteamAchievements.SetAchievementStat(Steam.SteamAchievements.Stat.ACH_STAT_SUBGAME_NO_HINT, noHintClear);
            }

            if(launchedBootEntry != null && launchedBootEntry.onFinishAchievement != Steam.SteamAchievements.Achievement.NONE)
            {
                Steam.SteamAchievements.SetAchievement(launchedBootEntry.onFinishAchievement);
            }

            if (Steam.SteamAchievements.achievementsToLoadCount > 0)
            {
                AchievementPopup popup = Popup.GetPopup<AchievementPopup>();
                popup.SetUpAchievements();
                popup.onHiddenOneshot += () => ExitMinigame();
                popup.Show();
            }
            else
                ExitMinigame();
        }

        void ExitMinigame()
        {

            if (returnToMainMenu)
            {
                GameController.instance.FadeToGameMenu();
                returnToMainMenu = false;
            }
            else if(returnToJournalUI)
            {
                GameController.instance.FadeToJournalMenu();
                returnToJournalUI = false;
            }
            else
            {
                GameController.instance.PlayNextRoom(launchedBootEntry);
            }
        }

	    void OnPostCompletionAnimation()
	    {
            //Cleanup();
            if (launchedBootEntry.onEndConversation != null)
            {
                GameController.instance.PlayConversation(launchedBootEntry.onEndConversation, (bool _) => PostConversation());
            }
            else
            {
                Debug.Log($"There is no end conversation for {launchedBootEntry.minigame.roomNameKey}");
                PostConversation();
            }

	    }

        public void OnComplete()
        {
            if (wasComplete) return;


            wasComplete = true;

            if (activeMinigame)
            {
                if(GameController.save.IsMinigameComplete(launchedBootEntry) == false && returnToMainMenu == false)
                {
                    GameController.save.SetMinigameComplete(launchedBootEntry);
                    minigameUI.Hide();

                    if (minigameCompleteClip != null)
                        Audio.instance.PlaySound(minigameCompleteClip);

                    NotificationPopup popup = Popup.GetPopup<NotificationPopup>();

                    string header = LocalizationUtil.FindLocalizationEntry("LevelUnlocked", string.Empty, false, TableCategory.UI);
                    string message = LocalizationUtil.FindLocalizationEntry("MinigameUnlocked", string.Empty, false, TableCategory.UI);

                    string mgRoomName = HORoomAssetManager.instance.mgTracker.GetMGRoomName(launchedBootEntry.minigame);
                    string roomName = LocalizationUtil.FindLocalizationEntry($"Minigame/{mgRoomName.ToLower()}_title", string.Empty, false, TableCategory.UI);


                    string colorPrefix = "<color=yellow>";
                    string colorSuffix = "</color>";

                    roomName = string.IsNullOrEmpty(roomName) ? "this minigame" : roomName;

                    roomName = colorPrefix + roomName + colorSuffix;

                    message = string.Format(message, roomName);

                    popup.SetupPopup(header, message);

                    activeMinigame.PlaySuccess(() => {

                        if (MinigameController.instance.returnToMainMenu || MinigameController.instance.returnToJournalUI || GameController.save.IsFlagSet("minigame_complete"))
                        {
                            OnPostCompletionAnimation();
                        }
                        else
                        {
                            GameController.save.currentProfile.flags.SetFlag("minigame_complete", true);
                            Savegame.SetDirty();
                            popup.onHiddenOneshot += () => OnPostCompletionAnimation();
                            popup.Show();
                        }
                    });
                }
                else
                {
                    GameController.save.SetMinigameComplete(launchedBootEntry);
                    minigameUI.Hide();

                    if (minigameCompleteClip != null)
                        Audio.instance.PlaySound(minigameCompleteClip);
                    activeMinigame.PlaySuccess(() => OnPostCompletionAnimation());
                }
            }
        }

        void TryLoadMinigame(string minigameName)
        {
            minigameLoadOperationHandle = Addressables.LoadAssetAsync<GameObject>(minigameName);

            minigameLoadOperationHandle.Completed += (AsyncOperationHandle<GameObject> handle) =>
            {
                isLoadingMinigame = false;

                if (handle.Status == AsyncOperationStatus.Succeeded &&
                    handle.Result != null)
                {
                    Debug.Log($"Successfully loaded minigame: {minigameName}");
                    OnMinigameLoadedSuccess(handle.Result);
                } else
                {
                    if (handle.OperationException != null)
                        Debug.Log($"Failed to load minigame: {minigameName} - {handle.OperationException.Message}");
                    else
                        Debug.Log($"Failed to load minigame: {minigameName} - no exception attached");

                    OnMinigameLoadedFailure();
                }
            };
        }

        void TryLoadMinigame(MinigameReference mgRef)
        {
            minigameLoadOperationHandle = Addressables.LoadAssetAsync<GameObject>(mgRef);

            minigameLoadOperationHandle.Completed += (AsyncOperationHandle<GameObject> handle) =>
            {
                isLoadingMinigame = false;

                if (handle.Status == AsyncOperationStatus.Succeeded &&
                    handle.Result != null)
                {
                    Debug.Log($"Successfully loaded minigame: {mgRef.AssetGUID}");
                    OnMinigameLoadedSuccess(handle.Result);
                } else
                {
                    if (handle.OperationException != null)
                        Debug.Log($"Failed to load minigame: {mgRef.AssetGUID} - {handle.OperationException.Message}");
                    else
                        Debug.Log($"Failed to load minigame: {mgRef.AssetGUID} - no exception attached");

                    OnMinigameLoadedFailure();
                }
            };
        }

        void OnMinigameLoadedFailure()
        {
            Cleanup();

            GameController.instance.FadeToGameMenu();
        }

        void OnMinigameLoadedSuccess(GameObject go)
        {
            var objInstance = Instantiate<GameObject>(go, transform);

            if (objInstance == null)
            {
                Debug.Log("Failed to instantiate minigame");
                Cleanup();

                return;
            }

            activeMinigame = objInstance.GetComponent<MinigameBase>();

            if (activeMinigame == null)
            {
                Debug.Log("Failed to get MinigameBase component from instantiated GameObject");
                Cleanup();

                return;
            }

 		    //World.instance.SetLocation("Minigames", null);
 		    activeMinigame.transform.localPosition = Vector3.zero;
		    activeMinigame.transform.localScale = Vector3.one;


            onMinigameLoadedOneShot?.Invoke();
            onMinigameLoadedOneShot = null;

		    //GameUI.instance.frontendUI.Show(false);

        }

        void ShowUI()
        {
            minigameUI.gameObject.PlayAnimation(this, "mg_in", () => { activeMinigame.disableInput = false; });
        }

        public void StartCurrentMinigame()
        {
            if (launchedBootEntry != null && launchedBootEntry.music != null)
            {
                Audio.instance.PlayMusic(launchedBootEntry.music);
            }

            if (launchedBootEntry != null && launchedBootEntry.ambient != null)
            {
                Audio.instance.PlayAmbient(launchedBootEntry.ambient);
            }

            Steam.SteamAchievements.ClearAchievementsList();
            isHintUsedOnce = false;
            isHintPlaying = false;
            skipTimerReady = skipTimer;

            UIController.instance.minigameUI.Show();
            UIController.instance.minigameUI.Initialize(activeMinigame);

            RectTransform uiSafeZone = UIController.instance.minigameUI.SafeZone;

            wasComplete = false;
            activeMinigame.SetupSafeZone(uiSafeZone.rect);
            activeMinigame.disableInput = true;
            activeMinigame.OnStart();

            if (launchedBootEntry.onStartConversation != null)
            {
                GameController.instance.PlayConversation(launchedBootEntry.onStartConversation, (bool _) => ShowUI());
            }
            else
            {
                Debug.Log($"There is no start conversation for {launchedBootEntry.minigame.roomNameKey}");
                ShowUI();
            }

            //Enable Click Events here to make sure conversation is shown before events are triggered
            UIController.instance.eventSystem.gameObject.SetActive(true);
        }

        public void LoadMinigame(MinigameReference mgRef, UnityEngine.Events.UnityAction andThen)
	    {
            launchedBootEntry = null;

            if (isLoadingMinigame)
                return;

            isLoadingMinigame = true;

            Cleanup();

            onMinigameLoadedOneShot = andThen;

            TryLoadMinigame(mgRef);
	    }

        public void LoadMinigame(Chapter.Entry entry, UnityEngine.Events.UnityAction andThen)
	    {
            LoadMinigame(entry.minigame, andThen);
            launchedBootEntry = entry;
	    }

	    public void LoadMinigame(string minigameName, UnityEngine.Events.UnityAction andThen)
	    {
            if (isLoadingMinigame)
                return;

            isLoadingMinigame = true;
            

            Cleanup();

            onMinigameLoadedOneShot = andThen;

            TryLoadMinigame(minigameName);
	    }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var rc = GetComponent<RectTransform>();

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position, new Vector3(3350f, 1536f, 0f));
            UnityEditor.Handles.Label(transform.position - new Vector3(0f, 0f, 10f), new GUIContent("MinigameController Area\nPlace nothing else"));
        }
        #endif


    }
}

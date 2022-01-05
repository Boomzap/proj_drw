using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System.Linq;
using Steam;

namespace ho
{
    public class HOGameController : SimpleSingleton<HOGameController>, IHOReactor, IWorldState
    {
        public Material staticOutlineMaterial;

        HOMainUI hoMainUI = null;

        public SubHOBlackout subHOBlackout;

        HOInteractiveObject hintObject = null;
        HORoom currentRoom = null;
        public HORoomReference currentRoomRef = null;

        public Camera hoCamera;
        public HOCameraController hoCameraController;

        [ReadOnly]
        public HODifficulty currentDifficulty = HODifficulty.Easy;

        public List<HODifficultySetting> difficultySettings = new List<HODifficultySetting>();
     

        [System.Serializable]
        public class HODifficultySetting
        {
            public HODifficulty hoDifficulty = HODifficulty.Easy;

            public float superHintCooldownTime = 10f;
            public float hintCooldownTime = 10f;

            [BoxGroup("Minigame Lock on Correct Piece")]
            public bool lockClickToRotate = false;
            [BoxGroup("Minigame Lock on Correct Piece")]
            public bool lockClickToSwap = false;
            [BoxGroup("Minigame Lock on Correct Piece")]
            public bool lockClickToSwapRotate = false;
        }

        List<string> keyItemsFound = new List<string>();

        public bool DisableInput
        {
            get { return disableInput || UIController.instance.hasActivePopup || GameController.instance.inConversation; }
            set => disableInput = value;
        }

        bool disableInput = true;
        HOLogic hoLogic = null;
        public HOLogic gameLogic { get { return hoLogic; } }

        int itemFoundTotal = 0;
        int itemToFindTotal = 12;



        public static bool isHintPlaying = false;
        public int hintUse = 0;

        public Material itemFoundGlowMaterial;
        public Material keyItemFoundGlowMaterial;
        public Material hintGlowMaterial;

        [SerializeField]
        HOGameplayConfig gameplayConfig;

        public static HOGameplayConfig config => instance.gameplayConfig;

        [ShowInInspector, ReadOnly]
        protected Chapter.Entry currentChapterEntry = null;
        public Chapter.Entry ChapterEntry => currentChapterEntry;

        public float subHOYOffset = 100f;

        [Header("Sound Effects")]
        public AudioClip onItemFoundAudio;
        public AudioClip onKeyItemFoundAudio;
        public AudioClip onItemFlyAudio;
        public AudioClip onUnlockDoorAudio;
        public AudioClip onOpenSubHOAudio;
        public AudioClip onCloseSubHOAudio;
        public AudioClip onWrongKeyAudio;

        public RandomAudio onHintUsedAudio;

        public bool ActiveRoomContains(HOInteractiveObject obj) => currentRoom?.ActiveRoomContains(obj) ?? false;
        public bool HasOpenSubHO => currentRoom?.HasOpenSubHO ?? false;
        public HORoom GetSubHOContaining(HOInteractiveObject obj) => currentRoom?.GetSubHOContaining(obj) ?? null;

        public List<HOFindableObject> GetActiveItems() => hoLogic.GetActiveItems();
        public List<GameObject> GetActiveDoors() => currentRoom.doorHandlers.Where(x => x.openState != null).Select(x => x.isOpen ? x.openState : x.closedState).ToList();

        public HODoorHandler GetActiveDoorHandler() => currentRoom.doorHandlers.Where(x => x.openState != null).First();

        //public GameObject GetActiveTriviaObject() => currentRoom.interactiveObjects.Where(x => x != null && x is HOTriviaObject).FirstOrDefault().gameObject;

        public List<string> GetActiveKeyItemNames() => currentRoom.interactiveObjects.Where(x => x != null && x is HOKeyItem).Select(y => y.name).ToList();

        public Boomzap.Conversation.Conversation specialItemConversation;

        List<HOFindableObject> superHintList = new List<HOFindableObject>();

        public MagCamDisplay magCamDisplay;

        public HOScoreSettings scoreSettings;

        bool showLevelUnlockPopup;

        //For UseHintTutorial
        public float hintTutorialTimer = 20f;
        float noInteractionTimer = 20f;

        public float GetSuperHintCooldown()
        {
            return difficultySettings.First(x => x.hoDifficulty == currentDifficulty).superHintCooldownTime;
        }

        public float GetHintCooldown()
        {
            return difficultySettings.First(x => x.hoDifficulty == currentDifficulty).hintCooldownTime;
        }

        public HODifficultySetting GetDifficultySetting()
        {
            return difficultySettings.First(x => x.hoDifficulty == currentDifficulty);
        }

        public bool returnToMainMenu = false;

        public bool returnToJournalUI = false;

        bool isReplayingRoom = false;

        int endGameScore = 0;
        int clearTime = 0;
        int rawScore = 0;
        int timeScore = 0;

        // Start is called before the first frame update
        void Start()
        {
            hoMainUI = UIController.instance.hoMainUI;
            subHOBlackout.gameObject.SetActive(false);

            if (GameController.save != null)
            {
                currentDifficulty = (HODifficulty)GameController.save.currentProfile.hoDifficultyIndex;
            }
        }


        void PlaySpecialItemConversation(HOFindableObject specialObject)
        {
            GameController.save.SetConversationFlag(specialObject.name);

            if (Boomzap.Conversation.ConversationManager.instance.WouldConversationPlay(specialItemConversation, GameController.instance.EvaluateConversationNode))
            {
                DisableInput = true;
                hoMainUI.gameObject.PlayAnimation(this, "ho_out", () =>
                {
                    GameController.instance.PlayConversation(specialItemConversation, (_) => ResumeGameplayAfterItemConversation(specialObject.name));
                    //Debug.Log($" Special Object Flag: {specialObject.name} was unset after playing conversation");
                    GameController.save.SetConversationFlag(specialObject.name, false);
                });
            } else
            {
                Debug.LogWarning($"Object: {specialObject.name} conversation would not play even with flag set.");
                GameController.save.SetConversationFlag(specialObject.name, false);
            }

        }

        void ResumeGameplayAfterItemConversation(string unsetFlag)
        {
            //GameController.save.SetConversationFlag(unsetFlag, false);
            hoMainUI.gameObject.PlayAnimation(this, "ho_in", () =>
            {
                DisableInput = false;
            });
        }

        void SetCustomCursor(bool isMouseOver)
        {
            if (isMouseOver)
            {
                Cursor.SetCursor(UIController.instance.mouseDoorCursor, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        void HandleInput()
        {
            if (UIController.instance.isPointerOverUIObject || DisableInput)
            {
                if (hoCameraController.isDragging)
                {
                    hoCameraController.OnEndDrag();
                }
                SetCustomCursor(false);
                return;
            }

            //For Tutorial

            if (noInteractionTimer > 0)
                noInteractionTimer -= Time.deltaTime;

            if (noInteractionTimer < 0)
            {
                if (hoMainUI.hintButton.IsHintReady)
                    Tutorial.TriggerTutorial(Tutorial.Trigger.UseHint);
                else
                    Tutorial.TriggerTutorial(Tutorial.Trigger.Zoom);
            }
                

            var mousePos = hoCamera.ScreenToWorldPoint(Input.mousePosition);

            //Key Item Drag Start
            if (Input.GetMouseButtonDown(0))
            {
                HOInteractiveObject[] hitObjects = currentRoom.HitTestAll(mousePos);

                foreach (var hitObject in hitObjects)
                {
                    if (hitObject is HOKeyItem && hoMainUI.currentKeyItemOnMouse != hitObject)
                    {
                        if (GameController.save.currentProfile.flags.HasFlag("tutorial_" + Tutorial.Trigger.UnlockRoom) == false)
                        {
                            //Tutorial.TriggerTutorial(Tutorial.Trigger.UnlockRoom);
                            //return;
                        }
                        hoMainUI.currentKeyItemOnMouse = hitObject as HOKeyItem;
                        hitObject.OnClick();
                    }
                }
            }

            //Key Item Drag
            if(Input.GetMouseButton(0))
            {
                if (hoMainUI.currentKeyItemOnMouse)
                {
                    hoMainUI.currentKeyItemOnMouse.IsDragging = true;
                }
            }

            if (Input.GetMouseButtonUp(0) && !hoCameraController.isDragging && !hoCameraController.isZooming)
            {
                HOInteractiveObject[] hitObjects = currentRoom.HitTestAll(mousePos);

                bool didHitObject = false;

                foreach (var hitObject in hitObjects)
                {
                    if (hitObject is HODoorItem || hitObject is HOTriviaObject || gameLogic.currentObjects.Contains(hitObject as HOFindableObject))
                    {
                        hitObject.OnClick();
                        didHitObject = true;
                        //Debug.Log($"hit: {hitObject}");
                        break;
                    }
                }

                if (!didHitObject)
                {
                    if (currentRoom.IsOutOfSubHOBounds(mousePos))
                    {
                        Audio.instance.PlaySound(onCloseSubHOAudio);
                        currentRoom.CloseSubHO();
                        hoMainUI.OnActiveHOChange();
                        hoCameraController.ResetCamera(false);
                    }
                    else
                    {
                        OnClickNothing();

                        if (hoLogic is HOLogicPairs)
                        {
                            HOLogicPairs pairLogic = hoLogic as HOLogicPairs;
                            pairLogic.ClearSelection();

                            HOPairUI pairUI = hoMainUI.CurrentSubUI as HOPairUI;
                            pairUI.OnSelectItem(string.Empty);
                        }
                    }
                }
            }
            else
            {
                bool isKeyHit = false;
                bool isDoorHit = false;

                var keyObjects = currentRoom.GetComponentsInChildren<HOKeyItem>(false);

                if (keyObjects.Length > 0)
                {

                    var hitResult = currentRoom.HitTestAll(mousePos);

                    foreach (var key in keyObjects)
                    {
                        if (hitResult.Contains(key))
                        {
                            isKeyHit = true;
                        }
                    }
                }

                var doorObjects = currentRoom.GetComponentsInChildren<HODoorItem>(false);

                if (doorObjects.Length > 0)
                {
                    var hitResult = currentRoom.HitTestAll(mousePos);

                    foreach (var door in doorObjects)
                    {
                        if (hitResult.Contains(door))
                        {
                            door.SetIsMouseover(true);
                            isDoorHit = true;
                        }
                        else
                            door.SetIsMouseover(false);
                    }

                    //foreach (var door in doorObjects)
                    //{
                    //    foreach (var hit in hitResult)
                    //    {
                    //        if (hit == door)
                    //        {
                    //            isMouseOver = true;
                    //        }
                    //    }

                    //    HODoorHandler doorHandler = currentRoom.GetHODoorHandler(door.doorName);
                    //    if (doorHandler != null)
                    //    {
                    //        isMouseOver &= ((doorHandler.openState == door.gameObject) || doorHandler.closedState == door.gameObject);

                    //        //&& doorHandler.keyItem == hoMainUI.currentKeyItemOnMouse

                    //    }
                    //    else isMouseOver = false;

                    //    isMouseOver &= ActiveRoomContains(door);

                    //    door.SetIsMouseover(isMouseOver);
                    //}
                }

                bool isDraggingKeyItem = hoMainUI.currentKeyItemOnMouse != null && hoMainUI.currentKeyItemOnMouse.IsDragging;

                SetCustomCursor(isDoorHit || (isKeyHit && isDraggingKeyItem == false));
            }

            HandleCheats();
         }


        void HandleCheats()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || ENABLE_CHEATS

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (!gameLogic.DebugHandleCtrl(true))
                {
                    foreach (var a in hoLogic.currentObjects)
                    {
                        if (a == hintObject) continue;

                        a.sdfRenderer.gameObject.SetActive(true);
                        a.sdfRenderer.material = a.sdfHitZone.hitZoneIndicator;
                    }

                    var triviaObject = hoLogic.GetActiveTriviaObject();

                    if(triviaObject)
                    {
                        triviaObject.sdfRenderer.gameObject.SetActive(true);
                        triviaObject.sdfRenderer.material = triviaObject.sdfHitZone.hitZoneIndicator;
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                if (!gameLogic.DebugHandleCtrl(false))
                {
                    foreach (var a in hoLogic.currentObjects)
                    {
                        if (a == hintObject) continue;

                        a.sdfRenderer.gameObject.SetActive(false);
                        a.sdfRenderer.material = a.sdfHitZone.hitZoneIndicator;
                    }

                    var triviaObject = hoLogic.GetActiveTriviaObject();

                    if (triviaObject)
                    {
                        triviaObject.sdfRenderer.gameObject.SetActive(false);
                        //triviaObject.sdfRenderer.material = triviaObject.sdfHitZone.hitZoneIndicator;
                    }
                }
            }
            else // left control is not pressed
            {
                gameLogic.DebugHandleCtrl(false);
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                hoMainUI.OnHintReady();
                //hoMainUI.superHintButton.OnHintReady();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                ReplayRoom();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var itemList = hoLogic.GetActiveItems();

                if (itemList.Count == 0) return;

                var item = itemList.Where(x => ActiveRoomContains(x)).OrderBy(x => Random.value).FirstOrDefault();
                if (item == null)
                    item = itemList.OrderBy(x => Random.value).FirstOrDefault();

                if (!ActiveRoomContains(item) && HasOpenSubHO)
                {
                    currentRoom.CloseSubHO();
                }

                if (!ActiveRoomContains(item))
                {
                    HORoom containing = GetSubHOContaining(item);
                    HODoorHandler handler = currentRoom.GetHODoorHandler(containing);

                    if (handler.isOpen)
                        hintObject = handler.openState.GetComponent<HODoorItem>();
                    else
                        hintObject = handler.closedState.GetComponent<HODoorItem>();
                }
                else
                {
                    hintObject = item;
                }

                hintObject.OnClick();
            }
#endif
        }


        public void ReplayRoom()
        {
            isReplayingRoom = true;

            GameController.instance.FadeToHOGame(currentChapterEntry);
        }

        // Update is called once per frame
        void Update()
        {
            HandleInput();
        }

        void OnClickNothing()
        {
            if (hoMainUI.currentKeyItemOnMouse)
            {
                hoMainUI.currentKeyItemOnMouse.ResetToDefaultState();
                hoMainUI.currentKeyItemOnMouse = null;
            }
            

            if(hoLogic is HOLogicReverse)
            {
                HOLogicReverse reverse = hoLogic as HOLogicReverse;
                reverse.selectedItem = null;
            }

            //if (hoMainUI.currentKeyItemOnMouse)
            //{
            //    hoMainUI.ClearKeyItemOnMouse(true, false);
            //}
        }

        public void OnHintUsed()
        {
            hintUse++;
            hoCameraController.ResetCamera(false);

            RandomItemHint(true, true);
        }

        public void DisableSuperHint()
        {
            for (int i = 0; i < superHintList.Count; i++)
            { 
                superHintList[i].StopAnimateGlow();

                if (superHintList[i].sdfRenderer == null) continue;

                superHintList[i].sdfRenderer.gameObject.SetActive(false);
            }

            superHintList.Clear();
        }

        public void OnSuperHintUse()
        {
            hintUse++;
            hoCameraController.ResetCamera(false);

            DisableSuperHint();

            StartCoroutine(OnAnimateSuperHint());
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var rc = GetComponent<RectTransform>();

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position, new Vector3(3350f, 1536f, 0f));
            UnityEditor.Handles.Label(transform.position - new Vector3(0f, 0f, 10f), new GUIContent("HOController Area\nPlace nothing else"));
        }
#endif

        public void Cleanup()
        {
            if (currentRoom)
            {
                currentRoom.ReleaseSubHOs();
                HORoomAssetManager.instance.UnloadRoom(currentRoomRef);
                Destroy(currentRoom.gameObject);
            }

            currentRoom = null;
            currentRoomRef = null;
        }

        IEnumerator OnAnimateSuperHint()
        {
            var itemList = hoLogic.GetActiveItems().Where(x => ActiveRoomContains(x)).ToList();
            superHintList = itemList;

            for (int i = 0; i < itemList.Count; i++)
            {
                //Sdf Renderer is destroyed when object is clicked
                if (itemList[i].sdfRenderer == null) continue;

                itemList[i].AnimateGlow();
                yield return new WaitForSeconds(.3f);
            }

            isHintPlaying = false;
        }

        void RandomItemHint(bool showPath, bool playAudio)
        {
            var itemList = hoLogic.GetActiveItems();

            List<HOInteractiveObject> activeKeyItems = new List<HOInteractiveObject>();
            //Check if there's an active key item
            if (currentRoom.interactiveObjects.Any(x => x != null && x is HOKeyItem && x.gameObject.activeInHierarchy))
            {
                activeKeyItems = currentRoom.interactiveObjects.Where(x => x != null && x is HOKeyItem && x.gameObject.activeInHierarchy).ToList();
            }


            //For Key Item hint
            bool hintKeyItem = Random.Range(0, itemList.Count + activeKeyItems.Count + 1) == 1; //25% chance for key hint

            //If key item is still active and there's no active items left
            if (activeKeyItems.Count > 0 && (hintKeyItem || itemList.Count == 0))
            {
                if(HasOpenSubHO)
                    currentRoom.CloseSubHO();

                hintObject = activeKeyItems[0];
                hintObject.StartCoroutine(ItemHintCor(hintObject, showPath, playAudio));
                return;
            }


            if (itemList.Count == 0) return;

            var item = itemList.Where(x => ActiveRoomContains(x)).OrderBy(x => Random.value).FirstOrDefault();
            if (item == null)
                item = itemList.OrderBy(x => Random.value).FirstOrDefault();

            if (!ActiveRoomContains(item) && HasOpenSubHO)
            {
                currentRoom.CloseSubHO();
            }

            if (!ActiveRoomContains(item))
            {
                HORoom containing = GetSubHOContaining(item);
                HODoorHandler handler = currentRoom.GetHODoorHandler(containing);

                if (handler.isOpen)
                    hintObject = handler.openState.GetComponent<HODoorItem>();
                else
                    hintObject = handler.closedState.GetComponent<HODoorItem>();
            } else
            {
                hintObject = item;
            }

            hintObject.StartCoroutine(ItemHintCor(hintObject, showPath, playAudio));

            //activeKeyItems.Cast<HOKeyItem>();
        }

        IEnumerator ItemHintCor(HOInteractiveObject obj, bool showPath, bool playAudio)
        {
            if (showPath)
            {
                hoMainUI.PlayHintPath(obj);
                while (hoMainUI.AnimatingHintPath)
                    yield return null;
            }

            if (playAudio)
                Audio.instance.PlaySound(onHintUsedAudio.GetClip(null));

            float timer = 0f;

            int orgSortOrder = obj.GetComponent<SpriteRenderer>().sortingOrder;
            obj.GetComponent<SpriteRenderer>().sortingOrder = 10000;

            Vector2 curScale = obj.transform.localScale;

            obj.sdfRenderer.gameObject.SetActive(true);
            obj.sdfRenderer.material = hintGlowMaterial;

            hintGlowMaterial.SetFloat("_GlowAlpha", 1f);

            while (timer <= 0.5f)
            {
                float a = timer * 2f;
                obj.transform.localScale = curScale * (1f + 0.1f * a);

                timer += Time.deltaTime;

                yield return null;
            }

            timer = 0f;
            while (timer < 1f)
            {
                float a = Mathf.Sin(timer * 4f * Mathf.PI * 2f);

                obj.transform.localEulerAngles = new Vector3(0f, 0f, 7.5f * a);

                timer += Time.deltaTime;
                yield return null;
            }


            timer = 0f;
            while (timer <= 0.5f)
            {
                float a = 1f - (timer * 2f);
                obj.transform.localScale = curScale * (1f + 0.1f * a);
                hintGlowMaterial.SetFloat("_GlowAlpha", a);
                timer += Time.deltaTime;
                yield return null;
            }

            obj.GetComponent<SpriteRenderer>().sortingOrder = orgSortOrder;
            obj.sdfRenderer.gameObject.SetActive(false);
            hintObject = null;

            isHintPlaying = false;
        }

        public void OnBeginFadeTo()
        {
            gameObject.SetActive(true);
            GameController.instance.SetActiveCamera(hoCamera);
            hoCameraController.ResetCamera(false);

            if (subHOBlackout.gameObject.activeInHierarchy)
            {
                subHOBlackout.Hide();
            }
            InitializeGameplay();
        }

        public void OnFadeOutDone()
        {
            magCamDisplay.Close(true);
            gameObject.SetActive(false);
            GameController.instance.SetActiveCamera(GameController.instance.defaultCamera);
            hoCameraController.ResetCamera(true);
            Cleanup();
        }

        public bool ShouldDestroyOnLeave()
        {
            return false;
        }

        public void OnLeave()
        {
            OnFadeOutDone();
        }

        public bool SetRoom(HORoomReference roomRef, Chapter.Entry chapterEntry)
        {
            var av = HORoomAssetManager.instance.GetRoomAssetStatus(roomRef);
            currentChapterEntry = chapterEntry;

            if (av.isLoaded && av.room != null)
            {
                Cleanup();

                currentRoom = Instantiate(av.room, transform);
                currentRoom.name = av.roomName;
                currentRoomRef = roomRef;
                currentRoom.LoadSubHOs();

                return true;
            }

            Debug.LogError("Trying to set room: " + av.roomName + " when it is not loaded");

            return false;
        }

        public void OnFadeToComplete()
        {
            if (currentChapterEntry.onStartConversation != null)
            {
                GameController.instance.PlayConversation(currentChapterEntry.onStartConversation, (bool _) => BeginGameplay());

                
            } else
            {
                Debug.Log($"There is no start conversation for {currentChapterEntry.hoRoom.roomName}");
                BeginGameplay();
            }

            
        }

        public void GetItemMaskSize(HOFindableObject fromObject, Canvas forCanvas, out Vector2 maskScreenPos, out Vector2 maskPos, out Vector2 maskSize)
        {
            var roomScale = hoCamera.pixelHeight / currentRoom.rectTransform.sizeDelta.y;
            var spriteRenderer = fromObject.gameObject.GetComponent<SpriteRenderer>();

            roomScale /= forCanvas.scaleFactor;

            float msize = Mathf.Max(spriteRenderer.size.x, spriteRenderer.size.y);

            maskSize = new Vector2(msize, msize) * roomScale;
            maskScreenPos = hoCamera.WorldToScreenPoint(fromObject.transform.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(forCanvas.transform as RectTransform, maskScreenPos,
                forCanvas.worldCamera, out maskPos);
        }

        UnityEngine.UI.Image CloneRoomObjectToCanvasImage(HOInteractiveObject obj, Canvas toCanvas)
        {
            var ssPos = hoCamera.WorldToScreenPoint(obj.transform.position);
            var spriteRenderer = obj.gameObject.GetComponent<SpriteRenderer>();
            var sprite = spriteRenderer.sprite;

            // base cloned object holds image for the item
            GameObject go = new GameObject("animating");
            go.transform.SetParent(toCanvas.transform);

            var image = go.gameObject.AddComponent<UnityEngine.UI.Image>();
            var roomScale = hoCamera.pixelHeight / currentRoom.rectTransform.sizeDelta.y;
            roomScale /= toCanvas.scaleFactor;

            image.sprite = sprite;
            image.rectTransform.localScale = new Vector3(roomScale, roomScale, 1f);
            image.rectTransform.sizeDelta = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y);

            // position from world to canvas space
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(toCanvas.transform as RectTransform, ssPos,
                toCanvas.worldCamera, out lp);
            image.transform.localPosition = lp;

            // copy the sdf into a child gameobject w/ image
            var sdfImageObject = new GameObject("sdf");
            sdfImageObject.transform.SetParent(go.transform);
            var sdfImage = sdfImageObject.AddComponent<UnityEngine.UI.Image>();

            Vector3 sdfScale = obj.sdfRenderer.transform.localScale;

            sdfImage.sprite = obj.sdfRenderer.sprite;
            sdfImage.rectTransform.sizeDelta = obj.sdfRenderer.sprite.rect.size;// * (sdfScale.x * 0.01f);

            if (obj is HOKeyItem)
                sdfImage.material = Instantiate(keyItemFoundGlowMaterial);
            else
                sdfImage.material = Instantiate(itemFoundGlowMaterial);

            sdfImageObject.transform.localScale = new Vector3(1f, 1f, 1f);
            sdfImageObject.transform.localPosition = new Vector3(0f, 0f, 0f);

            sdfImage.raycastTarget = false;
            image.raycastTarget = false;

            return image;
        }
        void FinishItemCollect(HOFindableObject foundObject, IEnumerable<HOFindableObject> nextObjects, UnityEngine.UI.Image clonedImage)
        {
            itemFoundTotal++;
            hoMainUI.SetItemFoundTotal(itemFoundTotal, hoLogic.itemsTotalToFind);

            if (foundObject is HOKeyItem)
            {
                HOKeyItemHolder keyHolder = hoMainUI.GetKeyItemHolder(foundObject);
                keyHolder.SetObject(foundObject as HOKeyItem);
                //Tutorial.TriggerTutorial(Tutorial.Trigger.FoundKeyItem);
            }

            HOItemHolder itemHolder = hoMainUI.GetItemHolder(foundObject);
            itemHolder.SetObjects(nextObjects, true);

            //Destroy(foundObject.gameObject);
            foundObject.gameObject.SetActive(false);
            Destroy(clonedImage.gameObject);

            SaveState();

            if (foundObject.isSpecialStoryItem)
                PlaySpecialItemConversation(foundObject);
        }

        void FinishItemCollect(HOFindableObject foundObject, IEnumerable<HOFindableObject> nextObjects)
        {
            itemFoundTotal++;
            hoMainUI.SetItemFoundTotal(itemFoundTotal, hoLogic.itemsTotalToFind);

            HOItemHolder itemHolder = hoMainUI.GetItemHolder(foundObject);
            itemHolder.SetObjects(nextObjects, true);

            SaveState();
        }

        #region IHOReactor implementation
        public void UpdateActiveItemInList(HOFindableObject foundObject, IEnumerable<HOFindableObject> nextObjects)
        {
            if (hintObject == foundObject)
            {
                hintObject.StopAllCoroutines();
                iTween.Stop(hintObject.gameObject);
            }

            HOItemHolder itemHolder = hoMainUI.GetItemHolder(foundObject);

            if(hoLogic is HOLogicReverse)
            {
                FinishItemCollect(foundObject, nextObjects);
            }
            else
            {
                var canvas = hoMainUI.GetComponent<Canvas>();

                // UI takes ownership now, no longer exists in scene
                var cloneImage = CloneRoomObjectToCanvasImage(foundObject, canvas);
                var sdfImage = cloneImage.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();

                // leave the object's base GameObject as it'll be used for data
                Destroy(foundObject.sdfRenderer.gameObject);
                //Destroy(foundObject.GetComponent<SpriteRenderer>());
                foundObject.GetComponent<SpriteRenderer>().enabled = false;
                var hitZone = foundObject.GetComponent<SDFHitZone>();
                if (hitZone)
                    hitZone.enabled = false;
                // ensure it exists in the main HO otherwise it'll become inactive
                foundObject.transform.SetParent(currentRoom.roomRoot.transform, true);

                var startPos = new Vector2(cloneImage.rectTransform.localPosition.x, cloneImage.rectTransform.localPosition.y);

                hoMainUI.AnimateObjectCollection(foundObject, cloneImage, sdfImage, startPos, () =>
                {
                    FinishItemCollect(foundObject, nextObjects, cloneImage);
                });
            }
        }

        void UpdateClearData()
        {
            endGameScore = UIController.instance.hoMainUI.hoScoreKeeper.GetTotalScore();
            clearTime = UIController.instance.hoMainUI.timeKeeper.GetCurrentTimePlayed();
            rawScore = hoMainUI.hoScoreKeeper.GetRawScore();
            //hintScore = hoMainUI.hoScoreKeeper.GetHintScore();
            timeScore = hoMainUI.hoScoreKeeper.GetTimeScore();

            HORoomDataHelper.instance.InsertModeData(currentRoomRef.AssetGUID, hoLogic.ToString(), endGameScore, clearTime);
            Savegame.SetDirty();
        }
        IEnumerator GameplayEndCor()
        {
            SaveState(true);

            yield return new WaitForSeconds(2f);

            currentRoom.CloseSubHO();
            while (GameController.instance.inConversation || UIController.instance.conversationUI.gameObject.activeInHierarchy)
            {
                // essentially loop until a conversation is complete if there is one.
                Debug.Log("Conversation loop...");
                yield return new WaitForEndOfFrame();
            }

            OnPostGameplay();
        }
        public void OnItemListEmpty()
        {
            Debug.Log("OnItemListEmpty()");

            StartCoroutine(GameplayEndCor());
        }

        public void SetInitialItemList(List<HOFindableObject> initialObjects, int totalToFind)
        {
            hoMainUI.SetInitialItemList(initialObjects, totalToFind);
            itemToFindTotal = totalToFind;
            SaveState();

            hintUse = 0;
            hoMainUI.UpdateHint();
            hoMainUI.gameObject.PlayAnimation(hoMainUI, "ho_in", CheckTutorials);
        }

        #endregion

        public void CheckTutorials()
        {
            hoMainUI.DisableInputForShowAnimation();

            if (hoLogic is HOLogicDetail)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.DetailMode);
            }
            else if(hoLogic is HOLogicFindX)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.FindXMode);
            }
            else if (hoLogic is HOLogicPicture)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.ImageMode);
            }
            else if (hoLogic is HOLogicOdd)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.OddMode);
            }
            else if (hoLogic is HOLogicReverse)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.ReverseHOMode);
            }
            else if (hoLogic is HOLogicSpecialRiddle)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.SpecialRiddle);
            }
            else if (hoLogic is HOLogicRiddle)
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.RiddleMode);
            }
            else
            {
                Tutorial.TriggerTutorial(Tutorial.Trigger.FindObject);
                Tutorial.TriggerTutorial(Tutorial.Trigger.ScoreMultiplier);
            }
                hoMainUI.EnableRaycaster();
            
            //Enable Input after Triggering tutorial
            DisableInput = false;
        }

        void OnShowCompletePopup(UnityAction andThen = null)
        {
            UpdateClearData();

            Popup.HidePopup<NotificationPopup>();

            CompletePopup popup = Popup.GetPopup<CompletePopup>();
            popup.SetupBeforeShow(clearTime, rawScore, timeScore, hintUse, endGameScore, currentRoomRef, andThen);
            popup.Show();
        }

        void OnShowAchievements(UnityAction andThen = null)
        {
            AchievementPopup popup = Popup.GetPopup<AchievementPopup>();
            popup.onHiddenOneshot += () => andThen?.Invoke();
            popup.SetUpAchievements();
            popup.Show();
        }

        [Button]
        void OnGameplayFinished()
        {
            Debug.Log("Gameplay Finished");
            //If Unlimited Mode -> Return to Unlimited mode UI
            if (GameController.instance.isUnlimitedMode)
            {
                GameController.save.currentProfile.flags.SetFlag("unlimited_clear", true);

                if (Steam.SteamAchievements.achievementsToLoadCount > 0)
                {
                    OnShowCompletePopup(() =>

                        OnShowAchievements(() =>
                        {
                            GameController.instance.FadeToUnlimitedMenu();
                        }
                      )
                    );
                }
                else
                {
                    //If no achievement to be set just go directly to Unlimited Menu
                    OnShowCompletePopup(() =>
                        GameController.instance.FadeToUnlimitedMenu());
                }
                return;
            }

            if (returnToMainMenu)
            {
                if (Steam.SteamAchievements.achievementsToLoadCount > 0)
                {
                    OnShowAchievements(() =>
                    {
                        GameController.instance.FadeToGameMenu();
                    });
                }
                else
                    GameController.instance.FadeToGameMenu();
                returnToMainMenu = false;
                Debug.Log("Return to Main");
            }
            else if (returnToJournalUI)
            {
                GameController.instance.FadeToJournalMenu();
                returnToJournalUI = false;
                return;
            }
            else if (hoLogic is HOLogicDetail)
            {
                if (currentChapterEntry.onEndConversation != null)
                {
                    hoMainUI.onHiddenOneshot += () =>
                    {
                        GameController.instance.PlayConversation(currentChapterEntry.onEndConversation, (bool_) =>
                        {
                            if (Steam.SteamAchievements.achievementsToLoadCount > 0)
                            {
                                OnShowAchievements(() => GameController.instance.PlayNextRoom(currentChapterEntry));
                            }
                            else
                            {
                                GameController.instance.PlayNextRoom(currentChapterEntry);
                            }
                        });
                    };
                }
                else
                {
                    Debug.Log($"There is no end conversation for {currentChapterEntry.hoRoom.roomName}");
                    if (Steam.SteamAchievements.achievementsToLoadCount > 0)
                    {
                        OnShowAchievements(() => GameController.instance.PlayNextRoom(currentChapterEntry));
                    }
                    else
                    {
                        GameController.instance.PlayNextRoom(currentChapterEntry);
                    }
                }

                GameController.save.CheckChapterCompletion();
            }
            else
            {

                Debug.Log("Play Conversation?");
                //Check if there's an end conversation
                if (currentChapterEntry.onEndConversation != null)
                {
                    Debug.Log("Play End Conversation");

                    hoMainUI.onHiddenOneshot += () =>
                    {
                        OnShowCompletePopup(() =>
                           GameController.instance.PlayConversation(currentChapterEntry.onEndConversation, (bool_) =>
                            {
                                if (Steam.SteamAchievements.achievementsToLoadCount > 0)
                                {
                                    OnShowAchievements(() => GameController.instance.PlayNextRoom(currentChapterEntry));
                                }
                                else
                                {
                                    GameController.instance.PlayNextRoom(currentChapterEntry);
                                }
                            })
                           );
                    };
                }
                else
                {
                    Debug.Log($"There is no end conversation for {currentChapterEntry.hoRoom.roomName}");
                    OnShowCompletePopup(() =>
                       {
                           if (Steam.SteamAchievements.achievementsToLoadCount > 0)
                           {
                               OnShowAchievements(() => GameController.instance.PlayNextRoom(currentChapterEntry));
                           }
                           else
                           {
                               GameController.instance.PlayNextRoom(currentChapterEntry);
                           }
                       }
                    );
                }

                GameController.save.CheckChapterCompletion();
            }

            hoMainUI.Hide();
        }

        void CheckAchievements()
        {
            clearTime = UIController.instance.hoMainUI.timeKeeper.GetCurrentTimePlayed();

            if (clearTime < 90)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_90);
            if (clearTime < 180)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_180);
            if (clearTime < 360)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_360);

            if (clearTime < 90)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_SCENE_FIN_90);

            if(GameController.instance.isUnlimitedMode)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_UNLIMITED);

            if (hoLogic is HOLogicPicture)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_IMAGE);

            if (hoLogic is HOLogicRiddle)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_RIDDLE);

            if (hoLogic is HOLogicFindX)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_COLLECTION);

            if (hoLogic is HOLogicScramble)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_SCRAMBLE);

            if (hoLogic is HOLogicNoVowel)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_NOVOWEL);

            if (hoLogic is HOLogicPairs)
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_PLAY_PAIRS);

            if(currentChapterEntry.onFinishAchievement != SteamAchievements.Achievement.NONE)
            {
                SteamAchievements.SetAchievement(currentChapterEntry.onFinishAchievement);
            }

            //Filter sub ho scenes and det scenes
            var roomReferences = HORoomAssetManager.instance.GetRoomReferences();
            List<Savegame.HORoomData> roomsUnlocked = HORoomDataHelper.instance.GetHORoomsUnlockedData(roomReferences);
            int triviasFound = roomsUnlocked.Select(x => x.triviasFound).Sum(y => y.Count);

            if(triviasFound == roomReferences.Count * 5)
            {
                SteamAchievements.SetAchievement(SteamAchievements.Achievement.ACH_TRIVIA_FOUND_ALL);
            }
        }

        void OnPostGameplay()
        {
            Debug.Log("Post Gameplay");
            magCamDisplay.Close(true);

            hoCameraController.ResetCamera(false);
            hoCameraController.isZoomEnabled = false;

            CheckAchievements();


            if (hoLogic is HOLogicDetail)
            {
                showLevelUnlockPopup = false;
            }

            // play animation , conversation, blah blah 
            if (showLevelUnlockPopup)
            {
                //Debug.Log("Show level Unlocked");
                showLevelUnlockPopup = false;
                NotificationPopup popup = Popup.GetPopup<NotificationPopup>();

                string header = LocalizationUtil.FindLocalizationEntry("LevelUnlocked", string.Empty, false, TableCategory.UI);
                string message = LocalizationUtil.FindLocalizationEntry("UnlimitedUnlock", string.Empty, false, TableCategory.UI);

                string colorPrefix = "<color=yellow>";
                string colorSuffix = "</color>";

                string roomName = string.IsNullOrEmpty(currentRoom.roomDisplayName) ? "This Room" : LocalizationUtil.FindLocalizationEntry(currentRoom.roomDisplayName);

                roomName = colorPrefix + roomName + colorSuffix;

                message = string.Format(message, roomName);

                popup.SetupPopup(header, message);

                popup.onHiddenOneshot += () => OnGameplayFinished();
                popup.Show();
            }
            else
            {
                OnGameplayFinished();
            }
        }

        void InitializeGameplay()
        {
            SetCustomCursor(false);
            magCamDisplay.ResetToDefault();

            DisableSuperHint();
            SteamAchievements.ClearAchievementsList();

            if (ChapterEntry != null && ChapterEntry.music != null)
            {
                Audio.instance.PlayMusic(ChapterEntry.music);
            }

            if (ChapterEntry != null && ChapterEntry.ambient != null)
            {
                Audio.instance.PlayAmbient(ChapterEntry.ambient);
            }

            hoMainUI = UIController.instance.hoMainUI;

            hoLogic = HOLogic.Create(currentChapterEntry.hoLogic);

            hoMainUI.Initialize(hoLogic);
            hoLogic.Initialize(currentRoom, currentRoomRef, this, hoMainUI.GetListCapacity(), currentChapterEntry, isReplayingRoom);

            hoMainUI.Show();
            hoMainUI.SetKeyItemCount(hoLogic.GetFindableItemCount<HOKeyItem>());

            Savegame.HOSceneState state = GameController.instance.isUnlimitedMode || isReplayingRoom? null : GameController.save.GetSceneState(currentChapterEntry);

            if (state != null && state.hasSaveState && !returnToMainMenu)
            {
                //Skip Adding Key Items to total count
                //var keyItems = state.heldKeyItems.Select(x => currentRoom.FindObjectByName(x) as HOKeyItem);
                //hoMainUI.SetKeyItemCount(hoLogic.GetFindableItemCount<HOKeyItem>() + keyItems.Count());

                //foreach (var key in keyItems)
                //{
                //    HOKeyItemHolder keyHolder = hoMainUI.GetKeyItemHolder(null);
                //    keyHolder.SetObject(key);
                //}

                foreach (var door in state.openDoors)
                {
                    var handler = currentRoom.GetHODoorHandler(door);
                    if (handler != null)
                    {
                        handler.SetOpen(true);
                    }
                }

                foreach (var door in state.closedDoors)
                {
                    var handler = currentRoom.GetHODoorHandler(door);
                    if (handler != null)
                    {
                        handler.SetOpen(false);
                    }
                }

                var badDoors = currentRoom.doorHandlers.Where(x => !state.openDoors.Contains(x.baseName) && !state.closedDoors.Contains(x.baseName));
                foreach (var door in badDoors)
                {
                    Destroy(door.closedState);
                    Destroy(door.openState);
                    if (door.keyItem == null)
                    {
                        Debug.Log($"Key Item Empty: {door.baseName}");
                        continue;
                    }
                    Destroy(door.keyItem.gameObject);
                }

                itemFoundTotal = hoLogic.itemsTotalToFind - hoLogic.itemsLeftToFind;
            }
            else
            {
                itemFoundTotal = 0;
            }

            keyItemsFound = new List<string>();

            noInteractionTimer = hintTutorialTimer;

            //Unset if replay mode after loading logic and states
            isReplayingRoom = false;

            isHintPlaying = false;
        }
        
        public void SaveState(bool postGameplay = false)
        {
            if (returnToMainMenu)   // debug mode
                return;

            Savegame.HOSceneState state = GameController.save.GetSceneState(currentChapterEntry);

            if (GameController.instance.isUnlimitedMode)
            {
                //if (state.unlocked && postGameplay)
                //{
                //    HORoomDataHelper.instance.InsertModeData(currentRoomRef.AssetGUID, hoLogic.ToString(), endGameScore, clearTime);
                //    Savegame.SetDirty();
                //}
                return;
            }
                
            state.hasSaveState = true;
            state.currentObjects = hoLogic.currentObjects.Select(x => x.name).ToList();
            state.futureObjects = hoLogic.futureObjects.Select(x => x.name).ToList();
            state.inactiveObjects = hoLogic.inactiveObjects.Select(x => x.name).ToList();
            //state.heldKeyItems = hoMainUI.heldKeyItems.Select(x => x.name).ToList(); -> Use this if you'll be using UI Key Item Holders

            state.heldKeyItems = GetActiveKeyItemNames();
            state.closedDoors = currentRoom.doorHandlers.Where(x => x.isValid && !x.isOpen).Select(x => x.baseName).ToList();
            state.openDoors = currentRoom.doorHandlers.Where(x => x.isValid && x.isOpen).Select(x => x.baseName).ToList();

            if (state.completed && postGameplay && state.unlocked == false)
            {
                showLevelUnlockPopup = true;
                state.unlocked = true;
            }

            Savegame.SetDirty();
        }

        void BeginGameplay()
        {
            DisableInput = true;

            hoCameraController.isZoomEnabled = true;

            hoLogic.StartGameplay();

            hoMainUI.SetItemFoundTotal(itemFoundTotal, hoLogic.itemsTotalToFind);
        }

        public void OnFindableObjectClick(HOFindableObject obj)
        {
            if (DisableInput) return;

            if (hoMainUI.haveItemOnMouse)
            {
                OnClickNothing();
                return;
            }

            noInteractionTimer = hintTutorialTimer;

            // if this is null, the animation hasn't completed from the previous item,
            // or.. a bug
            HOItemHolder itemHolder = hoMainUI.GetItemHolder(obj);
            if (itemHolder != null)
            {
                if (hoLogic.OnItemClicked(obj))
                {
                    //NOTE* Disable hint flag when hint object was clicked.
                    if (obj == hintObject)
                        isHintPlaying = false;

                    if (obj is HOKeyItem)
                    {
                        Audio.instance.PlaySound(onKeyItemFoundAudio);
                        
                    } else
                    {
                        Vector3 objPosition = hoCamera.WorldToScreenPoint(obj.transform.position);

                        if (hoLogic is HOLogicPairs)
                        {
                            HOLogicPairs pair = hoLogic as HOLogicPairs;
                            //Multiply x2 Score
                            hoMainUI.hoScoreKeeper.OnItemPicked(pair.currentSelectedPair.transform.position);
                            hoMainUI.hoScoreKeeper.OnItemPicked(objPosition);
                            pair.currentSelectedPair = null;
                        }
                        else if (hoLogic is HOLogicReverse)
                        {
                            HOLogicPairs pair = hoLogic as HOLogicPairs;
                        }
                        else
                        {
                            //Don't use scoring for HOLogicDetail
                            if(hoLogic is HOLogicDetail == false)
                                hoMainUI.hoScoreKeeper.OnItemPicked(objPosition);
                        }
                        Audio.instance.PlaySound(onItemFoundAudio);
                    }

                    if (superHintList.Count > 0)
                    {
                        superHintList.Remove(obj);
                        isHintPlaying = false;
                        StopCoroutine(OnAnimateSuperHint());
                        obj.StopAnimateGlow();
                        DisableSuperHint();
                    }
                       
                } else
                {
                    OnClickNothing();
                }
            }
        }

        public void OnDoorObjectClick(HODoorItem obj)
        {
            if (DisableInput) return;
            if (isHintPlaying) return;

            if(hoCameraController.isZooming==false)
                hoCameraController.ResetCamera(false);
           
            // handle door click
            HODoorHandler doorHandler = currentRoom.GetHODoorHandler(obj.doorName);

            if (hintObject == obj)
            {
                hintObject.StopAllCoroutines();
                iTween.Stop(hintObject.gameObject);
            }

            if (doorHandler != null)
            {
                //if (GameController.save.currentProfile.flags.HasFlag("tutorial_" + Tutorial.Trigger.UnlockRoom) == false)
                //{
                //    Tutorial.TriggerTutorial(Tutorial.Trigger.UnlockRoom);
                //    return;
                //}


                if (hoMainUI.currentKeyItemOnMouse)
                {
                    if (doorHandler.keyItem == hoMainUI.currentKeyItemOnMouse &&
                        doorHandler.closedState == obj.gameObject)
                    {
                        //Destroy Key Item when door is unlocked
                        Destroy(hoMainUI.currentKeyItemOnMouse.gameObject);
                        hoMainUI.currentKeyItemOnMouse = null;
                        
                        //hoMainUI.ClearKeyItemOnMouse(false, true);

                        doorHandler.SetOpen(true);
                        Audio.instance.PlaySound(onUnlockDoorAudio);

                        // there is a period where the key item will still be 'in' UI while it's animation, delay the save a little.
                        this.ExecuteAfterDelay(2f, () => SaveState());

                        return;
                    } else
                    {
                        Audio.instance.PlaySound(onWrongKeyAudio);
                    }
                }
                else
                {
                    if (doorHandler.closedState == obj.gameObject)
                    {
                        HODoorItem hoDoorItem = doorHandler.closedState.GetComponent<HODoorItem>();
                        hoMainUI.ShowItemPrompt(hoDoorItem.promptKey);
                    }
                }


                if (doorHandler.openState == obj.gameObject)
                {
                    Audio.instance.PlaySound(onOpenSubHOAudio);
                    currentRoom.OpenSubHO(doorHandler.subHO, obj);
                    hoMainUI.OnActiveHOChange();
                }
            }

            OnClickNothing();
        }

        public void OnTriviaObjectClick(HOTriviaObject obj)
        {
            Debug.Log("Trivia Object Clicked!");
            if (DisableInput) return;

            if (hoMainUI.haveItemOnMouse)
            {
                OnClickNothing();
                return;
            }

            NotificationPopup popup = Popup.GetPopup<NotificationPopup>();

            if(popup.gameObject.activeInHierarchy)
            {
                return;
            }

            Audio.instance.PlaySound(onUnlockDoorAudio);

            string header = LocalizationUtil.FindLocalizationEntry("TriviaFound", string.Empty, false, TableCategory.UI);
            string message = LocalizationUtil.FindLocalizationEntry(obj.displayKey, string.Empty, false, TableCategory.Trivia);

            popup.SetupPopup(header, message);

            obj.AnimateGlow(() =>
                {
                    obj.gameObject.SetActive(false);
                    popup.Show();
                    
                }, 1f);

            HORoomDataHelper.instance.InsertTriviaFound(currentRoomRef.AssetGUID, obj.displayKey);
            Savegame.SetDirty();
        }

        public void EnableRoomObjects(bool enable)
        {
            if(currentRoom)
             currentRoom.SetupDisplayMode(enable);
        }
    }
}

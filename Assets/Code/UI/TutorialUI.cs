using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class TutorialUI : Popup
    {
        [SerializeField]
        TextMeshProUGUI tutorialText;
        [SerializeField]
        TextMeshProUGUI headerText;
        [SerializeField]
        TutorialFilter tutorialInputFilter;
        [SerializeField]
        FullscreenCutout fullscreenCutout;
        [SerializeField]
        GameObject clickToContinueText;
        [SerializeField]
        float clickToContinueDelay = 0.5f;

        TutorialEntry showingEntry = null;
        Vector2 defaultPos;

        float finishShowTime = 0f;

        bool isPlayingAnimation = false;

        protected override bool UseBlackout => false;

        protected override void Awake()
        {
            base.Awake();
            defaultPos = transform.localPosition;
        }

        public override void Hide(bool instant = false)
        {
            if (instant)
            {
                fullscreenCutout.gameObject.SetActive(false);
            }
            else
            {
                UIController.instance.StartCoroutine(ShowHideCutoutCor(0f));
            }

            if (showingEntry != null)
            {
                GameController.save.currentProfile.flags.SetFlag("tutorial_" + showingEntry.name, true);
            }


            base.Hide(instant);
        }

        IEnumerator ShowHideCutoutCor(float targetAlpha)
        {

            float t = 0f;
            const float time = 0.3f;

            fullscreenCutout.gameObject.SetActive(true);

            while (t < time)
            {
                float a = t / time;

                if (targetAlpha == 0f)
                    a = (1f - a) * 0.8f;
                else
                    a *= 0.8f;

                fullscreenCutout.CoverColor = new Color(0f, 0f, 0f, a);

                t += Time.deltaTime;

                yield return null;
            }

            if (targetAlpha == 0f)
                fullscreenCutout.gameObject.SetActive(false);

        }

        public override void Show(bool instant = false)
        {
            clickToContinueText.SetActive(false);
            // figure best position
            finishShowTime = 0f;

            UIController.instance.StartCoroutine(ShowHideCutoutCor(1f));

            base.Show(false);
        }

        protected override void OnFinishShow()
        {
            base.OnFinishShow();
            finishShowTime = Time.time;
            this.ExecuteAfterDelay(clickToContinueDelay, () => clickToContinueText.SetActive(true));
        }

        protected override void OnFinishHide()
        {
            base.OnFinishHide();

            isPlayingAnimation = false;

            transform.localPosition = defaultPos;

            if (showingEntry == null) return;

            if (!string.IsNullOrWhiteSpace(showingEntry.next))
            {
                Tutorial.TriggerTutorial(showingEntry.next);
            }

            //Save Tutorial Flag
            Savegame.SetDirty();
        }

        private void Update()
        {
            if (finishShowTime > 0f && (Time.time - finishShowTime) > clickToContinueDelay)
            {
                if (Input.GetMouseButtonDown(0) && isPlayingAnimation == false)
                {
                    isPlayingAnimation = true;
                    Hide();
                }
            }
        }

        static SpriteRenderer[] GetMapEntries(bool onlyPuzzle, bool onlyBoss)
        {
            if (GameController.instance.CurrentWorldState != null &&
                GameController.instance.CurrentWorldState is MapController)
            {
                MapController mapController = GameController.instance.CurrentWorldState as MapController;

                if (onlyBoss)
                {
                    return new SpriteRenderer[] { mapController.GetBossSprite() };
                }
                else if (onlyPuzzle)
                {
                    return mapController.GetAllActivePuzzleSprites();
                }
                else
                {
                    return mapController.GetAllActiveNodeSprites();
                }
            }

            Debug.Log("Can't get map entries - we're not in the map.");

            return new SpriteRenderer[0];
        }


        //static SpriteRenderer[] GetTriviaKey()
        //{
        //    var triviaObject = HOGameController.instance.GetActiveTriviaObject();

        //    var renderer = triviaObject.GetComponent<SpriteRenderer>();
        //    return new SpriteRenderer[1] { renderer };
        //}

        static SpriteRenderer[] GetHOEntries(bool onlyKey, bool onlyDoor)
        {
            if (GameController.instance.CurrentWorldState != null &&
                GameController.instance.CurrentWorldState is HOGameController)
            {
                HOGameController hoController = HOGameController.instance;
                List<SpriteRenderer> sprites = new List<SpriteRenderer>();

                if (onlyDoor)
                {
                    var activeDoors = hoController.GetActiveDoors();

                    foreach (var item in activeDoors)
                    {
                        var renderer = item.GetComponent<SpriteRenderer>();
                        sprites.Add(renderer);
                    }

                }
                else if (onlyKey)
                {

                }

                else
                {

                    var activeItems = hoController.GetActiveItems();

                    foreach (var item in activeItems)
                    {
                        var renderer = item.GetComponent<SpriteRenderer>();
                        Vector3 screenPos = hoController.hoCamera.WorldToScreenPoint(renderer.transform.position);

                        if (screenPos.y < (Screen.height / 3))
                            sprites.Add(renderer);
                    }
                }

                if (sprites.Count == 0)
                {
                    Debug.LogWarning($"No tutorial entries found: {onlyKey}, {onlyDoor}, {hoController.currentRoomRef.roomName}");
                    return new SpriteRenderer[0];
                }

                return sprites.GetRange(0, 1).ToArray();
            }
            
            Debug.Log("Can't get HO entries - we're not in a scene.");

            return new SpriteRenderer[0];
        }

        static Graphic[] GetUIEntriesForSubsceneObjects()
        {
            if (GameController.instance.CurrentWorldState != null &&
               GameController.instance.CurrentWorldState is HOGameController)
            {
                List<Graphic> graphics = new List<Graphic>();
                HOMainUI ui = UIController.instance.hoMainUI;
                HOGameController hoController = HOGameController.instance;

                var subRoomItems = hoController.GetActiveItems().Where(x => !hoController.ActiveRoomContains(x)).ToArray();

                foreach (var e in subRoomItems)
                {
                    var holder = ui.CurrentSubUI.GetItemHolder(e);
                    if (holder == null) continue;
                    var graphic = holder.GetComponent<Graphic>();
                    if (graphic == null) continue;
                    graphics.Add(graphic);
                }

                if (graphics.Count == 0)
                {
                    Debug.LogWarning($"No sub-room entries found: {hoController.currentRoomRef.roomName}");
                }

                return graphics.ToArray();
            }

            Debug.Log("Can't get HO entries - we're not in a scene.");

            return new Graphic[0];
        }

        static void AddCutouts(SpriteRenderer[] sprites, FullscreenCutout cutout)
        {
            foreach (var s in sprites)
            {
                cutout.AddCutout(s, GameController.instance.currentCamera);
            }
        }

        static void AddCutouts(Graphic[] sprites, FullscreenCutout cutout)
        {
            foreach (var s in sprites)
            {
                cutout.AddCutout(s);
            }
        }

        void RepositionPopup(SpriteRenderer[] sprites)
        {
            if (sprites.Length == 0) return;

            if (sprites[0] == null) return;

            Camera c = HOGameController.instance.hoCamera;
            var y = c.WorldToScreenPoint(sprites[0].transform.position).y;

            if (y > (Screen.height / 2))
            {
                transform.localPosition = new Vector3(0f, -200f , 0f);
            }
            else
            {
                transform.localPosition = new Vector3(0f, 200f, 0f);
            }
        }

        public static void RunTutorial(TutorialEntry entry)
        {
            var me = GetPopup<TutorialUI>();

            me.tutorialText.text = LocalizationUtil.FindLocalizationEntry(entry.body, string.Empty, false, TableCategory.UI);
            me.headerText.text = LocalizationUtil.FindLocalizationEntry(entry.header, string.Empty, false, TableCategory.UI);
            me.fullscreenCutout.Clear();
            me.showingEntry = entry;
            me.transform.localPosition = entry.popupPosition;

            // setup cutout
            foreach (var c in entry.highlightUIObjects)
            {
                if (c.activeInHierarchy)
                {
                    var graphic = c.GetComponentInChildren<Graphic>();
                    if (graphic)
                    {
                        me.fullscreenCutout.AddCutout(graphic);
                    }
                }
            }

            // other conditions
            //if (entry.highlightActiveMapEntries)
            //{
            //    AddCutouts(GetMapEntries(false, false), me.fullscreenCutout);
            //}
            //else if (entry.highlightMapBossEntry)
            //{
            //    AddCutouts(GetMapEntries(false, true), me.fullscreenCutout);
            //}
            //else if (entry.highlightMapPuzzleEntries)
            //{
            //    AddCutouts(GetMapEntries(true, false), me.fullscreenCutout);
            //}

            if (entry.highlightAnyFindableObject)
            {
                Debug.Log("Highlight Any findable object...");
                SpriteRenderer objectToHighlight = null;
                TextMeshProUGUI objectToHighlightText = null;

                if (HOGameController.instance.gameLogic is HOLogicStandard == false) return;

                var activeItems = HOGameController.instance.GetActiveItems();

                activeItems = activeItems.OrderBy(x => UnityEngine.Random.value).ToList();

                foreach (var item in activeItems)
                {
                    var renderer = item.GetComponent<SpriteRenderer>();
                    HOMainUI mainUI = UIController.instance.hoMainUI;
                    Vector3 screenPos = HOGameController.instance.hoCamera.WorldToScreenPoint(renderer.transform.position);

                    if (screenPos.y < (Screen.height / 3))
                    {
                        HOStandardUI standardUI = mainUI.CurrentSubUI as HOStandardUI;
                        objectToHighlightText = standardUI.GetItemHolder(item).GetComponent<TextMeshProUGUI>();
                        objectToHighlight = renderer;
                        break;
                    }
                        
                }
                if(objectToHighlightText)
                {
                    me.fullscreenCutout.AddCutout(objectToHighlightText);
                }

                if (objectToHighlight)
                {
                    Debug.Log(objectToHighlight.name);
                    AddCutouts(new SpriteRenderer[] { objectToHighlight }, me.fullscreenCutout);
                }
                else
                    Debug.Log("No Object to highlight");
                    

                me.RepositionPopup(new SpriteRenderer[] { objectToHighlight });
            }
            else if (entry.highlightDoorObject)
            {
                var activeDoor = HOGameController.instance.GetActiveDoorHandler();

                List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

                var door = activeDoor.closedState.GetComponent<SpriteRenderer>();
                var key = activeDoor.keyItem.GetComponent<SpriteRenderer>();

                spriteRenderers.Add(door);
                spriteRenderers.Add(key);

                AddCutouts(spriteRenderers.ToArray(), me.fullscreenCutout);

                me.RepositionPopup(spriteRenderers.ToArray());
            }

            if (entry.highlightUIEntriesForSubsceneObjects)
            {
                AddCutouts(GetUIEntriesForSubsceneObjects(), me.fullscreenCutout);
            }

            //if(entry.highlightTriviaObject)
            //{
            //    AddCutouts(GetTriviaKey(), me.fullscreenCutout);
            //    me.RepositionPopup(GetTriviaKey());
            //}

            me.Show();
        }
    }
}

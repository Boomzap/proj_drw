using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.EventSystems;

namespace ho
{
    public class UIController : SimpleSingleton<UIController>
    {
        [Header("Constant UI Components (assigned at runtime)")]
        [ReadOnly]  public          HOMainUI    hoMainUI;
        [ReadOnly]  public          MainMenuUI  mainMenuUI;
        [SerializeField, ReadOnly]  FadeUI      fadeUI;
        [ReadOnly]  public          MinigameUI  minigameUI;
        [ReadOnly]  public          ChapterUI   chapterUI;
        //[ReadOnly]  public          MapUI       mapUI;
        [ReadOnly]  public          ConversationUI conversationUI;
        [ReadOnly]  public          JournalUI journalUI;
        [ReadOnly]  public          UnlimitedUI unlimitedUI;
        [ReadOnly]  public          AchievementUI achievementUI;
        [ReadOnly]  public          CreditsUI creditsUI;
        [ReadOnly]  public          SouvenirUI souvenirUI;
        [ReadOnly]  public          WallpaperUI wallpaperUI;
        [ReadOnly]  public          SoundtrackUI soundtrackUI;

        [ReadOnly]  public          PopupBlackout popupBlackout;

        [SerializeField, ReadOnly]  BaseUI[]    childUIs = null;

        [Header("Initialize me")]
        public                      DarkenWithMask darkenWithMaskMain;
        public                      DarkenWithMask darkenWithMaskPopup;
        public                      Canvas popupCanvas;
        public                      Canvas scoreCanvas;

        public                      AudioClip defaultClickAudio;

        Popup[]                                 childPopups = null;
        public bool                 isFading { get { return fadeUI.gameObject.activeInHierarchy; } }

        public bool                 isShowEndCredits = false;

        public                      Material grayscale;
        public Texture2D mouseDoorCursor;

        public MagCamDisplay magCamDisplay;

        public EventSystem eventSystem { get; private set; }

       

        public bool isPointerOverUIObject
        {
            get
            {
                return isUIInputDisabled ? false : eventSystem.IsPointerOverGameObject();
            }
        }

        public bool isPointerOverButton
        {
            get
            {
	            PointerEventData pe = new PointerEventData(eventSystem);
		        pe.position =  Input.mousePosition;
		 
		        List<RaycastResult> hits = new List<RaycastResult>();
		        eventSystem.RaycastAll( pe, hits );
		        foreach (var t in hits)
		        {
                    if (t.gameObject.GetComponent<Button>()) return true;
		        }
		        return false;                
            }
        }

        public bool isUIInputDisabled
        {
            get
            {
                return !eventSystem.gameObject.activeSelf;
            }

            set
            {
                eventSystem.gameObject.SetActive(!value);
            }
        }

        public int activePopupCount
        {
            get
            {
                if (childPopups == null) return 0;

                int i = 0;
                foreach (var p in childPopups)
                {
                    if (p.gameObject.activeInHierarchy)
                        i++;
                }

                return i;
            }
        }

        public bool hasActivePopup
        {
            get
            {
                if (childPopups == null) return false;

                foreach (var p in childPopups)
                {
                    if (p.gameObject.activeInHierarchy)
                    {
                        //Debug.Log(p.gameObject.name);
                        return true;
                    }
                }

                return false;
            }
        }

        public static float scaleFactor
        {
            get
            {
                const float native_width = 1920.0f;
		        const float native_height = 1080.0f; 
		        const float native_aspect = native_width / native_height;
		        float currentAspect = Screen.width / Screen.height;
		        if (currentAspect >  native_aspect)
		        {
			        return Screen.width / native_width;
		        } else
		        {
			        return Screen.height / native_height;
		        }
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            hoMainUI = GetComponentInChildren<HOMainUI>(true);
            fadeUI = GetComponentInChildren<FadeUI>(true);
            mainMenuUI = GetComponentInChildren<MainMenuUI>(true);
            minigameUI = GetComponentInChildren<MinigameUI>(true);
            chapterUI = GetComponentInChildren<ChapterUI>(true);
            //mapUI = GetComponentInChildren<MapUI>(true);
            conversationUI = GetComponentInChildren<ConversationUI>(true);
            journalUI = GetComponentInChildren<JournalUI>(true);
            unlimitedUI = GetComponentInChildren<UnlimitedUI>(true);
            achievementUI = GetComponentInChildren<AchievementUI>(true);
            creditsUI = GetComponentInChildren<CreditsUI>(true);
            popupBlackout = GetComponentInChildren<PopupBlackout>(true);
            souvenirUI = GetComponentInChildren<SouvenirUI>(true);
            wallpaperUI = GetComponentInChildren<WallpaperUI>(true);
            soundtrackUI = GetComponentInChildren<SoundtrackUI>(true);

            // nb childUIs includes popups
            childUIs = GetComponentsInChildren<BaseUI>(true);

            foreach (BaseUI u in childUIs)
            {
                u.gameObject.SetActive(false);
                u.Init();
            }

            childPopups = GetComponentsInChildren<Popup>(true);

            eventSystem = EventSystem.current;

            fadeUI.transform.SetAsLastSibling();
            //fadeUI.Show(true);
        }

        public void HideAll(bool instant = true)
        {
            foreach(BaseUI ui in childUIs)
            {
                if (ui == fadeUI) continue;

                ui.Hide(instant);
            }
        }

        public void FadeIn(BaseUI.ShowHideCallback andThen = null)
        {
            fadeUI.gameObject.SetActive(true);

            fadeUI.onHiddenOneshot += andThen;
            fadeUI.onHiddenOneshot += () => isUIInputDisabled = false;
            fadeUI.Hide();
        }

        public void FadeOut(BaseUI.ShowHideCallback andThen = null)
        {
            fadeUI.onShownOneshot = null;            
            fadeUI.onShownOneshot += andThen;
            fadeUI.Show();
            isUIInputDisabled = true;
        }

        IEnumerator DelayActionCor(float delayTime, UnityAction action)
        {   
            yield return new WaitForSeconds(delayTime);
            action?.Invoke();
        }

        public void DelayAction(float delayTime, UnityAction action)
        {
            StartCoroutine(DelayActionCor(delayTime, action));
        }

        public T GetPopup<T>() where T : Popup
        {
            T popup = childPopups.Single(x => x.GetType() == typeof(T)) as T;

            return popup;
        }
    }
}
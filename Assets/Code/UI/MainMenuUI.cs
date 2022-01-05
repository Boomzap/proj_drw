using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

namespace ho
{
    public partial class MainMenuUI : BaseUI
    {
        [Header("Main Buttons")]
        [SerializeField] Button storyButton;
        [SerializeField] public Button bonusButton;
        [SerializeField] public Button unlimitedButton;

        [Header("Bottom Row 1 Buttons")]
        [SerializeField] Button journalButton;
        [SerializeField] Button achievementsButton;
        [SerializeField] Button souvenirsButton;
        [SerializeField] Button soundtrackButton;
        [SerializeField] Button wallpapersButton;

        [Header("Bottom Row 2 Buttons")]
        [SerializeField] Button helpButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button otherGamesButton;
        [SerializeField] Button exitGamesButton;

        [Header("Profile Button")]
        [SerializeField] Button  profileBtn;
        [SerializeField] TextMeshProUGUI profileName;

        [Header("Others")]
        [SerializeField] AudioClip   mainMenuMusic;
        //[SerializeField] AudioClip   ambientMusic;
        [SerializeField] SkeletonGraphic miaGraphic;
        [SerializeField] GameObject ceImage;
        [SerializeField] GameObject bfgCEImage;

        [SerializeField] TextMeshProUGUI versionText;
        
        Spine.Slot miaMouth;

        public AudioClip MainMenuMusic => mainMenuMusic;

        //public AudioClip AmbientMusic => ambientMusic;

        bool canPlayCEBonus => GameController.save.currentProfile.flags.HasFlag("story_complete");

        private void Awake()
        {
            Init();
        }
        public override void Init()
        {
            //miaMouth = miaGraphic.Skeleton.FindSlot("mouth_talking");
            //miaMouth?.SetColor(new Color32(0, 0, 0, 0));

            storyButton.onClick.RemoveAllListeners();
            storyButton.onClick.AddListener(OnPlayStory);

            bonusButton.onClick.RemoveAllListeners();
            bonusButton.onClick.AddListener(OnPlayBonus);

            unlimitedButton.onClick.RemoveAllListeners();
            unlimitedButton.onClick.AddListener(OnPlayUnlimited);

            //Bottom Row 1 Buttons

            journalButton.onClick.RemoveAllListeners();
            journalButton.onClick.AddListener(OnPhotoJournal);

            achievementsButton.onClick.RemoveAllListeners();
            achievementsButton.onClick.AddListener(OnAchievements);

            souvenirsButton.onClick.RemoveAllListeners();
            souvenirsButton.onClick.AddListener(OnSouvenirs);

            soundtrackButton.onClick.RemoveAllListeners();
            soundtrackButton.onClick.AddListener(OnSoundtrack);

            wallpapersButton.onClick.RemoveAllListeners();
            wallpapersButton.onClick.AddListener(OnWallpapers);


            //Bottom Row 2 Buttons
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(OnHelp);

            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Popup.ShowPopup<OptionsPopup>());

            otherGamesButton.onClick.RemoveAllListeners();
            otherGamesButton.onClick.AddListener(() => Popup.ShowPopup<OtherGamesPopup>());

            exitGamesButton.onClick.RemoveAllListeners();
            exitGamesButton.onClick.AddListener(() => Application.Quit());


            profileBtn.onClick.RemoveAllListeners();
            profileBtn.onClick.AddListener(() => Popup.ShowPopup<ProfilePopup>());

            //Disable Other Games on Gamehouse, Denda & Gamigo
            bool shouldShowOtherGames =
                SystemSaveContainer.instance.Vendor.Contains("gamehouse") == false &&
                SystemSaveContainer.instance.Vendor.Contains("denda") == false &&
                SystemSaveContainer.instance.Vendor.Contains("gamigo") == false &&
                SystemSaveContainer.instance.Vendor.Contains("alawar") == false;

            bool isBigfish = SystemSaveContainer.instance.Vendor.Contains("bigfish");


            otherGamesButton.gameObject.SetActive(shouldShowOtherGames);
            bonusButton.gameObject.SetActive(false);
#if CE_BUILD
            ceImage.gameObject.SetActive(isBigfish == false);
            bfgCEImage.gameObject.SetActive(isBigfish);
            bonusButton.gameObject.SetActive(false);
#endif

#if SURVEY_BUILD
                versionText.text = "Version " + Application.version + " survey";
#elif CE_BUILD
            versionText.text = "Version " + Application.version + " CE";
#else
                versionText.text = "Version " + Application.version;
#endif

#if DEVELOPMENT_BUILD
                versionText.text += " dev";
#endif

            //Disable UI Buttons that is still under development

            //
            //journalButton.interactable = false;
            //achievementsButton.interactable = false;
            //souvenirsButton.interactable = false;
            //soundtrackButton.interactable = false;
            //wallpapersButton.interactable = false;
            //helpButton.interactable = false;
        }

        public void UpdatedActiveButtons()
        {
            bonusButton.interactable = GameController.save.canPlayCEContent && canPlayCEBonus;

            unlimitedButton.interactable = GameController.instance.isFirstHOSceneUnlocked;
        }

        #region Play Buttons
        void OnPlayStory()
        {
            //GameController.instance.isFreeplay = false;
            //GameController.instance.FadeToHOGame(GameController.instance.gameChapters[0].sceneEntries[0]);

            //Load Chapter Menu for now
            onHiddenOneshot += () => GameController.instance.FadeToChapterMenu();
            Hide();
        }

        void OnPlayBonus()
        {
            //Show Bonus Chapters
            onHiddenOneshot += () => GameController.instance.FadeToChapterMenu(false);
            Hide();
        }
        void OnPlayUnlimited()
        {
            //Show Unlimited Play UI
            onHiddenOneshot += () => UIController.instance.unlimitedUI.Show();
            Hide();
        }
        #endregion

        #region Bottom - Row 1 Buttons
        void OnPhotoJournal()
        {
            onHiddenOneshot += () => UIController.instance.journalUI.Show();
            Hide();
        }

        void OnAchievements()
        {
            //Show Achievements UI/World
            onHiddenOneshot += () => UIController.instance.achievementUI.Show();
            Hide();
        }

        void OnSouvenirs()
        {
            //Show souvenirs UI/World
            onHiddenOneshot += () => UIController.instance.souvenirUI.Show();
            Hide();
        }

        void OnSoundtrack()
        {
            //Show Soundtracks UI
            onHiddenOneshot += () => UIController.instance.soundtrackUI.Show();
            Hide();
        }

        void OnWallpapers()
        {
            //Show Wallpapers UI
            onHiddenOneshot += () => UIController.instance.wallpaperUI.Show();
            Hide();
        }
        #endregion

        #region Bottom - Row 2 Buttons
        void OnHelp()
        {
            onHiddenOneshot += () => Popup.ShowPopup<TutorialSettingPopup>( () => UIController.instance.mainMenuUI.Show());
            Hide();
            //Show Help UI
        }

        #endregion

        private void Start()
        {
            UpdateWelcomeName();
            HOGameController.instance.currentDifficulty = (HODifficulty)GameController.save.currentProfile.hoDifficultyIndex;
        }


        protected override void OnBeginHide(bool instant)
        {
            if (onHideAnimation && !instant)
            {
#if CE_BUILD
                gameObject.PlayAnimation(this, "mainmenu_out_ce", () => OnFinishHide());
#else
                gameObject.PlayAnimation(this, onHideAnimation.name, () => OnFinishHide());
#endif
            }
            else
            {
                OnFinishHide();
            }
        }

        protected override void OnFinishShow()
        {
            if(GameController.instance.isFirstHOSceneUnlocked)
                Tutorial.TriggerTutorial(Tutorial.Trigger.UnlimitedUnlocked);

            Audio.instance.PlayMusic(mainMenuMusic);
            Audio.instance.PlayAmbient(null);

            base.OnFinishShow();
        }

        protected override void OnBeginShow(bool instant)
        {
            UpdatedActiveButtons();

            gameObject.SetActive(true);

            if (onShowAnimation && !instant)
            {
#if CE_BUILD
                gameObject.PlayAnimation(this, "mainmenu_in_ce", () => OnFinishShow());
#else
                gameObject.PlayAnimation(this, onShowAnimation.name, () => OnFinishShow());
#endif
            }
            else
            {
                OnFinishShow();
            }
        }

        public void UpdateProfileName()
        {
            if (GameController.save.currentProfile != null)
                UpdateWelcomeName();
            else
                Debug.LogWarning("Current Profile not found");
        }

        void UpdateWelcomeName()
        {
            //string welcome = LocalizationUtil.FindLocalizationEntry("Welcome", string.Empty, false, TableCategory.UI);
            profileName.text = $"{GameController.save.currentProfile.playerName}!";
        }
    }
}

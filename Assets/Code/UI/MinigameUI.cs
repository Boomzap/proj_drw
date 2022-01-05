using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace ho
{
    public class MinigameUI : BaseUI
    {
        [Header("UI Components")]
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] TextMeshProUGUI progressDescText;
        [SerializeField] TextMeshProUGUI instructionText;
        [SerializeField] TextMeshProUGUI briefText;
        [SerializeField] RepairToolHolder toolHolder;
        [SerializeField] PaintColorHolder paintColorHolder;
        [SerializeField] SewColorHolder sewColorHolder;

        [SerializeField] HOHintButton hintButton;
        [SerializeField] Button skipButton;
        [SerializeField] Image skipButtonFill;
        [SerializeField] Image skipButtonBacking;

        //[SerializeField] Button mapButton;
        [SerializeField] Button menuButton;
        [SerializeField] Button settingsButton;

        [SerializeField] RectTransform bottomPanel;
        
        [SerializeField] RectTransform safeZone;

        [SerializeField] RectTransform mainPanel;
        public RectTransform SafeZone => safeZone;

        public Canvas       canvas { get{ return GetComponent<Canvas>(); } }

        public float hintCooldownTime = 10f;
        public float hintCooldownProgress = 0f;
        float hintUseTime = 0f;

        public bool isHintReady = true;
        public bool isSkipReady = false;

        public PaintColorHolder PaintColorHolder { get { return paintColorHolder; } }

        public SewColorHolder SewColorHolder { get { return sewColorHolder; } }

        // Start is called before the first frame update
        void Awake()
        {
            //mapButton.onClick.RemoveAllListeners();
            //mapButton.onClick.AddListener(OnMapButton);
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuButton);
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Popup.ShowPopup<OptionsPopup>());
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipButton);
        }

        //void OnMapButton()
        //{
        //    GenericPromptPopup prompt = Popup.ShowPopup<GenericPromptPopup>();

        //    string returnToMapHeader = LocalizationUtil.FindLocalizationEntry("ReturnToChapter", "", false, TableCategory.UI);
        //    string returnToMapPrompt = LocalizationUtil.FindLocalizationEntry("ReturnToChapterPrompt", "", false, TableCategory.UI);

        //    prompt.Setup(returnToMapHeader, returnToMapPrompt, PrompType.Options);
        //    prompt.onHiddenOneshot += () =>
        //    {
        //        if (prompt.isConfirmed)
        //        {
        //            GameController.instance.FadeToMap();
        //        }
        //    };
        //}

        void OnMenuButton()
        {
            GenericPromptPopup prompt = Popup.ShowPopup<GenericPromptPopup>();

            string returnToMapHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToChapter_header", "", false, TableCategory.UI);
            string returnToMapPrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToChapter_body", "", false, TableCategory.UI);

            prompt.Setup(returnToMapHeader, returnToMapPrompt, PrompType.Options);
            prompt.onHiddenOneshot += () =>
            {
                if (prompt.isConfirmed)
                {
                    GameController.instance.FadeToChapterMenu(GameController.instance.storyModeOpened);
                }
            };
        }

        private void OnEnable()
        {
           
        }

        public void UpdateSkip(float a)
        {
            if (isSkipReady) return;
            //float mw = skipButtonBacking.rectTransform.sizeDelta.x;
            //float p = Mathf.Clamp01(a);


            //skipButtonFill.rectTransform.sizeDelta = new Vector2(mw * p, skipButtonFill.rectTransform.sizeDelta.y);

            //Reverse fill amount here
            float fillAmount = 1 - a;
            skipButtonFill.fillAmount = fillAmount;

            isSkipReady = fillAmount <= 0 ;
            //skipButton.interactable = a >= 1f;
        }

        void OnSkipButton()
        {
            if (isSkipReady == false) return;

            isSkipReady = false;
            skipButtonFill.fillAmount = 1f;
            MinigameController.instance.OnSkip();
        }


        // Update is called once per frame
        void Update()
        {
            if (MinigameController.instance.ActiveMinigame == null) return;
            RefreshProgressText();
        }

        void RefreshProgressText()
        {
            if (MinigameController.instance.ActiveMinigame == null) return;

            float prog = MinigameController.instance.ActiveMinigame.GetCompletionProgress(out bool showAsPercent);

            if (showAsPercent)
            {
                progressText.text = $"{(int)(prog * 100f)}%";
            }
            else
            {
                progressText.text = $"{(int)prog}";
            }
        }

        public void GetHintWidgetPosSize(Canvas forCanvas, out Vector2 maskPos, out Vector2 maskSize)
        {
            RectTransform hintBtnTransform = hintButton.transform as RectTransform;
            
            Vector3[] corners = new Vector3[4];
            hintBtnTransform.GetWorldCorners(corners);

            maskPos = RectTransformUtility.WorldToScreenPoint(forCanvas.worldCamera, (corners[2] + corners[0]) * 0.5f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(forCanvas.transform as RectTransform, maskPos, null, out maskPos);

            float maxDim = Mathf.Max(hintBtnTransform.rect.size.x, hintBtnTransform.rect.size.y) * hintBtnTransform.lossyScale.x;
            maskSize = new Vector2(maxDim, maxDim);
            
        }

        protected override void OnBeginShow(bool instant)
        {
            mainPanel.anchoredPosition = new Vector2(0f, -255f);
            base.OnBeginShow(instant);

            paintColorHolder.gameObject.SetActive(false);
            sewColorHolder.gameObject.SetActive(false);
            toolHolder.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);

            if(MinigameController.instance.ActiveMinigame is RepairMG)
            {
                toolHolder.gameObject.SetActive(true);
            }
            else if(MinigameController.instance.ActiveMinigame is PaintMG)
            {
                paintColorHolder.gameObject.SetActive(true);
            }
            else if (MinigameController.instance.ActiveMinigame is SewMG)
            {
                sewColorHolder.gameObject.SetActive(true);
            }
            else
            {
                instructionText.gameObject.SetActive(true);
            }
            //gameObject.SetToFirstFrameOfAnimation("mg_in");
        }
         
        public void Initialize(MinigameBase mgBase)
        {
            //Note: Set Term later for different language switch
            //briefText.text = LocalizationUtil.FindLocalizationEntry(mgBase.GetBriefText());

            instructionText.text = LocalizationUtil.FindLocalizationEntry(mgBase.GetInstructionText(), string.Empty, false, TableCategory.UI);

            RefreshProgressText();
            progressDescText.text = LocalizationUtil.FindLocalizationEntry(mgBase.GetProgressDescription(), string.Empty, false, TableCategory.UI);

            hintCooldownProgress = 0f;
            isHintReady = false;
            hintUseTime = Time.time;
            isSkipReady = false;

            // skipButton.interactable = false;
        }

        public void PlayHintAttract()
        {
            hintButton.transform.parent.gameObject.PlayAnimation(this, "hint_attract");
        }

        public void OnHintUsed()
        {

            //hintCooldownProgress = 0f;
            //isHintReady = false;
            //hintUseTime = Time.time;
            MinigameController.instance.isHintUsedOnce = true;
            MinigameController.instance.ActiveMinigame.PlayHint();
        }


        public void OnHintReady()
        {
            hintButton.OnHintReady();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);

            Boomzap.Character.CharacterInfo charEntry = null;
            if (MinigameController.instance.ChapterEntry != null)
                charEntry = MinigameController.instance.ChapterEntry.hintCharacter;


            if (charEntry == null)
                charEntry = Boomzap.Character.CharacterManager.instance.characters[0];

            hintButton.SetCharacter(charEntry);
            hintButton.DisableHint();
        }

        public RepairToolHolder GetRepairToolHolder()
        {
            return toolHolder;
        }
    }
}
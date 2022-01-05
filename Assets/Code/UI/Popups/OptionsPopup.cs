using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;

namespace ho
{
    public class OptionsPopup : Popup
    {
        [SerializeField] TMP_Dropdown       resolutionDropdown;
        [SerializeField] Toggle             fullScreenToggle;
        [SerializeField] Toggle             skipDialogToggle;
        [SerializeField] Slider             sfxVolumeSlider;
        [SerializeField] Slider             musicVolumeSlider;
        [SerializeField] Slider             ambientVolumeSlider;
        [SerializeField] Button             closeButton;
        [SerializeField] Button             creditsButton;
        [SerializeField] Button             mainMenuButton;
        [SerializeField] TMP_Dropdown       languageDropdown;
        [SerializeField] TextMeshProUGUI    warningLabel;

        List<Resolution>                    resolutionList;

        int previousWindowWidth = 1920, previousWindowHeight = 1080;

        void UpdateResolutionDropdownSelectedIdxToCurrent()
        {
            int curResIdx = 0;
            for (int i = 0; i < resolutionList.Count; i++)
            {
                if (resolutionList[i].width == Screen.width &&
                    resolutionList[i].height == Screen.height)
                {
                    curResIdx = i;
                    break;
                }
            }

            resolutionDropdown.SetValueWithoutNotify(curResIdx);
        }

        protected override void OnBeginShow(bool instant)
        {
            base.OnBeginShow(instant);

            resolutionDropdown.ClearOptions();

            resolutionList = new List<Resolution>();

            // add some valid resolutions / remove if not needed
            resolutionList.Add(new Resolution{ width = 1024, height = 768, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1280, height = 800, refreshRate = Screen.currentResolution.refreshRate });
            //resolutionList.Add(new Resolution{ width = 1280, height = 1024, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1280, height = 720, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1366, height = 768, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1440, height = 900, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1600, height = 900, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1680, height = 1050, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 1920, height = 1080, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 2560, height = 1080, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 2560, height = 1440, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution{ width = 3440, height = 1440, refreshRate = Screen.currentResolution.refreshRate });

            resolutionList = resolutionList.Where(x => x.height <= Screen.currentResolution.height && x.width <= Screen.currentResolution.width).ToList();

//             Debug.Log($"Current resolution: {Screen.currentResolution}");
//             foreach (var reso in resolutionList)
//             {
//                 Debug.Log($"{reso}");    
//             }

            var resolutionStringList = resolutionList.Select(x => $"{x.width} x {x.height}").ToList();

            resolutionDropdown.AddOptions(resolutionStringList);
            UpdateResolutionDropdownSelectedIdxToCurrent();

            fullScreenToggle.isOn = GameController.systemSave.fullscreen;
            fullScreenToggle.onValueChanged.RemoveAllListeners();
            fullScreenToggle.onValueChanged.AddListener(OnToggleFullscreen);

            skipDialogToggle.isOn = GameController.systemSave.skipDialog;
            skipDialogToggle.onValueChanged.RemoveAllListeners();
            skipDialogToggle.onValueChanged.AddListener(OnToggleSkipDialog);

            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnChangeResolution);

            if (!Screen.fullScreen)
            {
                previousWindowWidth = Screen.width;
                previousWindowHeight = Screen.height;
            }

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());

            creditsButton.onClick.RemoveAllListeners();
            creditsButton.onClick.AddListener(() =>
                {
                    UIController.instance.creditsUI.Credits.isFromOptions = true;
                    GameController.instance.FadeToCredits();
                }
             );

            
            languageDropdown.ClearOptions();

            var languages = LocalizationSettings.AvailableLocales.Locales.Select(x => x.LocaleName).ToList();

            //languageDropdown.AddOptions(languages);
            languageDropdown.AddOptions(new List<string>() { languages[0] });

            int languageIdx = GameController.systemSave.languageIndex;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIdx];

            languageDropdown.SetValueWithoutNotify(languageIdx);
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageChange);
            
            sfxVolumeSlider.value = GameController.systemSave.audioVolume;
            musicVolumeSlider.value = GameController.systemSave.musicVolume;
            ambientVolumeSlider.value = GameController.systemSave.ambientVolume;

            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChange);

            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChange);

            ambientVolumeSlider.onValueChanged.RemoveAllListeners();
            ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChange);

            languageDropdown.interactable = GameController.instance.canChangeResolution;
            warningLabel.gameObject.SetActive(!GameController.instance.canChangeResolution);
            resolutionDropdown.interactable = (GameController.instance.canChangeResolution && !GameController.systemSave.fullscreen);
            fullScreenToggle.interactable = GameController.instance.canChangeResolution;

            mainMenuButton.gameObject.SetActive(
                !UIController.instance.mainMenuUI.gameObject.activeInHierarchy &&
                 !UIController.instance.chapterUI.gameObject.activeInHierarchy
                 //!UIController.instance.mapUI.gameObject.activeInHierarchy
                 );

            creditsButton.gameObject.SetActive(
                !GameController.instance.inGameplay &&
                //!UIController.instance.mapUI.gameObject.activeInHierarchy &&
                !UIController.instance.chapterUI.gameObject.activeInHierarchy);

#if SURVEY_BUILD
            creditsButton.interactable = false;
#else
            creditsButton.interactable = true;
#endif

            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => { Hide(); GameController.instance.FadeToGameMenu(); });
        }

        void OnSFXVolumeChange(float newValue)
        {
            GameController.systemSave.audioVolume = newValue;
            GameController.systemSave.isDirty = true;

            Audio.instance.UpdateSound();
        }

        void OnMusicVolumeChange(float newValue)
        {
            GameController.systemSave.musicVolume = newValue;
            GameController.systemSave.isDirty = true;

            Audio.instance.UpdateSound();
        }

        void OnAmbientVolumeChange(float newValue)
        {
            GameController.systemSave.ambientVolume = newValue;
            GameController.systemSave.isDirty = true;

            Audio.instance.UpdateSound();
        }

        void OnLanguageChange(int idx)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[idx];
            GameController.systemSave.languageIndex = idx;
            GameController.systemSave.isDirty = true;
        }

        void OnChangeResolution(int idx)
        {
//             previousWindowWidth = resolutionList[idx].width;
//             previousWindowHeight = resolutionList[idx].height;
// 
//             Screen.SetResolution(previousWindowWidth, previousWindowHeight, false);

            DoResolutionChangeCheck(resolutionList[idx].width, resolutionList[idx].height);
        }

        void OnResolutionChangeConfirm(int w, int h)
        {
            previousWindowWidth = w;
            previousWindowHeight = h;

            Screen.SetResolution(w, h, false);
        }

        void OnResolutionChangeCancel()
        {
            UpdateResolutionDropdownSelectedIdxToCurrent();
        }

        void OnToggleSkipDialog(bool skipDialog)
        {
            GameController.systemSave.skipDialog = skipDialog;
            GameController.systemSave.isDirty = true;
        }
        void OnToggleFullscreen(bool fullScreen)
        {
            GameController.systemSave.fullscreen = fullScreen;
            GameController.systemSave.isDirty = true;

            if (fullScreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
                resolutionDropdown.interactable = false;
            } else
            {
                Screen.SetResolution(previousWindowWidth, previousWindowHeight, FullScreenMode.Windowed);
                resolutionDropdown.interactable = true;
            }
        }

        void DoResolutionChangeCheck(int desiredWidth, int desiredHeight)
        {
            float curRatio = (float)Screen.width / (float)Screen.height;
            float tarRatio = (float)desiredWidth / (float)desiredHeight;

            // if not same ratio, and target ratio is less than curRatio (ie, 16:9 -> 4:3 / 1.77 -> 1.33) we need to reset the scene saves
            // so that the objects are not potentially. it SHOULD be fine going to a wider ratio as we always match height, however if there
            // are issues.. comment out the second part of this clause

            if (!Mathf.Approximately(curRatio, tarRatio) && tarRatio < curRatio)
            {
                int progressCount = 0;
                // have to now check if we have any incomplete scene states..
                foreach (var sceneState in GameController.save.currentProfile.hoSceneStates)
                {
                    if (!sceneState.completed && sceneState.hasSaveState)
                    {
                        progressCount++;
                    }
                }

                //if (progressCount > 0)
                {
                    GenericPromptPopup popup = Popup.ShowPopup<GenericPromptPopup>();


                    string resolutionHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ResolutionLoseProgress_header", "", false, TableCategory.UI);
                    string resolutionPrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ResolutionLoseProgress_body", "", false, TableCategory.UI);

                    resolutionPrompt = resolutionPrompt.Replace("[count]", progressCount.ToString());

                    popup.Setup(resolutionHeader, resolutionPrompt, PrompType.Options);
                    popup.onHiddenOneshot += () => { 
                        if (popup.isConfirmed)
                        {
                            OnResolutionChangeConfirm(desiredWidth, desiredHeight);
                            GameController.save.ResetHOProfile();

                            if(GameController.instance.GetWorldState<MapController>() != null)
                                GameController.instance.FadeToGameMenu();
                        }
                        else OnResolutionChangeCancel(); };

                    return;
                }
            }

            OnResolutionChangeConfirm(desiredWidth, desiredHeight);
        }
    }
}

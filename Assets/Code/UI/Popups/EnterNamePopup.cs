using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace ho
{
    public class EnterNamePopup : Popup
    {
        [SerializeField] TMP_InputField nameField;
        [SerializeField] Button confirmButton;
        [SerializeField] Button closeButton;

        [SerializeField] Toggle skipDialogToggle;
        [SerializeField] Toggle fullScreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;

        public bool isConfirmed;

        int previousWindowWidth = 1920, previousWindowHeight = 1080;

        List<Resolution> resolutionList;

        Color defaultColor;
        private void Start()
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => OnConfirm());

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => OnClosePopup());
        }
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

        void OnToggleFullscreen(bool fullScreen)
        {
            GameController.systemSave.fullscreen = fullScreen;
            GameController.systemSave.isDirty = true;

            if (fullScreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
                resolutionDropdown.interactable = false;
            }
            else
            {
                Screen.SetResolution(previousWindowWidth, previousWindowHeight, FullScreenMode.Windowed);
                resolutionDropdown.interactable = true;
            }
        }

        void OnToggleSkipDialog(bool skipDialog)
        {
            GameController.systemSave.skipDialog = skipDialog;
            GameController.systemSave.isDirty = true;
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

                            if (GameController.instance.GetWorldState<MapController>() != null)
                                GameController.instance.FadeToGameMenu();
                        }
                        else OnResolutionChangeCancel();
                    };

                    return;
                }
            }

            OnResolutionChangeConfirm(desiredWidth, desiredHeight);
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

        protected override void OnBeginShow(bool instant)
        {
            base.OnBeginShow(instant);
            nameField.text = string.Empty;
            closeButton.interactable = GameController.save.profiles.Count > 0;

            resolutionDropdown.ClearOptions();

            resolutionList = new List<Resolution>();

            // add some valid resolutions / remove if not needed
            resolutionList.Add(new Resolution { width = 1024, height = 768, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1280, height = 800, refreshRate = Screen.currentResolution.refreshRate });
            //resolutionList.Add(new Resolution{ width = 1280, height = 1024, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1280, height = 720, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1366, height = 768, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1440, height = 900, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1600, height = 900, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1680, height = 1050, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 1920, height = 1080, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 2560, height = 1080, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 2560, height = 1440, refreshRate = Screen.currentResolution.refreshRate });
            resolutionList.Add(new Resolution { width = 3440, height = 1440, refreshRate = Screen.currentResolution.refreshRate });

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

            skipDialogToggle.isOn = GameController.systemSave.fullscreen;
            skipDialogToggle.onValueChanged.RemoveAllListeners();
            skipDialogToggle.onValueChanged.AddListener(OnToggleSkipDialog);

            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnChangeResolution);

            if (!Screen.fullScreen)
            {
                previousWindowWidth = Screen.width;
                previousWindowHeight = Screen.height;
            }

            resolutionDropdown.interactable = (GameController.instance.canChangeResolution && !GameController.systemSave.fullscreen);
            fullScreenToggle.interactable = GameController.instance.canChangeResolution;
        }

        void OnConfirm()
        {
            //NOTE* Validate Name returns empty if name is valid.
            string errorText = GameController.save.ValidateProfileName(nameField.text);
            if (errorText.Equals("valid") == false)
            {
                string invalidNameHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/InvalidName_header", "", false, TableCategory.UI);

                GenericPromptPopup genericPrompt = Popup.GetPopup<GenericPromptPopup>();
                genericPrompt.Setup(invalidNameHeader, errorText);
                genericPrompt.Show();
            }
            else
            {
                Savegame.Profile newProfile = GameController.save.CreateProfile(nameField.text);
                GameController.save.currentProfile = newProfile;
                Savegame.SetDirty();
                UIController.instance.mainMenuUI.UpdateProfileName();
                isConfirmed = true;
                Hide();
                //Popup.ShowPopup<TutorialSettingPopup>(() =>
                Popup.ShowPopup<DifficultyPopup>(() => UIController.instance.mainMenuUI.Show());
            }
               
        }

        void OnClosePopup()
        {
            isConfirmed = false;
            Hide();
        }
    }
}



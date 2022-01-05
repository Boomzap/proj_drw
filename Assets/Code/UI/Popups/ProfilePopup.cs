using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace ho
{
    public class ProfilePopup : Popup
    {
        [SerializeField] TextMeshProUGUI chapterText;
        [SerializeField] TextMeshProUGUI timePlayedText;
        [SerializeField] TextMeshProUGUI difficultyText;

        [SerializeField] Button addButton;
        [SerializeField] Button closeButton;
        [SerializeField] Button deleteButton;
        [SerializeField] Button changeDifficultyButton;

        [SerializeField] GameObject profileButtonPrefab;
        [SerializeField] Transform scrollViewContent;

        HashSet<ProfileButton> profileButtons = new HashSet<ProfileButton>();

        ProfileButton selectedButton;
        protected override void OnBeginShow(bool instant)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() => OnAddProfile());

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => OnDeleteProfile());

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());

            changeDifficultyButton.onClick.RemoveAllListeners();
            changeDifficultyButton.onClick.AddListener(() => OnChangeDifficulty());

            SetupProfileList();
            OnUpdateProfileUI();
        }

        void SetupProfileList()
        {
            //NOTE* This prevents refilling profilesButtons reference when reopening this popup.
            if(profileButtons.Count == 0)
            {
                foreach(Savegame.Profile profile in GameController.save.profiles)
                {
                    ProfileButton profileButton = CreateProfileButton();

                    //Selects Button 
                    bool isSelected = profile == GameController.save.currentProfile;
                    if (isSelected)
                    {
                        selectedButton = profileButton;
                        selectedButton.SetSelected(true);
                    }

                    profileButton.CurrentProfile = profile;
                }
            } else
            {
                foreach (var button in profileButtons)
                {
                    if (button.CurrentProfile == GameController.save.currentProfile)
                    {
                        button.SetSelected(true);
                        selectedButton = button;
                    }
                }
            }


        }
        ProfileButton CreateProfileButton()
        {
            GameObject newObject = Instantiate(profileButtonPrefab, scrollViewContent);
            ProfileButton profileButton = newObject.GetComponent<ProfileButton>();
            profileButtons.Add(profileButton);
            return profileButton;
        }

        void OnUpdateProfileUI()
        {
            Savegame.Profile currentProfile = GameController.save.currentProfile;
            Savegame.SetDirty();

            long hoursPlayed = currentProfile.timePlayedSeconds / (60 * 60);
            long minutesPlayed = (currentProfile.timePlayedSeconds % (60 * 60)) / 60;

            timePlayedText.text = $"{hoursPlayed}:{minutesPlayed:D2}";

            //chapterText.text = HOUtil.LocStringWithParameters("UI/ChapterN", new string[] { "chapter", (currentProfile.completedChapters.Count + 1).ToString() });// ((I2.Loc.LocalizedString)"UI/Chapter") + " "+ (currentProfile.completedChapters.Count + 1);
            int completedChapters = currentProfile.completedChapters.Count + 1;
            if (completedChapters > GameController.instance.gameChapters.Count)
                completedChapters = GameController.instance.gameChapters.Count;

            chapterText.text = LocalizationUtil.FindLocalizationEntry($"UI/Day{completedChapters}/chapterDisplayName", string.Empty, false, TableCategory.UI);
            difficultyText.text = LocalizationUtil.FindLocalizationEntry($"UI/{((HODifficulty)GameController.save.currentProfile.hoDifficultyIndex).ToString()}", string.Empty, false, TableCategory.UI);
        }

        void OnChangeDifficulty()
        {
            Popup.ShowPopup<DifficultyPopup>(() => OnUpdateProfileUI());
        }

        void OnAddProfile()
        {
            if(GameController.save.profiles.Count >= 4)
            {
                GenericPromptPopup genericPrompt = Popup.GetPopup<GenericPromptPopup>();
                string profileFull = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ProfileFull_header", "", false, TableCategory.UI);
                string profileFullPrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ProfileFull_body", "", false, TableCategory.UI);

                genericPrompt.Setup(profileFull, profileFullPrompt, PrompType.Info);
                Popup.ShowPopup<GenericPromptPopup>();
            }
            else
                Popup.ShowPopup<EnterNamePopup>(() => OnConfirmAdd());
        }

        void OnDeleteProfile()
        {
            GenericPromptPopup genericPrompt = Popup.GetPopup<GenericPromptPopup>();

            string deleteProfile = LocalizationUtil.FindLocalizationEntry("UI/Prompt/DeleteProfile_header", "", false, TableCategory.UI);
            string deleteProfilePrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/DeleteProfile_body", "", false, TableCategory.UI);

            genericPrompt.Setup(deleteProfile, deleteProfilePrompt, PrompType.Options);
            Popup.ShowPopup<GenericPromptPopup>(() => OnConfirmDelete());
        }

        void OnConfirmAdd()
        {
            EnterNamePopup enterPopup = Popup.GetPopup<EnterNamePopup>();

            if (enterPopup.isConfirmed)
            {
                //Create new profile button
                ProfileButton profileButton = CreateProfileButton();
                profileButton.CurrentProfile = GameController.save.currentProfile;
                OnSelectProfile(profileButton);
                selectedButton.OnSelectProfile();
                UIController.instance.mainMenuUI.Hide();
                Hide();
            }
        }

        void OnConfirmDelete()
        {
            GenericPromptPopup genericPrompt = Popup.GetPopup<GenericPromptPopup>();

            if (genericPrompt.isConfirmed)
            {
                //Remove from SaveData
                Savegame.Profile newSelectedProfile = GameController.save.DeleteProfile(selectedButton.CurrentProfile);

                //Clear Currently Selected
                profileButtons.Remove(selectedButton);
                Destroy(selectedButton.gameObject);

                if (newSelectedProfile == null)
                {
                    OnAddProfile();
                }
                else
                {
                    OnSelectProfile(profileButtons.First());
                    selectedButton = profileButtons.First();
                }
            }
        }

        public void OnSelectProfile(ProfileButton profileButton)
        {
            if(selectedButton) selectedButton.SetSelected(false);
            profileButton.SetSelected(true);
            selectedButton = profileButton;

            UIController.instance.mainMenuUI.UpdatedActiveButtons();
            OnUpdateProfileUI();
        }

        public void Reset()
        {
            foreach (ProfileButton profileButton in profileButtons)
                Destroy(profileButton.gameObject);
            profileButtons.Clear();
            Hide();
        }
    }
}


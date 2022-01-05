using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;

namespace ho
{
    public class SoundtrackUI : BaseUI
    {
        [SerializeField] Transform activeMark;

        [SerializeField] List<AudioClip> trackList = new List<AudioClip>();

        [ReadOnly]
        public List<TextMeshProUGUI> trackTexts;

        [ReadOnly, ShowInInspector, BoxGroup("Music Buttons")]
        public List<Button> musicButtons = new List<Button>();

        [SerializeField] Button backButton;
        [SerializeField] Button achievementButton;
        [SerializeField] Button wallpapersButton;

        Button selectedButton;

        AudioClip currentTrack;

        public override void Init()
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
                {
                    onHiddenOneshot += () => UIController.instance.mainMenuUI.Show();
                    Hide();
                }
           );

            achievementButton.onClick.RemoveAllListeners();
            achievementButton.onClick.AddListener(() =>
            {
                onHiddenOneshot += () => UIController.instance.achievementUI.Show();
                Hide();
            }
           );

            wallpapersButton.onClick.RemoveAllListeners();
            wallpapersButton.onClick.AddListener(() =>
            {
                onHiddenOneshot += () => UIController.instance.wallpaperUI.Show();
                Hide();
            }
           );

            musicButtons.ForEach(x =>
            {
                x.onClick.RemoveAllListeners();
                x.onClick.AddListener(() => SetSelectedButton(x));
                gameObject.SetActive(false);
            }
            );

            for(int i = 0; i < musicButtons.Count; i++)
            {
                if(i < trackList.Count)
                {
                    musicButtons[i].gameObject.SetActive(true);
                }
            }
        }
        protected override void OnBeginShow(bool instant)
        {
            Init();
            SetSelectedTrack();
            base.OnBeginShow(instant);
        }

        public void SetSelectedTrack()
        {
            if (selectedButton != null)
            {
                SetSelectedButton(selectedButton);
            }
            else
                SetSelectedButton(musicButtons[0]);

        }

        void SetSelectedButton(Button button)
        {
            selectedButton = button;
            int buttonIndex = musicButtons.IndexOf(button);

            if (currentTrack != trackList[buttonIndex])
            {
                currentTrack = trackList[buttonIndex];
                Audio.instance.PlayMusic(trackList[buttonIndex]);
            }

            activeMark.SetParent(selectedButton.transform, false);
            activeMark.SetAsFirstSibling();
        }

        public void PlayDefaultMusic()
        {
            Audio.instance.PlayMusic(trackList[0]);
        }

#if UNITY_EDITOR


        [Button, BoxGroup("Setup")]
        void SetupTrackNames()
        {
            musicButtons = GetComponentsInChildren<Button>(true).Where(x => x.name.Contains("Music")).ToList();

            trackTexts = GetComponentsInChildren<TextMeshProUGUI>(true)
                .Where(x => x.name.Contains("BGM")).ToList();

            for(int i = 0; i < musicButtons.Count; i++)
            {
                if (i < trackList.Count)
                {
                    //trackTexts[i].text = LocalizationUtil.FindLocalizationEntry($"UI/music_theme_{i}", string.Empty, false, TableCategory.UI);
                    LocalizeStringEvent stringEvent = trackTexts[i].gameObject.GetComponent<LocalizeStringEvent>();

                    if(stringEvent)
                    {
                        stringEvent.StringReference = new LocalizedString() { TableReference = "UI", TableEntryReference = $"UI/music_theme_{i}" };
                    }

                    //stringEvent = trackTexts[i].gameObject.AddComponent<LocalizeStringEvent>();
                   

                    musicButtons[i].gameObject.SetActive(true);
                }
                else
                    musicButtons[i].gameObject.SetActive(false);
                
            }
        }
#endif
    }
}


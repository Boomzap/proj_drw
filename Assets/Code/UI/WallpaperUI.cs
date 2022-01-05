using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace ho
{
    public class WallpaperUI : BaseUI
    {
        [BoxGroup("CE Addressable Content")]
        [SerializeField] protected AssetReferenceSprite[] previewAssets = new AssetReferenceSprite[0];

        [ReadOnly, ShowInInspector, BoxGroup("Preview Buttons")]
        public List<Button> previewButtons = new List<Button>();

        [ReadOnly, ShowInInspector, BoxGroup("Preview Images")]
        public List<Image> previewImages = new List<Image>();

        [BoxGroup("Buttons")]
        [SerializeField] protected Button previousButton;
        [BoxGroup("Buttons")]
        [SerializeField] protected Button nextButton;
        [BoxGroup("Buttons")]
        [SerializeField] protected Button backButton;
        [BoxGroup("Buttons")]
        [SerializeField] protected Button achievementsButton;
        [BoxGroup("Buttons")]
        [SerializeField] protected Button soundtracksButton;

        protected int curViewIndex = 0;

        protected int maxViewIndex
        {
            get
            {

                int index = ((int)(previewAssets.Length / previewImages.Count)) - 1;

                if (previewAssets.Length % previewImages.Count > 0) index++;

                return index;
            }
        }

        [ReadOnly, BoxGroup("Setup")]
        public Transform[] previewTexts;

        protected AsyncOperationHandle<Sprite>[] previewHandles;

        protected Button selectedButton;

        void NextPage()
        {
            curViewIndex++;
            if (curViewIndex > maxViewIndex)
                curViewIndex = 0;

            LoadPreviewAssets();
        }

        void PreviousPage()
        {
            curViewIndex--;
            if (curViewIndex < 0)
                curViewIndex = maxViewIndex;

            LoadPreviewAssets();
        }
        public virtual void SetupPreviewButtons()
        {
            foreach (Button button in previewButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SetSelectedButton(button));
            }
        }

        public override void Init()
        {
            SetupPreviewButtons();

            if (previousButton)
            {
                previousButton.onClick.RemoveAllListeners();
                previousButton.onClick.AddListener(() => PreviousPage());
            }

            if (nextButton)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => NextPage());
            }

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
                {
                    onHiddenOneshot += () => UIController.instance.mainMenuUI.Show();
                    Hide();
                }
            );

            achievementsButton.onClick.RemoveAllListeners();
            achievementsButton.onClick.AddListener(() =>
            {
                onHiddenOneshot += () => UIController.instance.achievementUI.Show();
                Hide();
            }
            );

            soundtracksButton.onClick.RemoveAllListeners();
            soundtracksButton.onClick.AddListener(() =>
            {
                onHiddenOneshot += () => UIController.instance.soundtrackUI.Show();
                Hide();
            }
            );

            if (previewAssets.Length > 0)
            {
                previewHandles = new AsyncOperationHandle<Sprite>[previewAssets.Length];
                for (int i = 0; i < previewHandles.Length; i++)
                {
                    previewHandles[i] = new AsyncOperationHandle<Sprite>();
                }
            }

            EnablePreviewTexts(false);
        }

        protected override void OnBeginShow(bool instant)
        {
            Init();
            LoadPreviewAssets();
            base.OnBeginShow(instant);
        }

        protected virtual void LoadPreviewAssets()
        {
            previewImages.ForEach(x => x.sprite = null);

            for (int i = 0; i < previewButtons.Count; i++)
            {
                int assetIndex = (curViewIndex * previewButtons.Count) + i;

                if (assetIndex < previewAssets.Length)
                {
                    if (previewHandles[assetIndex].IsValid())
                    {
                        Addressables.Release(previewHandles[assetIndex]);
                    }

                    StateCache.instance.LoadAssetTexture(previewAssets[assetIndex], ref previewHandles[assetIndex], previewImages[i], null);
                }

                previewButtons[i].gameObject.SetActive(assetIndex < previewAssets.Length);
            }
        }


        protected override void OnFinishHide()
        {
            CleanUp();
            base.OnFinishHide();
        }

        protected virtual void CleanUp()
        {
            if ((previewHandles == null) || previewHandles.Length <= 0)
            {
                Debug.Log("No handles found!");
                return;
            }

            for (int i = 0; i < previewHandles.Length; i++)
            {
                if (previewHandles[i].IsValid())
                {
                    //StateCache.instance.ReleaseAssetTexture(previewAssets[i]);
                    Addressables.Release(previewHandles[i]);
                    //Debug.Log($"Released {i}");
                }

            }
            //bonusChapterPreviewImage.sprite = null;
        }

        [Button, BoxGroup("Setup")]
        public void EnablePreviewTexts(bool enable)
        {
            if (previewTexts == null)
            {
                Debug.Log("No preview texts found! Please Setup first.");
                return;
            }

            foreach (var previewText in previewTexts)
            {
                previewText.gameObject.SetActive(enable);
            }
        }

        protected virtual void SetSelectedButton(Button button)
        {
            selectedButton = button;

            int buttonIndex = previewButtons.IndexOf(selectedButton);

            if (buttonIndex < 0)
            {
                Debug.Log($"Selected button index not found!");
                return;
            }

            if (buttonIndex > previewAssets.Length)
            {
                Debug.Log($"Preview Image/Asset not Found!");
                return;
            }

            WallpaperPreviewPopup previewPopup = Popup.GetPopup<WallpaperPreviewPopup>();
            previewPopup.Setup(previewImages[buttonIndex].sprite);
            previewPopup.Show();
            //Debug.Log($"Clicked: {selectedButton.name}");
        }

        public AssetReference GetWallpaper(string wallpaperName)
        {
            var wallpapers = previewAssets.Where(x => x.ToString() == wallpaperName).ToArray();

            if (wallpapers.Length > 0) return wallpapers.First();
            return null;
        }


#if UNITY_EDITOR
        [Button, BoxGroup("Setup")]
        public virtual void Setup()
        {
            // Loads All Buttons

            previousButton = GetComponentsInChildren<Button>(true).Where(x => x.name.Contains("Previous")).FirstOrDefault();
            nextButton = GetComponentsInChildren<Button>(true).Where(x => x.name.Contains("Next")).FirstOrDefault();
            backButton = GetComponentsInChildren<Button>(true).Where(x => x.name.Contains("Main Menu")).FirstOrDefault();

            var assetButtonParent = GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Preview Buttons")).First();
            previewButtons = assetButtonParent.GetComponentsInChildren<Button>(true)
                .Where(x =>
                x.name.Contains("Previous") == false &&
                x.name.Contains("Next") == false &&
                x.name.Contains("Back") == false
                ).ToList();

            // Loads Preview Images
            previewImages.Clear();
            foreach (Button button in previewButtons)
            {
                Image previewImage = button.GetComponentsInChildren<Image>().Where(x => x.name.Contains("Preview")).First();
                previewImages.Add(previewImage);
            }

            // Loads Preview Texts for editing
            previewTexts = GetComponentsInChildren<Transform>(true).Where(x => x.name.Contains("DELETE")).ToArray();
        }
    

        [Button, BoxGroup("Setup"), LabelText("Assign addressable tags for CE Content")]
        protected virtual void ConfigureAddressableTags()
        {
            const string seTag = "ExcludeFromSE";

            foreach (var textureAsset in previewAssets)
            {
                GameController.instance.AddAddressableTagByGUID(seTag, textureAsset.AssetGUID, "preview", textureAsset.SubObjectName);
            }
        }
#endif
    }
}
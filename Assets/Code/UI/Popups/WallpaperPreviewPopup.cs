using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace ho
{
    public class WallpaperPreviewPopup : Popup
    {
        [SerializeField] Image previewImage;

        [SerializeField] Button applyButton;
        [SerializeField] Button closeButton;

        protected override bool UseBlackout => false;

        protected override void Awake()
        {
            base.Awake();

            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(() => OnApply());

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());
        }

        public void Setup(Sprite previewSprite)
        {
            previewImage.sprite = previewSprite;
        }


        void OnApply()
        {
            Texture2D texture = previewImage.sprite.texture;
            byte[] bytes = texture.EncodeToPNG();

#if UNITY_EDITOR
            Debug.Log(Savegame.GetPath("game.sav"));
#endif

            string path = Application.persistentDataPath + $"/{texture.name}.png";
            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(path, bytes);

            var popup = GetPopup<GenericPromptPopup>();
            string wallpaperSavedHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/WallpaperSaved_header", string.Empty, false, TableCategory.UI);
            string wallpaperSavedMessage = LocalizationUtil.FindLocalizationEntry("UI/Prompt/WallpaperSaved_body", string.Empty, false, TableCategory.UI);

            popup.Setup(wallpaperSavedHeader, wallpaperSavedMessage +" "+ path, PrompType.Info);
            popup.Show();
        }
    }

}

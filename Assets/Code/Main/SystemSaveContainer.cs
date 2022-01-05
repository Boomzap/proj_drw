using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
// this is so system settings are loaded early and persistent through loading scene
// you can/should still use GameController.systemSave to access it in World scene, but it will 
// be persisted here.

namespace ho
{
    public class SystemSaveContainer : SimpleSingleton<SystemSaveContainer>
    {
        public SystemSave systemSave;

        [SerializeField]
        string vendor;

        public string Vendor
        {
            get { return vendor; }
            set
            {
                vendor = value;
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            Vendor = vendor;
#endif
            systemSave = SystemSave.Load(Savegame.GetPath("system.sav"));

            if(systemSave.fullscreen)
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }

            Audio.instance.UpdateSound();
        }

        private void Start()
        {
            StartCoroutine(PreloadLocalizationCor());
        }


        IEnumerator PreloadLocalizationCor()
        {
            yield return LocalizationSettings.InitializationOperation;

            if (vendor.Contains("denda"))
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("nl");
            }
            else if (vendor.Contains("gamigo"))
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("de");
            }
            else
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[systemSave.languageIndex];
        }
    }
}
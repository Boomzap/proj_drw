using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
#endif
namespace ho
{
    [CreateAssetMenu(fileName = "MinigameTracker", menuName = "HO/Trackers/MinigameTracker", order = 1)]
    public class MinigameTracker : ScriptableObject
    {
        [System.Serializable]
        public class MinigameEntry
        {
            [HideInInspector] public MinigameReference mgReference;
            public Sprite previewSprite;
            public string roomName;
        }

        [InlineProperty]
        public MinigameEntry[] mgEntries;

#if UNITY_EDITOR
        public void InitData()
        {
            // only want to get objects that are actually addressables - this is the way they do it in the Addressables sourcecode,
            // so i assume that this is a valid method.
            List<AddressableAssetEntry> assets = new List<AddressableAssetEntry>();

            if (AddressableAssetSettingsDefaultObject.Settings == null)
                return;


            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(assets, false, null, (AddressableAssetEntry e) =>
            {
                return e.labels.Contains("Minigame") || e.address.ToLower().StartsWith("det");
            });

            mgEntries = assets.Select(x => {
                var entry = new MinigameEntry();
                entry.mgReference = new MinigameReference(x.guid);
                entry.mgReference.roomNameKey = LocalizationUtil.FindLocalizationEntry($"UI/mg_scene_name/{x.address.ToLower()}", string.Empty, true, TableCategory.UI);
                //entry.mgReference.roomDescKey = LocalizationUtil.FindLocalizationEntry($"UI/Minigame/{x.address.ToLower()}_desc", string.Empty, true, TableCategory.UI);
                var s = System.IO.Path.GetDirectoryName(x.AssetPath) + "\\" + x.address + "_room_preview.png";
                entry.previewSprite = AssetDatabase.LoadAssetAtPath<Sprite>(s);
                entry.roomName = x.address;
                return entry;
            }).ToArray();
        }

        [Button]
        private void OnEnable()
        {
            InitData();
        }

#endif

        public Sprite GetPreviewSprite(MinigameReference mgRef)
        {
            var entry = mgEntries.FirstOrDefault(x => mgRef.AssetGUID.Equals(x.mgReference.AssetGUID, System.StringComparison.OrdinalIgnoreCase));

            return entry?.previewSprite ?? null;
        }

        public string GetMGRoomName(MinigameReference mgRef)
        {
            var entry = mgEntries.FirstOrDefault(x => mgRef.AssetGUID.Equals(x.mgReference.AssetGUID, System.StringComparison.OrdinalIgnoreCase));

            return entry?.roomName ?? string.Empty;
        }
    }
}

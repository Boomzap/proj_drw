using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
#endif
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    [CreateAssetMenu(fileName = "RoomTracker", menuName = "HO/Trackers/RoomTracker", order = 1)]
    public class HORoomTracker : ScriptableObject
    {
        [System.Serializable]
        public class RoomEntry
        {
            [HideInInspector] public HORoomReference roomReference;
            public Sprite previewSprite;
            public string roomName;
        }

        [InlineProperty]
        public RoomEntry[] roomEntries;

  

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
                return e.labels.Contains("HO Scene");
            });

            roomEntries = assets.Select(x => {
                var entry = new RoomEntry();
                entry.roomReference = new HORoomReference(x.guid);
                entry.roomReference.roomName = x.address;
                entry.roomReference.roomLocalizationKey = HOUtil.GetRoomLocalizedName(x.address);
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

        string[] filteredPrefixes = new string[] {"det", "odd" };
        public bool IsFilteredPrefix(string roomName)
        {
            var room = roomName.ToLower().Split('_');

            if (room.Length > 2 && room[2].Contains("sub")) return true;

            return filteredPrefixes.Contains(room[0]);
        }

        public string[] GetRoomNames()
        {
            var roomNames = roomEntries.Select(x => x.roomName).Where(y => IsFilteredPrefix(y) == false).OrderBy(x => x).ToList();
            roomNames.Insert(0, "Default");
            return roomNames.ToArray();
        }

#endif
        public bool GetNameFromGUID(string guid, out string roomName)
        {
            var entry = roomEntries.FirstOrDefault(x => guid.Equals(x.roomReference.AssetGUID, System.StringComparison.OrdinalIgnoreCase));

            if (entry == null) 
            {
                roomName = string.Empty;
                return false;
            }

            roomName = entry.roomName;
            return false;
        }

        public HORoomReference GetItemByName(string name)
        {
            var entry = roomEntries.FirstOrDefault(x => name.Equals(x.roomName, System.StringComparison.OrdinalIgnoreCase));

            return entry?.roomReference ?? null;
        }

        public HORoomReference GetItemByGUID(string guid)
        {
            var entry = roomEntries.FirstOrDefault(x => guid.Equals(x.roomReference.AssetGUID, System.StringComparison.OrdinalIgnoreCase));

            return entry?.roomReference ?? null;
        }

        public Sprite GetPreviewSprite(HORoomReference roomRef)
        {
            var entry = roomEntries.FirstOrDefault(x => roomRef.AssetGUID.Equals(x.roomReference.AssetGUID, System.StringComparison.OrdinalIgnoreCase));

            return entry?.previewSprite ?? null;
        }
    }
}
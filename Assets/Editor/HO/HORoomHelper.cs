using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

namespace ho
{
    public class HORoomHelper : MonoBehaviour
    {
        [MenuItem("Assets/Create HORoom from PSB", priority = 1)]
        static void CreateHORoomFromPSB()
        {
            var toCheck = Selection.activeGameObject;
            var path = AssetDatabase.GetAssetPath(toCheck);
            var mainObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (mainObject != null)
            {
                var curPath = Path.GetDirectoryName(path);
                if (!curPath.EndsWith("psb", StringComparison.OrdinalIgnoreCase))
                {
                    EditorUtility.DisplayDialog("Invalid directory", "All psbs for rooms should be in a psb/ folder under the room's root. If this should still work, tell JD.", "OK");
                    return;
                }

                // up a directory
                var assetPath = Path.GetDirectoryName(curPath);
                var assetFile = assetPath + "/" + mainObject.name + ".prefab";
                var existingAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetFile);
                if (existingAsset)
                {
                    EditorGUIUtility.PingObject(existingAsset);
                    Debug.LogWarning("The corresponding room prefab already exists for this PSB. Delete or rename it first if you wish to re-import.");
                    return;
                }

                GameObject newPrefab = new GameObject(mainObject.name);

                RectTransform rectTransform = newPrefab.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(3350f, 1536f);

                HORoom hoRoom = newPrefab.AddComponent<HORoom>();

                var newRoomAsset = PrefabUtility.InstantiatePrefab(mainObject, newPrefab.transform) as GameObject;
                PrefabUtility.RevertPrefabInstance(newRoomAsset, InteractionMode.AutomatedAction);

                hoRoom.SetRoomRoot(newRoomAsset);
                hoRoom.SetupRoom();

                int instanceID = PrefabUtility.SaveAsPrefabAssetAndConnect(newPrefab, assetFile, InteractionMode.AutomatedAction).GetInstanceID();
                AssetDatabase.ImportAsset(assetFile);
                AssetDatabase.Refresh();

                string hoRoomName = hoRoom.name;
                DestroyImmediate(newPrefab);

                var AASettings = AddressableAssetSettingsDefaultObject.Settings;
                if (AASettings)
                {
                    var ScenesGroup = AASettings.DefaultGroup;
                    if (ScenesGroup)
                    {
                        var guid = AssetDatabase.GUIDFromAssetPath(assetFile);
                        var entry = AASettings.CreateOrMoveEntry(guid.ToString(), ScenesGroup, false, false);
                        entry.SetLabel("HO Scene", true);
                        entry.address = hoRoomName;
                        AASettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
                    } else
                    {
                        Debug.LogError("No HO Scenes addressables group?");
                    }
                }

                EditorGUIUtility.PingObject(instanceID);
            }
        }

        [MenuItem("Assets/Create HORoom from PSB", true, 1)]
        static bool ValidateCreateHORoomFromPSB()
        {
            if (Selection.activeObject is GameObject && Selection.activeObject != null)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);

                return path.EndsWith(".psb", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

    }
}

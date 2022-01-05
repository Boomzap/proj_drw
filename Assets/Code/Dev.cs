using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using UnityEditor.U2D.PSD;
using UnityEditor.Presets;
using ho;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEditor.Localization;
#endif

public class Dev : SimpleSingleton<Dev>
{
    #if UNITY_EDITOR
    public Preset  PSDImporterBuildPreset;
    public Preset  PSDImporterDevPreset;
    public Preset  TextureImporterDefaultPreset;

    public AddressableAssetGroup surveyGroup;
    public AddressableAssetGroup seGroup;
    public AddressableAssetGroup ceGroup;

    public Chapter[] chaptersInSurvey = new Chapter[0];
    public Chapter[] chaptersInCE = new Chapter[0];

    public enum ETargetFormat
    {
        Build,
        Dev
    }

    GameObject HasMinigameUsingPSB(string psb)
    {
        foreach (var mg in MinigameController.instance.minigamePrefabs)
        {
            var go = mg.editorAsset;

            var firstRenderer = go.GetComponentInChildren<SpriteRenderer>();
            if (firstRenderer == null) continue;

            if (firstRenderer.sprite == null) continue;

            var path = AssetDatabase.GetAssetPath(firstRenderer.sprite.texture);

            if (path == psb)
                return go;
        }

        return null;
    }

    [Button]
    void AttemptToGetPath(string guid)
    {
        Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
    }

    [Button]
    void ResetTextureImportSettingsForSpineTextures()
    {
        Preset spinePreset = AssetDatabase.LoadAssetAtPath<Preset>(Spine.Unity.Editor.SpineEditorUtilities.Preferences.textureSettingsReference);

        if (spinePreset == null)
        {
            Debug.LogError("Spine settings doesn't have a default texture import preset");
            return;
        }

        var atlasAssets = AssetDatabase.FindAssets("t:spineatlasasset").Select(x => AssetDatabase.GUIDToAssetPath(x));
        foreach (var asset in atlasAssets)
        {
            
            Spine.Unity.AtlasAssetBase atlas = AssetDatabase.LoadAssetAtPath<Spine.Unity.AtlasAssetBase>(asset);

            foreach (var mat in atlas.Materials)
            {
                Texture texture = mat.GetTexture("_MainTex");

                if (texture != null)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;

                    if (importer != null)
                    {
                        spinePreset.ApplyTo(importer);
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    [Button]
    void RegenerateAllSceneSDFs()
    {
        foreach (var sceneRef in HORoomAssetManager.instance.roomTracker.roomEntries)
        {
            sceneRef.roomReference.editorAsset.GetComponent<HORoom>().GenerateSDFs();
        }
    }


    [Button]
    void UpdateASTCTexturesToFast()
    {
        string[] aa = AssetDatabase.GetAllAssetPaths();
        var importers = aa.Where(x => x.ToLower().EndsWith(".png") || x.ToLower().EndsWith(".jpeg") || x.ToLower().EndsWith(".jpg")).Select(x => TextureImporter.GetAtPath(x) as TextureImporter);

        foreach (var importer in importers)
        {
            var settings = importer.GetPlatformTextureSettings("iPhone");
            if (settings.format == TextureImporterFormat.ASTC_4x4)
            {
                settings.format = TextureImporterFormat.ASTC_6x6;
                settings.compressionQuality = 0; // 'fast'

                importer.SetPlatformTextureSettings(settings);

                Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);

                if (t != null && (!Mathf.IsPowerOfTwo(t.width) || !Mathf.IsPowerOfTwo(t.height)))
                    importer.mipmapEnabled = false;

                importer.SaveAndReimport();
            }



        }

        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    [Button]
    void UpdateETC2TexturesToCrunched()
    {
        string[] aa = AssetDatabase.GetAllAssetPaths();
        var importers = aa.Where(x => x.ToLower().EndsWith(".png") || x.ToLower().EndsWith(".jpeg") || x.ToLower().EndsWith(".jpg")).Select(x => TextureImporter.GetAtPath(x) as TextureImporter);

        foreach (var importer in importers)
        {
            var settings = importer.GetPlatformTextureSettings("Android");
            if (settings.format == TextureImporterFormat.ETC2_RGBA8)
            {
                settings.format = TextureImporterFormat.ETC2_RGBA8Crunched;
                settings.compressionQuality = 50;
                importer.SetPlatformTextureSettings(settings);

                Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);

                if (t != null && (!Mathf.IsPowerOfTwo(t.width) || !Mathf.IsPowerOfTwo(t.height)))
                    importer.mipmapEnabled = false;

                importer.SaveAndReimport();
            }



        }

        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    [Button]
    void CheckBadTextures()
    {
        string[] paths = AssetDatabase.FindAssets("t:Texture").Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();

        foreach (var p in paths)
        {
            if (p.Contains(".sdf.")) continue;
            if (!p.StartsWith("Assets")) continue;

            TextureImporter ti = TextureImporter.GetAtPath(p) as TextureImporter;
            if (ti == null) continue;

            var iPhoneSettings = ti.GetPlatformTextureSettings("iPhone");
            var AndroidSettings = ti.GetPlatformTextureSettings("Android");

            if (iPhoneSettings.format != TextureImporterFormat.ASTC_6x6)
            {
                Debug.Log(p);
                continue;
            }

            if (AndroidSettings.format != TextureImporterFormat.ETC2_RGBA8Crunched)
            {
                Debug.Log(p);
                continue;
            }
            
        }
    }
    
    void AddEntry(List<string> guids, Chapter.Entry e)
    {
        if (e.IsHOScene)
        {
            if (guids.Contains(e.hoRoom.AssetGUID)) return;

            guids.Add(e.hoRoom.AssetGUID);
            guids.AddRange(e.hoRoom.editorAsset.GetComponent<HORoom>().subHO.Select(x => x.AssetGUID));
        }
        else
        {
            if (guids.Contains(e.minigame.AssetGUID)) return;

            guids.Add(e.minigame.AssetGUID);
        }
    }

    void AddEntry(List<string> guids, Boomzap.Conversation.Conversation c)
    {
        if (c == null) return;

        foreach (var n in c.GetAllNodes())
        {
            foreach (var ch in n.characters)
            {
                if (ch == null) continue;
                if (ch.noChangeFromParent) continue;
                if (ch.character == null) continue;

                if (guids.Contains(ch.character.characterRef.AssetGUID)) continue;
                
                guids.Add(ch.character.characterRef.AssetGUID);
            }
        }
    }

    [Button, BoxGroup("Addressable Assets Group Setup")]
    void ResetGroups()
    {
        var toMoveCE = ceGroup.entries.Where(x => x.labels.Contains("Minigame") || x.labels.Contains("HO Scene") || x.labels.Contains("Character")).ToList();
        var toMoveSurvey = surveyGroup.entries.Where(x => x.labels.Contains("Minigame") || x.labels.Contains("HO Scene") || x.labels.Contains("Character")).ToList();

        AddressableAssetSettingsDefaultObject.Settings.MoveEntries(toMoveCE, seGroup);
        AddressableAssetSettingsDefaultObject.Settings.MoveEntries(toMoveSurvey, seGroup);
    }

    [Button, BoxGroup("Addressable Assets Group Setup")]
    void SetupSurveyGroup()
    {
        chaptersInSurvey = GameController.instance.gameChapters.Where(x => x.isSurveyContent).ToArray();

       
        List<string> guids = new List<string>();

        foreach (var chapter in chaptersInSurvey)
        {
            foreach (var e in chapter.sceneEntries)
            {
                AddEntry(guids, e);
            }
        }

        foreach (var chapter in chaptersInSurvey)
        {
            //AddEntry(guids, chapter.openChapterConversation);
            //AddEntry(guids, chapter.preBossConversation);
            //AddEntry(guids, chapter.finishChapterConversation);
            
            foreach (var e in chapter.sceneEntries)
            {
                AddEntry(guids, e.onStartConversation);
                AddEntry(guids, e.onEndConversation);
            }
        }


        foreach (var g in guids)
        {
            var existingEntry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(g);

            if (existingEntry == null)
            {
                Debug.LogError("Entry not existing");
                continue;
            }

            if (existingEntry.parentGroup != surveyGroup)
            {
                AddressableAssetSettingsDefaultObject.Settings.MoveEntry(existingEntry, surveyGroup);
            }
        }
    }

    [Button, BoxGroup("Addressable Assets Group Setup")]
    void SetupCEGroup()
    {
        chaptersInCE = GameController.instance.gameChapters.Where(x => x.isCEContent).ToArray();


        List<string> guidsInSEContent = new List<string>();

        foreach (var chapter in GameController.instance.gameChapters.Where(x => !x.isCEContent))
        {
            foreach (var e in chapter.sceneEntries)
            {
                AddEntry(guidsInSEContent, e);
            }
        }

        List<string> guidsInCEContent = new List<string>();

        foreach (var chapter in chaptersInCE)
        {
            foreach (var e in chapter.sceneEntries)
            {
                AddEntry(guidsInCEContent, e);
            }
        }

        List<string> guidsOnlyInCE = guidsInCEContent.Where(x => !guidsInSEContent.Contains(x)).ToList();

        foreach (var g in guidsOnlyInCE)
        {
            var existingEntry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(g);

            if (existingEntry == null)
            {
                Debug.LogError("Entry not existing");
                continue;
            }

            if (existingEntry.parentGroup != ceGroup)
            {
                AddressableAssetSettingsDefaultObject.Settings.MoveEntry(existingEntry, ceGroup);
            }
        }
    }

    [Button, BoxGroup("Addressable Assets Group Setup")]
    void LogGroups()
    {
        string msg = "Survey:\n";
        foreach (var a in surveyGroup.entries)
        {
            msg += $"\t{a.address}\n";
        }
        msg += "\nStandardEdition:\n";
        foreach (var a in seGroup.entries)
        {
            msg += $"\t{a.address}\n";
        }
        msg += "\nCollectorsEdition:\n";
        foreach (var a in ceGroup.entries)
        {
            msg += $"\t{a.address}\n";
        }

        Debug.Log(msg);
    }

    void SetSceneTexturesBulk(ETargetFormat fmt)
    {
        MinigameController.instance.RefreshEditorMinigameList();
    
        string [] aa = AssetDatabase.GetAllAssetPaths();

        var importers = aa.Where(x => x.ToLower().EndsWith(".psb")).Select(x => PSDImporter.GetAtPath(x) as PSDImporter);

        Debug.Log($"Setting up import settings for {importers.Count()} psbs");

        foreach (var importer in importers)
        {
            // we were a bit silly with the minigame setup - we can use this to restore sprite connections after the psb reimport
            var minigame = HasMinigameUsingPSB(importer.assetPath);
            Dictionary<SpriteRenderer, string> d = new Dictionary<SpriteRenderer, string>();

            if (minigame)
            {
                foreach (var r in minigame.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    d.Add(r, r.sprite.name);
                }
            }

            if (fmt == ETargetFormat.Build)
            {
                PSDImporterBuildPreset.ApplyTo(importer);
            }
            else
            {
                PSDImporterDevPreset.ApplyTo(importer);
            }

            importer.SaveAndReimport();

            if (minigame && d.Count > 0)            
            {
                var result = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);

                foreach (var pair in d)
                {
                    var match = result.FirstOrDefault(x => x.name == pair.Value && x is Sprite) as Sprite;

                    if (match)
                    {
                        pair.Key.sprite = match;
                    }
                }

                PrefabUtility.SavePrefabAsset(minigame);
                Debug.Log($"Relinked sprites for {minigame.name}");
            }
            
        }

        EditorUtility.UnloadUnusedAssetsImmediate();

    }
    [Button]
    void FindSaveDataPath()
    {
        Debug.Log(Application.persistentDataPath);
    }


#endif
}

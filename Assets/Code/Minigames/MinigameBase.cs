using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.Events;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    public class MinigameBase : MonoBehaviour
    {
        public List<GameObject> completeImages = new List<GameObject>();
        
        bool _disableInput = false;
        public bool disableInput
        {
            get
            {
                return _disableInput || UIController.instance.hasActivePopup;
            }

            set
            {
                _disableInput = value;
            }
        }

        [SerializeField, Range(0f, 1f)]
        float SDFAlphaThreshold = (15f/255f);

        protected virtual IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return null;
        }

        // "fix all the objects!"
        public virtual string GetBriefText()
        {
            string objNameNoClone = gameObject.name.Replace("(Clone)", "").Trim();
            return $"UI/Minigame/{objNameNoClone}_brief";
        }

        // "use the right tool to fix up the objects"
        public virtual string GetInstructionText()
        {
            string objNameNoClone = gameObject.name.Replace("(Clone)", "").Trim();
            return $"UI/Minigame/Instruction/{objNameNoClone}";
        }

        // "Progress"
        public virtual string GetProgressDescription()
        {   
            return "UI/Progress";
        }

        // 100%
        public virtual float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;
            return 1f;
        }

        public virtual bool DisplayProgressBar()
        {
            return true;
        }

        public static bool				InMinigame()
	    {
	
            if (UIController.instance.minigameUI.gameObject.activeInHierarchy) return true;

		    return false;
	    }

        public virtual bool IsComplete()
        {
            return false;
        }

        public virtual void PlaySuccess(UnityAction andThen)
        {
            andThen?.Invoke();
        }

        public virtual void SetupSafeZone(Rect worldSpaceSafeZone)
        {

        }

        public virtual void OnPostHintAnimation()
        {
            MinigameController.instance.isHintPlaying = false;
        }

        
        public virtual void PlayHint()
        {
            OnPostHintAnimation();
        }

	    private void Update()
	    {
		    UpdateMusic();
	    }

	    public virtual void OnStart()
	    {
		
	    }

        public virtual void Skip()
        {
            
        }

	    public void UpdateMusic()
	    {
// 		    if (!GameUI.isValid) return;
// 		    if ((!GameUI.instance.uiAudio.music.IsPlaying(music)) && (!GameUI.instance.uiAudio.music.IsPlaying(victorySting)))
// 		    {
// 			    GameUI.PlayMusic(music);
// 		    }

	    }

	    public void VictorySting()
 	    {
// 		    GameUI.PlayMusic(victorySting);
// 		    GameUI.instance.uiAudio.music.restartLocalAudio = true; // after this revert to local
	    }

	    public virtual void OnComplete()
	    {
// 		    if (completeConversation)
// 		    {
// 			    ConversationUI.StartConversation(completeConversation, null, false);
// 		    }
// 		    if (action != null)
// 		    {
// 			    action.Trigger(Conversation.actionProcessor);
// 		    }
	    }


#if UNITY_EDITOR
        [BoxGroup("Initial Setup"), Button("Step 2: Generate SDFs", ButtonSizes.Large), PropertyOrder(1f)]
        public void GenerateSDFs()
        {
            var allObjects = GetInteractivePartsForSDFGeneration();

            if (allObjects == null || allObjects.Count() == 0)
                return;

            Dictionary<SpriteRenderer, string> d = new Dictionary<SpriteRenderer, string>();

            foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            {
                d.Add(r, r.sprite.name);
            }
            string psbPath = AssetDatabase.GetAssetPath(allObjects.ElementAt(0).sprite.sprite.texture);
            string path = Path.GetFullPath(psbPath + "../../../sdf/");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // check if the texture is compressed already.. if it's not, we need to temporarily uncompress it.. sorry
            UnityEditor.Presets.Preset previousImportSettings = null;
            UnityEditor.U2D.PSD.PSDImporter textureImporter = null;
            foreach (MinigamePiece piece in allObjects)
            {
                var spriteRenderer = piece.sprite;
                if (spriteRenderer == null) continue;
                var sprite = spriteRenderer.sprite;
                if (sprite == null) continue;

                textureImporter = (UnityEditor.U2D.PSD.PSDImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.texture));

                if (textureImporter.PlatformSettings[0].format != TextureImporterFormat.RGBA32)
                {
                    Debug.Log("minigame PSB is compressed.. temporarily decompressing");
                    previousImportSettings = new UnityEditor.Presets.Preset(textureImporter);
                    bool s = Dev.instance.PSDImporterDevPreset.ApplyTo(textureImporter);
                    textureImporter.SaveAndReimport();
                }

                break;
            }

            var loadedResult = AssetDatabase.LoadAllAssetsAtPath(psbPath);
            foreach (var pair in d)
            {
                var m = loadedResult.FirstOrDefault(x => x.name == pair.Value && x is Sprite) as Sprite;
                if (m) pair.Key.sprite = m;
            }

            foreach (MinigamePiece piece in allObjects)
            {
                var spriteRenderer = piece.sprite;
                if (spriteRenderer == null) continue;
                var sprite = spriteRenderer.sprite;
                if (sprite == null) continue;

                float atlasScale = spriteRenderer.bounds.size.x / sprite.rect.width;

                Texture2D scaledTex = null;
                Texture2D SDFTexture = null;

                Rect textureRect = sprite.textureRect;
                textureRect.x *= atlasScale;
                textureRect.y *= atlasScale;
                textureRect.size *= atlasScale;



                if (Boomzap.SDFGenerator.TextureMap.TryGetValue(sprite.texture.GetInstanceID(), out scaledTex))
                {
                }
                else
                {
                    scaledTex = Boomzap.SDFGenerator.CreateReadableScaledTexture(sprite.texture, atlasScale);
                    Boomzap.SDFGenerator.TextureMap.Add(sprite.texture.GetInstanceID(), scaledTex);

                    Debug.Log($"Create scaledTexture for: {sprite.texture.name}");
                }

                SDFTexture = Boomzap.SDFGenerator.Generate(scaledTex, textureRect, new Vector2Int(100, 100), SDFAlphaThreshold);

                string outPath = path + gameObject.name + "_" + sprite.name + ".sdf.png";

                File.WriteAllBytes(outPath, SDFTexture.EncodeToPNG());
                AssetDatabase.Refresh();
                int ri = outPath.IndexOf("Assets/");
                if (ri < 0)
                    ri = outPath.IndexOf("Assets\\");


                outPath = outPath.Substring(ri);

                if (ri < 0) continue;
                AssetDatabase.ImportAsset(outPath);

                if (spriteRenderer.transform.Find("sdf") == null)
                {
                    GameObject holder = new GameObject("sdf");
                    holder.AddComponent<SpriteRenderer>();
                    holder.transform.SetParent(spriteRenderer.transform);
                }

                Transform sdfHolder = spriteRenderer.transform.Find("sdf");
                if (sdfHolder)
                {
                    SpriteRenderer sdfRenderer = sdfHolder.GetComponent<SpriteRenderer>();
                    Sprite loaded = AssetDatabase.LoadAssetAtPath(outPath, typeof(Sprite)) as Sprite;
                    sdfRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
                    sdfRenderer.sortingOrder = 500;
                    sdfRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
                    sdfRenderer.sprite = loaded;

                    sdfHolder.SetParent(spriteRenderer.transform);
                    sdfRenderer.gameObject.SetActive(false);
                    piece.sdfRenderer = sdfRenderer;


                }

                // return;
            }

            Boomzap.SDFGenerator.ClearTextureCache();
            if (previousImportSettings != null)
            {
                previousImportSettings.ApplyTo(textureImporter);
                textureImporter.SaveAndReimport();
            }


            loadedResult = AssetDatabase.LoadAllAssetsAtPath(psbPath);
            foreach (var pair in d)
            {
                var m = loadedResult.FirstOrDefault(x => x.name == pair.Value && x is Sprite) as Sprite;
                if (m) pair.Key.sprite = m;
            }
        }

        [BoxGroup("Initial Setup"), Button("Step 3: Mark MG as Addressable and Refresh MG List", ButtonSizes.Large), PropertyOrder(2f)]
        void MarkAsAddressableAndRefreshMinigameList()
        {
            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage == null)
            {
                EditorUtility.DisplayDialog("Woops", "This functionality only seems to work if the minigame is opened in prefab mode.", "OK");
                return;
            }

            var AASettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (AASettings)
            {
                var ScenesGroup = AASettings.DefaultGroup;
                if (ScenesGroup)
                {
                    var guid = AssetDatabase.GUIDFromAssetPath(prefabStage.assetPath);
                    var entry = AASettings.CreateOrMoveEntry(guid.ToString(), ScenesGroup, false, false);
                    entry.SetLabel("Minigame", true);
                    entry.address = gameObject.name;
                    AASettings.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
                }
                else
                {
                    Debug.LogError("No minigame addressables group?");
                }
            }

            MinigameController.instance.RefreshEditorMinigameList();
        }

        [BoxGroup("Initial Setup"), Button("Step 4: Regenerate Preview Image", ButtonSizes.Large), PropertyOrder(3f)]
        public void RegeneratePreviewImage()
        {
            //4 : 3 aspect ratio
            const int previewTextureWidth = 512;
            const int previewTextureHeight = 512;

            var interactiveObjects = GetComponentsInChildren<SpriteRenderer>();

            string assetPath = AssetDatabase.GetAssetPath(interactiveObjects[0].GetComponent<SpriteRenderer>().sprite.texture);

            string fileAdd = "\\" + gameObject.name + "_room_preview.png";

            assetPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(assetPath)) + fileAdd;
            string outPath = Path.GetFullPath(assetPath);

            GameController.instance.SetActiveCamera(GameController.instance.defaultCamera);
            UIController.instance.gameObject.SetActive(false);

            var tempgo = Instantiate(gameObject, GameController.instance.transform);

            var renderTexture = new RenderTexture(previewTextureWidth, previewTextureHeight, 24, RenderTextureFormat.ARGB32);
            //var resizeTexture = new RenderTexture(512, 288, 24, RenderTextureFormat.ARGB32);

            Texture2D newTexture = new Texture2D(previewTextureWidth, previewTextureHeight);
            newTexture.name = "PreviewTexture";

            RenderTexture.active = renderTexture;

            GameController.instance.defaultCamera.targetTexture = renderTexture;
            GameController.instance.defaultCamera.Render();

            //Graphics.Blit(renderTexture, resizeTexture);
            RenderTexture.active = renderTexture;

            newTexture.ReadPixels(new Rect(0f, 0f, previewTextureWidth, previewTextureHeight), 0, 0);
            newTexture.Apply();

            GameController.instance.defaultCamera.targetTexture = null;
            RenderTexture.active = null;

            byte[] previewTexture = newTexture.EncodeToPNG();



            System.IO.File.WriteAllBytes(outPath, previewTexture);

            UnityEditor.AssetDatabase.SaveAssets();

            //DestroyImmediate(resizeTexture);
            DestroyImmediate(newTexture);
            DestroyImmediate(renderTexture);
            DestroyImmediate(tempgo);

            UIController.instance.gameObject.SetActive(true);
            GameController.instance.SetActiveCamera(GameController.instance.defaultCamera);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
#endif
    }

}
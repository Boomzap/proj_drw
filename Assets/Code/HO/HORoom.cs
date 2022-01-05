using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif

namespace ho
{
    [RequireComponent(typeof(SDFHitZoneRegister))]
    public class HORoom : MonoBehaviour
    {
        [SerializeField, Required] GameObject roomRootObject = null;
        public HOInteractiveObject[] interactiveObjects { get; private set; } = null;

        [ShowInInspector]
        public string roomDisplayName => string.Format("{0}/scene_name", gameObject.name.ToLower());

        public HORoomReference[]    subHO;
        public List<HODoorHandler>  doorHandlers = new List<HODoorHandler>();

        public Bounds       roomBounds;

        SDFHitZoneRegister  hitZones;

        protected List<HORoom>      subHOInstance = new List<HORoom>();

        HORoom              currentSubHO = null;

        public GameObject roomRoot { get { return roomRootObject; } } 

        public RectTransform rectTransform
        {
            get; private set;
        }

        public bool HasOpenSubHO => currentSubHO != null;

        public HORoom GetSubHOContaining(HOInteractiveObject obj)
        {
            //  GetComponentInParent doesn't work if not active.
            var roomObjs = obj.GetComponentsInParent<HORoom>(true);
            var roomObj = roomObjs.FirstOrDefault();


            return roomObj;
        }

        public bool ActiveRoomContains(HOInteractiveObject obj)
        {
            //  GetComponentInParent doesn't work if not active.
            var roomObjs = obj.GetComponentsInParent<HORoom>(true);
            var roomObj = roomObjs.FirstOrDefault();

            if (currentSubHO)
                return roomObj == currentSubHO;

            return roomObj == this;
        }

        HORoom()
        {
            
        }

        public HOFindableObject FindObjectByName(string name)
        {
            foreach (var o in interactiveObjects)
            {
                if (!(o is HOFindableObject)) continue;
                if (o.name == name) return o as HOFindableObject;
            }
            foreach (var sub in subHOInstance)
            {
                var s = sub.FindObjectByName(name);
                if (s != null) return s;
            }

            return null;
        }

        public bool IsOutOfSubHOBounds(Vector2 worldPos)
        {
            if (currentSubHO == null) 
            {
                worldPos -= (Vector2) transform.position;
                return (worldPos.x < roomBounds.min.x ||
                    worldPos.x > roomBounds.max.x ||
                    worldPos.y < roomBounds.min.y ||
                    worldPos.y > roomBounds.max.y);
            }

            return currentSubHO.IsOutOfSubHOBounds(worldPos);
        }

        public HOInteractiveObject[] HitTestAll(Vector2 worldPos)
        {
            if (currentSubHO)
                return currentSubHO.HitTestAll(worldPos);

            var result = hitZones.HitTestAll(worldPos);
            
            return result.Select(x => x.GetComponent<HOInteractiveObject>()).Where(x => x != null).ToArray();
            //return new HOInteractiveObject[0];
        }

        public HOInteractiveObject HitTest(Vector2 worldPos)
        {
            GameObject result = null;

            if (currentSubHO)
                return currentSubHO.HitTest(worldPos);

            result = hitZones.HitTest(worldPos);
            if (result == null)
                return null;

            return result.GetComponent<HOInteractiveObject>();
        }

        public void ReleaseSubHOs()
        {
            foreach (var sub in subHO)
            {
                HORoomAssetManager.instance.UnloadRoom(sub);
            }
            foreach (var inst in subHOInstance)
            {
                Destroy(inst.gameObject);
            }

            subHOInstance.Clear();
        }
        public HORoom GetSubHOInstance(HORoomReference roomRef)
        {
            foreach (var room in subHOInstance)
                if (roomRef.roomName == room.name)
                    return room;
            
            return null;
        }

        public HODoorHandler GetHODoorHandler(HORoom toRoom)
        {
            foreach (var handler in doorHandlers)
            {
                if (handler.subHO.roomName == toRoom.name)
                    return handler;
            }

            return null;
        }


        public HODoorHandler GetHODoorHandler(string forDoor)
        {
            foreach (var handler in doorHandlers)
            {
                if (handler.baseName == forDoor)
                    return handler;
            }

            return null;
        }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            hitZones = GetComponent<SDFHitZoneRegister>();
          
            rectTransform = GetComponent<RectTransform>();
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true);
        }

        void OnFinishCloseSubHO()
        {
            foreach (var subHO in subHOInstance)
            {
                subHO.gameObject.SetActive(false);
                    
                subHO.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        public void CloseSubHO()
        {
            if (currentSubHO == null) return;

            HOGameController.instance.subHOBlackout.Hide();

            iTween.ScaleTo(currentSubHO.gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeInBack"));
            this.ExecuteAfterDelay(0.3f, OnFinishCloseSubHO);
            currentSubHO = null;
        }

        public void OpenSubHO(HORoomReference subHORef, HODoorItem fromObj)
        {
            if (currentSubHO) return;
            
            currentSubHO = GetSubHOInstance(subHORef);

            if (currentSubHO)
            {
                currentSubHO.gameObject.SetActive(true);
                currentSubHO.transform.SetAsLastSibling();

                HOGameController.instance.subHOBlackout.Show(currentSubHO.transform);

                currentSubHO.transform.position = fromObj.transform.position;
                iTween.ScaleFrom(currentSubHO.gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeOutBack"));
                iTween.MoveTo(currentSubHO.gameObject, iTween.Hash("position", HOGameController.instance.transform.position + new Vector3(0f, HOGameController.instance.subHOYOffset, -2f), "time", 0.3f, "easetype", "easeOutBack"));

            } else
            {
                Debug.LogError($"Failed to open subHO instance: {subHORef.roomName}");
            }
        }

        public void LoadSubHOs()
        {
            if (subHOInstance.Count > 0)
            {
                Debug.LogError("Something's gone wrong: subHOs are still loaded when calling LoadSubHOs()");
                return;
            }

            foreach (var subRef in subHO)
            {
                var status = HORoomAssetManager.instance.GetRoomAssetStatus(subRef);
                if (!status.isValid)
                {
                    Debug.LogError("SubHO is not valid");
                    return;
                }

                if(!status.isLoaded)
                {
                    Debug.LogError("SubHO has not been loaded properly");
                    return;
                }

                var subRoom = Instantiate(status.room, HOGameController.instance.transform, true);
                subRoom.gameObject.SetActive(false);
                subRoom.name = status.roomName;
                subHOInstance.Add(subRoom);
                subRoom.Init();

                foreach (SpriteRenderer s in subRoom.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    s.sortingLayerName = "SubHO";
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        #if UNITY_EDITOR

        public void SetRoomRoot(GameObject root)
        {
            roomRootObject = root;
            EditorUtility.SetDirty(this);
        }

        [BoxGroup("Initial Setup"), Button("Step 3: Regenerate Preview Image", ButtonSizes.Large), PropertyOrder(3f)]
        public void RegeneratePreviewImage()
        {
            const int previewTextureWidth = 480;
            const int previewTextureHeight = 384;

            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true); 
            
            if (interactiveObjects.Length == 0)
                return;

            string assetPath = AssetDatabase.GetAssetPath(interactiveObjects[0].GetComponent<SpriteRenderer>().sprite.texture);
            
            string fileAdd = "\\" + gameObject.name + "_room_preview.png";

            assetPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(assetPath)) + fileAdd;
            string outPath = Path.GetFullPath(assetPath);

            GameController.instance.SetActiveCamera(HOGameController.instance.hoCamera);
            UIController.instance.gameObject.SetActive(false);
        
            var tempgo = Instantiate(gameObject, HOGameController.instance.transform);
            var tempRoom = tempgo.GetComponent<HORoom>();
            tempRoom.Init();
            foreach(var io in tempRoom.interactiveObjects)
                DestroyImmediate(io.gameObject);
            
            var renderTexture = new RenderTexture(previewTextureWidth, previewTextureHeight, 24, RenderTextureFormat.ARGB32);
            //var resizeTexture = new RenderTexture(512, 288, 24, RenderTextureFormat.ARGB32);

            Texture2D newTexture = new Texture2D(previewTextureWidth, previewTextureHeight);
            newTexture.name = "PreviewTexture";

            RenderTexture.active = renderTexture;

            HOGameController.instance.hoCamera.targetTexture = renderTexture;
            HOGameController.instance.hoCamera.Render();
            
            //Graphics.Blit(renderTexture, resizeTexture);
            RenderTexture.active = renderTexture;

            newTexture.ReadPixels(new Rect(0f, 0f, previewTextureWidth, previewTextureHeight), 0, 0);
            newTexture.Apply();

            HOGameController.instance.hoCamera.targetTexture = null;
            RenderTexture.active = null;

            byte[] previewTexture = newTexture.EncodeToPNG();
            


            System.IO.File.WriteAllBytes(outPath, previewTexture);

            UnityEditor.AssetDatabase.SaveAssets();

            //DestroyImmediate(resizeTexture);
            DestroyImmediate(newTexture);
            DestroyImmediate(renderTexture);
            DestroyImmediate(tempRoom.gameObject);

            UIController.instance.gameObject.SetActive(true);
            GameController.instance.SetActiveCamera(GameController.instance.defaultCamera);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
        #endif

        public List<HOInteractiveObject> GetOutOfBoundsObjects()
        {
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true); 
            List<HOInteractiveObject> outOfBounds = new List<HOInteractiveObject>();

            foreach (HOInteractiveObject o in interactiveObjects)
            {
                if (!o.IsFullyOnScreen())
                    outOfBounds.Add(o);
            }

            return outOfBounds;
        }

        public List<HOInteractiveObject> GetObjectsCoveredByRect(Rect rc)
        {
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true); 
            List<HOInteractiveObject> covered = new List<HOInteractiveObject>();

            Rect[] coveredAreas = UIController.instance.hoMainUI.GetCoveredAreas();



            foreach (HOInteractiveObject o in interactiveObjects)
            {
                if (o.IsCoveredByRect(coveredAreas[0]))
                    covered.Add(o);
            }

            return covered;
        }

        public void SetupForDisplay()
        {
            foreach (var obj in interactiveObjects)
            {
                Destroy(obj.gameObject);
            }

            var displayObjects = GetComponentsInChildren<SpriteRenderer>();

            foreach (var display in displayObjects)
            {
                display.sortingLayerName = "Conversation BG";
            }
        }

        public void SetupDisplayMode(bool displayItems)
        {
            //Debug.Log($"Enable Room Objects {displayItems}");
            var roomObjects = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var obj in roomObjects)
            {
                var objName = obj.name.ToLower();
                if (objName.StartsWith("d")) continue;
                if (objName.StartsWith("k")) continue;

                if (objName.StartsWith("o") || objName.StartsWith("p") || objName.StartsWith("t") || objName.StartsWith("x"))
                {
                    //Debug.Log($"Active Game Object: {obj.name} {displayItems} ");
                    obj.gameObject.SetActive(displayItems);
                }
            }
        }

#if UNITY_EDITOR

        [BoxGroup("Utility", order: 5), Button]
        public void InitializeNewObjectsToDefault()
        {
            foreach (Transform child in roomRootObject.transform)
            {
                if (child.GetComponent<HOInteractiveObject>())
                    continue;

                HOUtil.SetupObjectDefaultsFromName(child.gameObject, gameObject.name);
                HOInteractiveObject o = child.GetComponent<HOInteractiveObject>();
                if (o != null)
                    GenerateSDFs(new HOInteractiveObject[] { o });
            }

            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true); 

            //LocalizationManager.Sources[0].UpdateDictionary();
        }

        [BoxGroup("Utility", order:5), Button]
        public void ReInitializeAllRoomObjects()
        {
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true);

            foreach (HOInteractiveObject child in interactiveObjects)
            {
                child.Initialize();
            }
        }

        [BoxGroup("Utility", order: 5), Button]
        public void GenerateRiddleLocKeys()
        {
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true);

            foreach (HOInteractiveObject child in interactiveObjects)
            {
                if(child is HOFindableObject)
                {
                    HOFindableObject obj = (HOFindableObject)child;

                    if(obj.hasRiddleMode)
                        obj.GenerateRiddleKeys();
                }
            }
        }



        [BoxGroup("Initial Setup"), Button("Step 1: Setup Room", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("Create HO Room from PSB also triggers Setup Room, you may proceed to Step 2 to if room is created from PSB.")]
        public void SetupRoom()
        {
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true);

            if ((interactiveObjects?.Length ?? 0) > 0)
            {
                if (!UnityEditor.EditorUtility.DisplayDialog("Are you sure?", "This will reset all objects and reinitialize them with fresh data. Your localization changes will remain.", "Reset room", "Cancel"))
                {
                    return;
                }
            }

            interactiveObjects = null;
            doorHandlers.Clear();

            GetLocTermData();
            //GenerateFunFactKeys();

            roomBounds = roomRootObject.transform.GetChild(0).GetComponent<SpriteRenderer>().bounds;
            foreach (Transform child in roomRootObject.transform)
            {
                HOUtil.SetupObjectDefaultsFromName(child.gameObject, gameObject.name);

                var sr = child.GetComponent<SpriteRenderer>();
                roomBounds.Encapsulate(sr.bounds);
            }

            //Debug.Log($"RoomBounds with size of: {roomBounds.size.x}x{roomBounds.size.y}");

            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true);

            //RegeneratePreviewImage();
            //GenerateSDFs();
            //SetupDoorItemGlow();
        }

        //[BoxGroup("Initial Setup"), Button("Step 4: Setup Door Glow",ButtonSizes.Large), PropertyOrder(6f)]
        void SetupDoorItemGlow()
        {
            foreach (var handler in doorHandlers)
            {
                if (handler.openState)
                {
                    handler.openState.GetComponent<HODoorItem>().SetupOpenDoorGlow();
                }
            }
        }

        Texture2D ScaleAndPadTexture(Texture2D source, int newWidth, int newHeight, int padX, int padY)
        {
            Texture2D result = new Texture2D(newWidth+padX, newHeight+padY, source.format, false);

            Color32 blank = new Color32(0, 0, 0, 0);
            var ar = result.GetPixels32();
            for(int i = 0; i < ar.Length; i++)
                ar[i] = blank;
            result.SetPixels32(ar);
            result.Apply();

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    Color nc = source.GetPixelBilinear((float)x / (float)(newWidth), (float)y / (float)(newHeight), 0);
                    result.SetPixel(x+padX / 2, y+padY / 2, nc);
                }
            }

            result.Apply();

            return result;
        }

        //[BoxGroup("Utility", order: 5), Button]
        //public void RegenerateDoorHandlers()
        //{
        //    doorHandlers.Clear();

        //    var doorItems = gameObject.GetComponentsInChildren<HODoorItem>();

        //    Debug.Log(doorItems.Length);

        //    foreach(var door in doorItems)
        //    {
        //        HOUtil.SetupDoorItem(door as HODoorItem);
        //    }

        //    //SetupDoorItemGlow();
        //}

        [BoxGroup("Initial Setup"), Button("Step 2: Generate SDFs", ButtonSizes.Large), PropertyOrder(2f)]
        public void GenerateSDFs()
        {
            IEnumerable<HOInteractiveObject> forObjects = null;

            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true);

            if (forObjects == null)
                forObjects = interactiveObjects;

            if (forObjects.Count() == 0)
                return;

            string path = AssetDatabase.GetAssetPath(forObjects.ElementAt(0).GetComponent<SpriteRenderer>().sprite.texture) + "../../../sdf/";
            path = Path.GetFullPath(path);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            // check if the texture is compressed already.. if it's not, we need to temporarily uncompress it.. sorry
            UnityEditor.Presets.Preset previousImportSettings = null;
            UnityEditor.U2D.PSD.PSDImporter textureImporter = null;
            foreach (var obj in forObjects)
            {
                var sprite = obj.gameObject.GetComponent<SpriteRenderer>();
                if (sprite == null) continue;

                textureImporter = (UnityEditor.U2D.PSD.PSDImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.sprite.texture));

                if (textureImporter.PlatformSettings[0].format != TextureImporterFormat.RGBA32)
                {
                    Debug.Log("room PSB is compressed.. temporarily decompressing");
                    previousImportSettings = new UnityEditor.Presets.Preset(textureImporter);
                    bool s = Dev.instance.PSDImporterDevPreset.ApplyTo(textureImporter);
                    textureImporter.SaveAndReimport();
                }

                break;
            }


            foreach (var obj in forObjects)
            {
                var sprite = obj.gameObject.GetComponent<SpriteRenderer>();
                if (sprite == null) continue;

                float atlasScale = sprite.bounds.size.x / sprite.sprite.rect.width;

                Texture2D scaledTex = null;
                Texture2D SDFTexture = null;

                Rect textureRect = sprite.sprite.textureRect;
                textureRect.x *= atlasScale;
                textureRect.y *= atlasScale;
                textureRect.size *= atlasScale;

                if (Boomzap.SDFGenerator.TextureMap.TryGetValue(sprite.sprite.texture.GetInstanceID(), out scaledTex))
                {
                }
                else
                {
                    scaledTex = Boomzap.SDFGenerator.CreateReadableScaledTexture(sprite.sprite.texture, atlasScale);
                    Boomzap.SDFGenerator.TextureMap.Add(sprite.sprite.texture.GetInstanceID(), scaledTex);

                    Debug.Log($"Create scaledTexture for: {sprite.sprite.texture.name}");
                }

                SDFTexture = Boomzap.SDFGenerator.Generate(scaledTex, textureRect, new Vector2Int(100, 100));

                string outPath = path + gameObject.name + "_" + sprite.name + ".sdf.png";

                File.WriteAllBytes(outPath, SDFTexture.EncodeToPNG());
                AssetDatabase.Refresh();
                int ri = outPath.IndexOf("Assets/");
                if (ri < 0)
                    ri = outPath.IndexOf("Assets\\");


                outPath = outPath.Substring(ri);

                if (ri < 0) continue;
                AssetDatabase.ImportAsset(outPath);

                if (obj.gameObject.transform.childCount == 0)
                {
                    GameObject holder = new GameObject("sdf");
                    holder.AddComponent<SpriteRenderer>();
                    holder.transform.SetParent(obj.gameObject.transform);
                }

                if (obj.gameObject.transform.childCount == 1)
                {
                    SpriteRenderer sdfRenderer = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    SDFHitZone sdfHitZone = obj.GetComponent<SDFHitZone>();
                    if (sdfHitZone)
                    {
                        sdfHitZone.sdfSprite = sdfRenderer;
                    }

                    Sprite loaded = AssetDatabase.LoadAssetAtPath(outPath, typeof(Sprite)) as Sprite;
                    sdfRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
                    sdfRenderer.sortingOrder = sprite.sortingOrder - 1;
                    sdfRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
                    sdfRenderer.sprite = loaded;
                    obj.sdfRenderer = sdfRenderer;

                    obj.sdfRenderer.gameObject.SetActive(false);
                }

                // return;
            }

            Boomzap.SDFGenerator.ClearTextureCache();
            if (previousImportSettings != null)
            {
                previousImportSettings.ApplyTo(textureImporter);
                textureImporter.SaveAndReimport();
            }
        }

        public void GenerateSDFs(IEnumerable<HOInteractiveObject> forObjects = null)
        {
            interactiveObjects = roomRootObject.GetComponentsInChildren<HOInteractiveObject>(true); 

            if (forObjects == null)
                forObjects = interactiveObjects;
           
            if (forObjects.Count() == 0)
                return;

            string path = AssetDatabase.GetAssetPath(forObjects.ElementAt(0).GetComponent<SpriteRenderer>().sprite.texture) + "../../../sdf/";
            path = Path.GetFullPath(path);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            
            // check if the texture is compressed already.. if it's not, we need to temporarily uncompress it.. sorry
            UnityEditor.Presets.Preset previousImportSettings = null;
            UnityEditor.U2D.PSD.PSDImporter textureImporter = null;
            foreach (var obj in forObjects)
            {
                var sprite = obj.gameObject.GetComponent<SpriteRenderer>();
                if (sprite == null) continue;

                textureImporter = (UnityEditor.U2D.PSD.PSDImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.sprite.texture));

                if (textureImporter.PlatformSettings[0].format != TextureImporterFormat.RGBA32)
                {
                    Debug.Log("room PSB is compressed.. temporarily decompressing");
                    previousImportSettings = new UnityEditor.Presets.Preset(textureImporter);
                    bool s = Dev.instance.PSDImporterDevPreset.ApplyTo(textureImporter);
                    textureImporter.SaveAndReimport();
                }

                break;
            }


            foreach (var obj in forObjects)
            {
                var sprite = obj.gameObject.GetComponent<SpriteRenderer>();
                if (sprite == null) continue;

                float atlasScale = sprite.bounds.size.x / sprite.sprite.rect.width;

                Texture2D scaledTex = null;
                Texture2D SDFTexture = null;

                Rect textureRect = sprite.sprite.textureRect;
                textureRect.x *= atlasScale;
                textureRect.y *= atlasScale;
                textureRect.size *= atlasScale;

                if (Boomzap.SDFGenerator.TextureMap.TryGetValue(sprite.sprite.texture.GetInstanceID(), out scaledTex))
                {
                } else
                {
                    scaledTex = Boomzap.SDFGenerator.CreateReadableScaledTexture(sprite.sprite.texture, atlasScale);
                    Boomzap.SDFGenerator.TextureMap.Add(sprite.sprite.texture.GetInstanceID(), scaledTex);                    

                    Debug.Log($"Create scaledTexture for: {sprite.sprite.texture.name}");
                }

                SDFTexture = Boomzap.SDFGenerator.Generate(scaledTex, textureRect, new Vector2Int(100, 100));                

                string outPath = path + gameObject.name + "_" + sprite.name + ".sdf.png";

                File.WriteAllBytes(outPath, SDFTexture.EncodeToPNG());
                AssetDatabase.Refresh();
                int ri = outPath.IndexOf("Assets/");
                if (ri < 0)
                    ri = outPath.IndexOf("Assets\\");
                    

                    outPath = outPath.Substring(ri);

                if (ri < 0) continue;
                AssetDatabase.ImportAsset(outPath);

                if (obj.gameObject.transform.childCount == 0)
                {
                    GameObject holder = new GameObject("sdf");
                    holder.AddComponent<SpriteRenderer>();
                    holder.transform.SetParent(obj.gameObject.transform);
                }

                if (obj.gameObject.transform.childCount == 1)
                {
                    SpriteRenderer sdfRenderer = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    SDFHitZone sdfHitZone = obj.GetComponent<SDFHitZone>();
                    if (sdfHitZone)
                    {
                        sdfHitZone.sdfSprite = sdfRenderer;
                    }

                    Sprite loaded = AssetDatabase.LoadAssetAtPath(outPath, typeof(Sprite)) as Sprite;
                    sdfRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
                    sdfRenderer.sortingOrder = sprite.sortingOrder - 1;
                    sdfRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
                    sdfRenderer.sprite = loaded;
                    obj.sdfRenderer = sdfRenderer;

                    obj.sdfRenderer.gameObject.SetActive(false);
                }

               // return;
            }

            Boomzap.SDFGenerator.ClearTextureCache();
            if (previousImportSettings != null)
            {
                previousImportSettings.ApplyTo(textureImporter);
                textureImporter.SaveAndReimport();
            }
        }
        #endif

        private string GetLocTermData()
        {
            string nameLower = gameObject.name.ToLower();
            string sceneName = string.Format("{0}/scene_name", nameLower);

            return HOUtil.GetOrAddDefaultTermIfNeeded(sceneName);
        }

        //[BoxGroup("Fun Facts"), Button]
        //private void GenerateFunFactKeys()
        //{
        //    string nameLower = gameObject.name.ToLower();

        //    //NOTE*: 5 means number of fun fact keys
        //    int funFactCount = 5;

        //    funFactKeys = new string[funFactCount];

        //    for (int i = 0; i < funFactKeys.Length; i++ )
        //    {
        //        string sceneName = string.Format("{0}/fun_fact_{1}", nameLower, i+1);
        //        funFactKeys[i] = sceneName;
        //        LocalizationUtil.FindLocalizationEntry(sceneName, "", true, TableCategory.Trivia);
        //    }
        //}

#if UNITY_EDITOR

        void AddAddressableEntry(AddressableAssetSettings AASettings, AddressableAssetGroup scenesGroup, GameObject newAddressableObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(newAddressableObject);
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            var entry = AASettings.CreateOrMoveEntry(guid.ToString(), scenesGroup, false, false);
            entry.SetLabel("HO Scene", true);
            entry.address = gameObject.name;
            AASettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
        }


        [BoxGroup("Addressable", order : 10), Button]
        void MarkAsAddressable()
        {
            var AASettings = AddressableAssetSettingsDefaultObject.Settings;
            if (AASettings)
            {
                var ScenesGroup = AASettings.DefaultGroup;
                if (ScenesGroup)
                {
                    AddAddressableEntry(AASettings, ScenesGroup, gameObject);
                }
                else
                {
                    Debug.LogError("No HO Scenes addressables group?");
                }
            }
        }
#endif
    }
}

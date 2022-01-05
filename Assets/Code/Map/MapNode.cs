using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor;
#endif

namespace ho
{
    public class MapNode : MonoBehaviour
    {
        public SpriteRenderer       mapObjSelected;
        public SpriteRenderer       mapObjUnselected;
        
        public HORoomReference[]    roomReferences;
        public MinigameReference[]  mgReferences;

        [NonSerialized, HideInInspector]
        public Sprite               nodePreviewSprite;

        public SpriteRenderer       nodeCircleBacking;
        public SpriteRenderer       nodePuzzleOverlay;
        public SpriteRenderer       nodeCompletedOverlay;
        public TextMeshPro          nodeItemCountLeft;

        [SerializeField]
        TextMeshPro                 mapNodeLabel;
        [SerializeField]
        SpriteRenderer              mapNodeLabelBacking;

 //       public LocalizedString      mapNodeName;

        MaterialPropertyBlock       propertyBlock;
        SpriteRenderer              spriteRenderer;

        bool                        isMouseover = false;
        float                       fxAlpha = 0f;
        float                       fxAlphaTarget = 0f;

        Vector3                     labelBaseScale;
        bool                        _isInteractive = true;
        public bool                 isInteractive 
        {
            get { return _isInteractive; }
            set { _isInteractive = value; propertyBlock.SetFloat("_DesatIntensity", value ? 0f : 1f); }
        }

        public bool                 isCompleted { get; set; } = false;


#if UNITY_EDITOR
        GUIStyle                    editorStyle = new GUIStyle();
        Texture2D                   backgroundTexture = null;

        static string[] dupeMG = new string[0];
        static string[] dupeHO = new string[0];

        private void OnDrawGizmos()
        {
            if (name == "ho01")
            {
                var controller = GetComponentInParent<MapController>();
                dupeMG = controller.mapNodes.SelectMany(x => x.mgReferences).Select(x => x.AssetGUID).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
                dupeHO = controller.mapNodes.SelectMany(x => x.roomReferences).Select(x => x.AssetGUID).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
            }

            if (editorStyle.normal.background != backgroundTexture || backgroundTexture == null)
            {
                Color32[] pixels = new Color32[16];
                for (int i =0 ; i < 16; i++)
                    pixels[i] = new Color32(0, 0, 0, 255);

                backgroundTexture = new Texture2D(4, 4);
                backgroundTexture.SetPixels32(pixels);
                backgroundTexture.Apply();
                editorStyle.normal.background = backgroundTexture;
                editorStyle.normal.textColor = Color.white;
            }

            string txt = $"NODE: {name.ToUpper()}\n";

            for (int i = 0; i < roomReferences.Length; i++)
            {
                //txt += roomReferences[i].AssetGUID.name;
                var n = AssetDatabase.GUIDToAssetPath(roomReferences[i].AssetGUID);

                n = System.IO.Path.GetFileNameWithoutExtension(n);
                if (dupeHO.Contains(roomReferences[i].AssetGUID))
                    n = "[" + n + "]";
                txt += n;

                if (i < (roomReferences.Length - 1))
                    txt += "\n";
            }

            if (mgReferences.Length > 0)
                txt += "\n";

            for (int i = 0; i < mgReferences.Length; i++)
            {
                //txt += mgReferences[i].editorAsset.name;
                var n = AssetDatabase.GUIDToAssetPath(mgReferences[i].AssetGUID);
                n = System.IO.Path.GetFileNameWithoutExtension(n);
                if (dupeMG.Contains(mgReferences[i].AssetGUID))
                    n = "[" + n + "]";
                txt += n;

                if (i < (mgReferences.Length - 1))
                    txt += "\n";
            }

            RectTransform t = (transform as RectTransform);
            Vector3 pos = new Vector2(t.rect.xMin, t.rect.yMax);

            UnityEditor.Handles.Label(pos + transform.position, new GUIContent(txt), editorStyle);
        }
#endif

        public void SetupForChapter(Chapter chapter)
        {
            bool isActive = false;
            bool isPuzzle = false;
            bool isComplete = false;
            int itemCountLeft = 0;

            foreach (var entry in chapter.sceneEntries)
            {
                if (entry.IsMinigame)
                {
                    if (mgReferences.Contains(entry.minigame))
                    {
                        isActive = true;
                        isPuzzle = true;

                        Savegame.MinigameState s = GameController.save.GetMinigameState(entry);
                        isComplete = s.completed;

                        break;
                    }
                } else
                {
                    if (roomReferences.Contains(entry.hoRoom))
                    {
                        isActive = true;
                        isPuzzle = entry.forceShowAsMinigameInMap;

                        Savegame.HOSceneState s = GameController.save.GetSceneState(entry);
                        isComplete = s.completed;

                        itemCountLeft = s.hasSaveState ? (s.currentObjects.Count + s.futureObjects.Count) : entry.objectCount + entry.unlockableSubHO.Count;
                        break;
                    }
                }
            }

            if (!isActive)
            {
                mapObjSelected.gameObject.SetActive(false);
                mapObjUnselected.gameObject.SetActive(true);
            } else
            {
                mapObjSelected.gameObject.SetActive(true);
                mapObjUnselected.gameObject.SetActive(true);                

                nodePuzzleOverlay.gameObject.SetActive(isPuzzle);
                nodeCompletedOverlay.gameObject.SetActive(isComplete);

                if (!isComplete && !isPuzzle)
                {
                    nodeItemCountLeft.gameObject.SetActive(true);
                    nodeItemCountLeft.text = itemCountLeft.ToString();
                    nodeCircleBacking.gameObject.SetActive(true);
                }
            }

            isCompleted = isComplete;

            isInteractive = isActive;
        }

        public void SetupForFreePlay()
        {
            bool isActive = true;
            //bool isPuzzle = false;
            bool isComplete = false;
            //int itemCountLeft = 0;


            mapObjSelected.gameObject.SetActive(true);
            mapObjUnselected.gameObject.SetActive(true);

            nodePuzzleOverlay.gameObject.SetActive(false);
            nodeCompletedOverlay.gameObject.SetActive(false);

            nodeItemCountLeft.gameObject.SetActive(false);
            nodeCircleBacking.gameObject.SetActive(false);

            isCompleted = isComplete;

            isInteractive = isActive;
        }


        private void Awake()
        {
            if (roomReferences.Length > 0)
                nodePreviewSprite = roomReferences[0].roomPreviewSprite;
            

            spriteRenderer = GetComponent<SpriteRenderer>();

            propertyBlock = new MaterialPropertyBlock();

            propertyBlock.SetFloat("_LightIntensity", 0f);
            propertyBlock.SetFloat("_DesatIntensity", 0f);
            propertyBlock.SetTexture("_MainTex", nodePreviewSprite.texture);

            nodePuzzleOverlay.gameObject.SetActive(false);
            nodeCompletedOverlay.gameObject.SetActive(false);
            nodeItemCountLeft.gameObject.SetActive(false);
            nodeCircleBacking.gameObject.SetActive(false);
            isMouseover = false;

            mapObjSelected.material = spriteRenderer.material;
            mapObjUnselected.material = spriteRenderer.material;

            spriteRenderer.SetPropertyBlock(propertyBlock);

            fxAlphaTarget = fxAlpha = 0f;
            labelBaseScale = mapNodeLabelBacking.transform.localScale;

            spriteRenderer.sprite = nodePreviewSprite;


        }

        private void Update()
        {
            if (!isInteractive)
            {
                fxAlphaTarget = 0f;
                isMouseover = false;
            }

            float fxDelta = Mathf.Abs(fxAlpha - fxAlphaTarget);

            if (fxDelta > 0.001f)
            {
                fxAlpha += Mathf.Min(Time.deltaTime*3f, fxDelta) * Mathf.Sign(fxAlphaTarget - fxAlpha);
            } else
            {
                fxAlpha = fxAlphaTarget;
            }

            propertyBlock.SetFloat("_LightIntensity", 0.4f * fxAlpha);
            propertyBlock.SetFloat("_DesatIntensity", isInteractive ? 0f : 1f);

            //transform.localScale = Vector2.one * (1f + 0.1f * fxAlpha);
            mapObjSelected.transform.localScale = Vector2.one * (1f + 0.1f * fxAlpha);
            mapObjUnselected.transform.localScale = Vector2.one * (1f + 0.1f * fxAlpha);

            mapObjSelected.color = new Color(1f, 1f, 1f, fxAlpha);
            //mapObjUnselected.color = new Color(1f, 1f, 1f, 1f-fxAlpha);

            propertyBlock.SetTexture("_MainTex", nodePreviewSprite.texture);
            spriteRenderer.SetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture("_MainTex", mapObjSelected.sprite.texture);
            mapObjSelected.SetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture("_MainTex", mapObjUnselected.sprite.texture);
            mapObjUnselected.SetPropertyBlock(propertyBlock);

            Color labelColor = mapNodeLabelBacking.color;
            labelColor.a = fxAlpha;

            Color textColor = mapNodeLabel.color;
            textColor.a = fxAlpha;

            mapNodeLabelBacking.color = labelColor;
            mapNodeLabel.color = textColor;

            mapNodeLabel.gameObject.SetActive(fxAlpha > 0f);
            mapNodeLabelBacking.gameObject.SetActive(fxAlpha > 0f);

            AutofitToText();
        }

        private void OnEnable()
        {
            //mapNodeLabel.text = mapNodeName;
            mapNodeLabelBacking.transform.position = mapObjSelected.transform.position;
            AutofitToText();
        }

        public void SetMouseover(bool b)
        {
            if (b == isMouseover) return;

            isMouseover = b;

            fxAlphaTarget = isMouseover ? 1f : 0f;

            if (isMouseover)
            {
                Vector3 labelTargetPos = mapObjSelected.transform.position + new Vector3(0f, 100f);
                mapNodeLabelBacking.gameObject.transform.position = labelTargetPos;
                AutofitToText();
            } 

        }

        [Button]
        void AutofitToText()
        {
            mapNodeLabel.ForceMeshUpdate();
            Bounds textBounds = mapNodeLabel.textBounds;

            float desiredW = textBounds.size.x + 100f;

            mapNodeLabelBacking.size = new Vector2(desiredW, mapNodeLabelBacking.size.y);
        }
    }
}

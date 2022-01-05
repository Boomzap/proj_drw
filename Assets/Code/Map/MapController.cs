using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class MapController : MonoBehaviour, IWorldState
    {
        [SerializeField, Required]
        GameObject      mapRootObject;

        [SerializeField, Required]
        LineRenderer    lineRenderer;
        
        [SerializeField, Required]
        SpriteRenderer  lineCap;

        [SerializeField, Required]
        GameObject      mapTilePrefab;

        [SerializeField, Required]
        RectTransform   autoLayoutRect;

        [SerializeField]
        float           mapTopCenterGap = 300f;

        [SerializeField]
        Vector2         mapCapNodeOffset = new Vector2(14f, 14f);

        [SerializeField, ReadOnly]
        public List<MapNode>   mapNodes = new List<MapNode>();

        Vector2[,]      gridSnapPositions;
        int             gridSnapLengthX, gridSnapLengthY;

        MapNode         curMouseoverNode = null;
        public bool            launchingGameplay = false;
        

        [SerializeField]
        AudioClip       launchClip;

        [SerializeField]
        Animator        mapBossIconAnimator;
        [SerializeField]
        Collider2D      mapBossIcon;

        [SerializeField]
        Boomzap.Conversation.Conversation   sceneAlreadyCompleteConversation;

        bool            canPlayBossEntry = false;

        public SpriteRenderer GetBossSprite()
        {
            return mapBossIcon.GetComponent<SpriteRenderer>();
        }

        public SpriteRenderer[] GetAllActivePuzzleSprites()
        {
            List<SpriteRenderer> sprites = new List<SpriteRenderer>();

            var allActiveNodes = mapNodes.Where(x => x.isInteractive && !x.isCompleted).ToArray();
            foreach (var n in allActiveNodes)
            {
                var bootEntry = GameController.instance.currentChapter.FindMatchingBootEntry(n.roomReferences, n.mgReferences, false);

                if (bootEntry == null) continue;
                if (bootEntry.IsMinigame) 
                {
                    sprites.Add(n.GetComponent<SpriteRenderer>());
                    sprites.Add(n.mapObjUnselected);
                }

            }

            return sprites.ToArray();
        }

        public SpriteRenderer[] GetAllActiveNodeSprites()
        {
            List<SpriteRenderer> sprites = new List<SpriteRenderer>();

            var allActiveNodes = mapNodes.Where(x => x.isInteractive && !x.isCompleted).ToArray();
            foreach (var n in allActiveNodes)
            {
                sprites.Add(n.GetComponent<SpriteRenderer>());
                sprites.Add(n.mapObjUnselected);
            }

            return sprites.ToArray();
        }

        public bool ShouldDestroyOnLeave()
        {
            return true;
        }

        public void OnLeave()
        {
            StateCache.instance.UnloadMapScreen();
        }

        void GenerateGridSnapPositions()
        {
            int verticalFit = 0;
            int horizontalFit = 0;

            Vector2 localOffset = autoLayoutRect.localPosition;

            RectTransform prefabRc = mapTilePrefab.GetComponent<RectTransform>();

            verticalFit = Mathf.FloorToInt(autoLayoutRect.sizeDelta.y / prefabRc.sizeDelta.y);
            horizontalFit = Mathf.FloorToInt((autoLayoutRect.sizeDelta.x - mapTopCenterGap) / prefabRc.sizeDelta.x);

            float verticalGap = (autoLayoutRect.sizeDelta.y - (verticalFit * prefabRc.sizeDelta.y)) / (verticalFit - 1);
            float horizontalGap = (autoLayoutRect.sizeDelta.x - mapTopCenterGap - (horizontalFit * prefabRc.sizeDelta.x)) / (horizontalFit - 2);

            gridSnapPositions = new Vector2[verticalFit, horizontalFit];

            for (int y = 0; y < verticalFit; y++)
            {
                for (int x = 0; x < horizontalFit; x++)
                {
                    float horzOffset = (x >= (horizontalFit / 2)) ? (mapTopCenterGap - horizontalGap) : 0f;
                    
                        

                    gridSnapPositions[y,x] =
                        localOffset + prefabRc.sizeDelta * 0.5f +
                            new Vector2(horzOffset + x * (prefabRc.sizeDelta.x + horizontalGap) + autoLayoutRect.rect.xMin, y * (prefabRc.sizeDelta.y + verticalGap) + autoLayoutRect.rect.yMin);
                }
            }

            gridSnapLengthX = horizontalFit;
            gridSnapLengthY = verticalFit;
      
        }

        #if UNITY_EDITOR
        [Button]
        void DebugRandomAssignmentForTesting()
        {
            var rooms = HORoomAssetManager.instance.roomTracker.roomEntries.Where(x => !x.roomName.ToLower().Contains("sub") && x.roomName.ToLower().Contains("ho")).ToList();

            for (int i = 0; i < mapNodes.Count; i++)
            {
                var room = rooms[i % rooms.Count];
                var mg = MinigameController.instance.minigamePrefabs[i % MinigameController.instance.minigamePrefabs.Length];

                mapNodes[i].roomReferences = new HORoomReference[] { room.roomReference };
                mapNodes[i].mgReferences = new MinigameReference[] { mg };
                mapNodes[i].nodePreviewSprite = room.previewSprite;
            }
        }

        [Button]
        void SnapNodesToAutoLayout()
        {
            GenerateGridSnapPositions();

            foreach (MapNode n in mapNodes)
            {
                float min = float.MaxValue;
                Vector2 minV = Vector2.zero;

                for (int i = 0; i < mapNodes.Count; i++)
                {
                    Vector2 v = CalcPosForIndex(i);
                    float dm = (v - (Vector2)n.transform.localPosition).sqrMagnitude;
                    if (dm < min)
                    {
                        min = dm;
                        minV = v;
                    }
                }

                n.transform.localPosition = minV;
            }
        }
        #endif
        Vector2 CalcPosForIndex(int i)
        {
            GenerateGridSnapPositions();

            // BL to TL to TR to BR
            if (i < gridSnapLengthY)
            {
                return gridSnapPositions[i,0];
            } 
            
            i -= gridSnapLengthY;

            if (i < (gridSnapLengthX - 1))
            {
                return gridSnapPositions[gridSnapLengthY-1, i+1];
            }

            i -= (gridSnapLengthX - 1);

            return gridSnapPositions[i, gridSnapLengthX-1];     
        }

        private void Reset()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void SetupHover(MapNode forNode)
        {
            lineRenderer.gameObject.SetActive(forNode != null);

            if (forNode)
            {
                Vector3[] posArray = new Vector3[3];
                
                posArray[0] = forNode.transform.position;
                posArray[1] = (transform.position - forNode.transform.position).normalized * 120f + posArray[0];  // ensuring it's outside of the border
                posArray[2] = forNode.mapObjSelected.transform.position;
                lineRenderer.positionCount = 3;
                lineRenderer.SetPositions(posArray);


                Vector3 newPos = (posArray[2] - posArray[1]).normalized;
                newPos.x *= mapCapNodeOffset.x;
                newPos.y *= mapCapNodeOffset.y;
                newPos.z = 0f;

                lineCap.transform.position = forNode.mapObjSelected.transform.position + newPos;
            }
        }

        void SetMouseoverNode(MapNode node)
        {
            if (node != curMouseoverNode)
            {
                if (node != null && !node.isInteractive)
                {
                    SetupHover(null);

                    node.SetMouseover(false);

                } else
                {
                    SetupHover(node);

                    if (node)
                        node.SetMouseover(true);
                }

                curMouseoverNode?.SetMouseover(false);
                curMouseoverNode = node;
            }
        }

        private void Update()
        {
            if (GameController.instance.inConversation || UIController.instance.hasActivePopup)
            {
                SetMouseoverNode(null);
                mapBossIconAnimator.SetBool("mouseover", false);
                return;
            }

            var castResult = Physics2D.GetRayIntersection(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));

            if (castResult)
            {
                var obj = castResult.transform.gameObject.GetComponent<MapNode>();

                if (obj != null)
                {
                    SetMouseoverNode(obj);
                } else
                {
                    bool hasMouseover = false;

                    foreach (var n in mapNodes)
                    {
                        if (n.mapObjSelected.gameObject == castResult.transform.gameObject ||
                            n.mapObjUnselected.gameObject == castResult.transform.gameObject)
                        {
                            SetMouseoverNode(n);   
                            hasMouseover = true;
                            break;
                        }
                    }

                    if (!hasMouseover)
                        SetMouseoverNode(null);
                }

                mapBossIconAnimator.SetBool("mouseover", canPlayBossEntry && castResult.collider == mapBossIcon);
                

            } else
            {
                SetMouseoverNode(null);
                mapBossIconAnimator.SetBool("mouseover", false);
            }

            if (Input.GetMouseButtonDown(0) && !launchingGameplay)
            {
                //Freeplay
                if (GameController.instance.isUnlimitedMode)
                {
                    if (curMouseoverNode != null && curMouseoverNode.isInteractive)
                    {
                        FreeplayPopup popup = Popup.GetPopup<FreeplayPopup>();
                        popup.Setup(curMouseoverNode);
                        popup.Show();
                        launchingGameplay = true;
                    }
                }
                else
                {
                    Chapter.Entry bootEntry = null;

                    if (curMouseoverNode != null && curMouseoverNode.isInteractive)
                    {
                        if (curMouseoverNode.isCompleted)
                        {
                            GameController.instance.PlayConversation(sceneAlreadyCompleteConversation);
                            return;
                        }

                        bootEntry = GameController.instance.currentChapter.FindMatchingBootEntry(curMouseoverNode.roomReferences, curMouseoverNode.mgReferences, false);
                    }
                    if (bootEntry != null)
                    {
                        Audio.instance.PlaySound(launchClip);

                        launchingGameplay = true;

                        GameController.instance.LaunchBootEntry(bootEntry);
                    }
                }
            }
        }

        private void OnEnable()
        {
            curMouseoverNode = null;
            foreach (var n in mapNodes)
                n.SetMouseover(false);
        }

        private void OnDisable()
        {
            curMouseoverNode = null;
            foreach (var n in mapNodes)
                n.SetMouseover(false);            
        }

        private void Start()
        {
            //Freeplay
            if (GameController.instance.isUnlimitedMode)
            {
                foreach (var n in mapNodes)
                {
                    n.SetupForFreePlay();
                }

                MapBossInactive();

                return;
            }

            foreach (var n in mapNodes)
            {
                n.SetupForChapter(GameController.instance.currentChapter);
            }

            canPlayBossEntry = GameController.instance.currentChapter.sceneEntries.All(x => GameController.save.IsChapterEntryComplete(x));

            if (canPlayBossEntry)
                MapBossActive();
            else
                MapBossInactive();

            launchingGameplay = false;
        }

        [Button]
        void MapBossActive()
        {
            mapBossIconAnimator.SetBool("active", true);
        }

        [Button]
        void MapBossInactive()
        {
            mapBossIconAnimator.SetBool("active", false);
        }

#if UNITY_EDITOR
        GUIStyle editorStyle = new GUIStyle();
        Texture2D backgroundTexture = null;


        void OnDrawGizmos()
        {
            if (editorStyle.normal.background != backgroundTexture || backgroundTexture == null)
            {
                Color32[] pixels = new Color32[16];
                for (int i = 0; i < 16; i++)
                    pixels[i] = new Color32(0, 0, 0, 255);

                backgroundTexture = new Texture2D(4, 4);
                backgroundTexture.SetPixels32(pixels);
                backgroundTexture.Apply();
                editorStyle.normal.background = backgroundTexture;
                editorStyle.normal.textColor = Color.white;
            }

            string txt = "";

            var allSlotMgs = mapNodes.SelectMany(x => x.mgReferences);

            foreach (var c in GameController.instance.gameChapters)
            {
                //// boss mg in slot

                // mg and scene would be shown at same time
                var scenes = c.sceneEntries.Where(x => x.IsHOScene);
                var mgs = c.sceneEntries.Where(x => x.IsMinigame);

                foreach (var h in scenes)
                {
                    var tile = mapNodes.Where(x => x.roomReferences.Contains(h.hoRoom)).FirstOrDefault();
                    if (tile == null) continue;

                    int ovl = mgs.Count(x => tile.mgReferences.Contains(x.minigame));

                    if (ovl > 0)
                    {
                        txt += $"{c.name} HO in tile {tile.name} would be showing {ovl} minigame(s) at the same time as the scene\n";
                    }
                }
            }


            RectTransform t = (transform as RectTransform);
            Vector3 pos = new Vector2(t.rect.xMin, t.rect.yMax);

            UnityEditor.Handles.Label(pos + transform.position + new Vector3(0f, 300f, 0f), new GUIContent(txt), editorStyle);
        }

        [Button]
        public void Setup()
        {
            if (!mapRootObject)
            {
                Debug.LogError("Map Root object is not set");
                return;
            }


            // code may need to be adjusted depending on how good the artist is about naming...
            Dictionary<string, MapNode> mapNodeDict = new Dictionary<string, MapNode>();

            foreach (Transform child in mapRootObject.transform)
            {
                GameObject go = child.gameObject;

                if (go.name.Contains("boss")) continue;
                if (go.name.Contains("bg")) continue;

                var nameParts = go.name.Split('_');
                if (nameParts.Length < 2) continue;
                string pairName = "";
                bool isSelectedVar = false;

                if (nameParts[0] == "select" || nameParts[0] == "map")
                    pairName = nameParts[1];
                else
                    pairName = nameParts[0];

               
                isSelectedVar = go.name.Contains("select");

                var colliders = go.GetComponents<Collider2D>();
                foreach (var collider in colliders)
                    DestroyImmediate(collider);

                go.AddComponent<PolygonCollider2D>();

                MapNode node;
                if (mapNodeDict.ContainsKey(pairName))
                {
                    node = mapNodeDict[pairName];
                }
                else
                {
                    var newTile = Instantiate(mapTilePrefab, transform);
                    newTile.name = pairName;
                    node = newTile.GetComponent<MapNode>();
                    HOUtil.GetOrAddDefaultTermIfNeeded("Map/" + pairName, "", false);
                    //node.mapNodeName = "Map/" + pairName;
                    mapNodeDict[pairName] = node;
                }


                if (isSelectedVar)
                    node.mapObjSelected = go.GetComponent<SpriteRenderer>();
                else
                    node.mapObjUnselected = go.GetComponent<SpriteRenderer>();
            }

            mapNodes = mapNodeDict.Values.ToList();
            for (int i = 0; i < mapNodes.Count; i++)
            {
                mapNodes[i].transform.localPosition = CalcPosForIndex(i);
            }
        }
    #endif
    }
}

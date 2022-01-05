using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;
using System;


namespace ho
{
    public class RepairMG : MinigameBase
    {
        [BoxGroup("Debug")]
        [ReadOnly]
        public RepairType currentRepairType;

        [ReadOnly, BoxGroup("Repair Pieces")]
        public List<RepairablePiece> brokenPieces = new List<RepairablePiece>();
        int totalBrokenPieces;

        List<RepairToolList.RepairTool> mgTools = new List<RepairToolList.RepairTool>();

        SpriteRenderer fixedPiece;

        Dictionary<SpriteRenderer, MaterialPropertyBlock> bgMaterials = new Dictionary<SpriteRenderer, MaterialPropertyBlock>();

        float hintAlphaCur = 0f, hintAlphaTar = 0f;

        [BoxGroup("Tools Available")]
        public ToolToggle[] toolsAvailable = new ToolToggle[] { new ToolToggle() { Enabled = true } };

        [System.Serializable]
        public class ToolToggle
        {
            [ToggleGroup("Enabled", "$Label")]
            public bool Enabled;

            [ReadOnly]
            public RepairToolList.RepairTool tool;

            [HideInInspector]
            public string Label { get { return tool.toolName; } }
        }

#if UNITY_EDITOR
        #region Prefab Setup

        [BoxGroup("PSB Settings")]
        //NOTE* Set to true if PSB Fixed piece layers is higher than Repairable pieces
        public bool isFixedPieceInFront = false;

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        public void Setup()
        {
            brokenPieces.Clear();
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var piece in spriteRenderers)
            {
                //NOTE* If sprite ends with b = fixed piece.
                if (piece.name.Contains("bg") == false && piece.name.Contains("complete") == false)
                {
                    if (piece.name.EndsWith("b") || piece.name.Contains("B"))
                    {
                        CreateBrokenPiece(piece);
                    }else
                    {
                        fixedPiece = piece;
                    }
                }
            }
            AddPieceRenderer();
            RefreshToolList();
        }

        [Button] void AddPieceRenderer()
        {
            foreach(var piece in brokenPieces)
            {
                piece.sprite = piece.GetComponent<SpriteRenderer>();
            }
        }

        void CreateBrokenPiece(SpriteRenderer piece)
        {
            //Checks if components exists
            RepairablePiece newPiece = piece.GetComponent<RepairablePiece>();
            PolygonCollider2D pieceCollider = piece.GetComponent<PolygonCollider2D>();

            //Add new components if it doesn't exist
            if (newPiece == null) newPiece = piece.gameObject.AddComponent<RepairablePiece>();
            if (pieceCollider == null) pieceCollider = piece.gameObject.AddComponent<PolygonCollider2D>();

            //Creates Reference of Broken Piece Renderer and Collider
            newPiece.sprite = piece;
            newPiece.pieceCollider = pieceCollider; 

            piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, 0);

            //Makes sure fixed Piece In back
            //NOTE* This is for PSBs that fixed piece is in higher layer.
            if(isFixedPieceInFront)
            {
                fixedPiece.sortingOrder = piece.sortingOrder;
                fixedPiece.sortingOrder -= 1;
            }

            brokenPieces.Add(newPiece);
        }

        [Button]
        public void RemoveComponents()
        {
            brokenPieces.Clear();

            var colliders = GetComponentsInChildren<PolygonCollider2D>();
            var pieces = GetComponentsInChildren<RepairablePiece>();

            foreach (var collider in colliders)
                DestroyImmediate(collider);

            foreach (var piece in pieces)
                DestroyImmediate(piece);
        }

        [Button]
        public void GenerateRepairGroup()
        {
            GameObject repairGroup = new GameObject();
            repairGroup.transform.SetParent(this.transform);
            repairGroup.AddComponent<RepairGroup>();
            repairGroup.name = "[GROUP] Repair Tool";
        }

        #endregion
        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return brokenPieces;
        }

        #region Refresh Tool List

        [Button]
        public void RefreshToolList()
        {
            string[] guid = AssetDatabase.FindAssets("RepairTools");
            string path = AssetDatabase.GUIDToAssetPath(guid[0]);
            RepairToolList repairToolList = (RepairToolList)AssetDatabase.LoadAssetAtPath(path, typeof(RepairToolList));

            if (repairToolList)
            {
                toolsAvailable = new ToolToggle[repairToolList.repairTools.Count];
                for (int i = 0; i < toolsAvailable.Length; i++)
                {
                    toolsAvailable[i] = new ToolToggle();
                    toolsAvailable[i].Enabled = false;
                    toolsAvailable[i].tool = repairToolList.repairTools[i];
                }
            }
            else
                Debug.Log("Tools not found");
        }
        #endregion
#endif

        

        private void Start()
        {
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            bgMaterials = new Dictionary<SpriteRenderer, MaterialPropertyBlock>();
            foreach (var s in sprites)
            {
                if ((s.name.Contains("b") == false && s.name.Contains("B") == false) || s.name.Contains("bg") || s.name.ToLower().Contains("complete"))
                {
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();

                    mpb.SetTexture("_MainTex", s.sprite.texture);
                    mpb.SetFloat("_DesatIntensity", 0f);
                    mpb.SetFloat("_LightIntensity", 0f);

                    bgMaterials[s] = mpb;
                    s.material = MinigameController.instance.InactiveObjectMaterial;
                    s.SetPropertyBlock(mpb);
                }
            }
        }

        private void Update()
        {
            if (disableInput)
            {
                return;
            }

            float dt = Time.deltaTime * 2f;

            if (hintAlphaCur > hintAlphaTar)
            {
                hintAlphaCur -= dt;
                if (hintAlphaCur < hintAlphaTar)
                    hintAlphaCur = hintAlphaTar;
            }
            else if (hintAlphaCur < hintAlphaTar)
            {
                hintAlphaCur += dt;
                if (hintAlphaCur > hintAlphaTar)
                    hintAlphaCur = hintAlphaTar;
            }

            foreach (var pair in bgMaterials)
            {
                pair.Value.SetFloat("_DesatIntensity", MinigameController.instance.InactiveDesatFactor * hintAlphaCur);
                pair.Value.SetFloat("_LightIntensity", MinigameController.instance.InactiveBrightenFactor * hintAlphaCur);
                pair.Key.SetPropertyBlock(pair.Value);
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            InitializeGameplay();
        }

        void InitializeGameplay()
        {
            LoadMinigameTools();
            UIController.instance.minigameUI.GetRepairToolHolder().UpdateTools();
            totalBrokenPieces = brokenPieces.Count;
        }

        void LoadMinigameTools()
        {
            mgTools.Clear();
            foreach (ToolToggle toggle in toolsAvailable)
            {
                if (toggle.Enabled) mgTools.Add(toggle.tool);
            }
        }

        public override void PlayHint()
        {
            if (brokenPieces.Any(x => x.isRepaired == false && x.isRepairable))
            {
                //Randomize broken piece
                var rnd = new System.Random();
                var randomized = brokenPieces.OrderBy(x => rnd.Next());
                var brokenPiece = randomized.FirstOrDefault(x => x.isRepaired == false && x.isRepairable);
                UIController.instance.minigameUI.GetRepairToolHolder().SelectTool(brokenPiece.repairType.ToString());
                currentRepairType = brokenPiece.repairType;
                StartCoroutine(HintCor());
            }
        }

        IEnumerator HintCor()
        {
            yield return new WaitForSeconds(0.5f);

            hintAlphaTar = 1f;

            //Darken not compatible pieces
            foreach (var v in brokenPieces.Where(x => x.repairType != currentRepairType))
            {
                v.FadeAlpha = hintAlphaTar;
            }
               

            yield return new WaitForSeconds(5f);

            hintAlphaTar = 0f;

            foreach (var v in brokenPieces.Where(x => x.repairType != currentRepairType))
            {
                v.FadeAlpha = hintAlphaTar;
            }

            OnPostHintAnimation();
        }

        #region Public Methods

        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(SkipMinigame());
        }

        IEnumerator SkipMinigame()
        {

            UIController.instance.minigameUI.GetRepairToolHolder().ToolCopy.SetActive(true);

            while (brokenPieces.Count != 0)
            {
                //Select a Broken Piece 
                RepairablePiece brokenPiece = brokenPieces.FirstOrDefault(x => x.isRepaired == false && x.isRepairable);

                while(brokenPiece == null)
                {
                    //Wait for other repairable pieces because there are groups that are not ready.
                    yield return new WaitForSeconds(0.1f);
                    brokenPiece = brokenPieces.FirstOrDefault(x => x.isRepaired == false && x.isRepairable);
                }

                //
                UIController.instance.minigameUI.GetRepairToolHolder().SkipMinigame = true;

                //Select Right tool for broken piece
                UIController.instance.minigameUI.GetRepairToolHolder().SelectTool(brokenPiece.repairType.ToString());
                currentRepairType = brokenPiece.repairType;

                Vector3 movePosition = Camera.main.WorldToScreenPoint(brokenPiece.transform.position);
                UIController.instance.minigameUI.GetRepairToolHolder().MoveToolCopy(movePosition);
                yield return new WaitForSeconds(0.3f);
                brokenPiece.OnRepairPiece();
            }
            UIController.instance.minigameUI.GetRepairToolHolder().DeselectTool();
        }

        public override bool IsComplete()
        {
            return brokenPieces.Count == 0;
        }
        public void UpdateRepairType(RepairToolList.RepairTool toolSelected)
        {
            if (toolSelected != null)
            {
                bool parsed = Enum.TryParse(toolSelected.toolName, out currentRepairType);
                //Debug.Log("Parsed? " + parsed);
            }
            else
                currentRepairType = RepairType.None;
            
            Debug.Log("Repair Type: " + currentRepairType);
        }

        public List<RepairToolList.RepairTool> GetMinigameTools()
        {
            return mgTools;
        }

        public void OnRepairedPiece(RepairablePiece brokenPiece)
        {
            if (brokenPieces.Contains(brokenPiece))
            {
                brokenPieces.Remove(brokenPiece);
            }
            else
                Debug.Log("Piece not found!");


            if(brokenPieces.Count == 0)
            {
                UIController.instance.minigameUI.GetRepairToolHolder().DeselectTool();
            }
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;
            return  (float)(totalBrokenPieces - brokenPieces.Count) /totalBrokenPieces;
        }

        public override string GetInstructionText()
        {
            return $"UI/Minigame/Instruction/RepairMG";
        }

        #endregion
    }
}

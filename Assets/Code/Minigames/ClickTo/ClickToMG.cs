using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector;
    using System.Linq;

    public class ClickToMG : MinigameBase
    {
        public enum MGType
        {
            Swap,
            Rotate,
            SwapRotate
        }
        [SerializeField] public MGType mgType;
        [SerializeField] float selectedHeight = -1;
        [SerializeField] float selectedHeightSpeed = 4.0f;
        [SerializeField] float pieceSuccessTime = 0.25f;
        [SerializeField] bool lockOnComplete = false;

        Collider2D[] filterResults;
        ClickToPiece[] allPieces;
        ClickToPiece selectedPiece = null;
        ClickToPiece highlightPiece = null;

        public float SelectedHeightSpeed { get { return selectedHeightSpeed; } }
        public float PieceSuccessTime { get { return pieceSuccessTime; } }
        public Material SelectedMaterial => MinigameController.instance.JigsawSelectedMaterial;

        public int GoalsComplete { get { int count = 0; foreach (var t in allPieces) { if (t.IsComplete || t.IsCorrect()) count++; } return count; } }
        public int TotalGoals { get { return allPieces.Length; } }

        public float SelectedHeight { get { return selectedHeight; } }
        public ClickToPiece SelectedPiece { get { return selectedPiece; } set { selectedPiece = value; } }
        public bool LockOnComplete { get { return lockOnComplete; } }

        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return GetComponentsInChildren<ClickToPiece>(true);
        }

        public override bool IsComplete()
        {
            if (allPieces == null) return false;

            return GoalsComplete >= allPieces.Length;
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;

            if (allPieces == null) return 0f;

            return (float)GoalsComplete / (float)TotalGoals;
        }

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        void Setup()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var exPieces = GetComponentsInChildren<ClickToPiece>();
                foreach (var e in exPieces)
                {
                    DestroyImmediate(e);
                }

                SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
                foreach (var t in sprites)
                {
                    if (StrReplace.Equals(t.name, "bg")) continue;
                    if (StrReplace.Equals(t.name, "background")) continue;
                    if (t.name.Contains("full") || t.name.Contains("complete"))
                    {
                        completeImages.Add(t.gameObject);
                        continue;
                    }
                    if (t.name[0] == '_') continue;
                    ClickToPiece piece = t.GetComponent<ClickToPiece>();
                    if (piece != null)
                    {
                        DestroyImmediate(piece);
                    }

                    switch (mgType)
                    {
                        case MGType.Swap: piece = t.gameObject.AddComponent<ClickToSwapPiece>(); break;
                        case MGType.Rotate: piece = t.gameObject.AddComponent<ClickToRotatePiece>(); break;
                        case MGType.SwapRotate:
                            piece = t.gameObject.AddComponent<ClickToSwapPiece>();
                            piece = t.gameObject.AddComponent<ClickToRotatePiece>(); break;
                    }
                }
            }
#endif

            completeImages.ForEach(x => x.gameObject.SetActive(false));
            allPieces = GetComponentsInChildren<ClickToPiece>();
            foreach (var t in allPieces) t.SetupPiece();
        }

        [Button]
        void Complete()
        {
            foreach (var t in allPieces)
            {
                t.ForceComplete();
            }
        }

        //Note: This is only used for Click to Swap otherwise it will produce an error.
        [Button]
        void RandomizePositions()
        {
            List<string> swapIDs = new List<string>();

            var swappablePieces = allPieces.Where(x => x.GetType() == typeof(ClickToSwapPiece)).ToList();
            //Record all swap groups
            foreach (var piece in swappablePieces)
            {
                ClickToSwapPiece swapPiece = piece as ClickToSwapPiece;
                if (swapIDs.Contains(swapPiece.swapID) == false)
                {
                    swapIDs.Add(swapPiece.swapID);
                    //Debug.Log(swapPiece.swapID);
                }
            }

            foreach (var id in swapIDs)
            {
                var swapPieces = swappablePieces.Cast<ClickToSwapPiece>().Where(x => x.swapID.Equals(id)).ToList();

                List<Vector3> positions = new List<Vector3>();
                for (int i = 0; i < swapPieces.Count; i++)
                {
                    positions.Add(swapPieces[i].transform.position);
                }
                for (int i = 1; i < swapPieces.Count; i++)
                {
                    Vector3 ownPosition = swapPieces[i].transform.position;
                    Vector3 generatedPosition = ownPosition;
                    int idx = 0;
                    while (Vector3.Distance(ownPosition, generatedPosition) < 2.0f)
                    {
                        idx = Random.Range(0, positions.Count);
                        generatedPosition = positions[idx];
                    }
                    swapPieces[i].transform.position = generatedPosition;
                    positions.RemoveAt(idx);
                }
                if (Vector3.Distance(swapPieces[0].transform.position, positions[0]) < 2.0f)
                {
                    Vector3 swapPosition = swapPieces[0].transform.position;
                    swapPieces[0].transform.position = swapPieces[swapPieces.Count - 1].transform.position;
                    swapPieces[swapPieces.Count - 1].transform.position = swapPosition;
                }
                else
                {
                    swapPieces[0].transform.position = positions[0];
                }
                positions.Clear();
            }


        }

        [Button]
        void RandomizeRotations()
        {
            var rotatePieces = allPieces.Where(x => x.GetType() == typeof(ClickToRotatePiece)).ToList();
            foreach (var t in rotatePieces.Where(x => x.isFixedPiece == false))
            {
                while(t.IsCorrect())
                {
                    t.RandomizeRotation();
                }
            }
        }
        [Button]
        public override void OnStart()
        {
            base.OnStart();

            
            Setup();

            if (mgType == MGType.Rotate)
            {
                RandomizeRotations();
                lockOnComplete = HOGameController.instance.GetDifficultySetting().lockClickToRotate;
            }
            else
            if (mgType == MGType.Swap)
            {
                RandomizePositions();
                lockOnComplete = HOGameController.instance.GetDifficultySetting().lockClickToSwap;
            }
            else if (mgType == MGType.SwapRotate)
            {
                RandomizeRotations();
                RandomizePositions();
                lockOnComplete = HOGameController.instance.GetDifficultySetting().lockClickToSwapRotate;
            }

            foreach (var p in allPieces)
            {
                if (p.IsCorrect() && LockOnComplete)
                {
                    p.ForceComplete();
                }
            }
        }



        IEnumerator PlaySuccessCo(UnityEngine.Events.UnityAction andThen)
        {
            //VictorySting();
            //             List<ClickToPiece> completedPieces = new List<ClickToPiece>(allPieces);
            //             while (completedPieces.Count > 0)
            //             {
            //                 ClickToPiece piece = completedPieces[0];
            //                 completedPieces.RemoveAt(0);
            //                 piece.OnSuccess();
            //                 yield return new WaitForSeconds(0.05f);
            //             }
            // 
            //             yield return new WaitForSeconds(1f);

            var pieces = allPieces.OrderByDescending(x => x.transform.localPosition.x);
            foreach (var p in pieces)
            {
                p.OnSuccess();
                yield return new WaitForSeconds(0.02f);
            }

            completeImages?.ForEach(x => x.SetActive(true));

            yield return new WaitForSeconds(1f);


            andThen?.Invoke();
        }

        public override void PlaySuccess(UnityEngine.Events.UnityAction andThen)
        {
            // animation here?
            StartCoroutine(PlaySuccessCo(andThen));

        }


        // Start is called before the first frame update
        void Start()
        {
        }

        ClickToPiece PieceFromMouse()
        {
            RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
            if (hit2D != null && hit2D.Length > 0)
            {
                ClickToPiece top = null;
                foreach (var t in hit2D)
                {
                    ClickToPiece piece = mgType == MGType.SwapRotate? t.transform.GetComponent<ClickToSwapPiece>() : t.transform.GetComponent<ClickToPiece>();
                    if (!piece || piece.IsComplete) continue;
                    if (top == null || piece.SortValue > top.SortValue) top = piece;
                }
                if (top != null)
                {
                    return top;
                }
            }
            return null;
        }

        // Update is called once per frame
        void Update()
        {
            if (disableInput || IsComplete())
            {
                if (highlightPiece)
                    highlightPiece.SetSelected(false);
                highlightPiece = null;

                return;
            }

            ClickToPiece top = PieceFromMouse();
            if (top != highlightPiece)
            {
                if (highlightPiece) highlightPiece.SetSelected(false);
                if (top) top.SetSelected(true);
                highlightPiece = top;
            }

          


            if (Input.GetMouseButtonDown(0))
            {
                if (top != null)
                {
                    if (mgType == MGType.SwapRotate)
                    {
                        if(selectedPiece != null && top == selectedPiece)
                        {
                            top.OnClick();
                            top.GetComponent<ClickToRotatePiece>().OnClick();
                        }
                        else
                            top.OnClick();
                    }
                    else
                    {
                        top.OnClick();
                    }
                }

            }
        }

        IEnumerator HintCorRotate(ClickToRotatePiece piece)
        {
            while (!piece.IsCorrect())
            {
                piece.OnClick();

                TapFeedbackFX.instance.CreateAtWorldPos(piece.transform.position);

                yield return new WaitForSeconds(0.2f);
            }
            OnPostHintAnimation();
        }

        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(SkipCor());
        }

        IEnumerator SkipCor()
        {
            List<ClickToPiece> allIncorrect;

            do
            {
                allIncorrect = allPieces.Where(x => !x.IsCorrect()).ToList();

                foreach (var p in allIncorrect)
                {
                    if (p.IsCorrect()) continue;

                    if (p is ClickToRotatePiece)
                    {
                        p.OnClick();
                    }
                    else if (p is ClickToSwapPiece)
                    {
                        if (!p.IsAnimating() && (Random.value < 0.3f || allIncorrect.Count < 3))
                            (p as ClickToSwapPiece).Skip();
                    }

                }

                yield return new WaitForSeconds(0.2f);
            } while (allIncorrect.Count > 0);


            disableInput = false;
        }

        public override void PlayHint()
        {
            var incorrect = allPieces.FirstOrDefault(x => !x.IsCorrect());

            if (incorrect)
            {
                if (incorrect is ClickToRotatePiece)
                {
                    StartCoroutine(HintCorRotate(incorrect as ClickToRotatePiece));
                }
                else if (incorrect is ClickToSwapPiece)
                {
                    ClickToSwapPiece pcA = incorrect as ClickToSwapPiece;
                    Vector3 orgLocation = pcA.CorrectLocation;

                    float closest = float.MaxValue;
                    ClickToSwapPiece pcB = null;

                    foreach (var piece in allPieces)
                    {
                        float d = (orgLocation - piece.transform.position).sqrMagnitude;
                        if (d < closest)
                        {
                            closest = d;
                            pcB = piece as ClickToSwapPiece;
                        }
                    }

                    if (pcA.IsSelected) pcA.SetSelected(false);
                    if (pcB.IsSelected) pcB.SetSelected(false);

                    pcA.OnClick();
                    pcB.OnClick();

                    TapFeedbackFX.instance.CreateAtWorldPos(pcA.transform.position);
                    TapFeedbackFX.instance.CreateAtWorldPos(pcB.transform.position);

                    OnPostHintAnimation();
                }
            }
        }

        public override string GetInstructionText()
        {
            switch(mgType)
            {
                case MGType.Rotate: return "UI/Minigame/Instruction/ClickToRotate";
                case MGType.Swap: return "UI/Minigame/Instruction/ClickToSwap";
                case MGType.SwapRotate: return "UI/Minigame/Instruction/ClickToSwapRotate";
                default: return string.Empty;
            }
        }

#if UNITY_EDITOR
        bool isCTS => mgType == MGType.Swap || mgType == MGType.SwapRotate;
        [ShowIf("isCTS"), BoxGroup("Initial Setup"), Button("Step 5: Setup Swap ID", ButtonSizes.Large), PropertyOrder(4f)]
        void SetupSwapIDs()
        {
            foreach (var piece in allPieces)
            {
                if (piece is ClickToSwapPiece)
                {
                    ClickToSwapPiece swapPiece = piece as ClickToSwapPiece;
                    swapPiece.swapID = "Set A";
                }
            }
        }
#endif
    }

}
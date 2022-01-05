using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace ho
{

    public class Jigsaw : MinigameBase
    {
        [SerializeField] float dragThreshold = 20.0f;
        [SerializeField] float snapThreshold = 20.0f;
        float selectedHeight = -1.0f;
        Vector2 autoMoveTime = new Vector2(0.5f, 2.0f);

        [SerializeField] JigsawPiece[] pieces;
        bool canRotate = false;

        Rect safeZone;

        public float SelectedHeight { get { return selectedHeight; } }
        public Vector2 AutoMoveTime { get { return autoMoveTime; } }
        public float SnapThreshold { get { return snapThreshold; } }
        public int DragLayer { get { return 9000; } }

        public int topPieceOrder
        {
            get
            {
                if (pieces == null || pieces.Length == 0) return 0;

                return pieces.Max(x =>
                {
                    if (x.sprite == null) return 0;

                    return x.sprite.sortingOrder == DragLayer ? 0 : x.sprite.sortingOrder;
                }) + 1;
            }
        }
        public int UnsetLayerOffset { get { return 1000; } }
        public int NextUnsetLayer { get { int v = topPieceOrder; if (v < UnsetLayerOffset) return UnsetLayerOffset; return v; } }

        public bool CanRotate { get { return canRotate; } }

        JigsawPiece selected;
        bool isDragging = false;
        Vector3 startDrag = Vector3.zero;
        List<JigsawPiece> completedPieces = new List<JigsawPiece>();


        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return GetComponentsInChildren<JigsawPiece>(true);
        }

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        void Setup()
        {
            if (!Application.isPlaying)
            {
                SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
                List<JigsawPiece> tempPieces = new List<JigsawPiece>();
                pieces = null;

                foreach (var t in sprites)
                {
                    if (StrReplace.Equals(t.name, "bg")) continue;
                    if (StrReplace.Equals(t.name, "background")) continue;
                    if (t.name[0] == '_') continue;
                    if (t.name.Contains("full") || t.name.Contains("complete"))
                    {
                        completeImages.Add(t.gameObject);
                        continue;
                    }

                    JigsawPiece piece = t.GetComponent<JigsawPiece>();
                    if (piece != null) DestroyImmediate(piece); // clear the old
                    piece = t.gameObject.AddComponent<JigsawPiece>();
                    tempPieces.Add(piece);
                    piece.SetJigsaw(this);
                    piece.Init();
                }
            }

            completeImages?.ForEach(x => x.SetActive(false));

            pieces = GetComponentsInChildren<JigsawPiece>();

            if (Application.isPlaying)
            {
                foreach (var t in pieces.OrderBy(x => x.sprite.sortingOrder))
                {
                    t.SetJigsaw(this);
                    t.Init();
                }

                var nonPieceRenderers = GetComponentsInChildren<SpriteRenderer>(true).Where(x => x.GetComponentInParent<JigsawPiece>() == null);
                foreach (var t in nonPieceRenderers)
                {
                    t.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
            }
        }

        public override void SetupSafeZone(Rect worldSpaceSafeZone)
        {
            safeZone = worldSpaceSafeZone;
            Setup();

            foreach (var t in pieces)
            {
                //Vector3 toPos = new Vector3(Random.Range(worldSpaceSafeZone.xMin, worldSpaceSafeZone.xMax), Random.Range(worldSpaceSafeZone.yMin, worldSpaceSafeZone.yMax), t.transform.position.z);

                //Note* Screen Camera Canvas Safe Zone Computation
                Vector3 toPos = new Vector3(
                    Random.Range(transform.position.x - (worldSpaceSafeZone.width / 2f), transform.position.x + (worldSpaceSafeZone.width / 2f)),
                    Random.Range(transform.position.y - (worldSpaceSafeZone.height / 2f), transform.position.y + (worldSpaceSafeZone.height / 2f)),
                    transform.position.z);

                t.Scatter(toPos, true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (disableInput)
            {
                if (selected) selected.SetHighlight(false);
                selected = null;

                return;
            }

            if (!isDragging)
            {
                RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
                JigsawPiece[] hitPiece = hit2D.Select(x => x.transform.gameObject.GetComponent<JigsawPiece>()).Where(x => x != null).OrderByDescending(x => x.sort).ToArray();

                JigsawPiece piece = hitPiece.Length > 0 ? hitPiece[0] : null;

                if (piece != null)
                {
                    if (piece.IsSet)
                    {
                        // don't pick up solved puzzles
                        piece = null;
                    }

                    if (selected != piece)
                    {
                        if (selected)
                            selected.SetHighlight(false);
                        if (piece)
                            piece.SetHighlight(true);

                        selected = piece;
                    }

                }
                else if (selected != null)
                {
                    selected.SetHighlight(false);
                    selected = null;
                }

                if (selected)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        isDragging = true;
                        startDrag = Input.mousePosition;

                        Vector3 mousePos = GameController.instance.currentCamera.ScreenToWorldPoint(Input.mousePosition);

                        selected.StartDrag(mousePos);

                        Audio.instance.PlaySound(MinigameController.instance.onPieceSelected.GetClip(null));
                    }
                }

            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (selected == null) return;

                    //Debug.Log($"Is inside screen? {IsInsideScreen(selected.transform.position)}");

                    //Reset if outside screen
                    if (IsInsideScreen(selected.transform.position) == false)
                    {
                        Vector3 mousePos = GameController.instance.currentCamera.ScreenToWorldPoint(startDrag);
                        selected.ResetToPosition(mousePos);
                    }


                    // it's been released. Did we move much?
                    float distance = (startDrag - Input.mousePosition).magnitude;
                    if (distance < dragThreshold && canRotate)
                    {
                        selected.Rotate(1);
                    }
                    else
                    {
                        if (selected.isClose)
                        {
                            selected.Snap();
                            completedPieces.Add(selected);
                            Audio.instance.PlaySound(MinigameController.instance.onPieceCorrect.GetClip(null));
                            //GameUI.PlaySound("PartPlace");
                        }
                        else
                        {
                            //GameUI.PlaySound("PartDrop");
                        }
                    }

                    selected.StopDragging();
                    isDragging = false;
                }
                else
                {
                    if (selected)
                    {
                        Vector3 mousePos = GameController.instance.currentCamera.ScreenToWorldPoint(Input.mousePosition);
                        selected.TrackMouse(mousePos);
                    }
                }
            }
        }

        IEnumerator PlaySuccessCo(UnityEngine.Events.UnityAction onDone)
        {
            foreach (var piece in pieces)
            {
                piece.OnSuccess();
            }

            completeImages?.ForEach(x => x.SetActive(true));

            yield return new WaitForSeconds(1.5f);

            if (onDone != null) onDone();
        }


        public override void PlaySuccess(UnityEngine.Events.UnityAction onDone)
        {
            StartCoroutine(PlaySuccessCo(onDone));
        }

        public override void PlayHint()
        {
            var uncomplete = pieces.FirstOrDefault(x => !x.IsSet);

            if (uncomplete)
            {
                completedPieces.Add(selected);
                uncomplete.Snap();
                TapFeedbackFX.instance.CreateAtWorldPos(uncomplete.transform.position);
                Audio.instance.PlaySound(MinigameController.instance.onPieceCorrect.GetClip(null));
                OnPostHintAnimation();
            }
        }

        public override string GetInstructionText()
        {
            return "UI/Minigame/Instruction/Jigsaw";
        }

        public override string GetProgressDescription()
        {
            return "UI/PiecesLeft";
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = false;
            return pieces.Length - pieces.Count(x => x.IsSet);
        }

        public override bool IsComplete()
        {
            if (isDragging) return false;
            foreach (var t in pieces)
            {
                if (!t.IsSet) return false;
            }
            return true;
        }

        IEnumerator SkipCor()
        {
            var incomplete = pieces.Where(x => !x.IsSet).ToList();

            foreach (var p in incomplete)
            {
                p.Snap();
                completedPieces.Add(p);

                yield return new WaitForSeconds(0.2f);
            }

            disableInput = false;
        }

        public override void Skip()
        {
            disableInput = true;
            StartCoroutine(SkipCor());
        }

        public float Progress
        {
            get
            {
                int complete = 0;
                foreach (var t in pieces)
                {
                    if (t.IsSet) complete++;
                }
                return complete / (float)pieces.Length;
            }
        }


        public bool IsInsideScreen(Vector3 position)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

            //Note* screen point start from 0 -> Screen Width 

            if (screenPos.x > Screen.width || screenPos.y > Screen.height)
                return false;
            if (screenPos.x < 0 || screenPos.y < 0)
                return false;


            return true;
        }
    }

}
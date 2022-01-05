using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class SudokuMG : MinigameBase
    {
        [ReadOnly]
        public List<SudokuPiece> sudokuPieces = new List<SudokuPiece>();

        public int gridSize = 0;

        SudokuPiece highlightPiece;

        public SudokuPiece selectedPiece;

        public float selectedHeightSpeed = 4.0f;

        [SerializeField] int correctRows => sudokuPieces.Count(x => x.isInCorrectRow);
        [SerializeField] int correctColumns => sudokuPieces.Count(x => x.isInCorrectColumn);

        //bool isComplete = true;

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        public void Setup()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                sudokuPieces.Clear();

                var exPieces = GetComponentsInChildren<ClickToPiece>(true);
                foreach (var e in exPieces)
                {
                    DestroyImmediate(e);
                }

                SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

                for(int i = 0; i < sprites.Length; i++)
                {
                    var t = sprites[i];

                    if (StrReplace.Equals(t.name, "bg")) continue;
                    if (StrReplace.Equals(t.name, "background")) continue;
                    if (t.name.Contains("full") || t.name.Contains("complete"))
                    {
                        completeImages.Add(t.gameObject);
                        continue;
                    }
                    if (t.name[0] == '_') continue;
                    if (t.name.StartsWith("m_")) continue;

                    int sudokuIndex = sudokuPieces.Count + 1;
                    SudokuPiece piece = t.gameObject.AddComponent<SudokuPiece>();
                    piece.SetupPiece(sudokuIndex);
                    sudokuPieces.Add(piece);
                }
            }
#endif
            //Check if board is a perfect square
            double result = Mathf.Sqrt(sudokuPieces.Count);
            bool isSquare = result % 1 == 0;

            if(isSquare == false)
            {
                Debug.Log($"{sudokuPieces.Count} {result % 1}");
                Debug.LogError("Sudoku board must be a perfect square to be setup.");
                return;
            }
            //Setup Piece for shared default material
            sudokuPieces.ForEach(x => x.SetupPiece());
            gridSize = (int) System.Math.Ceiling(result);
            completeImages.ForEach(x => x.gameObject.SetActive(false));
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;
            return (correctRows + correctColumns) / (gridSize * gridSize * 2f);
        }

        public override bool IsComplete()
        {
            return sudokuPieces.All(x => x.IsCorrect());
            //return Mathf.FloorToInt((correctRows + correctColumns) / (gridSize * 2f)) == 1;
        }

        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return sudokuPieces;
        }

        public override string GetInstructionText()
        {
            return $"UI/Minigame/Instruction/Sudoku";
        }

        public void CheckMGComplete()
        {
            //Check Correct Rows
            var rows = sudokuPieces.GroupBy(x => x.row).OrderBy(x => x.First().row).ToList();

            foreach (var row in rows)
            {
                foreach(var col in row)
                {
                    if (row.Any(x => x != col && x.groupId.Equals(col.groupId)))
                    {
                        //If any col is not equal to this.
                        col.isInCorrectRow = false;
                    }
                    else
                        col.isInCorrectRow = true;
                }
            }

            //Check Correct Columns
            var cols = sudokuPieces.GroupBy(x => x.col).OrderBy(x => x.First().col).ToList();

            foreach (var col in cols)
            {
                foreach (var row in col)
                {
                    if (col.Any(x => x != row && x.groupId.Equals(row.groupId)))
                    {
                        //If any row is not equal to this.
                        row.isInCorrectColumn = false;
                    }
                    else
                        row.isInCorrectColumn = true;
                }
            }

        }
        void RandomizePositions()
        {
            List<int> positionIndex = new List<int>();

            for(int i = 0; i < sudokuPieces.Count; i++)
            {
                int randomIndex = Random.Range(0, sudokuPieces.Count);
                while(positionIndex.Contains(randomIndex))
                {
                    randomIndex = Random.Range(0, sudokuPieces.Count);
                }

                sudokuPieces[i].transform.localPosition = sudokuPieces[randomIndex].origLocalPosition;
                sudokuPieces[i].boardIndex = randomIndex + 1;
                positionIndex.Add(randomIndex);
            }
        }

        public override void OnStart()
        {
            Setup();

            RandomizePositions();

            CheckMGComplete();
        }

      

        IEnumerator PlaySuccessCo(UnityEngine.Events.UnityAction andThen)
        {
            var pieces = sudokuPieces.OrderByDescending(x => x.transform.localPosition.x);
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

        SudokuPiece PieceFromMouse()
        {
            RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
            if (hit2D != null && hit2D.Length > 0)
            {
                SudokuPiece top = null;
                foreach (var t in hit2D)
                {
                    SudokuPiece piece = t.transform.GetComponent<SudokuPiece>() ;
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

        public override void PlayHint()
        {
            if (selectedPiece != null)
                selectedPiece.OnClick();

            var incorrect = sudokuPieces.FirstOrDefault(x => !x.IsCorrect());

            var randomIncorrect = sudokuPieces.Where(x => x != incorrect && !x.IsCorrect()).OrderBy(x => Random.value).ThenBy(x => x.isInCorrectRow == false).ThenBy(x => x.isInCorrectColumn).First();

            if (incorrect && randomIncorrect)
            {
                incorrect.OnClick();
                randomIncorrect.OnClick();
            }
        }
        void Update()
        {
            if (disableInput || IsComplete())
            {
                if (highlightPiece)
                    highlightPiece.SetSelected(false);
                highlightPiece = null;

                return;
            }

            SudokuPiece top = PieceFromMouse();
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
                    top.OnClick();
                }
            }
        }
    }
}

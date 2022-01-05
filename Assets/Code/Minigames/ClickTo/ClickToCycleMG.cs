using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{

    public class ClickToCycleMG : MinigameBase
    {
        ClickToCyclePiece[] allPieces;
        ClickToCyclePiece highlightPiece;

        public AudioClip onClickAudio => MinigameController.instance.defaultClickAudio;


        public int GoalsComplete { get => allPieces.Count(x => x.isCorrect && x.gameObject.activeInHierarchy); }
        public int TotalGoals { get { return allPieces.Count(x => x.isCorrect); } }

        public override string GetInstructionText()
        {
            return "UI/Minigame/Instruction/ClickToCycle";
        }

        public override bool IsComplete()
        {
            if (allPieces == null) return false;

            return GoalsComplete >= TotalGoals;
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;

            if (allPieces == null) return 0f;

            return (float)GoalsComplete / (float)TotalGoals;
        }

        string GetKey(string pieceName)
        {
            string lastPart = pieceName.Substring(pieceName.LastIndexOf('_') + 1);

            while (!char.IsNumber(lastPart[lastPart.Length - 1]))
            {
                lastPart = lastPart.Substring(0, lastPart.Length - 1);
            }
               

            return lastPart;
        }

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        void Setup()
        {
            if (!Application.isPlaying)
            {
                SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
                foreach (var t in sprites)
                {
                    if (StrReplace.Equals(t.name, "bg")) continue;
                    if (StrReplace.Equals(t.name, "background")) continue;
                    if (t.name.StartsWith("_")) continue;
                    if (t.name.Contains("full") || t.name.Contains("complete"))
                    {
                        completeImages.Add(t.gameObject);
                        continue;
                    }
                    ClickToCyclePiece piece = t.GetComponent<ClickToCyclePiece>();
                    if (piece != null)
                    {
                        DestroyImmediate(piece);
                    }
                }

                var grouped = sprites.Where(x => x.name.StartsWith("_") == false).GroupBy(x => GetKey(x.name)).ToList();

                foreach (var pair in grouped)
                {
                    var first = pair.First();

                    foreach (var sr in pair)
                    {
                        sr.gameObject.AddComponent<ClickToCyclePiece>();
                    }

                    for (int i = 0; i < pair.Count(); i++)
                    {
                        var sr = pair.ElementAt(i);
                        var piece = sr.gameObject.GetComponent<ClickToCyclePiece>();
                        piece.isCorrect = i == 0;

                        if ((i + 1) == pair.Count())
                            piece.next = pair.First().GetComponent<ClickToCyclePiece>();
                        else
                            piece.next = pair.ElementAt(i + 1).GetComponent<ClickToCyclePiece>();
                    }
                }
            }

            completeImages?.ForEach(x => x.SetActive(false));

            allPieces = GetComponentsInChildren<ClickToCyclePiece>(true);
            foreach (var t in allPieces)
            {
                t.gameObject.SetActive(false);
            }

            Random.InitState((int)Time.time);
            foreach (var t in allPieces.Where(x => x.isCorrect))
            {
                var p = t;
                int q = Random.Range(0, 10);
                for (int i = 0; i < q; i++)
                {
                    p = p.next;
                }

                p.gameObject.SetActive(true);
            }
        }

        [Button]
        void Complete()
        {

        }


        [Button]
        public override void OnStart()
        {
            base.OnStart();
            Setup();

        }

        IEnumerator HintCor(ClickToCyclePiece piece)
        {
            while (!piece.isCorrect)
            {
                piece.OnClick();
                piece = piece.next;
                Audio.instance.PlaySound(onClickAudio);

                yield return new WaitForSeconds(0.2f);
            }
            OnPostHintAnimation();
        }

        public override void PlayHint()
        {
            var incorrect = allPieces.FirstOrDefault(x => !x.isCorrect && x.gameObject.activeInHierarchy);
            if (incorrect)
            {
                StartCoroutine(HintCor(incorrect));
            }
        }

        IEnumerator PlaySuccessCo(UnityEngine.Events.UnityAction andThen)
        {
            //VictorySting();
            List<ClickToCyclePiece> completedPieces = allPieces.Where(x => x.isCorrect).ToList();
            while (completedPieces.Count > 0)
            {
                ClickToCyclePiece piece = completedPieces[0];
                completedPieces.RemoveAt(0);
                piece.OnSuccess();
                yield return new WaitForSeconds(0.05f);
            }

            completeImages?.ForEach(x => x.SetActive(true));

            //Delay after complete
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

        ClickToCyclePiece PieceFromMouse()
        {
            RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
            if (hit2D != null && hit2D.Length > 0)
            {
                ClickToCyclePiece top = null;
                foreach (var t in hit2D)
                {
                    ClickToCyclePiece piece = t.transform.GetComponent<ClickToCyclePiece>();
                    if (!piece) continue;
                    if (top == null) top = piece;
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
                {
                    highlightPiece.SetSelected(false);
                    highlightPiece = null;
                }
                return;
            }


            ClickToCyclePiece top = PieceFromMouse();
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
                    Audio.instance.PlaySound(onClickAudio);
                }
            }
        }


        #region Skip Minigame
        [Button]
        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(AnimateSkipMinigame());
        }

        IEnumerator AnimateSkipMinigame()
        {
            //var activePieces = allPieces.Where(x => x.gameObject.activeInHierarchy);
            var incorrectPieces = allPieces.Where(x => x.isCorrect == false && x.gameObject.activeInHierarchy);

            foreach (var piece in incorrectPieces)
            {
                var correctPiece = piece;
                while (!correctPiece.isCorrect)
                {
                    correctPiece.OnClick();
                    correctPiece = correctPiece.next;
                    Audio.instance.PlaySound(onClickAudio);
                    //yield return new WaitForSeconds(0.1f);
                    yield return null;
                }

                //piece.OnFadeOut();
                yield return null;
            }

            disableInput = false;
        }
        #endregion
    }

}
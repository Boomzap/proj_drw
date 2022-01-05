using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class SewMG : MinigameBase
    {
        [ReadOnly]
        public Sprite defaultSprite;

        [ShowInInspector, ReadOnly]
        public List<SewMGPiece> sewMGPieces = new List<SewMGPiece>();

        [SerializeField] List<Sprite> colorSprites = new List<Sprite>();

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        public void Setup()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                sewMGPieces.Clear();
                colorSprites.Clear();

                var exPieces = GetComponentsInChildren<SewMGPiece>(true);
                foreach (var e in exPieces)
                {
                    DestroyImmediate(e);
                }

                

                SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>(true);

                for (int i = 0; i < sprites.Length; i++)
                {
                    var t = sprites[i];

                    if (StrReplace.Equals(t.name, "bg")) continue;
                    if (StrReplace.Equals(t.name, "background")) continue;
                    if (t.name.Contains("full") || t.name.Contains("complete") || t.name.Contains("guide"))
                    {
                        completeImages.Add(t.gameObject);
                        continue;
                    }
                    if (t.name[0] == '_') continue;
                    if (t.name.StartsWith("m_")) continue;

                    if(t.name.Contains("sew_item"))
                    {
                        if(t.name.Contains("default"))
                        {
                            defaultSprite = t.sprite;
                        }
                        colorSprites.Add(t.sprite);
                        t.gameObject.SetActive(false);
                        continue;
                    }

                    SewMGPiece piece = t.gameObject.AddComponent<SewMGPiece>();
                    sewMGPieces.Add(piece);
                    piece.SetupPiece();
                }
            }
#endif
            completeImages.ForEach(x => x.gameObject.SetActive(false));
        }

        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            Debug.Log("For this subgame, Please proceed to next step and skip sdf generation.");
            return null;
        }

        public override string GetInstructionText()
        {
            return $"UI/Minigame/Instruction/SewMG";
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;
            //Debug.Log($"{ paintablePieces.Count(x => x.isCorrect)} / {paintablePieces.Count}");
            return (float)sewMGPieces.Count(x => x.IsCorrect()) / sewMGPieces.Count;
        }

        public override bool IsComplete()
        {
            return sewMGPieces.All(x => x.IsCorrect());
        }

        IEnumerator PlaySuccessCo(UnityEngine.Events.UnityAction andThen)
        {
            //VictorySting();
            //List<PaintablePiece> completedPieces = paintablePieces.Where(x => x.isCorrect).ToList();
            //while (completedPieces.Count > 0)
            //{
            //    PaintablePiece piece = completedPieces[0];
            //    completedPieces.RemoveAt(0);
            //    piece.OnSuccess();
            //    yield return new WaitForSeconds(0.05f);
            //}

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

        public override void PlayHint()
        {
            var incorrect = sewMGPieces.FirstOrDefault(x => !x.IsCorrect());

            if (incorrect)
            {
                incorrect.ForceComplete();
                OnPostHintAnimation();
            }
        }

        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(SkipCor());
        }

        IEnumerator SkipCor()
        {
            var incorrectPieces = sewMGPieces.Where(x => x.IsCorrect() == false).ToList();

            foreach (var piece in incorrectPieces)
            {
                piece.ForceComplete();
                yield return new WaitForSeconds(0.1f);
            }
        }

        SewMGPiece PieceFromMouse()
        {
            RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
            if (hit2D != null && hit2D.Length > 0)
            {
                SewMGPiece top = null;
                foreach (var t in hit2D)
                {
                    SewMGPiece piece = t.transform.GetComponent<SewMGPiece>();
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

        void Update()
        {
            if (disableInput || IsComplete())
            {
                return;
            }

            SewMGPiece top = PieceFromMouse();

            if (Input.GetMouseButton(0))
            {
                Debug.Log("Mouse held down");

                if (top != null)
                {
                    top.OnClick();
                }
                //else
                //    Debug.Log("Clicked Nothing");
            }
        }

        private void Start()
        {
            Sprite defaultSprite = colorSprites.Where(x => x.name.ToLower().Contains("default")).First();
            sewMGPieces.ForEach(x => x.sprite.sprite = defaultSprite);
        }

        private void Awake()
        {
            UIController.instance.minigameUI.SewColorHolder.SetupSewColors(colorSprites);
            UIController.instance.minigameUI.SewColorHolder.sewMG = this;
        }
    }
}


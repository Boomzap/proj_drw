using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class PaintMG : MinigameBase
    {
        public List<ColorData> colors = new List<ColorData>();

        [System.Serializable]
        public class ColorData
        {
            public string colorKey;
            public Color color = new Color(0, 0, 0, 1);
        }

        public Color selectColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        [ReadOnly]
        public List<PaintablePiece> paintablePieces = new List<PaintablePiece>();

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        public void Setup()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                paintablePieces.Clear();

                var exPieces = GetComponentsInChildren<MinigamePiece>(true);
                foreach (var e in exPieces)
                {
                    DestroyImmediate(e);
                }

                SpriteRenderer[] sprites = transform.GetComponentsInChildren<SpriteRenderer>(true);

                if (sprites[0].sprite.texture.isReadable == false)
                {
                    Debug.LogError("Please enable read/write for this psb.");
                    return;
                }

                for (int i = 0; i < sprites.Length; i++)
                {
                    var sprite = sprites[i];
                    
                   

                    if (StrReplace.Equals(sprite.name, "bg")) continue;
                    if (StrReplace.Equals(sprite.name, "background")) continue;
                    if (sprite.name.Contains("full") || sprite.name.Contains("complete"))
                    {
                        completeImages.Add(sprite.gameObject);
                        continue;
                    }
                    if (sprite.name[0] == '_') continue;
                    if (sprite.name.StartsWith("m_")) continue;

                    if (sprite.name.Contains("ui"))
                    {
                        sprite.gameObject.SetActive(false);
                        continue;
                    }
                        

                    var spriteFormat = sprite.name.ToLower().Split('_');

                    if (spriteFormat.Length > 1 && spriteFormat[0].Equals("color") == false)
                    {
                        sprite.gameObject.SetActive(false);
                        continue;
                    }

                    if (sprite.name.Contains("color") && sprite.name.Contains("on") == false)
                        continue;

                    PaintablePiece piece = sprite.gameObject.AddComponent<PaintablePiece>();
                    piece.SetupPiece();
                    paintablePieces.Add(piece);
                }
            }

            SetupColorData();
#endif
        }

        [Button]
        public void ClearColors()
        {
            while(colors.Count != 18)
            {
                colors.RemoveAt(colors.Count - 1);
            }
        }

        [Button]
        public void SetupColorData()
        {
            colors.Clear();
            var colorPallete = transform.GetComponentsInChildren<SpriteRenderer>(true).Where(x => x.name.Contains("ui")).OrderBy(x => int.Parse(x.name.Split('_').Last())).ToList();

            Debug.Log(colorPallete.Count);

            string currentKey = string.Empty;

            foreach (var colorSprite in colorPallete)
            {
                ColorData newColor = new ColorData();

                newColor.colorKey = colorSprite.name.Split('_').Last();
                //newColor.color = spriteTexture.GetPixel((int) colorSprite.sprite.textureRect.x, (int) colorSprite.sprite.textureRect.y);
                newColor.color = colorSprite.sprite.texture.GetPixel((int)colorSprite.sprite.textureRect.x + 10, (int)colorSprite.sprite.textureRect.y + 10) ;
                newColor.color.a = 1f;

                colors.Add(newColor);
            }
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = true;
            //Debug.Log($"{ paintablePieces.Count(x => x.isCorrect)} / {paintablePieces.Count}");
            return  (float) paintablePieces.Count(x => x.isCorrect) / (float) paintablePieces.Count ;
        }

        public override bool IsComplete()
        {
            return paintablePieces.All(x => x.isCorrect);
        }

        PaintablePiece PieceFromMouse()
        {
            RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
            if (hit2D != null && hit2D.Length > 0)
            {
                PaintablePiece top = null;
                foreach (var t in hit2D)
                {
                    PaintablePiece piece = t.transform.GetComponent<PaintablePiece>();
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

        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            Debug.Log("For this subgame, Please proceed to next step and skip sdf generation.");
            return null;
        }

        public override string GetInstructionText()
        {
            return $"UI/Minigame/Instruction/PaintMG";
        }

        void Update()
        {
            if (disableInput || IsComplete())
            { 
                return;
            }

            PaintablePiece top = PieceFromMouse();

            if (Input.GetMouseButton(0))
            {
                //Debug.Log("Mouse held down");

                if (top != null)
                {
                    top.OnClick();
                }
                //else
                //    Debug.Log("Clicked Nothing");
            }
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
            var incorrect = paintablePieces.FirstOrDefault(x => !x.IsCorrect());

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
            var incorrectPieces = paintablePieces.Where(x => x.IsCorrect() == false).ToList();

            foreach(var piece in incorrectPieces)
            {
                piece.ForceComplete();
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void Awake()
        {
            UIController.instance.minigameUI.PaintColorHolder.SetupPaintColors(colors);
            UIController.instance.minigameUI.PaintColorHolder.paintMg = this;
        }
    }
}

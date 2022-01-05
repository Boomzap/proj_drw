using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    public class DiffMG : MinigameBase
    {
	    Collider2D[]						filterResults;

	    int									totalGoals = 0;
	    int									goalsFound;
	    List<DiffMGPiece>    			    allPieces = new List<DiffMGPiece>();

        [SerializeField, Required]
        SpriteRenderer                       [] boundaries;

        Dictionary<SpriteRenderer, MaterialPropertyBlock>   bgMaterials = new Dictionary<SpriteRenderer, MaterialPropertyBlock>();

        float                               hintAlphaCur = 0f, hintAlphaTar = 0f;

        bool pieceFoundLastHint = false;

        public override bool IsComplete()
        {
            return goalsFound >= totalGoals;
        }

        public override string GetInstructionText()
        {
            return "UI/Minigame/Instruction/DiffMG";
        }

        public override string GetProgressDescription()
        {
            return "UI/DifferencesLeft";
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = false;
            return totalGoals - goalsFound;
        }

        void SimplifyMesh(PolygonCollider2D collider)
        {
            // we know that these points are always in a box shape, just sometimes rotated, which box collider can't handle..
            // polygoncollider also takes the 'outlined edge' of the sprite, so we need to simply this to be a bounding rect

            // and it turns out that the away polycolider works is, it just 'cuts out' the innards by a 2 dn path, so .. easy
            collider.pathCount = 1;
        }

        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return allPieces;
        }

        bool IsInsideBoundary(Vector3 piecePosition, Vector3 boundaryPosition, float width)
        {
            //Debug.Log($"Piece Position: {piecePosition} BoundaryPosition: {boundaryPosition} Width: {width} ");
            float halfWidth = width / 2f;
            if (boundaryPosition.x - halfWidth < piecePosition.x && piecePosition.x < boundaryPosition.x + halfWidth)
                return true;

            return false;
        }

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01)" +
             "\n\n You'll also need to add the left & right boundaries to the prefab, located in the Assets/_Minigames/Diff Folder", InfoMessageType = InfoMessageType.Warning)]
        void Setup()
        {
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>(true);

            boundaries = sprites.Where(x => x.name.ToLowerInvariant().Contains("boundary")).ToArray();

            if (boundaries == null || boundaries.Length <= 1)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Missing setup", "Please setup the boundaries first", "OK");

#endif
                Debug.LogError("Missing boundaries");
                return;
            }

            goalsFound = 0;
            totalGoals = 0;
          
            if(Application.isPlaying == false)
            {
                foreach (var t in sprites)
                {
                    //Note: SDFs are also in sprites
                    if (t == null) continue;
                    if (StrReplace.Equals(t.name, "bg")) continue;
                    if (StrReplace.Equals(t.name, "background")) continue;
                    if (t.name[0] == '_') continue;
                    if (t.name.ToLower().Contains("_alt")) continue;
                    if (t.name.ToLower().Contains("boundary"))
                    {
                        t.gameObject.SetActive(false);
                        continue;
                    }

                    DiffMGPiece piece = t.GetComponent<DiffMGPiece>();

                    if (piece == null)
                    {
                        piece = t.gameObject.AddComponent<DiffMGPiece>();
                    }

                    //SimplifyMesh(piece.Collider);

                    totalGoals++;
                    piece.IsGoal = true;


                    piece.alters.ForEach(x => DestroyImmediate(x.gameObject));
                    piece.alters.Clear();

                    //Check which boundary the piece belongs

                    int altIndex = 0;

                    var currentBoundary = boundaries.First(x => IsInsideBoundary(piece.transform.position, x.transform.position, x.size.x));

                    //Debug.Log(currentBoundary.name);

                    foreach (var boundary in boundaries)
                    {
                        //Skip current boundary
                        if (boundary == currentBoundary) continue;

                        //Increment current index
                        altIndex++;

                        GameObject alter = new GameObject(t.name + $"_alt{altIndex:D2}", typeof(SpriteRenderer));

                        if (piece.alters.Any(x => x.name == alter.name)) continue;

                        SpriteRenderer altSprite = alter.GetComponent<SpriteRenderer>();
                        altSprite.sprite = t.sprite;

                        float posX = piece.transform.localPosition.x - currentBoundary.transform.localPosition.x; ;

                        alter.transform.localPosition = new Vector3(posX + boundary.transform.localPosition.x, piece.transform.localPosition.y, 0);

                        alter.transform.SetParent(t.transform, true);

                        altSprite.sortingOrder = t.sortingOrder;

                        alter.AddComponent<BoxCollider2D>();

                        altSprite.enabled = false;

                        piece.alters.Add(alter);
                    }
                }

            }

            allPieces = new List<DiffMGPiece>(GetComponentsInChildren<DiffMGPiece>());

            totalGoals = allPieces.Count;

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GenerateSDFs();

                foreach (var piece in allPieces)
                {
                    foreach(var alter in piece.alters)
                    {
                        if (alter != null)
                            alter.GetComponent<SpriteRenderer>().sprite = piece.sdfRenderer.sprite;
                    }
                }
            }
            #endif
	    }

        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(SkipCor());
        }

        IEnumerator SkipCor()
        {
            var incomplete = allPieces.Where(x => !x.IsComplete).ToList();

            foreach (var p in incomplete)
            {
                p.Select();
                goalsFound++;

                yield return new WaitForSeconds(0.2f);
            }

            disableInput = false;
        }

        [Button] void Complete()
	    {
		    goalsFound = totalGoals;
	    }

	    public override void PlaySuccess(UnityEngine.Events.UnityAction andThen)
	    {
		    // animation here?
		    VictorySting();
		
            andThen?.Invoke();
	    }


        // Start is called before the first frame update
        void Start()
        {
		    Setup();

            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            bgMaterials = new Dictionary<SpriteRenderer, MaterialPropertyBlock>();
            foreach (var s in sprites)
            {
                if (s.name.StartsWith("_") || s.name.ToLower() == "background")
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

            pieceFoundLastHint = true;
        }

	    DiffMGPiece	PieceFromMouse()
	    {
 		    RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
            if (hit2D != null && hit2D.Length > 0)
            {
                DiffMGPiece top = null;
                foreach (var t in hit2D)
                {
                    DiffMGPiece piece = t.transform.GetComponent<DiffMGPiece>();
                    if (piece)
                    {
                        if (piece.IsComplete) continue;
                        if (top == null || piece.SortValue > top.SortValue)
                        {
                            top = piece;
                            continue;
                        }
                    }

                    foreach (var p in allPieces)
                    {
                        if (!p.IsComplete && (p.alters.Contains(t.transform.gameObject)))
                        {
                            if (top == null || p.SortValue > top.SortValue)
                            {
                                top = p;
                                break;
                            }
                        }
                    }
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

            UpdateMusic();
		    
            if (Input.GetMouseButtonDown(0))
            {
                DiffMGPiece top = PieceFromMouse();    
                
                if (top)
                {
                    if(MinigameController.instance.isHintPlaying)
                    {
                        pieceFoundLastHint = true;
                    }
                    top.Select();
                    goalsFound++;
                }
            }
	    }

        public override void PlayHint()
        {
            //             var piece = allPieces.FirstOrDefault(x => !x.IsComplete);
            //             if (piece == null) return;
            // 
            //             piece.Select();
            //             goalsFound++;
            // 
            //             TapFeedbackFX.instance.CreateAtWorldPos(piece.transform.position);

            StartCoroutine(HintCor());
        }

        IEnumerator HintCor()
        {
            hintAlphaTar = .75f;

            foreach (var v in allPieces.Where(x => x.IsComplete))
                v.FadeAlpha = hintAlphaTar;

            if (pieceFoundLastHint == false)
            {
                yield return new WaitForSeconds(.5f);
                var piece = allPieces.FirstOrDefault(x => !x.IsComplete);
                piece.GlowPiece();
            }
            else
                pieceFoundLastHint = false;

            yield return new WaitForSeconds(5f);

            hintAlphaTar = 0f;

            foreach (var v in allPieces.Where(x => x.IsComplete))
                v.FadeAlpha = hintAlphaTar;

            OnPostHintAnimation();
        }
    }

}
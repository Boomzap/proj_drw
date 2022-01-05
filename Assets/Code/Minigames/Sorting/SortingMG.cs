using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{

    public class SortingMG : MinigameBase
    {
	    Collider2D[]						filterResults;

	    int									totalGoals = 0;
	    int									goalsFound;
	    List<SortingMGPiece>    			allPieces = new List<SortingMGPiece>();

	    SortingMGPiece						lastTop = null;
        SortingMGPiece                      pairSelected = null;

        float                               hintFadeAlpha = 0f;

        protected override IEnumerable<MinigamePiece> GetInteractivePartsForSDFGeneration()
        {
            return allPieces;
        }

        public override bool IsComplete()
        {
            return goalsFound >= totalGoals;
        }

        public override float GetCompletionProgress(out bool showAsPercent)
        {
            showAsPercent = false;
            return totalGoals - goalsFound;
        }
        public override string GetInstructionText()
        {
            return "UI/Minigame/Instruction/SortingMG";
        }

        public override string GetProgressDescription()
        {
            return "UI/PiecesLeft";
        }

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        void Setup()
	    {
        #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var exPieces = new List<SortingMGPiece>(GetComponentsInChildren<SortingMGPiece>(true));
                foreach (var e in exPieces)
                {
                    DestroyImmediate(e);
                }
            }
        #endif


		    SpriteRenderer[]	sprites = GetComponentsInChildren<SpriteRenderer>();
		    goalsFound = 0;
		    totalGoals = 0;
		    foreach (var t in sprites)
		    {
			    if (StrReplace.Equals(t.name, "bg")) continue;
			    if (StrReplace.Equals(t.name, "background")) continue;
			    if (t.name[0] == '_') continue;
			    SortingMGPiece piece = t.GetComponent<SortingMGPiece>();
			    if (piece == null) 
			    {
				    piece = t.gameObject.AddComponent<SortingMGPiece>();
			    }
			    //if (SearchArray<SpriteRenderer>.FindFirst(goalPieces, (SpriteRenderer item) => StrReplace.Equals(item.name, piece.name)) != null)
			    {
				    totalGoals++;
				    piece.IsGoal = true;

                    piece.pairKey = piece.name.Split('_')[1].ToLower();
                    piece.pairKey = piece.pairKey.Substring(0, piece.pairKey.Length - 1);
			    }
		    }
		    allPieces = new List<SortingMGPiece>(GetComponentsInChildren<SortingMGPiece>());
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
            pairSelected = null;
            UpdateFree();
        }

	    bool IsFree(SortingMGPiece piece)
	    {
		    filterResults =  new Collider2D[allPieces.Count];


		    ContactFilter2D contactFilter = new ContactFilter2D();
		    contactFilter.useTriggers = false;
		    contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(piece.gameObject.layer));
		    contactFilter.useLayerMask = true;
		    int count = Physics2D.OverlapCollider(piece.Collider, contactFilter, filterResults);
		    for (int i=0; i<count; i++)
		    {
			    SortingMGPiece current = filterResults[i].GetComponent<SortingMGPiece>();
			    if (current == null) continue;
                if (current.IsComplete) continue;
			    if (current.SortValue < piece.SortValue) continue; // below
			    return false;
		    }
		    return true;
	    }

        void UpdateFree()
        {
            foreach (var p in allPieces)
            {
                if (p.IsComplete) continue;

                bool free = IsFree(p);
                p.IsFree = free;

                if (free)
                    p.FadeAlpha = 0f;
            }
        }

        public override void PlayHint()
        {
            StartCoroutine(HintCor());
        }

        IEnumerator HintCor()
        {
            hintFadeAlpha = 1f;
            
            yield return new WaitForSeconds(MinigameController.instance.SortingHintDuration);

            hintFadeAlpha = 0f;

            OnPostHintAnimation();
        }

        void OnPiecesCorrect(SortingMGPiece p1, SortingMGPiece p2)
        {
            p1.OnPickup();
            p2.OnPickup();

            allPieces.Remove(p1);
            allPieces.Remove(p2);

            goalsFound += 2;

            pairSelected = null;
            lastTop = null;

            UpdateFree();
        }

        SortingMGPiece	PieceFromMouse()
	    {
		    RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
		    if(hit2D != null && hit2D.Length > 0)
		    {
			    SortingMGPiece top = null;
			    foreach (var t in hit2D)
			    {
				    SortingMGPiece piece = 	t.transform.GetComponent<SortingMGPiece>();
				    if (!piece || piece.IsComplete) continue;
				    if (top == null || piece.SortValue > top.SortValue) top = piece;
			    }
			    if (top != null && IsFree(top))
			    {
				    return top;
			    }
		    }
		    return null;
	    }

        IEnumerator SkipCor()
        {
            string[] incomplete;
            do
            {
                incomplete = allPieces.Where(x => !x.IsComplete).GroupBy(x => x.pairKey).Select(x => x.Key).ToArray();
                var allFree = allPieces.Where(x => x.IsFree).ToArray();

                foreach (var key in incomplete)
                {
                    var pieces = allPieces.Where(x => x.pairKey == key && x.IsFree).ToArray();


                    if (pieces.Length != 2) continue;

                    foreach (var p in pieces)
                    {
                        p.SetSelected(false);
                    }

                    OnPiecesCorrect(pieces[0], pieces[1]);

                    break;
                }

                yield return new WaitForSeconds(0.2f);

                if (Input.GetKey(KeyCode.T) == true)
                    break;

            } while (incomplete.Length > 0);


            disableInput = false;
        }

        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(SkipCor());
        }


        // Update is called once per frame
        void Update()
        {
		    UpdateMusic();

            foreach (var f in allPieces.Where(x => !x.IsFree))
            {
                f.FadeAlpha = hintFadeAlpha;
            }
            
            if (disableInput)
            {
                if (lastTop != null) lastTop.SetSelected(false);
                return;
            }


		    SortingMGPiece top = PieceFromMouse();
		    if (top != lastTop)
		    {
			    if (lastTop && lastTop != pairSelected) lastTop.SetSelected(false);
			    if (top) top.SetSelected(true);
			    lastTop = top;
		    }

		    if (Input.GetMouseButtonDown(0) )
		    {
                if (top != null)
                {
                    top.OnClick();

                    if (top == pairSelected)
                    {
                        pairSelected.SetSelected(false);
                        pairSelected = null;
                    } else if (pairSelected != null)
                    {
                        if (top.pairKey.Equals(pairSelected.pairKey))
                        {
                            //correct
                            OnPiecesCorrect(top, pairSelected);
                        } else
                        {
                            //incorrect
                            pairSelected.SetSelected(false);
                            pairSelected = null;
                        }
                    } else
                    {
                        // select
                        pairSelected = top;
                        pairSelected.SetSelected(true);
                    }
                }
                
		    }
	    }
    }

}
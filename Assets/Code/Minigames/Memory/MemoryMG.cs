using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace ho
{
    public class MemoryMG : MinigameBase
    {
	    Collider2D[]						filterResults;

	    int									totalGoals = 0;
	    int									goalsFound;
	    List<MemoryMGPiece>    			    allPieces = new List<MemoryMGPiece>();

	    MemoryMGPiece						lastTop = null;
        MemoryMGPiece                       pairSelected = null;

        public Sprite                       cardbackSprite;

        List<MemoryMGPiece>                 busyPieces = new List<MemoryMGPiece>();

        public AudioClip                    flipSound;

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
            showAsPercent = true;
            return (float)goalsFound / (float)totalGoals;
        }

        [BoxGroup("Initial Setup"), Button("Step 1: Setup MG", ButtonSizes.Large), PropertyOrder(0f)]
        [InfoBox("NOTE: Before Setup, add an '_' before an object's name if it's not a puzzle piece. (E.g. m_01 -> _m_01). ", InfoMessageType = InfoMessageType.Warning)]
        void Setup()
	    {
        #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var exPieces = GetComponentsInChildren<MemoryMGPiece>(true).ToArray();
                foreach (var e in exPieces)
                {
                    DestroyImmediate(e);
                }
            }
        #endif

            busyPieces.Clear(); 
		    SpriteRenderer[]	sprites = GetComponentsInChildren<SpriteRenderer>();
		    goalsFound = 0;
		    totalGoals = 0;
		    foreach (var t in sprites)
		    {
			    if (StrReplace.Equals(t.name, "bg")) continue;
			    if (StrReplace.Equals(t.name, "background")) continue;
			    if (t.name[0] == '_') continue;
			    
                if (t.name.Contains("cardback"))
                {
                    cardbackSprite = t.sprite;
                    t.enabled = false;
                    continue;
                }

                MemoryMGPiece piece = t.GetComponent<MemoryMGPiece>();
			    if (piece == null) 
			    {
				    piece = t.gameObject.AddComponent<MemoryMGPiece>();
			    }
			    
				totalGoals++;
				piece.IsGoal = true;

                piece.pairKey = piece.name.Split('_')[1].ToLower();
                piece.pairKey = piece.pairKey.Substring(0, piece.pairKey.Length - 1);
                piece.owner = this;
		    }

		    allPieces = new List<MemoryMGPiece>(GetComponentsInChildren<MemoryMGPiece>());
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

            if (allPieces.Count > 1)
            {
                for (int i = 0; i < allPieces.Count; i++)
                {
                    Vector3 a = allPieces[i].transform.localPosition;

                    int j;
                    do
                    {
                        j = UnityEngine.Random.Range(0, allPieces.Count);
                    } while (j == i);

                    allPieces[i].transform.localPosition = allPieces[j].transform.localPosition;
                    allPieces[j].transform.localPosition = a;
                }
            }
        }

        IEnumerator SkipCor()
        {
            var incomplete = allPieces.Where(x => !x.IsComplete).GroupBy(x => x.pairKey).Select(x => x.Key).ToArray();

            foreach (var key in incomplete)
            {
                var pieces = allPieces.Where(x => x.pairKey == key).ToArray();

                foreach (var p in pieces)
                {
                    p.SetSelected(false);
                    p.SetFlipped(true);
                }

                yield return new WaitForSeconds(0.3f);

                foreach (var p in pieces)
                {
                    allPieces.Remove(p);

                    goalsFound ++;

                    p.OnPickup();
                }
            }
            

            disableInput = false;
        }

        public override void Skip()
        {
            disableInput = true;

            StartCoroutine(SkipCor());
        }

        MemoryMGPiece	PieceFromMouse()
	    {
		    RaycastHit2D[] hit2D = Physics2D.GetRayIntersectionAll(GameController.instance.currentCamera.ScreenPointToRay(Input.mousePosition));
		    if(hit2D != null && hit2D.Length > 0)
		    {
			    MemoryMGPiece top = null;
			    foreach (var t in hit2D)
			    {
				    MemoryMGPiece piece = 	t.transform.GetComponent<MemoryMGPiece>();
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
            StartCoroutine(HintCor());
        }

        IEnumerator HintCor()
        {
            disableInput = true;

            bool exitCor = false;

            if (lastTop != null)
            {
                lastTop.SetSelected(false);
            }

            lastTop = null;

            if (pairSelected == null)
            {
                //top.SetFlipped(true);
                //pairSelected = top;
                var hintCard = allPieces.FirstOrDefault(x => !busyPieces.Contains(x) && !x.IsComplete);

                if (hintCard == null)
                {
                    exitCor = true;
                } else
                {
                    hintCard.SetFlipped(true);
                    TapFeedbackFX.instance.CreateAtWorldPos(hintCard.transform.position);
                }

                pairSelected = hintCard;
                yield return new WaitForSeconds(0.2f);
            }

            if (!exitCor)
            {
                var pairCard = allPieces.FirstOrDefault(x => !busyPieces.Contains(x) && !x.IsComplete && x.pairKey == pairSelected.pairKey && x != pairSelected);

                pairCard.SetFlipped(true);
                TapFeedbackFX.instance.CreateAtWorldPos(pairCard.transform.position);

                yield return new WaitForSeconds(0.3f);

                StartCoroutine(UnflipCor(true, 1f, pairSelected, pairCard));
            }

            disableInput = false;

            pairSelected = null;

            OnPostHintAnimation();
        }

        IEnumerator UnflipCor(bool wasCorrect, float delay, MemoryMGPiece c1, MemoryMGPiece c2)
        {
            busyPieces.Add(c1);
            busyPieces.Add(c2);

            yield return new WaitForSeconds(delay);

            if (wasCorrect)
            {
                Audio.instance.PlaySound(MinigameController.instance.onPieceCorrect.GetClip(null));

                c1.OnPickup();
                c2.OnPickup();

                allPieces.Remove(c1);
                allPieces.Remove(c2);

                goalsFound+=2;
            } else
            {
                c1.SetFlipped(false);
                c2.SetFlipped(false);
            }

            yield return new WaitForSeconds(0.3f);

            busyPieces.Remove(c1);
            busyPieces.Remove(c2);
        }

	    // Update is called once per frame
	    void Update()
        {
            if (disableInput)
            {
                if (lastTop != null) lastTop.SetSelected(false);
                return;
            }

		    UpdateMusic();
		    MemoryMGPiece top = PieceFromMouse();

		    if (top != lastTop)
		    {
			    if (lastTop) lastTop.SetSelected(false);
			    if (top) top.SetSelected(true);
			    lastTop = top;
		    }

		    if (Input.GetMouseButtonDown(0) && top != null && busyPieces.Count == 0)
		    {
                
                if (top == pairSelected)
                {
                    pairSelected.SetFlipped(false);
                    pairSelected = null;
                } else if (pairSelected != null)
                {
                    if (top.pairKey.Equals(pairSelected.pairKey))
                    {
                        top.SetFlipped(true);

                        //correct
                        StartCoroutine(UnflipCor(true, 1f, top, pairSelected));

                        pairSelected = null;
                        lastTop = null;
                    } else
                    {
                        //incorrect
                        top.SetFlipped(true);
                        StartCoroutine(UnflipCor(false, 1f, top, pairSelected));
                        pairSelected = null;
                    }
                } else
                {
                    //// select
                    top.SetFlipped(true);
                    pairSelected = top;
                }
            }
        }

        public override string GetInstructionText()
        {
            return "UI/Minigame/Instruction/Memory";
        }
       
    }

}
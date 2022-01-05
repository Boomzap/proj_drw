using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class MemoryMGPiece : MinigamePiece
    {
	    [SerializeField] bool	isGoal;
	    //[SerializeField] float	popDuration = 0.5f;
	    int						sortValue = 0;
	    
	    PolygonCollider2D		polyCollider;
	    bool					isComplete = false;
	    public int	SortValue	{ get { return sortValue; } }
	    public bool IsGoal		{ get { return isGoal; } set { isGoal = value; } }
	    public bool IsComplete	{ get { return isComplete; } }
	    public PolygonCollider2D	Collider { get { return polyCollider; } } 
	    bool					isSelected  = false;

        public string           pairKey;
        public Sprite           backfaceSprite;
        public Sprite           cardSprite;

        public MemoryMG         owner;
        
        public bool isFlipped = false;
        Coroutine flipCorHandle = null;

        public float FadeAlpha { get; set; } = 0f;
        float fadeAlphaCur = 0f;

        // Start is called before the first frame update
        void Start()
        {
		    sprite = GetComponent<SpriteRenderer>();

		    sortValue = sprite.sortingOrder;
		    polyCollider = GetComponent<PolygonCollider2D>();

            cardSprite = sprite.sprite;   
            backfaceSprite = owner.cardbackSprite;

            sprite.sprite = backfaceSprite;
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);

            sdfRenderer.material = MinigameController.instance.MemoryMouseoverMaterial;
            sdfRenderer.color = new Color(1f, 1f, 1f, 0f);
        }


	    public void SetSelected(bool _isSelected)
	    {
		    isSelected = _isSelected;
            
            FadeAlpha = isSelected ? 1f : 0f;
	    }

        public void SetFlipped(bool b)
        {
            if (b == isFlipped) return;
            isFlipped = b;

            if (owner.flipSound != null)
                Audio.instance.PlaySound(owner.flipSound);

            if (flipCorHandle != null)
                StopCoroutine(flipCorHandle);
            flipCorHandle = StartCoroutine(FlipCor(b));
        }

        IEnumerator FlipCor(bool b)
        {
            if (owner.flipSound != null)
            {
                Audio.instance.PlaySound(owner.flipSound);
            }

            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 250f);

            float targetRot = 90f;
            float sourceRot = transform.localEulerAngles.y;

            float dist = Mathf.DeltaAngle(sourceRot, targetRot);
            float time = 0.15f;

            float timer = 0f;

            while (timer < time)
            {
                transform.localEulerAngles = new Vector3(0f, Mathf.Lerp(sourceRot, targetRot, (timer / time)), 0f);
                timer += Time.deltaTime;
                yield return null;
            }
            transform.localEulerAngles = new Vector3(0f, targetRot, 0f);

            sprite.sprite = b ? cardSprite : backfaceSprite;

            timer = 0f;
            time = 0.15f;
            sourceRot = 90f;
            targetRot = b ? 0f : 180f;

            while (timer < time)
            {
                transform.localEulerAngles = new Vector3(0f, Mathf.Lerp(sourceRot, targetRot, (timer / time)), 0f);
                timer += Time.deltaTime;
                yield return null;
            }
            transform.localEulerAngles = new Vector3(0f, targetRot, 0f);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
        }

        public void AnimFaceSwap()
        {
            if (isFlipped)
            {
                isFlipped = false;
                sprite.sprite = backfaceSprite;
            } else
            {
                isFlipped = true;
                sprite.sprite = cardSprite;
            }
        }

	    public void OnPickup()
	    {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 250f);
            //StartCoroutine(OnPickupCo());
            Audio.instance.PlaySound(MinigameController.instance.onPieceCorrect.GetClip(null));
            isComplete = true;
            //polyCollider.enabled = false;
            sdfRenderer.gameObject.SetActive(false);

            Animation ani = GetComponent<Animation>();
            if (ani != null && ani.GetClipCount() > 0)
            {
                gameObject.PlayAnimation(this, "mg_mem_pop", () => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
	    }

        // Update is called once per frame
        void Update()
        {
            float dt = Time.deltaTime * 4f;

            if (fadeAlphaCur > FadeAlpha)
            {
                fadeAlphaCur -= dt;
                if (fadeAlphaCur < FadeAlpha)
                    fadeAlphaCur = FadeAlpha;
            }
            else if (fadeAlphaCur < FadeAlpha)
            {
                fadeAlphaCur += dt;
                if (fadeAlphaCur > FadeAlpha)
                    fadeAlphaCur = FadeAlpha;
            }

            sdfRenderer.color = new Color(1f, 1f, 1f, fadeAlphaCur);
            sdfRenderer.gameObject.SetActive(fadeAlphaCur > 0f);
        }

    }

}
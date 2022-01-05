using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class SortingMGPiece : MinigamePiece
    {
	    [SerializeField] bool	isGoal;

	    int						sortValue = 0;
	    
	    PolygonCollider2D		polyCollider;
	    bool					isComplete = false;
	    public int	SortValue	{ get { return sortValue; } }
	    public bool IsGoal		{ get { return isGoal; } set { isGoal = value; } }
	    public bool IsComplete	{ get { return isComplete; } }
	    public PolygonCollider2D	Collider { get { return polyCollider; } } 
	    bool					isSelected  = false;
	    Material				defaultMaterial;

        public string           pairKey;

        public bool IsFree {  get; set; } = false;
        public float FadeAlpha{  get; set; } = 0f;
        float fadeAlphaCur = 0f;

        MaterialPropertyBlock materialPropertyBlock;

        // Start is called before the first frame update
        void Awake()
        {

		    sortValue = sprite.sortingOrder;
		    polyCollider = GetComponent<PolygonCollider2D>();
		    defaultMaterial = sprite.material;

            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
            materialPropertyBlock.SetFloat("_DesatIntensity", 0f);
            materialPropertyBlock.SetFloat("_LightIntensity", 0f);

            sdfRenderer.material = MinigameController.instance.SortingSelectedObjectOutlineMaterial;
            sdfRenderer.gameObject.SetActive(false);
            sdfRenderer.color = new Color(1f, 1f, 1f, 0f);
            sdfRenderer.sortingOrder = sortValue - 1;

            sprite.SetPropertyBlock(materialPropertyBlock);
        }

        public void OnClick()
        {
            Audio.instance.PlaySound(MinigameController.instance.onPieceSelected.GetClip(null));
        }

	    public void SetSelected(bool _isSelected)
	    {
            if (isSelected == _isSelected) return;
		    isSelected = _isSelected;

            StopAllCoroutines();
            StartCoroutine(AnimGlowCor(isSelected ? 1f : 0f));
        }

        IEnumerator AnimGlowCor(float target)
        {
            if (target > 0f)
                sdfRenderer.gameObject.SetActive(true);

            while (!Mathf.Approximately(target, sdfRenderer.color.a))
            {
                float newa = 0f;
                float dt = Time.deltaTime * 4f;

                if (sdfRenderer.color.a > target)
                {
                    newa = sdfRenderer.color.a - dt;
                    if (newa < target) newa = target;
                } else
                {
                    newa = sdfRenderer.color.a + dt;
                    if (newa > target) newa = target;
                }

                Color nc = Color.white;
                nc.a = newa;
                sdfRenderer.color = nc;

                yield return null;
            }

            if (target <= 0f)
                sdfRenderer.gameObject.SetActive(false);
        }

	    public void OnPickup()
	    {
            //StartCoroutine(OnPickupCo());
            Audio.instance.PlaySound(MinigameController.instance.onPieceCorrect.GetClip(null));
            isComplete = true;
            //polyCollider.enabled = false;
            sdfRenderer.gameObject.SetActive(false);

            Animation ani = GetComponent<Animation>();
            if (ani != null && ani.GetClipCount() > 0)
            {
                gameObject.PlayAnimation(this, "mg_sort_pop", () => gameObject.SetActive(false));
            } else
            {
                gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            sprite.material = IsFree ? defaultMaterial : MinigameController.instance.InactiveObjectMaterial;

            float dt = Time.deltaTime * 2f;

            if (fadeAlphaCur > FadeAlpha)
            {
                fadeAlphaCur -= dt;
                if (fadeAlphaCur < FadeAlpha)
                    fadeAlphaCur = FadeAlpha;
            } else if (fadeAlphaCur < FadeAlpha)
            {
                fadeAlphaCur += dt;
                if (fadeAlphaCur > FadeAlpha)
                    fadeAlphaCur = FadeAlpha;
            }
            
            materialPropertyBlock.SetFloat("_DesatIntensity", MinigameController.instance.InactiveDesatFactor * fadeAlphaCur);
            materialPropertyBlock.SetFloat("_LightIntensity", MinigameController.instance.InactiveBrightenFactor * fadeAlphaCur);
            sprite.SetPropertyBlock(materialPropertyBlock);
        }

    }

}
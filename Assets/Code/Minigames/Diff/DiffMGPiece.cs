using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DiffMGPiece : MinigamePiece
    {
	    [SerializeField] bool	isGoal;
	    int						sortValue = 0;
	    
        bool					isComplete = false;
	    public int	SortValue	{ get { return sortValue; } }
	    public bool IsGoal		{ get { return isGoal; } set { isGoal = value; } }
	    public bool IsComplete	{ get { return isComplete; } }
	    public BoxCollider2D    Collider { get { return GetComponent<BoxCollider2D>(); } } 

        public List<GameObject>      alters = new List<GameObject>();

        Material                matInstance;
        public float FadeAlpha { get; set; } = 0f;
        float fadeAlphaCur = 0f;

        MaterialPropertyBlock materialPropertyBlock;

        // Start is called before the first frame update
        void Start()
        {
            sprite = GetComponent<SpriteRenderer>();
            //sprite.enabled = false;

            sortValue = sprite.sortingOrder;
            


            sprite.material = MinigameController.instance.InactiveObjectMaterial;
            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetFloat("_DesatIntensity", MinigameController.instance.InactiveDesatFactor * fadeAlphaCur);
            materialPropertyBlock.SetFloat("_LightIntensity", MinigameController.instance.InactiveBrightenFactor * fadeAlphaCur);
            materialPropertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
            sprite.SetPropertyBlock(materialPropertyBlock);

        }

        // Update is called once per frame
        void Update()
        {
            float dt = Time.deltaTime * 2f;

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

            materialPropertyBlock.SetFloat("_DesatIntensity", MinigameController.instance.InactiveDesatFactor * fadeAlphaCur);
            materialPropertyBlock.SetFloat("_LightIntensity", MinigameController.instance.InactiveBrightenFactor * fadeAlphaCur);
            sprite.SetPropertyBlock(materialPropertyBlock);
        }

        IEnumerator FoundAnimCor()
        {
            const float animTime = 1f;
            float time = 0f;

            while (time < animTime)
            {
                float a = time / animTime;
                float g = Mathf.Sin(a * Mathf.PI);

                matInstance.SetFloat("_GlowAlpha", g);
                sprite.color = new Color(1f, 1f, 1f, (1f-a) * 0.7f);

                time += Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
            alters.ForEach(x => Destroy(x.gameObject));
        }

        public void Select()
        {
            if (isComplete) return;
            
            isComplete = true;


            //sprite.enabled = true;
            //alter.GetComponent<SpriteRenderer>().enabled = true;

            matInstance = Instantiate(MinigameController.instance.FTDSDFGlowMaterial);

            sdfRenderer.gameObject.SetActive(true);

            foreach(var alter in alters)
            {
                alter.GetComponent<SpriteRenderer>().enabled = true;
                alter.GetComponent<SpriteRenderer>().sortingOrder = sdfRenderer.sortingOrder;
                sdfRenderer.material = matInstance;
                alter.GetComponent<SpriteRenderer>().material = matInstance;
            }

            matInstance.SetFloat("_GlowAlpha", 0f);

            StopAllCoroutines();
            StartCoroutine(FoundAnimCor());

            // playsound
            Audio.instance.PlaySound(MinigameController.instance.FTDFoundAudio);
        }

        public void GlowPiece()
        {
            matInstance = Instantiate(MinigameController.instance.FTDSDFGlowMaterial);
            sdfRenderer.gameObject.SetActive(true);
            sdfRenderer.material = matInstance;

            alters.ForEach(x => x.GetComponent<SpriteRenderer>().material = matInstance);

            matInstance.SetFloat("_GlowAlpha", 0f);
            StartCoroutine(GlowPieceCor());
        }

        IEnumerator GlowPieceCor()
        {
            const float animTime = 1f;
            float time = 0f;

            while (time < animTime)
            {
                float a = time / animTime;
                float g = Mathf.Sin(a * Mathf.PI);

                matInstance.SetFloat("_GlowAlpha", g);
                //sprite.color = new Color(1f, 1f, 1f, (1f - a) * 0.7f);

                time += Time.deltaTime;

                yield return null;
            }

            sdfRenderer.gameObject.SetActive(false);
        }

    }
}
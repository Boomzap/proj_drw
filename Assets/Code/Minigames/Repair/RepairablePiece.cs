using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho 
{
    public class RepairablePiece : MinigamePiece
    {
        [ReadOnly] public PolygonCollider2D pieceCollider;

        [BoxGroup("Repair Types")]
        public RepairType repairType = RepairType.None;

        [HideInInspector]
        public RepairGroup repairGroup;

        Color alphaColor = new Color(1,1,1,1);
        bool repaired = false;
        bool isRepairing = false;

        int sortValue = 0;
        public int SortValue { get { return sortValue; } }

        public bool isRepaired { get { return repaired; } }

        public bool isRepairable { get { return pieceCollider.enabled; } }

        public float FadeAlpha { get; set; } = 0f;
        float fadeAlphaCur = 0f;
        MaterialPropertyBlock materialPropertyBlock;

        private void Awake()
        {
            sortValue = sprite.sortingOrder;
            sprite.material = MinigameController.instance.InactiveObjectMaterial;
            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetFloat("_DesatIntensity", MinigameController.instance.InactiveDesatFactor * fadeAlphaCur);
            materialPropertyBlock.SetFloat("_LightIntensity", MinigameController.instance.InactiveBrightenFactor * fadeAlphaCur);
            materialPropertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
            sprite.SetPropertyBlock(materialPropertyBlock);
        }


#if UNITY_EDITOR
        [Button] void UpFadeAlpha()
        {
            FadeAlpha = 1;
        }

        [Button]
        void DownFadeAlpha()
        {
            FadeAlpha = 0;
        }
#endif

        private void Update()
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

            materialPropertyBlock.SetFloat("_DesatIntensity", MinigameController.instance.InactiveDesatFactor * fadeAlphaCur);
            materialPropertyBlock.SetFloat("_LightIntensity", MinigameController.instance.InactiveBrightenFactor * fadeAlphaCur);
            sprite.SetPropertyBlock(materialPropertyBlock);

            //sdfRenderer.color = new Color(1f, 1f, 1f, fadeAlphaCur);
            //sdfRenderer.gameObject.SetActive(fadeAlphaCur > 0f);
        }

        private void OnMouseDown()
        {
#if UNITY_EDITOR
            Debug.Log("Mouse Hit: " + gameObject.name +" Repair Type: "+ repairType);
#endif
            if (repaired == false && repairType == MinigameController.instance.ActiveMinigameAsType<RepairMG>().currentRepairType)
            {
                OnRepairPiece();
            }
        }

        public void OnRepairPiece()
        {
            //NOTE* Update this if Repair types for an item is more than one
            repaired = true;

            Audio.instance.PlaySound(MinigameController.instance.onPieceCorrect.GetClip(null));

            //Prevents Multiple Click
            if (isRepairing == false)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateFade());
            }
        }

        IEnumerator AnimateFade()
        {
            isRepairing = true;
            //Play Some Audio

            //Speed in Milliseconds
            float fadeSpeed = 0.05f;

            sprite.color = new Color(1, 1, 1, 1);

            while(alphaColor.a > 0)
            {
                alphaColor.a -= fadeSpeed;
                sprite.color = alphaColor;
                yield return new WaitForSeconds(0.05f);
            }

            repairGroup.OnRepairedPiece(this);
            MinigameController.instance.ActiveMinigameAsType<RepairMG>().OnRepairedPiece(this);

            gameObject.SetActive(false);
        }

    }
}



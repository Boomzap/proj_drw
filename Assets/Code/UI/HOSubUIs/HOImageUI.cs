using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;

namespace ho
{
    // used for Silhouette, Detail, Picture
    public class HOImageUI : HOSubUI
    {
        HOItemHolder[]              itemHolders;

        public Image imageCopy;

        [SerializeField] LayoutGroup layoutGroup;

        [SerializeField]
        TextMeshProUGUI itemCounter;

        [SerializeField]
        TextMeshProUGUI itemDetailCounter;

        public override int GetListCapacity()
        {
            itemHolders = GetComponentsInChildren<HOItemHolder>(true);
            
            return itemHolders.Length;
        }

        public override void OnActiveHOChange()
        {
            foreach (var holder in itemHolders)
                holder.OnActiveHOChange();
        }

        private void Update()
        {
            // force relayout (animating when a list becomes empty)
            //layoutGroup.SetLayoutHorizontal();
            //LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
        }

        public override void Setup(List<HOFindableObject> findableObjects, int totalToFind)
        { 
            itemHolders = GetComponentsInChildren<HOItemHolder>(true);

            foreach (var holder in itemHolders)
                holder.gameObject.SetActive(false);

            if (findableObjects.Count > GetListCapacity())
            {
                Debug.LogError($"Have been given {findableObjects.Count} display labels with space for only {GetListCapacity()}");

                return;
            }

            for (int i = 0; i < itemHolders.Length; i++)
            {
                itemHolders[i].gameObject.SetActive(i < findableObjects.Count);

                if (i < findableObjects.Count)
                {
                    itemHolders[i].SetObject(findableObjects[i]);
                }
            }
        }
        
        public override void SetItemFoundTotal(int currentFound, int total, bool isFirst)
        {
            itemDetailCounter.text = itemCounter.text = $"{currentFound}/{total}";
        }

        public override HOItemHolder GetItemHolder(HOFindableObject forObject)
        {

            foreach (var holder in itemHolders)
            {
                if (holder.HoldsFindable(forObject))
                    return holder;
            }

            return null;
        }

        public override void UpdateItemList(HOFindableObject previousObject, IEnumerable<HOFindableObject> newObjects)
        {
            
        }

        public override bool IsValidUIForLogic(HOLogic logic)
        {
            return  (logic is HOLogicPicture) ||
                    (logic is HOLogicSilhouette) ||
                    (logic is HOLogicDetail) || (logic is HOLogicReverse);
        }

        IEnumerator ItemCollectAnimationCor(HOFindableObject obj, Image image, Image sdf, Vector2 startPos, UnityAction onDone)
        {
            HOItemHolderImage holder = GetItemHolder(obj) as HOItemHolderImage;
            
            Vector2 targetPos = new Vector2();
            Canvas canvas = UIController.instance.hoMainUI.canvas;

            
            Vector4 quad = holder.GetSpriteQuadWhenAspectCorrected(obj.GetComponent<SpriteRenderer>().sprite);
            Vector3 center = new Vector3((quad.z + quad.x) * 0.5f, (quad.w + quad.y) * 0.5f, 0f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
                holder.GetComponent<RectTransform>().position + center, null, out targetPos);

            Vector2 targetSize  = new Vector2(quad.z - quad.x, quad.w - quad.y);
            Vector2 startScale  = image.rectTransform.localScale;
            Vector3 targetScale = new Vector3(targetSize.x / image.rectTransform.sizeDelta.x,
                                              targetSize.y / image.rectTransform.sizeDelta.y,
                                              1f);
  
            yield return HOMainUI.DefaultCollectWobbleAnimation(image, sdf, startPos, targetPos, startScale, targetScale);

            onDone?.Invoke();
        }

        public override void AnimateObjectCollection(HOFindableObject obj, Image clonedImage, Image clonedSDF, Vector2 startPos, UnityAction onDone)
        {
            StartCoroutine(ItemCollectAnimationCor(obj, clonedImage, clonedSDF, startPos, onDone));
        }
    }
}
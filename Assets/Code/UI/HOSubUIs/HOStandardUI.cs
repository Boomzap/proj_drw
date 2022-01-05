using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;

namespace ho
{
    public class HOStandardUI : HOSubUI
    {
        HOItemHolderList[] itemHolderLists;

        [SerializeField]
        LayoutGroup layoutGroup;

        [SerializeField]
        TextMeshProUGUI itemCounter;
        public override int GetListCapacity()
        {
            itemHolderLists = GetComponentsInChildren<HOItemHolderList>(true);
            int c = 0;
            foreach (var list in itemHolderLists)
            {
                c += list.itemHolders.Length;
            }

            return c;
        }

        public override void OnActiveHOChange()
        {
            foreach (var list in itemHolderLists)
            {
                foreach (var holder in list.itemHolders)
                {
                    if (holder != null)
                        holder.OnActiveHOChange();
                }
            }
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
            itemHolderLists = GetComponentsInChildren<HOItemHolderList>(true);

            int totalHolderCount = 0;

            foreach (var list in itemHolderLists)
            {
                list.ClearHolders();
                list.gameObject.SetActive(false);
                totalHolderCount += list.itemHolders.Length;
            }


            int uniqueHolderCount = 0;
            List<string> countedNames = new List<string>();

            foreach (var obj in findableObjects)
            {
                if (!string.IsNullOrEmpty(obj.objectGroup))
                {
                    if (!countedNames.Contains(obj.objectGroup))
                    {
                        countedNames.Add(obj.objectGroup);
                        uniqueHolderCount++;
                    }
                } else
                {
                    uniqueHolderCount++;
                }
            }

            if (uniqueHolderCount > totalHolderCount)
            {
                Debug.LogError($"Have been given {uniqueHolderCount} display labels with space for only {totalHolderCount}");

                return;
            }

            int currentHolderIdx = 0;
            int currentListIdx = 0;

            countedNames.Clear();

            itemHolderLists[0].gameObject.SetActive(true);

            foreach (var obj in findableObjects)
            {
                if (!string.IsNullOrEmpty(obj.objectGroup))
                {
                    if (!countedNames.Contains(obj.objectGroup))
                    {
                        countedNames.Add(obj.objectGroup);

                        itemHolderLists[currentListIdx].itemHolders[currentHolderIdx].SetObjects(
                            findableObjects.Where(x => x.objectGroup == obj.objectGroup)
                        );
                    } else
                    {
                        continue;
                    }
                } else
                {
                    itemHolderLists[currentListIdx].itemHolders[currentHolderIdx].SetObject(obj);
                }

                if (obj == findableObjects.Last())
                    break;

                currentHolderIdx++;
                if (currentHolderIdx >= itemHolderLists[currentListIdx].itemHolders.Length)
                {
                    currentListIdx++;

                    if (currentListIdx >= itemHolderLists.Length)
                        break;

                    itemHolderLists[currentListIdx].gameObject.SetActive(true);
                    currentHolderIdx = 0;
                }
            }


            foreach (var list in itemHolderLists)
            {
                list.shouldAutoClose = true;
            }
        }

        public override void SetItemFoundTotal(int currentFound, int total, bool isFirst)
        {
            itemCounter.text = $"{currentFound}/{total}";
        }

        public override HOItemHolder GetItemHolder(HOFindableObject forObject)
        {
            foreach (var list in itemHolderLists)
            {
                foreach (var holder in list.itemHolders)
                {
                    if (holder.HoldsFindable(forObject))
                        return holder;
                }
            }

            return null;
        }

        public override void UpdateItemList(HOFindableObject previousObject, IEnumerable<HOFindableObject> newObjects)
        {

        }

        public override bool IsValidUIForLogic(HOLogic logic)
        {
            return logic is HOLogicStandard || logic is HOLogicScramble || logic is HOLogicNoVowel;
        }

        IEnumerator ItemCollectAnimationCor(HOFindableObject obj, Image image, Image sdf, Vector2 startPos, UnityAction onDone)
        {
            HOItemHolder holder = GetItemHolder(obj);

            Vector2 targetPos = new Vector2();
            Canvas canvas = UIController.instance.hoMainUI.canvas;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
                holder.GetComponent<RectTransform>().position, null, out targetPos);

            Vector2 startScale = image.rectTransform.localScale;
            Vector3 targetScale = Vector2.zero;


            yield return HOMainUI.DefaultCollectWobbleAnimation(image, sdf, startPos, targetPos, startScale, targetScale);

            if (HOGameController.instance.gameLogic is HOLogicNoVowel || HOGameController.instance.gameLogic is HOLogicScramble)
            {
                holder.ResetToOriginalText();
                yield return new WaitForSeconds(1.5f);
            }

            onDone?.Invoke();
        }

        public override void AnimateObjectCollection(HOFindableObject obj, Image clonedImage, Image clonedSDF, Vector2 startPos, UnityAction onDone)
        {
            StartCoroutine(ItemCollectAnimationCor(obj, clonedImage, clonedSDF, startPos, onDone));
        }
    }
}
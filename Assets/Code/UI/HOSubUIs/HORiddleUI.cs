using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;

namespace ho
{
    public class HORiddleUI : HOSubUI
    {
        [SerializeField] HOItemHolder            riddleItemHolder;
        [SerializeField] TextMeshProUGUI         itemCounter;

        public override int GetListCapacity()
        {
            return 1;
        }

        public override void OnActiveHOChange()
        {
            
        }

        private void Update()
        {

        }

        public override void Setup(List<HOFindableObject> findableObjects, int totalToFind)
        {
            riddleItemHolder = GetComponentInChildren<HOItemHolder>(true);

            riddleItemHolder.Clear();
            riddleItemHolder.SetObjects(findableObjects);
        }

        IEnumerator PumpCor()
        {
            float time = 0f;
            float pumpTime = 0.3f;

            while (time < pumpTime)
            {
                float a = time / pumpTime;
                a *= 2f;
                if (a > 1f)
                    a = 1f - (a-1f);

                itemCounter.transform.parent.localScale = Vector2.one * (1f + a * 0.1f);

                time += Time.deltaTime;
                yield return null;
            }
        }
        
        public override void SetItemFoundTotal(int currentFound, int total, bool isFirst)
        {
            itemCounter.text = $"{currentFound}/{total}";

            if (!isFirst)
            {
                StartCoroutine(PumpCor());
            }
        }

        public override HOItemHolder GetItemHolder(HOFindableObject forObject)
        {
            return riddleItemHolder.HoldsFindable(forObject) ? riddleItemHolder : null;
        }

        public override void UpdateItemList(HOFindableObject previousObject, IEnumerable<HOFindableObject> newObjects)
        {
            
        }

        public override bool IsValidUIForLogic(HOLogic logic)
        {
            return logic is HOLogicRiddle;
        }

        IEnumerator ItemCollectAnimationCor(HOFindableObject obj, Image image, Image sdf, Vector2 startPos, UnityAction onDone)
        {
            Vector2 targetPos = new Vector2();
            Canvas canvas = UIController.instance.hoMainUI.canvas;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
                itemCounter.GetComponent<RectTransform>().position, null, out targetPos);

            Vector2 startScale  = image.rectTransform.localScale;
            Vector3 targetScale = Vector2.zero;

            yield return HOMainUI.DefaultCollectWobbleAnimation(image, sdf, startPos, targetPos, startScale, targetScale);

            onDone?.Invoke();
        }

        public override void AnimateObjectCollection(HOFindableObject obj, Image clonedImage, Image clonedSDF, Vector2 startPos, UnityAction onDone)
        {
            StartCoroutine(ItemCollectAnimationCor(obj, clonedImage, clonedSDF, startPos, onDone));
        }
    }
}
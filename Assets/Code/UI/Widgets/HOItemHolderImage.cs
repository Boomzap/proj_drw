using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ho
{
    public class HOItemHolderImage : HOItemHolder 
    {
        HOImageUI imageUI;

        public Image                itemImage;

        float                       inSubHOAlpha => UIController.instance.hoMainUI.inSubHOAlpha;

        [SerializeField]            Material silhouetteMaterial;
        [SerializeField]            Material pictureMaterial;
        [SerializeField]            Material detailMaterial;

        public EventTrigger eventTrigger;

        bool isDragging = false;

        HOLogicReverse reverseLogic => HOGameController.instance.gameLogic as HOLogicReverse;

        bool itemImageEmpty => itemImage == null;
        bool isReverseLogic => HOGameController.instance.gameLogic is HOLogicReverse;


        private void Awake()
        {
            imageUI = GetComponentInParent<HOImageUI>();
        }

        public Vector4 GetSpriteQuadWhenAspectCorrected(Sprite sprite)
        {
            var padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
            var size = new Vector2(sprite.rect.width, sprite.rect.height);

            Rect r = itemImage.GetPixelAdjustedRect();

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                    padding.x / spriteW,
                    padding.y / spriteH,
                    (spriteW - padding.z) / spriteW,
                    (spriteH - padding.w) / spriteH);

            if (size.sqrMagnitude > 0.0f)
            {
                var spriteRatio = size.x / size.y;
                var rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio)
                {
                    var oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * itemImage.rectTransform.pivot.y;
                }
                else
                {
                    var oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * itemImage.rectTransform.pivot.x;
                }
            }

            v = new Vector4(
                    r.x + r.width * v.x,
                    r.y + r.height * v.y,
                    r.x + r.width * v.z,
                    r.y + r.height * v.w
                    );

            return v;            
        }
        public void OnPointerEnterDelegate(PointerEventData data)
        {
            //Debug.Log("OnPointerDownDelegate called.");
            iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
        }

        public void OnPointerExitDelegate(PointerEventData data)
        {
            iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
        }

        public void OnPointerDownDelegate(PointerEventData data)
        {
            isDragging = true;
            imageUI.imageCopy.gameObject.SetActive(true);
            imageUI.imageCopy.sprite = itemImage.sprite;
            imageUI.imageCopy.preserveAspect = true;
            imageUI.imageCopy.transform.position = data.position;
            itemImage.enabled = false;

            reverseLogic.selectedItem = itemImage.sprite;
        }

        public void OnPointerUpDelegate(PointerEventData data)
        {
            isDragging = false;
            imageUI.imageCopy.gameObject.SetActive(false);
            itemImage.enabled = true;
        }


        void EnableMouseEvents()
        {
            AddEventTrigger((data) => { OnPointerEnterDelegate((PointerEventData)data); }, EventTriggerType.PointerEnter);
            AddEventTrigger((data) => { OnPointerExitDelegate((PointerEventData)data); }, EventTriggerType.PointerExit);
            AddEventTrigger((data) => { OnPointerDownDelegate((PointerEventData)data); }, EventTriggerType.PointerDown);
            AddEventTrigger((data) => { OnPointerUpDelegate((PointerEventData)data); }, EventTriggerType.PointerUp);
        }

        void AddEventTrigger(UnityAction<BaseEventData> eventEntry, EventTriggerType triggerType)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = triggerType;
            entry.callback.AddListener(eventEntry);
            eventTrigger.triggers.Add(entry);
        }

        private void OnEnable()
        {
            Clear();

            if (HOGameController.instance.gameLogic is HOLogicSilhouette)
                itemImage.material = silhouetteMaterial;
            else if (HOGameController.instance.gameLogic is HOLogicPicture)
                itemImage.material = pictureMaterial;
            else if (HOGameController.instance.gameLogic is HOLogicDetail)
                itemImage.material = detailMaterial;
            else if (HOGameController.instance.gameLogic is HOLogicReverse)
                itemImage.material = pictureMaterial;

            eventTrigger.triggers.Clear();

            if (isReverseLogic)
                EnableMouseEvents();
        }

        private void OnDisable()
        {
            Clear();
        }

        public override void Clear()
        {
            base.Clear();

            itemImage.sprite = null;
            itemImage.color = new Color(1f, 1f, 1f, 1f);
        }


        IEnumerator UpdateColorCor(Color targetColor, float time)
        {
            float maxTime = time;
            float timer = 0f;
            Color startColor = itemImage.color;

            while (timer < maxTime)
            {
                timer += Time.deltaTime;

                float alpha = timer / maxTime;

                itemImage.color = Color.Lerp(startColor, targetColor, alpha);

                yield return new WaitForEndOfFrame();
            }

            itemImage.color = targetColor;
        }

        public override void OnActiveHOChange(bool animate = true)
        {
            if (!isEmpty)
            {
                Color targetColor;
                //StopAllCoroutines();

                targetColor = HOGameController.instance.ActiveRoomContains(findables[0]) ? 
                    new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, inSubHOAlpha);

                if (animate)
                {
                    StopCoroutine(UpdateColorCor(targetColor, 0.3f));
                    StartCoroutine(UpdateColorCor(targetColor, 0.3f));
                } 
                else
                {
                    itemImage.color = targetColor;
                }
            }
        }

        IEnumerator SwapItemCor(Sprite newSprite)
        {
            float time = 0f;
            float swapTime = 0.25f;

            while (time < swapTime)
            {
                float a = time/swapTime;

                transform.localRotation = Quaternion.Euler(0f, a * 270f, 0f);

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (newSprite == null)
            {
                time = 0f;
                while (time < swapTime)
                {
                    float a = time / swapTime;

                    transform.localRotation = Quaternion.Euler(0f, 270f + a * 90f, 0f);
                    itemImage.color = new Color(1f, 1f, 1f, 1f-a);

                    time += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                //Make sure alpha is zero
                itemImage.color = new Color(1f, 1f, 1f, 0f);
            } else
            {
                itemImage.sprite = newSprite;
                itemImage.preserveAspect = true;

                time = 0f;
                while (time < swapTime)
                {
                    float a = time / swapTime;

                    transform.localRotation = Quaternion.Euler(0f, 270f + a * 90f, 0f);

                    time += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }


        }

        public override void SetObjects(IEnumerable<HOFindableObject> findableObjects, bool animate = false)
        {
            haveBeenSet = true;
            findables = new List<HOFindableObject>(findableObjects);

            if (findableObjects.Count() > 1)
            {
                Debug.LogError("Have more than one object being set in a picture field");

                return;
            }

            foreach (var f in findables)
            {
                if (!prevFindables.Contains(f))
                    prevFindables.Add(f);
            }

            if (findables.Count == 1)
            {
                if (findables[0])
                {
                    if (animate)
                    {
                        StartCoroutine(SwapItemCor(findableObjects.ElementAt(0).GetComponent<SpriteRenderer>().sprite));
                    } else
                    {
                        itemImage.sprite = findableObjects.ElementAt(0).GetComponent<SpriteRenderer>().sprite;
                        itemImage.preserveAspect = true;
                    }
                } else
                {
                    if (isReverseLogic)
                    {
                        //Remove triggers when item holder is empty.
                        eventTrigger.triggers.Clear();
                    }


                    if (animate)
                    //iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeInBack"));
                    {

                        StartCoroutine(SwapItemCor(null));
                    }
                    else
                    {
                        itemImage.sprite = null;
                    }
                }

            }

            OnActiveHOChange(false);
        }

        public override void SetObject(HOFindableObject findableObject, bool animate = false)
        {
            haveBeenSet = true;
            findables = new List<HOFindableObject>();
            findables.Add(findableObject);

            if (!prevFindables.Contains(findableObject))
                prevFindables.Add(findableObject);

            if (findableObject)
            {
                if (animate)
                {
                    StartCoroutine(SwapItemCor(findableObject.GetComponent<SpriteRenderer>().sprite));
                } else
                {
                    itemImage.sprite = findableObject.GetComponent<SpriteRenderer>().sprite;
                    itemImage.preserveAspect = true;
                }
            } else
            {
                if (animate)
                //iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeInBack"));
                {
                    StartCoroutine(SwapItemCor(null));
                } else
                {
                    itemImage.sprite = null;
                }
            }

            OnActiveHOChange(false);
        }

         void Update()
        {
            if (isDragging)
                imageUI.imageCopy.transform.position = Input.mousePosition;
        }
    }
}
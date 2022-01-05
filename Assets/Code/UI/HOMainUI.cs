using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Linq;

namespace ho
{
    public class HOMainUI : BaseUI
    {
        [Header("UI Components")]
        public HOHintButton   hintButton;
        //public SuperHintButton superHintButton;

        [SerializeField] Button[]       keyItemButtons;
        HOKeyItemHolder[] keyItemHolders;


        [SerializeField] RectTransform bottomPanel;
        
        [Header("Config for visuals")]
        public Color    defaultItemTextColor;
        public Color    keyItemTextColor;
        public float    inSubHOAlpha = 0.5f;
        public Material brightenMaterial;
        public TextMeshProUGUI debugLabel;

        [Header("Sub-UIs used for different HO modes")]
        public HOSubUI[]    subUIs;
        [SerializeField, ReadOnly]  HOSubUI currentSubUI = null;

        [SerializeField] ParticleSystem hintParticleSystem;
        
        [SerializeField] Spline2DComponent splineHolder;
        [SerializeField] Material hintMaterial;

        public bool         haveItemOnMouse { get => currentKeyItemOnMouse != null; }
        public HOKeyItem    currentKeyItemOnMouse;
        Image               currentKeyItemImageOnMouse = null;
        Vector3             keyItemImageOrgPos;
        Vector3             keyItemImageOrgScale;
        Transform           keyItemImageOrgParent;

        bool                hintPathPlaying = false;
        public bool         AnimatingHintPath { get => hintPathPlaying; }

        public Canvas       canvas { get{ return GetComponent<Canvas>(); } }

        public HOSubUI      CurrentSubUI => currentSubUI;

        public RectTransform mainPanel;

        [SerializeField] Button mapButton;
        [SerializeField] Button menuButton;
        [SerializeField] Button settingsButton;

        [SerializeField] Button zoomButton;

        public HOScoreKeeper hoScoreKeeper;
        public TimeKeeper timeKeeper;

        public HOItemPrompt itemPrompt;

        public GameObject hoInfo;
        public GameObject hoDetailInfo;

        public List<HOKeyItem> heldKeyItems
        {
            get
            {
                return keyItemHolders.Where(x => x.keyItem != null).Select(x => x.keyItem).ToList();
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            splineHolder = GetComponentInChildren<Spline2DComponent>();
        }

        private void OnEnable()
        {
            currentKeyItemImageOnMouse = null;
            currentKeyItemOnMouse = null;

            keyItemHolders = new HOKeyItemHolder[keyItemButtons.Length];
            for (int i = 0; i < keyItemButtons.Length; i++)
            {
                keyItemHolders[i] = keyItemButtons[i].GetComponent<HOKeyItemHolder>();

                keyItemButtons[i].onClick.RemoveAllListeners();
                var cap = i; 
                keyItemButtons[i].onClick.AddListener(() => { OnSelectKeyItem(cap); });
            }
           

            zoomButton.onClick.RemoveAllListeners();
            zoomButton.onClick.AddListener(OnZoomButton);

            mapButton.onClick.RemoveAllListeners();
            mapButton.onClick.AddListener(OnChapterButton);

            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMainMenuButton);

            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Popup.ShowPopup<OptionsPopup>());
        }

        void OnZoomButton()
        {
           HOGameController.instance.magCamDisplay.ToggleOpen();
        }

        void OnMainMenuButton()
        {
            GenericPromptPopup prompt = Popup.ShowPopup<GenericPromptPopup>();

            string returnToMapHeader = string.Empty;
            string returnToMapPrompt = string.Empty;

            returnToMapHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToMain_header", "", false, TableCategory.UI);
            returnToMapPrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToMain_body", "", false, TableCategory.UI);

            prompt.Setup(returnToMapHeader, returnToMapPrompt, PrompType.Options);
            prompt.onHiddenOneshot += () =>
            {
                if (prompt.isConfirmed)
                {
                    GameController.instance.FadeToGameMenu();
                }
            };
        }

        void OnChapterButton()
        {
            GenericPromptPopup prompt = Popup.ShowPopup<GenericPromptPopup>();

            string returnToMapHeader = string.Empty;
            string returnToMapPrompt = string.Empty;

            if (GameController.instance.isUnlimitedMode)
            {
                returnToMapHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToUnlimited_header", "", false, TableCategory.UI);
                returnToMapPrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToUnlimited_body", "", false, TableCategory.UI);
            }
            else
            {
                returnToMapHeader = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToChapter_header", "", false, TableCategory.UI);
                returnToMapPrompt = LocalizationUtil.FindLocalizationEntry("UI/Prompt/ReturnToChapter_body", "", false, TableCategory.UI);
            }

            prompt.Setup(returnToMapHeader, returnToMapPrompt, PrompType.Options);
            prompt.onHiddenOneshot += () =>
            {
                if (prompt.isConfirmed)
                {
                    if(GameController.instance.isUnlimitedMode)
                    {
                        GameController.instance.FadeToUnlimitedMenu();
                    }
                    else
                        GameController.instance.FadeToChapterMenu(GameController.instance.storyModeOpened);
                }
            };
        }

        public void SetDebugLabel(string text, Vector2 position)
        {
            if (debugLabel == null) return;

            if (string.IsNullOrEmpty(text))
            {
                debugLabel.gameObject.SetActive(false);
                return;
            }

            debugLabel.gameObject.SetActive(true);
            debugLabel.text = text;
            debugLabel.transform.localPosition = position;
        }

        public void OnActiveHOChange()
        {
            if (currentSubUI)
            {
                currentSubUI.OnActiveHOChange();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (currentKeyItemImageOnMouse)
            {
                currentKeyItemImageOnMouse.rectTransform.position = Input.mousePosition;
            }
        }

        void ClearKeyItemInUI()
        {
            if (currentKeyItemOnMouse == null) return;

            var holder = GetKeyItemHolder(currentKeyItemOnMouse);

            currentKeyItemImageOnMouse.transform.localScale = keyItemImageOrgScale;
            currentKeyItemImageOnMouse.transform.position = keyItemImageOrgPos;

            if (holder)
            {
                holder.SetObject(null);
            }

            currentKeyItemOnMouse = null;
            currentKeyItemImageOnMouse = null;
        }

        public void ClearKeyItemOnMouse(bool showErrorMessage = false, bool clearItemFromUI = false)
        {
            if (currentKeyItemImageOnMouse == null)
                return;

            if (clearItemFromUI)
            {
                currentKeyItemImageOnMouse.gameObject.transform.SetParent(keyItemImageOrgParent, true);    
                //iTween.ColorTo(currentKeyItemImageOnMouse.gameObject, iTween.Hash("color",new Color(1f, 1f, 1f, 0f),"time",0.5,"oncompletetarget",gameObject,"oncomplete","ClearKeyItemInUI"));
                currentKeyItemImageOnMouse.CrossFadeAlpha(0f, 0.25f, true);

                this.ExecuteAfterDelay(0.25f, ClearKeyItemInUI);

            } else
            {
                currentKeyItemImageOnMouse.gameObject.transform.SetParent(keyItemImageOrgParent, true);
                iTween.ScaleTo(currentKeyItemImageOnMouse.gameObject, keyItemImageOrgScale, 0.25f);
                iTween.MoveTo(currentKeyItemImageOnMouse.gameObject, keyItemImageOrgPos, 0.25f);
                currentKeyItemOnMouse = null;
                currentKeyItemImageOnMouse = null;
            }

            itemPrompt.DisablePrompt();
            if (showErrorMessage)
            {
                // genericpopup  "that's not how i'm supposed to use this"
            } 
        }

        void OnSelectKeyItem(int idx)
        {
            if (keyItemHolders[idx].isEmpty) return;

            if (currentKeyItemOnMouse == keyItemHolders[idx].keyItem)
            {
                ClearKeyItemOnMouse();
            } else
            {
                ClearKeyItemOnMouse();
                // new item on mouse
                keyItemImageOrgPos = keyItemHolders[idx].itemImage.rectTransform.position;
                keyItemImageOrgScale = keyItemHolders[idx].itemImage.rectTransform.localScale;
                currentKeyItemImageOnMouse = keyItemHolders[idx].itemImage;
                currentKeyItemOnMouse = keyItemHolders[idx].keyItem;

                keyItemImageOrgParent = currentKeyItemImageOnMouse.gameObject.transform.parent;
                currentKeyItemImageOnMouse.gameObject.transform.SetParent(gameObject.transform, true);
                currentKeyItemImageOnMouse.gameObject.transform.SetAsLastSibling();

                //itemPrompt.ShowPrompt(LocalizationUtil.FindLocalizationEntry(currentKeyItemOnMouse.promptKey));
            }

        }

        public int GetListCapacity()
        {
            return currentSubUI.GetListCapacity();
        }

        public void GetHintWidgetPosSize(Canvas forCanvas, out Vector2 maskPos, out Vector2 maskSize)
        {
            RectTransform hintBtnTransform = hintButton.transform as RectTransform;
            
            Vector3[] corners = new Vector3[4];
            hintBtnTransform.GetWorldCorners(corners);

            maskPos = RectTransformUtility.WorldToScreenPoint(forCanvas.worldCamera, (corners[2] + corners[0]) * 0.5f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(forCanvas.transform as RectTransform, maskPos, null, out maskPos);

            float maxDim = Mathf.Max(hintBtnTransform.rect.size.x, hintBtnTransform.rect.size.y) * hintBtnTransform.lossyScale.x;
            maskSize = new Vector2(maxDim, maxDim);
            
        }

        [Sirenix.OdinInspector.Button]
        public Rect[] GetCoveredAreas()
        {
//             Rect[] coveredAreas = new Rect[2];
// 
//             Vector3[] bottomCorners = new Vector3[4];
//             Vector3[] topCorners = new Vector3[4];
// 
//             bool wasActive = gameObject.activeSelf;
// 
//             gameObject.SetActive(true);
//             bottomPanel.GetWorldCorners(bottomCorners);
//             topPanel.GetWorldCorners(topCorners);
//             gameObject.SetActive(wasActive);
// 
//             Vector3 center = (bottomCorners[2] + bottomCorners[0]) * 0.5f;
//             Vector3 size = bottomCorners[2] - bottomCorners[0];
// 
//             coveredAreas[0] = new Rect(center - new Vector3(size.x, size.y) * 0.5f, size);
// 
//             center = (topCorners[2] + topCorners[0]) * 0.5f;
//             size = topCorners[2] - topCorners[0];
// 
//             coveredAreas[1] = new Rect(center - new Vector3(size.x, size.y) * 0.5f, size);
// 
//             return coveredAreas;
            return null;
        }

        protected override void OnFinishShow()
        {
            //base.OnFinishShow();
        }

        public void OnHintReady()
        {
            hintButton.OnHintReady();
        }

        public void UpdateHint()
        {
            hintButton.ResetHintCooldown();
            //hintButton.CooldownProgress = HOGameController.instance.hintCooldownProgress;
            //superHintButton.ResetHintTimer();
        }

        protected override void OnBeginShow(bool instant)
        {
            //expect to be shown by an animation that starts under, but we want to show it later.
            //mainPanel.anchoredPosition = new Vector2(0f, -255f);

            base.OnBeginShow(instant);
            hoScoreKeeper.ResetScore();
            itemPrompt.gameObject.SetActive(false);
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);

            var charEntry = HOGameController.instance.ChapterEntry?.hintCharacter ?? null;


            if (charEntry == null)
                charEntry = Boomzap.Character.CharacterManager.instance.characters[0];

            hintButton.SetCharacter(charEntry);
            hintButton.DisableHint();
        }

        public HOKeyItemHolder GetKeyItemHolder(HOFindableObject forObject)
        {
            foreach (var holder in keyItemHolders)
            {
                if (forObject == null && holder.isEmpty)
                    return holder;

                if (forObject != null && holder.keyItem == forObject)
                    return holder;
            }

            return null;
        }

        public HOItemHolder GetItemHolder(HOFindableObject forObject)
        {
            return currentSubUI?.GetItemHolder(forObject) ?? null;
        }

        public void Initialize(HOLogic hoLogic)
        {
            bool isDetailMode = hoLogic is HOLogicDetail;

            hoInfo.SetActive(isDetailMode == false);
            hoDetailInfo.SetActive(isDetailMode);

            currentSubUI = null;

            foreach (var ui in subUIs)
            {
                bool validUI = ui.IsValidUIForLogic(hoLogic);

                ui.gameObject.SetActive(validUI);

                if (validUI)
                {
                    currentSubUI = ui;
                }
            }            

            if (currentSubUI == null)
            {
                Debug.LogError($"No valid UI found for logic : {hoLogic.GetType()}");
            }

            timeKeeper.ResetTime();
        }

        public void SetInitialItemList(List<HOFindableObject> findableObjects, int totalToFind)
        {
            if (currentSubUI)
            {
                currentSubUI.Setup(findableObjects, totalToFind);
            } else
            {
                Debug.LogError("No valid UI currently active");
            }
        }

        public void SetItemFoundTotal(int currentFound, int total, bool isFirst = false)
        {
            currentSubUI?.SetItemFoundTotal(currentFound, total, isFirst);
        }

        IEnumerator HintPathCor()
        {
            float maxTime = 1.5f;
            float timer = 0f;

            hintPathPlaying = true;

            hintParticleSystem.Play(true);

            while (timer <= maxTime)
            {
                float alpha = timer / maxTime;
                
                Vector3 pos = splineHolder.InterpolateWorldSpace(alpha);
                hintParticleSystem.transform.position = pos;

                timer += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            hintParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            hintPathPlaying = false;
        }

        public void ShowItemPrompt(string key)
        {
            itemPrompt.ShowPrompt(LocalizationUtil.FindLocalizationEntry(key));
        }
        public void SetKeyItemCount(int count)
        {
            for (int i = 0; i < keyItemHolders.Length; i++)
            {
                keyItemHolders[i].Clear();
            }
        }

        public void PlayHintAttract()
        {
            hintButton.transform.parent.gameObject.PlayAnimation(this, "hint_attract");
        }

        public void PlayHintPath(HOInteractiveObject toObject)
        {
//             splineHolder.Clear();
// 
//             RectTransform hintBtnTransform = hintButton.transform as RectTransform;
//             Vector3 startPos = HOUtil.UIWidgetCenterPosToWorldPos(hintBtnTransform);
//             Vector3 endPos = toObject.transform.position;
// 
//             float tX = Random.Range(0.2f, 0.8f) * GameController.instance.currentCamera.pixelWidth;
//             Vector3 tPos = startPos + new Vector3(0f, 800f, 0f);
//             float tY = Random.Range(0.2f, 0.8f) * GameController.instance.currentCamera.pixelHeight * 0.7f;
//             float ttX = Random.Range(0.0f, 1.0f) > 0.5f ? 0f : GameController.instance.currentCamera.pixelWidth;
//             Vector3 tPos2 = GameController.instance.currentCamera.ScreenToWorldPoint(new Vector3(ttX, tY, 1f));
// 
//             splineHolder.AddPointWorldSpace(startPos);
//             splineHolder.AddPointWorldSpace(tPos);
//             //splineHolder.AddPointWorldSpace(tPos2);
//             splineHolder.AddPointWorldSpace(endPos);
//             splineHolder.IsClosed = false;
// 
//             hintObject.StartCoroutine(HintPathCor());
        }

        IEnumerator KeyItemCollectAnimationCor(HOFindableObject obj, Image image, Image sdf, Vector2 startPos, UnityAction onDone)
        {
            HOKeyItemHolder keyHolder = GetKeyItemHolder(null);
            
            Vector2 targetPos = new Vector2();

            Vector4 quad = keyHolder.GetSpriteQuadWhenAspectCorrected(obj.GetComponent<SpriteRenderer>().sprite);
            Vector3 center = new Vector3((quad.z + quad.x) * 0.5f, (quad.w + quad.y) * 0.5f, 0f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
                keyHolder.itemImage.GetComponent<RectTransform>().position + center, null, out targetPos);

            keyHolder.keyItem = obj as HOKeyItem;            

            Vector2 targetSize  = new Vector2(quad.z - quad.x, quad.w - quad.y);
            Vector2 startScale  = image.rectTransform.localScale;
            Vector3 targetScale = new Vector3(targetSize.x / image.rectTransform.sizeDelta.x,
                                              targetSize.y / image.rectTransform.sizeDelta.y,
                                              1f);

            yield return DefaultCollectWobbleAnimation(image, sdf, startPos, targetPos, startScale, targetScale);

            onDone?.Invoke();
        }

        void AnimateKeyItemCollection(HOFindableObject obj, Image clonedImage, Image clonedSDF, Vector2 startPos, UnityAction onDone)
        {
            StartCoroutine(KeyItemCollectAnimationCor(obj, clonedImage, clonedSDF, startPos, onDone));
        }

        public void AnimateObjectCollection(HOFindableObject obj, Image clonedImage, Image clonedSDF, Vector2 startPos, UnityAction onDone)
        {
            if (obj is HOKeyItem)
            {
                AnimateKeyItemCollection(obj, clonedImage, clonedSDF, startPos, onDone);

            } else
            {
                currentSubUI.AnimateObjectCollection(obj, clonedImage, clonedSDF, startPos, onDone);
            }
        }

        static IEnumerator CollectGlowMaterialAnimationCor(Material mat)
        {
            float flareTime = 0.6f;
            float time = 0f;

            while (time < flareTime)
            {
                float a = time / flareTime;

                if (a > 0.8f)
                    mat.SetFloat("_GlowAlpha", 1f - (a - 0.8f) * 5f);
                else if (a < 0.2f)
                    mat.SetFloat("_GlowAlpha", a * 5f);

                mat.SetFloat("_GlowSizeA", Mathf.Lerp(0.05f, 0.4f, a));

                time += Time.deltaTime;
                yield return null;
            }

            // [0.2 -> a = {0.0, 1.0}
            // [0.2,0.8] -> a = 1.0
            // 0.8] -> a = {1.0, 0.0}
        }

        public static IEnumerator DefaultCollectWobbleAnimation(MonoBehaviour target, Image sdfImage, Vector2 sourcePos, Vector2 targetPos, Vector2 sourceScale, Vector2 targetScale)
        {
            // animT = 0.6
            target.StartCoroutine(CollectGlowMaterialAnimationCor(sdfImage.material));

            float wobbleTime = 0.1f;
            float time = 0f;

            while (time < wobbleTime)
            {
                time += Time.deltaTime;

                float a = time / wobbleTime;
                target.transform.localScale = sourceScale * (Vector2.one + new Vector2(0.5f, 0.75f) * a);

                yield return null;
            }


            // wobble between 1.5 and 1.75
            Vector2 dbg = new Vector2();
            time = 0f;
            wobbleTime = 0.2f;
            while (time < wobbleTime)
            {
                time += Time.deltaTime;

                float b = (time / wobbleTime) * 2f;
                float m = b % 1f;

                float x = (Mathf.FloorToInt(b) % 2 == 1 ? 1f-m : m);
                float y = 1f - x;

                dbg = (new Vector2(1.5f + x * 0.25f, 1.5f + y * 0.25f));
                target.transform.localScale = sourceScale * dbg;

                yield return null;
            }
            // animT = 0.4

            // back to 1.5
            time = 0f;
            wobbleTime = 0.1f;
            while (time < wobbleTime)
            {
                time += Time.deltaTime;

                float c = time / wobbleTime;
                target.transform.localScale = sourceScale * new Vector2(1.5f, 1.5f + 0.25f * (1f - c));

                yield return null;
            }
                    
            // animT = 0.3
            // and pause
            yield return new WaitForSeconds(0.2f);

            Audio.instance.PlaySound(HOGameController.instance.onItemFlyAudio);

            float symX = (sourcePos.x + targetPos.x) * 0.5f;
            float peakY = sourcePos.y + 100f;
            Spline2D spline = new Spline2D();

            spline.AddPoint(sourcePos);
            spline.AddPoint(new Vector2(symX, peakY));
            spline.AddPoint(targetPos);

            Vector2 peak = new Vector2(symX, peakY);

            float totalM = (peak - sourcePos).magnitude + (targetPos - peak).magnitude;
            float peakT = (peak - sourcePos).magnitude / totalM;
            float fallT = (targetPos - peak).magnitude / totalM;

            time = 0f;
            wobbleTime = 0.5f;
            while (time < wobbleTime)
            {
                time += Time.deltaTime;

                float d = time / wobbleTime;
                target.transform.localScale = Vector2.Lerp(sourceScale * 1.5f, targetScale, d);

// 
//                 if (d < peakT)
//                 {
//                     float q = d / peakT;
//                     q = q * q * q;
//                     target.transform.localPosition = spline.Interpolate(0, q);
// 
//                 } else
//                 {
//                     float q = (d - peakT) / fallT;
//                     q = q * q * q;
// 
//                     target.transform.localPosition = spline.Interpolate(1, q);
//                 }

                target.transform.localPosition = spline.Interpolate(d);
                

                yield return null;
            }
        }


    }
}
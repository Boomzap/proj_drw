using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using System.Text.RegularExpressions;

namespace ho
{
    public class HOItemHolder : MonoBehaviour
    {
        public TextMeshProUGUI      itemNameText;

        [ReadOnly]
        public List<HOFindableObject> findables = new List<HOFindableObject>();

        // we maintain this list to guard against race conditions where multiple items belong to the same holder
        // and are being clicked rapidly
        [ReadOnly]
        public List<HOFindableObject> prevFindables = new List<HOFindableObject>();

        public bool                 isEmpty{ get { return findables.Count == 0 || findables[0] == null; } }
        public bool                 hasBeenSet{ get { return haveBeenSet; } }

        protected bool              haveBeenSet = false;
        bool                        isAnimatingOut = false;

        float                       inSubHOAlpha => UIController.instance.hoMainUI.inSubHOAlpha;
        Color                       normalItemColor => UIController.instance.hoMainUI.defaultItemTextColor;
        Color                       keyItemColor => UIController.instance.hoMainUI.keyItemTextColor;

        bool                        isScrambleMode => HOGameController.instance.gameLogic is HOLogicScramble;

        bool                        isNoVowelMode => HOGameController.instance.gameLogic is HOLogicNoVowel;

        const string                vowels = "aeiou";

        string                      originalText = string.Empty;


        Color                       fadedNormalColor
        {
            get
            {
                var baseColor = normalItemColor;
                baseColor.a = inSubHOAlpha;

                return baseColor;
            }
        }

        Color                       fadedKeyColor
        {
            get
            {
                var baseColor = keyItemColor;
                baseColor.a = inSubHOAlpha;

                return baseColor;
            }
        }

        public virtual bool HoldsFindable(HOFindableObject findable)
        {
            return findables.Contains(findable) || prevFindables.Contains(findable);
        }

        // Start is called before the first frame update
        void Awake()
        {

        }

        private void OnEnable()
        {
            Clear();
        }

        public void Hide()
        {
            Clear();
        }

        public virtual void Clear()
        {
            //haveBeenSet = false;
            findables = new List<HOFindableObject>();
            ResetTransform();
            StopAllCoroutines();
            prevFindables.Clear();

            if (itemNameText)
                itemNameText.text = "";
        }

        IEnumerator AnimateResetToOriginal()
        {
            itemNameText.characterSpacing = 0f;
            iTween.ScaleTo(itemNameText.gameObject, iTween.Hash("scale", Vector3.one * 1.3f, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
            yield return new WaitForSeconds(0.26f);
            iTween.ScaleTo(itemNameText.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
        }

        public void ResetToOriginalText()
        {
            if(isScrambleMode || isNoVowelMode)
            {
                itemNameText.text = originalText;
                StartCoroutine(AnimateResetToOriginal());
            }
        }

        private void ResetTransform()
        {
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        IEnumerator UpdateColorCor(Color targetColor, float time)
        {
            float maxTime = time;
            float timer = 0f;
            Color startColor = itemNameText.color;

            while (timer < maxTime)
            {
                timer += Time.deltaTime;

                float alpha = timer / maxTime;

                itemNameText.color = Color.Lerp(startColor, targetColor, alpha);

                yield return new WaitForEndOfFrame();
            }

            itemNameText.color = targetColor;
        }

        public virtual void OnActiveHOChange(bool animate = true)
        {
            if (!isEmpty)
            {
                Color targetColor;

                if (findables[0] is HOKeyItem)
                {
                    targetColor = HOGameController.instance.ActiveRoomContains(findables[0]) ?
                        keyItemColor :
                        fadedKeyColor;
                } else
                {
                    targetColor = HOGameController.instance.ActiveRoomContains(findables[0]) ?
                        normalItemColor :
                        fadedNormalColor;                    
                }

                if (animate)
                {
                    StopCoroutine(UpdateColorCor(targetColor, 0.3f));
                    StartCoroutine(UpdateColorCor(targetColor, 0.3f));
                } else
                {
                    itemNameText.color = targetColor;
                }
            }
        }

        public virtual void SetObjects(IEnumerable<HOFindableObject> findableObjects, bool animate = false)
        {
            haveBeenSet = true;

            findables = new List<HOFindableObject>(findableObjects);

            foreach (var f in findables)
            {
                if (!prevFindables.Contains(f))
                    prevFindables.Add(f);
            }

            if (findables.Count > 1)
            {
                var roomRoot = findables[0].GetComponentInParent<HORoom>();
                string s = HOUtil.GetRoomObjectPluralization(roomRoot.name, findables[0].objectBaseName, findables.Count);
                UpdateText(s, animate);

            } else if (findables.Count > 0)
            {
                if (findables[0] == null)
                    UpdateText("", animate);
                else
                    UpdateText(findables[0].GetDisplayText(), animate);
            } else
            {
                UpdateText("", animate);
            }

            OnActiveHOChange(false);
        }


        public virtual void SetObject(HOFindableObject findableObject, bool animate = false)
        {
            haveBeenSet = true;

            findables = new List<HOFindableObject>();
            findables.Add(findableObject);

            if (!prevFindables.Contains(findableObject))
                prevFindables.Add(findableObject);

            if (findableObject)
            {
                UpdateText(findableObject.GetDisplayText(), animate);
            } else
            {
                UpdateText("", animate);
            }

            OnActiveHOChange(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isAnimatingOut && haveBeenSet && isEmpty)
            {
                iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 1f), "time", 0.3f, "easetype", "easeInBack"));
                isAnimatingOut = true;
            }        
        }

        string RemoveVowels(string txt)
        {
            string newText = originalText = txt;
            newText = Regex.Replace(txt, "[aeiou]", "_", RegexOptions.IgnoreCase);
            return newText;
        }

        string ScrambleText(string txt)
        {
            const float scramblePercent = 0.75f;

            originalText = txt.TrimEnd();

            string result = "";

            string shuffledText = "";

            foreach (var split in txt.Split(' '))
            {
                char[] chars = split.ToCharArray();
                List<char> remaining = new List<char>(chars);

                int nonFlipCount = Mathf.CeilToInt((1f - scramblePercent) * chars.Length);

                if (split.Length == 1)
                {
                    result += split + " ";
                    continue;
                }


                //Reorder name
                int sameCount = 0;
                do
                {
                    sameCount = 0;
                    remaining = remaining.OrderBy(x => Random.value).ToList();

                    for (int i = 0; i < remaining.Count; i++)
                    {  
                        if (remaining[i] == chars[i])
                            sameCount++;
                    } 
                } while (sameCount < nonFlipCount);

                shuffledText += result;

                for (int i = 0; i < chars.Length; i++)
                {
                    bool didSwap = (remaining[i] != split[i]);

                    if (didSwap)
                        result += "<color=#" + ColorUtility.ToHtmlStringRGB(keyItemColor) + ">";

                    shuffledText += remaining[i];
                    result += remaining[i];
                    if (didSwap)
                        result += "</color>";
                }

                result += " ";
            }

            result.TrimEnd(' ');

            //Recursive loop keep scrambling the text if it ended up the same
            if (originalText.Length > 1 && shuffledText.Equals(originalText))
            {
                return ScrambleText(shuffledText);
            }

            return result;
        }

        protected virtual void UpdateText(string newText, bool animate)
        {
            itemNameText.characterSpacing = isScrambleMode || isNoVowelMode ? 15f : 0f;

            if (isScrambleMode)
            {
                newText = ScrambleText(newText);
            }

            if (isNoVowelMode)
            {
                newText = RemoveVowels(newText);
            }

            if (animate)
            {
                StopCoroutine(SwapItemCor(newText));
                StartCoroutine(SwapItemCor(newText));
            }
            else
            {
                itemNameText.text = newText;
            }
        }


        IEnumerator SwapItemCor(string newText)
        {
            float time = 0f;
            float swapTime = 0.25f;

            while (time < swapTime)
            {
                float a = time/swapTime;

                transform.localRotation = Quaternion.Euler(a * 270f, 0f, 0f);

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            itemNameText.text = newText;

            time = 0f;
            while (time < swapTime)
            {
                float a = time/swapTime;

                transform.localRotation = Quaternion.Euler(270f + a * 90f, 0f, 0f);

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

    }
}
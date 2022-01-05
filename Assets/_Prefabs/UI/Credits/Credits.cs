using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

namespace ho
{
    public class Credits : MonoBehaviour
    {

        [SerializeField]
        GameObject      largeHeaderPrefab;
        [SerializeField]
        GameObject      mediumHeaderPrefab;
        [SerializeField]
        GameObject      normalLinePrefab;
        [SerializeField]
        GameObject      endPointPrefab;

        //[SerializeField]
        //float           lineSpaceSize = 50f;

        [SerializeField]
        float           delayBeforeScroll = 2f;

        [SerializeField]
        TextAsset       creditsScript;

        [SerializeField, Tooltip("How many seconds it takes to reach the end of the list")]
        float           scrollSpeed = 10f;

        float           delay;

        public bool isFromOptions;

        [ReadOnly]
        public GameObject    endPoint;

        RectTransform rectTransform;

        Vector2 rectPosition = Vector2.zero;
        float screenMidPoint;

        [SerializeField] float delayHideAfterEnd = 5f;
        float endDelay;

        [Serializable]
        class CreditsList
        {
            public string[] credits = new string[0];
        }

        void Awake()
        {
            rectTransform = transform as RectTransform;
        }

#if UNITY_EDITOR
        [Button]
        void CreateCreditsContent()
        {
            var childObjects = transform.GetComponentsInChildren<Transform>();

            foreach (Transform childObj in childObjects)
            {
                if (childObj == transform) continue;

                DestroyImmediate(childObj.gameObject);
            }
                
            CreditsList credits = JsonUtility.FromJson<CreditsList>(@"{""credits"":" + creditsScript.text + @"}");

            for(int i = 0; i < credits.credits.Length; i++)
            {
                ProcessLine(credits.credits[i]);
            }
        }

        void ProcessLine(string line)
        {
            GameObject prefab = normalLinePrefab;

            if (line.StartsWith("*"))
                prefab = mediumHeaderPrefab;
            else if (line.StartsWith("#"))
                prefab = largeHeaderPrefab;
            else if(line.StartsWith("endpoint"))
            {
                prefab = endPointPrefab;
                endPoint = Instantiate(prefab, transform);
                return; 
            }
                

            line = line.TrimStart(new char[] { '*', '#', });

            GameObject newLine = Instantiate(prefab, transform);

            if (line.StartsWith("UI"))
            {
                newLine.name = line;
                string entry = LocalizationUtil.FindLocalizationEntry(line, string.Empty, false, TableCategory.UI);
                TextMeshProUGUI newText = newLine.GetComponent<TextMeshProUGUI>();
                newText.text = entry;
            }
            else
            {
                TextMeshProUGUI newText = newLine.GetComponent<TextMeshProUGUI>();
                newText.text = line;
            }

           //Debug.Log(line);
        }



#endif
        [Button]
        void Restart()
        {
            delay = delayBeforeScroll;

            rectPosition.x = rectTransform.position.x;
            rectPosition.y = 0;
            rectTransform.position = rectPosition;

            screenMidPoint = Screen.height / 2;

            endDelay = delayHideAfterEnd;
        }
        private void OnEnable()
        {
            Restart();
        }

        private void Update()
        {
            //Delay before show
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
                return;
            }

            //Debug.Log("End Point Pos Y: " + endPoint.transform.position.y);
            //Debug.Log("Screen Midpoint: " + screenMidPoint);
            if (endPoint.transform.position.y > screenMidPoint)
            {
                endDelay -= Time.deltaTime;
                if(endDelay <= 0)
                {
                    endDelay = 9999f;
                    HideCredits();
                }
                return;
            }

            rectPosition.y +=  Time.deltaTime * (scrollSpeed * Screen.height / 768);
            rectTransform.position = rectPosition;
        }


        public void HideCredits()
        {
            if (GameController.instance.GetWorldState<MainMenuWorld>() && isFromOptions)
            {
                UIController.instance.creditsUI.onHiddenOneshot += () =>
                {
                    UIController.instance.mainMenuUI.Show();
                    Popup.ShowPopup<OptionsPopup>();
                };
            }
            else
            {
                UIController.instance.creditsUI.onHiddenOneshot += () => UIController.instance.mainMenuUI.Show();
            }
                


            UIController.instance.creditsUI.Hide();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace ho
{
    public class HOComboMeter : MonoBehaviour
    {
        [SerializeField] Image fillImage;

        [BoxGroup("Fill Settings"), SerializeField] float maxFillTime = 20f;
        [BoxGroup("Fill Settings"), SerializeField] float extraSecondsPerClick = 4f;
        float currentTime = 0f;

        [BoxGroup("Combo Text")] public ComboData[] comboDatas;
        [BoxGroup("Combo Text"), SerializeField] Color32 normalColor;
        [BoxGroup("Combo Text"), SerializeField] Color32 comboColor;

        [System.Serializable]
        public class ComboData
        {
            [BoxGroup("Combo Settings")] public TextMeshProUGUI comboText;
            [BoxGroup("Combo Settings")] public float comboTrigger;

        }

        [BoxGroup("Debug"), ShowInInspector, ReadOnly] int currentComboIndex = 0;
        int prevComboIndex = 0;

        void AnimateTextScaleUp(int index)
        {
            iTween.ScaleTo(comboDatas[index].comboText.gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuart));
            comboDatas[index].comboText.color = comboColor;
        }

        void AnimateTextScaleDown(int index)
        {
            iTween.ScaleTo(comboDatas[index].comboText.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuart));
            comboDatas[index].comboText.color = normalColor;
        }

        [BoxGroup("Combo Text"), Button]
        public void AddCombo()
        {
            float overLimit = maxFillTime;
            float currentTimeTemp = currentTime + extraSecondsPerClick;

            currentTime = currentTimeTemp > overLimit ? overLimit : currentTimeTemp;
        }

        public void ResetComboFill()
        {
            currentTime = 0;
        }

        void HandleComboFill()
        {
            float fillAmount = currentTime / maxFillTime;
            fillImage.fillAmount = fillAmount;

            currentComboIndex = -1;

            for(int i = 0; i < comboDatas.Length; i++)
            {
                if(fillAmount > comboDatas[i].comboTrigger)
                {
                    currentComboIndex = i;
                }
            }

            if(currentComboIndex != prevComboIndex)
            {
                if (prevComboIndex >= 0)
                    AnimateTextScaleDown(prevComboIndex);

                prevComboIndex = currentComboIndex;

                if(prevComboIndex >= 0)
                    AnimateTextScaleUp(prevComboIndex);
            }
        }

        private void Update()
        {
            if (HOGameController.instance.DisableInput) return;

            //Dont run combo meter when timer is 0
            if (currentTime < 0)
            {
                currentTime = 0;
                return;
            }
               
            currentTime -= Time.deltaTime;

            HandleComboFill();

        }

        public float GetScoreMultiplier()
        {
            return currentComboIndex + 2; 
        }

#if UNITY_EDITOR
        [Button] void FillComboMeter()
        {
            currentComboIndex = comboDatas.Length - 1;
            currentTime = maxFillTime;
            fillImage.fillAmount = 1f;
        }
#endif
    }
}

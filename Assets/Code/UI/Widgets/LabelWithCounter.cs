using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace ho
{
    public class LabelWithCounter : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] TextMeshProUGUI value;

        public int currentValue { get; private set; }= 0;
        
        int startValue = 0;
        int targetValue = 0;
        float updateOverTime = 1f; 
        float updateStart = 0f;

        public void SetValue(int v)
        {
            value.text = v.ToString();
            currentValue = v;
            targetValue = v;
        }

        public void UpdateValue(int toV, float overTime)
        {
            startValue = currentValue;
            targetValue = toV;
            updateOverTime = overTime;
            updateStart = Time.time;
        }

        private void Update()
        {
            if (currentValue != targetValue)
            {
                float a = (Time.time - updateStart) / updateOverTime;
                a = Mathf.Clamp(a, 0f, 1f);

                if (a >= 1f)
                {
                    value.text = targetValue.ToString();
                    currentValue = targetValue;
                }
                else
                {
                    currentValue = (startValue + (int)(a * (float)(targetValue - startValue)));
                    value.text = currentValue.ToString();
                }
            }

        }
    }
}
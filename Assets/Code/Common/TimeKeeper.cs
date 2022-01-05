using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
namespace ho
{
    public class TimeKeeper : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI timerText;

        [ReadOnly] public float currentTimeInSeconds;

        public void ResetTime()
        {
            currentTimeInSeconds = 0;
        }

        public int GetCurrentTimePlayed()
        {
            return (int) currentTimeInSeconds;
        }

        void UpdateTime()
        {
            currentTimeInSeconds += Time.deltaTime;
            int timeInMinutes = (int)(currentTimeInSeconds / 60f);
            int timeInSeconds = (int)(currentTimeInSeconds % 60f);
            timerText.text = $"{timeInMinutes}:{timeInSeconds:D2}";
        }

        private void Update()
        {
            if (HOGameController.instance.DisableInput) return;

            UpdateTime();
        }
    }
}

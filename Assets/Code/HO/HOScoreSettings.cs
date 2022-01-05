using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOScoreSettings : MonoBehaviour
    {
        [BoxGroup("Item Score Setting")] public int scorePerItem = 1000;

        [BoxGroup("Time Score Setting")] public int maxSpeedBonus = 25000;
        [BoxGroup("Time Score Setting")] public int maxBonusTime = 60;
        [BoxGroup("Time Score Setting")] public int timeBonusEnd = 180;

        [BoxGroup("Hint Score Setting")] public int hintMaxBonus = 10000;
        [BoxGroup("Hint Score Setting")] public int hintScorePenaltyPerUse = 1000;
    }
}
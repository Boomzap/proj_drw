using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace ho
{
    [CreateAssetMenu(fileName = "gameplayConfig", menuName = "HO/Gameplay Config", order = 1)]
    public class HOGameplayConfig : ScriptableObject
    {
        [BoxGroup("Easy Difficulty"), PropertyRange(0f, 1f), LabelText("Medium items %")]
        public float   easyDiffNormalItems = 0.25f;
        [BoxGroup("Easy Difficulty"), PropertyRange(0f, 1f), LabelText("Hard items %")]
        public float   easyDiffHardItems = 0f;
        [BoxGroup("Easy Difficulty"), ShowInInspector, LabelText("Easy items %"), PropertyOrder(-1)]
        public float   easyDiffEasyItems { get { return 1f - easyDiffNormalItems - easyDiffHardItems; } }

        [BoxGroup("Medium Difficulty"), PropertyRange(0f, 1f), LabelText("Medium items %")]
        public float   mediumDiffNormalItems = 0.5f;
        [BoxGroup("Medium Difficulty"), PropertyRange(0f, 1f), LabelText("Hard items %")]
        public float   mediumDiffHardItems = 0.25f;
        [BoxGroup("Medium Difficulty"), ShowInInspector, LabelText("Easy items %"), PropertyOrder(-1)]
        public float   mediumDiffEasyItems { get { return 1f - mediumDiffNormalItems - mediumDiffHardItems; } }

        [BoxGroup("Hard Difficulty"), PropertyRange(0f, 1f), LabelText("Medium items %")]
        public float   hardDiffNormalItems = 0.25f;
        [BoxGroup("Hard Difficulty"), PropertyRange(0f, 1f), LabelText("Hard items %")]
        public float   hardDiffHardItems = 0.5f;
        [BoxGroup("Hard Difficulty"), ShowInInspector, LabelText("Easy items %"), PropertyOrder(-1)]
        public float   hardDiffEasyItems { get { return 1f - hardDiffNormalItems - hardDiffHardItems; } }

        [Header("General")]
        [InfoBox("Multiplied by amount of active items in the scene")]
        public float   inactiveItemsAmount = 2f;
    }
}

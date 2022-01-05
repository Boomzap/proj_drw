using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Reflection;
using System.Reflection.Emit;

namespace ho 
{
    //NOTE* Update Repair Types when updating Repair Tools
    public enum RepairType
    {
        None,
        Acid,
        Brush,
        BuffingCloth, 
        Cleaner,
        CleaningCloth,
        Fabric,
        Glue,
        Hammer,
        NeedleThread,
        PaintBrush,
        Polish,
        Putty,
    }

    [CreateAssetMenu(fileName = "RepairTools", menuName = "Minigame/RepairToolsObject", order = 1)]
    public class RepairToolList: ScriptableObject
    {
        public List<RepairTool> repairTools = new List<RepairTool>();

        [System.Serializable]
        public class RepairTool
        {
            public string toolName;
            [PreviewField(50, ObjectFieldAlignment.Right)]
            public Sprite toolSprite_default;
            public Sprite toolSprite_on;
        }
    }
}


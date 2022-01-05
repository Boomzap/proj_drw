using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using ho;

[CustomPropertyDrawer(typeof(HOFindableLogicValidity))]
public class HOFindableLogicValidityDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float labelHeight = base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;

        return labelHeight * HOFindableLogicValidity.logicTypes.Length + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        SerializedProperty vltList = property.FindPropertyRelative("validLogicTypes");

        List<string> vs = new List<string>();
        
        float yincr = base.GetPropertyHeight(property, label);

        for (int i = 0; i < vltList.arraySize; i++)
        {
            vs.Add(vltList.GetArrayElementAtIndex(i).stringValue);
        }

        for (int i = 0; i < HOFindableLogicValidity.logicTypes.Length; i++)
        {
            string logicType = HOFindableLogicValidity.logicTypes[i];
            string friendlyType = HOFindableLogicValidity.friendlyTypes[i];

            Rect pos = new Rect(position.x, position.y, position.width, yincr);

            EditorGUI.BeginChangeCheck();

            bool isPiggyBackOnStandard = logicType == "HOLogicScramble";
            
            EditorGUI.BeginDisabledGroup(isPiggyBackOnStandard);
            bool newValue = EditorGUI.ToggleLeft(pos, new GUIContent(friendlyType), isPiggyBackOnStandard ? vs.Contains("HOLogicStandard") : vs.Contains(logicType));
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck() && !isPiggyBackOnStandard)
            {
                if (!newValue)
                    vs.Remove(logicType);
                else
                    vs.Add(logicType);
            }
            position.y += yincr + EditorGUIUtility.standardVerticalSpacing;
        }

        vltList.ClearArray();
        for(int i = 0; i < vs.Count; i++)
        {
            vltList.InsertArrayElementAtIndex(0);
            vltList.GetArrayElementAtIndex(0).stringValue = vs[i];
        }

        

        EditorGUI.EndProperty();
    }
}

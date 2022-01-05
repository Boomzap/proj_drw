using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using System.Reflection;
using System.Linq;

using ho;



[DrawerPriority(0, 0, 0)]
public class ValidValueListDrawer : OdinAttributeDrawer<CheckListAttribute>
{
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
// 
//         float labelHeight = base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
// 
//         return labelHeight * HOFindableLogicValidity.logicTypes.Length + EditorGUIUtility.standardVerticalSpacing;
//     }

    internal static string GetLabel(object obj)
    {
        if (obj is HORoomReference)
        {
            return (obj as HORoomReference).roomName;
        }

        FieldInfo fld = obj.GetType().GetField("name");
        if (fld != null)
            return fld.GetValue(obj) as string;

        return obj.ToString();
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        if (!(Property.ChildResolver is ICollectionResolver))
        {
            CallNextDrawer(label);
            return;
        }

        ICollectionResolver resolver = Property.ChildResolver as ICollectionResolver;

        ValueResolver<object> r = ValueResolver.Get<object>(Property, Attribute.getter);
        var s = r.GetValue();

        if (s == null)
        {
            SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
            EditorGUILayout.LabelField("No valid objects");
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
        else
        {
            IEnumerable<object> valueList = (s as IEnumerable).Cast<object>().Where(x => x != null);

            if (valueList.Count() == 0)
            {
                SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
                EditorGUILayout.LabelField("No valid objects");
                SirenixEditorGUI.EndHorizontalPropertyLayout();

                return;
            }

//             if (valueList.Count() <= 2)
//             {
//                 SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
//             } else
            {
                SirenixEditorGUI.BeginBox(label);
            }

            List<object> old = (Property.ValueEntry.WeakSmartValue as IEnumerable<object>).ToList();
            resolver.QueueClear();

            for (int i = 0; i < valueList.Count(); i++)
            {
                object o = valueList.ElementAt(i);
                bool want = EditorGUILayout.ToggleLeft(new GUIContent(GetLabel(o)), old.Contains(o));

                if (want)
                {
                    resolver.QueueAdd(new object[]{o});
                }
                
                        
            }

            resolver.ApplyChanges();

//             if (valueList.Count() <= 2)
//             {
//                 SirenixEditorGUI.EndHorizontalPropertyLayout();
//             } else
            {
                SirenixEditorGUI.EndBox();
            }
        }
    }
}


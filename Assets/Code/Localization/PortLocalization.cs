using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using TMPro;


namespace Boomzap.HOPA
{
public class PortLocalization : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] UnityEditor.Localization.StringTableCollection locTable;
        [SerializeField] UnityEngine.Localization.LocaleIdentifier locale;

        [Button]
        void PortFromI2()
        {
            StringTable stringTable = locTable.GetTable(locale) as StringTable;

            ////I2.Loc.Localize[] objects = Transform.FindObjectsOfType<I2.Loc.Localize>(true);

            //foreach (var t in objects)
            //{
            //    TextMeshProUGUI ui = t.gameObject.GetComponent<TextMeshProUGUI>();
            //    if (ui == null)     // no textmesh.
            //        continue;
            //    LocalizeStringEvent existing = t.gameObject.GetComponent<LocalizeStringEvent>();
            //    if (!existing)
            //    {
            //        string id = ui.text;        // change this up?
            //                                    // look for duplicates

            //        long entryId = locTable.SharedData.GetId(id);
            //        if (entryId != 0)
            //        {
            //            //Duplicate Entry
            //            continue;
            //        }
            //        SharedTableData.SharedTableEntry key = locTable.SharedData.AddKey(id);
            //        if (key != null)
            //        {
            //            StringTableEntry tableEntry = stringTable.AddEntry(key.Id, ui.text);

            //            UnityEditor.EditorUtility.SetDirty(stringTable);
            //            UnityEditor.EditorUtility.SetDirty(stringTable.SharedData);

            //            LocalizeStringEvent newItem = t.gameObject.AddComponent<LocalizeStringEvent>();
            //            newItem.StringReference = new LocalizedString(locTable.TableCollectionNameReference, tableEntry.KeyId);
            //        }
            //    }
             //   DestroyImmediate(t);
           // }
        }
#endif
    }
}
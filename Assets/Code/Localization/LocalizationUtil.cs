using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;

using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
#endif

namespace ho
{
    public enum TableCategory
    {
        UI,
        Game,
        Conversation,
        Trivia,
        MissingTranslations
    }
    public class LocalizationUtil : SimpleSingleton<LocalizationUtil>
    {
#if UNITY_EDITOR
        //NOTE: Bank Key is the raw psb layer name for HO Objects. Ex: o_duck
        //      Localization Key is the combined name of room and HO object name. Ex. ho_22/o_duck
        public static bool FindEntryInLocalizationBank(string localizationKey, string bankKey, TableCategory category)
        {
            //["o"] = Normal Hidden Object
            //["k"] = Key Item
            //["d"] = Door Item
            //["x"] = Collection Item
            //["p"] = Pair Item
            //["t"] = Trivia Item

            if (bankKey.StartsWith("p_") || bankKey.StartsWith("k_") || bankKey.StartsWith("d_"))
            {
                //Replace Object Prefix for bank key
                bankKey = "o_" + bankKey.Substring(2);
            }

            if (LocalizationSettings.SelectedLocale == null)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            }

            var localizationBank = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == "LocalizationBank");
            var locale = LocalizationSettings.SelectedLocale;

            var bankSharedEntry = localizationBank.SharedData.GetEntry(bankKey);

            if(bankSharedEntry == null)
            {
                Debug.Log($"Key {bankKey} does not exist on Localization Bank");
                return false;
            }

            //Get Translations from Bank and add it to Main Game Translation

            var tableToInsert = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == category.ToString());
            
            if(tableToInsert == null)
            {
                //Table not found
                Debug.LogError($"Table with category {category.ToString()} does not exist! Please make sure that table name is the same as the category.");
                return false;
            }

            if(localizationBank.StringTables.Count != tableToInsert.StringTables.Count)
            {
                //Locales count did not match
                Debug.LogError($"The number of locales found for {category.ToString()} did not match with the Localization Bank. Please make sure that the number of languages used is the same as the bank.");
                return false;
            }

            for(int i = 0; i < localizationBank.StringTables.Count; i++)
            {
                if(localizationBank.StringTables[i].LocaleIdentifier != tableToInsert.StringTables[i].LocaleIdentifier)
                {
                    //Locales order did not match
                    Debug.LogError($"Languages column did not match. Please make sure that the languages column are in the same exact order.");
                    return false;
                }
            }

            for (int i = 0; i < localizationBank.StringTables.Count; i++)
            {
                //Get Bank Entry from current Language
                var bankEntry = localizationBank.StringTables[i].GetEntry(bankKey);

                if (bankEntry == null) continue;

                var currentStringTable = tableToInsert.StringTables[i];
                var tableEntry = currentStringTable.GetEntry(localizationKey);
                if(tableEntry == null)
                {
                    //Add New Entry from Table if it does not exist
                    currentStringTable.AddEntry(localizationKey, bankEntry.Value);
                }
                else
                {
                    tableEntry.Value = bankEntry.Value;
                }
            }

            foreach (var table in tableToInsert.StringTables)
                EditorUtility.SetDirty(table);

            EditorUtility.SetDirty(tableToInsert);

            return true;
        }
#endif


        public static string FindLocalizationEntry(string key, string value = "", bool returnKey = false, TableCategory category = TableCategory.Game)
        {
            //LocalizationSettings.InitializationOperation.WaitForCompletion();
            //LocalizationEditorSettings.EditorEvents.

            if (LocalizationSettings.SelectedLocale == null)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            }
            StringTableEntry entry = null;

            if (Application.isPlaying)
            {
                var table = LocalizationSettings.StringDatabase.GetTable(category.ToString());

                entry = table.GetEntry(key);
                //Debug.Log($"Table Name: {table.ToString()} English Table Count: {table.Count} Key Name: {key} Found Entry? {entry != null}");
                if (entry != null)
                {
                    //If needs to return key, for display object texts;
                    if (returnKey) return entry.Key;

                    //Return key if value is empty
                    if (string.IsNullOrEmpty(entry.Value)) return entry.Key;

                    //Return value if not empty
                    return entry.Value;
                }
                else
                {
#if UNITY_EDITOR
                    if (LocalizationSettings.SelectedLocale != null)
                    {
                        Debug.LogWarning($"Key {key} not found on Table {category.ToString()} with Language {LocalizationSettings.SelectedLocale.LocaleName}");
                        //Adds new entry for missing UI key on runtime.
                        if(string.IsNullOrEmpty(key) == false && category == TableCategory.UI)
                        {
                            Debug.Log($"Key {key} was added to table");
                            table.AddEntry(key, string.Empty);
                            UnityEditor.EditorUtility.SetDirty(table);
                            UnityEditor.EditorUtility.SetDirty(table.SharedData);
                        }
                    }
#endif
                }

            }
            else
            {
                //NOTE* This code block is using Editor Only Script which will cause an error when doing builds.
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    //Get String Table Collection Reference
                    //StringTableCollection tableCollection = (StringTableCollection)AssetDatabase.LoadAssetAtPath($"Assets/Localization/Tables/{category.ToString()}.asset", typeof(StringTableCollection));
                    var tableCollection = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == category.ToString());
                    var locale = LocalizationEditorSettings.GetLocales()[0];
                    //Get the Table from current locale

                    if(tableCollection == null)
                    {
                        Debug.LogWarning($"Table Collection with name {category.ToString()} not found.");
                        return null;
                    }

                    var englishTable = tableCollection.StringTables[0]; //English Table = 0
                    var englishEntry = englishTable.GetEntry(key);

                    if (englishEntry == null)
                    {
                        if(englishTable == null || englishTable.LocaleIdentifier != "en")
                        {
                            Debug.LogError("English Table not found");
                            return null;
                        }

                        entry = englishTable.AddEntry(key, value);
                    }
                    else
                    {
                        entry = englishEntry;
                        //Update Entry value if not null
                        if (string.IsNullOrEmpty(value) == false)
                        {
                            entry.Value = value;
                        }
                    }

                   
                    UnityEditor.EditorUtility.SetDirty(tableCollection);
                    UnityEditor.EditorUtility.SetDirty(tableCollection.SharedData);
                    UnityEditor.EditorUtility.SetDirty(englishTable);
                }
#endif
            }
            if (entry == null) return string.Empty;

            //Debug.Log($"{returnKey}");
            return returnKey ? entry.Key : entry.Value;
        }

        public static string UpdateLocalizationEntry(string key, string value = "", bool returnKey = false, TableCategory category = TableCategory.Game, string languageCode = "en")
        {
            //LocalizationSettings.InitializationOperation.WaitForCompletion();
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);

            StringTableEntry entry = null;

            //NOTE* This code block is using Editor Only Script which will cause an error when doing builds.
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                if (LocalizationSettings.HasSettings == false || LocalizationSettings.ProjectLocale == null)
                {
                    //Debug.LogWarning("No Localization Settings found");
                    return string.Empty;
                }

                //Get String Table Collection Reference
                //StringTableCollection tableCollection = (StringTableCollection)AssetDatabase.LoadAssetAtPath($"Assets/Localization/Tables/{category.ToString()}.asset", typeof(StringTableCollection));
                var tableCollection = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == category.ToString());
                var locale = LocalizationSettings.SelectedLocale;
                //Get the Table from current locale
                StringTable strTable = tableCollection.GetTable(locale.Identifier) as StringTable;

                long keyId = strTable.SharedData.GetId(key);

                entry = strTable.GetEntry(keyId);

                if (entry != null && keyId != 0)
                {
                    Debug.Log($"Entry Found with key {key}");
                    //Update Value
                    if (string.IsNullOrEmpty(value) == false || languageCode.Equals("en") == false)
                    {
                        entry.Value = value;
                    }
                    return returnKey ? entry.Key : entry.Value;
                }
                else
                {
                    Debug.LogWarning($"Key {key} with value {value} is not found in current table {category}");
                }

                if (entry == null && keyId != 0)
                    entry = strTable.AddEntry(key, value);

                UnityEditor.EditorUtility.SetDirty(strTable);
                UnityEditor.EditorUtility.SetDirty(tableCollection);
                UnityEditor.EditorUtility.SetDirty(tableCollection.SharedData);
            }
#endif
            if (entry == null) return key;

            return returnKey ? entry.Key : entry.Value;
        }

#if UNITY_EDITOR

        public static void RemoveEntry(string key, TableCategory category)
        {
            var tableCollection = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == category.ToString());
            //Get the Table from current locale
            var entry = tableCollection.SharedData.GetEntry(key);

            //Debug.Log(entry);

            if (entry != null && entry.Id != 0)
            {
                Debug.Log($"Removed entry with key {key}");
                tableCollection.SharedData.RemoveKey(entry.Id);
            }
            else
            {
                Debug.Log($"Key not found");
            }

            UnityEditor.EditorUtility.SetDirty(tableCollection);
            UnityEditor.EditorUtility.SetDirty(tableCollection.SharedData);

        }
#endif


        [Button]
        void Awake()
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
        }
    }
}



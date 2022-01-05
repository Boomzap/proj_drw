using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine.Localization;
using ho;
using System.IO;
using UnityEditor;

using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;

using UnityEditor.Localization;
using System.Linq;
#endif

namespace Boomzap.HOPA
{
    public class LocalizationTools : SimpleSingleton<LocalizationTools>
    {
#if UNITY_EDITOR
        [BoxGroup("Loc Util")]
        public string keyToDelete;

        [BoxGroup("Loc Util")]
        public TableCategory deleteTableEntry;


        [BoxGroup("Loc Util"), Button]
        void DeleteKey()
        {
            LocalizationUtil.RemoveEntry(keyToDelete, deleteTableEntry);
        }

        [BoxGroup("Loc Translation Helper")]
        public Object tsvFile;

        [BoxGroup("Loc Translation Helper")]
        public TableCategory tableForInsertion;

        [BoxGroup("Loc Translation Helper")]
        public List<string> linesWithoutKey = new List<string>();

        [BoxGroup("Loc Translation Helper"), Button]
        public void MatchTSVFile()
        {
            linesWithoutKey.Clear();

            string path = AssetDatabase.GetAssetPath(tsvFile);

            string sheet = File.ReadAllText(path);

            string[] rows = sheet.Split('\n');

            string[] headerColumns = rows[0].Split('\t');

            // 0 = Key Column
            // 1 = en - English
            // 2 = de - German
            // 3 = es - Spanish
            // 4 = fr - French
            // 5 = it - Italian
            // 6 = nl = Dutch
            // 7 = pt = Portuguese

            for (int i = 1; i < rows.Length; i++)
            {
                string[] columnData = rows[i].Split('\t');

                for (int cidx = 0; cidx < columnData.Length; cidx++)
                {
                    if (string.IsNullOrEmpty(columnData[cidx])) continue;
                    if (cidx == columnData.Length - 1) continue; //Skip Last Edited Column
                    if (cidx == 0 || cidx == 1) continue; //Skip Key Column and English Column

                    string trimmedKey = columnData[0].Trim(); // Trim Keys because some entries have extra space in the end

                    Debug.Log($"Adding Key {trimmedKey} {GetLanguageCodeByIndex(cidx)}");

                    string keyUsed = LocalizationUtil.UpdateLocalizationEntry(trimmedKey, columnData[cidx], false, tableForInsertion, GetLanguageCodeByIndex(cidx));

                    bool isKeySameWithEnglish = trimmedKey.Equals(columnData[1]);

                    if (isKeySameWithEnglish)
                        Debug.Log($"Key data {trimmedKey} is same with English Data");

                    if (isKeySameWithEnglish == false && keyUsed.Equals(columnData[0]) && linesWithoutKey.Contains(rows[i]) == false)
                    {
                        //Key Was returned and not found in table 
                        //Register this line as lines without key
                        linesWithoutKey.Add(rows[i]);
                        cidx = columnData.Length; // end line already;
                    }
                }
            }
        }

        [BoxGroup("Loc Translation Helper"), Button]
        public void DeleteTranslatedEntries()
        {
            linesWithoutKey.Clear();

            string path = AssetDatabase.GetAssetPath(tsvFile);

            string sheet = File.ReadAllText(path);

            string[] rows = sheet.Split('\n');

            string[] headerColumns = rows[0].Split('\t');

            // 0 = Key Column
            // 1 = en - English
            // 2 = nl - Dutch
            // 3 = fr - French
            // 4 = de - German
            // 5 = it - Italian
            // 6 = pt = Portuguese
            // 7 = es = Spanish

            for (int i = 1; i < rows.Length; i++)
            {
                string[] columnData = rows[i].Split('\t');

                string trimmedKey = columnData[0].Trim(); // Trim Keys because some entries have extra space in the end

                LocalizationUtil.RemoveEntry(trimmedKey, tableForInsertion);
            }
        }

        string GetLanguageCodeByIndex(int index)
        {
            //NOTE: Sheet Language Order must be the same as Locale Column Order
            switch (index)
            {
                case 2: return "nl";
                case 3: return "fr";
                case 4: return "de";
                case 5: return "it";
                case 6: return "pt";
                case 7: return "es";
                default: return "en";
            }
        }

        [BoxGroup("Loc Add Missing Translations"), Button]
        public void FindMissingTranslations()
        {
            var tableCollections = LocalizationEditorSettings.GetStringTableCollections();

            StringTableCollection missingTranslationsTable = LocalizationEditorSettings.GetStringTableCollection("MissingTranslations");

            missingTranslationsTable.SharedData.Entries.Clear();

            foreach (var tableCollection in tableCollections)
            {
                //Skip Scanning Sync Tables
                if (tableCollection.name.Contains("Sync")) continue;
                if (tableCollection == missingTranslationsTable) continue;

                Debug.Log($"Scanning table {tableCollection.name}...");
                Debug.Log($"Detected {tableCollection.StringTables.Count} Languages...");
                Debug.Log($"Detected {tableCollection.SharedData.Entries.Count} entries...");

                //Get Shared Entries
                var tableEntries = tableCollection.SharedData.Entries;


                //Iterate through each table entries
                foreach (var entry in tableEntries)
                {
                    //Iterate through localized columns
                    for (int i = 0; i < tableCollection.StringTables.Count; i++)
                    {
                        //Get the current Localization
                        var stringTable = tableCollection.StringTables[i];

                        //Get its localized version
                        var localizedEntry = stringTable.GetEntry(entry.Id);

                        bool isLocalized = localizedEntry != null;
                        bool isNotInMissingTranslations = missingTranslationsTable.SharedData.Entries.Any(x => x.Key == entry.Key) == false;
                        bool isValueEmpty = isLocalized ? localizedEntry.Value.Length == 0 || string.IsNullOrEmpty(localizedEntry.Value) : false;

                        if (i == 0 && (isLocalized == false || (isLocalized && isValueEmpty)))
                        {
                            //Skip Entries without english translation
                            break;
                        }

                        if (isLocalized && isValueEmpty && isNotInMissingTranslations)
                        {
                            //Record Missing Translation if it meets the condition
                            Debug.Log($"Value Empty {entry.Key} {stringTable.name}");
                            missingTranslationsTable.SharedData.Entries.Add(entry);

                            //Add English Translation
                            var englishTable = tableCollection.StringTables[0];
                            var englishEntry = englishTable.GetEntry(entry.Id);
                            if (englishEntry != null)
                                missingTranslationsTable.StringTables[0].AddEntry(englishEntry.KeyId, englishEntry.Value);

                            break;
                        }
                        else if (isLocalized == false)
                        {
                            Debug.Log($"Value Empty {entry.Key} {stringTable.name}");
                            missingTranslationsTable.SharedData.Entries.Add(entry);

                            //Add English Translation
                            var englishTable = tableCollection.StringTables[0];
                            var englishEntry = englishTable.GetEntry(entry.Id);
                            if (englishEntry != null)
                                missingTranslationsTable.StringTables[0].AddEntry(englishEntry.KeyId, englishEntry.Value);
                            break;
                        }
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(missingTranslationsTable);
            UnityEditor.EditorUtility.SetDirty(missingTranslationsTable.SharedData);
        }

        [BoxGroup("Update Localization Bank"), Button]
        public void UpdateLocalizationBank()
        {
            //var tableCollections = LocalizationEditorSettings.GetStringTableCollections();

            StringTableCollection bankTable = LocalizationEditorSettings.GetStringTableCollection("LocalizationBank");
            StringTableCollection gameTable = LocalizationEditorSettings.GetStringTableCollection("Game");

            if(gameTable == null || bankTable == null)
            {
                Debug.LogError("Localization Bank or Game Table was not found! Please make sure that the table is named correctly.");
                return;
            }

            for(int i = 0; i < gameTable.StringTables.Count; i++)
            {
                if (gameTable.StringTables[i].LocaleIdentifier != bankTable.StringTables[i].LocaleIdentifier)
                {
                    Debug.LogError("Localized Column count does not match! Please make sure that the column order of languages in sheet is the same.");
                    return;
                }
            }

            //bankTable.SharedData.Entries.Clear();
            //return;

            var bankEntries = bankTable.SharedData.Entries;
            var gameEntries = gameTable.SharedData.Entries;

            float entryCount = gameEntries.Count;

            float progress = 0;
            float step = 1 / entryCount;

            EditorUtility.DisplayProgressBar("Updating Localization Bank", "Initializing...", progress);

            //Iterate through each Localized Entries
            for (int i = 0; i < gameEntries.Count; i++)
            {
                progress += entryCount;
                EditorUtility.DisplayProgressBar("Updating Localization Bank", $"Processing Items {i}/{gameEntries.Count}", progress);
                var gameEntry = gameEntries[i];

                //Then Iterate through each available Language/String Table
                for (int tableIndex = 0; tableIndex < gameTable.StringTables.Count; tableIndex++)
                {
                    var localizedEntry = gameTable.StringTables[tableIndex].GetEntry(gameEntry.Key);

                    if (localizedEntry == null)
                    {
                        if (tableIndex == 0)
                        {
                            //Move to next Entry if no english entry
                            break;
                        }
                        //Skip not localized entries.
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(localizedEntry.Value))
                    {
                        if (tableIndex == 0)
                        {
                            //Move to next Entry if english empty
                            break;
                        }
                        //Skip blank entries
                        continue;
                    }

                    string localizedKey = localizedEntry.Key.Split('/').Last();

                    Debug.Log($"Current localized key: {localizedKey} Localized Key Entry: {localizedEntry.Key}");

           

                    if (localizedKey.Contains("prompt"))
                    {
                        //Skip Door/Key Prompt Messages
                        continue;
                    }

                    bool isHoObject = localizedKey.StartsWith("x_") || localizedKey.StartsWith("o_") || localizedKey.StartsWith("p_") || localizedKey.StartsWith("k_") || localizedKey.StartsWith("d_");

                    if (localizedKey.StartsWith("p_") || localizedKey.StartsWith("k_") || localizedKey.StartsWith("d_"))
                    {
                        //Replace Object Prefix for bank key
                        localizedKey = "o_" + localizedKey.Substring(2);
                    }

                    Debug.Log($"Current localized key Entry Number: {i} Table Index: {tableIndex} Key: {localizedKey} isHOObject? {isHoObject}");

                    if (isHoObject == false)
                    {
                        continue;
                    }

                    var currentEntry = bankTable.StringTables[tableIndex].GetEntry(localizedKey);

                    if (currentEntry == null)
                    {
                        //Add New Entry in localization bank
                        Debug.Log($"Added new entry with key {localizedKey} and value {localizedEntry.Value}");
                        bankTable.StringTables[tableIndex].AddEntry(localizedKey, localizedEntry.Value);
                    }
                    else
                    {
                        //Update Current Entry Value if it exists
                        //currentEntry.Key = localizedKey;
                        currentEntry.Value = localizedEntry.Value;
                    }

                }
            }

            gameTable.SharedData.Entries = gameEntries.OrderBy(x => x.Key).ToList();
            bankTable.SharedData.Entries = bankEntries.OrderBy(x => x.Key).ToList();

            for(int i = 0; i < gameTable.StringTables.Count; i++)
            {
                EditorUtility.SetDirty(gameTable.StringTables[i]);
                EditorUtility.SetDirty(bankTable.StringTables[i]);
            }

            EditorUtility.SetDirty(gameTable);
            EditorUtility.SetDirty(gameTable.SharedData);
            EditorUtility.SetDirty(bankTable);
            EditorUtility.SetDirty(bankTable.SharedData);

            Debug.Log("Update complete");
            EditorUtility.ClearProgressBar();
        }


        [BoxGroup("Loc Clean Up")]
        public TableCategory tableToClearEntries;

        [BoxGroup("Loc Clean Up"), Button]
        public void CleanUpTable()
        {
            StringTableCollection table = LocalizationEditorSettings.GetStringTableCollection(tableToClearEntries.ToString());

            if (table != null)
            {
                table.SharedData.Entries.Clear();

                foreach(var stringTable in table.StringTables)
                {
                    stringTable.Clear();
                    EditorUtility.SetDirty(stringTable);
                }
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);

                Debug.Log($"Table {tableToClearEntries.ToString()} entries deleted.");
            }
        }
#endif
    }
}


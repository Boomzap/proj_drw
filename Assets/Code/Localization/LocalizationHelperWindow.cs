
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEngine.Localization;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor;
using System.Linq;
using UnityEditor.Localization.Plugins.Google.Columns;
using System;
using UnityEngine.Localization.Tables;
using System.Collections.ObjectModel;
using UnityEditor.Localization.UI;
using ho;
#endif

namespace Boomzap.HOPA.Editor
{
    public class LocalizationHelperWindow : MonoBehaviour
    {
#if UNITY_EDITOR

        [MenuItem("Localization/Conversation", false, 100)]
        [MenuItem("Localization/Game", false, 100)]
        //[MenuItem("Localization/Trivia", priority = 3)]
        [MenuItem("Localization/UI", false, 100)]

     

        #region Conversation Table

        [MenuItem("Localization/Conversation/Push", false, 100)]
        static void PushSortConversation()
        {
            PushMerge(TableCategory.Conversation.ToString());
        }

        [MenuItem("Localization/Conversation/Pull", false, 100)]
        static void PullConversation()
        {
            PullTable(TableCategory.Conversation.ToString());
        }

        [MenuItem("Localization/Conversation/Open", false, 99)]
        static void OpenConversationTable()
        {
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(TableCategory.Conversation.ToString());
            LocalizationTablesWindow.ShowWindow(tableCollection);
        }

        #endregion

        #region Game Table
        [MenuItem("Localization/Game/Push", false, 100)]
        static void PushGameTable()
        {
            PushMerge(TableCategory.Game.ToString());
        }

        [MenuItem("Localization/Game/Pull", false, 100)]
        static void PullGame()
        {
            PullTable(TableCategory.Game.ToString());
        }

        [MenuItem("Localization/Game/Open", false, 99)]
        static void OpenGameTable()
        {
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(TableCategory.Game.ToString());
            LocalizationTablesWindow.ShowWindow(tableCollection);
        }

        #endregion

        #region Trivia Table
        //[MenuItem("Localization/Trivia/Push", priority = 1)]
        //static void PushSortTrivia()
        //{
        //    PushSortTable(TableCategory.Trivia.ToString());
        //}

        //[MenuItem("Localization/Trivia/Pull", priority = 2)]
        //static void PullTrivia()
        //{
        //    PullTable(TableCategory.Trivia.ToString());
        //}

        //[MenuItem("Localization/Trivia/Open", priority = 0)]
        //static void OpenTriviaTable()
        //{
        //    var tableCollection = LocalizationEditorSettings.GetStringTableCollection(TableCategory.Trivia.ToString());
        //    LocalizationTablesWindow.ShowWindow(tableCollection);
        //}
        #endregion

        #region UI Table
        [MenuItem("Localization/UI/Push", false, 100)]
        static void PushSortUI()
        {
            PushMerge(TableCategory.UI.ToString());
        }

        [MenuItem("Localization/UI/Pull", false, 100)]
        static void PullUI()
        {
            PullTable(TableCategory.UI.ToString());
        }

        [MenuItem("Localization/UI/Open", false, 99)]
        static void OpenUITable()
        {
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(TableCategory.UI.ToString());
            LocalizationTablesWindow.ShowWindow(tableCollection);
        }
        #endregion

        static void PullTable(string tableName)
        {
            EditorUtility.DisplayProgressBar($"Import {tableName} Table", $"Importing {tableName} table from Google Sheets", 0);
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);

            //tableCollection.SharedData.Entries.Clear();

            //for (int i = 0; i < tableCollection.StringTables.Count; i++)
            //{
            //    tableCollection.StringTables[i].Clear();
            //}

            GoogleSheetsExtension ex = tableCollection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;
            if (ex == null)
            {
                Debug.LogError("Google sheets extension not found! Please attach a valid Google service to the collection");
                EditorUtility.ClearProgressBar();
                return;
            }

            GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
            gs.SpreadSheetId = ex.SpreadsheetId;

            EditorUtility.DisplayProgressBar($"Import {tableName} Table", $"Importing {tableName} table to Google Sheets", 0.1f);

            try
            {
                gs.PullIntoStringTableCollection(ex.SheetId, tableCollection, ex.Columns, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.ClearProgressBar();
            }


            EditorUtility.SetDirty(tableCollection);
            foreach (var table in tableCollection.StringTables)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            Debug.Log("Done Importing Sheet.");
            EditorUtility.ClearProgressBar();

            PushSortTable(tableName);

            AssetDatabase.SaveAssets();
        }

        static void PushSortTable(string tableName)
        {
            EditorUtility.DisplayProgressBar($"Export {tableName} Table", $"Exporting {tableName} table to Google Sheets", 0);
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);

            GoogleSheetsExtension ex = tableCollection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;
            if (ex == null)
            {
                Debug.LogError("Google sheets extension not found! Please attach a valid Google service to the collection");
                EditorUtility.ClearProgressBar();
                return;
            }

            tableCollection.SharedData.Entries = tableCollection.SharedData.Entries.OrderBy(x => x.Key).ToList();

            GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
            gs.SpreadSheetId = ex.SpreadsheetId;

            EditorUtility.DisplayProgressBar($"Export {tableName} Table", $"Exporting {tableName} table to Google Sheets", 0.1f);

            try
            {
                gs.PushStringTableCollection(ex.SheetId, tableCollection, ex.Columns);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.SetDirty(tableCollection);
            foreach (var table in tableCollection.StringTables)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            Debug.Log("Done Exporting Sheet.");
            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
        }

        static void PushMerge(string tableName)
        {
            EditorUtility.DisplayProgressBar($"Import {tableName} Table", $"Importing {tableName} table from Google Sheets", 0);
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);

            //Clear Sync Table Data before merge
            var syncTable = LocalizationEditorSettings.GetStringTableCollection("SyncTable");
            syncTable.SharedData.Entries.Clear();

            for (int i = 0; i < syncTable.StringTables.Count; i++)
            {
                syncTable.StringTables[i].Clear();
            }

            if (tableCollection.StringTables.Count != syncTable.StringTables.Count)
            {
                Debug.LogError($"StringTables differ between MergeTable and {tableCollection.name}, they need to have the same locales in the same order.");
                return;
            }

            for (int i = 0; i < tableCollection.StringTables.Count; i++)
            {
                //Debug.Log($"{tableCollection.StringTables[i].LocaleIdentifier} {syncTable.StringTables[i].LocaleIdentifier}");
                if (tableCollection.StringTables[i].LocaleIdentifier != syncTable.StringTables[i].LocaleIdentifier)
                {
                    Debug.LogError($"StringTables differ between MergeTable and {tableCollection.name}, they need to have the same locales in the same order.");
                    return;
                }
            }

            long curTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            LastEditedTimeMetadata lastSync = tableCollection.SharedData.Metadata.GetMetadata<LastEditedTimeMetadata>();
            if (lastSync == null)
            {
                lastSync = new LastEditedTimeMetadata();
                lastSync.lastModifiedTimeUTC = 0;
                tableCollection.SharedData.Metadata.AddMetadata(lastSync);
            }

            GoogleSheetsExtension ex = tableCollection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;
            if (ex == null)
            {
                Debug.LogError("Google sheets extension not found! Please attach a valid Google service to the collection");
                EditorUtility.ClearProgressBar();
                return;
            }

            GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
            gs.SpreadSheetId = ex.SpreadsheetId;

            try
            {
                gs.PullIntoStringTableCollection(ex.SheetId, syncTable, ex.Columns);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.ClearProgressBar();
            }

            List<string> toRemoveKeys = new List<string>();

            foreach (var entry in tableCollection.GetRowEnumerator())
            {
                LastEditedTimeMetadata entryLastEdited = entry.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>();
                if (entryLastEdited == null)
                {
                    entry.KeyEntry.Metadata.AddMetadata(lastSync);
                    entryLastEdited = lastSync;
                }

                var syncEntry = syncTable.SharedData.GetEntry(entry.KeyEntry.Key);

                if (syncEntry != null)
                {
                    //syncEntry = syncTable.SharedData.GetEntry(entry.KeyEntry.Id);
                    long syncEntryLastModified = syncEntry.Metadata.GetMetadata<LastEditedTimeMetadata>()?.lastModifiedTimeUTC ?? curTime;

                    if (syncEntryLastModified == curTime)
                    {
                        Debug.LogWarning($"Loc Entry {entry.KeyEntry.Key} does not have modification metadata.");
                    }

                    if(entryLastEdited.lastModifiedTimeUTC < syncEntryLastModified)
                    {
                        for(int i = 0; i < entry.TableEntries.Length; i++)
                        {
                            var lastSyncEntry = syncTable.StringTables[i].GetEntry(entry.KeyEntry.Key);

                            if(entry.TableEntries[i] == null)
                            {
                                tableCollection.StringTables[i].AddEntry(entry.KeyEntry.Key, string.Empty);
                                Debug.LogWarning($"Entry with key {entry.KeyEntry.Key} does not have a valid entry in string table {tableCollection.StringTables[i].TableCollectionName}");
                            }
                            entry.TableEntries[i].Value = lastSyncEntry?.Value ?? String.Empty;
                        }
                    }
                }
                else
                {
                    // not in mergeTable, remove it locally if our entry's modified time is older than mergeTable's last sync time
                    if (entryLastEdited.lastModifiedTimeUTC < lastSync.lastModifiedTimeUTC)
                    {
                        toRemoveKeys.Add(entry.KeyEntry.Key);
                    }
                }
            }


            // now, add any rows in mergeTable that don't exist locally and are newer than last sync time (otherwise this means it was deleted locally)
            foreach (var e in syncTable.GetRowEnumerator())
            {
                var tableEntry = tableCollection.SharedData.GetEntry(e.KeyEntry.Key);
                if (tableEntry != null)
                    continue;

                // if there is no merge data, the entry was surely added manually. in which case, we will add it.

                long mergeLastModified = e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>()?.lastModifiedTimeUTC ?? curTime;

                if (mergeLastModified > lastSync.lastModifiedTimeUTC)
                {
                    var newKey = tableCollection.SharedData.AddKey(e.KeyEntry.Key);
                    newKey.Metadata = e.KeyEntry.Metadata;

                    for (int i = 0; i < e.TableEntries.Length; i++)
                    {
                        tableCollection.StringTables[i].AddEntry(e.KeyEntry.Key, e.TableEntries[i]?.Value ?? "");
                    }
                }

            }

            foreach (var remove in toRemoveKeys)
            {
                tableCollection.SharedData.RemoveKey(remove);
                for (int i = 0; i < tableCollection.StringTables.Count; i++)
                    tableCollection.StringTables[i].RemoveEntry(remove);
            }

            EditorUtility.SetDirty(tableCollection);
            foreach (var table in tableCollection.StringTables)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            EditorUtility.SetDirty(syncTable);
            foreach (var table in syncTable.StringTables)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            lastSync.lastModifiedTimeUTC = curTime;
            EditorUtility.SetDirty(tableCollection.SharedData);

            PushSortTable(tableName);
        }

        [MenuItem("Localization/Find Missing Translations", false, 120)]
        static void FindMissingTranslations()
        {
            LocalizationTools.instance.FindMissingTranslations();
            PushSortTable("MissingTranslations");

            var tableCollection = LocalizationEditorSettings.GetStringTableCollection("MissingTranslations");

            GoogleSheetsExtension ex = tableCollection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;
            if (ex == null)
            {
                Debug.LogError("Google sheets extension not found! Please attach a valid Google service to the collection");
                EditorUtility.ClearProgressBar();
                return;
            }

            GoogleSheets.OpenSheetInBrowser(ex.SpreadsheetId, ex.SheetId);
        }

        [MenuItem("Localization/Open Google Sheet", false, 120)]
        static void OpenGoogleSheet()
        {
            var tableCollection = LocalizationEditorSettings.GetStringTableCollection("Game");

            GoogleSheetsExtension ex = tableCollection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;
            if (ex == null)
            {
                Debug.LogError("Google sheets extension not found! Please attach a valid Google service to the collection");
                EditorUtility.ClearProgressBar();
                return;
            }

            GoogleSheets.OpenSheetInBrowser(ex.SpreadsheetId, ex.SheetId);
        }
#endif
    }


}

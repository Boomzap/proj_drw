using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Localization.Settings;
using UnityEditor;
using UnityEditor.Localization.Editor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEngine;
using UnityEditor.Localization.Plugins.Google.Columns;
using UnityEngine.Localization.Metadata;

using UnityEngine.Localization.Tables;

namespace Boomzap.HOPA.Editor
{
    public class LocalizationSync
    {
        //[MenuItem("HOPA/Localization Sync %#l")]
        static void Sync()
        {
            EditorUtility.DisplayProgressBar("Syncing localization", "Preparing", 0f);

            StringTableCollection mergeTable = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == "SyncConversation");
            if (mergeTable == null)
            {
                Debug.LogError("No merge table found");
                return;
            }

            float progPer = 1f / LocalizationEditorSettings.GetAssetTableCollections().Count;
            float progStep = progPer * 0.5f;
            float curProg = 0f;

            foreach (var collection in LocalizationEditorSettings.GetStringTableCollections())
            {
                if (collection == mergeTable)
                    continue;

                if (collection.StringTables.Count != mergeTable.StringTables.Count)
                {
                    Debug.LogError($"StringTables differ between MergeTable and {collection.name}, they need to have the same locales in the same order.");
                    continue;
                }

                bool isOK = true;
                for (int i = 0; i < collection.StringTables.Count; i++)
                {
                    if (collection.StringTables[i].LocaleIdentifier != mergeTable.StringTables[i].LocaleIdentifier)
                    {
                        Debug.LogError($"StringTables differ between MergeTable and {collection.name}, they need to have the same locales in the same order.");
                        isOK = false;
                        break;
                    }
                }

                if (!isOK) continue;


                long curTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                LastEditedTimeMetadata lastSync = collection.SharedData.Metadata.GetMetadata<LastEditedTimeMetadata>();
                if (lastSync == null)
                {
                    lastSync = new LastEditedTimeMetadata();
                    lastSync.lastModifiedTimeUTC = 0;
                    collection.SharedData.Metadata.AddMetadata(lastSync);
                }

                GoogleSheetsExtension ex = collection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;
                
                if (ex == null) 
                    continue;

                GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
                gs.SpreadSheetId = ex.SpreadsheetId;

                var columnMapping = ex.Columns.Where(x => x is IPullKeyColumn || x is LastEditedTimeColumn).ToList();

                EditorUtility.DisplayProgressBar("Syncing localization", $"Pulling {collection.name}...", curProg);
                try
                {
                    gs.PullIntoStringTableCollection(ex.SheetId, mergeTable, ex.Columns, true);
                }
                catch(Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.Log(e);
                    Debug.Log("Failed Importing Sheet Data");
                }
                
                curProg += progStep;

                List<long> toRemoveIDs = new List<long>();

                // find entries in current which are newer than sheet
                foreach (var e in collection.GetRowEnumerator())
                {
                    if (mergeTable.SharedData.Contains(e.KeyEntry.Id))
                    {
                        // if the entry does NOT have metadata, someone fucked with it manually. we'll sync it anyway but create a warning
                        var mergeEntry = mergeTable.SharedData.GetEntry(e.KeyEntry.Id);
                        long mergeLastModified = mergeEntry.Metadata.GetMetadata<LastEditedTimeMetadata>()?.lastModifiedTimeUTC ?? curTime;

                        if (mergeLastModified == curTime)
                        {
                            Debug.LogWarning($"Loc Entry {e.KeyEntry.Key} does not have modification metadata.");
                        }

                        // if our entry is older than mergeTable - replace
                        if (e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>().lastModifiedTimeUTC < mergeLastModified)
                        {
                            e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>().lastModifiedTimeUTC = mergeLastModified;

                            for (int i = 0; i < e.TableEntries.Length; i++)
                            {
                                var mergeStringEntry = mergeTable.StringTables[i].GetEntry(e.KeyEntry.Id);
                                e.TableEntries[i].Value = mergeStringEntry?.Value ?? "";
                            }
                        }
                    } else
                    {
                        // not in mergeTable, remove it locally if our entry's modified time is older than mergeTable's last sync time
                        if (e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>().lastModifiedTimeUTC <
                            lastSync.lastModifiedTimeUTC)
                            {
                                toRemoveIDs.Add(e.KeyEntry.Id);
                            }
                    }
                }

                // now, add any rows in mergeTable that don't exist locally and are newer than last sync time (otherwise this means it was deleted locally)
                foreach (var e in mergeTable.GetRowEnumerator())
                {
                    if (collection.SharedData.Contains(e.KeyEntry.Id))
                        continue;

                    // if there is no merge data, the entry was surely added manually. in which case, we will add it.

                    long mergeLastModified = e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>()?.lastModifiedTimeUTC ?? curTime;

                    if (mergeLastModified > lastSync.lastModifiedTimeUTC)
                    {
                        var newKey = collection.SharedData.AddKey(e.KeyEntry.Key, e.KeyEntry.Id);
                        newKey.Metadata = e.KeyEntry.Metadata;
                        
                        for (int i = 0; i < e.TableEntries.Length; i++)
                        {
                            collection.StringTables[i].AddEntry(e.KeyEntry.Id, e.TableEntries[i]?.Value ?? "");
                        }
                    }

                }
                
                foreach (var remove in toRemoveIDs)
                {
                    collection.SharedData.RemoveKey(remove);
                    for (int i = 0; i < collection.StringTables.Count; i++)
                        collection.StringTables[i].Remove(remove);
                }

                EditorUtility.SetDirty(collection);
                foreach (var table in collection.StringTables)
                {
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                }

                lastSync.lastModifiedTimeUTC = curTime;
                EditorUtility.SetDirty(collection.SharedData);

                columnMapping = ex.Columns;

                EditorUtility.DisplayProgressBar("Syncing localization", $"Pushing to {collection.name}...", curProg);
                curProg += progStep;
                gs.PushStringTableCollection(ex.SheetId, collection, columnMapping);

                //gs.PullIntoStringTableCollection(ex.SheetId, tempCollection, columnMapping, false);
            }

            EditorUtility.ClearProgressBar();
        }

        //[MenuItem("HOPA/Sync Conversation %#1")]
        static void SyncConversation()
        {
            SyncTable(ho.TableCategory.Conversation, "Sync" + ho.TableCategory.Conversation.ToString());
        }

        //[MenuItem("HOPA/Sync Game %#2")]
        static void SyncGame()
        {
            SyncTable(ho.TableCategory.Game, "Sync" + ho.TableCategory.Game.ToString());
        }

        //[MenuItem("HOPA/Import Sheets from Google Locs")]
        static void PullSheets()
        {
            EditorUtility.DisplayProgressBar("Syncing localization", "Preparing", 0f);

            StringTableCollection mergeTable = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == "SyncConversation");
            if (mergeTable == null)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("No merge table found");
                return;
            }


            float step = 1 / (LocalizationEditorSettings.GetStringTableCollections().Count - 1);
            float currProgress = 0;

            foreach (var collection in LocalizationEditorSettings.GetStringTableCollections())
            {

                if (collection.ToString().Contains("Sync"))
                {
                    continue;
                }

                GoogleSheetsExtension ex = collection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;

                if (ex == null)
                {
                    currProgress += step;
                    continue;
                }
              
                GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
                gs.SpreadSheetId = ex.SpreadsheetId;

                EditorUtility.DisplayProgressBar("Syncing localization", $"Pulling {collection.name}...", currProgress);

                try
                {
                    gs.PullIntoStringTableCollection(ex.SheetId, collection, ex.Columns, true);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("There is a missing entry in Google Sheets. Please check if there is a key missing.");
                    Debug.LogError(e);
                    EditorUtility.ClearProgressBar();
                }

                currProgress += step;

                EditorUtility.SetDirty(collection);
                foreach (var table in collection.StringTables)
                {
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                }
            }
            Debug.Log("Done Importing Sheets.");
            EditorUtility.ClearProgressBar();
        }

        //[MenuItem("HOPA/Export Sheets to Google Locs")]
        static void PushSheets()
        {
            EditorUtility.DisplayProgressBar("Syncing localization", "Preparing", 0f);

            float step = 1 / (LocalizationEditorSettings.GetStringTableCollections().Count - 1);
            float currProgress = 0;

            foreach (var collection in LocalizationEditorSettings.GetStringTableCollections())
            {

                if (collection.ToString().Contains("Sync"))
                {
                    continue;
                }

                GoogleSheetsExtension ex = collection.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;

                if (ex == null)
                {
                    currProgress += step;
                    continue;
                }

                GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
                gs.SpreadSheetId = ex.SpreadsheetId;

                EditorUtility.DisplayProgressBar("Syncing localization", $"Exporting {collection.name}...", currProgress);

                try
                {
                    gs.PushStringTableCollection(ex.SheetId, collection, ex.Columns);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                    EditorUtility.ClearProgressBar();
                }
                currProgress += step;

                EditorUtility.SetDirty(collection);
                foreach (var table in collection.StringTables)
                {
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                }
            }
            Debug.Log("Done Exporting Sheets.");
            EditorUtility.ClearProgressBar();
        }

        static void SyncTable(ho.TableCategory category, string syncTableName)
        {
            EditorUtility.DisplayProgressBar("Syncing localization", "Preparing", 0f);

            StringTableCollection mergeTable = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == syncTableName);
            if (mergeTable == null)
            {
                Debug.LogError("No sync table found");
                EditorUtility.ClearProgressBar();
                return;
            }

            StringTableCollection syncTable = LocalizationEditorSettings.GetStringTableCollections().FirstOrDefault(x => x.name == category.ToString());
            if (syncTable == null)
            {
                Debug.LogError($"No {category.ToString()} table found");
                return;
            }
            
            GoogleSheetsExtension ex = syncTable.Extensions.FirstOrDefault(x => x is GoogleSheetsExtension) as GoogleSheetsExtension;

            if (ex == null)
            {
                Debug.LogError($"No Google Service found for {category.ToString()} table");
                return;
            }

            if (syncTable.StringTables.Count != mergeTable.StringTables.Count)
            {
                Debug.LogError($"StringTables differ between MergeTable and {syncTable.name}, they need to have the same locales in the same order.");
                return;
            }

            bool isOK = true;
            for (int i = 0; i < syncTable.StringTables.Count; i++)
            {
                if (syncTable.StringTables[i].LocaleIdentifier != mergeTable.StringTables[i].LocaleIdentifier)
                {
                    Debug.LogError($"StringTables differ between MergeTable and {syncTable.name}, they need to have the same locales in the same order.");
                    isOK = false;
                    break;
                }
            }

            if (!isOK) return;

            // Compare last syn time
            long curTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            LastEditedTimeMetadata lastSync = syncTable.SharedData.Metadata.GetMetadata<LastEditedTimeMetadata>();
            if (lastSync == null)
            {
                lastSync = new LastEditedTimeMetadata();
                lastSync.lastModifiedTimeUTC = 0;
                syncTable.SharedData.Metadata.AddMetadata(lastSync);
            }

            //Get Linked Sheets Provider
            GoogleSheets gs = new GoogleSheets(ex.SheetsServiceProvider);
            gs.SpreadSheetId = ex.SpreadsheetId;

            float curProg = 0f;

            var columnMapping = ex.Columns.Where(x => x is IPullKeyColumn || x is LastEditedTimeColumn).ToList();

            EditorUtility.DisplayProgressBar("Syncing localization", $"Pulling {category.ToString()}...", curProg);

            try 
            {
                gs.PullIntoStringTableCollection(ex.SheetId, mergeTable, ex.Columns, true);
            }
            catch(Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Failed to pull data from Google Sheets");
                Debug.Log(e);
            }
            
            curProg = 0.5f; // Done Pull


            List<long> toRemoveIDs = new List<long>();

            // find entries in current which are newer than sheet
            foreach (var e in syncTable.GetRowEnumerator())
            {
                if (mergeTable.SharedData.Contains(e.KeyEntry.Id))
                {
                    // if the entry does NOT have metadata, someone fucked with it manually. we'll sync it anyway but create a warning
                    var mergeEntry = mergeTable.SharedData.GetEntry(e.KeyEntry.Id);
                    long mergeLastModified = mergeEntry.Metadata.GetMetadata<LastEditedTimeMetadata>()?.lastModifiedTimeUTC ?? curTime;

                    if (mergeLastModified == curTime)
                    {
                        Debug.LogWarning($"Loc Entry {e.KeyEntry.Key} does not have modification metadata.");
                    }

                    // if our entry is older than mergeTable - replace
                    if (e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>().lastModifiedTimeUTC < mergeLastModified)
                    {
                        e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>().lastModifiedTimeUTC = mergeLastModified;

                        for (int i = 0; i < e.TableEntries.Length; i++)
                        {
                            var mergeStringEntry = mergeTable.StringTables[i].GetEntry(e.KeyEntry.Id);
                            var mergeEntryValue = mergeStringEntry?.Value ?? "";

                            //Remove Keys with same Value in sync table
                            if(e.TableEntries[i].Value.Equals(mergeEntryValue))
                            {
                                toRemoveIDs.Add(mergeStringEntry.KeyId);
                            }
                            else
                            {
                                e.TableEntries[i].Value = mergeEntryValue;
                            }
                        }
                    }
                }
                else
                {
                    // not in mergeTable, remove it locally if our entry's modified time is older than mergeTable's last sync time
                    if (e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>().lastModifiedTimeUTC < lastSync.lastModifiedTimeUTC)
                    {
                        toRemoveIDs.Add(e.KeyEntry.Id);
                    }
                }
            }

            // now, add any rows in mergeTable that don't exist locally and are newer than last sync time (otherwise this means it was deleted locally)
            foreach (var e in mergeTable.GetRowEnumerator())
            {
                if (syncTable.SharedData.Contains(e.KeyEntry.Id))
                    continue;

                // if there is no merge data, the entry was surely added manually. in which case, we will add it.

                long mergeLastModified = e.KeyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>()?.lastModifiedTimeUTC ?? curTime;

                if (mergeLastModified > lastSync.lastModifiedTimeUTC)
                {
                    var newKey = syncTable.SharedData.AddKey(e.KeyEntry.Key, e.KeyEntry.Id);
                    newKey.Metadata = e.KeyEntry.Metadata;

                    for (int i = 0; i < e.TableEntries.Length; i++)
                    {
                        syncTable.StringTables[i].AddEntry(e.KeyEntry.Id, e.TableEntries[i]?.Value ?? "");
                    }
                }

            }

            foreach (var remove in toRemoveIDs)
            {
                syncTable.SharedData.RemoveKey(remove);
                for (int i = 0; i < syncTable.StringTables.Count; i++)
                    syncTable.StringTables[i].Remove(remove);
            }

            EditorUtility.SetDirty(syncTable);
            foreach (var table in syncTable.StringTables)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            lastSync.lastModifiedTimeUTC = curTime;
            EditorUtility.SetDirty(syncTable.SharedData);

            columnMapping = ex.Columns;

            EditorUtility.DisplayProgressBar("Syncing localization", $"Pushing to {syncTable.name}...", curProg);

            try
            {
                gs.PushStringTableCollection(ex.SheetId, syncTable, columnMapping);
            }
            catch(Exception e)
            {
                Debug.Log(e);
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.ClearProgressBar();
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using Boomzap.Conversation;

namespace ho
{
    // provide ordering to the conversation nodes for localization sheet.
    public class ConversationLocStringHelper : UnityEditor.AssetModificationProcessor
    {
//         static string[] OnWillSaveAssets(string[] paths)
//         {
//             foreach (string path in paths)
//             {
//                 if (path.Contains("i2Languages.asset"))
//                     return paths;
//             }
// 
//             foreach (string path in paths)
//             {
//                 var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
//                 if (assetType == typeof(Conversation))
//                 {
//                     Conversation c = AssetDatabase.LoadAssetAtPath<Conversation>(path);
//                     ProcessConversationTidy(c);
//                     ProcessConversationOrdering(c);
//                 }
//             }
// 
//             return paths;
//         }

        //static AssetDeleteResult OnWillDeleteAsset(string sourcePath, RemoveAssetOptions options)
        //{
        //    bool didUpdateLoc = false;
        //    int removeCount = 0;

        //    System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(sourcePath);

        //    if (assetType == typeof(Conversation))
        //    {
        //        string filePath = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
                
        //        List<string> existingTerms = LocalizationManager.GetTermsList(filePath);
        //        if (existingTerms.Count > 0)
        //        {
        //            LanguageSourceData sourceData = LocalizationManager.GetSourceContaining(existingTerms[0]);

        //            if (sourceData != null) 
        //            {
        //                foreach (var term in existingTerms)
        //                    sourceData.RemoveTerm(term);

        //                didUpdateLoc = true;
        //                removeCount = existingTerms.Count;
        //            }

        //        }
        //    }

        //    if (didUpdateLoc)
        //    {
        //        foreach (var s in LocalizationManager.Sources)
        //        {
        //            s.UpdateDictionary(true);
        //        }
        //        LocalizationManager.UpdateSources();

        //        Debug.Log($"On deleting {sourcePath}, {removeCount} localization terms were also removed.");
        //        LocalizationEditor.ParseTerms(true, false, false);
        //    }

        //    return AssetDeleteResult.DidNotDelete;
        //}

        //static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        //{
        //    bool didUpdateLoc = false;
        //    System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(sourcePath);

        //    if (assetType == typeof(Conversation))
        //    {
        //        int replaceCount = 0;
        //        string filePath = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
        //        string newFilePath = System.IO.Path.GetFileNameWithoutExtension(destinationPath);

        //        if (filePath.Equals(newFilePath, System.StringComparison.OrdinalIgnoreCase))
        //        {
        //            return AssetMoveResult.DidNotMove;
        //        }
        //        // fixup the loc..
        //        List<string> existingTerms = LocalizationManager.GetTermsList(filePath);

        //        foreach (var term in existingTerms)
        //        {
        //            LanguageSourceData sourceData = LocalizationManager.GetSourceContaining(term);
                        
        //            if (sourceData == null) continue;

        //            TermData termData = sourceData.GetTermData(term);

        //            if (termData == null) continue;

        //            string key, cat;
        //            LanguageSourceData.DeserializeFullTerm(term, out key, out cat);
        //            string newKey = newFilePath + "/" + key;

        //            replaceCount++;
        //            termData.Term = newKey;
        //        }

        //        if (replaceCount > 0)
        //        {
        //            Debug.Log($"On renaming {filePath} to {newFilePath}, {replaceCount} localization terms were recategorized. Please make sure you update your loc sheet.");
        //            didUpdateLoc = true;
        //        }
        //    }

        //    if (didUpdateLoc)
        //    {
        //        foreach (var s in LocalizationManager.Sources)
        //        {
        //            s.UpdateDictionary(true);
        //        }
        //        LocalizationManager.UpdateSources();

        //        //LocalizationEditor.ScheduleUpdateTermsToShowInList();
        //        LocalizationEditor.ParseTerms(true, false, false);
        //    }

        //    return AssetMoveResult.DidNotMove;
        //}

        
    }
}

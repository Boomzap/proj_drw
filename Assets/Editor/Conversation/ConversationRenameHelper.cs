using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Boomzap.Conversation
{
    public class ConversationRenameHelper : AssetPostprocessor
    {
//         static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//         {
//             bool didUpdateLoc = false;
//             for (int i = 0; i < movedAssets.Length; i++)
//             {
//                 //Debug.Log($"Moved asset: {movedAssets[i]} from {movedFromAssetPaths[i]}");
//                 System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(movedAssets[i]);
// 
//                 if (assetType == typeof(Conversation))
//                 {
//                     int replaceCount = 0;
//                     string filePath = System.IO.Path.GetFileNameWithoutExtension(movedFromAssetPaths[i]);
//                     string newFilePath = System.IO.Path.GetFileNameWithoutExtension(movedAssets[i]);
//                     // fixup the loc..
//                     List<string> existingTerms = LocalizationManager.GetTermsList(filePath);
// 
//                     foreach (var term in existingTerms)
//                     {
//                         LanguageSourceData sourceData = LocalizationManager.GetSourceContaining(term);
//                         
//                         if (sourceData == null) continue;
// 
//                         TermData termData = sourceData.GetTermData(term);
// 
//                         if (termData == null) continue;
// 
//                         string key, cat;
//                         LanguageSourceData.DeserializeFullTerm(term, out key, out cat);
//                         string newKey = newFilePath + "/" + key;
// 
//                         replaceCount++;
//                         termData.Term = newKey;
//                     }
// 
//                     if (replaceCount > 0)
//                     {
//                         Debug.Log($"On renaming {filePath} to {newFilePath}, {replaceCount} localization terms were recategorized. Please make sure you update your loc sheet.");
//                         didUpdateLoc = true;
//                     }
//                 }
//             }
// 
//             if (didUpdateLoc)
//             {
//                 foreach (var s in LocalizationManager.Sources)
//                 {
//                     s.UpdateDictionary(true);
//                 }
//                 LocalizationManager.UpdateSources();
//             }
//         }
    }
}

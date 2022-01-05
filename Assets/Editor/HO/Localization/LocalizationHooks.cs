using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor.Localization.Editor;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using UnityEditor;

namespace Boomzap.HOPA.Editor
{
    [InitializeOnLoad]
    public class LocalizationHooks
    {
        static LocalizationHooks()
        {
            LocalizationEditorSettings.EditorEvents.TableEntryAdded += TableEntryAdded;
            LocalizationEditorSettings.EditorEvents.TableEntryModified += TableEntryModified;
        }

        static void TableEntryAdded(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry)
        {
            LastEditedTimeMetadata md = new LastEditedTimeMetadata();
            entry.Metadata.AddMetadata(md);
            md.lastModifiedTimeUTC = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        static void TableEntryModified(SharedTableData.SharedTableEntry entry)
        {
            LastEditedTimeMetadata md = entry.Metadata.GetMetadata<LastEditedTimeMetadata>();
            if (md == null)
            {
                md = new LastEditedTimeMetadata();
                entry.Metadata.AddMetadata(md);
            }

            md.lastModifiedTimeUTC = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            

            // TODO update this to set only the active table (this isn't easily exposed rn)
            foreach (var v in LocalizationEditorSettings.GetStringTableCollections())
            {
                EditorUtility.SetDirty(v.SharedData);
            }
        }
    }
}

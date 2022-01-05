using System;
using UnityEditor.Localization.Plugins.Google.Columns;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Collections.Generic;

namespace Boomzap.HOPA.Editor
{
    /// <summary>
    /// LocaleMetadataColumn is a version of SheetColumn just for handling Metadata.
    /// This can now be added to the Column Mappings for any Push or Pull request.
    /// </summary>
    public class LastEditedTimeColumn : LocaleMetadataColumn<LastEditedTimeMetadata>
    {
        public override PushFields PushFields => PushFields.Value;

        // currently doesn't do anything
        public override void PullMetadata(StringTableEntry entry, LastEditedTimeMetadata metadata, string cellValue, string cellNote)
        {
            // Metadata will be null if the entry does not already contain any.
            if (metadata == null)
            {
                metadata = new LastEditedTimeMetadata();
                entry.AddMetadata(metadata);
            }

            if (!long.TryParse(cellValue, out metadata.lastModifiedTimeUTC))
                metadata.lastModifiedTimeUTC = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public override void PushHeader(StringTableCollection collection, out string header, out string headerNote)
        {
            header = "Last Edited";
            headerNote = null;
        }

        public override void PushCellData(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, out string value, out string note)
        {
            LastEditedTimeMetadata md = keyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>();

            if (md == null)
            {
                md = new LastEditedTimeMetadata{ lastModifiedTimeUTC = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
                keyEntry.Metadata.AddMetadata(md);
            }
            
            value = md.lastModifiedTimeUTC.ToString();
            note = null;
        }

        public override void PullCellData(SharedTableData.SharedTableEntry keyEntry, string cellValue, string cellNote)
        {
            LastEditedTimeMetadata md = keyEntry.Metadata.GetMetadata<LastEditedTimeMetadata>();
            long modTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            long.TryParse(cellValue, out modTime);

            if (md == null)
            {
                md = new LastEditedTimeMetadata { lastModifiedTimeUTC = modTime };
                keyEntry.Metadata.AddMetadata(md);
            }

            md.lastModifiedTimeUTC = modTime;
        }


        // currently doesn't do anything
        public override void PushMetadata(LastEditedTimeMetadata metadata, out string value, out string note)
        {
            note = null;
            value = metadata.lastModifiedTimeUTC.ToString();
        }
    }
}
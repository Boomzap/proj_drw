using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Localization.Metadata;

// see Editor/HOPA/Localization/LocalizationHooks.cs

namespace Boomzap.HOPA
{
    [Metadata(AllowedTypes = MetadataType.StringTableEntry | MetadataType.SharedStringTableEntry | MetadataType.StringTable, AllowMultiple = false)]
    [Serializable]
    public class LastEditedTimeMetadata : IMetadata
    {
        public long lastModifiedTimeUTC;
    }
}

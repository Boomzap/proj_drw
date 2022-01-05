using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boomzap.Character
{

    [Serializable]
    public class CharacterReference : AssetReferenceGameObject
    {
        public CharacterReference(string guid)
            : base(guid)
        {
        
        }

        public static bool operator ==(CharacterReference a, CharacterReference b)
        {
            return System.Object.Equals(a,b);
        }

        public static bool operator !=(CharacterReference a, CharacterReference b)
        {
            return !System.Object.Equals(a,b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is CharacterReference)
            {
                return AssetGUID.Equals((obj as CharacterReference).AssetGUID, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override bool ValidateAsset(UnityEngine.Object obj)
        {
            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;    
                return go.GetComponent<Character>() != null;
            }

            return false;
        }

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) return false;

            return ValidateAsset(obj);
#else
            return false;
#endif
        }
    }
}
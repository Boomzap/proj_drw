using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Boomzap.Character
{
    // per-character 'bio sheet' and stats that are available outside of the addressable
    
    public class CharacterInfo : ScriptableObject
    {
        // these fields are required
        [SerializeField] public TMPro.TMP_FontAsset     useFont = null;
        [SerializeField] public Color                   characterColor = Color.white;

        [SerializeField] public string                  firstName = "";
        [SerializeField] public string                  lastName = "";

        [SerializeField] public CharacterReference      characterRef;

        [SerializeField, HideInInspector]
        SerializableGUID _guid = new SerializableGUID();
        public SerializableGUID guid { get { return _guid; } }

        #if UNITY_EDITOR
        public Character EditorCharacter
        {
            get
            {
                if (characterRef.RuntimeKeyIsValid())
                {
                    if (characterRef.editorAsset != null)
                        return characterRef.editorAsset.GetComponent<Character>();
                }
                return null;
            }
        }
        #endif

        //////////////////////////////
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boomzap.Character
{
    public class CharacterManager : SimpleSingleton<CharacterManager>
    {
        [ReadOnly]  public CharacterInfo[] characters;
        public CharacterCanvas characterCanvas;

        public CharacterInfo GetCharacterByName(string name)
        {
            return characters.FirstOrDefault(x => StrReplace.Equals(name, x.name));
        }

        public CharacterInfo GetCharacterByGuid(SerializableGUID guid)
        {
            return characters.First(x => x.guid == guid);
        }


        [Button]
        void RefreshCharacterList()
        {
            #if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:"+ typeof(CharacterInfo).Name); 

            characters = guids.Select(x => AssetDatabase.LoadAssetAtPath<CharacterInfo>(AssetDatabase.GUIDToAssetPath(x))).ToArray();
            #endif
        }

        private void OnEnable()
        {
            RefreshCharacterList();
        }


        private void Start()
        {
            foreach (var c in characters)
            {
                StrReplace.AddChunk(c.name, c.firstName);
                StrReplace.AddChunk(c.name + "_last", c.lastName);
            }

//             characterRenderer.SetSlotCount(5);
//             characterRenderer.UpdateSlot(0, characters[0], "Default.explaining", "sleeping", "look_back", false, false);
        }
    }
}

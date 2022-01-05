using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class ObjectRenamer : MonoBehaviour
    {

        public string namePrefix;

        [Button]
        void RenameChildPrefix()
        {
            var childObjs = transform.GetComponentsInChildren<SpriteRenderer>();

            foreach (var obj in childObjs)
                obj.name = namePrefix + obj.sprite.name;
        }

        [Button]
        void RenameChild()
        {
            var childObjs = transform.GetComponentsInChildren<SpriteRenderer>();

            for (int i = 0; i < childObjs.Length; i++)
            {
                string suffix = "_" + childObjs[i].sprite.texture.name.Split('_').Last();

                if(suffix.Equals("_white"))
                {
                    suffix = "_default";
                }

                childObjs[i].name = $"sew_{i + 1:D2}" + suffix;
                childObjs[i].color = Color.white;
            }
        }
    }


}

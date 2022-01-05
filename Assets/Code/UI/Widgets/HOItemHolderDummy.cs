using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOItemHolderDummy : HOItemHolder
    {
        void Awake()
        {

        }
        
        public override void OnActiveHOChange(bool animate = true)
        {

        }

        public override void SetObjects(IEnumerable<HOFindableObject> findableObjects, bool animate = false)
        {

            findables = new List<HOFindableObject>(findableObjects);

            foreach (var f in findables)
            {
                if (!prevFindables.Contains(f))
                    prevFindables.Add(f);
            }

        }

        public override void SetObject(HOFindableObject findableObject, bool animate = false)
        {
            findables = new List<HOFindableObject>();
            findables.Add(findableObject);

            foreach (var f in findables)
            {
                if (!prevFindables.Contains(f))
                    prevFindables.Add(f);
            }

        }

        private void Update()
        {
            
        }

        public override void Clear()
        {

        }
    }
}
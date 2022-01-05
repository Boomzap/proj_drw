using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ho
{
    public abstract class HOSubUI : MonoBehaviour
    {
        public abstract void         Setup(List<HOFindableObject> findableObjects, int totalToFind);
        public abstract void         SetItemFoundTotal(int currentFound, int total, bool isFirst);
        public abstract HOItemHolder GetItemHolder(HOFindableObject forObject);
        public abstract void         UpdateItemList(HOFindableObject previousObject, IEnumerable<HOFindableObject> newObjects);

        public abstract void         AnimateObjectCollection(HOFindableObject obj, Image clonedImage, Image clonedSDF, Vector2 startPos, UnityAction onDone);

        public abstract int          GetListCapacity();
        public abstract bool         IsValidUIForLogic(HOLogic logic);

        public abstract void         OnActiveHOChange();
    }
}

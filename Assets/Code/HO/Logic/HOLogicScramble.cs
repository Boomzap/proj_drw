using ho;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[FriendlyName("Scramble")]
public class HOLogicScramble : HOLogic
{
    protected override bool AllowSubHOs() => true;
    protected override bool AllowKeyItems() => true;
    
    protected override List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
    {
        List<HOFindableObject> selectableObjects = allValidObjects
            .Where(x => x.IsValidForLogic(this) && string.IsNullOrEmpty(x.objectGroup) && 
            x.isSpecialStoryItem == false
            ).ToList();
        allValidObjects.RemoveAll(x => x.IsValidForLogic(this));

        return selectableObjects;
    }

    public override bool OnItemClicked(HOFindableObject obj)
    {
        if (currentObjects.Contains(obj))
        {
            currentObjects.Remove(obj);
            itemsLeftToFind--;

            if (currentObjects.Count == 0 && futureObjects.Count == 0)
                reactor.OnItemListEmpty();

            // if we found a group item, check if we have any more of that group.
            // if not, we're done and can dispose. otherwise, we're just refreshing the object.
            if (!string.IsNullOrEmpty(obj.objectGroup))
            {
                var remainingGroup = currentObjects.Where(x => x.objectGroup == obj.objectGroup);

                if (remainingGroup.Count() > 0)
                {
                    reactor.UpdateActiveItemInList(obj, remainingGroup);

                    return true;
                }
            }

            var nextFindable = futureObjects.Count == 0 ? null : futureObjects.First();

            if (nextFindable && !string.IsNullOrEmpty(nextFindable.objectGroup))
            {
                var nextGroup = futureObjects.Where(x => x.objectGroup == nextFindable.objectGroup);
                futureObjects.RemoveAll(x => x.objectGroup == nextFindable.objectGroup);
                reactor.UpdateActiveItemInList(obj, nextGroup);
                currentObjects.AddRange(nextGroup);
                return true; 
            }
            
            if (nextFindable)
            {
                futureObjects.Remove(nextFindable);
                currentObjects.Add(nextFindable);
            }

            reactor.UpdateActiveItemInList(obj, new HOFindableObject[] { nextFindable });    

            return true;
        }

        return false;
    }

}

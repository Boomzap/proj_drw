using ho;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[FriendlyName("Silhouette")]
public class HOLogicSilhouette : HOLogic
{
    protected override bool AllowKeyItems() => false;
    protected override bool IgnoreDifficulty() => true;

    protected override List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
    {
        List<HOFindableObject> selectableObjects = allValidObjects
            .Where(x => x.IsValidForLogic(this) && string.IsNullOrEmpty(x.objectGroup) &&
            x.isSpecialStoryItem == false
        ).ToList();
        allValidObjects.RemoveAll(x => x.IsValidForLogic(this) && string.IsNullOrEmpty(x.objectGroup) && x.isSpecialStoryItem == false);

        return selectableObjects;
    }


    protected override List<HOFindableObject> PreventObjectsFromDeletion(ref List<HOFindableObject> toDelete)
    {
        List<HOFindableObject> preserve = toDelete.Where(x => x.IsValidForLogic(this)).ToList();
        toDelete.RemoveAll(x => x.IsValidForLogic(this));

        // ensure no Detail
        var ensureDelete = toDelete.Where(x => x.IsValidForLogic<HOLogicDetail>()).ToList();
        toDelete.RemoveAll(x => ensureDelete.Contains(x));

        var l = base.PreventObjectsFromDeletion(ref toDelete);

        preserve.AddRange(l);
        toDelete.AddRange(ensureDelete);

        return preserve;
    }

    protected override void GroupSelectedObjectsToCurrentAndFuture(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
    {
        futureObjects.Clear();
        
        int currentCount = Math.Min(maxItemsToShow, selectedObjectsList.Count);
        int futureCount = Math.Min(totalToFind - currentCount, (selectedObjectsList.Count - currentCount));

        currentObjects.AddRange(selectedObjectsList.Take(currentCount));
        selectedObjectsList.RemoveRange(0, currentCount);
        futureObjects.AddRange(selectedObjectsList.Take(futureCount));
        selectedObjectsList.RemoveRange(0, futureCount);

        if (currentObjects.Count + futureObjects.Count < totalToFind)
        {
            Debug.LogWarning($"Silhouette wanted {totalToFind} items, but we only came up with {currentObjects.Count}");
        }

        validObjectsList.AddRange(selectedObjectsList);
    }

    public override bool OnItemClicked(HOFindableObject obj)
    {
        if (currentObjects.Contains(obj))
        {
            currentObjects.Remove(obj);
            itemsLeftToFind--;

            if (currentObjects.Count == 0 && futureObjects.Count == 0)
                reactor.OnItemListEmpty();

            HOFindableObject nextFindable = futureObjects.Count > 0 ? futureObjects.First() : null;
            if (nextFindable)
            {
                futureObjects.Remove(nextFindable);
                currentObjects.Add(nextFindable);
                reactor.UpdateActiveItemInList(obj, new HOFindableObject[] { nextFindable });
            } else
            {
                futureObjects.Remove(nextFindable);
                reactor.UpdateActiveItemInList(obj, new HOFindableObject[] { null });
            }

            return true;
        }

        return false;
    }

}

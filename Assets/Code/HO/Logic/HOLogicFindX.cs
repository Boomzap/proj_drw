using ho;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[FriendlyName("FindX")]
public class HOLogicFindX : HOLogic
{
    protected override bool AllowKeyItems() => false;
    protected override bool IgnoreDifficulty() => true;
    protected override bool AllowSubHOs() => false;


    protected override List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
    {
        List<HOFindableObject> selectableObjects = allValidObjects
            .Where(x => x.IsValidForLogic(this) && 
            x.isSpecialStoryItem == false
            ).ToList();

        allValidObjects.RemoveAll(x => x.IsValidForLogic(this) && x.isSpecialStoryItem == false);

        return selectableObjects;
    }

    protected override void GroupSelectedObjectsToCurrentAndFuture(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
    {
        futureObjects.Clear();
        currentObjects.AddRange(selectedObjectsList);

        if (currentObjects.Count < totalToFind)
        {
            totalToFind = currentObjects.Count;
            Debug.LogWarning($"FindX wanted {totalToFind} items, but we only came up with {currentObjects.Count}");
        }
    }

    protected override List<HOFindableObject> PreventObjectsFromDeletion(ref List<HOFindableObject> toDelete)
    {
        List<HOFindableObject> preserve = toDelete.Where(x => !x.IsValidForLogic(this)).ToList();
        toDelete.RemoveAll(x => preserve.Contains(x));

        // ensure all FindX not selected are removed
        var ensureDelete = toDelete.Where(x => x.IsValidForLogic<HOLogicFindX>()).ToList();
        toDelete.RemoveAll(x => ensureDelete.Contains(x));

        var ol = base.PreventObjectsFromDeletion(ref toDelete);
        preserve.AddRange(ol);
        toDelete.AddRange(ensureDelete);

        return preserve;
    }

    public override bool OnItemClicked(HOFindableObject obj)
    {
        if (currentObjects.Contains(obj))
        {
            currentObjects.Remove(obj);
            itemsLeftToFind--;

            if (currentObjects.Count == 0 && futureObjects.Count == 0)
                reactor.OnItemListEmpty();

            reactor.UpdateActiveItemInList(obj, currentObjects);

            return true;
        }

        return false;
    }

}

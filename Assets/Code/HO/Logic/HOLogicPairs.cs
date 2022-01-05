using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ho;
using System.Linq;

[FriendlyName("Pairs")]
public class HOLogicPairs : HOLogic
{
    protected override bool AllowSubHOs() => false;
    protected override bool AllowKeyItems() => false;
    protected override bool IgnoreDifficulty() => true;

    public HOFindableObject currentSelectedPair;

    public override int itemsTotalToFind { get { return totalToFind * 2; } }

    protected override List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
    {
        List<HOFindableObject> selectableObjects = allValidObjects
            .Where(x => x.IsValidForLogic(this) &&
            x.isSpecialStoryItem == false
            ).ToList();

        allValidObjects.RemoveAll(x => x.IsValidForLogic(this) && x.isSpecialStoryItem == false);

        return selectableObjects;
    }

    protected override void SelectObjectsFromValidObjects(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
    {
        int totalPairCount = totalToFind * 2;

        // Shuffle List
        var objsSelected = validObjectsList.GroupBy(x => x.objectGroup).OrderBy(y => Random.value).Where(x => x.Count<HOFindableObject>() == 2).ToList();

        if (totalPairCount > objsSelected.Count * 2)
        {
            Debug.LogError($"There is not enough pair objects. Total to find is set to {totalToFind} while valid objects is {validObjectsList.Count}");
            return;
        }

        int index = 0;
        while (selectedObjectsList.Count < totalPairCount)
        {
            var pairObject = objsSelected[index].ToArray<HOFindableObject>();

            foreach (var obj in pairObject)
            {
                //Debug.Log(obj.name);
                selectedObjectsList.Add(obj);
                validObjectsList.Remove(obj);
            }
            index++;
        }
    }

    protected override void GroupSelectedObjectsToCurrentAndFuture(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
    {
        futureObjects.Clear();
        currentObjects.AddRange(selectedObjectsList);

        if (currentObjects.Count < totalToFind)
        {
            totalToFind = currentObjects.Count;
            Debug.LogWarning($"Pairs wanted {totalToFind} items, but we only came up with {currentObjects.Count}");
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
            if(currentSelectedPair == null)
            {
                currentSelectedPair = obj;
                HOPairUI pairUI = UIController.instance.hoMainUI.CurrentSubUI as HOPairUI;
                pairUI.OnSelectItem(LocalizationUtil.FindLocalizationEntry(obj.displayKey));
                return false;
            }
            //If same object was clicked or not the same
            else if (currentSelectedPair == obj || currentSelectedPair.objectGroup != obj.objectGroup)
            {
                ClearSelection();
                HOPairUI pairUI = UIController.instance.hoMainUI.CurrentSubUI as HOPairUI;
                pairUI.OnSelectItem(string.Empty);
                return false;
            }
            else
            {
                //Remove paired Object
                currentObjects.Remove(currentSelectedPair);
                //currentSelectedPair = null;
            }
            currentObjects.Remove(obj);

            //Pairs are counted as 1
            itemsLeftToFind--;

            if (currentObjects.Count == 0 && futureObjects.Count == 0)
                reactor.OnItemListEmpty();

            reactor.UpdateActiveItemInList(currentSelectedPair, currentObjects);
            reactor.UpdateActiveItemInList(obj, currentObjects);

            return true;
        }

        return false;
    }

    public void ClearSelection()
    {
        currentSelectedPair = null;
    }

}

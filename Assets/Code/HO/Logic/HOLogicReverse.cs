using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ho;

[FriendlyName("Reverse")]
public class HOLogicReverse : HOLogic
{
    protected override bool AllowSubHOs() => false;
    protected override bool AllowKeyItems() => false;

    public Sprite selectedItem = null;

    protected override List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
    {
        List<HOFindableObject> selectableObjects = allValidObjects
            .Where(x => x.IsValidForLogic(this) && string.IsNullOrEmpty(x.objectGroup) &&
            x.isSpecialStoryItem == false
            ).ToList();
        allValidObjects.RemoveAll(x => x.IsValidForLogic(this));

        return selectableObjects;
    }

    protected override void SelectObjectsFromValidObjects(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
    {
        List<string> selectedGroups = new List<string>();

        if (!IgnoreDifficulty())
        {
            Debug.Log("!Ignore Difficulty");
            for (int i = 0; i < 3; i++)
            {
                var selectByDifficulty = SelectNByDifficulty(itemCountPerDifficulty[i], (HODifficulty)i, validObjectsList, true, selectedGroups);
                selectedObjectsList.AddRange(selectByDifficulty);
                itemCountPerDifficulty[i] -= selectByDifficulty.Count();
            }
        }
        else
        {
            selectedObjectsList.AddRange(SelectN(totalToFind, validObjectsList, true, selectedGroups));
        }

        if (selectedObjectsList.Count < totalToFind)
        {
            // care about difficulty here or no?
            int needCount = totalToFind - selectedObjectsList.Count;

            var randomObjs = validObjectsList.Where(x => x.isSpecialStoryItem == false).OrderBy(x => UnityEngine.Random.value).Take(needCount).ToList();
            foreach (var obj in randomObjs)
            {
                validObjectsList.Remove(obj);
            }

            selectedObjectsList.AddRange(randomObjs);
        }

        selectedObjectsList.ForEach(x =>
        {
            x.GetComponent<SpriteRenderer>().enabled = false;
            //x.sdfHitZone.enabled = false;
         });
    }

    public override bool OnItemClicked(HOFindableObject obj)
    {
        Debug.Log($"Clicked object with name {obj.name}");
        if(currentObjects.Contains(obj))
        {
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

            if (spriteRenderer.sprite != selectedItem)
            {
                selectedItem = null;
                return false;
            }

            //Animate Obj placement?
            spriteRenderer.enabled = true;
            selectedItem = null;

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
            }
            else
            {
                futureObjects.Remove(nextFindable);
                reactor.UpdateActiveItemInList(obj, new HOFindableObject[] { null });
            }

            return true;
        }


        return false;
    }

}
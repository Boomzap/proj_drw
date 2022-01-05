using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ho;
using System.Linq;

[FriendlyName("SpecialRiddle")]
public class HOLogicSpecialRiddle : HOLogicRiddle
{
    protected override List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
    {
        List<HOFindableObject> selectableObjects = allValidObjects
            .Where(x => x.IsValidForLogic(this) &&
            x.isSpecialStoryItem == false
            ).ToList();

        allValidObjects.RemoveAll(x => x.IsValidForLogic(this) && x.isSpecialStoryItem == false);

        return selectableObjects;
    }

}

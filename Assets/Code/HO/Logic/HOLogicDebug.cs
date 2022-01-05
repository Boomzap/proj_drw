using ho;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

[FriendlyName("Debug")]
public class HOLogicDebug : HOLogic
{

    public override void Initialize(HORoom room, HORoomReference roomRef, IHOReactor reactor, int listCapacity, Chapter.Entry entry, bool replayRoom = false)
    {
        maxItemsToShow = 2000;
        inactiveItemsToShow = 0;
        this.room = room;
        this.reactor = reactor;

        int itemTotal = room.interactiveObjects.Where(x => x is HOFindableObject).Count();

        unlockableSubHO = new List<HORoomReference>();



        foreach (var subRoom in room.subHO)
        {
            subRoom.roomPrefab.Init();
            itemTotal += subRoom.roomPrefab.interactiveObjects.Where(x => x is HOFindableObject).Count();
            unlockableSubHO.Add(subRoom);
        }

        this.totalToFind = itemTotal;
        this.keyItems = new List<string>();
        this.roomRef = roomRef;
            
        itemCountPerDifficulty = GetItemCounts(itemTotal, entry.itemDifficulty);

        currentObjects = new List<HOFindableObject>(entry.objectCount);
        futureObjects = new List<HOFindableObject>(entry.objectCount);

        triviaObjects = new List<HOInteractiveObject>();
        unlockedSubHO = new List<HORoomReference>();

        SetupRoomObjects();
    }

    protected override void SelectTriviaObject()
    {
        if (room.interactiveObjects.Any(x => x is HOTriviaObject))
        {
            var trivias = room.interactiveObjects.Where(x => x is HOTriviaObject).OrderBy(x => UnityEngine.Random.value).ToList();
            trivias?.ForEach(x => triviaObjects.Add(x));
        }
    }

    protected override void SetupRoomObjects()
    {
        currentObjects.Clear();
        futureObjects.Clear();

        List<HOFindableObject> allValidObjects = GetAllValidFindables();
        List<HOFindableObject> selectedObjects = new List<HOFindableObject>();

        //DestroyOccludedObjects();

        SelectTriviaObject();

        HandleSubHOFindables(ref allValidObjects, ref selectedObjects);

        // special items here

        List<HOFindableObject> selectableObjects = GetAllSelectableObjects(ref allValidObjects);

        SelectObjectsFromValidObjects(ref selectableObjects, ref selectedObjects);

        // remove subHO objects from valid objects list, as we destroy all ValidObjects later when selection is done
        selectableObjects.RemoveAll(x => x.transform.parent.gameObject != room.roomRoot);

        // remove all grouped objects which are part of a group

        var groupDuplicates = selectableObjects.Where(x => !string.IsNullOrEmpty(x.objectGroup)).ToList();
        allValidObjects.RemoveAll(x => groupDuplicates.Contains(x));
        selectableObjects.RemoveAll(x => groupDuplicates.Contains(x));

        //Skip Destroying objects in debug mode
        //Debug.Log($"Group de-dupe removed {groupDuplicates.Count()}");
        //foreach (var duplicate in groupDuplicates)
        //    GameObject.Destroy(duplicate.gameObject);

        GroupSelectedObjectsToCurrentAndFuture(ref selectableObjects, ref selectedObjects);

        allValidObjects.AddRange(selectableObjects);
        HandleSpecialStoryObjects(ref allValidObjects);

        // shuffle
        allValidObjects = allValidObjects.OrderBy(x => UnityEngine.Random.value).ToList();

        // remove a subset of remaining validObjects to spare from deletion, to serve as our inactive objects
        inactiveObjects = PreventObjectsFromDeletion(ref allValidObjects);

        //Skip Destroying objects in debug mode
        //foreach (var leftover in allValidObjects)
        //    GameObject.Destroy(leftover.gameObject);

        // destroy unused special items, or do they stay in the pool?

        var specialItems = inactiveObjects.Where(x => x.isSpecialStoryItem).ToList();

        foreach (var item in specialItems)
        {
            GameObject.Destroy(item.gameObject);
            inactiveObjects.Remove(item);
        }

        itemsLeftToFind = futureObjects.Count() + currentObjects.Count();
    }


    public override bool DebugHandleCtrl(bool isDown)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        var mousePos = HOGameController.instance.hoCamera.ScreenToWorldPoint(Input.mousePosition);
        var hitResult = room.HitTestAll(mousePos);
        bool haveHit = false;

        if (isDown)
        {
            foreach (var a in triviaObjects)
            {
                a.sdfRenderer.gameObject.SetActive(false);
            }

            foreach (var a in currentObjects)
            {
                a.sdfRenderer.gameObject.SetActive(false);
            }     

            foreach (var a in room.doorHandlers)
            {
                if (a == null || a.closedState == null)
                {
                    continue;
                }

                if (a.closedState.activeInHierarchy)
                {
                    a.closedState.GetComponent<HODoorItem>().sdfRenderer.gameObject.SetActive(false);
                }
                if (a.openState.activeInHierarchy)
                {
                    a.openState.GetComponent<HODoorItem>().sdfRenderer.gameObject.SetActive(false);
                }

                if(a.keyItem.sdfHitZone.hitZoneIndicator != null)
                {
                    a.keyItem.sdfRenderer.gameObject.SetActive(false);
                }
            }

        } else
        {
            foreach(var a in triviaObjects)
            {
                if(a.sdfHitZone.hitZoneIndicator != null)
                {
                    a.sdfRenderer.gameObject.SetActive(true);
                    a.sdfRenderer.material = a.sdfHitZone.hitZoneIndicator;
                    a.sdfHitZone.hitZoneIndicator.SetColor("_GlowColor", new Color(1f, 0f, 0f, 1f));
                }
            }


            foreach (var a in currentObjects)
            {
                if (a.sdfHitZone.hitZoneIndicator != null)
                {
                    a.sdfRenderer.gameObject.SetActive(true);
                    a.sdfRenderer.material = a.sdfHitZone.hitZoneIndicator;
                    a.sdfHitZone.hitZoneIndicator.SetColor("_GlowColor", new Color(1f, 0f, 0f, 1f));
                }
            }

            foreach (var a in room.doorHandlers)
            {
                if(a == null || a.closedState == null)
                {
                    continue;
                }

                if (a.closedState.activeInHierarchy)
                {
                    a.closedState.GetComponent<HODoorItem>().sdfRenderer.material = a.closedState.GetComponent<HODoorItem>().sdfHitZone.hitZoneIndicator;
                    a.closedState.GetComponent<HODoorItem>().sdfRenderer.gameObject.SetActive(true);
                    a.closedState.GetComponent<HODoorItem>().sdfHitZone.hitZoneIndicator.SetColor("_GlowColor", new Color(0.3f, 0.3f, 1f, 1f));
                }
                if (a.openState.activeInHierarchy)
                {
                    a.closedState.GetComponent<HODoorItem>().sdfRenderer.material = a.closedState.GetComponent<HODoorItem>().sdfHitZone.hitZoneIndicator;
                    a.openState.GetComponent<HODoorItem>().sdfRenderer.gameObject.SetActive(true);
                    a.openState.GetComponent<HODoorItem>().sdfHitZone.hitZoneIndicator.SetColor("_GlowColor", new Color(0.3f, 0.3f, 1f, 1f));
                }

                if (a.keyItem.sdfHitZone.hitZoneIndicator != null && a.keyItem.sdfRenderer != null)
                {
                    a.keyItem.sdfRenderer.gameObject.SetActive(true);
                    a.keyItem.sdfRenderer.material = a.closedState.GetComponent<HODoorItem>().sdfHitZone.hitZoneIndicator;
                }
            }

            foreach (var a in hitResult)
            {
                if (a.sdfHitZone.hitZoneIndicator != null)
                {
                    a.sdfHitZone.hitZoneIndicator.SetColor("_GlowColor", new Color(0f, 1f, 0f, 1f));
                }
            }


            foreach (var a in hitResult)
            {
                foreach (var hit in hitResult)
                {
                    if (hit is HOInteractiveObject)
                    {
                        haveHit = true;

                        var ssPos = HOGameController.instance.hoCamera.WorldToScreenPoint(hit.transform.position);
                        Vector2 lp;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIController.instance.hoMainUI.canvas.transform as RectTransform, ssPos,
                            UIController.instance.hoMainUI.canvas.worldCamera, out lp);


                        if(hit is HOTriviaObject == false && hit is HODoorItem == false)
                            UIController.instance.hoMainUI.SetDebugLabel(hit.GetDisplayText(), lp);
                        break;
                    }
                }
            }

        }

        if (!haveHit)
            UIController.instance.hoMainUI.SetDebugLabel("", Vector2.zero);
        return true;
        #else

        return false;
        #endif
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


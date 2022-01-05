using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

namespace ho
{
    public enum HODifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum HOLogicType
    {
        HOLogicStandard, //Normal
        HOLogicPairs,
        HOLogicPicture, //Image
        HOLogicRiddle,
        HOLogicFindX,
        HOLogicSilhouette,
        HOLogicScramble,
        HOLogicNoVowel
    }

    public abstract class HOLogic
    {
        public class HOPlayResult
        {
            public bool wasSuccess = false;
            public int itemsFound = 0;
        }

        public static HOLogic Create(string typeName)
        {
            //Debug.Log(typeName);
            //Debug.Log(Type.GetType(typeName));
            return Activator.CreateInstance(Type.GetType(typeName)) as HOLogic;
        }

        protected HORoomReference roomRef;
        protected HORoom room;
        protected IHOReactor reactor;
        protected int maxItemsToShow;
        protected int inactiveItemsToShow;
        protected int totalToFind;
        protected List<string> keyItems;
        protected int[] itemCountPerDifficulty = new int[3];
        protected Chapter.Entry chapterEntry;

        protected List<HORoomReference> unlockableSubHO;
        protected List<HORoomReference> unlockedSubHO;

        public int itemsLeftToFind { get; protected set; }
        public virtual int itemsTotalToFind { get { return totalToFind; } }

        public List<HOFindableObject> currentObjects { get; protected set; }
        public List<HOFindableObject> futureObjects { get; protected set; }

        public List<HOFindableObject> inactiveObjects = new List<HOFindableObject>();

        public List<HOInteractiveObject> triviaObjects = new List<HOInteractiveObject>();

        HOInteractiveObject selectedTrivia = null;

        protected virtual bool IgnoreDifficulty() => false;

        public virtual bool DebugHandleCtrl(bool isDown)
        {
            return false;
        }

        public static int[] GetItemCounts(int totalItems, HODifficulty sceneDifficulty)
        {
            int[] itemCounts = new int[3];
            var config = HOGameController.config;

            switch (sceneDifficulty)
            {
                case HODifficulty.Easy:
                    {
                        itemCounts[1] = Mathf.RoundToInt(config.easyDiffNormalItems * totalItems);
                        itemCounts[2] = Mathf.RoundToInt(config.easyDiffHardItems * totalItems);

                        break;
                    }
                case HODifficulty.Medium:
                    {
                        itemCounts[1] = Mathf.RoundToInt(config.mediumDiffNormalItems * totalItems);
                        itemCounts[2] = Mathf.RoundToInt(config.mediumDiffHardItems * totalItems);

                        break;
                    }
                case HODifficulty.Hard:
                    {
                        itemCounts[1] = Mathf.RoundToInt(config.hardDiffNormalItems * totalItems);
                        itemCounts[2] = Mathf.RoundToInt(config.hardDiffHardItems * totalItems);

                        break;
                    }
            }

            itemCounts[0] = totalItems - itemCounts[1] - itemCounts[2];

            return itemCounts;
        }

        public HOInteractiveObject GetActiveTriviaObject()
        {
            return selectedTrivia;
        }

        public virtual void StartGameplay()
        {
            reactor.SetInitialItemList(currentObjects, totalToFind);
        }

        public virtual void Initialize(HORoom room, HORoomReference roomRef, IHOReactor reactor, int listCapacity, Chapter.Entry entry, bool replayRoom = false)
        {
            chapterEntry = entry;
            maxItemsToShow = listCapacity;
            this.room = room;
            this.reactor = reactor;
            this.totalToFind = entry.objectCount;
            this.keyItems = new List<string>();
            this.roomRef = roomRef;
            inactiveItemsToShow = (int)(entry.objectCount * HOGameController.config.inactiveItemsAmount);

            itemCountPerDifficulty = GetItemCounts(totalToFind, entry.itemDifficulty);

            currentObjects = new List<HOFindableObject>(entry.objectCount);
            futureObjects = new List<HOFindableObject>(entry.objectCount);

            unlockableSubHO = new List<HORoomReference>(entry.unlockableSubHO);
            unlockedSubHO = new List<HORoomReference>(entry.unlockedSubHO);

            Savegame.HOSceneState state = GameController.save.GetSceneState(entry);

            if (GameController.instance.isUnlimitedMode)
            {
                state = null;
                replayRoom = false;
            }
               

            if(replayRoom || (state != null && state.completed))
            {
                state.hasSaveState = false;
            }

            if (state != null && state.hasSaveState && !HOGameController.instance.returnToMainMenu)
            {
                futureObjects = state.futureObjects.Select(x => room.FindObjectByName(x)).ToList();
                currentObjects = state.currentObjects.Select(x => room.FindObjectByName(x)).ToList();
                inactiveObjects = state.inactiveObjects.Select(x => room.FindObjectByName(x)).ToList();

                var keyItems = state.heldKeyItems.Select(x => room.FindObjectByName(x)).ToList();
                //Debug.Log($"{keyItems.Count} -> Held Key Items {state.heldKeyItems.Count}");

                var toDestroy = room.interactiveObjects.Where(x => x is HOFindableObject &&
                    !futureObjects.Contains(x as HOFindableObject) &&
                    !currentObjects.Contains(x as HOFindableObject) &&
                    !inactiveObjects.Contains(x as HOFindableObject) &&
                    !keyItems.Contains(x as HOFindableObject));

                foreach (var o in toDestroy)
                {
                    GameObject.Destroy(o.gameObject);
                }

                //Skip Deactivating key items for Drag and Drop
                //foreach (var i in keyItems)
                //{
                //    i.gameObject.SetActive(false);
                //}

                itemsLeftToFind = futureObjects.Count() + currentObjects.Count();

                SelectTriviaObject();
                Debug.Log($"Loaded logic saved state, items Left = {itemsLeftToFind}");

            } else
            {
                SetupRoomObjects();
            }
        }

        public virtual int GetFindableItemCount<T>() where T : HOFindableObject
        {
            return currentObjects.Count(x => x is T) + futureObjects.Count(x => x is T);
        }

        public virtual List<HOFindableObject> GetActiveItems()
        {
            return currentObjects;
        }

        public abstract bool OnItemClicked(HOFindableObject obj);

        protected List<HOInteractiveObject> GetOccludedObjects()
        {
            List<HOInteractiveObject> oob = room.GetOutOfBoundsObjects();

            //             foreach(Rect rc in UIController.instance.hoMainUI.GetCoveredAreas())
            //             {
            //                 var occludedLocal = room.GetObjectsCoveredByRect(rc);
            //                 oob.AddRange(occludedLocal);
            //             }

            return oob;
        }

        protected virtual List<HOFindableObject> GetAllValidFindables()
        {
            List<HOFindableObject> findableObjects = new List<HOFindableObject>();
            List<HOFindableObject> collectionObjects = new List<HOFindableObject>();
            var occluded = GetOccludedObjects();

            foreach (var obj in room.interactiveObjects.Where(x => x is HOFindableObject && !(x is HOKeyItem) && !occluded.Contains(x)))
            {
                HOFindableObject currObj = obj as HOFindableObject;
                
                //Record find x objects if current logic is not Find X
                if (this is HOLogicFindX == false && currObj.IsValidForLogic<HOLogicFindX>())
                {
                    //Debug.Log($"Object {currObj.name} is valid for find x");
                    collectionObjects.Add(currObj);
                    continue;
                }

                if (this is HOLogicPairs == false && currObj.IsValidForLogic<HOLogicPairs>())
                {
                    //Debug.Log($"Object {currObj.name} is valid for find x");
                    collectionObjects.Add(currObj);
                    continue;
                }

                //Remove Grouped objects for Pairs mode
                if (this is HOLogicPairs && currObj.IsValidForLogic<HOLogicPairs>() == false && string.IsNullOrEmpty(currObj.objectGroup) == false)
                {
                    collectionObjects.Add(currObj);
                    continue;
                }

                findableObjects.Add(obj as HOFindableObject);
            }

            //Destroy Find X & Pair Objects 
            foreach (var obj in collectionObjects)
            {
                GameObject.Destroy(obj.gameObject);
            }

            return findableObjects;
        }

        protected virtual bool AllowKeyItems() => true;
        protected virtual bool AllowSubHOs() => true;

        protected virtual void SetupRoomObjects()
        {
            currentObjects.Clear();
            futureObjects.Clear();

            List<HOFindableObject> allValidObjects = GetAllValidFindables();
            List<HOFindableObject> selectedObjects = new List<HOFindableObject>();

            DestroyOccludedObjects();

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

            Debug.Log($"Group de-dupe removed {groupDuplicates.Count()}");
            foreach (var duplicate in groupDuplicates)
                GameObject.Destroy(duplicate.gameObject);

            GroupSelectedObjectsToCurrentAndFuture(ref selectableObjects, ref selectedObjects);

            allValidObjects.AddRange(selectableObjects);
            HandleSpecialStoryObjects(ref allValidObjects);

            // shuffle
            allValidObjects = allValidObjects.OrderBy(x => UnityEngine.Random.value).ToList();

            // remove a subset of remaining validObjects to spare from deletion, to serve as our inactive objects
            inactiveObjects = PreventObjectsFromDeletion(ref allValidObjects);

            // and destroy everything else
            foreach (var leftover in allValidObjects)
                GameObject.Destroy(leftover.gameObject);

            // destroy unused special items, or do they stay in the pool?

            var specialItems = inactiveObjects.Where(x => x.isSpecialStoryItem).ToList();

            foreach (var item in specialItems)
            {
                GameObject.Destroy(item.gameObject);
                inactiveObjects.Remove(item);
            }

            itemsLeftToFind = futureObjects.Count() + currentObjects.Count();
        }

        protected virtual void HandleSpecialStoryObjects(ref List<HOFindableObject> allValidObjects)
        {
            if (chapterEntry == null) return;
            // at this point specialstoryobjects will be in allValidObjects

            var specialItemsToAdd = allValidObjects.Where(x => x.isSpecialStoryItem && chapterEntry.specialItems.Contains(x.name)).ToList();
            
            while (specialItemsToAdd.Count > 0)
            {
                // find some index where it's not a grouped item, not a key item, and not another special item
                //int i = currentObjects.FindIndex(x => !x.isSpecialStoryItem && !(x is HOKeyItem) && string.IsNullOrWhiteSpace(x.objectGroup));
                //if (i >= 0)
                //{
                //    allValidObjects.Add(currentObjects[i]);
                //    currentObjects[i] = specialItemsToAdd[0];
                //    allValidObjects.Remove(specialItemsToAdd[0]);
                //    specialItemsToAdd.RemoveAt(0);
                //    continue;
                //}

                //i = futureObjects.FindIndex(x => !x.isSpecialStoryItem && !(x is HOKeyItem) && string.IsNullOrWhiteSpace(x.objectGroup));
                //if (i >= 0)
                //{
                //    allValidObjects.Add(futureObjects[i]);
                //    futureObjects[i] = specialItemsToAdd[0];
                //    allValidObjects.Remove(specialItemsToAdd[0]);
                //    specialItemsToAdd.RemoveAt(0);
                //    continue;
                //}

                futureObjects.Add(specialItemsToAdd[0]);
                allValidObjects.Remove(specialItemsToAdd[0]);
                specialItemsToAdd.RemoveAt(0);
                continue;

                //Unreachable Code?
                //Debug.Log($"No valid place in list to put special story item {specialItemsToAdd[0].name}, total of {specialItemsToAdd.Count} remaining");
                //break;
            }
        }

        protected virtual List<HOFindableObject> GetAllSelectableObjects(ref List<HOFindableObject> allValidObjects)
        {
            List<HOFindableObject> selectableObjects = allValidObjects
                .Where(x => x.IsValidForLogic(this) && x.isSpecialStoryItem == false).ToList();

            allValidObjects.RemoveAll(x => x.IsValidForLogic(this) && x.isSpecialStoryItem == false);

            return selectableObjects;
        }

        protected virtual void SelectTriviaObject()
        {
            if(room.interactiveObjects.Any(x => x is HOTriviaObject))
            {
                Savegame.HORoomData roomData = HORoomDataHelper.instance.GetHORoomData(roomRef.AssetGUID);
                var triviaObjects = room.interactiveObjects.Where(x => x is HOTriviaObject).OrderBy(x => UnityEngine.Random.value).ToList();

                selectedTrivia = null;

                if(roomData.triviasFound.Count < triviaObjects.Count)
                {
                    selectedTrivia = triviaObjects.First(x => roomData.triviasFound.Contains(x.displayKey) == false);
                    triviaObjects.Remove(selectedTrivia);
                }

                //Debug.LogWarning(selectedTrivia);

                foreach (var item in triviaObjects)
                {
                    GameObject.Destroy(item.gameObject);
                }
            }
        }

        protected virtual void DestroyOccludedObjects()
        {
            //Destroy Out of Bound objects
            List<HOInteractiveObject> occludedObjects = GetOccludedObjects();
            foreach (var occludedObject in occludedObjects)
            {
                //Don't Destroy Door Items
                if (occludedObject is HODoorItem)
                {
                    Debug.LogWarning($"Door {occludedObject.name} is not fully visible!");
                    continue;
                }

                //Don't Destroy Key Items
                if (occludedObject is HOKeyItem)
                {
                    Debug.LogWarning($"Key {occludedObject.name} is not fully visible!");
                    continue;
                }

                //Don't Destroy Special Items
                if (occludedObject is HOFindableObject)
                {
                    HOFindableObject findable = occludedObject as HOFindableObject;

                    if (findable.isSpecialStoryItem)
                    {
                        Debug.LogWarning($"Special Item {occludedObject.name} is not fully visible!");
                        continue;
                    }
                }
                GameObject.Destroy(occludedObject.gameObject);
            }
        }

        protected virtual List<HOFindableObject> PreventObjectsFromDeletion(ref List<HOFindableObject> toDelete)
        {
            List<HOFindableObject> preservedInactive = toDelete.GetRange(0, Math.Min(toDelete.Count, inactiveItemsToShow));
            toDelete.RemoveRange(0, Math.Min(toDelete.Count, inactiveItemsToShow));
            return preservedInactive;
        }

        protected virtual void GroupSelectedObjectsToCurrentAndFuture(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
        {
            // we need to now:
            //  keep items with same group together
            //  ensure key items are on the first 'page' of items

            // we'll substitute a group marker into the list to represent the entire group
            var uniqueGroupMarkers = selectedObjectsList.Where(x => !string.IsNullOrEmpty(x.objectGroup))
                                                        .GroupBy(x => x.objectGroup)
                                                        .Select(x => x.First());

            // shuffle non-key, non-group items into the list (group items just their marker)
            currentObjects.AddRange(
                selectedObjectsList.Where(x => !(x is HOKeyItem) && (string.IsNullOrEmpty(x.objectGroup) || uniqueGroupMarkers.Contains(x)))
                                   .OrderBy(x => UnityEngine.Random.value)
            );

            // insert key items and group items to the front
            var keyItems = selectedObjectsList.Where(x => x is HOKeyItem);
            for (int i = 0; i < keyItems.Count(); i++)
            {
                // insert at a random position between 0 and N-(totalInsertSize) to ensure we never push a key item out
                int pos = Math.Min(maxItemsToShow, currentObjects.Count) - (keyItems.Count() + i);
                pos = UnityEngine.Random.Range(0, pos);
                currentObjects.Insert(pos, keyItems.ElementAt(i));
            }



            // replace our group markers with the full groups
            foreach (var gm in uniqueGroupMarkers)
            {
                int iidx = currentObjects.IndexOf(gm);
                currentObjects.RemoveAt(iidx);

                currentObjects.InsertRange(iidx, selectedObjectsList.Where(x => x.objectGroup.Equals(gm.objectGroup, StringComparison.OrdinalIgnoreCase)));
            }

            // do a count now, counting groups as 1 item, and figure out how many leftovers we have
            int ri = -1, rc = 1;
            for (int i = 1; i < currentObjects.Count; i++)
            {
                if (string.IsNullOrEmpty(currentObjects[i].objectGroup) ||
                    !currentObjects[i].objectGroup.Equals(currentObjects[i-1].objectGroup, StringComparison.OrdinalIgnoreCase))
                    rc++;

                if (rc == maxItemsToShow)
                {
                    // count forward

                    if (!string.IsNullOrEmpty(currentObjects[i].objectGroup))
                    {
                        while (i + 1 < currentObjects.Count &&
                               currentObjects[i + 1].objectGroup.Equals(currentObjects[i].objectGroup, StringComparison.OrdinalIgnoreCase))
                            i++;
                    }

                    ri = i;

                    break;
                }
            }

            // cull out the objects past ri marker if it's not the whole list
            if (ri > 0 && ri < currentObjects.Count - 1)
                currentObjects.RemoveRange(ri+1, currentObjects.Count - ri - 1);

            // remove what we've already processed from selectedObjects list, and start tacking on the rest to future objects
            selectedObjectsList.RemoveAll(x => currentObjects.Contains(x));

            // and just double check we have enough objects left in the pool....
            int futureCount = Math.Min(totalToFind - currentObjects.Count, (validObjectsList.Count + selectedObjectsList.Count));

            if (futureCount > 0 && selectedObjectsList.Count > 0)
            {
                futureObjects.AddRange(selectedObjectsList.GetRange(0, Math.Min(futureCount, selectedObjectsList.Count)));
                futureCount -= futureObjects.Count;
            }

            // if we STILL have items remaining.. we'll have to just pull some random shit from the scene
            if (futureCount > 0 && validObjectsList.Count > 0)
            {
                int takeCount = Math.Min(futureCount, validObjectsList.Count);
                futureObjects.AddRange(validObjectsList.GetRange(0, takeCount));
                validObjectsList.RemoveRange(0, takeCount);

                Debug.Log($"Had to steal {takeCount} from the validObjectsList - not good, check data.");
            }
        }

        // remove from valid objects, add to selected objects
        protected virtual void SelectObjectsFromValidObjects(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
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
            } else
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
        }

        // will: add keys if needed, add at least 1 selected object per open subHO if applicable, and add valid objects to the list
        protected virtual void HandleSubHO(HODoorHandler handler, ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
        {
            if (AllowSubHOs())
            {
                if ((unlockableSubHO.Contains(handler.subHO) && AllowKeyItems()) || unlockedSubHO.Contains(handler.subHO))
                {
                    HORoom subRoom = room.GetSubHOInstance(handler.subHO);

                    // we don't really care about subHO item difficulty, just ensure we take at least 1 item
                    var allFindables = subRoom.interactiveObjects.Where(x => x is HOFindableObject).Select(x => x as HOFindableObject).ToList();

                    if (allFindables.Count > 0)
                    {
                        // select at least one object that is valid for this logic
                        var subHOObjects = allFindables.Where(x => x.IsValidForLogic(this)).OrderBy(x => { return UnityEngine.Random.value; }).ToList();

                        if (subHOObjects.Count > 0)
                        {
                            var forceSelectObj = subHOObjects.First();
                            itemCountPerDifficulty[(int)forceSelectObj.difficultyLevel]--;

                            allFindables.Remove(forceSelectObj);
                            selectedObjectsList.Add(forceSelectObj);

                            //  if not unlocked already, ensure the key item is also in the selected items
                            if (unlockableSubHO.Contains(handler.subHO))
                            {
                                // and this doesn't "count" as an item accdng to ho3
                                if (handler.keyItem)
                                {
                                    //Prevent key items from being added to item list
                                    //This is for key drag & drop mechanic
                                    //selectedObjectsList.Add(handler.keyItem);
                                    //GameObject.Destroy(handler.keyItem.gameObject);
                                }
                                    

                                handler.SetOpen(false);
                            } else
                            {
                                // otherwise destroy it
                                if (handler.keyItem)
                                    GameObject.Destroy(handler.keyItem.gameObject);
                                handler.SetOpen(true);
                            }

                            validObjectsList.AddRange(allFindables);

                            return;
                        }
                    }
                }
            }

        
            // delete door items, don't show key items
            if (handler.keyItem)
                GameObject.Destroy(handler.keyItem.gameObject);
            if (handler.closedState)
                GameObject.Destroy(handler.closedState.gameObject);
            if (handler.openState)
                GameObject.Destroy(handler.openState.gameObject);
        }

        protected virtual void HandleSubHOFindables(ref List<HOFindableObject> validObjectsList, ref List<HOFindableObject> selectedObjectsList)
        {
            foreach (var door in room.doorHandlers)
            {
                if(door.isValid)
                    HandleSubHO(door, ref validObjectsList, ref selectedObjectsList);
            }
        }

        // take some items by difficulty, while prioritizing already-selected group items
        protected static IEnumerable<HOFindableObject> SelectNByDifficulty(int n, HODifficulty difficulty, List<HOFindableObject> fromList, bool removeFromSourceList, List<string> selectedGroupsToWeight)
        {
            const float alreadyGroupedWeight = 0.3f;
            // to make sure selectedGroupsToWeight is accounted for we need to reshuffle each select...
            // -- obviously, this is very slow code, but since it's one-time, on-load, it is ok in this instance
            var difficultyMatch = fromList.Where(x => x.difficultyLevel == difficulty && x.isSpecialStoryItem == false).OrderBy(x => UnityEngine.Random.value);
            int selectCount = Math.Min(n, difficultyMatch.Count());
            List<HOFindableObject> selected = new List<HOFindableObject>(selectCount);
                        
            while (selected.Count < selectCount)
            {
                var toAdd = difficultyMatch.First();
                selected.Add(toAdd);

                if (removeFromSourceList)
                    fromList.Remove(toAdd);

                if (!string.IsNullOrEmpty(toAdd.objectGroup) && !selectedGroupsToWeight.Contains(toAdd.objectGroup))
                {
                    selectedGroupsToWeight.Add(toAdd.objectGroup);
                }

                difficultyMatch = fromList.Where(x => x.difficultyLevel == difficulty && !selected.Contains(x))
                                          .OrderBy(x => UnityEngine.Random.value - (selectedGroupsToWeight.Contains(x.objectGroup) ? alreadyGroupedWeight : 0f));
            }
        
            return selected;
        }

        protected static IEnumerable<HOFindableObject> SelectN(int n, List<HOFindableObject> fromList, bool removeFromSourceList, List<string> selectedGroupsToWeight)
        {
            const float alreadyGroupedWeight = 0.3f;
            // to make sure selectedGroupsToWeight is accounted for we need to reshuffle each select...
            // -- obviously, this is very slow code, but since it's one-time, on-load, it is ok in this instance
            var randomList = fromList.Where(x => !x.isSpecialStoryItem).OrderBy(x => UnityEngine.Random.value);
            int selectCount = Math.Min(n, randomList.Count());
            List<HOFindableObject> selected = new List<HOFindableObject>(selectCount);

            while (selected.Count < selectCount)
            {
                var toAdd = randomList.First();
                selected.Add(toAdd);

                if (removeFromSourceList)
                    fromList.Remove(toAdd);

                if (!string.IsNullOrEmpty(toAdd.objectGroup) && !selectedGroupsToWeight.Contains(toAdd.objectGroup))
                {
                    selectedGroupsToWeight.Add(toAdd.objectGroup);
                }

                randomList = fromList.Where(x => !selected.Contains(x))
                                          .OrderBy(x => UnityEngine.Random.value - (selectedGroupsToWeight.Contains(x.objectGroup) ? alreadyGroupedWeight : 0f));
            }

            return selected;
        }
    }
}

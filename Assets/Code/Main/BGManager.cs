using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;
using System.Linq;
using Boomzap.Conversation;
using UnityEngine.Events;
using UnityEditor;

namespace ho
{
    public class BGManager : SimpleSingleton<BGManager>
    {
        [SerializeField] protected AssetReferenceSprite[] bgAssets = new AssetReferenceSprite[0];

        public string[] bgAssetNames => GetBGAssetNames();

        [ReadOnly]
        public List<string> loadedBGNames = new List<string>();

        [ReadOnly]
        public List<HORoom> loadedBG = new List<HORoom>();

        GameObject activeBG = null;


        Vector3 cameraPos = Vector3.zero;

        string firstConversationBG;
        string[] GetBGAssetNames()
        {
            var assets = bgAssets.Select(x => x.SubObjectName).ToArray();
            string[] newBGAssets = new string[assets.Length + 1];

            newBGAssets[0] = "Default";
            for(int i = 1; i < newBGAssets.Length; i++)
            {
                newBGAssets[i] = assets[i - 1];
            }

            return newBGAssets;
        }


        public void DisableAllBackground()
        {
            activeBG = null;
            //loadedBG.ForEach(x => Debug.Log(x.name));
            loadedBG.ForEach(x => x.gameObject.SetActive(false));
        }

        public void MoveBGtoCamera(GameObject bgObject)
        {
            bgObject.transform.position = cameraPos;
            bgObject.gameObject.SetActive(true);
        }

        public void LoadBackground(string bgName)
        {
            if (string.IsNullOrWhiteSpace(bgName))
            {
                Debug.LogError($"Bg loaded is null!");
                return;
            }

            bgName = bgName.ToLower();

            if (loadedBGNames.Contains(bgName) == false)
            {
                //DevDebug.Log($"{bgName} not found!");
                return;
            }

            if(activeBG)
            {
                //Check if loading same BG;
                if (activeBG.name.StartsWith(bgName))
                {
                    //Debug.LogError($"Active BG {bgName} is already playing!");
                    return;
                }

                activeBG.gameObject.SetActive(false);
                activeBG = null;
            }

            if(loadedBG.Any(x => x.name.StartsWith(bgName, System.StringComparison.OrdinalIgnoreCase)))
            {
                activeBG = loadedBG.FirstOrDefault(x => x.name.StartsWith(bgName, System.StringComparison.OrdinalIgnoreCase)).gameObject;

                MoveBGtoCamera(activeBG);
                //Debug.LogError($"Loading active bg {bgName}...");
            }
            else
            {
                //Debug.LogError($"Bg with name {bgName} not found!");
            }
        }

        //IEnumerator UnloadBGCor(UnityAction andThen = null)
        //{
        //    //foreach (HORoom room in loadedBG)
        //    //{
        //    //    HORoomReference roomRef = HORoomAssetManager.instance.roomTracker.GetItemByName(room.name);

        //    //    Destroy(room.gameObject);
        //    //    if (roomRef != null)
        //    //    {
        //    //        var status = HORoomAssetManager.instance.GetRoo(roomRef);

        //    //        if (status.isLoaded && status.loadOperationHandle.IsValid())
        //    //        {
        //    //            status.isLoaded = false;
        //    //            Addressables.Release(status.loadOperationHandle);

        //    //            yield return status.loadOperationHandle;
        //    //        }
        //    //    }
        //    //}

        //    loadedBGNames.Clear();
        //    loadedBG.Clear();

        //    andThen?.Invoke();
        //}

        public void UnloadBGs(UnityAction andThen = null)
        {
            activeBG = null;

            foreach (HORoom room in loadedBG)
            {
                HORoomReference roomRef = HORoomAssetManager.instance.roomTracker.GetItemByName(room.name);

                if(roomRef != null)
                    HORoomAssetManager.instance.UnloadRoom(roomRef);
            }

            loadedBG.ForEach(x => Destroy(x.gameObject));

            //StartCoroutine(UnloadBGCor(andThen));
            loadedBGNames.Clear();
            loadedBG.Clear();

            andThen?.Invoke();
        }

        void LoadBackgroundRooms()
        {
            foreach (var bgName in loadedBGNames)
            {
                if (string.IsNullOrWhiteSpace(bgName)) continue;

                HORoomReference roomRef = HORoomAssetManager.instance.roomTracker.GetItemByName(bgName);

                if (roomRef == null)
                {
                    Debug.LogWarning($"bg name with {bgName} not found");
                    continue;
                }

                var av = HORoomAssetManager.instance.GetRoomAssetStatus(roomRef);

                HORoomAssetManager.instance.LoadMainRoomAsync(roomRef, (GameObject go) =>
                {
                    if (go)
                    {
                        if (av.isLoaded && av.room != null)
                        {
                            HORoom roomRef = Instantiate(av.room);
                            roomRef.SetupForDisplay();

                            loadedBG.Add(roomRef);

                            if(roomRef == null)
                            {
                                Debug.Log($"Room Ref with bg name {bgName} not Instantiated");
                                return;
                            }

                            bool isFirstConversation = roomRef.name.StartsWith(firstConversationBG);
                            if (isFirstConversation)
                                MoveBGtoCamera(roomRef.gameObject);
                            else
                                roomRef.gameObject.SetActive(false);
                        }
                    }
                });
            }
        }



        void SetupBGFromConversation(Conversation conversation, bool forcePlay = false)
        {
            if (conversation == null || (GameController.save.IsConversationTriggered(conversation.guid) && !conversation.repeatable))
            {
                if(forcePlay == false)
                    return;
            }

            var backgroundsToLoad = conversation.GetAllNodes().Select(x => x.background).Where(y => loadedBGNames.Contains(y) == false).Distinct().ToList();

            //Debug.Log(backgroundsToLoad.Count);

            backgroundsToLoad.ForEach(x => loadedBGNames.Add(x));
            //Debug.Log(loadedBGNames.Count);
        }

        public void PreloadBackgrounds(Chapter.Entry entry, bool fadeToHo = false)
        {
            cameraPos = fadeToHo ? HOGameController.instance.transform.position : MinigameController.instance.transform.position;
            cameraPos.z = -1;

            loadedBGNames.Clear();
            loadedBG.Clear();

            if(entry.onStartConversation == null && entry.onEndConversation == null)
            {
                //No Need to load backgrounds
                return;
            }

            SetupBGFromConversation(entry.onStartConversation);
            SetupBGFromConversation(entry.onEndConversation);

            if (loadedBGNames.Contains("Default"))
                loadedBGNames.Remove("Default");

            if (entry.IsHOScene && loadedBGNames.Contains(entry.hoRoom.roomName))
            {
                loadedBGNames.Remove(entry.hoRoom.roomName);
                Debug.Log($"Removed {entry.hoRoom.roomName} in to load list");
            }
                

            firstConversationBG = "Default";

            if (entry.onStartConversation != null)
            {
                firstConversationBG = entry.onStartConversation.GetAllNodes().Select(x => x.background).FirstOrDefault();
                Debug.Log($"First Conversation: {firstConversationBG}");
            }

            LoadBackgroundRooms();
        }

        public void PreloadBackgrounds(Conversation conversation)
        {
            cameraPos = GameController.instance.defaultCamera.transform.position;
            cameraPos.z = -1;

            loadedBGNames.Clear();
            loadedBG.Clear();

            SetupBGFromConversation(conversation, true);

            var nextNode = conversation.GetAllNodes()[0];

            firstConversationBG = "Default";

            if (nextNode != null)
            {
                firstConversationBG = nextNode.background;
                Debug.Log($"First Conversation: {firstConversationBG}");
            }

            LoadBackgroundRooms();
        }
    }
}

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

namespace ho
{
    public class HORoomAssetManager : SimpleSingleton<HORoomAssetManager>
    {
        public HORoomTracker    roomTracker;
        public MinigameTracker  mgTracker;
        
        public delegate         void OnDownloadCompleteCallback(bool success);
        public delegate         void OnLoadCompleteCallback(GameObject obj);

        public class AssetStatus
        {
            public bool         isLocal = true;
            public bool         isValid = true;
            public long         downloadSize = 0;

            public bool         isDownloading = false;
            public bool         isLoading = false;
            public bool         isLoaded = false;

            public string       roomName;

            public int          childrenLoading = 0;

            public AsyncOperationHandle             downloadOperationHandle;
            public AsyncOperationHandle<GameObject> loadOperationHandle;

            public OnLoadCompleteCallback          onLoadComplete = null;
            public OnDownloadCompleteCallback      onDownloadComplete = null;
            
            public HORoom room 
            {
                get
                {
                    if (isLoaded)
                        return loadOperationHandle.Result.GetComponent<HORoom>();

                    return null;
                }
            }

            public float GetDownloadProgress()
            {
                if (isDownloading)
                    return downloadOperationHandle.GetDownloadStatus().Percent;
                if (isLocal)
                    return 1f;

                return 0f;
            }

            public float GetLoadProgress()
            {
                if (isLoading)
                {
                    if (loadOperationHandle.GetDownloadStatus().IsDone || isLocal)
                        return (loadOperationHandle.PercentComplete - 0.5f) * 2f;

                    return loadOperationHandle.GetDownloadStatus().Percent;
                }
                if (isLoaded)
                    return 1f;

                return 0f;
            }
        }

        public Dictionary<string, AssetStatus>  roomAssetStatus { get; private set; } = new Dictionary<string, AssetStatus>();
        int                                     pendingQueries = 0;
        
        public bool IsReady()
        {
            return pendingQueries == 0 && roomAssetStatus.Count > 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (var roomRef in roomTracker.roomEntries)
            {
                var av = GetRoomAssetStatus(roomRef.roomReference.AssetGUID);

                av.isLocal = true;
                av.isValid = true;
                av.downloadSize = 0;
                roomTracker.GetNameFromGUID(roomRef.roomReference.AssetGUID, out av.roomName);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void QueryAllRoomAvailability()
        {
            foreach(var roomRef in roomTracker.roomEntries)
            {
                pendingQueries++;

                CheckDownloadSizeAsync(roomRef.roomReference, (AsyncOperationHandle<long> handle) => { var id = roomRef.roomReference.AssetGUID; OnQueryComplete(handle, id); });
            }
        }

        void OnQueryComplete(AsyncOperationHandle<long> handle, string id)
        {
            var av = GetRoomAssetStatus(id);

            av.isLocal = handle.Result <= 0;
            av.isValid = handle.Status == AsyncOperationStatus.Succeeded;
            av.downloadSize = handle.Result;
            roomTracker.GetNameFromGUID(id, out av.roomName);
            
            pendingQueries--;
        }
        
        public void UnloadRoom(HORoomReference roomRef)
        {
            var status = GetRoomAssetStatus(roomRef);

            if (status.isLoaded && status.loadOperationHandle.IsValid())
            {
                status.isLoaded = false;
                Addressables.Release(status.loadOperationHandle);
            }
        }

        public float GetLoadProgress(HORoomReference roomRef)
        {
            var status = GetRoomAssetStatus(roomRef);

            if (status.isLoading)
            {
                return status.loadOperationHandle.PercentComplete;
            } else if (status.isLoaded)
            {
                return 1f;
            }

            return 0f;
        }

        void OnRoomLoaded(AsyncOperationHandle<GameObject> goHandle, AssetStatus status)
        {
            Debug.Log("OnRoomLoaded status for " + status.roomName + ": " + goHandle.Status);

            bool isOK = goHandle.Status == AsyncOperationStatus.Succeeded;

            status.isLoading = false;

            if (isOK)
            {
                //loadedRoomObj = Instantiate(goHandle.Result, gameObject.transform);
                //loadedRoom = loadedRoomObj.GetComponent<Room>();
                status.isLoaded = true;
                status.isLocal = true;

                if (status.room.subHO.Length > 0)
                {
                    status.childrenLoading += status.room.subHO.Length;
                    
                    foreach (var subHO in status.room.subHO)
                    {
                        var subStatus = GetRoomAssetStatus(subHO);
                        if (subStatus.isLoaded || subStatus.isLoading || subStatus.isDownloading)
                        {
                            status.childrenLoading--;
                            Debug.LogWarning("Child already loading in subHO load");
                            continue;
                        }

                        LoadRoomAsync(subHO, (GameObject room) =>
                        {
                            var capture = status;
                           
                            status.childrenLoading--;

                            if (status.childrenLoading <= 0)
                            {
                                status.onLoadComplete?.Invoke(goHandle.Result);
                                status.onLoadComplete = null;
                            }
                        });
                    }

                    if (status.childrenLoading == 0)
                    {
                        Debug.LogWarning("No children loading when there should be--");
                        status.onLoadComplete?.Invoke(goHandle.Result);
                        status.onLoadComplete = null;
                    }

                } else
                {
                    status.onLoadComplete?.Invoke(goHandle.Result);
                    status.onLoadComplete = null;
                }
            } else
            {
                OnRoomFailedToLoad(status);
            }
        }

        void OnRoomFailedToLoad(AssetStatus status)
        {
            status.isLoaded = false;
            status.isValid = false;

            Addressables.Release(status.loadOperationHandle);

            status.onLoadComplete?.Invoke(null);
            status.onLoadComplete = null;
        }

        void OnRoomDownloaded(AsyncOperationHandle goHandle, AssetStatus status)
        {
            Debug.Log("OnRoomLoaded status for " + status.roomName + ": " + goHandle.Status);

            bool isOK = goHandle.Status == AsyncOperationStatus.Succeeded;

            status.isDownloading = false;

            if (isOK)
            {
                status.isLocal = true;

                status.onDownloadComplete?.Invoke(true);
                status.onDownloadComplete = null;
            } else
            {
                OnRoomFailedToDownload(status);
            }

            Addressables.Release(goHandle);
        }

        void OnRoomFailedToDownload(AssetStatus status)
        {
            status.isLoaded = false;
            status.isValid = false;

            status.onDownloadComplete?.Invoke(false);
            status.onDownloadComplete = null;
        }

        public AssetStatus GetRoomAssetStatus(string roomGUID)
        {
            AssetStatus av = null;

            if (roomAssetStatus.ContainsKey(roomGUID))
            {
                return roomAssetStatus[roomGUID];
            }
            
            av = new AssetStatus();
            roomAssetStatus[roomGUID] = av;

//             if (pendingQueries <= 0)
//                 Debug.LogWarning("Requested room asset status for room guid " + roomGUID + " which is not valid");

            return av;
        }

        public List<Chapter.Entry> GetRoomReferences()
        {
            List<Chapter.Entry> roomReferences = new List<Chapter.Entry>();
            foreach (var chapter in GameController.instance.gameChapters)
            {
                foreach (var sceneEntry in chapter.sceneEntries)
                {
                    //Check if scene entry is already added to unlimited mode
                    if (sceneEntry.IsHOScene)
                    {
                        if (sceneEntry.hoRoom == null) continue;

                        string hoRoomName = sceneEntry.hoRoom.roomName.ToLower();

                        if (hoRoomName.Contains("ho") == false) continue;

                        //Add Room Reference only to Unique Rooms
                        if (roomReferences.Any(x => x.hoRoom.roomName.ToLower() == hoRoomName) == false)
                        {
                            roomReferences.Add(sceneEntry);
                        }
                    }
                }
            }

            return roomReferences;
        }


        public AssetStatus GetRoomAssetStatus(HORoomReference roomRef)
        {
            return GetRoomAssetStatus(roomRef.AssetGUID);
        }

        public void CheckDownloadSizeAsync(HORoomReference roomRef, UnityAction<AsyncOperationHandle<long>> result)
        {
            var sizeQueryHandle = Addressables.GetDownloadSizeAsync(roomRef.RuntimeKey);
            
            sizeQueryHandle.Completed += (AsyncOperationHandle<long> handle) =>
            {
                result?.Invoke(handle);
                Addressables.Release(handle);
            };
        }

        public void LoadRoomAsync(HORoomReference roomRef, OnLoadCompleteCallback onComplete)
        {
            var status = GetRoomAssetStatus(roomRef);

            if (status.isLoaded)
            {
                onComplete?.Invoke(status.loadOperationHandle.Result);
            } else if (status.isLoading)
            {
                status.onLoadComplete += onComplete;
            } else if (status.isDownloading)
            {
                // add operation chain?
                Debug.LogWarning("Load asset requested when still downloading");
            } else if (!status.isValid)
            {
                onComplete?.Invoke(null);
            } else
            {
                status.loadOperationHandle = Addressables.LoadAssetAsync<GameObject>(roomRef.RuntimeKey);
                status.isLoading = true;
                status.onLoadComplete += onComplete;

                Debug.Log("Begin loading room: " + status.roomName);

                status.loadOperationHandle.Completed += (AsyncOperationHandle<GameObject> goHandle) => 
                {
                    var capture = status;
                    OnRoomLoaded(goHandle, capture);
                };
            }
        }

        public void LoadMainRoomAsync(HORoomReference roomRef, OnLoadCompleteCallback onComplete)
        {
            var status = GetRoomAssetStatus(roomRef);

            if (status.isLoaded)
            {
                onComplete?.Invoke(status.loadOperationHandle.Result);
            }
            else if (status.isLoading)
            {
                status.onLoadComplete += onComplete;
            }
            else if (status.isDownloading)
            {
                // add operation chain?
                Debug.LogWarning("Load asset requested when still downloading");
            }
            else if (!status.isValid)
            {
                onComplete?.Invoke(null);
            }
            else
            {
                status.loadOperationHandle = Addressables.LoadAssetAsync<GameObject>(roomRef.RuntimeKey);
                status.isLoading = true;
                status.onLoadComplete += onComplete;

                Debug.Log("Begin loading room: " + status.roomName);

                status.loadOperationHandle.Completed += (AsyncOperationHandle<GameObject> goHandle) =>
                {
                    var capture = status;
                    status.isLoading = false;

                    bool isOK = goHandle.Status == AsyncOperationStatus.Succeeded;

                    status.isLoading = false;

                    if (isOK)
                    {
                        //loadedRoomObj = Instantiate(goHandle.Result, gameObject.transform);
                        //loadedRoom = loadedRoomObj.GetComponent<Room>();
                        status.isLoaded = true;
                        status.isLocal = true;

                        status.onLoadComplete?.Invoke(goHandle.Result);
                        status.onLoadComplete = null;
                    }

                    };
            }
        }


        public void DownloadRoomAsync(HORoomReference roomRef, OnDownloadCompleteCallback onComplete)
        {
            var status = GetRoomAssetStatus(roomRef);

            if (status.isLoaded)
            {
                onComplete?.Invoke(true);
            } else if (status.isDownloading)
            {
                status.onDownloadComplete += onComplete;
            } else if (!status.isValid)
            {
                onComplete?.Invoke(false);
            } else
            {
                status.downloadOperationHandle = Addressables.DownloadDependenciesAsync(roomRef.RuntimeKey, false);
                status.isDownloading = true;
                status.onDownloadComplete += onComplete;

                Debug.Log("Begin downloading room: " + status.roomName);

                status.downloadOperationHandle.Completed += (AsyncOperationHandle goHandle) => 
                {
                    var capture = status;
                    OnRoomDownloaded(goHandle, capture);
                };
            }
        }

    }
}
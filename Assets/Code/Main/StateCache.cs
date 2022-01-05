using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ho
{
    public partial class StateCache : SimpleSingleton<StateCache>
    {
        [SerializeField]
        AssetReferenceGameObject    mainMenuRef;
        [SerializeField]
        AssetReferenceGameObject    chapterViewRef;
        //[SerializeField]
        //AssetReferenceGameObject    mapRef;

        AsyncOperationHandle<GameObject>    mainMenuLoader;
        AsyncOperationHandle<GameObject>    chapterLoader;
        //AsyncOperationHandle<GameObject>    mapLoader;

        UnityAction onAllTasksComplete;
        int tasksPending = 0;

        // these objects needs to be instantiated, not used raw
        public GameObject           MainMenu { get { if (mainMenuLoader.IsValid() && mainMenuLoader.IsDone) return mainMenuLoader.Result; return null; } }
        public GameObject           ChapterScreen { get { if (chapterLoader.IsValid() && chapterLoader.IsDone) return chapterLoader.Result; return null; } }
        //public GameObject           MapScreen { get { if (mapLoader.IsValid() && mapLoader.IsDone) return mapLoader.Result; return null; } }

        public float                LoadProgress()
        {
            // only time we'd use this is during the splash - so we report load progress of all
            //return (mainMenuLoader.PercentComplete + chapterLoader.PercentComplete + mapLoader.PercentComplete) / 3f;
            return (mainMenuLoader.PercentComplete + chapterLoader.PercentComplete) / 2f;
        }

        void OnAssetLoaded(AsyncOperationHandle<GameObject> handle)
        {
            tasksPending--;

            if (tasksPending <= 0)
            {
                onAllTasksComplete?.Invoke();
                onAllTasksComplete = null;
                tasksPending = 0;
            }


            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Loaded {handle.Result.name}");
            } else
            {
                Debug.LogError($"Failed to load {handle.DebugName}");
            }
        }

        void PreloadAsset(AssetReferenceGameObject reference, ref AsyncOperationHandle<GameObject> handle, UnityAction onComplete)
        {
            if (handle.IsValid())
            {
                if (handle.IsDone)
                {
                    onComplete?.Invoke();
                    return;
                }
            } else
            {
                handle = reference.LoadAssetAsync();
                handle.Completed += OnAssetLoaded;
            }

            if (onComplete != null)
                handle.Completed += (AsyncOperationHandle<GameObject> handle) => { onComplete.Invoke(); };

            tasksPending++;
        }
                
        public void PreloadChapterScreen(UnityAction onComplete = null)
        {
            PreloadAsset(chapterViewRef, ref chapterLoader, onComplete);
        }

        public void PreloadMapScreen(UnityAction onComplete = null)
        {
            //PreloadAsset(mapRef, ref mapLoader, onComplete);
        }

        public void PreloadMainMenu(UnityAction onComplete = null)
        {
            PreloadAsset(mainMenuRef, ref mainMenuLoader, onComplete);
        }

        void UnloadAsset(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid())
            {
                Debug.Log($"Unloading {handle.Result.name}");
                Addressables.Release(handle);
            }
        }

        public void UnloadMapScreen()
        {
            //UnloadAsset(mapLoader);
        }

        public void UnloadMainMenu()
        {
            UnloadAsset(mainMenuLoader);
        }

        public void UnloadChapterScreen()
        {
            UnloadAsset(chapterLoader);
        }

        public void UnloadAll()
        {
            UnloadChapterScreen();
            UnloadMainMenu();
            UnloadMapScreen();
        }

        public void PreloadAll(UnityAction onComplete = null)
        {
            onAllTasksComplete = onComplete;
            PreloadChapterScreen();
            PreloadMainMenu();
            PreloadMapScreen();

            if (tasksPending <= 0)
            {
                tasksPending = 0;
                onAllTasksComplete?.Invoke();
                onAllTasksComplete = null;
            }
        }

        public void LoadAssetTexture(AssetReference assetReference, ref AsyncOperationHandle<Sprite> handle, UnityEngine.UI.Image imageContainer, SpriteRenderer bg)
        {
            handle = Addressables.LoadAssetAsync<Sprite>(assetReference);

            if (handle.IsValid())
            {
                handle.Completed += (AsyncOperationHandle<Sprite> handle) =>
                {
                    if (imageContainer)
                        imageContainer.sprite = handle.Result;

                    if (bg)
                        bg.sprite = handle.Result;

                    //Debug.Log($"Loaded New Asset: { imageContainer.sprite.name}");

                };
            }
        }

        public void LoadWallPaper(string wallpaperRef, ref AsyncOperationHandle<Sprite> handle, SpriteRenderer bg)
        {
            Debug.Log("Loading " + wallpaperRef);
            //AssetReference wallpaper = Popup.GetPopup<WallpaperUI>().GetWallpaper(wallpaperRef);

            //if (wallpaper != null)
            //{
            //    handle = Addressables.LoadAssetAsync<Sprite>(wallpaper);

            //    if (handle.IsValid())
            //    {
            //        handle.Completed += (AsyncOperationHandle<Sprite> handle) =>
            //        {
            //            if (bg)
            //                bg.sprite = handle.Result;
            //        };
            //    }
            //}
        }
    }
}

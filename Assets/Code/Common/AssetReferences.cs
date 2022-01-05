using UnityEngine.AddressableAssets;
using UnityEngine;
using Unity;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace ho
{
    [Serializable]
    public class NoLoadReference : AssetReferenceGameObject
    {
        public NoLoadReference(string guid)
            : base(guid)
        {
            
        }
               
        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override AsyncOperationHandle<GameObject> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)

        {
            return default;
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override AsyncOperationHandle<GameObject> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return default;
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override AsyncOperationHandle<GameObject> LoadAssetAsync()
        {
            return default;
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override AsyncOperationHandle<TObject> LoadAssetAsync<TObject>()
        {
            return default;
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override AsyncOperationHandle<SceneInstance> LoadSceneAsync(LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return default;
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override void ReleaseAsset()
        {
            
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override void ReleaseInstance(GameObject obj)
        {
            
        }

        [Obsolete("Use the appropriate asset manager to do this. Reference handles serve only to be a handle, nothing more.", true)]
        public override AsyncOperationHandle<SceneInstance> UnLoadScene()
        {
            return default;
        }
    }

    [Serializable]
    public class HORoomReference : NoLoadReference
    {
        public Sprite           roomPreviewSprite => HORoomAssetManager.instance.roomTracker.GetPreviewSprite(this);

        public string           roomLocalizationKey;
        public string           roomName;

        public HORoomAssetManager.AssetStatus assetStatus
        {
            get
            {
                return HORoomAssetManager.instance.GetRoomAssetStatus(base.AssetGUID);
            }
        }

        public HORoom roomPrefab
        {
            get
            {
                return assetStatus.room;
            }
        }

        public HORoomReference(string guid)
            : base(guid)
        {
        
        }

        public static bool operator ==(HORoomReference a, HORoomReference b)
        {
            return System.Object.Equals(a,b);
        }

        public static bool operator !=(HORoomReference a, HORoomReference b)
        {
            return !System.Object.Equals(a,b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is HORoomReference)
            {
                return AssetGUID.Equals((obj as HORoomReference).AssetGUID, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        #if UNITY_EDITOR
        public override bool SetEditorAsset(UnityEngine.Object value)
        {
            if (base.SetEditorAsset(value))
            {
                if (value == null)
                {
                    roomLocalizationKey = "";
                    roomName = "";
                    return true;
                }

                GameObject go = value as GameObject;

                roomLocalizationKey = HOUtil.GetRoomLocalizedName(go.name);
                roomName = go.name;

                return true;
            }

            return false;
        }
        #endif

        public override bool ValidateAsset(UnityEngine.Object obj)
        {
            var type = obj.GetType();
            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;    
                return go.GetComponent<HORoom>() != null;
            }

            return false;
        }

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) return false;

            return ValidateAsset(obj);
#else
            return false;
#endif
        }
    }

    [Serializable]
    public class MinigameReference : AssetReferenceGameObject
    {
        public Sprite roomPreviewSprite => HORoomAssetManager.instance.mgTracker.GetPreviewSprite(this);

        public string roomNameKey;

        public string roomDescKey;
        public MinigameReference(string guid)
            : base(guid)
        {
        
        }

        public static bool operator ==(MinigameReference a, MinigameReference b)
        {
            return System.Object.Equals(a,b);
        }

        public static bool operator !=(MinigameReference a, MinigameReference b)
        {
            return !System.Object.Equals(a,b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MinigameReference)
            {
                return AssetGUID.Equals((obj as MinigameReference).AssetGUID, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override bool ValidateAsset(UnityEngine.Object obj)
        {
            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;    
                return go.GetComponent<MinigameBase>() != null;
            }

            return false;
        }

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) return false;

            return ValidateAsset(obj);
#else
            return false;
#endif
        }
    }

}



#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
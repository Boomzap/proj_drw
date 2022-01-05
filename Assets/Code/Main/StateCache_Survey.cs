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
        AssetReferenceGameObject surveyRef;

        AsyncOperationHandle<GameObject> surveyLoader;
        
        public GameObject SurveyEnd { get { if (surveyLoader.IsValid() && surveyLoader.IsDone) return surveyLoader.Result; return null; } }

        public void PreloadSurveyEnd(UnityAction onComplete = null)
        {
            PreloadAsset(surveyRef, ref surveyLoader, onComplete);
        }

        public void UnloadSurveyEnd()
        {
            UnloadAsset(surveyLoader);
        }

    }
}

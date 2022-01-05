using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace ho.Build
{
    [CreateAssetMenu(fileName = "BuildSettings.asset", menuName = "Boomzap/BuildSettings")]
    public class BuildSettings : ScriptableObject
    {
        public AddressableAssetGroup[] includeInSurvey = new AddressableAssetGroup[0];
        public AddressableAssetGroup[] includeInCE = new AddressableAssetGroup[0];
        public AddressableAssetGroup[] includeInSE = new AddressableAssetGroup[0];

        public enum BuildSwitch
        {
            StandardEdition,
            CollectorsEdition,
            Survey
        };

        public static BuildSettings Get()
        {
            return AssetDatabase.LoadAssetAtPath<BuildSettings>("Assets/Editor/BuildScripts/BuildSettings.asset");
        }

        public void SetGroups(BuildSwitch mode)
        {
            var allGroups = includeInCE.Union(includeInSE).Union(includeInSurvey);
            foreach (var g in allGroups)
            {
                g.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = false;
            }

            AddressableAssetGroup[] grps = null;
            switch (mode)
            {
                case BuildSwitch.StandardEdition:
                    grps = includeInSE;
                    break;
                case BuildSwitch.CollectorsEdition:
                    grps = includeInCE;
                    break;
                case BuildSwitch.Survey:
                    grps = includeInSurvey;
                    break;
            }

            foreach (var g in grps)
            {
                g.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = true;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor;
using UnityEngine;

namespace ho.Build
{
    public class Versioning
    {
        [MenuItem("Tools/Increase Version Minor")]
        static void IncreaseVersionMinor()
        {
            string[] current = PlayerSettings.bundleVersion.Split('.');

            int major = int.Parse(current[0]);
            int minor = int.Parse(current[1]);

            minor++;

            PlayerSettings.bundleVersion = $"{major}.{minor}";

            PlayerSettings.Android.bundleVersionCode = 10000 + major * 1000 + minor;
            PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Increase Version Major")]
        static void IncreaseVersionMajor()
        {
            string[] current = PlayerSettings.bundleVersion.Split('.');

            int major = int.Parse(current[0]);
            int minor = int.Parse(current[1]);

            major++;
            minor = 0;

            PlayerSettings.bundleVersion = $"{major}.{minor}";

            PlayerSettings.Android.bundleVersionCode = 10000 + major * 1000 + minor;
            PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();

            AssetDatabase.SaveAssets();
        }
    }
}

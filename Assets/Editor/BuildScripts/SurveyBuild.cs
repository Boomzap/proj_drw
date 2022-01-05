using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.AddressableAssets;
using UnityEditor;
using UnityEngine;

using UnityEditor.Build.Reporting;

using Unity;

namespace ho.Build
{
    class Survey
    {
        const string SEBuildName = "FaircroftsAntiques4.exe";
        
        const string SEBuildNameOSX = "FaircroftsAntiques4.app";
        
        [MenuItem("Tools/Build Survey 32-bit")]
        public static void BuildSurvey32()
        {
            BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.Survey);
            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.SURVEY_BUILD, typeof(BuildScriptPackedMode),
                "survey", SEBuildName, false, false, true, new Vendor[] { Vendor.Bigfish } );
        }

        [MenuItem("Tools/Build Survey 32-bit Dev")]
        public static void BuildSurvey32Dev()
        {
            BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.Survey);
            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.SURVEY_BUILD, typeof(BuildScriptPackedMode),
                "survey_dev", SEBuildName, true, false, true, new Vendor[] { Vendor.Bigfish });
        }

        public static void BuildSurveyAll()
        {
            BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.Survey);
            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.SURVEY_BUILD, typeof(BuildScriptPackedMode),
                "survey_dev", "DEV " + SEBuildName, true, false, true, new Vendor[] { Vendor.Bigfish });
            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.SURVEY_BUILD, typeof(BuildScriptPackedMode),
                "survey", SEBuildName, false, false, true, new Vendor[] { Vendor.Bigfish });

            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.SURVEY_BUILD, typeof(BuildScriptPackedMode),
                "survey_dev", "DEV " + SEBuildNameOSX, true, false, true, new Vendor[] { Vendor.Bigfish });
            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.SURVEY_BUILD, typeof(BuildScriptPackedMode),
                "survey", SEBuildNameOSX, false, false, true, new Vendor[] { Vendor.Bigfish });
        }
    }
}

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
    class Release
    {
        const string SEBuildName = "FaircroftsAntiques5.exe";
        const string SEBuildNameOSX = "FaircroftsAntiques5.app";

        const string CEBuildName = "FaircroftsAntiques5.exe";
        const string CEBuildNameOSX = "FaircroftsAntiques5.app";

        public static void BuildVendors(Vendor[] targetVendors, List<BuildTarget> buildTargets, string[] buildVersions = null)
        {

            if (targetVendors.Length <= 0)
            {
                Debug.LogError("No specified target vendor to build.");
                return;
            }

            if (buildTargets.Count <= 0)
            {
                Debug.LogError("No specified build target.");
                return;
            }

            if (buildVersions.Length <= 0)
            {
                Debug.LogError("No specified build version.");
                return;
            }

            Debug.Log("Build Settings Passed...");

            foreach (var buildKey in buildVersions)
            {
                bool devMode = false;
                bool cheatsMode = false;
                bool disableSteamWorks = true;

                if (buildKey.Equals("ce"))
                {
                    Debug.Log("Building ce...");

                    devMode = false;
                    cheatsMode = false;
                    disableSteamWorks = true;

                    BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.CollectorsEdition);

                    if (buildTargets.Contains(BuildTarget.StandaloneWindows))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                                     buildKey, CEBuildName, devMode, cheatsMode, disableSteamWorks, targetVendors);

                    if (buildTargets.Contains(BuildTarget.StandaloneOSX))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                                 buildKey, CEBuildNameOSX, devMode, cheatsMode, disableSteamWorks, targetVendors);
                    continue;
                }

                if (buildKey.Equals("ce_dev"))
                {
                    Debug.Log("Building ce_dev...");

                    devMode = true;
                    cheatsMode = true;
                    disableSteamWorks = true;

                    BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.CollectorsEdition);
                    if (buildTargets.Contains(BuildTarget.StandaloneWindows))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                                     buildKey, CEBuildName, devMode, cheatsMode, disableSteamWorks, targetVendors);

                    if (buildTargets.Contains(BuildTarget.StandaloneOSX))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                                 buildKey, CEBuildNameOSX, devMode, cheatsMode, disableSteamWorks, targetVendors);
                    continue;
                }

                if (buildKey.Equals("ce_cheats"))
                {
                    Debug.Log("Building ce_cheats...");

                    devMode = false;
                    cheatsMode = true;
                    disableSteamWorks = true;

                    BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.CollectorsEdition);

                    if (buildTargets.Contains(BuildTarget.StandaloneWindows))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                                     buildKey, CEBuildName, devMode, cheatsMode, disableSteamWorks, targetVendors);

                    if (buildTargets.Contains(BuildTarget.StandaloneOSX))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                                 buildKey, CEBuildNameOSX, devMode, cheatsMode, disableSteamWorks, targetVendors);
                    continue;
                }

                if (buildKey.Equals("se"))
                {
                    Debug.Log("Building se...");

                    devMode = false;
                    cheatsMode = false;
                    disableSteamWorks = true;

                    BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.StandardEdition);

                    if (buildTargets.Contains(BuildTarget.StandaloneWindows))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.SE_BUILD, typeof(BuildScriptPackedMode),
                                     buildKey, SEBuildName, devMode, cheatsMode, disableSteamWorks, targetVendors);

                    if (buildTargets.Contains(BuildTarget.StandaloneOSX))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.SE_BUILD, typeof(BuildScriptPackedMode),
                                 buildKey, SEBuildNameOSX, devMode, cheatsMode, disableSteamWorks, targetVendors);
                    continue;
                }

                if (buildKey.Equals("se_dev"))
                {
                    Debug.Log("Building se_dev...");

                    devMode = true;
                    cheatsMode = true;
                    disableSteamWorks = true;

                    BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.StandardEdition);
                    if (buildTargets.Contains(BuildTarget.StandaloneWindows))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.SE_BUILD, typeof(BuildScriptPackedMode),
                                     buildKey, SEBuildName, devMode, cheatsMode, disableSteamWorks, targetVendors);

                    if (buildTargets.Contains(BuildTarget.StandaloneOSX))
                        BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.SE_BUILD, typeof(BuildScriptPackedMode),
                                 buildKey, SEBuildNameOSX, devMode, cheatsMode, disableSteamWorks, targetVendors);
                    continue;
                }
            }

            if (Directory.Exists("build/boomzap"))
            {
                Directory.Delete("build/boomzap", true);
            }
        }

        [MenuItem("Tools/Build Target Vendors")]
        public static void BuildTargetVendors()
        {
            Debug.Log("Build Start...");

            Vendor[] targetVendors = { Vendor.GameHouse };
            string[] buildVersions = { "ce_dev"};

            List<BuildTarget> buildTargets = new List<BuildTarget>() { BuildTarget.StandaloneWindows };

            //NOTE* Keywords used for build versions
            // ce_dev = CE Dev Build
            // ce_cheats = CE Cheats Build
            // ce = CE Build
            // se_dev = SE Dev Build
            // se = SE Build

            BuildVendors(targetVendors, buildTargets, buildVersions);
        }

        [MenuItem("Tools/Build Steam Mac")]
        public static void BuildSteam()
        {
            bool isDevBuild = true;
            bool isCheatsEnabled = true;

            const string steamBuildNameWin = "FaircroftsAntiques5.exe";
            const string steamBuildNameOSX = "FaircroftsAntiques5.app";

            BuildSettings.Get().SetGroups(BuildSettings.BuildSwitch.CollectorsEdition);

            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                            "ce", steamBuildNameWin, isDevBuild, isCheatsEnabled, false, null);

            if (!Directory.Exists("Steam/ContentBuilder/content/"))
                Directory.CreateDirectory("Steam/ContentBuilder/content");

            if (!Directory.Exists("Steam/ContentBuilder/content/osx_build"))
                Directory.CreateDirectory("Steam/ContentBuilder/content/osx_build");

            // honestly just going to hardcode this
            if (Directory.Exists("Steam/ContentBuilder/content/win32_build"))
                Directory.Delete("Steam/ContentBuilder/content/win32_build", true);

            Directory.Move($"build/{Vendor.Steam.ToString().ToLower()}/win32/ce", "Steam/ContentBuilder/content/win32_build");


            //WIN 64
            if (Directory.Exists("Steam/ContentBuilder/content/win64_build"))
                Directory.Delete("Steam/ContentBuilder/content/win64_build", true);


            BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
                         "ce", steamBuildNameWin, isDevBuild, isCheatsEnabled, false, null);

            Directory.Move($"build/{Vendor.Steam.ToString().ToLower()}/win64/ce", "Steam/ContentBuilder/content/win64_build");

            string outputBuild = BuildUtil.BuildStandalonePlayer(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, BuildUtil.DefineSymbols.CE_BUILD, typeof(BuildScriptPackedMode),
               "ce", steamBuildNameOSX, isDevBuild, isCheatsEnabled, false, null);

            if (Directory.Exists("Steam/ContentBuilder/content/osx_build/" + CEBuildNameOSX))
                Directory.Delete("Steam/ContentBuilder/content/osx_build/" + CEBuildNameOSX, true);

            Directory.Move($"build/{Vendor.Steam.ToString().ToLower()}/macos/ce/" + CEBuildNameOSX, "Steam/ContentBuilder/content/osx_build/" + CEBuildNameOSX);

            BuildUtil.NotarizeWithBoomzap("Steam/ContentBuilder/content/osx_build/" + CEBuildNameOSX, "com.Boomzap.Faircrofts-Antiques-5");

            if (Directory.Exists("build/steam"))
            {
                Directory.Delete("build/steam", true);
            }
        }
    }
}

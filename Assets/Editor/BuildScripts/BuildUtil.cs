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

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using Unity;


namespace ho.Build
{
    public enum Vendor
    {
        Alawar,
        Boomzap,
        Bigfish,
        Denda,
        GameHouse,
        Gamigo,
        iWin,
        Legacy,
        Steam,
        WildTangent
    }
    public class BuildUtil
    {
        class VendorData
        {
            public string spriteIDs;
            public string bundleName;
            public string vendorName;
        }

        const string productName = "First Time in Paris";

        const string CEMark = " - Collector's Edition";

        public static string CEProductName
        {
            get { return productName + CEMark; }
        }

        public int callbackOrder => throw new NotImplementedException();

        public enum DefineSymbols
        {
            SURVEY_BUILD,
            SE_BUILD,
            CE_BUILD
        }
        public static List<string> GenerateDefineSymbols(List<string> scriptDefines, DefineSymbols defineSymbol, bool disableSteamWorks, bool enableCheats)
        {
            //Remove these Define Symbols if it exists and add them later
            if (scriptDefines.Contains("DISABLESTEAMWORKS"))
                scriptDefines.Remove("DISABLESTEAMWORKS");

            if (scriptDefines.Contains("ENABLE_CHEATS"))
                scriptDefines.Remove("ENABLE_CHEATS");

            if (scriptDefines.Contains(DefineSymbols.SURVEY_BUILD.ToString()))
                scriptDefines.Remove(DefineSymbols.SURVEY_BUILD.ToString());

            if (scriptDefines.Contains(DefineSymbols.SE_BUILD.ToString()))
                scriptDefines.Remove(DefineSymbols.SE_BUILD.ToString());

            if (scriptDefines.Contains(DefineSymbols.CE_BUILD.ToString()))
                scriptDefines.Remove(DefineSymbols.CE_BUILD.ToString());

            switch (defineSymbol)
            {
                case DefineSymbols.SURVEY_BUILD:
                    {
                        scriptDefines.Add(DefineSymbols.SURVEY_BUILD.ToString());
                        break;
                    }
                case DefineSymbols.SE_BUILD:
                    {
                        scriptDefines.Add(DefineSymbols.SE_BUILD.ToString());
                        break;
                    }
                case DefineSymbols.CE_BUILD:
                    {
                        scriptDefines.Add(DefineSymbols.CE_BUILD.ToString());
                        break;
                    }
            }

            if (disableSteamWorks)
            {
                scriptDefines.Add("DISABLESTEAMWORKS");
            }

            if (enableCheats)
                scriptDefines.Add("ENABLE_CHEATS");

            return scriptDefines;
        }

        //        public void OnPostprocessBuild(BuildReport report)
        //        {
        //#if UNITY_STANDALONE_OSX
        //            UnityEngine.Debug.Log("Signing files for MacOS Build");
        //            UnityEditor.OSXStandalone.MacOSCodeSigning.CodeSignAppBundle(report.summary.outputPath + "/Contents/PlugIns/steam_api.bundle");
        //            UnityEditor.OSXStandalone.MacOSCodeSigning.CodeSignAppBundle(report.summary.outputPath);
        //#endif
        //            UnityEngine.Debug.Log("MyCustomBuildProcessor.OnPostprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        //        }

        public static void ExportBuildVersion()
        {
            File.WriteAllText("build/version.txt", PlayerSettings.bundleVersion);
        }

        public static void DisableAllVendorPackages()
        {
            var vendorGroups = AddressableAssetSettingsDefaultObject.Settings.groups.FindAll(x => x.name.ToLower().StartsWith("vendor-"));

            foreach (var vendor in vendorGroups)
            {
                if (vendor.Schemas.Count > 0 && vendor.HasSchema<BundledAssetGroupSchema>())
                {
                    var schema = vendor.Schemas.First(x => x is BundledAssetGroupSchema) as BundledAssetGroupSchema;
                    schema.IncludeInBuild = false;
                }
            }

            SplashConfig.currentBuildVendorSplashScreens = new AssetReferenceSprite[0];
            SplashConfig.currentBuildVendorName = "Boomzap";
        }

        public static void BuildVendorBuilds(Vendor[] includedVendors, BuildTarget target, BuildTargetGroup targetGroup, string folderName, string binaryName, bool isDevelopment)
        {
            List<VendorData> vendorData = new List<VendorData>();

            DisableAllVendorPackages();

            List<string> includedVendorNames = new List<string>();

            foreach (Vendor vendor in includedVendors)
            {
                Debug.Log($"Fetching vendor name {vendor.ToString().ToLower()}");
                includedVendorNames.Add(vendor.ToString().ToLower());
            }
                
            var vendorGroups = AddressableAssetSettingsDefaultObject.Settings.groups.FindAll(x => x.name.ToLower().StartsWith("vendor-"));

            foreach (var vendor in vendorGroups)
            {
                string vendorName = vendor.name.ToLower().Substring(7);

                //Create Vendor Data only for included vendors
                if (includedVendorNames.Contains(vendorName) == false) continue;

                if (vendor.Schemas.Count > 0 && vendor.HasSchema<BundledAssetGroupSchema>())
                {
                    var schema = vendor.Schemas.First(x => x is BundledAssetGroupSchema) as BundledAssetGroupSchema;

                    List<AddressableAssetEntry> assets = new List<AddressableAssetEntry>();

                    vendor.GatherAllAssets(assets, false, false, true);

                    AssetReferenceSprite[] assetReferenceSprites = assets.Where(x => x.ParentEntry.labels.Contains("VendorSplash"))
                        .Select((AddressableAssetEntry x) =>
                        {
                            var ar = new AssetReferenceSprite(x.ParentEntry.guid);
                            ar.SubObjectName = x.TargetAsset.name;
                            return ar;
                        }).ToArray();

                    schema.IncludeInBuild = true;

                    string ids = string.Join("\n", assetReferenceSprites.Select(x => x.AssetGUID));

                    vendorData.Add(new VendorData { bundleName = vendor.name + "_assets_all.bundle", vendorName = vendorName, spriteIDs = ids });

                    Debug.Log($"Added target vendor data with name {vendorName}.");
                }
            }

            BuildPlayerOptions options = CreatePlayerOptions(target, targetGroup, folderName, "model", binaryName, isDevelopment);
            AddressableAssetSettings.BuildPlayerContent();
            BuildPipeline.BuildPlayer(options);

            string modelPath;
            if (target != BuildTarget.StandaloneOSX)
            {
                modelPath = options.locationPathName.Substring(0, options.locationPathName.Length - binaryName.Length);
            }
            else
            {
                modelPath = options.locationPathName + "/";
            }

            string[] vendorBundles = vendorData.Select(x =>
                UnityEditor.Build.Pipeline.Utilities.HashingMethods.Calculate(x.bundleName.ToLower()) + ".bundle"
            ).ToArray();

            foreach (var v in vendorData)
            {
                string outPath = CreatePlayerOptions(target, targetGroup, folderName, v.vendorName, binaryName, isDevelopment).locationPathName;

                if (target != BuildTarget.StandaloneOSX)
                {
                    outPath = outPath.Substring(0, outPath.Length - binaryName.Length);

                    if (Directory.Exists(outPath))
                        Directory.Delete(outPath, true);

                    Directory.CreateDirectory(outPath);

                    File.WriteAllText(outPath + "config", v.spriteIDs);

                    DirectoryCopy(modelPath, outPath, true);

                    // addressables
                    string addressableDir = outPath + binaryName.Substring(0, binaryName.Length - 4) + "_Data/StreamingAssets/aa/StandaloneWindows/";
                    string nameHash = UnityEditor.Build.Pipeline.Utilities.HashingMethods.Calculate(v.bundleName.ToLower()).ToString();

                    foreach (var d in Directory.EnumerateFiles(addressableDir))
                    {
                        string f = d.Substring(addressableDir.Length);
                        if (f.ToLower() == (nameHash + ".bundle")) continue;
                        if (!vendorBundles.Contains(f.ToLower())) continue;
                        File.Delete(d);
                    }
                }
                else
                {
                    var appPath = outPath;

                    outPath += "/";

                    if (Directory.Exists(outPath))
                        Directory.Delete(outPath, true);

                    Directory.CreateDirectory(outPath);

                    DirectoryCopy(modelPath, outPath, true);

                    File.WriteAllText(outPath + "Contents/config", v.spriteIDs);


                    // addressables
                    string addressableDir = outPath + "Contents/Resources/Data/StreamingAssets/aa/StandaloneOSX/";
                    string nameHash = UnityEditor.Build.Pipeline.Utilities.HashingMethods.Calculate(v.bundleName.ToLower()).ToString();

                    foreach (var d in Directory.EnumerateFiles(addressableDir))
                    {
                        string f = d.Substring(addressableDir.Length);
                        if (f.ToLower() == (nameHash + ".bundle")) continue;
                        if (!vendorBundles.Contains(f.ToLower())) continue;
                        File.Delete(d);
                    }

                    ForceDeepCodeSign(appPath);
                }

            }

            if (target == BuildTarget.StandaloneOSX)
            {
                modelPath = options.locationPathName.Substring(0, options.locationPathName.Length - binaryName.Length);
            }

            Directory.Delete(modelPath, true);


            //schema.IncludeInBuild = true;



            //schema.IncludeInBuild = false;
        }

        public static void AssignDataBuilder<T>() where T : IDataBuilder
        {
            var dataBuilder = AddressableAssetSettingsDefaultObject.Settings.DataBuilders.First(x => x is T);
            AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex =
                AddressableAssetSettingsDefaultObject.Settings.DataBuilders.IndexOf(dataBuilder);
        }

        public static void AssignDataBuilder(Type dataBuilderType)
        {
            var dataBuilder = AddressableAssetSettingsDefaultObject.Settings.DataBuilders.First(x => x.GetType() == dataBuilderType);
            AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex =
                AddressableAssetSettingsDefaultObject.Settings.DataBuilders.IndexOf(dataBuilder);
        }


        public static string[] AssignScriptDefines(DefineSymbols symbols, BuildTargetGroup targetGroup, bool disableSteamWorks = false, bool enableCheats = false)
        {
            string[] scriptDefines;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup, out scriptDefines);
            var newDefines = GenerateDefineSymbols(scriptDefines.ToList(), symbols, disableSteamWorks, enableCheats);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefines.ToArray());

            return scriptDefines;
        }

        public static string[] AssignScriptDefines(string[] defines, BuildTargetGroup targetGroup)
        {
            string[] scriptDefines;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup, out scriptDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);

            return scriptDefines;
        }

        public static BuildPlayerOptions CreatePlayerOptions(BuildTarget target, BuildTargetGroup targetGroup, string folder, string vendorName, string binaryName, bool isDevelopment)
        {
            BuildPlayerOptions options = new BuildPlayerOptions();

            string platform = "win32";
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                    platform = "win32";
                    break;
                case BuildTarget.StandaloneOSX:
                    platform = "macos";
                    break;
                case BuildTarget.Android:
                    platform = "android";
                    break;
                case BuildTarget.iOS:
                    platform = "ios";
                    break;
                case BuildTarget.StandaloneWindows64:
                    platform = "win64";
                    break;
            }

            options.target = target;
            options.targetGroup = targetGroup;

            options.locationPathName = $"build/{vendorName}/{platform}/{folder}/{binaryName}";

            options.options = isDevelopment ? BuildOptions.Development : BuildOptions.None;
            options.scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();

            return options;
        }

        public static string BuildStandalonePlayer(BuildTargetGroup group, BuildTarget target, DefineSymbols symbols, Type dataBuilder, string folder, string binaryName, bool isDevelopment, bool isCheatsEnabled, bool disableSteamWorks, Vendor[] includedVendors, bool rebuildAddressables = true)
        {
            if (!Directory.Exists("build"))
            {
                Directory.CreateDirectory("build");
            }

            PlayerSettings.productName = symbols == DefineSymbols.CE_BUILD ? CEProductName : productName;

            ExportBuildVersion();
            DisableAllVendorPackages();

            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            AssignDataBuilder(dataBuilder);

            string[] scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();

            var oldDefines = AssignScriptDefines(symbols, group, disableSteamWorks, isCheatsEnabled);

            PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.Mono2x);

            if (rebuildAddressables)
            {
                UnityEditor.Build.Pipeline.Utilities.BuildCache.PurgeCache(false);
                AddressableAssetSettings.CleanPlayerContent();
            }

            string vendorName = disableSteamWorks ? "boomzap" : "steam";

            if (includedVendors.Length > 0)
            {
                Debug.Log("Start building vendors...");
                BuildVendorBuilds(includedVendors, target, group, folder, binaryName, isDevelopment);
            }
                

            if (disableSteamWorks == false || symbols == DefineSymbols.SURVEY_BUILD)
            {
                BuildPlayerOptions options = CreatePlayerOptions(target, group, folder, vendorName, binaryName, isDevelopment);

                BuildPipeline.BuildPlayer(options);

                if (symbols == DefineSymbols.SURVEY_BUILD)
                {
                    File.Copy("bfg_survey.txt", Directory.GetParent(options.locationPathName).FullName + "/bfg_survey.txt");
                }
                Debug.Log("Build for Steam or Survey");
                //For Steamworks
                return options.locationPathName;
            }

            AssignScriptDefines(oldDefines, group);


           
            Debug.Log("Build is done");

            return string.Empty;
        }

        public static void ForceDeepCodeSign(string appPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "/usr/bin/codesign";
            process.StartInfo.Arguments = "--deep -s - -f \"" + appPath + "\"";

            process.Start();

            Debug.Log(process.StandardOutput.ReadToEnd());
            process.WaitForExit();


        }

        static string RunProcessAndReturnOutput(string proc, string args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = proc;
            process.StartInfo.Arguments = args;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        public static void NotarizeWithBoomzap(string appPath, string bundleID)
        {
            string entitlementsFile =
                @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
                <plist version=""1.0"">
                    <dict>
	                        <key>com.apple.security.cs.allow-dyld-environment-variables</key>
	                        <true/>
	                        <key>com.apple.security.cs.disable-library-validation</key>
	                        <true/>
	                        <key>com.apple.security.cs.disable-executable-page-protection</key>
	                        <true/>
                    </dict>
                </plist>";

            File.WriteAllText("boomzap.entitlements", entitlementsFile);

            RunProcessAndReturnOutput("/bin/chmod", $"-R a+xr \"{appPath}\"");
            string codeSignOutput = RunProcessAndReturnOutput("/usr/bin/codesign", $"--deep --force --verify --verbose --timestamp --options runtime --entitlements \"boomzap.entitlements\" --sign \"Developer ID Application: Boomzap Pte Ltd (8HA5L8ZDD4)\" \"{appPath}\"");
            Debug.Log(codeSignOutput);

            string outZipPath = appPath.Replace(".app", ".zip");
            RunProcessAndReturnOutput("/usr/bin/ditto", $"-c -k --sequesterRsrc --keepParent \"{appPath}\" \"{outZipPath}\"");

            //string notarizeResponse = RunProcessAndReturnOutput("/usr/bin/xcrun", $"altool --notarize-app --username jordan@boomzap.com --password hodo-uway-vuje-tdba --asc-provider BoomzapPteLtd --primary-bundle-id \"{bundleID}\" --file \"{outZipPath}\" --wait");
            //Debug.Log(notarizeResponse);

            string notarizeResponse = RunProcessAndReturnOutput("/usr/bin/xcrun", $"notarytool submit \"{outZipPath}\" --apple-id jordan@boomzap.com --password hodo-uway-vuje-tdba --team-id 8HA5L8ZDD4 --wait");
            Debug.Log(notarizeResponse);

            string stapleResponse = RunProcessAndReturnOutput("/usr/bin/xcrun", $"stapler staple \"{appPath}\"");
            Debug.Log(stapleResponse);

            File.Delete(outZipPath);
            File.Delete("boomzap.entitlements");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.AddressableAssets;

public class SplashConfig
{
    public static string currentBuildVendorName = "Boomzap";
    public static AssetReferenceSprite[] currentBuildVendorSplashScreens = new AssetReferenceSprite[0];

    [PostProcessScene(0)]
    public static void OnPostprocessScene()
    {
        if (!BuildPipeline.isBuildingPlayer) return;

        ho.SplashController splashController = Object.FindObjectOfType<ho.SplashController>();
        if (splashController == null) return;

        Debug.Log($"Configuring splash screens for {currentBuildVendorName}");

        splashController.SetSplashScreens(2f, currentBuildVendorSplashScreens);
    }
}

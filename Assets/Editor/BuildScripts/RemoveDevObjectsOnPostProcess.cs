using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class RemoveDevObjects
{
    [PostProcessScene(0)]
    public static void OnPostprocessScene()
    {
        if (!BuildPipeline.isBuildingPlayer) return;

        GameObject[] devObjects = GameObject.FindGameObjectsWithTag("RemoveFromReleaseBuild");

        Debug.Log($"Removing {devObjects.Length} dev objects from scene");

        foreach (var go in devObjects)
            GameObject.DestroyImmediate(go);
    }
}

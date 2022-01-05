using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.IO;

using Boomzap.Utilities;

public class SDFCreate : EditorWindow
{
    [MenuItem("Tools/SDFGenerator")]
    static void Go()
    {
        GetWindow<SDFCreate>().Start();
    }

    public Texture2D sourceTexture;

    public void Start()
    {
        Show();
    }

    private void OnGUI()
    {
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture"), sourceTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Generate"))
        {
            string srcPath = AssetDatabase.GetAssetPath(sourceTexture);
            string destPath = srcPath.Replace(".png", ".sdf.png");

            Texture2D readable = Boomzap.SDFGenerator.CreateReadableScaledTexture(sourceTexture, 1f);

            Texture2D generated = Boomzap.SDFGenerator.Generate(readable, new Rect(Vector2.zero, new Vector2(readable.width, readable.height)), new Vector2Int(100, 100));

            File.WriteAllBytes(destPath, generated.EncodeToPNG());

            DestroyImmediate(generated);
            Boomzap.SDFGenerator.ClearTextureCache();
        }
    }
}

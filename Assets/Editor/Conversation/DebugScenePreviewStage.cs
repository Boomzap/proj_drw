using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Boomzap.Conversation
{
    public class DebugScenePreviewStage : SceneView
    {
        Scene scene;

        private new void OnDestroy()
        {
            EditorSceneManager.ClosePreviewScene(scene);
            base.OnDestroy();
        }

        private void Update()
        {
            
        }

        public static void Show(Scene previewScene)
        {
            DebugScenePreviewStage window = CreateWindow<DebugScenePreviewStage>();

            window.titleContent = new GUIContent("Conversation Preview", EditorGUIUtility.IconContent("GameObject Icon").image);

            window.customScene = previewScene;
            window.scene = previewScene;
            
            window.drawGizmos = false;

            Selection.activeObject = previewScene.GetRootGameObjects()[0];
            window.FrameSelected();

            window.Repaint();
            
        }

        private new void OnGUI()
        {
            base.OnGUI();
        }
    }
}
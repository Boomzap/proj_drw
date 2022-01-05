using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;

namespace Boomzap.Conversation
{
    internal class ConversationPreviewWindow : EditorWindow
    {
        RenderTexture previewTexture = null;

        public static void Open(RenderTexture texture)
        {
            GetWindow<ConversationPreviewWindow>().Show();
            GetWindow<ConversationPreviewWindow>().SetPreviewTexture(texture);
        }

        public void SetPreviewTexture(RenderTexture texture)
        {
            previewTexture = texture;
        }

        private void OnGUI()
        {
            minSize = new Vector2(480f, 270f);

            titleContent = new GUIContent("Conversation Preview");

            if (previewTexture != null)
                EditorGUI.DrawTextureTransparent(new Rect(0, 0, position.width, position.height), previewTexture);
        }
    }

    public partial class ConversationEditor : EditorWindow
    {
        RenderTexture previewTexture = null;
        Unity.EditorCoroutines.Editor.EditorCoroutine currentUpdateCor = null;

        void ShowVisualizer()
        {
            ConversationPreviewWindow.Open(previewTexture);
        }

        void UpdateVisualizer(ConversationNode node)
        {
            if (!HasOpenInstances<ConversationPreviewWindow>()) return;
            
            if (currentUpdateCor != null)
                Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StopCoroutine(currentUpdateCor);
            Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutine(UpdatePreviewScene(node), this);
        }

        void ForceRepaintVisualizer()
        {
            if (HasOpenInstances<ConversationPreviewWindow>())
            {
                GetWindow<ConversationPreviewWindow>().SetPreviewTexture(previewTexture);
                GetWindow<ConversationPreviewWindow>().Repaint();
            }
        }

        IEnumerator UpdatePreviewScene(ConversationNode previewNode)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            
            GameObject go = new GameObject();
            Camera previewCamera = go.AddComponent<Camera>();
            EditorSceneManager.MoveGameObjectToScene(go, scene);

            previewCamera.transform.position = new Vector3(0f, 0f, -10f);
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 540f;      // cam resolution = 1920x1080
            previewCamera.aspect = 16f/9f;
            previewCamera.nearClipPlane = 0.3f;
            previewCamera.farClipPlane = 1000f;

            if (previewTexture == null)
                previewTexture = new RenderTexture(1920, 1080, 32);

            previewCamera.targetTexture = previewTexture;
            previewCamera.scene = scene;

            float xDelta = (1920f - 400f) / (ConversationManager.MaxCharacterSlots-1);
            float xPos = -((1920f - 400f) * 0.5f);

            foreach (var stateDef in previewNode.characters)
            {
                if (stateDef.character == null)
                {
                    xPos += xDelta;
                    continue;
                }

                GameObject charObj = Instantiate(stateDef.character.characterRef.editorAsset);

                Character.Character character = charObj.GetComponent<Character.Character>();
                character.ForceUpdateWithoutSaving(stateDef.state, stateDef.emotion, stateDef.eyes, stateDef.spineSlots, !stateDef.faceLeft, stateDef.lookBack);
                character.ForceAlpha(1f);
                

                EditorSceneManager.MoveGameObjectToScene(charObj, scene);
                charObj.transform.localScale = new Vector3(70f, 70f, 1f);
                charObj.transform.position = new Vector3(xPos, -100f, 0f);

                xPos += xDelta;
            }

            if(previewNode.background.Equals("Default") == false)
            {
                GameObject bgObject = Instantiate(ho.HORoomAssetManager.instance.roomTracker.GetItemByName(previewNode.background).editorAsset);
                EditorSceneManager.MoveGameObjectToScene(bgObject, scene);
                bgObject.transform.position = new Vector3(0, 0f, 0f);
            }

            yield return new WaitForEndOfFrame();

            previewCamera.Render();
            previewCamera.targetTexture = null;
// 
//             Texture2D newTexture = new Texture2D(1920, 1080);
// 
//             RenderTexture.active = renderTexture;
//             newTexture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
//             newTexture.Apply();
// 
//             RenderTexture.active = null;
// 
//             byte[] previewTexture = newTexture.EncodeToPNG();
// 
//             System.IO.File.WriteAllBytes("w:\\test.png", previewTexture);

            EditorSceneManager.ClosePreviewScene(scene);
            currentUpdateCor = null;

            ForceRepaintVisualizer();
            EditorWindow.FocusWindowIfItsOpen<ConversationEditor>();
        }
    }
}
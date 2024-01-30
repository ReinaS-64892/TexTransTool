using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.Security.Cryptography;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    public abstract class TTTImportedImage : ScriptableObject
    {
        public TTTImportedCanvasDescription CanvasDescription;
        public Texture2D PreviewTexture;

        // R8 or RGBA32 Non MipMap
        internal abstract void LoadImage(byte[] importSouse, RenderTexture WriteTarget);
        internal abstract Vector2Int Pivot { get; }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(TTTImportedImage))]
    public class TTTImportedPngEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            EditorGUILayout.LabelField("Debugなどでこのオブジェクトの中身を見ようとしないでください!!!、UnityEditorが停止します!!!");
            var thisTarget = target as TTTImportedImage;
            // if (thisTarget.PreviewTexture != null) { EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), thisTarget.PreviewTexture, ScaleMode.ScaleToFit); }

        }
    }
#endif
}
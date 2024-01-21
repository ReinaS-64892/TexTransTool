using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.Security.Cryptography;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    public class TTTImportedPng : ScriptableObject
    {
        public byte[] PngBytes;

        public Texture2D PreviewTexture;
    }

    [CustomEditor(typeof(TTTImportedPng))]
    public class TTTImportedPngEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            EditorGUILayout.LabelField("Debugなどでこのオブジェクトの中身を見ようとしないでください!!!、UnityEditorが停止します!!!");
            var thisTarget = target as TTTImportedPng;
            if (thisTarget.PreviewTexture != null) { EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), thisTarget.PreviewTexture, ScaleMode.ScaleToFit); }

        }
    }
}
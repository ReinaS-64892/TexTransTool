using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.TextureAtlas.Editor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(TextureCompressionData))]
    internal class CompressionQualityDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CompressTuningDrawer.DrawCompressEditor(position, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return CompressTuningDrawer.GetPropertyHeightInter(property);
        }

    }
}

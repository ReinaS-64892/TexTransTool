using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.Editor;

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

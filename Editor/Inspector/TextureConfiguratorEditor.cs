using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Preview;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureConfigurator))]
    internal class TextureConfiguratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("TextureConfigurator");

            var targetTC = target as TextureConfigurator;
            var sObj = serializedObject;

            var sTargetTexture = sObj.FindProperty("TargetTexture");

            var sOverrideTextureSetting = sObj.FindProperty("OverrideTextureSetting");
            var sTextureSize = sObj.FindProperty("TextureSize");
            var sMipMap = sObj.FindProperty("MipMap");

            var sOverrideCompression = sObj.FindProperty("OverrideCompression");
            var sCompressionSetting = sObj.FindProperty("CompressionSetting");

            EditorGUILayout.PropertyField(sTargetTexture);

            EditorGUILayout.PropertyField(sOverrideTextureSetting,"OverrideTextureSetting".GlcV());
            using (new EditorGUI.DisabledScope(!sOverrideTextureSetting.boolValue))
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sTextureSize, "TextureSize".GlcV());
                EditorGUILayout.PropertyField(sMipMap, "MipMap".GlcV());
            }

            EditorGUILayout.PropertyField(sOverrideCompression,"OverrideCompression".GlcV());
            using (new EditorGUI.DisabledScope(!sOverrideCompression.boolValue))
            using (new EditorGUI.IndentLevelScope())
            { EditorGUILayout.PropertyField(sCompressionSetting, "OverrideCompression".GlcV()); }

            PreviewButtonDrawUtil.Draw(targetTC);
            sObj.ApplyModifiedProperties();
        }
    }
}

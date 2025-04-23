using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureConfigurator))]
    internal class TextureConfiguratorEditor : TexTransMonoBaseEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var sObj = serializedObject;

            var sTargetTexture = sObj.FindProperty(nameof(TextureConfigurator.TargetTexture));

            var sOverrideTextureSetting = sObj.FindProperty(nameof(TextureConfigurator.OverrideTextureSetting));
            var sTextureSize = sObj.FindProperty(nameof(TextureConfigurator.TextureSize));
            var sMipMap = sObj.FindProperty(nameof(TextureConfigurator.MipMap));

            var sOverrideCompression = sObj.FindProperty(nameof(TextureConfigurator.OverrideCompression));
            var sCompressionSetting = sObj.FindProperty(nameof(TextureConfigurator.CompressionSetting));

            EditorGUILayout.PropertyField(sTargetTexture, "TargetTexture".GlcV());

            EditorGUILayout.PropertyField(sOverrideTextureSetting, "OverrideTextureSetting".GlcV());
            using (new EditorGUI.DisabledScope(!sOverrideTextureSetting.boolValue))
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sTextureSize, "TextureSize".GlcV());
                EditorGUILayout.PropertyField(sMipMap, "MipMap".GlcV());
            }

            EditorGUILayout.PropertyField(sOverrideCompression, "OverrideCompression".GlcV());
            using (new EditorGUI.DisabledScope(!sOverrideCompression.boolValue))
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sCompressionSetting, "OverrideCompression".GlcV());
            }
        }
    }
}

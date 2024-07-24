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
            var sDownScalingAlgorism = sObj.FindProperty("DownScalingAlgorism");
            var sDownScalingWithLookAtAlpha = sObj.FindProperty("DownScalingWithLookAtAlpha");

            var sOverrideCompression = sObj.FindProperty("OverrideCompression");
            var sCompressionSetting = sObj.FindProperty("CompressionSetting");

            EditorGUILayout.PropertyField(sTargetTexture);

            EditorGUILayout.PropertyField(sOverrideTextureSetting);
            using (new EditorGUI.DisabledScope(!sOverrideTextureSetting.boolValue))
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sTextureSize);
                EditorGUILayout.PropertyField(sMipMap);
                EditorGUILayout.PropertyField(sDownScalingAlgorism);
                EditorGUILayout.PropertyField(sDownScalingWithLookAtAlpha);
            }

            EditorGUILayout.PropertyField(sOverrideCompression);
            using (new EditorGUI.DisabledScope(!sOverrideCompression.boolValue))
            using (new EditorGUI.IndentLevelScope())
            { EditorGUILayout.PropertyField(sCompressionSetting); }

            OneTimePreviewContext.instance.DrawApplyAndRevert(targetTC);
            sObj.ApplyModifiedProperties();
        }
    }
}

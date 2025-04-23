using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(ParallelProjectionWithLilToonDecal))]
    internal class ParallelProjectionWithLilToonDecalEditor : TexTransMonoBaseEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var thisSObject = serializedObject;

            var sTargetMaterial = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.TargetMaterial));
            var sDecalTexture = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.DecalTexture));
            var sColor = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.Color));
            var sMSDFTexture = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.MSDFTexture));
            var sCullMode = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.CullMode));
            var sWriteUVTarget = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.WriteUVTarget));
            var sReplaceTextureTarget = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.ReplaceTextureTarget));
            var sTransparentMode = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.TransparentMode));
            var sShaderBlendingMode = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.ShaderBlendingMode));
            var sIslandSelector = thisSObject.FindProperty(nameof(ParallelProjectionWithLilToonDecal.IslandSelector));

            EditorGUILayout.LabelField("MainSetting");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sTargetMaterial, "TargetMaterial".GlcV());
                EditorGUILayout.PropertyField(sDecalTexture, "DecalTexture".GlcV());
            }

            EditorGUILayout.LabelField("lilToonSetting");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sColor, "Color".Glc());
                EditorGUILayout.PropertyField(sMSDFTexture, "MSDFTexture".Glc());
                EditorGUILayout.PropertyField(sCullMode, "CullMode".Glc());
                EditorGUILayout.PropertyField(sTransparentMode, "TransparentMode".Glc());
                EditorGUILayout.PropertyField(sShaderBlendingMode, "ShaderBlendingMode".Glc());
            }

            EditorGUILayout.LabelField("WriteSetting");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(sWriteUVTarget, "WriteUVTarget".GlcV());
                EditorGUILayout.PropertyField(sReplaceTextureTarget, "ReplaceTextureTarget".GlcV());
                EditorGUILayout.PropertyField(sIslandSelector, "IslandSelector".GlcV());
            }
        }
    }

}

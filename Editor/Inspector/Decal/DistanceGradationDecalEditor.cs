using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(DistanceGradationDecal))]
    internal class DistanceGradationDecalEditor : TTCanBehaveAsLayerEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            using (new EditorGUI.IndentLevelScope(1))
                if (IsLayerMode is false)
                {
                    EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);

                    var sRendererSelector = serializedObject.FindProperty("RendererSelector");
                    EditorGUILayout.PropertyField(sRendererSelector);
                }

            EditorGUILayout.LabelField("CommonDecal:label:GradationSettings".Glc(), EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope(1))
            {
                var sGradationMinDistance = serializedObject.FindProperty("GradationMinDistance");
                EditorGUILayout.PropertyField(sGradationMinDistance, "DistanceGradationDecal:prop:GradationMinDistance".Glc());
                var sGradationMaxDistance = serializedObject.FindProperty("GradationMaxDistance");
                EditorGUILayout.PropertyField(sGradationMaxDistance, "DistanceGradationDecal:prop:GradationMaxDistance".Glc());


                var sGradient = serializedObject.FindProperty("Gradient");
                EditorGUILayout.PropertyField(sGradient, "GradationDecal:prop:Gradient".Glc());

                var sAlpha = serializedObject.FindProperty("Alpha");
                EditorGUILayout.PropertyField(sAlpha, "GradationDecal:prop:Alpha".Glc());

                var sGradientClamp = serializedObject.FindProperty("GradientClamp");
                EditorGUILayout.PropertyField(sGradientClamp, "GradationDecal:prop:GradientClamp".Glc());
            }

            EditorGUILayout.LabelField("CommonDecal:label:DecalSettings".Glc(), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                var sIslandSelector = serializedObject.FindProperty("IslandSelector");
                EditorGUILayout.PropertyField(sIslandSelector, "CommonDecal:prop:IslandSelector".Glc());

                var sBlendTypeKey = serializedObject.FindProperty("BlendTypeKey");
                EditorGUILayout.PropertyField(sBlendTypeKey, "CommonDecal:prop:BlendTypeKey".Glc());

                var sTargetPropertyName = serializedObject.FindProperty("TargetPropertyName");
                if (IsLayerMode is false) EditorGUILayout.PropertyField(sTargetPropertyName, "CommonDecal:prop:TargetPropertyName".Glc());
            }

            DecalEditorUtil.DrawerAdvancedOption(serializedObject);
        }

    }
}

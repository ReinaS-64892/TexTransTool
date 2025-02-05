using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using System.Collections.Generic;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using System.Linq;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(DistanceGradationDecal))]
    internal class DistanceGradationDecalEditor : UnityEditor.Editor
    {
        CanBehaveAsLayerEditorUtil behaveLayerUtil;
        void BehaveUtilInit() { behaveLayerUtil = new(target as Component); }
        void OnEnable() { BehaveUtilInit(); EditorApplication.hierarchyChanged += BehaveUtilInit; }
        void OnDisable() { EditorApplication.hierarchyChanged -= BehaveUtilInit; }
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            TextureTransformerEditor.DrawerWarning(target.GetType().Name);

            EditorGUI.indentLevel += 1;
            if (behaveLayerUtil.IsLayerMode is false)
            {
                EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);

                var sRendererSelector = serializedObject.FindProperty("RendererSelector");
                EditorGUILayout.PropertyField(sRendererSelector);
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("CommonDecal:label:GradationSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


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

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("CommonDecal:label:DecalSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sIslandSelector = serializedObject.FindProperty("IslandSelector");
            EditorGUILayout.PropertyField(sIslandSelector, "CommonDecal:prop:IslandSelector".Glc());

            var sBlendTypeKey = serializedObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendTypeKey, "CommonDecal:prop:BlendTypeKey".Glc());

            var sTargetPropertyName = serializedObject.FindProperty("TargetPropertyName");
            if (behaveLayerUtil.IsLayerMode is false) EditorGUILayout.PropertyField(sTargetPropertyName, "CommonDecal:prop:TargetPropertyName".Glc());

            EditorGUI.indentLevel -= 1;

            DecalEditorUtil.DrawerAdvancedOption(serializedObject);

            if (behaveLayerUtil.IsDrawPreviewButton) PreviewButtonDrawUtil.Draw(target as TexTransMonoBase);
            behaveLayerUtil.DrawAddLayerButton(target as Component);

            serializedObject.ApplyModifiedProperties();
        }


    }
}

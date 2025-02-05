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
    [CustomEditor(typeof(SingleGradationDecal))]
    internal class SingleGradationDecalEditor : UnityEditor.Editor
    {
        CanBehaveAsLayerEditorUtil behaveLayerUtil;
        void BehaveUtilInit() { behaveLayerUtil = new(target as Component); }
        void OnEnable() { BehaveUtilInit(); EditorApplication.hierarchyChanged += BehaveUtilInit; }
        void OnDisable() { EditorApplication.hierarchyChanged -= BehaveUtilInit; }
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
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

            var tf = new SerializedObject((serializedObject.targetObject as SingleGradationDecal).transform);
            var sLocalScale = tf.FindProperty("m_LocalScale");
            var length = sLocalScale.vector3Value.y;
            var lengthRect = EditorGUILayout.GetControlRect();
            var propGUIContent = EditorGUI.BeginProperty(lengthRect, "SingleGradationDecal:prop:GradationLength".Glc(), sLocalScale);
            length = EditorGUI.FloatField(lengthRect, propGUIContent, length);
            sLocalScale.vector3Value = new Vector3(length, length, length);
            EditorGUI.EndProperty();


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
            EditorGUILayout.PropertyField(sBlendTypeKey, "GradationDecal:prop:BlendTypeKey".Glc());

            var sTargetPropertyName = serializedObject.FindProperty("TargetPropertyName");
            if (behaveLayerUtil.IsLayerMode is false) EditorGUILayout.PropertyField(sTargetPropertyName, "GradationDecal:prop:TargetPropertyName".Glc());

            EditorGUI.indentLevel -= 1;

            DecalEditorUtil.DrawerAdvancedOption(serializedObject);

            if (behaveLayerUtil.IsDrawPreviewButton) PreviewButtonDrawUtil.Draw(target as TexTransMonoBase);
            behaveLayerUtil.DrawAddLayerButton(target as Component);

            serializedObject.ApplyModifiedProperties();
            tf.ApplyModifiedProperties();
        }


    }
}

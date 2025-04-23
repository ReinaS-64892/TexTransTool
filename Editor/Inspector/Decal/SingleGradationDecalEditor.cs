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
    internal class SingleGradationDecalEditor : TTCanBehaveAsLayerEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            if (IsLayerMode is false)
            {
                EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);

                using var indexScope = new EditorGUI.IndentLevelScope(1);
                var sRendererSelector = serializedObject.FindProperty("RendererSelector");
                EditorGUILayout.PropertyField(sRendererSelector);
            }

            EditorGUILayout.LabelField("CommonDecal:label:GradationSettings".Glc(), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                var tf = new SerializedObject((serializedObject.targetObject as SingleGradationDecal).transform);
                var sLocalScale = tf.FindProperty("m_LocalScale");
                var length = sLocalScale.vector3Value.y;
                var lengthRect = EditorGUILayout.GetControlRect();
                var propGUIContent = EditorGUI.BeginProperty(lengthRect, "SingleGradationDecal:prop:GradationLength".Glc(), sLocalScale);
                length = EditorGUI.FloatField(lengthRect, propGUIContent, length);
                sLocalScale.vector3Value = new Vector3(length, length, length);
                EditorGUI.EndProperty();


                var sGradient = serializedObject.FindProperty(nameof(SingleGradationDecal.Gradient));
                EditorGUILayout.PropertyField(sGradient, "GradationDecal:prop:Gradient".Glc());

                var sAlpha = serializedObject.FindProperty(nameof(SingleGradationDecal.Alpha));
                EditorGUILayout.PropertyField(sAlpha, "GradationDecal:prop:Alpha".Glc());

                var sGradientClamp = serializedObject.FindProperty(nameof(SingleGradationDecal.GradientClamp));
                EditorGUILayout.PropertyField(sGradientClamp, "GradationDecal:prop:GradientClamp".Glc());
            }

            EditorGUILayout.LabelField("CommonDecal:label:DecalSettings".Glc(), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                var sIslandSelector = serializedObject.FindProperty(nameof(SingleGradationDecal.IslandSelector));
                EditorGUILayout.PropertyField(sIslandSelector, "CommonDecal:prop:IslandSelector".Glc());

                var sBlendTypeKey = serializedObject.FindProperty(nameof(SingleGradationDecal.BlendTypeKey));
                EditorGUILayout.PropertyField(sBlendTypeKey, "GradationDecal:prop:BlendTypeKey".Glc());

                var sTargetPropertyName = serializedObject.FindProperty(nameof(SingleGradationDecal.TargetPropertyName));
                if (IsLayerMode is false) EditorGUILayout.PropertyField(sTargetPropertyName, "GradationDecal:prop:TargetPropertyName".Glc());

                DecalEditorUtil.DrawerAdvancedOption(serializedObject);
            }
        }


    }
}

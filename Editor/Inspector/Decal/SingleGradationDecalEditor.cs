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
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(target.GetType().Name);

            EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sRendererSelector = serializedObject.FindProperty("RendererSelector");
            EditorGUILayout.PropertyField(sRendererSelector);

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
            EditorGUILayout.PropertyField(sGradient, "SingleGradationDecal:prop:Gradient".Glc());

            var sAlpha = serializedObject.FindProperty("Alpha");
            EditorGUILayout.PropertyField(sAlpha, "SingleGradationDecal:prop:Alpha".Glc());

            var sGradientClamp = serializedObject.FindProperty("GradientClamp");
            EditorGUILayout.PropertyField(sGradientClamp, "SingleGradationDecal:prop:GradientClamp".Glc());

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("CommonDecal:label:DecalSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sIslandSelector = serializedObject.FindProperty("IslandSelector");
            EditorGUILayout.PropertyField(sIslandSelector, "SingleGradationDecal:prop:IslandSelector".Glc());

            var sBlendTypeKey = serializedObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendTypeKey, "SingleGradationDecal:prop:BlendTypeKey".Glc());

            var sTargetPropertyName = serializedObject.FindProperty("TargetPropertyName");
            EditorGUILayout.PropertyField(sTargetPropertyName, "SingleGradationDecal:prop:TargetPropertyName".Glc());

            EditorGUI.indentLevel -= 1;

            DecalEditorUtil.DrawerAdvancedOption(serializedObject);

            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);
            serializedObject.ApplyModifiedProperties();
            tf.ApplyModifiedProperties();
        }


    }
}

using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;

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

                float editLength;
                if (length >= 0.0f && length <= 2.0f) editLength = EditorGUI.Slider(lengthRect, propGUIContent, length, 0.0f, 2.0f);
                else editLength = EditorGUI.FloatField(lengthRect, propGUIContent, length);

                if (editLength != length) sLocalScale.vector3Value = new Vector3(editLength, editLength, editLength);

                EditorGUI.EndProperty();
                tf.ApplyModifiedProperties();


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

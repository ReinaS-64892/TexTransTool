#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureBlender))]
    public class TextureBlenderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("TextureBlender");

            var Target = target as TextureBlender;
            var This_S_Object = serializedObject;

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(Target));

            DrawerRelativeTextureSelector(This_S_Object.FindProperty("TargetTexture"));

            var S_BlendTexture = This_S_Object.FindProperty("BlendTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(S_BlendTexture);

            var S_Color = This_S_Object.FindProperty("Color");
            EditorGUILayout.PropertyField(S_Color);

            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType);

            EditorGUI.EndDisabledGroup();


            PreviewContext.instance.DrawApplyAndRevert(Target);
            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrawerRelativeTextureSelector(SerializedProperty s_RelativeTextureSelector)
        {
            var S_TargetRenderer = s_RelativeTextureSelector.FindPropertyRelative("TargetRenderer");
            TextureTransformerEditor.DrawerObjectReference<Renderer>(S_TargetRenderer, null, TextureTransformerEditor.RendererFiltering);


            var S_MaterialSelect = s_RelativeTextureSelector.FindPropertyRelative("MaterialSelect");

            var TargetRenderer = S_TargetRenderer.objectReferenceValue as Renderer;
            var TargetMaterials = TargetRenderer?.sharedMaterials;

            S_MaterialSelect.intValue = ArraySelector(S_MaterialSelect.intValue, TargetMaterials);

            var S_TargetPropertyName = s_RelativeTextureSelector.FindPropertyRelative("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(S_TargetPropertyName);
            if (TargetMaterials != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ReplaceTexturePreview".GetLocalize());
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(64f)), TargetMaterials[S_MaterialSelect.intValue].GetTexture(S_TargetPropertyName.FindPropertyRelative("_propertyName").stringValue) as Texture2D, ScaleMode.ScaleToFit);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
        }

        public static int ArraySelector<T>(int Select, T[] Array) where T : UnityEngine.Object
        {
            if (Array == null) return Select;
            int SelectCount = 0;
            int DistSelect = Select;
            int NewSelect = Select;
            foreach (var ArrayValue in Array)
            {
                EditorGUILayout.BeginHorizontal();

                if (EditorGUILayout.Toggle(SelectCount == Select, GUILayout.Width(20)) && DistSelect != SelectCount) NewSelect = SelectCount;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(ArrayValue, typeof(Material), true);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                SelectCount += 1;
            }
            return NewSelect;
        }
        public static void DrawerSummary(TextureBlender target)
        {
            var s_obj = new SerializedObject(target);
            var s_TargetRenderer = s_obj.FindProperty("TargetRenderer");
            EditorGUILayout.PropertyField(s_TargetRenderer);
            var s_BlendTexture = s_obj.FindProperty("BlendTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(s_BlendTexture);

            s_obj.ApplyModifiedProperties();
        }
    }
}
#endif

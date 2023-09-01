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
            var Target = target as TextureBlender;
            var This_S_Object = serializedObject;

            EditorGUI.BeginDisabledGroup(Target.IsApply);
            var S_TargetRenderer = This_S_Object.FindProperty("TargetRenderer");
            TextureTransformerEditor.DrawerObjectReference<Renderer>(S_TargetRenderer, TextureTransformerEditor.RendererFiltering);


            var S_MaterialSelect = This_S_Object.FindProperty("MaterialSelect");

            var TargetRenderer = S_TargetRenderer.objectReferenceValue as Renderer;
            var TargetMaterials = TargetRenderer?.sharedMaterials;

            var MaterialSelect = S_MaterialSelect.intValue;
            S_MaterialSelect.intValue = ArraySelector(MaterialSelect, TargetMaterials);


            var S_BlendTexture = This_S_Object.FindProperty("BlendTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(S_BlendTexture);

            var S_Color = This_S_Object.FindProperty("Color");
            EditorGUILayout.PropertyField(S_Color);

            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType);


            var S_TargetPropertyName = This_S_Object.FindProperty("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(S_TargetPropertyName);
            EditorGUI.EndDisabledGroup();


            TextureTransformerEditor.DrawerApplyAndRevert(Target);
            This_S_Object.ApplyModifiedProperties();
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
    }
}
#endif

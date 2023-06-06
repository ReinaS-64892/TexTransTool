#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
namespace Rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureBlender))]
    public class TextureBlenderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var Target = target as TextureBlender;
            var This_S_Object = serializedObject;

            var S_TargetRendare = This_S_Object.FindProperty("TargetRenderer");
            TextureTransformerEditor.objectReferencePorpty<Renderer>(S_TargetRendare, TextureTransformerEditor.RendererFiltaling);


            var S_MaterialSelect = This_S_Object.FindProperty("MaterialSelect");

            var TargetRendare = S_TargetRendare.objectReferenceValue as Renderer;
            var TargetMaterials = TargetRendare?.sharedMaterials;

            var MaterialSelect = S_MaterialSelect.intValue;
            S_MaterialSelect.intValue = ArraySelector(MaterialSelect, TargetMaterials);


            var S_BlendTexture = This_S_Object.FindProperty("BlendTexture");
            TextureTransformerEditor.objectReferencePorpty<Texture2D>(S_BlendTexture);


            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType);


            var S_TargetPropatyName = This_S_Object.FindProperty("TargetPropatyName");
            EditorGUILayout.PropertyField(S_TargetPropatyName);


            TextureTransformerEditor.DrowApplyAndRevart(Target);
            This_S_Object.ApplyModifiedProperties();
        }

        public static int ArraySelector<T>(int Select, T[] Array) where T : UnityEngine.Object
        {
            if (Array == null) return Select;
            int SelecCount = 0;
            int DistSelect = Select;
            int NewSelect = Select;
            foreach (var ArryValue in Array)
            {
                EditorGUILayout.BeginHorizontal();

                if (EditorGUILayout.Toggle(SelecCount == Select, GUILayout.Width(20)) && DistSelect != SelecCount) NewSelect = SelecCount;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(ArryValue, typeof(Material), true);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                SelecCount += 1;
            }
            return NewSelect;
        }
    }
}
#endif
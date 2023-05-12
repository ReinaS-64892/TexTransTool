#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
namespace Rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureTransformer))]
    public class TextureTransformerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var Target = target as TextureTransformer;
            TextureTransformerEditorDrow(Target);
        }
        public static void TextureTransformerEditorDrow(TextureTransformer Target)
        {
            EditorGUI.BeginDisabledGroup(!Target.IsPossibleAppry);
            {
                if (!Target.IsAppry)
                {
                    if (GUILayout.Button("Appry"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Appry");
                        Target.Appry();
                    }
                }
                else
                {
                    if (GUILayout.Button("Revart"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Revart");
                        Target.Revart();

                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!(Target.IsPossibleCompile && !Target.IsAppry));
            if (GUILayout.Button("Compile"))
            {
                Undo.RecordObject(Target, "TextureTransformer - Compile");
                Target.Compile();
            }
            EditorGUI.EndDisabledGroup();
        }
        public static void objectReferenceEditActionAndPorpty<T>(SerializedProperty Prop, Action<T> EditoCollBack) where T : UnityEngine.Object
        {
            var valu = Prop.objectReferenceValue as T;
            var Editvalu = EditorGUILayout.ObjectField(Prop.name, valu, typeof(T), true) as T;
            if (valu != Editvalu)
            {
                EditoCollBack.Invoke(Editvalu);
                Prop.objectReferenceValue = Editvalu;
            }
        }
    }
}
#endif
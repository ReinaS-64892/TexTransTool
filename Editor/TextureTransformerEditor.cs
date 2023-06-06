#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

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
            DrowApplyAndRevart(Target);
            DrowCompile(Target);
        }
        public static void DrowApplyAndRevart(TextureTransformer Target)
        {
            if (Target == null) return;
            EditorGUI.BeginDisabledGroup(!Target.IsPossibleApply);
            {
                if (!Target.IsApply)
                {
                    if (GUILayout.Button("Apply"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Apply");
                        Target.Apply();
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
        }
        public static void DrowCompile(TextureTransformer Target)
        {
            if (Target == null) return;
            EditorGUI.BeginDisabledGroup(!(Target.IsPossibleCompile && !Target.IsApply));
            if (GUILayout.Button("Compile"))
            {
                Undo.RecordObject(Target, "TextureTransformer - Compile");
                Target.Compile();
            }
            EditorGUI.EndDisabledGroup();
        }
        public static void objectReferencePorpty<T>(SerializedProperty Prop, Action<T> EditCollBack) where T : UnityEngine.Object
        {
            var valu = Prop.objectReferenceValue as T;
            var Editvalu = EditorGUILayout.ObjectField(Prop.name, valu, typeof(T), true) as T;
            if (valu != Editvalu)
            {
                EditCollBack.Invoke(Editvalu);
                Prop.objectReferenceValue = Editvalu;
            }
        }
        public delegate T Filter<T>(T Target);
        public static void objectReferencePorpty<T>(SerializedProperty Prop, Filter<T> EditAndFilterCollBack) where T : UnityEngine.Object
        {
            var valu = Prop.objectReferenceValue as T;
            var Editvalu = EditorGUILayout.ObjectField(Prop.name, valu, typeof(T), true) as T;
            if (valu != Editvalu)
            {
                Prop.objectReferenceValue = EditAndFilterCollBack.Invoke(Editvalu);
            }
        }
        public static void objectReferencePorpty<T>(SerializedProperty Prop) where T : UnityEngine.Object
        {
            Prop.objectReferenceValue = EditorGUILayout.ObjectField(Prop.name, Prop.objectReferenceValue, typeof(T), true) as T;
        }

        public static Renderer RendererFiltaling(Renderer TargetRendererEditValue)
        {
            Renderer FiltalingdRendarer;
            if (TargetRendererEditValue is SkinnedMeshRenderer || TargetRendererEditValue is MeshRenderer)
            {
                FiltalingdRendarer = TargetRendererEditValue;
            }
            else
            {
                FiltalingdRendarer = null;
            }

            return FiltalingdRendarer;
        }

        public static IEnumerable<Renderer> RendererFiltaling(IEnumerable<Renderer> TargetRenderers)
        {

            return TargetRenderers.Where(Renderer => RendererFiltaling(Renderer) != null);
        }

    }
}
#endif
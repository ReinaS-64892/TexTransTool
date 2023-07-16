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
        public static void DrowProperty(SerializedProperty Prop, Action<bool> EditCollBack = null)
        {
            var Vulue = Prop.boolValue;
            var EditVulue = EditorGUILayout.Toggle(Prop.displayName, Vulue);
            if (EditCollBack != null && Vulue != EditVulue) { EditCollBack.Invoke(EditVulue); };
        }
        public static void DrowProperty(string label, bool Prop, Action<bool> EditCollBack = null)
        {
            var Vulue = Prop;
            var EditVulue = EditorGUILayout.Toggle(label, Vulue);
            if (EditCollBack != null && Vulue != EditVulue) { EditCollBack.Invoke(EditVulue); };
        }
        public static void DrowProperty(SerializedProperty Prop, Action<float> EditCollBack = null)
        {
            var Vulue = Prop.floatValue;
            var EditVulue = EditorGUILayout.FloatField(Prop.displayName, Vulue);
            if (EditCollBack != null && Vulue != EditVulue) { EditCollBack.Invoke(EditVulue); };
        }
        public static void DrowProperty(string label, float Prop, Action<float> EditCollBack = null)
        {
            var Vulue = Prop;
            var EditVulue = EditorGUILayout.FloatField(label, Vulue);
            if (EditCollBack != null && Vulue != EditVulue) { EditCollBack.Invoke(EditVulue); };
        }
        public static void DrowProperty(SerializedProperty Prop, Action<Vector2> EditCollBack = null)
        {
            var Vulue = Prop.vector2Value;
            var EditVulue = EditorGUILayout.Vector2Field(Prop.displayName, Vulue);
            if (EditCollBack != null && Vulue != EditVulue) { EditCollBack.Invoke(EditVulue); };
        }
        public static void DrowProperty(string label, Vector2 Prop, Action<Vector2> EditCollBack = null)
        {
            var Vulue = Prop;
            var EditVulue = EditorGUILayout.Vector2Field(label, Vulue);
            if (EditCollBack != null && Vulue != EditVulue) { EditCollBack.Invoke(EditVulue); };
        }

        public static void ObjectReferencePorpty<T>(SerializedProperty Prop, Action<T> EditCollBack) where T : UnityEngine.Object
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
        public static void ObjectReferencePorpty<T>(SerializedProperty Prop, Filter<T> EditAndFilterCollBack) where T : UnityEngine.Object
        {
            var valu = Prop.objectReferenceValue as T;
            var Editvalu = EditorGUILayout.ObjectField(Prop.name, valu, typeof(T), true) as T;
            if (valu != Editvalu)
            {
                Prop.objectReferenceValue = EditAndFilterCollBack.Invoke(Editvalu);
            }
        }
        public static void ObjectReferencePorpty<T>(SerializedProperty Prop) where T : UnityEngine.Object
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
        public static void DrowArryResizeButton(SerializedProperty ArrayPorpatye)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) ArrayPorpatye.arraySize += 1;
            EditorGUI.BeginDisabledGroup(ArrayPorpatye.arraySize <= 1);
            if (GUILayout.Button("-")) ArrayPorpatye.arraySize -= 1;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        public static void DorwRendarar(SerializedProperty RendererListSP, bool MultiRendararMode)
        {

            if (!MultiRendararMode)
            {
                RendererListSP.arraySize = 1;
                var S_TRArryElemt = RendererListSP.GetArrayElementAtIndex(0);
                var TRArryElemetValue = S_TRArryElemt.objectReferenceValue;
                var TRArryElemetEditValue = EditorGUILayout.ObjectField("TargetRenderer", TRArryElemetValue, typeof(Renderer), true) as Renderer;
                if (TRArryElemetValue != TRArryElemetEditValue)
                {
                    Renderer FiltalingdRendarer = TextureTransformerEditor.RendererFiltaling(TRArryElemetEditValue);
                    S_TRArryElemt.objectReferenceValue = FiltalingdRendarer;
                }
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderer");
                foreach (var Index in Enumerable.Range(0, RendererListSP.arraySize))
                {
                    var S_TargetRendererValue = RendererListSP.GetArrayElementAtIndex(Index);
                    var TargetRendererValue = S_TargetRendererValue.objectReferenceValue;
                    var TargetRendererEditValue = EditorGUILayout.ObjectField("Target " + (Index + 1), TargetRendererValue, typeof(Renderer), true) as Renderer;
                    if (TargetRendererValue != TargetRendererEditValue)
                    {
                        Renderer FiltalingdRendarer = TextureTransformerEditor.RendererFiltaling(TargetRendererEditValue);
                        S_TargetRendererValue.objectReferenceValue = FiltalingdRendarer;
                    }
                }

                DrowArryResizeButton(RendererListSP);
            }
        }

    }
}
#endif
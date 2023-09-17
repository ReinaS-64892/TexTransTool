#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureTransformer))]
    public class TextureTransformerEditor : UnityEditor.Editor
    {
        public static Renderer RendererFiltering(Renderer TargetRendererEditValue)
        {
            Renderer FilteredRenderer;
            if (TargetRendererEditValue is SkinnedMeshRenderer || TargetRendererEditValue is MeshRenderer)
            {
                FilteredRenderer = TargetRendererEditValue;
            }
            else
            {
                FilteredRenderer = null;
            }

            return FilteredRenderer;
        }
        public static void DrawerArrayResizeButton(SerializedProperty ArrayProperty, bool AllowZero = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) ArrayProperty.arraySize += 1;
            EditorGUI.BeginDisabledGroup(ArrayProperty.arraySize <= (AllowZero ? 0 : 1));
            if (GUILayout.Button("-")) ArrayProperty.arraySize -= 1;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawerRenderer(SerializedProperty RendererListSP, bool MultiRendererMode)
        {

            if (!MultiRendererMode)
            {
                RendererListSP.arraySize = 1;
                var S_TRArrayElement = RendererListSP.GetArrayElementAtIndex(0);
                var TRArrayElementValue = S_TRArrayElement.objectReferenceValue;
                var TRArrayElementEditValue = EditorGUILayout.ObjectField("TargetRenderer", TRArrayElementValue, typeof(Renderer), true) as Renderer;
                if (TRArrayElementValue != TRArrayElementEditValue)
                {
                    Renderer FlatlingRenderer = TextureTransformerEditor.RendererFiltering(TRArrayElementEditValue);
                    S_TRArrayElement.objectReferenceValue = FlatlingRenderer;
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
                        Renderer FilteredRenderer = TextureTransformerEditor.RendererFiltering(TargetRendererEditValue);
                        S_TargetRendererValue.objectReferenceValue = FilteredRenderer;
                    }
                }

                DrawerArrayResizeButton(RendererListSP);
            }
        }

        #region DrawerProperty
        public static void DrawerProperty(SerializedProperty Prop, Action<bool> EditCollBack = null)
        {
            var Value = Prop.boolValue;
            var EditValue = EditorGUILayout.Toggle(Prop.displayName, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(string label, bool Prop, Action<bool> EditCollBack = null)
        {
            var Value = Prop;
            var EditValue = EditorGUILayout.Toggle(label, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(SerializedProperty Prop, Action<float> EditCollBack = null, bool WithoutLabel = false)
        {
            var Value = Prop.floatValue;
            var EditValue = WithoutLabel ? EditorGUILayout.FloatField(Value) : EditorGUILayout.FloatField(Prop.displayName, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(string label, float Prop, Action<float> EditCollBack = null)
        {
            var Value = Prop;
            var EditValue = EditorGUILayout.FloatField(label, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(SerializedProperty Prop, Action<Vector2> EditCollBack = null)
        {
            var Value = Prop.vector2Value;
            var EditValue = EditorGUILayout.Vector2Field(Prop.displayName, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(string label, Vector2 Prop, Action<Vector2> EditCollBack = null)
        {
            var Value = Prop;
            var EditValue = EditorGUILayout.Vector2Field(label, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(SerializedProperty Prop, Action<int> EditCollBack = null, bool WithoutLabel = false)
        {
            var Value = Prop.intValue;
            var EditValue = WithoutLabel ? EditorGUILayout.IntField(Value) : EditorGUILayout.IntField(Prop.displayName, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerProperty(string label, int Prop, Action<int> EditCollBack = null)
        {
            var Value = Prop;
            var EditValue = EditorGUILayout.IntField(label, Value);
            if (EditCollBack != null && Value != EditValue) { EditCollBack.Invoke(EditValue); };
        }
        public static void DrawerObjectReference<T>(SerializedProperty Prop, Action<T> EditCollBack) where T : UnityEngine.Object
        {
            var Value = Prop.objectReferenceValue as T;
            var EditValue = EditorGUILayout.ObjectField(Prop.name, Value, typeof(T), true) as T;
            if (Value != EditValue)
            {
                EditCollBack.Invoke(EditValue);
            }
        }
        public delegate T Filter<T>(T Target);
        public static void DrawerObjectReference<T>(SerializedProperty Prop, Filter<T> EditAndFilterCollBack) where T : UnityEngine.Object
        {
            var Value = Prop.objectReferenceValue as T;
            var EditValue = EditorGUILayout.ObjectField(Prop.name, Value, typeof(T), true) as T;
            if (Value != EditValue)
            {
                Prop.objectReferenceValue = EditAndFilterCollBack.Invoke(EditValue);
            }
        }
        public static void DrawerObjectReference<T>(SerializedProperty Prop) where T : UnityEngine.Object
        {
            Prop.objectReferenceValue = EditorGUILayout.ObjectField(Prop.name, Prop.objectReferenceValue, typeof(T), true) as T;
        }

        #endregion

        public static void DrawerTargetRenderersSummary(SerializedProperty s_TargetRenderers)
        {
            if (s_TargetRenderers.arraySize == 1)
            {
                var s_rd = s_TargetRenderers.GetArrayElementAtIndex(0);
                EditorGUILayout.PropertyField(s_rd, new GUIContent("TargetRenderers"));
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderers");
                for (var i = 0; s_TargetRenderers.arraySize > i; i += 1)
                {
                    var s_rd = s_TargetRenderers.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(s_rd, GUIContent.none);
                }
            }
        }
    }
}
#endif

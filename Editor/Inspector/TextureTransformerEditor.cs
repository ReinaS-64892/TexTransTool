#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Editor.Decal;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureTransformer))]
    public class TextureTransformerEditor : UnityEditor.Editor
    {
        public static void DrawerWarning(string TypeName)
        {
            EditorGUILayout.HelpBox(TypeName + "is an experimental feature. Features may be changed or removed without notice.".GetLocalize(), MessageType.Warning);
        }
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
                var TRArrayElementEditValue = EditorGUILayout.ObjectField("TargetRenderer".GetLocalize(), TRArrayElementValue, typeof(Renderer), true) as Renderer;
                if (TRArrayElementValue != TRArrayElementEditValue)
                {
                    Renderer FlatlingRenderer = TextureTransformerEditor.RendererFiltering(TRArrayElementEditValue);
                    S_TRArrayElement.objectReferenceValue = FlatlingRenderer;
                }
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderer".GetLocalize());
                foreach (var Index in Enumerable.Range(0, RendererListSP.arraySize))
                {
                    var S_TargetRendererValue = RendererListSP.GetArrayElementAtIndex(Index);
                    var TargetRendererValue = S_TargetRendererValue.objectReferenceValue;
                    var TargetRendererEditValue = EditorGUILayout.ObjectField("Target".GetLocalize() + " " + (Index + 1), TargetRendererValue, typeof(Renderer), true) as Renderer;
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

        public delegate T Filter<T>(T Target);
        public static void DrawerPropertyBool(SerializedProperty Prop, GUIContent gUIContent = null, Filter<bool> EditAndFilterCollBack = null)
        {
            var preValue = Prop.boolValue;
            EditorGUILayout.PropertyField(Prop, gUIContent != null ? gUIContent : new GUIContent(Prop.displayName));
            var postValue = Prop.boolValue;
            if (EditAndFilterCollBack != null && preValue != postValue) { Prop.boolValue = EditAndFilterCollBack.Invoke(postValue); }
        }
        public static void DrawerPropertyFloat(SerializedProperty Prop, GUIContent gUIContent = null, Filter<float> EditAndFilterCollBack = null)
        {
            var preValue = Prop.floatValue;
            EditorGUILayout.PropertyField(Prop, gUIContent != null ? gUIContent : new GUIContent(Prop.displayName));
            var postValue = Prop.floatValue;
            if (EditAndFilterCollBack != null && !Mathf.Approximately(preValue, postValue)) { Prop.floatValue = EditAndFilterCollBack.Invoke(postValue); }
        }
        public static void DrawerTexture2D(SerializedProperty Prop, GUIContent gUIContent = null, Filter<Texture2D> EditAndFilterCollBack = null, float PreviewTextureSize = 64f)
        {
            var Value = Prop.objectReferenceValue as Texture2D;
            if (Value != null) { EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(PreviewTextureSize)), Value, ScaleMode.ScaleToFit); }
            EditorGUILayout.PropertyField(Prop, gUIContent != null ? gUIContent : Prop.displayName.GetLC());
            if (EditAndFilterCollBack != null && Prop.objectReferenceValue != Value)
            {
                Prop.objectReferenceValue = EditAndFilterCollBack.Invoke(Prop.objectReferenceValue as Texture2D);
            }
        }
        public static void DrawerObjectReference<T>(SerializedProperty Prop, GUIContent gUIContent = null, Filter<T> EditAndFilterCollBack = null) where T : UnityEngine.Object
        {
            var Value = Prop.objectReferenceValue as T;
            EditorGUILayout.PropertyField(Prop, gUIContent != null ? gUIContent : Prop.displayName.GetLC());
            if (EditAndFilterCollBack != null && Prop.objectReferenceValue != Value)
            {
                Prop.objectReferenceValue = EditAndFilterCollBack.Invoke(Prop.objectReferenceValue as T);
            }
        }

        #endregion

        public static void DrawerTargetRenderersSummary(SerializedProperty s_TargetRenderers)
        {
            if (s_TargetRenderers.arraySize == 1)
            {
                var s_rd = s_TargetRenderers.GetArrayElementAtIndex(0);
                EditorGUILayout.PropertyField(s_rd, "TargetRenderer".GetLC());
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderer".GetLocalize());
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

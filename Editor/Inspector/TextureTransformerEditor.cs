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
    internal class TextureTransformerEditor : UnityEditor.Editor
    {
        public static void DrawerWarning(string typeName)
        {
            EditorGUILayout.HelpBox(typeName + "is an experimental feature. Features may be changed or removed without notice.".GetLocalize(), MessageType.Warning);
        }
        public static Renderer RendererFiltering(Renderer targetRendererEditValue)
        {
            Renderer FilteredRenderer;
            if (targetRendererEditValue is SkinnedMeshRenderer || targetRendererEditValue is MeshRenderer)
            {
                FilteredRenderer = targetRendererEditValue;
            }
            else
            {
                FilteredRenderer = null;
            }

            return FilteredRenderer;
        }
        public static void DrawerArrayResizeButton(SerializedProperty arrayProperty, bool allowZero = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) arrayProperty.arraySize += 1;
            EditorGUI.BeginDisabledGroup(arrayProperty.arraySize <= (allowZero ? 0 : 1));
            if (GUILayout.Button("-")) arrayProperty.arraySize -= 1;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawerRenderer(SerializedProperty sRendererList, bool multiRendererMode)
        {

            if (!multiRendererMode)
            {
                sRendererList.arraySize = 1;
                var sArrayElement = sRendererList.GetArrayElementAtIndex(0);
                var arrayElementValue = sArrayElement.objectReferenceValue;
                var arrayElementEditValue = EditorGUILayout.ObjectField("TargetRenderer".GetLocalize(), arrayElementValue, typeof(Renderer), true) as Renderer;
                if (arrayElementValue != arrayElementEditValue)
                {
                    Renderer flatlingRenderer = TextureTransformerEditor.RendererFiltering(arrayElementEditValue);
                    sArrayElement.objectReferenceValue = flatlingRenderer;
                }
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderer".GetLocalize());
                foreach (var Index in Enumerable.Range(0, sRendererList.arraySize))
                {
                    var sTargetRendererValue = sRendererList.GetArrayElementAtIndex(Index);
                    var targetRendererValue = sTargetRendererValue.objectReferenceValue;
                    var targetRendererEditValue = EditorGUILayout.ObjectField("Target".GetLocalize() + " " + (Index + 1), targetRendererValue, typeof(Renderer), true) as Renderer;
                    if (targetRendererValue != targetRendererEditValue)
                    {
                        Renderer filteredRenderer = TextureTransformerEditor.RendererFiltering(targetRendererEditValue);
                        sTargetRendererValue.objectReferenceValue = filteredRenderer;
                    }
                }

                DrawerArrayResizeButton(sRendererList);
            }
        }

        #region DrawerProperty

        public delegate T Filter<T>(T Target);
        public static void DrawerPropertyBool(SerializedProperty prop, GUIContent gUIContent = null, Filter<bool> editAndFilterCollBack = null)
        {
            var preValue = prop.boolValue;
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : new GUIContent(prop.displayName));
            var postValue = prop.boolValue;
            if (editAndFilterCollBack != null && preValue != postValue) { prop.boolValue = editAndFilterCollBack.Invoke(postValue); }
        }
        public static void DrawerPropertyFloat(SerializedProperty prop, GUIContent gUIContent = null, Filter<float> editAndFilterCollBack = null)
        {
            var preValue = prop.floatValue;
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : new GUIContent(prop.displayName));
            var postValue = prop.floatValue;
            if (editAndFilterCollBack != null && !Mathf.Approximately(preValue, postValue)) { prop.floatValue = editAndFilterCollBack.Invoke(postValue); }
        }
        public static void DrawerTexture2D(SerializedProperty prop, GUIContent gUIContent = null, Filter<Texture2D> editAndFilterCollBack = null, float PreviewTextureSize = 64f)
        {
            var Value = prop.objectReferenceValue as Texture2D;
            if (Value != null) { EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(PreviewTextureSize)), Value, ScaleMode.ScaleToFit); }
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : prop.displayName.GetLC());
            if (editAndFilterCollBack != null && prop.objectReferenceValue != Value)
            {
                prop.objectReferenceValue = editAndFilterCollBack.Invoke(prop.objectReferenceValue as Texture2D);
            }
        }
        public static void DrawerObjectReference<T>(SerializedProperty prop, GUIContent gUIContent = null, Filter<T> editAndFilterCollBack = null) where T : UnityEngine.Object
        {
            var Value = prop.objectReferenceValue as T;
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : prop.name.GetLC());
            if (editAndFilterCollBack != null && prop.objectReferenceValue != Value)
            {
                prop.objectReferenceValue = editAndFilterCollBack.Invoke(prop.objectReferenceValue as T);
            }
        }

        #endregion

        public static void DrawerTargetRenderersSummary(SerializedProperty sTargetRenderers)
        {
            if (sTargetRenderers.arraySize == 1)
            {
                var srd = sTargetRenderers.GetArrayElementAtIndex(0);
                EditorGUILayout.PropertyField(srd, "TargetRenderer".GetLC());
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderer".GetLocalize());
                for (var i = 0; sTargetRenderers.arraySize > i; i += 1)
                {
                    var srd = sTargetRenderers.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(srd, GUIContent.none);
                }
            }
        }
    }
}
#endif

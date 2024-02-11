using UnityEngine;
using UnityEditor;
using System.Linq;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TexTransBehavior), true)]
    internal class TextureTransformerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawerWarning(target.GetType().Name);
            base.OnInspectorGUI();
            PreviewContext.instance.DrawApplyAndRevert(target as TexTransBehavior);
        }
        public static void DrawerWarning(string typeName)
        {
            EditorGUILayout.HelpBox(typeName + " " + "Common:ExperimentalWarning".GetLocalize(), MessageType.Warning);
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

        public static void DrawerRenderer(SerializedProperty sRendererList, GUIContent label, bool multiRendererMode)
        {

            if (!multiRendererMode)
            {
                sRendererList.arraySize = 1;
                var sArrayElement = sRendererList.GetArrayElementAtIndex(0);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sArrayElement, label);

                if (EditorGUI.EndChangeCheck())
                {
                    Renderer flatlingRenderer = TextureTransformerEditor.RendererFiltering(sArrayElement.objectReferenceValue as Renderer);
                    sArrayElement.objectReferenceValue = flatlingRenderer;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sRendererList, label);

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (SerializedProperty sTargetRendererValue in sRendererList)
                    {
                        Renderer filteredRenderer = TextureTransformerEditor.RendererFiltering(sTargetRendererValue.objectReferenceValue as Renderer);
                        sTargetRendererValue.objectReferenceValue = filteredRenderer;
                    }
                }

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
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : prop.displayName.Glc());
            if (editAndFilterCollBack != null && prop.objectReferenceValue != Value)
            {
                prop.objectReferenceValue = editAndFilterCollBack.Invoke(prop.objectReferenceValue as Texture2D);
            }
        }
        public static void DrawerObjectReference<T>(SerializedProperty prop, GUIContent gUIContent = null, Filter<T> editAndFilterCollBack = null) where T : UnityEngine.Object
        {
            var Value = prop.objectReferenceValue as T;
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : prop.name.Glc());
            if (editAndFilterCollBack != null && prop.objectReferenceValue != Value)
            {
                prop.objectReferenceValue = editAndFilterCollBack.Invoke(prop.objectReferenceValue as T);
            }
        }

        #endregion

        public static void DrawerTargetRenderersSummary(SerializedProperty sTargetRenderers, GUIContent gUIContent)
        {
            if (sTargetRenderers.arraySize == 1)
            {
                var srd = sTargetRenderers.GetArrayElementAtIndex(0);
                EditorGUILayout.PropertyField(srd, gUIContent);
            }
            else
            {
                EditorGUILayout.LabelField(gUIContent);
                for (var i = 0; sTargetRenderers.arraySize > i; i += 1)
                {
                    var srd = sTargetRenderers.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(srd, GUIContent.none);
                }
            }
        }
    }
}

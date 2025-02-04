using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.Preview.RealTime;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TexTransMonoBase), true)]
    internal class TextureTransformerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawerWarning(target.GetType().Name);
            base.OnInspectorGUI();
            PreviewButtonDrawUtil.Draw(target as TexTransMonoBase);
        }
        public static void DrawOldSaveDataVersionWarning(TexTransMonoBase ttMonoBase)
        {
            if ((ttMonoBase as ITexTransToolTag).SaveDataVersion < TexTransMonoBase.TTTDataVersion)
                if (GUILayout.Button("Common:button:ThisComponentSaveDataIsOldOpenMigratorWindow".Glc()))
                    Migration.MigratorWindow.ShowWindow();
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
        public static void DrawerRealTimePreviewEditorButton(TexTransRuntimeBehavior texTransRuntimeBehavior)
        {
            if (texTransRuntimeBehavior == null) { return; }
            var rpm = RealTimePreviewContext.instance;
            if (!rpm.IsPreview())
            {
                bool IsPossibleRealTimePreview = !OneTimePreviewContext.IsPreviewContains;
                IsPossibleRealTimePreview &= !AnimationMode.InAnimationMode();
                IsPossibleRealTimePreview |= rpm.IsPreview();

                EditorGUI.BeginDisabledGroup(!IsPossibleRealTimePreview);
                if (GUILayout.Button(IsPossibleRealTimePreview ? "SimpleDecal:button:RealTimePreview".Glc() : "Common:PreviewNotAvailable".Glc()))
                {
                    var domainRoot = DomainMarkerFinder.FindMarker(texTransRuntimeBehavior.gameObject);
                    if (domainRoot != null)
                    {
                        rpm.EnterRealtimePreview(domainRoot);
                    }
                    else
                    {
                        Debug.Log("Domain not found");
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                // EditorGUILayout.LabelField(RealTimePreviewManager.instance.LastDecalUpdateTime + "ms", GUILayout.Width(40));

                if (GUILayout.Button("SimpleDecal:button:ExitRealTimePreview".Glc()))
                {
                    rpm.ExitRealTimePreview();
                }
                EditorGUILayout.EndHorizontal();
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

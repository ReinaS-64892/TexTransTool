using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool.Editor.OtherMenuItem;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(SimpleDecalExperimentalFeature), true)]
    internal class SimpleDecalExperimentalFeatureEditor : UnityEditor.Editor
    {

        CanBehaveAsLayerEditorUtil behaveLayerUtil;
        void BehaveUtilInit() { behaveLayerUtil = new(target as Component); }
        void OnEnable() { BehaveUtilInit(); EditorApplication.hierarchyChanged += BehaveUtilInit; }
        void OnDisable() { EditorApplication.hierarchyChanged -= BehaveUtilInit; }
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            var thisSObj = serializedObject;
            thisSObj.Update();


            var sOverrideDecalTextureWithMultiLayerImageCanvas = thisSObj.FindProperty("OverrideDecalTextureWithMultiLayerImageCanvas");
            if (behaveLayerUtil.IsLayerMode is false) EditorGUILayout.PropertyField(sOverrideDecalTextureWithMultiLayerImageCanvas);

            var sUseDepth = thisSObj.FindProperty("UseDepth");
            var sDepthInvert = thisSObj.FindProperty("DepthInvert");
            EditorGUILayout.PropertyField(sUseDepth, "SimpleDecal:prop:ExperimentalFuture:UseDepth".Glc());
            if (sUseDepth.boolValue) { EditorGUILayout.PropertyField(sDepthInvert, "SimpleDecal:prop:ExperimentalFuture:DepthInvert".Glc()); }

        }
    }
}

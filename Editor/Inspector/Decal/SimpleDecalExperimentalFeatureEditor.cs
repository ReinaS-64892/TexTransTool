using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool.Editor.OtherMenuItem;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(SimpleDecalExperimentalFeature), true)]
    internal class SimpleDecalExperimentalFeatureEditor : TTCanBehaveAsLayerEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var thisSObj = serializedObject;

            var sOverrideDecalTextureWithMultiLayerImageCanvas = thisSObj.FindProperty(nameof(SimpleDecalExperimentalFeature.OverrideDecalTextureWithMultiLayerImageCanvas));
            if (IsLayerMode is false) EditorGUILayout.PropertyField(sOverrideDecalTextureWithMultiLayerImageCanvas);

            var sUseDepth = thisSObj.FindProperty(nameof(SimpleDecalExperimentalFeature.UseDepth));
            var sDepthInvert = thisSObj.FindProperty(nameof(SimpleDecalExperimentalFeature.DepthInvert));
            EditorGUILayout.PropertyField(sUseDepth, "SimpleDecal:prop:ExperimentalFuture:UseDepth".Glc());
            if (sUseDepth.boolValue) { EditorGUILayout.PropertyField(sDepthInvert, "SimpleDecal:prop:ExperimentalFuture:DepthInvert".Glc()); }

        }
    }
}

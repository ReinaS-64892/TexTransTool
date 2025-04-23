using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool.Editor.OtherMenuItem;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTextureExperimentalFeature), true)]
    internal class AtlasTextureExperimentalFeatureEditor : TexTransMonoBaseEditor
    {

        protected override void OnTexTransComponentInspectorGUI()
        {
            var thisTarget = target as AtlasTextureExperimentalFeature;
            var thisSObj = serializedObject;

            var sTextureIndividualFineTuning = thisSObj.FindProperty(nameof(AtlasTextureExperimentalFeature.TextureIndividualFineTuning));
            var sUnsetTextures = thisSObj.FindProperty(nameof(AtlasTextureExperimentalFeature.UnsetTextures));

            var sAutoTextureSizeSetting = thisSObj.FindProperty(nameof(AtlasTextureExperimentalFeature.AutoTextureSizeSetting));
            var sAutoReferenceCopySetting = thisSObj.FindProperty(nameof(AtlasTextureExperimentalFeature.AutoReferenceCopySetting));
            var sAutoMergeTextureSetting = thisSObj.FindProperty(nameof(AtlasTextureExperimentalFeature.AutoMergeTextureSetting));

            EditorGUILayout.PropertyField(sUnsetTextures, "AtlasTexture:prop:ExperimentalFuture:UnsetTextures".GlcV());

            EditorGUILayout.PropertyField(sAutoTextureSizeSetting, "AtlasTexture:prop:ExperimentalFuture:AutoTextureSizeSetting".GlcV());
            EditorGUILayout.PropertyField(sAutoReferenceCopySetting, "AtlasTexture:prop:ExperimentalFuture:AutoReferenceCopySetting".GlcV());
            EditorGUILayout.PropertyField(sAutoMergeTextureSetting, "AtlasTexture:prop:ExperimentalFuture:AutoMergeTextureSetting".GlcV());

            EditorGUILayout.PropertyField(sTextureIndividualFineTuning, "AtlasTexture:prop:TextureIndividualFineTuning".GlcV());
            if (sTextureIndividualFineTuning.isExpanded is false && PreviewUtility.IsPreviewContains is false)
            {
                if (GUILayout.Button("AtlasTexture:prop:OpenTextureFineTuningManager".GlcV(), GUILayout.Height(18f)))
                {
                    var thisTargetAtlasTexture = thisTarget.GetComponent<AtlasTexture>();
                    TextureFineTuningManager.OpenAtlasTexture((thisTargetAtlasTexture, thisTarget));
                }
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTexture), true)]
    internal class AtlasTextureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            var thisTarget = target as AtlasTexture;
            var thisSObject = serializedObject;
            thisSObject.Update();

#pragma warning disable CS0612
            if (thisTarget is ITexTransToolTag TTTag && TTTag.SaveDataVersion != TexTransBehavior.TTTDataVersion)
            {
                if (TTTag.SaveDataVersion == 0 && GUILayout.Button("Migrate DSV0 To DSV1"))
                {
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.MigrationAtlasTextureV0ToV1(thisTarget);
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.FinalizeMigrationAtlasTextureV0ToV1(thisTarget);
                }
                if (TTTag.SaveDataVersion == 1 && GUILayout.Button("Migrate DSV1 To DSV2"))
                {
                    net.rs64.TexTransTool.Migration.V1.AtlasTextureV1.MigrationAtlasTextureV1ToV2(thisTarget);
                }
                if (TTTag.SaveDataVersion == 2 && GUILayout.Button("Migrate DSV2 To DSV3"))
                {
                    net.rs64.TexTransTool.Migration.V2.AtlasTextureV2.MigrationAtlasTextureV2ToV3(thisTarget);
                }
                return;
            }
#pragma warning restore CS0612

            var sAtlasSetting = thisSObject.FindProperty("AtlasSetting");
            var sTargetRoot = thisSObject.FindProperty("TargetRoot");
            var sMatSelectors = thisSObject.FindProperty("SelectMatList");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sTargetRoot, "AtlasTexture:prop:TargetRoot".Glc());
            if (EditorGUI.EndChangeCheck()) { RefreshMaterials(sTargetRoot.objectReferenceValue as GameObject, thisTarget.AtlasSetting.IncludeDisabledRenderer); }

            if (sTargetRoot.objectReferenceValue != null && !PreviewContext.IsPreviewContains)
            {
                if (GUILayout.Button("AtlasTexture:button:RefreshMaterials".GetLocalize()) || _displayMaterial == null)
                { RefreshMaterials(thisTarget.TargetRoot, thisTarget.AtlasSetting.IncludeDisabledRenderer); }
                MaterialSelectEditor(sMatSelectors, _displayMaterial);
            }

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(sMatSelectors, "AtlasTexture:prop:SelectedMaterialView".Glc());
            EditorGUI.indentLevel -= 1;


            DrawAtlasSettings(sAtlasSetting);

            PreviewContext.instance.DrawApplyAndRevert(thisTarget);

            serializedObject.ApplyModifiedProperties();

        }

        private static void DrawAtlasSettings(SerializedProperty sAtlasSettings)
        {
            EditorGUILayout.LabelField("AtlasTexture:label:AtlasSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var sAtlasTextureSize = sAtlasSettings.FindPropertyRelative("AtlasTextureSize");
            var sMergeMaterials = sAtlasSettings.FindPropertyRelative("MergeMaterials");
            var sMergeReferenceMaterial = sAtlasSettings.FindPropertyRelative("MergeReferenceMaterial");
            var sForceSetTexture = sAtlasSettings.FindPropertyRelative("ForceSetTexture");
            var sPropertyBakeSetting = sAtlasSettings.FindPropertyRelative("PropertyBakeSetting");
            var sPadding = sAtlasSettings.FindPropertyRelative("IslandPadding");
            var sAtlasIslandRelocator = sAtlasSettings.FindPropertyRelative("AtlasIslandRelocator");
            var sWriteOriginalUV = sAtlasSettings.FindPropertyRelative("WriteOriginalUV");
            var sIncludeDisabledRenderer = sAtlasSettings.FindPropertyRelative("IncludeDisabledRenderer");
            var sPixelNormalize = sAtlasSettings.FindPropertyRelative("PixelNormalize");
            var sUseUpScaling = sAtlasSettings.FindPropertyRelative("UseUpScaling");
            var sTextureFineTuning = sAtlasSettings.FindPropertyRelative("TextureFineTuning");



            EditorGUILayout.PropertyField(sAtlasTextureSize, "AtlasTexture:prop:AtlasTextureSize".Glc());
            EditorGUILayout.PropertyField(sMergeMaterials, "AtlasTexture:prop:MaterialMarge".Glc());
            if (sMergeMaterials.boolValue)
            {
                EditorGUILayout.PropertyField(sPropertyBakeSetting, "AtlasTexture:prop:PropertyBakeSetting".Glc());
                EditorGUILayout.PropertyField(sMergeReferenceMaterial, "AtlasTexture:prop:MergeReferenceMaterial".Glc());
            }
            EditorGUILayout.PropertyField(sForceSetTexture, "AtlasTexture:prop:ForceSetTexture".Glc());

            EditorGUILayout.PropertyField(sPadding, "AtlasTexture:prop:Padding".Glc());
            EditorGUILayout.PropertyField(sUseUpScaling, "AtlasTexture:prop:UseUpScaling".Glc());

            EditorGUILayout.PropertyField(sIncludeDisabledRenderer, "AtlasTexture:prop:IncludeDisabledRenderer".Glc());


            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Common:ExperimentalFuture".Glc());
            if (s_ExperimentalFutureOption)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(sAtlasIslandRelocator, "AtlasTexture:prop:ExperimentalFuture:AtlasIslandRelocator".Glc());
                EditorGUILayout.PropertyField(sWriteOriginalUV, "AtlasTexture:prop:ExperimentalFuture:WriteOriginalUV".Glc());
                EditorGUILayout.PropertyField(sPixelNormalize,"AtlasTexture:prop:ExperimentalFuture:PixelNormalize".Glc());
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.PropertyField(sTextureFineTuning, "AtlasTexture:prop:TextureFineTuning".Glc());


            EditorGUI.indentLevel -= 1;

        }

        static bool s_ExperimentalFutureOption = false;



        List<Material> _displayMaterial;
        void RefreshMaterials(GameObject targetRoot, bool includeDisabledRenderer)
        {
            if (targetRoot == null) { _displayMaterial = null; return; }
            var renderers = AtlasTexture.FilteredRenderers(targetRoot, includeDisabledRenderer);
            _displayMaterial = RendererUtility.GetMaterials(renderers).Distinct().ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty targetMaterial, List<Material> tempMaterial)
        {
            EditorGUI.indentLevel += 1;
            GUILayout.Label("AtlasTexture:label:TSM-Header".Glc());
            foreach (var mat in tempMaterial)
            {
                var sMatSelector = FindMatSelector(targetMaterial, mat);
                EditorGUILayout.BeginHorizontal();

                var isTarget = sMatSelector != null;

                var editIsTarget = EditorGUILayout.Toggle(isTarget);
                if (isTarget != editIsTarget)
                {
                    if (editIsTarget)
                    {
                        var index = targetMaterial.arraySize;
                        targetMaterial.arraySize += 1;
                        var sNewMatSelector = targetMaterial.GetArrayElementAtIndex(index);
                        sNewMatSelector.FindPropertyRelative("Material").objectReferenceValue = mat;
                        sNewMatSelector.FindPropertyRelative("AdditionalTextureSizeOffSet").floatValue = 1;
                    }
                    else
                    {
                        targetMaterial.DeleteArrayElementAtIndex(FindMatSelectorIndex(targetMaterial, mat));
                    }
                }
                else if (isTarget)
                {
                    var SOffset = sMatSelector.FindPropertyRelative("AdditionalTextureSizeOffSet");
                    SOffset.floatValue = EditorGUILayout.FloatField(SOffset.floatValue);
                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MaxWidth(1000));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

        }
        public static SerializedProperty FindMatSelector(SerializedProperty targetMaterialArray, Material material)
        {
            for (int i = 0; targetMaterialArray.arraySize > i; i += 1)
            {
                var SectorElement = targetMaterialArray.GetArrayElementAtIndex(i);
                var MatProp = SectorElement.FindPropertyRelative("Material");
                if (MatProp.objectReferenceValue == material)
                {
                    return SectorElement;
                }
            }
            return null;
        }
        public static int FindMatSelectorIndex(SerializedProperty targetMaterialArray, Material material)
        {
            for (int i = 0; targetMaterialArray.arraySize > i; i += 1)
            {
                var SectorElement = targetMaterialArray.GetArrayElementAtIndex(i);
                var MatProp = SectorElement.FindPropertyRelative("Material");
                if (MatProp.objectReferenceValue == material)
                {
                    return i;
                }
            }
            return -1;
        }
        [InitializeOnLoadMethod]
        internal static void RegisterSummary()
        {
            TexTransGroupEditor.s_summary[typeof(AtlasTexture)] = at =>
            {
                var ve = new VisualElement();
                var serializedObject = new SerializedObject(at);
                var sTargetRenderers = serializedObject.FindProperty("TargetRoot");
                var sAtlasTextureSize = serializedObject.FindProperty("AtlasSetting").FindPropertyRelative("AtlasTextureSize");

                var targetRoot = new PropertyField();
                targetRoot.label = "AtlasTexture:prop:TargetRoot".GetLocalize();
                targetRoot.BindProperty(sTargetRenderers);
                ve.hierarchy.Add(targetRoot);

                var atlasTextureSize = new PropertyField();
                atlasTextureSize.label = "AtlasTexture:prop:AtlasTextureSize".GetLocalize();
                atlasTextureSize.BindProperty(sAtlasTextureSize);
                ve.hierarchy.Add(atlasTextureSize);

                return ve;
            };
        }
    }
}

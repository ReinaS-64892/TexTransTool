using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTexture), true)]
    internal class AtlasTextureEditor : UnityEditor.Editor
    {
        private AtlasTexture thisTarget;
        private SerializedProperty sLimitCandidateMaterials, sMatSelectors, sAtlasSetting;

        private SerializedProperty sAtlasTextureSize, sCustomAspect, sAtlasTextureHeightSize;
        private SerializedProperty sAtlasTargetUVChannel;
        private SerializedProperty sMergeMaterialGroups, sAllMaterialMergeReference;
        private SerializedProperty sUsePrimaryMaximumTexture, sPrimaryTextureProperty;
        private SerializedProperty sPadding;
        private SerializedProperty sForceSetTexture;
        private SerializedProperty sForceSizePriority;
        private SerializedProperty sWriteOriginalUV, sOriginalUVWriteTargetChannel;
        private SerializedProperty sIncludeDisabledRenderer;
        private SerializedProperty sPixelNormalize;
        private SerializedProperty sTextureFineTuning, sIslandFineTuners, sTextureIndividualFineTuning;
        private SerializedProperty sAutoTextureSizeSetting, sAutoReferenceCopySetting, sAutoMergeTextureSetting;
        private SerializedProperty sBackGroundColor;
        private SerializedProperty sUnsetTextures;

        public void OnEnable()
        {
            thisTarget = target as AtlasTexture;
            var thisSObject = serializedObject;
            sAtlasSetting = thisSObject.FindProperty("AtlasSetting");

            sLimitCandidateMaterials = thisSObject.FindProperty("LimitCandidateMaterials");
            sMatSelectors = thisSObject.FindProperty("SelectMatList");
            sAtlasTargetUVChannel = sAtlasSetting.FindPropertyRelative("AtlasTargetUVChannel");

            sMergeMaterialGroups = thisSObject.FindProperty("MergeMaterialGroups");
            sAllMaterialMergeReference = thisSObject.FindProperty("AllMaterialMergeReference");



            sAtlasTextureSize = sAtlasSetting.FindPropertyRelative("AtlasTextureSize");
            sCustomAspect = sAtlasSetting.FindPropertyRelative("CustomAspect");
            sAtlasTextureHeightSize = sAtlasSetting.FindPropertyRelative("AtlasTextureHeightSize");


            sUsePrimaryMaximumTexture = sAtlasSetting.FindPropertyRelative("UsePrimaryMaximumTexture");
            sPrimaryTextureProperty = sAtlasSetting.FindPropertyRelative("PrimaryTextureProperty");

            sForceSetTexture = sAtlasSetting.FindPropertyRelative("ForceSetTexture");
            sPadding = sAtlasSetting.FindPropertyRelative("IslandPadding");
            sWriteOriginalUV = sAtlasSetting.FindPropertyRelative("WriteOriginalUV");
            sOriginalUVWriteTargetChannel = sAtlasSetting.FindPropertyRelative("OriginalUVWriteTargetChannel");
            sIncludeDisabledRenderer = sAtlasSetting.FindPropertyRelative("IncludeDisabledRenderer");
            sPixelNormalize = sAtlasSetting.FindPropertyRelative("PixelNormalize");
            sTextureFineTuning = sAtlasSetting.FindPropertyRelative("TextureFineTuning");
            sIslandFineTuners = sAtlasSetting.FindPropertyRelative("IslandFineTuners");
            sForceSizePriority = sAtlasSetting.FindPropertyRelative("ForceSizePriority");
            sTextureIndividualFineTuning = sAtlasSetting.FindPropertyRelative("TextureIndividualFineTuning");

            sAutoTextureSizeSetting = sAtlasSetting.FindPropertyRelative("AutoTextureSizeSetting");
            sAutoReferenceCopySetting = sAtlasSetting.FindPropertyRelative("AutoReferenceCopySetting");
            sAutoMergeTextureSetting = sAtlasSetting.FindPropertyRelative("AutoMergeTextureSetting");

            sBackGroundColor = sAtlasSetting.FindPropertyRelative("BackGroundColor");
            sUnsetTextures = sAtlasSetting.FindPropertyRelative("UnsetTextures");

        }
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            var thisSObject = serializedObject;
            thisSObject.Update();


            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(sLimitCandidateMaterials, "AtlasTexture:prop:LimitCandidateMaterials".Glc());
                if (cc.changed) RefreshMaterials();
            }

            if (PreviewUtility.IsPreviewContains is false)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (_displayMaterial is not null)
                    {
                        var l = new[] { GUILayout.MaxWidth(64f + 18f), GUILayout.MinWidth(18f), GUILayout.Height(18f) };
                        if (GUILayout.Button("AtlasTexture:button:SelectAll".GlcV(), l)) { SelectAll(sMatSelectors, _displayMaterial); }
                        if (GUILayout.Button("AtlasTexture:button:Invert".GlcV(), l)) { SelectInvert(sMatSelectors, _displayMaterial); }
                    }
                    if (GUILayout.Button("AtlasTexture:button:RefreshMaterials".GetLocalize()) || _displayMaterial == null)
                        RefreshMaterials();
                }
                if (_displayMaterial is not null)
                    using (new PFScope("MaterialSelectEditor"))
                        MaterialSelectEditor(sMatSelectors, _displayMaterial);

            }

            using (new EditorGUI.IndentLevelScope(1))
                EditorGUILayout.PropertyField(sMatSelectors, "AtlasTexture:prop:SelectedMaterialView".GlcV());


            EditorGUILayout.LabelField("AtlasTexture:label:MaterialSettings".Glc(), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            using (new PFScope("MaterialSettings"))
            {
                using (new PFScope("DrawMaterialMergeGroup"))
                    DrawMaterialMergeGroup(sMatSelectors, sMergeMaterialGroups);
                using (new EditorGUI.IndentLevelScope(1))
                    EditorGUILayout.PropertyField(sMergeMaterialGroups, "AtlasTexture:prop:MergeMaterialGroups".GlcV());
                EditorGUILayout.PropertyField(sAllMaterialMergeReference, "AtlasTexture:prop:AllMaterialMergeReference".GlcV());
            }

            using (new PFScope("DrawAtlasSettings"))
                DrawAtlasSettings();

            PreviewButtonDrawUtil.Draw(thisTarget);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAtlasSettings()
        {
            EditorGUILayout.LabelField("AtlasTexture:label:AtlasSettings".Glc(), EditorStyles.boldLabel);
            using var t = new EditorGUI.IndentLevelScope(1);

            EditorGUILayout.PropertyField(sAtlasTextureSize, "AtlasTexture:prop:AtlasTextureSize".GlcV());
            if (sCustomAspect.boolValue) EditorGUILayout.PropertyField(sAtlasTextureHeightSize, "AtlasTexture:prop:AtlasTextureHeightSize".GlcV());
            EditorGUILayout.PropertyField(sCustomAspect, "AtlasTexture:prop:CustomAspect".GlcV());

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(sAtlasTargetUVChannel, "AtlasTexture:prop:AtlasTargetUVChannel".GlcV());
                if (cc.changed) RefreshMaterials();
            }

            EditorGUILayout.PropertyField(sUsePrimaryMaximumTexture, "AtlasTexture:prop:UsePrimaryMaximumTexture".GlcV());
            if (sUsePrimaryMaximumTexture.boolValue is false) EditorGUILayout.PropertyField(sPrimaryTextureProperty, "AtlasTexture:prop:PrimaryTextureProperty".GlcV());


            EditorGUILayout.PropertyField(sPadding, "AtlasTexture:prop:Padding".GlcV());

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(sIncludeDisabledRenderer, "AtlasTexture:prop:IncludeDisabledRenderer".GlcV());
                if (cc.changed) RefreshMaterials();
            }
            EditorGUILayout.PropertyField(sForceSizePriority, "AtlasTexture:prop:ForceSizePriority".GlcV());


            EditorGUILayout.PropertyField(sForceSetTexture, "AtlasTexture:prop:ForceSetTexture".GlcV());
            EditorGUILayout.PropertyField(sBackGroundColor, "AtlasTexture:prop:BackGroundColor".GlcV());
            EditorGUILayout.PropertyField(sPixelNormalize, "AtlasTexture:prop:PixelNormalize".GlcV());

            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Common:ExperimentalFuture".Glc());
            using (new EditorGUI.IndentLevelScope(1))
                if (s_ExperimentalFutureOption)
                {
                    EditorGUILayout.PropertyField(sIslandFineTuners, "AtlasTexture:prop:IslandFineTuners".GlcV());
                    EditorGUILayout.PropertyField(sWriteOriginalUV, "AtlasTexture:prop:ExperimentalFuture:WriteOriginalUV".GlcV());
                    if (sWriteOriginalUV.boolValue) { EditorGUILayout.PropertyField(sOriginalUVWriteTargetChannel, "AtlasTexture:prop:ExperimentalFuture:OriginalUVWriteTargetChannel".GlcV()); }

                    EditorGUILayout.PropertyField(sUnsetTextures, "AtlasTexture:prop:ExperimentalFuture:UnsetTextures".GlcV());

                    EditorGUILayout.PropertyField(sAutoTextureSizeSetting, "AtlasTexture:prop:ExperimentalFuture:AutoTextureSizeSetting".GlcV());
                    EditorGUILayout.PropertyField(sAutoReferenceCopySetting, "AtlasTexture:prop:ExperimentalFuture:AutoReferenceCopySetting".GlcV());
                    EditorGUILayout.PropertyField(sAutoMergeTextureSetting, "AtlasTexture:prop:ExperimentalFuture:AutoMergeTextureSetting".GlcV());

                    EditorGUILayout.PropertyField(sTextureIndividualFineTuning, "AtlasTexture:prop:TextureIndividualFineTuning".GlcV());
                    if (PreviewUtility.IsPreviewContains is false)
                    {
                        if (GUILayout.Button("AtlasTexture:prop:OpenTextureFineTuningManager".GlcV(), GUILayout.Height(18f)))
                        { TextureFineTuningManager.OpenAtlasTexture(thisTarget); }
                    }
                }

            EditorGUILayout.PropertyField(sTextureFineTuning, "AtlasTexture:prop:TextureFineTuning".GlcV());
        }

        private static void DrawMaterialMergeGroup(SerializedProperty sMatSelectors, SerializedProperty sMaterialMergeGroups)
        {
            var headerRect = EditorGUILayout.GetControlRect();
            var guiContent = EditorGUI.BeginProperty(headerRect, "AtlasTexture:prop:ExperimentalFuture:MaterialMergeGroups".GlcV(), sMaterialMergeGroups);

            s_MaterialMergeGroupsFoldout = EditorGUI.Foldout(headerRect, s_MaterialMergeGroupsFoldout, guiContent);
            if (s_MaterialMergeGroupsFoldout)
            {

                var buttonWidth = headerRect.width * 0.125f;
                headerRect.x += headerRect.width - buttonWidth * 2;
                headerRect.width = buttonWidth;
                if (GUI.Button(headerRect, "+"))
                {
                    var newIndex = sMaterialMergeGroups.arraySize;
                    sMaterialMergeGroups.arraySize += 1;

                    var mmg = sMaterialMergeGroups.GetArrayElementAtIndex(newIndex);
                    mmg.FindPropertyRelative("Reference").objectReferenceValue = null;
                    mmg.FindPropertyRelative("Group").arraySize = 0;
                }
                headerRect.x += headerRect.width;
                if (GUI.Button(headerRect, "-")) { sMaterialMergeGroups.arraySize += -1; }
            }

            EditorGUI.EndProperty();

            if (!s_MaterialMergeGroupsFoldout) { return; }

            s_targetMatHash.Clear();
            for (var i = 0; sMatSelectors.arraySize > i; i += 1) { s_targetMatHash.Add(sMatSelectors.GetArrayElementAtIndex(i).FindPropertyRelative("Material").objectReferenceValue as Material); }

            using var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);

            for (var i = 0; sMaterialMergeGroups.arraySize > i; i += 1)
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var mmg = sMaterialMergeGroups.GetArrayElementAtIndex(i);
                    var mRef = mmg.FindPropertyRelative("Reference");
                    var mg = mmg.FindPropertyRelative("Group");
                    var mgGUIContent = "AtlasTexture:prop:ExperimentalFuture:MaterialMergeGroups:GroupMaterials".Glc();


                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        foreach (var mat in s_targetMatHash)
                        {
                            using var hs = new EditorGUILayout.HorizontalScope();

                            var matIndex = FindGroupMaterial(mg, mat);
                            var contains = matIndex != -1;
                            var mod = EditorGUILayout.Toggle(contains, GUILayout.Width(64f));

                            if (mod != contains)
                            {
                                if (mod)
                                {
                                    var newIndex = mg.arraySize;
                                    mg.arraySize += 1;
                                    mg.GetArrayElementAtIndex(newIndex).objectReferenceValue = mat;
                                }
                                else
                                {
                                    mg.DeleteArrayElementAtIndex(matIndex);
                                }
                            }

                            EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Width(18f)), AssetPreview.GetAssetPreview(mat));
                            EditorGUILayout.ObjectField(mat, typeof(Material), false);

                        }
                        for (var mgi = 0; mg.arraySize > mgi; mgi += 1) { s_targetMatHash.Remove(mg.GetArrayElementAtIndex(mgi).objectReferenceValue as Material); }
                    }

                    EditorGUILayout.PropertyField(mRef, "AtlasTexture:prop:ExperimentalFuture:MaterialMergeGroups:MergeReferenceMaterial".Glc());
                }
        }

        private static int FindGroupMaterial(SerializedProperty mg, Material mat)
        {
            for (var i = 0; mg.arraySize > i; i += 1) { if (mg.GetArrayElementAtIndex(i).objectReferenceValue == mat) { return i; } }
            return -1;
        }

        static bool s_ExperimentalFutureOption = false;
        static bool s_MaterialMergeGroupsFoldout = false;
        static HashSet<Material> s_targetMatHash = new();



        List<List<Material>> _displayMaterial;

        void RefreshMaterials()
        {
            var domainFindPoint = target as AtlasTexture;
            var limitRoot = sLimitCandidateMaterials.objectReferenceValue as GameObject;
            var includeDisabledRenderer = thisTarget.AtlasSetting.IncludeDisabledRenderer;
            var uvChannel = (UVChannel)sAtlasTargetUVChannel.enumValueIndex;

            _displayMaterial = null;
            // if (AtlasShaderSupportUtils.s_atlasShaderSupportList is null) { return; }

            var domainRoot = DomainMarkerFinder.FindMarker(domainFindPoint.gameObject);
            if (domainRoot == null) { return; }

            var domainRenderers = AtlasTexture.FilteredRenderers(domainRoot, includeDisabledRenderer);

            List<Material> filteredMaterials;
            if (limitRoot != null)
            {
                var limitedRenderers = AtlasTexture.FilteredRenderers(limitRoot, includeDisabledRenderer);
                filteredMaterials = RendererUtility.GetMaterials(domainRenderers).Intersect(RendererUtility.GetMaterials(limitedRenderers)).Distinct().Where(m => m != null).ToList();
            }
            else { filteredMaterials = RendererUtility.GetMaterials(domainRenderers).Distinct().Where(m => m != null).ToList(); }

            // var atlasSSupport = new AtlasShaderSupportUtils();
            // var supportDict = filteredMaterials.ToDictionary(m => m, m => AtlasShaderSupportUtils.GetAtlasShaderSupporter(m));

            // var atlasTexDict = filteredMaterials.ToDictionary(m => m, m => TTShaderTextureUsageInformationUtil.GetContainsUVUsage(m));

            _displayMaterial = new MaterialGroupingContext(filteredMaterials.ToHashSet(), uvChannel, null).GroupMaterials.Select(i => new List<Material>(i)).ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty targetMaterial, List<List<Material>> tempMaterialGroupAll)
        {
            // EditorGUI.indentLevel += 1;
            var headerRect = EditorGUILayout.GetControlRect();
            var headerWidth = headerRect.width;
            var targetTextWidth = headerRect.width = 64f + 18f;
            EditorGUI.LabelField(headerRect, "AtlasTexture:label:Target".GlcV());

            headerRect.width = (headerWidth * 0.5f) - targetTextWidth;
            headerRect.x = targetTextWidth;
            EditorGUI.LabelField(headerRect, "AtlasTexture:label:SizePriority".GlcV());

            headerRect.width = headerWidth * 0.5f;
            headerRect.x = headerRect.width;
            EditorGUI.LabelField(headerRect, "AtlasTexture:label:Material".Glc());

            foreach (var matGroup in tempMaterialGroupAll)
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    foreach (var mat in matGroup)
                    {
                        var sMatSelector = FindMatSelector(targetMaterial, mat);
                        var rect = EditorGUILayout.GetControlRect();

                        var drawWidth = rect.width * 0.5f;
                        var initialX = rect.x;

                        var isTarget = sMatSelector != null;

                        rect.width = 32f;
                        var editIsTarget = EditorGUI.Toggle(rect, isTarget);
                        rect.x += rect.width;

                        if (isTarget != editIsTarget)
                        {
                            if (editIsTarget)
                            {
                                var index = targetMaterial.arraySize;
                                targetMaterial.arraySize += 1;
                                var sNewMatSelector = targetMaterial.GetArrayElementAtIndex(index);
                                sNewMatSelector.FindPropertyRelative("Material").objectReferenceValue = mat;
                                sNewMatSelector.FindPropertyRelative("MaterialFineTuningValue").floatValue = 1;
                            }
                            else
                            {
                                targetMaterial.DeleteArrayElementAtIndex(FindMatSelectorIndex(targetMaterial, mat));
                            }
                        }
                        else if (isTarget)
                        {
                            rect.width = drawWidth - 32f;
                            rect.x = initialX + 24f;
                            var SOffset = sMatSelector.FindPropertyRelative("MaterialFineTuningValue");
                            SOffset.floatValue = EditorGUI.Slider(rect, SOffset.floatValue, 0, 1);
                            rect.x = initialX + drawWidth;
                        }
                        rect.x = initialX + drawWidth - 18f;
                        rect.width = 18f;
                        var matPrevTex = AssetPreview.GetAssetPreview(mat);
                        if (matPrevTex != null) { EditorGUI.DrawTextureTransparent(rect, matPrevTex, ScaleMode.ScaleToFit); }

                        rect.width = drawWidth;
                        rect.x = initialX + drawWidth;
                        EditorGUI.ObjectField(rect, mat, typeof(Material), false);
                    }

            // EditorGUI.indentLevel -= 1;
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
        private static void SelectAll(SerializedProperty sMatSelectors, List<List<Material>> tempMaterialGroupAll)
        {
            var materials = tempMaterialGroupAll.SelectMany(i => i);
            foreach (var m in materials)
            {
                if (FindMatSelector(sMatSelectors, m) is not null) { continue; }
                var newIndex = sMatSelectors.arraySize;
                sMatSelectors.arraySize += 1;
                var newSelector = sMatSelectors.GetArrayElementAtIndex(newIndex);
                newSelector.FindPropertyRelative("Material").objectReferenceValue = m;
                newSelector.FindPropertyRelative("MaterialFineTuningValue").floatValue = 1;
            }
        }
        private static void SelectInvert(SerializedProperty sMatSelectors, List<List<Material>> tempMaterialGroupAll)
        {
            var enables = tempMaterialGroupAll.SelectMany(i => i).Where(m => FindMatSelector(sMatSelectors, m) is null).ToArray();
            sMatSelectors.arraySize = 0;
            foreach (var m in enables)
            {
                var newIndex = sMatSelectors.arraySize;
                sMatSelectors.arraySize += 1;
                var newSelector = sMatSelectors.GetArrayElementAtIndex(newIndex);
                newSelector.FindPropertyRelative("Material").objectReferenceValue = m;
                newSelector.FindPropertyRelative("MaterialFineTuningValue").floatValue = 1;
            }
        }


        // [InitializeOnLoadMethod]
        // internal static void RegisterSummary()
        // {
        //     TexTransGroupEditor.s_summary[typeof(AtlasTexture)] = at =>
        //     {
        //         var ve = new VisualElement();
        //         var serializedObject = new SerializedObject(at);
        //         var sTargetRenderers = serializedObject.FindProperty("LimitCandidateMaterials");
        //         var sAtlasTextureSize = serializedObject.FindProperty("AtlasSetting").FindPropertyRelative("AtlasTextureSize");

        //         var limitCandidateMaterials = new PropertyField();
        //         limitCandidateMaterials.label = "AtlasTexture:prop:LimitCandidateMaterials".GetLocalize();
        //         limitCandidateMaterials.BindProperty(sTargetRenderers);
        //         ve.hierarchy.Add(limitCandidateMaterials);

        //         var atlasTextureSize = new PropertyField();
        //         atlasTextureSize.label = "AtlasTexture:prop:AtlasTextureSize".GetLocalize();
        //         atlasTextureSize.BindProperty(sAtlasTextureSize);
        //         ve.hierarchy.Add(atlasTextureSize);

        //         return ve;
        //     };
        // }
    }
}

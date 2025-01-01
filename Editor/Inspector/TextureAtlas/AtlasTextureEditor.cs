using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using net.rs64.TexTransTool.Utils;

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

            var sAtlasSetting = thisSObject.FindProperty("AtlasSetting");
            var sLimitCandidateMaterials = thisSObject.FindProperty("LimitCandidateMaterials");
            var sMatSelectors = thisSObject.FindProperty("SelectMatList");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sLimitCandidateMaterials, "AtlasTexture:prop:LimitCandidateMaterials".Glc());
            if (EditorGUI.EndChangeCheck()) { RefreshMaterials(thisTarget, sLimitCandidateMaterials.objectReferenceValue as GameObject, thisTarget.AtlasSetting.IncludeDisabledRenderer, thisTarget.AtlasSetting.MergeMaterials ? thisTarget.AtlasSetting.PropertyBakeSetting != PropertyBakeSetting.NotBake : false); }

            if (PreviewUtility.IsPreviewContains is false)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (_displayMaterial is not null)
                    {
                        var l = new[] { GUILayout.MaxWidth(64f), GUILayout.MinWidth(18f) };
                        if (GUILayout.Button("AtlasTexture:button:SelectAll".GetLocalize(), l)) { SelectAll(sMatSelectors, _displayMaterial); }
                        if (GUILayout.Button("AtlasTexture:button:Invert".GetLocalize(), l)) { SelectInvert(sMatSelectors, _displayMaterial); }
                    }
                    if (GUILayout.Button("AtlasTexture:button:RefreshMaterials".GetLocalize()) || _displayMaterial == null)
                    { RefreshMaterials(thisTarget, thisTarget.LimitCandidateMaterials, thisTarget.AtlasSetting.IncludeDisabledRenderer, thisTarget.AtlasSetting.MergeMaterials ? thisTarget.AtlasSetting.PropertyBakeSetting != PropertyBakeSetting.NotBake : false); }
                }
                if (_displayMaterial is not null) { MaterialSelectEditor(sMatSelectors, _displayMaterial); }
            }

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(sMatSelectors, "AtlasTexture:prop:SelectedMaterialView".Glc());
            EditorGUI.indentLevel -= 1;


            DrawAtlasSettings(sAtlasSetting, sMatSelectors);

            PreviewButtonDrawUtil.Draw(thisTarget);

            serializedObject.ApplyModifiedProperties();

        }

        private void DrawAtlasSettings(SerializedProperty sAtlasSettings, SerializedProperty sMatSelectors)
        {
            var thisTarget = target as AtlasTexture;
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
            var sOriginalUVWriteTargetChannel = sAtlasSettings.FindPropertyRelative("OriginalUVWriteTargetChannel");
            var sIncludeDisabledRenderer = sAtlasSettings.FindPropertyRelative("IncludeDisabledRenderer");
            var sPixelNormalize = sAtlasSettings.FindPropertyRelative("PixelNormalize");
            var sTextureFineTuning = sAtlasSettings.FindPropertyRelative("TextureFineTuning");
            var sIslandFineTuners = sAtlasSettings.FindPropertyRelative("IslandFineTuners");
            var sForceSizePriority = sAtlasSettings.FindPropertyRelative("ForceSizePriority");
            var sMaterialMergeGroups = sAtlasSettings.FindPropertyRelative("MaterialMergeGroups");
            var sTextureIndividualFineTuning = sAtlasSettings.FindPropertyRelative("TextureIndividualFineTuning");
            var sAutoReferenceCopySetting = sAtlasSettings.FindPropertyRelative("AutoReferenceCopySetting");
            var sAutoMergeTextureSetting = sAtlasSettings.FindPropertyRelative("AutoMergeTextureSetting");
            var sBackGroundColor = sAtlasSettings.FindPropertyRelative("BackGroundColor");
            var sDownScalingAlgorithm = sAtlasSettings.FindPropertyRelative("DownScalingAlgorithm");
            var sTextureScaleOffsetReset = sAtlasSettings.FindPropertyRelative("TextureScaleOffsetReset");
            var sBakedPropertyWriteMaxValue = sAtlasSettings.FindPropertyRelative("BakedPropertyWriteMaxValue");
            var sUnsetTextures = sAtlasSettings.FindPropertyRelative("UnsetTextures");
            var sHeightDenominator = sAtlasSettings.FindPropertyRelative("HeightDenominator");

            var sLimitCandidateMaterials = sIncludeDisabledRenderer.serializedObject.FindProperty("LimitCandidateMaterials");

            EditorGUILayout.PropertyField(sAtlasTextureSize, "AtlasTexture:prop:AtlasTextureSize".Glc());
            EditorGUILayout.PropertyField(sPadding, "AtlasTexture:prop:Padding".Glc());

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sIncludeDisabledRenderer, "AtlasTexture:prop:IncludeDisabledRenderer".Glc());
            if (EditorGUI.EndChangeCheck()) { RefreshMaterials(thisTarget, sLimitCandidateMaterials.objectReferenceValue as GameObject, sIncludeDisabledRenderer.boolValue, sMergeMaterials.boolValue ? sPropertyBakeSetting.enumValueIndex != 0 : false); }
            EditorGUILayout.PropertyField(sForceSizePriority, "AtlasTexture:prop:ForceSizePriority".Glc());


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sMergeMaterials, "AtlasTexture:prop:MaterialMerge".Glc());
            if (EditorGUI.EndChangeCheck()) { RefreshMaterials(thisTarget, sLimitCandidateMaterials.objectReferenceValue as GameObject, sIncludeDisabledRenderer.boolValue, sMergeMaterials.boolValue ? sPropertyBakeSetting.enumValueIndex != 0 : false); }
            if (sMergeMaterials.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sPropertyBakeSetting, "AtlasTexture:prop:PropertyBakeSetting".Glc());
                if (EditorGUI.EndChangeCheck()) { RefreshMaterials(thisTarget, sLimitCandidateMaterials.objectReferenceValue as GameObject, sIncludeDisabledRenderer.boolValue, sMergeMaterials.boolValue ? sPropertyBakeSetting.enumValueIndex != 0 : false); }
                EditorGUILayout.PropertyField(sMergeReferenceMaterial, "AtlasTexture:prop:MergeReferenceMaterial".Glc());
            }
            EditorGUILayout.PropertyField(sForceSetTexture, "AtlasTexture:prop:ForceSetTexture".Glc());

            EditorGUILayout.PropertyField(sBackGroundColor, "AtlasTexture:prop:BackGroundColor".Glc());

            EditorGUILayout.PropertyField(sPixelNormalize, "AtlasTexture:prop:PixelNormalize".Glc());

            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Common:ExperimentalFuture".Glc());
            if (s_ExperimentalFutureOption)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(sHeightDenominator, "AtlasTexture:prop:HeightDenominator".Glc());
                EditorGUILayout.PropertyField(sIslandFineTuners, "AtlasTexture:prop:IslandFineTuners".Glc());
                EditorGUILayout.PropertyField(sAtlasIslandRelocator, "AtlasTexture:prop:ExperimentalFuture:AtlasIslandRelocator".Glc());
                EditorGUILayout.PropertyField(sWriteOriginalUV, "AtlasTexture:prop:ExperimentalFuture:WriteOriginalUV".Glc());
                if (sWriteOriginalUV.boolValue) { EditorGUILayout.PropertyField(sOriginalUVWriteTargetChannel, "AtlasTexture:prop:ExperimentalFuture:OriginalUVWriteTargetChannel".Glc()); }
                if (sMergeMaterials.boolValue) { DrawMaterialMergeGroup(sMatSelectors, sMaterialMergeGroups); }

                EditorGUILayout.PropertyField(sTextureScaleOffsetReset, "AtlasTexture:prop:ExperimentalFuture:TextureScaleOffsetReset".Glc());
                if (sMergeMaterials.boolValue && (PropertyBakeSetting)sPropertyBakeSetting.enumValueIndex != PropertyBakeSetting.NotBake)
                { EditorGUILayout.PropertyField(sBakedPropertyWriteMaxValue, "AtlasTexture:prop:ExperimentalFuture:BakedPropertyWriteMaxValue".Glc()); }

                EditorGUILayout.PropertyField(sUnsetTextures, "AtlasTexture:prop:ExperimentalFuture:UnsetTextures".Glc());

                EditorGUILayout.PropertyField(sDownScalingAlgorithm, "AtlasTexture:prop:DownScalingAlgorithm".Glc());
                if ((PropertyBakeSetting)sPropertyBakeSetting.enumValueIndex == PropertyBakeSetting.NotBake) { EditorGUILayout.PropertyField(sAutoReferenceCopySetting, "AtlasTexture:prop:ExperimentalFuture:AutoReferenceCopySetting".Glc()); }
                EditorGUILayout.PropertyField(sAutoMergeTextureSetting, "AtlasTexture:prop:ExperimentalFuture:AutoMergeTextureSetting".Glc());

                EditorGUILayout.PropertyField(sTextureIndividualFineTuning, "AtlasTexture:prop:TextureIndividualFineTuning".Glc());
                if (PreviewUtility.IsPreviewContains is false)
                {
                    if (GUILayout.Button("AtlasTexture:prop:OpenTextureFineTuningManager".Glc()))
                    { TextureFineTuningManager.OpenAtlasTexture(thisTarget); }
                }

                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.PropertyField(sTextureFineTuning, "AtlasTexture:prop:TextureFineTuning".Glc());


            EditorGUI.indentLevel -= 1;

        }

        private static void DrawMaterialMergeGroup(SerializedProperty sMatSelectors, SerializedProperty sMaterialMergeGroups)
        {
            var headerRect = EditorGUILayout.GetControlRect();
            var guiContent = EditorGUI.BeginProperty(headerRect, "AtlasTexture:prop:ExperimentalFuture:MaterialMergeGroups".Glc(), sMaterialMergeGroups);

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
                    mmg.FindPropertyRelative("MergeReferenceMaterial").objectReferenceValue = null;
                    mmg.FindPropertyRelative("GroupMaterials").arraySize = 0;
                }
                headerRect.x += headerRect.width;
                if (GUI.Button(headerRect, "-")) { sMaterialMergeGroups.arraySize += -1; }
            }

            EditorGUI.EndProperty();

            if (!s_MaterialMergeGroupsFoldout) { return; }

            s_targetMatHash.Clear();
            for (var i = 0; sMatSelectors.arraySize > i; i += 1) { s_targetMatHash.Add(sMatSelectors.GetArrayElementAtIndex(i).FindPropertyRelative("Material").objectReferenceValue as Material); }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (var i = 0; sMaterialMergeGroups.arraySize > i; i += 1)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var mmg = sMaterialMergeGroups.GetArrayElementAtIndex(i);
                        var mRef = mmg.FindPropertyRelative("MergeReferenceMaterial");
                        var mg = mmg.FindPropertyRelative("GroupMaterials");
                        var mgGUIContent = "AtlasTexture:prop:ExperimentalFuture:MaterialMergeGroups:GroupMaterials".Glc();

                        EditorGUILayout.PropertyField(mRef, "AtlasTexture:prop:ExperimentalFuture:MaterialMergeGroups:MergeReferenceMaterial".Glc());

                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            foreach (var mat in s_targetMatHash)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
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
                            }
                            for (var mgi = 0; mg.arraySize > mgi; mgi += 1) { s_targetMatHash.Remove(mg.GetArrayElementAtIndex(mgi).objectReferenceValue as Material); }
                        }
                        EditorGUILayout.PropertyField(mg, mgGUIContent);
                    }
                }
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
        void RefreshMaterials(AtlasTexture domainFindPoint, GameObject limitRoot, bool includeDisabledRenderer, bool usePropertyBake)
        {
            _displayMaterial = null;
            if (AtlasShaderSupportUtils.s_atlasShaderSupportList is null) { return; }

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

            var atlasSSupport = new AtlasShaderSupportUtils();
            var supportDict = filteredMaterials.ToDictionary(m => m, m => atlasSSupport.GetAtlasShaderSupporter(m));
            var atlasTexDict = supportDict.ToDictionary(m => m.Key, m => m.Value.GetAtlasShaderTexture2D(m.Key).ToDictionary(s => s.PropertyName, s => s));

            _displayMaterial = AtlasContext.LookUpMaterialGroup(atlasTexDict, supportDict, usePropertyBake).Select(i => new List<Material>(i)).ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty targetMaterial, List<List<Material>> tempMaterialGroupAll)
        {
            // EditorGUI.indentLevel += 1;
            var hadeRect = EditorGUILayout.GetControlRect();
            var hadeWidth = hadeRect.width;
            hadeRect.width = 64f;
            EditorGUI.LabelField(hadeRect, "AtlasTexture:label:Target".Glc());

            hadeRect.width = (hadeWidth * 0.5f) - 64f;
            hadeRect.x = 64f;
            EditorGUI.LabelField(hadeRect, "AtlasTexture:label:SizePriority".Glc());

            hadeRect.width = hadeWidth * 0.5f;
            hadeRect.x = hadeRect.width;
            EditorGUI.LabelField(hadeRect, "AtlasTexture:label:Material".Glc());

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


        [InitializeOnLoadMethod]
        internal static void RegisterSummary()
        {
            TexTransGroupEditor.s_summary[typeof(AtlasTexture)] = at =>
            {
                var ve = new VisualElement();
                var serializedObject = new SerializedObject(at);
                var sTargetRenderers = serializedObject.FindProperty("LimitCandidateMaterials");
                var sAtlasTextureSize = serializedObject.FindProperty("AtlasSetting").FindPropertyRelative("AtlasTextureSize");

                var limitCandidateMaterials = new PropertyField();
                limitCandidateMaterials.label = "AtlasTexture:prop:LimitCandidateMaterials".GetLocalize();
                limitCandidateMaterials.BindProperty(sTargetRenderers);
                ve.hierarchy.Add(limitCandidateMaterials);

                var atlasTextureSize = new PropertyField();
                atlasTextureSize.label = "AtlasTexture:prop:AtlasTextureSize".GetLocalize();
                atlasTextureSize.BindProperty(sAtlasTextureSize);
                ve.hierarchy.Add(atlasTextureSize);

                return ve;
            };
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using net.rs64.TexTransCore.Utils;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.Preview;

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
                if (TTTag.SaveDataVersion == 3 && GUILayout.Button("Migrate DSV3 To DSV4"))
                {
                    net.rs64.TexTransTool.Migration.V3.AtlasTextureV3.MigrationAtlasTextureV3ToV4(thisTarget);
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

            if (sTargetRoot.objectReferenceValue != null && !OneTimePreviewContext.IsPreviewContains)
            {
                if (GUILayout.Button("AtlasTexture:button:RefreshMaterials".GetLocalize()) || _displayMaterial == null)
                { RefreshMaterials(thisTarget.TargetRoot, thisTarget.AtlasSetting.IncludeDisabledRenderer); }
                MaterialSelectEditor(sMatSelectors, _displayMaterial);
            }

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(sMatSelectors, "AtlasTexture:prop:SelectedMaterialView".Glc());
            EditorGUI.indentLevel -= 1;


            DrawAtlasSettings(sAtlasSetting, sMatSelectors);

            OneTimePreviewContext.instance.DrawApplyAndRevert(thisTarget);

            serializedObject.ApplyModifiedProperties();

        }

        private void DrawAtlasSettings(SerializedProperty sAtlasSettings, SerializedProperty sMatSelectors)
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
            var sTextureFineTuning = sAtlasSettings.FindPropertyRelative("TextureFineTuning");
            var sIslandFineTuners = sAtlasSettings.FindPropertyRelative("IslandFineTuners");
            var sForceSizePriority = sAtlasSettings.FindPropertyRelative("ForceSizePriority");
            var sMaterialMergeGroups = sAtlasSettings.FindPropertyRelative("MaterialMergeGroups");



            EditorGUILayout.PropertyField(sAtlasTextureSize, "AtlasTexture:prop:AtlasTextureSize".Glc());
            EditorGUILayout.PropertyField(sPadding, "AtlasTexture:prop:Padding".Glc());

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sIncludeDisabledRenderer, "AtlasTexture:prop:IncludeDisabledRenderer".Glc());
            if (EditorGUI.EndChangeCheck()) { RefreshMaterials(sIncludeDisabledRenderer.serializedObject.FindProperty("TargetRoot").objectReferenceValue as GameObject, sIncludeDisabledRenderer.boolValue); }
            EditorGUILayout.PropertyField(sForceSizePriority, "AtlasTexture:prop:ForceSizePriority".Glc());



            EditorGUILayout.PropertyField(sMergeMaterials, "AtlasTexture:prop:MaterialMerge".Glc());
            if (sMergeMaterials.boolValue)
            {
                EditorGUILayout.PropertyField(sPropertyBakeSetting, "AtlasTexture:prop:PropertyBakeSetting".Glc());
                EditorGUILayout.PropertyField(sMergeReferenceMaterial, "AtlasTexture:prop:MergeReferenceMaterial".Glc());
            }
            EditorGUILayout.PropertyField(sForceSetTexture, "AtlasTexture:prop:ForceSetTexture".Glc());


            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Common:ExperimentalFuture".Glc());
            if (s_ExperimentalFutureOption)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(sIslandFineTuners, "AtlasTexture:prop:IslandFineTuners".Glc());
                EditorGUILayout.PropertyField(sAtlasIslandRelocator, "AtlasTexture:prop:ExperimentalFuture:AtlasIslandRelocator".Glc());
                EditorGUILayout.PropertyField(sWriteOriginalUV, "AtlasTexture:prop:ExperimentalFuture:WriteOriginalUV".Glc());
                EditorGUILayout.PropertyField(sPixelNormalize, "AtlasTexture:prop:ExperimentalFuture:PixelNormalize".Glc());
                if (sMergeMaterials.boolValue) { DrawMaterialMergeGroup(sMatSelectors, sMaterialMergeGroups); }
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

            foreach (var mat in tempMaterial)
            {
                var sMatSelector = FindMatSelector(targetMaterial, mat);
                var rect = EditorGUILayout.GetControlRect();

                var drawWidth = rect.width * 0.5f;

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
                    rect.width = drawWidth - 18f - 24f;
                    var SOffset = sMatSelector.FindPropertyRelative("MaterialFineTuningValue");
                    SOffset.floatValue = EditorGUI.Slider(rect, SOffset.floatValue, 0, 1);
                    rect.x += rect.width;
                }
                rect.x = drawWidth - 2.5f;
                rect.width = 18f;
                var matPrevTex = AssetPreview.GetAssetPreview(mat);
                if (matPrevTex != null) { EditorGUI.DrawTextureTransparent(rect, matPrevTex, ScaleMode.ScaleToFit); }

                rect.width = drawWidth;
                rect.x = rect.width;
                EditorGUI.ObjectField(rect, mat, typeof(Material), false);
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

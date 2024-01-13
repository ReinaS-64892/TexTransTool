
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTexture), true)]
    internal class AtlasTextureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            var thisTarget = target as AtlasTexture;
            var thisSObject = serializedObject;

#pragma warning disable CS0612
            if (thisTarget.SaveDataVersion != TexTransBehavior.TTTDataVersion)
            {
                if (thisTarget.SaveDataVersion == 0 && GUILayout.Button("Migrate DSV0 To DSV1"))
                {
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.MigrationAtlasTextureV0ToV1(thisTarget);
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.FinalizeMigrationAtlasTextureV0ToV1(thisTarget);
                }
                if (thisTarget.SaveDataVersion == 1 && GUILayout.Button("Migrate DSV1 To DSV2"))
                {
                    net.rs64.TexTransTool.Migration.V1.AtlasTextureV1.MigrationAtlasTextureV1ToV2(thisTarget);
                }
                return;
            }
#pragma warning restore CS0612

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(thisTarget));

            var sAtlasSetting = thisSObject.FindProperty("AtlasSetting");

            var sTargetRoot = thisSObject.FindProperty("TargetRoot");

            var sSelectReferenceMatList = thisSObject.FindProperty("SelectReferenceMat");
            var sMatSelectors = thisSObject.FindProperty("SelectMatList");

            TextureTransformerEditor.DrawerObjectReference<GameObject>(sTargetRoot, "TargetRoot".GetLC(), newRoot =>
            {
                Undo.RecordObject(thisTarget, "AtlasTexture - TargetRoot");
                RefreshMaterials(thisTarget, sTargetRoot, newRoot);
                thisSObject.ApplyModifiedProperties();
                return newRoot;
            });
            if (sTargetRoot.objectReferenceValue != null)
            {
                if (GUILayout.Button("RefreshMaterials".GetLocalize()))
                {
                    Undo.RecordObject(thisTarget, "AtlasTexture - SetTargetRoot");
                    RefreshMaterials(thisTarget, sTargetRoot, thisTarget.TargetRoot);
                    thisSObject.ApplyModifiedProperties();
                }
                if (_tempMaterial == null)
                {
                    Undo.RecordObject(thisTarget, "AtlasTexture - SetTargetRoot");
                    RefreshMaterials(thisTarget, sTargetRoot, thisTarget.TargetRoot);
                }
                MaterialSelectEditor(sMatSelectors, _tempMaterial);
            }


            DrawAtlasSettings(sAtlasSetting);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(thisTarget);
            serializedObject.ApplyModifiedProperties();

        }

        private static void DrawAtlasSettings(SerializedProperty sAtlasSettings)
        {
            EditorGUILayout.LabelField("AtlasSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var sAtlasTextureSize = sAtlasSettings.FindPropertyRelative("AtlasTextureSize");
            var sMergeMaterials = sAtlasSettings.FindPropertyRelative("MergeMaterials");
            var sMergeReferenceMaterial = sAtlasSettings.FindPropertyRelative("MergeReferenceMaterial");
            var sForceSetTexture = sAtlasSettings.FindPropertyRelative("ForceSetTexture");
            var sPropertyBakeSetting = sAtlasSettings.FindPropertyRelative("PropertyBakeSetting");
            var sPadding = sAtlasSettings.FindPropertyRelative("Padding");
            var sSorterName = sAtlasSettings.FindPropertyRelative("SorterName");
            var sWriteOriginalUV = sAtlasSettings.FindPropertyRelative("WriteOriginalUV");
            var sIncludeDisabledRenderer = sAtlasSettings.FindPropertyRelative("IncludeDisabledRenderer");
            var sUseUpScaling = sAtlasSettings.FindPropertyRelative("UseUpScaling");
            var sTextureFineTuningDataList = sAtlasSettings.FindPropertyRelative("TextureFineTuningDataList");



            EditorGUILayout.PropertyField(sAtlasTextureSize, new GUIContent("AtlasTextureSize".GetLocalize()));
            EditorGUILayout.PropertyField(sMergeMaterials, new GUIContent("MergeMaterials".GetLocalize()));
            if (sMergeMaterials.boolValue)
            {
                EditorGUILayout.PropertyField(sPropertyBakeSetting, new GUIContent("PropertyBakeSetting".GetLocalize()));
                EditorGUILayout.PropertyField(sMergeReferenceMaterial, new GUIContent("MergeReferenceMaterial".GetLocalize()));
            }
            EditorGUILayout.PropertyField(sForceSetTexture, new GUIContent("ForceSetTexture".GetLocalize()));
            EditorGUILayout.PropertyField(sPadding, new GUIContent("Padding".GetLocalize()));
            EditorGUI.BeginDisabledGroup(true); EditorGUILayout.PropertyField(sSorterName, new GUIContent("IslandSorterName".GetLocalize())); EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(sWriteOriginalUV, new GUIContent("WriteOriginalUV".GetLocalize()));
            EditorGUILayout.PropertyField(sIncludeDisabledRenderer, sIncludeDisabledRenderer.name.GetLC());
            EditorGUILayout.PropertyField(sUseUpScaling, sUseUpScaling.name.GetLC());
            DrawTextureFineTuningDataList(sTextureFineTuningDataList);


            EditorGUI.indentLevel -= 1;

        }

        private static void DrawTextureFineTuningDataList(SerializedProperty sFineTunings)
        {
            EditorGUILayout.LabelField("TextureFineTuning".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            for (int i = 0; sFineTunings.arraySize > i; i += 1)
            {
                var sFineSettingData = sFineTunings.GetArrayElementAtIndex(i);
                var sSelect = sFineSettingData.FindPropertyRelative("Select");
                EditorGUILayout.PropertyField(sSelect, new GUIContent("Setting".GetLocalize() + " " + i));
                switch (sSelect.enumValueIndex)
                {
                    default:
                        {
                            EditorGUILayout.LabelField($"{sSelect.enumValueIndex} enumValue Not Found");
                            break;
                        }
                    case 0:
                        {
                            var sResize_Size = sFineSettingData.FindPropertyRelative("Resize_Size");
                            var sResize_PropertyNames = sFineSettingData.FindPropertyRelative("Resize_PropertyNames");
                            var sResize_select = sFineSettingData.FindPropertyRelative("Resize_Select");
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.PropertyField(sResize_Size, new GUIContent("Size".GetLocalize()));
                            PropertyNameEditor.DrawInspectorGUI(sResize_PropertyNames);
                            EditorGUILayout.PropertyField(sResize_select, new GUIContent("Select".GetLocalize()));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 1:
                        {
                            var sCompressFormatQuality = sFineSettingData.FindPropertyRelative("Compress_FormatQuality");
                            var sCompressCompressionQuality = sFineSettingData.FindPropertyRelative("Compress_CompressionQuality");
                            var sCompressPropertyNames = sFineSettingData.FindPropertyRelative("Compress_PropertyNames");
                            var sCompressSelect = sFineSettingData.FindPropertyRelative("Compress_Select");
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.PropertyField(sCompressFormatQuality, new GUIContent("FormatQuality".GetLocalize()));
                            EditorGUILayout.PropertyField(sCompressCompressionQuality, new GUIContent("CompressionQuality".GetLocalize()));
                            PropertyNameEditor.DrawInspectorGUI(sCompressPropertyNames);
                            EditorGUILayout.PropertyField(sCompressSelect, new GUIContent("Select".GetLocalize()));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 2:
                        {
                            var sReferenceCopy_SousePropertyName = sFineSettingData.FindPropertyRelative("ReferenceCopy_SourcePropertyName");
                            var sReferenceCopy_TargetPropertyName = sFineSettingData.FindPropertyRelative("ReferenceCopy_TargetPropertyName");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(sReferenceCopy_SousePropertyName, "Source".GetLocalize() + "PropertyName".GetLocalize());
                            PropertyNameEditor.DrawInspectorGUI(sReferenceCopy_TargetPropertyName, "Target".GetLocalize() + "PropertyName".GetLocalize());
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 3:
                        {
                            var sRemove_PropertyNames = sFineSettingData.FindPropertyRelative("Remove_PropertyNames");
                            var sRemove_Select = sFineSettingData.FindPropertyRelative("Remove_Select");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(sRemove_PropertyNames);
                            EditorGUILayout.PropertyField(sRemove_Select, new GUIContent("Select".GetLocalize()));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 4:
                        {
                            var sMipMapRemove_PropertyNames = sFineSettingData.FindPropertyRelative("MipMapRemove_PropertyNames");
                            var sMipMapRemove_Select = sFineSettingData.FindPropertyRelative("MipMapRemove_Select");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(sMipMapRemove_PropertyNames);
                            EditorGUILayout.PropertyField(sMipMapRemove_Select, new GUIContent("Select".GetLocalize()));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }

                }
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("[" + "NewFineTuningSetting".GetLocalize() + "]");
            TextureTransformerEditor.DrawerArrayResizeButton(sFineTunings, true);
        }


        List<Material> _tempMaterial;
        void RefreshMaterials(AtlasTexture thisTarget, SerializedProperty sTargetRoot, GameObject NewRoot)
        {
            sTargetRoot.objectReferenceValue = NewRoot;
            if (NewRoot == null) { return; }

            var renderers = thisTarget.FilteredRenderers(NewRoot);
            _tempMaterial = RendererUtility.GetMaterials(renderers).Distinct().ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty targetMaterial, List<Material> tempMaterial)
        {
            EditorGUI.indentLevel += 1;
            GUILayout.Label("Target".GetLocalize() + "       " + "Scale".GetLocalize() + "       " + "Material".GetLocalize());
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
        public static void DrawerSummary(AtlasTexture target)
        {
            var sObj = new SerializedObject(target);
            var sTargetRenderers = sObj.FindProperty("TargetRoot");
            EditorGUILayout.PropertyField(sTargetRenderers, sTargetRenderers.name.GetLC());
            var sAtlasTextureSize = sObj.FindProperty("AtlasSetting").FindPropertyRelative("AtlasTextureSize");
            EditorGUILayout.PropertyField(sAtlasTextureSize, sAtlasTextureSize.name.GetLC());
            sObj.ApplyModifiedProperties();
        }
    }
}
#endif

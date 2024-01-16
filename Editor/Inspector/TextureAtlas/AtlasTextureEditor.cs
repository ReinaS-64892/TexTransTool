using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTexture), true)]
    internal class AtlasTextureEditor : UnityEditor.Editor
    {
        public static readonly Dictionary<BuildTarget, (string[] displayName, TextureFormat[] formats)> SimpleFormatChoices = new()
        {
            {BuildTarget.StandaloneWindows64,
                (new string[]{"RGBA-BC7","RGBA-DXT5|BC3","RGB-DXT1|BC1","RG-BC5","R-BC4"},
                 new TextureFormat[]{TextureFormat.BC7,TextureFormat.DXT5,TextureFormat.DXT1,TextureFormat.BC5,TextureFormat.BC4})},
            {BuildTarget.Android,
                (new string[]{"RGBA-ASTC_4x4","RGBA-ASTC_5x5","RGBA-ASTC_6x6","RGBA-ASTC_8x8","RGBA-ASTC_10x10","RGBA-ASTC_12x12"},
                 new TextureFormat[]{TextureFormat.ASTC_4x4,TextureFormat.ASTC_5x5,TextureFormat.ASTC_6x6,TextureFormat.ASTC_8x8,TextureFormat.ASTC_10x10,TextureFormat.ASTC_12x12})}
        };
        public override void OnInspectorGUI()
        {

            var thisTarget = target as AtlasTexture;
            var thisSObject = serializedObject;

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
            EditorGUILayout.PropertyField(sTextureFineTuningDataList, "TextureFineTuning".GetLC());


            EditorGUI.indentLevel -= 1;

        }


        [CustomPropertyDrawer(typeof(TextureFineTuningData))]
        public class TextureFineTuningDataDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var sFineTuningData = property;
                var sSelect = sFineTuningData.FindPropertyRelative("Select");
                position.height = 18f;
                EditorGUI.PropertyField(position, sSelect, new GUIContent(sSelect.enumNames[sSelect.enumValueIndex].GetLocalize()));
                position.y += 18;
                switch (sSelect.enumValueIndex)
                {
                    default:
                        {
                            EditorGUI.LabelField(position, $"{sSelect.enumValueIndex} enumValue Not Found");
                            position.y += 18;
                            break;
                        }
                    case 0:
                        {
                            var sResize_Size = sFineTuningData.FindPropertyRelative("Resize_Size");
                            var sResize_PropertyNames = sFineTuningData.FindPropertyRelative("Resize_PropertyNames");
                            var sResize_select = sFineTuningData.FindPropertyRelative("Resize_Select");
                            EditorGUI.PropertyField(position, sResize_Size, new GUIContent("Size".GetLocalize()));
                            position.y += 18;
                            EditorGUI.PropertyField(position, sResize_PropertyNames, "TargetPropertyName".GetLC());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sResize_select, new GUIContent("Select".GetLocalize()));
                            position.y += 18;
                            break;
                        }
                    case 1:
                        {
                            var sCompressFormatQuality = sFineTuningData.FindPropertyRelative("Compress_FormatQuality");
                            var sUseOverride = sFineTuningData.FindPropertyRelative("Compress_UseOverride");
                            var sOverrideTextureFormat = sFineTuningData.FindPropertyRelative("Compress_OverrideTextureFormat");
                            var sCompressCompressionQuality = sFineTuningData.FindPropertyRelative("Compress_CompressionQuality");
                            var sCompressPropertyNames = sFineTuningData.FindPropertyRelative("Compress_PropertyNames");
                            var sCompressSelect = sFineTuningData.FindPropertyRelative("Compress_Select");
                            if (!sUseOverride.boolValue)
                            {
                                EditorGUI.PropertyField(position, sCompressFormatQuality, new GUIContent("FormatQuality".GetLocalize()));
                                position.y += 18;
                            }
                            else
                            {
                                var nowChoicer = SimpleFormatChoices[EditorUserBuildSettings.activeBuildTarget];
                                var preIndex = Array.IndexOf(nowChoicer.formats, (TextureFormat)sOverrideTextureFormat.enumValueFlag);//なぜかenumValueIndexではなくenumValueFlagのほうを使うと正しい挙動をする。
                                var postIndex = EditorGUI.Popup(position, "SimpleFormatChoices".GetLocalize(), preIndex, nowChoicer.displayName);
                                if (preIndex != postIndex) { sOverrideTextureFormat.enumValueFlag = (int)nowChoicer.formats[postIndex]; }
                                position.y += 18;

                                EditorGUI.PropertyField(position, sOverrideTextureFormat, new GUIContent("OverrideTextureFormat".GetLocalize()));
                                position.y += 18;
                                EditorGUI.PropertyField(position, sCompressCompressionQuality, new GUIContent("CompressionQuality".GetLocalize()));
                                position.y += 18;
                            }
                            EditorGUI.PropertyField(position, sUseOverride, new GUIContent("UseOverrideTextureFormat".GetLocalize()));
                            position.y += 18;
                            EditorGUI.PropertyField(position, sCompressPropertyNames, "TargetPropertyName".GetLC());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sCompressSelect, new GUIContent("Select".GetLocalize()));
                            position.y += 18;
                            break;
                        }
                    case 2:
                        {
                            var sReferenceCopy_SousePropertyName = sFineTuningData.FindPropertyRelative("ReferenceCopy_SourcePropertyName");
                            var sReferenceCopy_TargetPropertyName = sFineTuningData.FindPropertyRelative("ReferenceCopy_TargetPropertyName");
                            EditorGUI.PropertyField(position, sReferenceCopy_SousePropertyName, new GUIContent("Source".GetLocalize() + "PropertyName".GetLocalize()));
                            position.y += 18;
                            EditorGUI.PropertyField(position, sReferenceCopy_TargetPropertyName, new GUIContent("Target".GetLocalize() + "PropertyName".GetLocalize()));
                            position.y += 18;
                            break;
                        }
                    case 3:
                        {
                            var sRemove_PropertyNames = sFineTuningData.FindPropertyRelative("Remove_PropertyNames");
                            var sRemove_Select = sFineTuningData.FindPropertyRelative("Remove_Select");
                            EditorGUI.PropertyField(position, sRemove_PropertyNames, "TargetPropertyName".GetLC());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sRemove_Select, new GUIContent("Select".GetLocalize()));
                            position.y += 18;
                            break;
                        }
                    case 4:
                        {
                            var sMipMapRemove_PropertyNames = sFineTuningData.FindPropertyRelative("MipMapRemove_PropertyNames");
                            var sMipMapRemove_Select = sFineTuningData.FindPropertyRelative("MipMapRemove_Select");
                            EditorGUI.PropertyField(position, sMipMapRemove_PropertyNames, "TargetPropertyName".GetLC());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sMipMapRemove_Select, new GUIContent("Select".GetLocalize()));
                            position.y += 18;
                            break;
                        }
                    case 5:
                        {
                            var sColorSpaceSelect = sFineTuningData.FindPropertyRelative("ColorSpace_Select");
                            var sColorSpacePropertyNames = sFineTuningData.FindPropertyRelative("ColorSpace_PropertyNames");
                            var sColorSpaceLinear = sFineTuningData.FindPropertyRelative("ColorSpace_Linear");
                            EditorGUI.PropertyField(position, sColorSpaceLinear, new GUIContent("Linear".GetLocalize()));
                            position.y += 18;
                            EditorGUI.PropertyField(position, sColorSpacePropertyNames, "TargetPropertyName".GetLC());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sColorSpaceSelect, new GUIContent("Select".GetLocalize()));
                            position.y += 18;
                            break;
                        }
                }

            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var sSelect = property.FindPropertyRelative("Select");
                var baseHeight = 18f;
                switch (sSelect.enumValueIndex)
                {
                    default: return baseHeight * 2;
                    case 0: return baseHeight * 4;
                    case 1:
                        {
                            var sUseOverride = property.FindPropertyRelative("Compress_UseOverride");
                            return baseHeight * (sUseOverride.boolValue ? 7 : 5);
                        }
                    case 2: return baseHeight * 3;
                    case 3: return baseHeight * 3;
                    case 4: return baseHeight * 3;
                    case 5: return baseHeight * 4;
                }
            }


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

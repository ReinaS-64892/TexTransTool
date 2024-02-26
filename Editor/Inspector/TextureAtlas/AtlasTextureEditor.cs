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
            var sUseUpScaling = sAtlasSettings.FindPropertyRelative("UseUpScaling");
            var sTextureFineTuningDataList = sAtlasSettings.FindPropertyRelative("TextureFineTuningDataList");



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
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.PropertyField(sTextureFineTuningDataList, "AtlasTexture:prop:TextureFineTuning".Glc());


            EditorGUI.indentLevel -= 1;

        }

        static bool s_ExperimentalFutureOption = false;

        [CustomPropertyDrawer(typeof(TextureFineTuningData))]
        public class TextureFineTuningDataDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var sFineTuningData = property;
                var sSelect = sFineTuningData.FindPropertyRelative("Select");
                position.height = 18f;
                EditorGUI.PropertyField(position, sSelect, ("TextureFineTuning:prop:" + sSelect.enumNames[sSelect.enumValueIndex]).Glc());
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
                            EditorGUI.PropertyField(position, sResize_Size, "TextureFineTuning:prop:Resize:Size".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sResize_PropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sResize_select, "TextureFineTuning:prop:Select".Glc());
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
                                EditorGUI.PropertyField(position, sCompressFormatQuality, "TextureFineTuning:prop:FormatQuality".Glc());
                                position.y += 18;
                            }
                            else
                            {
                                var nowChoicer = SimpleFormatChoices[EditorUserBuildSettings.activeBuildTarget];
                                var preIndex = Array.IndexOf(nowChoicer.formats, (TextureFormat)sOverrideTextureFormat.enumValueFlag);//なぜかenumValueIndexではなくenumValueFlagのほうを使うと正しい挙動をする。
                                var postIndex = EditorGUI.Popup(position, "TextureFineTuning:prop:SimpleFormatChoices".GetLocalize(), preIndex, nowChoicer.displayName);
                                if (preIndex != postIndex) { sOverrideTextureFormat.enumValueFlag = (int)nowChoicer.formats[postIndex]; }
                                position.y += 18;

                                EditorGUI.PropertyField(position, sOverrideTextureFormat, "TextureFineTuning:prop:OverrideTextureFormat".Glc());
                                position.y += 18;
                                EditorGUI.PropertyField(position, sCompressCompressionQuality, "TextureFineTuning:prop:CompressionQuality".Glc());
                                position.y += 18;
                            }
                            EditorGUI.PropertyField(position, sUseOverride, "TextureFineTuning:prop:UseOverrideTextureFormat".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sCompressPropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sCompressSelect, "TextureFineTuning:prop:Select".Glc());
                            position.y += 18;
                            break;
                        }
                    case 2:
                        {
                            var sReferenceCopy_SousePropertyName = sFineTuningData.FindPropertyRelative("ReferenceCopy_SourcePropertyName");
                            var sReferenceCopy_TargetPropertyName = sFineTuningData.FindPropertyRelative("ReferenceCopy_TargetPropertyName");
                            EditorGUI.PropertyField(position, sReferenceCopy_SousePropertyName, "TextureFineTuning:prop:ReferenceCopy:SourcePropertyName".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sReferenceCopy_TargetPropertyName, "TextureFineTuning:prop:ReferenceCopy:TargetPropertyName".Glc());
                            position.y += 18;
                            break;
                        }
                    case 3:
                        {
                            var sRemove_PropertyNames = sFineTuningData.FindPropertyRelative("Remove_PropertyNames");
                            var sRemove_Select = sFineTuningData.FindPropertyRelative("Remove_Select");
                            EditorGUI.PropertyField(position, sRemove_PropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sRemove_Select, "TextureFineTuning:prop:Select".Glc());
                            position.y += 18;
                            break;
                        }
                    case 4:
                        {
                            var sMipMapRemove_PropertyNames = sFineTuningData.FindPropertyRelative("MipMapRemove_PropertyNames");
                            var sMipMapRemove_Select = sFineTuningData.FindPropertyRelative("MipMapRemove_Select");
                            EditorGUI.PropertyField(position, sMipMapRemove_PropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sMipMapRemove_Select, "TextureFineTuning:prop:Select".Glc());
                            position.y += 18;
                            break;
                        }
                    case 5:
                        {
                            var sColorSpaceSelect = sFineTuningData.FindPropertyRelative("ColorSpace_Select");
                            var sColorSpacePropertyNames = sFineTuningData.FindPropertyRelative("ColorSpace_PropertyNames");
                            var sColorSpaceLinear = sFineTuningData.FindPropertyRelative("ColorSpace_Linear");
                            EditorGUI.PropertyField(position, sColorSpaceLinear, "TextureFineTuning:prop:Linear".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sColorSpacePropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
                            position.y += 18;
                            EditorGUI.PropertyField(position, sColorSpaceSelect, "TextureFineTuning:prop:Select".Glc());
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


#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTexture), true)]
    public class AtlasTextureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            var ThisTarget = target as AtlasTexture;
            var This_S_Object = serializedObject;

#pragma warning disable CS0612
            if (ThisTarget.SaveDataVersion != ToolUtils.ThiSaveDataVersion)
            {
                if (ThisTarget.SaveDataVersion == 0 && GUILayout.Button("Migrate DSV0 To DSV1"))
                {
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.MigrationAtlasTextureV0ToV1(ThisTarget);
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.FinalizeMigrationAtlasTextureV0ToV1(ThisTarget);
                }
                if (ThisTarget.SaveDataVersion == 1 && GUILayout.Button("Migrate DSV1 To DSV2"))
                {
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.MigrationAtlasTextureV0ToV1(ThisTarget);
                    net.rs64.TexTransTool.Migration.V0.AtlasTextureV0.FinalizeMigrationAtlasTextureV0ToV1(ThisTarget);
                }
                return;
            }
#pragma warning restore CS0612

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(ThisTarget));

            var S_AtlasSetting = This_S_Object.FindProperty("AtlasSetting");

            var S_TargetRoot = This_S_Object.FindProperty("TargetRoot");

            var S_SelectReferenceMatList = This_S_Object.FindProperty("SelectReferenceMat");
            var S_MatSelectors = This_S_Object.FindProperty("SelectMatList");

            TextureTransformerEditor.DrawerObjectReference<GameObject>(S_TargetRoot, NewRoot =>
            {
                Undo.RecordObject(ThisTarget, "AtlasTexture - TargetRoot");
                RefreshMaterials(S_TargetRoot, NewRoot);
                This_S_Object.ApplyModifiedProperties();
            }, "TargetRoot".GetLocalize());
            if (S_TargetRoot.objectReferenceValue != null)
            {
                if (GUILayout.Button("RefreshMaterials".GetLocalize()))
                {
                    Undo.RecordObject(ThisTarget, "AtlasTexture - SetTargetRoot");
                    RefreshMaterials(S_TargetRoot, ThisTarget.TargetRoot);
                    This_S_Object.ApplyModifiedProperties();
                }
                if (TempMaterial == null)
                {
                    Undo.RecordObject(ThisTarget, "AtlasTexture - SetTargetRoot");
                    RefreshMaterials(S_TargetRoot, ThisTarget.TargetRoot);
                }
                MaterialSelectEditor(S_MatSelectors, TempMaterial);
            }


            DrawAtlasSettings(S_AtlasSetting);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(ThisTarget);
            serializedObject.ApplyModifiedProperties();

        }

        private static void DrawAtlasSettings(SerializedProperty s_AtlasSettings)
        {
            EditorGUILayout.LabelField("AtlasSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var S_AtlasTextureSize = s_AtlasSettings.FindPropertyRelative("AtlasTextureSize");
            var S_MergeMaterials = s_AtlasSettings.FindPropertyRelative("MergeMaterials");
            var S_MergeReferenceMaterial = s_AtlasSettings.FindPropertyRelative("MergeReferenceMaterial");
            var S_ForceSetTexture = s_AtlasSettings.FindPropertyRelative("ForceSetTexture");
            var S_PropertyBakeSetting = s_AtlasSettings.FindPropertyRelative("PropertyBakeSetting");
            var S_Padding = s_AtlasSettings.FindPropertyRelative("Padding");
            var s_UseIslandCache = s_AtlasSettings.FindPropertyRelative("UseIslandCache");
            var S_SortingType = s_AtlasSettings.FindPropertyRelative("SortingType");
            var S_TextureFineTuningDataList = s_AtlasSettings.FindPropertyRelative("TextureFineTuningDataList");



            EditorGUILayout.PropertyField(S_AtlasTextureSize, new GUIContent("AtlasTextureSize".GetLocalize()));
            EditorGUILayout.PropertyField(S_MergeMaterials, new GUIContent("MergeMaterials".GetLocalize()));
            if (S_MergeMaterials.boolValue)
            {
                EditorGUILayout.PropertyField(S_PropertyBakeSetting, new GUIContent("PropertyBakeSetting".GetLocalize()));
                EditorGUILayout.PropertyField(S_MergeReferenceMaterial, new GUIContent("MergeReferenceMaterial".GetLocalize()));
            }
            EditorGUILayout.PropertyField(S_ForceSetTexture, new GUIContent("ForceSetTexture".GetLocalize()));
            EditorGUILayout.PropertyField(S_Padding, new GUIContent("Padding".GetLocalize()));
            EditorGUILayout.PropertyField(s_UseIslandCache, new GUIContent("UseIslandCache".GetLocalize()));
            EditorGUILayout.PropertyField(S_SortingType, new GUIContent("SortingType".GetLocalize()));
            DrawTextureFineTuningDataList(S_TextureFineTuningDataList);


            EditorGUI.indentLevel -= 1;

        }

        private static void DrawTextureFineTuningDataList(SerializedProperty s_fineSettings)
        {
            EditorGUILayout.LabelField("TextureFineTuning".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            for (int i = 0; s_fineSettings.arraySize > i; i += 1)
            {
                var S_fineSettingData = s_fineSettings.GetArrayElementAtIndex(i);
                var S_select = S_fineSettingData.FindPropertyRelative("Select");
                EditorGUILayout.PropertyField(S_select, new GUIContent("Setting".GetLocalize() + " " + i));
                switch (S_select.enumValueIndex)
                {
                    default:
                        {
                            EditorGUILayout.LabelField($"{S_select.enumValueIndex} enumValue Not Found");
                            break;
                        }
                    case 0:
                        {
                            var S_Resize_Size = S_fineSettingData.FindPropertyRelative("Resize_Size");
                            var S_Resize_PropertyNames = S_fineSettingData.FindPropertyRelative("Resize_PropertyNames");
                            var S_Resize_select = S_fineSettingData.FindPropertyRelative("Resize_Select");
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.PropertyField(S_Resize_Size, new GUIContent("Size".GetLocalize()));
                            PropertyNameEditor.DrawInspectorGUI(S_Resize_PropertyNames);
                            EditorGUILayout.PropertyField(S_Resize_select, new GUIContent("Select".GetLocalize()));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 1:
                        {
                            var S_Compress_FormatQuality = S_fineSettingData.FindPropertyRelative("Compress_FormatQuality");
                            var S_Compress_CompressionQuality = S_fineSettingData.FindPropertyRelative("Compress_CompressionQuality");
                            var S_Compress_PropertyNames = S_fineSettingData.FindPropertyRelative("Compress_PropertyNames");
                            var S_Compress_Select = S_fineSettingData.FindPropertyRelative("Compress_Select");
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.PropertyField(S_Compress_FormatQuality, new GUIContent("FormatQuality"));
                            EditorGUILayout.PropertyField(S_Compress_CompressionQuality, new GUIContent("CompressionQuality"));
                            PropertyNameEditor.DrawInspectorGUI(S_Compress_PropertyNames);
                            EditorGUILayout.PropertyField(S_Compress_Select, new GUIContent("Select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 2:
                        {
                            var S_ReferenceCopy_SousePropertyName = S_fineSettingData.FindPropertyRelative("ReferenceCopy_SourcePropertyName");
                            var S_ReferenceCopy_TargetPropertyName = S_fineSettingData.FindPropertyRelative("ReferenceCopy_TargetPropertyName");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_ReferenceCopy_SousePropertyName, "Source".GetLocalize() + "PropertyName".GetLocalize());
                            PropertyNameEditor.DrawInspectorGUI(S_ReferenceCopy_TargetPropertyName, "Target".GetLocalize() + "PropertyName".GetLocalize());
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 3:
                        {
                            var S_Remove_PropertyNames = S_fineSettingData.FindPropertyRelative("Remove_PropertyNames");
                            var S_Remove_Select = S_fineSettingData.FindPropertyRelative("Remove_Select");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_Remove_PropertyNames);
                            EditorGUILayout.PropertyField(S_Remove_Select, new GUIContent("Select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 4:
                        {
                            var S_MipMapRemove_PropertyNames = S_fineSettingData.FindPropertyRelative("MipMapRemove_PropertyNames");
                            var S_MipMapRemove_Select = S_fineSettingData.FindPropertyRelative("MipMapRemove_Select");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_MipMapRemove_PropertyNames);
                            EditorGUILayout.PropertyField(S_MipMapRemove_Select, new GUIContent("Select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }

                }
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("[" + "NewFineTuningSetting".GetLocalize() + "]");
            TextureTransformerEditor.DrawerArrayResizeButton(s_fineSettings, true);
        }


        List<Material> TempMaterial;
        void RefreshMaterials(SerializedProperty s_TargetRoot, GameObject NewRoot)
        {
            s_TargetRoot.objectReferenceValue = NewRoot;
            if (NewRoot == null) { return; }

            var renderers = AtlasTexture.FilteredRenderers(NewRoot);
            TempMaterial = RendererUtility.GetMaterials(renderers).Distinct().ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty TargetMaterial, List<Material> TempMaterial)
        {
            EditorGUI.indentLevel += 1;
            GUILayout.Label("Target".GetLocalize() + "       " + "Scale".GetLocalize() + "       " + "Material".GetLocalize());
            foreach (var mat in TempMaterial)
            {
                var S_MatSelector = FindMatSelector(TargetMaterial, mat);
                EditorGUILayout.BeginHorizontal();

                var isTarget = S_MatSelector != null;

                var editIsTarget = EditorGUILayout.Toggle(isTarget);
                if (isTarget != editIsTarget)
                {
                    if (editIsTarget)
                    {
                        var index = TargetMaterial.arraySize;
                        TargetMaterial.arraySize += 1;
                        var S_NewMatSelector = TargetMaterial.GetArrayElementAtIndex(index);
                        S_NewMatSelector.FindPropertyRelative("Material").objectReferenceValue = mat;
                        S_NewMatSelector.FindPropertyRelative("TextureSizeOffSet").floatValue = 1;
                    }
                    else
                    {
                        TargetMaterial.DeleteArrayElementAtIndex(FindMatSelectorIndex(TargetMaterial, mat));
                    }
                }
                else if (isTarget)
                {
                    var SOffset = S_MatSelector.FindPropertyRelative("TextureSizeOffSet");
                    SOffset.floatValue = Mathf.Clamp01(EditorGUILayout.FloatField(SOffset.floatValue));
                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MaxWidth(1000));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

        }
        public static SerializedProperty FindMatSelector(SerializedProperty TargetMaterialArray, Material material)
        {
            for (int i = 0; TargetMaterialArray.arraySize > i; i += 1)
            {
                var SectorElement = TargetMaterialArray.GetArrayElementAtIndex(i);
                var MatProp = SectorElement.FindPropertyRelative("Material");
                if (MatProp.objectReferenceValue == material)
                {
                    return SectorElement;
                }
            }
            return null;
        }
        public static int FindMatSelectorIndex(SerializedProperty TargetMaterialArray, Material material)
        {
            for (int i = 0; TargetMaterialArray.arraySize > i; i += 1)
            {
                var SectorElement = TargetMaterialArray.GetArrayElementAtIndex(i);
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
            var s_obj = new SerializedObject(target);
            var s_TargetRenderers = s_obj.FindProperty("TargetRoot");
            EditorGUILayout.PropertyField(s_TargetRenderers, s_TargetRenderers.name.GetLC());
            var S_AtlasTextureSize = s_obj.FindProperty("AtlasSetting").FindPropertyRelative("AtlasTextureSize");
            EditorGUILayout.PropertyField(S_AtlasTextureSize, S_AtlasTextureSize.name.GetLC());
            s_obj.ApplyModifiedProperties();
        }
    }
}
#endif

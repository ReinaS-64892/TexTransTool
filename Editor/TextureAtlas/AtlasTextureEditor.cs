
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using System;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomEditor(typeof(AtlasTexture), true)]
    public class AtlasTextureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            var ThisTarget = target as AtlasTexture;
            var This_S_Object = serializedObject;

            var S_AtlasSettings = This_S_Object.FindProperty("AtlasSettings");
            var ChannelCount = S_AtlasSettings.arraySize;

            var S_TargetRoot = This_S_Object.FindProperty("TargetRoot");

            var S_SelectReferenceMatList = This_S_Object.FindProperty("SelectReferenceMat");
            var S_MatSelectors = This_S_Object.FindProperty("MatSelectors");

            TextureTransformerEditor.DrawerObjectReference<GameObject>(S_TargetRoot, NewRoot =>
            {
                Undo.RecordObject(ThisTarget, "AtlasTexture - TargetRoot");
                RefreshMaterials(S_TargetRoot, NewRoot, S_SelectReferenceMatList, S_MatSelectors, ChannelCount);
                This_S_Object.ApplyModifiedProperties();
            });
            if (S_TargetRoot.objectReferenceValue != null)
            {
                if (GUILayout.Button("Refresh Materials"))
                {
                    Undo.RecordObject(ThisTarget, "AtlasTexture - SetTargetRoot");
                    RefreshMaterials(S_TargetRoot, ThisTarget.TargetRoot, S_SelectReferenceMatList, S_MatSelectors, ChannelCount);
                    This_S_Object.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Automatic OffSet Setting"))
                {
                    Undo.RecordObject(ThisTarget, "AtlasTexture - Automatic OffSet Setting");
                    ThisTarget.AutomaticOffSetSetting();
                }
                MaterialSelectEditor(S_MatSelectors, ChannelCount);
            }

            var s_UseIslandCash = This_S_Object.FindProperty("UseIslandCash");
            EditorGUILayout.PropertyField(s_UseIslandCash);


            DrawAtlasSettings(S_AtlasSettings);


            TextureTransformerEditor.DrawerApplyAndRevert(ThisTarget);
            serializedObject.ApplyModifiedProperties();

        }

        private static void DrawAtlasSettings(SerializedProperty s_AtlasSettings)
        {
            EditorGUILayout.LabelField("Atlas Settings");
            EditorGUI.indentLevel += 1;
            for (var Index = 0; Index < s_AtlasSettings.arraySize; Index += 1)
            {

                EditorGUILayout.LabelField("Channel " + Index);
                var S_Channel = s_AtlasSettings.GetArrayElementAtIndex(Index);
                var S_AtlasTextureSize = S_Channel.FindPropertyRelative("AtlasTextureSize");
                var S_IsMergeMaterial = S_Channel.FindPropertyRelative("IsMergeMaterial");
                var S_MergeReferenceMaterial = S_Channel.FindPropertyRelative("MergeReferenceMaterial");
                var S_ForceSetTexture = S_Channel.FindPropertyRelative("ForceSetTexture");
                var S_PropertyBakeSetting = S_Channel.FindPropertyRelative("PropertyBakeSetting");
                var S_PaddingType = S_Channel.FindPropertyRelative("PaddingType");
                var S_Padding = S_Channel.FindPropertyRelative("Padding");
                var S_SortingType = S_Channel.FindPropertyRelative("SortingType");
                var S_fineSettings = S_Channel.FindPropertyRelative("fineSettings");



                EditorGUILayout.PropertyField(S_AtlasTextureSize);
                EditorGUILayout.PropertyField(S_IsMergeMaterial);
                if (S_IsMergeMaterial.boolValue)
                {
                    EditorGUILayout.PropertyField(S_PropertyBakeSetting);
                    EditorGUILayout.PropertyField(S_MergeReferenceMaterial);
                }
                EditorGUILayout.PropertyField(S_ForceSetTexture);
                EditorGUILayout.PropertyField(S_PaddingType);
                EditorGUILayout.PropertyField(S_Padding);
                EditorGUILayout.PropertyField(S_SortingType);
                DrawFineSettings(S_fineSettings);

            }
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                var LastIndex = s_AtlasSettings.arraySize;
                s_AtlasSettings.arraySize += 1;
                SetDefault(s_AtlasSettings.GetArrayElementAtIndex(LastIndex));
            }
            EditorGUI.BeginDisabledGroup(s_AtlasSettings.arraySize <= 1);
            if (GUILayout.Button("-")) { s_AtlasSettings.arraySize -= 1; }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

        }

        private static void DrawFineSettings(SerializedProperty s_fineSettings)
        {
            EditorGUILayout.LabelField("Fine Settings");
            EditorGUI.indentLevel += 1;

            for (int i = 0; s_fineSettings.arraySize > i; i += 1)
            {
                var S_fineSettingData = s_fineSettings.GetArrayElementAtIndex(i);
                var S_select = S_fineSettingData.FindPropertyRelative("select");
                EditorGUILayout.PropertyField(S_select, new GUIContent("FineSetting " + i));
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
                            EditorGUILayout.PropertyField(S_Resize_Size, new GUIContent("Size"));
                            PropertyNameEditor.DrawInspectorGUI(S_Resize_PropertyNames);
                            EditorGUILayout.PropertyField(S_Resize_select, new GUIContent("Select"));
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
                            var S_ReferenceCopy_SousePropertyName = S_fineSettingData.FindPropertyRelative("ReferenceCopy_SousePropertyName");
                            var S_ReferenceCopy_TargetPropertyName = S_fineSettingData.FindPropertyRelative("ReferenceCopy_TargetPropertyName");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_ReferenceCopy_SousePropertyName, "Souse Property Name");
                            PropertyNameEditor.DrawInspectorGUI(S_ReferenceCopy_TargetPropertyName, "Target Property Name");
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

            TextureTransformerEditor.DrawerArrayResizeButton(s_fineSettings);
            EditorGUI.indentLevel -= 1;
        }

        static void SetDefault(SerializedProperty serializedProperty)
        {

            var S_AtlasTextureSize = serializedProperty.FindPropertyRelative("AtlasTextureSize");
            S_AtlasTextureSize.vector2IntValue = new Vector2Int(2048, 2048);
            var S_PaddingType = serializedProperty.FindPropertyRelative("PaddingType");
            S_PaddingType.enumValueIndex = 0;
            var S_Padding = serializedProperty.FindPropertyRelative("Padding");
            S_Padding.floatValue = -10;
            var S_SortingType = serializedProperty.FindPropertyRelative("SortingType");
            S_SortingType.enumValueIndex = 2;
        }
        static void RefreshMaterials(SerializedProperty s_TargetRoot, GameObject NewRoot, SerializedProperty S_SelectRefarensMatList, SerializedProperty S_MatSelectors, int ChannelCount)
        {
            s_TargetRoot.objectReferenceValue = NewRoot;
            if (NewRoot == null) { return; }
            var NewRefSouseMats = AtlasTexture.GetMaterials(AtlasTexture.FilteredRenderers(NewRoot.GetComponentsInChildren<Renderer>(true))).ToList(false);
            S_SelectRefarensMatList.arraySize = NewRefSouseMats.Count;
            for (var Index = 0; Index < NewRefSouseMats.Count; Index += 1)
            {
                var Mat = NewRefSouseMats[Index];
                S_SelectRefarensMatList.GetArrayElementAtIndex(Index).objectReferenceValue = Mat;
            }

            S_MatSelectors.arraySize = NewRefSouseMats.Count;

            for (var Index = 0; Index < NewRefSouseMats.Count; Index += 1)
            {
                var MatSelect = S_MatSelectors.GetArrayElementAtIndex(Index);
                var SMat = MatSelect.FindPropertyRelative("Material");
                var SChannel = MatSelect.FindPropertyRelative("AtlasChannel");


                SMat.objectReferenceValue = NewRefSouseMats[Index];
                SChannel.intValue = Mathf.Clamp(SChannel.intValue, 0, ChannelCount - 1);
            }
        }

        public static void MaterialSelectEditor(SerializedProperty TargetMaterial, int ChannelCount)
        {
            if (ChannelCount < 1) { return; }
            EditorGUI.indentLevel += 1;
            GUILayout.Label("IsTarget  (Offset) (Channel) Material");
            foreach (var Index in Enumerable.Range(0, TargetMaterial.arraySize))
            {
                var MatSelect = TargetMaterial.GetArrayElementAtIndex(Index);
                var SMat = MatSelect.FindPropertyRelative("Material");
                var SISelect = MatSelect.FindPropertyRelative("IsTarget");
                var SAtlsChannel = MatSelect.FindPropertyRelative("AtlasChannel");
                var SOffset = MatSelect.FindPropertyRelative("TextureSizeOffSet");
                EditorGUILayout.BeginHorizontal();
                SISelect.boolValue = EditorGUILayout.Toggle(SISelect.boolValue);
                if (SISelect.boolValue)
                {
                    var floatValue = EditorGUILayout.FloatField(SOffset.floatValue); SOffset.floatValue = floatValue > 0.01 ? floatValue : 1;
                    SAtlsChannel.intValue = Mathf.Clamp(EditorGUILayout.IntField(SAtlsChannel.intValue), 0, ChannelCount - 1);
                }
                EditorGUILayout.ObjectField(SMat.objectReferenceValue, typeof(Material), false, GUILayout.MaxWidth(1000));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

        }

    }
}
#endif

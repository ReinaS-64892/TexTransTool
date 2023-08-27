
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

            var S_SelectRefarensMatList = This_S_Object.FindProperty("SelectRefarensMat");
            var S_MatSelectors = This_S_Object.FindProperty("MatSelectors");

            TextureTransformerEditor.ObjectReferencePorpty<GameObject>(S_TargetRoot, NewRoot =>
            {
                Undo.RecordObject(ThisTarget, "AtlasTexture - TargetRoot");
                ResarchMaterials(S_TargetRoot, NewRoot, S_SelectRefarensMatList, S_MatSelectors, ChannelCount);
                This_S_Object.ApplyModifiedProperties();
            });
            if (S_TargetRoot.objectReferenceValue != null)
            {
                if (GUILayout.Button("ResearchRenderas"))
                {
                    Undo.RecordObject(ThisTarget, "AtlasTexture - SetTargetRoot");
                    ResarchMaterials(S_TargetRoot, ThisTarget.TargetRoot, S_SelectRefarensMatList, S_MatSelectors, ChannelCount);
                    This_S_Object.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Automatic OffSet Setting"))
                {
                    Undo.RecordObject(ThisTarget, "AtlasTexture - Automatic OffSet Setting");
                    ThisTarget.AutomaticOffSetSetting();
                }
                MaterialSelectEditor(S_MatSelectors, ChannelCount);
            }
            DrawAtlasSettings(S_AtlasSettings);


            TextureTransformerEditor.DrawerApplyAndRevert(ThisTarget);
            serializedObject.ApplyModifiedProperties();

        }

        private static void DrawAtlasSettings(SerializedProperty s_AtlasSettings)
        {
            EditorGUILayout.LabelField("AtlasSetiings");
            EditorGUI.indentLevel += 1;
            for (var Index = 0; Index < s_AtlasSettings.arraySize; Index += 1)
            {

                EditorGUILayout.LabelField("Channel " + Index);
                var S_Channel = s_AtlasSettings.GetArrayElementAtIndex(Index);
                var S_AtlasTextureSize = S_Channel.FindPropertyRelative("AtlasTextureSize");
                var S_IsMargeMaterial = S_Channel.FindPropertyRelative("IsMargeMaterial");
                var S_MargeRefarensMaterial = S_Channel.FindPropertyRelative("MargeRefarensMaterial");
                var S_ForseSetTexture = S_Channel.FindPropertyRelative("ForseSetTexture");
                var S_PropertyBakeSetting = S_Channel.FindPropertyRelative("PropertyBakeSetting");
                var S_PadingType = S_Channel.FindPropertyRelative("PadingType");
                var S_Pading = S_Channel.FindPropertyRelative("Pading");
                var S_SortingType = S_Channel.FindPropertyRelative("SortingType");
                var S_fineSettings = S_Channel.FindPropertyRelative("fineSettings");



                EditorGUILayout.PropertyField(S_AtlasTextureSize);
                EditorGUILayout.PropertyField(S_IsMargeMaterial);
                if (S_IsMargeMaterial.boolValue)
                {
                    EditorGUILayout.PropertyField(S_PropertyBakeSetting);
                    EditorGUILayout.PropertyField(S_MargeRefarensMaterial);
                }
                EditorGUILayout.PropertyField(S_ForseSetTexture);
                EditorGUILayout.PropertyField(S_PadingType);
                EditorGUILayout.PropertyField(S_Pading);
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
            EditorGUILayout.LabelField("FineSettings");
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
                            var S_Resize_select = S_fineSettingData.FindPropertyRelative("Resize_select");
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.PropertyField(S_Resize_Size, new GUIContent("Size"));
                            PropertyNameEditor.DrawInspectorGUI(S_Resize_PropertyNames);
                            EditorGUILayout.PropertyField(S_Resize_select, new GUIContent("select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 1:
                        {
                            var S_Compless_fromatQuality = S_fineSettingData.FindPropertyRelative("Compless_fromatQuality");
                            var S_Compless_compressionQuality = S_fineSettingData.FindPropertyRelative("Compless_compressionQuality");
                            var S_Compless_PropertyNames = S_fineSettingData.FindPropertyRelative("Compless_PropertyNames");
                            var S_Compless_select = S_fineSettingData.FindPropertyRelative("Compless_select");
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.PropertyField(S_Compless_fromatQuality, new GUIContent("fromatQuality"));
                            EditorGUILayout.PropertyField(S_Compless_compressionQuality, new GUIContent("compressionQuality"));
                            PropertyNameEditor.DrawInspectorGUI(S_Compless_PropertyNames);
                            EditorGUILayout.PropertyField(S_Compless_select, new GUIContent("select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 2:
                        {
                            var S_RefarensCopy_SousePropertyName = S_fineSettingData.FindPropertyRelative("RefarensCopy_SousePropertyName");
                            var S_RefarensCopy_TargetPropertyName = S_fineSettingData.FindPropertyRelative("RefarensCopy_TargetPropertyName");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_RefarensCopy_SousePropertyName);
                            PropertyNameEditor.DrawInspectorGUI(S_RefarensCopy_TargetPropertyName);
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 3:
                        {
                            var S_Remove_PropertyNames = S_fineSettingData.FindPropertyRelative("Remove_PropertyNames");
                            var S_Remove_select = S_fineSettingData.FindPropertyRelative("Remove_select");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_Remove_PropertyNames);
                            EditorGUILayout.PropertyField(S_Remove_select, new GUIContent("select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }
                    case 4:
                        {
                            var S_MipMapRemove_PropertyNames = S_fineSettingData.FindPropertyRelative("MipMapRemove_PropertyNames");
                            var S_MipMapRemove_select = S_fineSettingData.FindPropertyRelative("MipMapRemove_select");
                            EditorGUI.indentLevel += 1;
                            PropertyNameEditor.DrawInspectorGUI(S_MipMapRemove_PropertyNames);
                            EditorGUILayout.PropertyField(S_MipMapRemove_select, new GUIContent("select"));
                            EditorGUI.indentLevel -= 1;
                            break;
                        }

                }
            }

            TextureTransformerEditor.DrawerArryResizeButton(s_fineSettings);
            EditorGUI.indentLevel -= 1;
        }

        static void SetDefault(SerializedProperty serializedProperty)
        {

            var S_AtlasTextureSize = serializedProperty.FindPropertyRelative("AtlasTextureSize");
            S_AtlasTextureSize.vector2IntValue = new Vector2Int(2048, 2048);
            var S_PadingType = serializedProperty.FindPropertyRelative("PadingType");
            S_PadingType.enumValueIndex = 0;
            var S_Pading = serializedProperty.FindPropertyRelative("Pading");
            S_Pading.floatValue = -10;
            var S_SortingType = serializedProperty.FindPropertyRelative("SortingType");
            S_SortingType.enumValueIndex = 2;
        }
        static void ResarchMaterials(SerializedProperty s_TargetRoot, GameObject NewRoot, SerializedProperty S_SelectRefarensMatList, SerializedProperty S_MatSelectors, int ChannelCount)
        {
            s_TargetRoot.objectReferenceValue = NewRoot;
            if (NewRoot == null) { return; }
            var NewRefSouseMats = AtlasTexture.GetMaterials(AtlasTexture.FilterdRendarer(NewRoot.GetComponentsInChildren<Renderer>(true))).ToList(false);
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
                var SChannel = MatSelect.FindPropertyRelative("AtlsChannel");


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
                var SAtlsChannel = MatSelect.FindPropertyRelative("AtlsChannel");
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

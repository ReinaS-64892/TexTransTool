
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Rs64.TexTransTool.Editor;
using System.Collections.Generic;

namespace Rs64.TexTransTool.TexturAtlas.Editor
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






            TextureTransformerEditor.TextureTransformerEditorDrow(ThisTarget);
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
                var S_PadingType = S_Channel.FindPropertyRelative("PadingType");
                var S_Pading = S_Channel.FindPropertyRelative("Pading");
                var S_SortingType = S_Channel.FindPropertyRelative("SortingType");


                EditorGUILayout.PropertyField(S_AtlasTextureSize);
                EditorGUILayout.PropertyField(S_IsMargeMaterial);
                if (S_IsMargeMaterial.boolValue) { EditorGUILayout.PropertyField(S_MargeRefarensMaterial); }
                EditorGUILayout.PropertyField(S_ForseSetTexture);
                EditorGUILayout.PropertyField(S_PadingType);
                EditorGUILayout.PropertyField(S_Pading);
                EditorGUILayout.PropertyField(S_SortingType);

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
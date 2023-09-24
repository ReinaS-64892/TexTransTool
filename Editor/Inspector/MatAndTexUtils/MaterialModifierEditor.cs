using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MatAndTexUtils;
using System.Collections.Generic;
namespace net.rs64.TexTransTool.Editor.MatAndTexUtils
{
    [CustomEditor(typeof(MaterialModifier))]
    public class MaterialModifierEditor : UnityEditor.Editor
    {
        List<Material> TempMaterial;
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MaterialModifier");

            var This_S_Object = serializedObject;
            var ThisObject = target as MaterialModifier;

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(ThisObject));


            var s_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var s_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(s_TargetRenderers, s_MultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(s_MultiRendererMode);

            if (TempMaterial == null || GUILayout.Button("Refresh Materials")) { MatAndTexAbsoluteSeparatorEditor.RefreshMaterials(s_TargetRenderers, ref TempMaterial); }
            var s_ModifiedTarget = This_S_Object.FindProperty("ModifiedTarget");
            MatAndTexAbsoluteSeparatorEditor.MaterialSelectEditor(s_ModifiedTarget, TempMaterial, "Target? Material");


            EditorGUILayout.LabelField("Modify Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var s_ChangeList = This_S_Object.FindProperty("ChangeList");

            for (var i = 0; s_ChangeList.arraySize > i; i += 1)
            {
                EditorGUILayout.LabelField("Setting " + i + "---");
                var s_Change = s_ChangeList.GetArrayElementAtIndex(i);
                var s_ModTypeEnum = s_Change.FindPropertyRelative("ModType");
                EditorGUILayout.PropertyField(s_ModTypeEnum);
                switch (s_ModTypeEnum.enumValueIndex)
                {
                    case 0://Float
                        {
                            EditorGUILayout.PropertyField(s_Change.FindPropertyRelative("Float_PropertyName"));
                            EditorGUILayout.PropertyField(s_Change.FindPropertyRelative("Float_Value"));
                            break;
                        }
                    case 1://Texture
                        {
                            PropertyNameEditor.DrawInspectorGUI(s_Change.FindPropertyRelative("Texture_PropertyName"));
                            EditorGUILayout.PropertyField(s_Change.FindPropertyRelative("Texture_Value"));
                            break;
                        }
                    case 2://Color
                        {
                            EditorGUILayout.PropertyField(s_Change.FindPropertyRelative("Color_PropertyName"));
                            EditorGUILayout.PropertyField(s_Change.FindPropertyRelative("Color_Value"));
                            break;
                        }
                }
            }

            TextureTransformerEditor.DrawerArrayResizeButton(s_ChangeList);
            EditorGUI.indentLevel -= 1;
            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(ThisObject);
            This_S_Object.ApplyModifiedProperties();
        }
    }
}

using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MatAndTexUtils;
using System.Collections.Generic;
namespace net.rs64.TexTransTool.Editor.MatAndTexUtils
{
    [CustomEditor(typeof(MaterialModifier))]
    internal class MaterialModifierEditor : UnityEditor.Editor
    {
        List<Material> _tempMaterial;
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MaterialModifier");

            var thisSObject = serializedObject;
            var thisObject = target as MaterialModifier;

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(thisObject));


            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode);

            if (_tempMaterial == null || GUILayout.Button("Refresh Materials")) { MatAndTexAbsoluteSeparatorEditor.RefreshMaterials(sTargetRenderers, ref _tempMaterial); }
            var sModifiedTarget = thisSObject.FindProperty("ModifiedTarget");
            MatAndTexAbsoluteSeparatorEditor.MaterialSelectEditor(sModifiedTarget, _tempMaterial, "Target? Material");


            EditorGUILayout.LabelField("Modify Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sChangeList = thisSObject.FindProperty("ChangeList");

            for (var i = 0; sChangeList.arraySize > i; i += 1)
            {
                EditorGUILayout.LabelField("Setting " + i + "---");
                var sChange = sChangeList.GetArrayElementAtIndex(i);
                var sModTypeEnum = sChange.FindPropertyRelative("ModType");
                EditorGUILayout.PropertyField(sModTypeEnum);
                switch (sModTypeEnum.enumValueIndex)
                {
                    case 0://Float
                        {
                            EditorGUILayout.PropertyField(sChange.FindPropertyRelative("Float_PropertyName"));
                            EditorGUILayout.PropertyField(sChange.FindPropertyRelative("Float_Value"));
                            break;
                        }
                    case 1://Texture
                        {
                            PropertyNameEditor.DrawInspectorGUI(sChange.FindPropertyRelative("Texture_PropertyName"));
                            EditorGUILayout.PropertyField(sChange.FindPropertyRelative("Texture_Value"));
                            break;
                        }
                    case 2://Color
                        {
                            EditorGUILayout.PropertyField(sChange.FindPropertyRelative("Color_PropertyName"));
                            EditorGUILayout.PropertyField(sChange.FindPropertyRelative("Color_Value"));
                            break;
                        }
                }
            }

            TextureTransformerEditor.DrawerArrayResizeButton(sChangeList);
            EditorGUI.indentLevel -= 1;
            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(thisObject);
            thisSObject.ApplyModifiedProperties();
        }
    }
}

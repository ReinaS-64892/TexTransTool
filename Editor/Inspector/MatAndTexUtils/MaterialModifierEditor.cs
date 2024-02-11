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
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, "CommonDecal:prop:TargetRenderer".Glc(), sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode);

            if (_tempMaterial == null || GUILayout.Button("Refresh Materials")) { MatAndTexAbsoluteSeparatorEditor.RefreshMaterials(sTargetRenderers, ref _tempMaterial); }
            var sModifiedTarget = thisSObject.FindProperty("ModifiedTarget");
            MatAndTexAbsoluteSeparatorEditor.MaterialSelectEditor(sModifiedTarget, _tempMaterial, "Target? Material");


            EditorGUILayout.LabelField("Modify Settings", EditorStyles.boldLabel);

            var sChangeList = thisSObject.FindProperty("ChangeList");
            EditorGUILayout.PropertyField(sChangeList);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(thisObject);
            thisSObject.ApplyModifiedProperties();
        }
    }


    [CustomPropertyDrawer(typeof(MaterialModifier.MatMod))]
    public class MatModDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sChange = property;
            var sModTypeEnum = sChange.FindPropertyRelative("ModType");
            position.height = 18;
            EditorGUI.PropertyField(position, sModTypeEnum);
            position.y += position.height;
            switch (sModTypeEnum.enumValueIndex)
            {
                case 0://Float
                    {
                        EditorGUI.PropertyField(position, sChange.FindPropertyRelative("Float_PropertyName"));
                        position.y += position.height;
                        EditorGUI.PropertyField(position, sChange.FindPropertyRelative("Float_Value"));
                        position.y += position.height;
                        break;
                    }
                case 1://Texture
                    {
                        EditorGUI.PropertyField(position, sChange.FindPropertyRelative("Texture_Value"));
                        position.y += position.height;
                        EditorGUI.PropertyField(position, sChange.FindPropertyRelative("Texture_PropertyName"));
                        position.y += position.height;
                        break;
                    }
                case 2://Color
                    {
                        EditorGUI.PropertyField(position, sChange.FindPropertyRelative("Color_PropertyName"));
                        position.y += position.height;
                        EditorGUI.PropertyField(position, sChange.FindPropertyRelative("Color_Value"));
                        position.y += position.height;
                        break;
                    }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18 * 3;
        }
    }
}

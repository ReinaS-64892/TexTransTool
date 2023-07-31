#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AvatarDomainDefinition), true)]
    public class AvatarDomainDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Avatar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GenereatCustomMipMap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TexTransGroup"));

            var thsitarget = target as AvatarDomainDefinition;

            if (thsitarget.TexTransGroup == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("TexTransGroup is null");
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUI.BeginDisabledGroup(!thsitarget.TexTransGroup.IsPossibleApply);
            if (thsitarget.TexTransGroup.IsApply == false)
            {
                if (GUILayout.Button("MaterialDomainUse - Apply"))
                {
                    Undo.RecordObject(thsitarget, "AvatarMaterialDomain - Apply");
                    thsitarget.Apply();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!thsitarget.IsSelfCallApply);
                if (GUILayout.Button("Revart"))
                {
                    Undo.RecordObject(thsitarget, "AvatarMaterialDomain - Revart");
                    thsitarget.Revart();

                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
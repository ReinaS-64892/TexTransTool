#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Bulige;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(AvatarDomainDefinition), true)]
    public class AvatarDomainDefinitionEditor : UnityEditor.Editor
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
                    thsitarget.Apply();
                    EditorUtility.SetDirty(thsitarget);
                    EditorUtility.SetDirty(thsitarget.TexTransGroup);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!thsitarget.IsSelfCallApply);
                if (GUILayout.Button("Revart"))
                {
                    thsitarget.Revart();
                    EditorUtility.SetDirty(thsitarget);
                    EditorUtility.SetDirty(thsitarget.TexTransGroup);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
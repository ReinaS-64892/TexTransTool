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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GenerateCustomMipMap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TexTransGroup"));

            var thistarget = target as AvatarDomainDefinition;

            if (thistarget.TexTransGroup == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("TexTransGroup is null");
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUI.BeginDisabledGroup(!thistarget.TexTransGroup.IsPossibleApply);
            if (thistarget.TexTransGroup.IsApply == false)
            {
                if (GUILayout.Button("MaterialDomainUse - Apply"))
                {
                    thistarget.Apply();
                    EditorUtility.SetDirty(thistarget);
                    EditorUtility.SetDirty(thistarget.TexTransGroup);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!thistarget.IsSelfCallApply);
                if (GUILayout.Button("Revert"))
                {
                    thistarget.Revert();
                    EditorUtility.SetDirty(thistarget);
                    EditorUtility.SetDirty(thistarget.TexTransGroup);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

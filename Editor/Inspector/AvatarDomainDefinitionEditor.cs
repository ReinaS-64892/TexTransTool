#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(AvatarDomainDefinition), true)]
    public class AvatarDomainDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thisTarget = target as AvatarDomainDefinition;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Avatar"), new GUIContent("Preview Avatar"));

            EditorGUI.BeginDisabledGroup(!thisTarget.TexTransGroup.IsPossibleApply || thisTarget.Avatar == null);
            if (!PreviewContext.IsPreviewing(thisTarget.TexTransGroup))
            {
                if (GUILayout.Button("Preview - AvatarDomain-Apply"))
                {
                    thisTarget.Apply();
                    EditorUtility.SetDirty(thisTarget);
                    EditorUtility.SetDirty(thisTarget.TexTransGroup);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!thisTarget.IsSelfCallApply);
                if (GUILayout.Button("Revert"))
                {
                    thisTarget.Revert();
                    EditorUtility.SetDirty(thisTarget);
                    EditorUtility.SetDirty(thisTarget.TexTransGroup);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

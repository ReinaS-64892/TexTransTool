#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(AvatarDomainDefinition), true)]
    public class AvatarDomainDefinitionEditor : UnityEditor.Editor
    {
        private PreviewAvatarDomain _domain;
        private bool isPreviewing;

        public override void OnInspectorGUI()
        {
            var thisTarget = target as AvatarDomainDefinition;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Avatar"), new GUIContent("Preview Avatar"));

            EditorGUI.BeginDisabledGroup(!thisTarget.TexTransGroup.IsPossibleApply || thisTarget.Avatar == null);
            PreviewContext.instance.DrawApplyAndRevert(thisTarget);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

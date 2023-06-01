#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AvatarMaterialDomain), true)]
    public class AvatarMaterialDomainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Avatar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TexTransGroup"));

            var thsitarget = target as AvatarMaterialDomain;

            if (thsitarget.TexTransGroup == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("TexTransGroup is null");
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUI.BeginDisabledGroup(!thsitarget.TexTransGroup.IsPossibleAppry);
            if (thsitarget.TexTransGroup.IsAppry == false)
            {
                if (GUILayout.Button("MaterialDomainUse - Appry"))
                {
                    Undo.RecordObject(thsitarget, "AvatarMaterialDomain - Appry");
                    thsitarget.Appry();
                }
            }
            else
            {
                if (GUILayout.Button("Revart"))
                {
                    Undo.RecordObject(thsitarget, "AvatarMaterialDomain - Revart");
                    thsitarget.Revart();

                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
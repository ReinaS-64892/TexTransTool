using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.ReferenceResolver;
using net.rs64.TexTransTool.ReferenceResolver.ATResolver;

namespace net.rs64.TexTransTool.Editor.ReferenceResolver
{
    [CustomEditor(typeof(RootObject), true)]
    public class RootObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("RootObject");

            var s_Obj = serializedObject;

            var s_SelectType = s_Obj.FindProperty("SelectType");
            EditorGUILayout.PropertyField(s_SelectType);

            switch (s_SelectType.enumValueIndex)
            {
                case 0://AvatarRoot
                    {
                        break;
                    }
                case 1://RootFormPath
                    {
                        var s_RootFormPath = s_Obj.FindProperty("RootFormPath");
                        EditorGUILayout.PropertyField(s_RootFormPath);
                        break;
                    }
            }

            s_Obj.ApplyModifiedProperties();
        }
    }
}
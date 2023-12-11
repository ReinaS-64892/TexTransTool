using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.ReferenceResolver;
using net.rs64.TexTransTool.ReferenceResolver.ATResolver;

namespace net.rs64.TexTransTool.Editor.ReferenceResolver
{
    [CustomEditor(typeof(RootObject), true)]
    internal class RootObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("RootObject");

            var sObj = serializedObject;

            var sSelectType = sObj.FindProperty("SelectType");
            EditorGUILayout.PropertyField(sSelectType);

            switch (sSelectType.enumValueIndex)
            {
                case 0://AvatarRoot
                    {
                        break;
                    }
                case 1://RootFormPath
                    {
                        var sRootFormPath = sObj.FindProperty("RootFormPath");
                        EditorGUILayout.PropertyField(sRootFormPath);
                        break;
                    }
            }

            sObj.ApplyModifiedProperties();
        }
    }
}
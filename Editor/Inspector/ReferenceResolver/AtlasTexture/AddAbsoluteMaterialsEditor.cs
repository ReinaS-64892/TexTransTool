using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.ReferenceResolver;
using net.rs64.TexTransTool.ReferenceResolver.ATResolver;

namespace net.rs64.TexTransTool.Editor.ReferenceResolver
{
    [CustomEditor(typeof(AddAbsoluteMaterials), true)]
    public class AddAbsoluteMaterialsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("AddAbsoluteMaterials");
            var s_Obj = serializedObject;
            var t_Obj = target as AddAbsoluteMaterials;

            var s_AddSelectors = s_Obj.FindProperty("AddSelectors");

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Count".GetLocalize() + "-" + "TextureSizeOffSet".GetLocalize()  + " : " + "Material".GetLocalize() );
            GUILayout.EndHorizontal();
            for (int i = 0; i < s_AddSelectors.arraySize; i += 1)
            {
                var selector = s_AddSelectors.GetArrayElementAtIndex(i);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(10f));
                EditorGUILayout.PropertyField(selector.FindPropertyRelative("TextureSizeOffSet"), GUIContent.none);
                EditorGUILayout.PropertyField(selector.FindPropertyRelative("Material"), GUIContent.none);
                if (GUILayout.Button("X")) { s_AddSelectors.DeleteArrayElementAtIndex(i); break; }
                GUILayout.EndHorizontal();
            }

            var addMat = EditorGUILayout.ObjectField("Add Material".GetLocalize(), null, typeof(Material), false) as Material;
            if (addMat != null && t_Obj.AddSelectors.FindIndex(I => I.Material == addMat) == -1)
            {
                var index = s_AddSelectors.arraySize;
                s_AddSelectors.arraySize += 1;

                var newSelector = s_AddSelectors.GetArrayElementAtIndex(index);
                newSelector.FindPropertyRelative("Material").objectReferenceValue = addMat;
                newSelector.FindPropertyRelative("TextureSizeOffSet").floatValue = 1;
            }





            s_Obj.ApplyModifiedProperties();
        }
    }
}
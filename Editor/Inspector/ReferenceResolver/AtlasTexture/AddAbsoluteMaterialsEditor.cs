using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.ReferenceResolver;
using net.rs64.TexTransTool.ReferenceResolver.ATResolver;

namespace net.rs64.TexTransTool.Editor.ReferenceResolver
{
    [CustomEditor(typeof(AddAbsoluteMaterials), true)]
    internal class AddAbsoluteMaterialsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("AddAbsoluteMaterials");
            var sObj = serializedObject;
            var obj = target as AddAbsoluteMaterials;

            var sAddSelectors = sObj.FindProperty("AddSelectors");

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Count".GetLocalize() + "-" + "TextureSizeOffSet".GetLocalize()  + " : " + "Material".GetLocalize() );
            GUILayout.EndHorizontal();
            for (int i = 0; i < sAddSelectors.arraySize; i += 1)
            {
                var selector = sAddSelectors.GetArrayElementAtIndex(i);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(10f));
                EditorGUILayout.PropertyField(selector.FindPropertyRelative("AdditionalTextureSizeOffSet"), GUIContent.none);
                EditorGUILayout.PropertyField(selector.FindPropertyRelative("Material"), GUIContent.none);
                if (GUILayout.Button("X")) { sAddSelectors.DeleteArrayElementAtIndex(i); break; }
                GUILayout.EndHorizontal();
            }

            var addMat = EditorGUILayout.ObjectField("Add Material".GetLocalize(), null, typeof(Material), false) as Material;
            if (addMat != null && obj.AddSelectors.FindIndex(I => I.Material == addMat) == -1)
            {
                var index = sAddSelectors.arraySize;
                sAddSelectors.arraySize += 1;

                var newSelector = sAddSelectors.GetArrayElementAtIndex(index);
                newSelector.FindPropertyRelative("Material").objectReferenceValue = addMat;
                newSelector.FindPropertyRelative("AdditionalTextureSizeOffSet").floatValue = 1;
            }





            sObj.ApplyModifiedProperties();
        }
    }
}
using UnityEditor;
using UnityEngine;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TileAtlasBreaker))]
    internal class TileAtlasBreakerEditor : TexTransMonoBaseEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var sObj = serializedObject;

            var sTargetMaterial = sObj.FindProperty(nameof(TileAtlasBreaker.TargetMaterial));
            var sOriginalMaterials = sObj.FindProperty(nameof(TileAtlasBreaker.OriginalMaterials));


            EditorGUILayout.PropertyField(sTargetMaterial, nameof(TileAtlasBreaker.TargetMaterial).GlcV());
            EditorGUILayout.PropertyField(sOriginalMaterials, nameof(TileAtlasBreaker.OriginalMaterials).GlcV());
            if (sOriginalMaterials.isExpanded is false)
            {
                var tMat = sTargetMaterial.objectReferenceValue;
                if (tMat != null && tMat is Material tMat2)
                    EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.MinHeight(256f)), tMat2.mainTexture, ScaleMode.ScaleToFit);

                sOriginalMaterials.arraySize = 4;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(sOriginalMaterials.GetArrayElementAtIndex(2), GUIContent.none);
                    EditorGUILayout.PropertyField(sOriginalMaterials.GetArrayElementAtIndex(3), GUIContent.none);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(sOriginalMaterials.GetArrayElementAtIndex(0), GUIContent.none);
                    EditorGUILayout.PropertyField(sOriginalMaterials.GetArrayElementAtIndex(1), GUIContent.none);
                }
            }

            sObj.ApplyModifiedProperties();
        }
    }
}

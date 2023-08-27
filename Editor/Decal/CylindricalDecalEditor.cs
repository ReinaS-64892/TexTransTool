#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.Editor;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(CylindricalDecal), true)]
    public class CylindricalDecalEditor : UnityEditor.Editor
    {
        bool FoldoutOption;

        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as CylindricalDecal;


            EditorGUI.BeginDisabledGroup(ThisObject.IsApply);

            var cylindricalCoordinatesSystem = This_S_Object.FindProperty("cylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(cylindricalCoordinatesSystem);

            AbstructSingleDecalEditor.DrowDecalEditor(This_S_Object);

            EditorGUILayout.LabelField("ScaleSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var S_Scale = This_S_Object.FindProperty("Scale");
            var S_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            AbstructSingleDecalEditor.DorwScaileEditor(ThisObject, This_S_Object, S_Scale, S_FixedAspect);
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.LabelField("CullingSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var s_SideCulling = This_S_Object.FindProperty("SideCulling");
            EditorGUILayout.PropertyField(s_SideCulling);
            var s_FarCulling = This_S_Object.FindProperty("OutDistansCulling");
            EditorGUILayout.PropertyField(s_FarCulling, new GUIContent("Far Culling OffSet"));
            var s_NierCullingOffSet = This_S_Object.FindProperty("InDistansCulling");
            EditorGUILayout.PropertyField(s_NierCullingOffSet, new GUIContent("Nier Culling OffSet"));
            EditorGUI.indentLevel -= 1;


            FoldoutOption = EditorGUILayout.Foldout(FoldoutOption, "Advanced Option");
            if (FoldoutOption)
            {
                EditorGUI.indentLevel += 1;
                var s_FastMode = This_S_Object.FindProperty("FastMode");
                EditorGUILayout.PropertyField(s_FastMode, new GUIContent("FastMode"));

                var s_Pading = This_S_Object.FindProperty("Pading");
                EditorGUILayout.PropertyField(s_Pading, new GUIContent("Pading"));

                var s_IsSeparateMatAndTexture = This_S_Object.FindProperty("IsSeparateMatAndTexture");
                EditorGUILayout.PropertyField(s_IsSeparateMatAndTexture, new GUIContent("SeparateMaterialAndTexture"));
                EditorGUI.indentLevel -= 1;
            }

            EditorGUI.EndDisabledGroup();

            TextureTransformerEditor.DrowApplyAndRevart(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }


    }


}
#endif
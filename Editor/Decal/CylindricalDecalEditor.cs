#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;

namespace Rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(CylindricalDecal), true)]
    public class CylindricalDecalEditor : UnityEditor.Editor
    {
        bool FordiantAdvansd;
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as CylindricalDecal;

            EditorGUI.BeginDisabledGroup(ThisObject.IsApply);

            AbstractDecalEditor.DrowDecalEditor(This_S_Object);

            var S_Scale = This_S_Object.FindProperty("Scale");
            var S_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            AbstractDecalEditor.DorwScaileEditor(ThisObject, This_S_Object, S_Scale, S_FixedAspect);
            EditorGUILayout.PropertyField(S_FixedAspect);

            var S_SideChek = This_S_Object.FindProperty("SideChek");
            EditorGUILayout.PropertyField(S_SideChek);
            var S_FarCulling = This_S_Object.FindProperty("FarCulling");
            EditorGUILayout.PropertyField(S_FarCulling);
            var S_NierCullingOffSet = This_S_Object.FindProperty("NierCullingOffSet");
            EditorGUILayout.PropertyField(S_NierCullingOffSet);


            var cylindricalCoordinatesSystem = This_S_Object.FindProperty("cylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(cylindricalCoordinatesSystem);

            EditorGUI.EndDisabledGroup();

            TextureTransformerEditor.TextureTransformerEditorDrow(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }


    }


}
#endif
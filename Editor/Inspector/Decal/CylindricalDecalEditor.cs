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
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("CylindricalDecal");

            var This_S_Object = serializedObject;
            var ThisObject = target as CylindricalDecal;


            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(ThisObject));

            var cylindricalCoordinatesSystem = This_S_Object.FindProperty("cylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(cylindricalCoordinatesSystem);

            AbstractDecalEditor.DrawerDecalEditor(This_S_Object);

            EditorGUILayout.LabelField("ScaleSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var S_Scale = This_S_Object.FindProperty("Scale");
            var S_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            AbstractDecalEditor.DrawerScaleEditor(ThisObject, This_S_Object, S_Scale, S_FixedAspect);
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.LabelField("CullingSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var s_SideCulling = This_S_Object.FindProperty("SideCulling");
            EditorGUILayout.PropertyField(s_SideCulling);
            var s_FarCulling = This_S_Object.FindProperty("OutDistanceCulling");
            EditorGUILayout.PropertyField(s_FarCulling, new GUIContent("Far Culling OffSet"));
            var s_NearCullingOffSet = This_S_Object.FindProperty("InDistanceCulling");
            EditorGUILayout.PropertyField(s_NearCullingOffSet, new GUIContent("Near Culling OffSet"));
            EditorGUI.indentLevel -= 1;


            AbstractDecalEditor.DrawerAdvancedOption(This_S_Object);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }


    }


}
#endif

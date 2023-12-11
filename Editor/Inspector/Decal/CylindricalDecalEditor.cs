#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransCore.Decal;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(CylindricalDecal), true)]
    internal class CylindricalDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("CylindricalDecal");

            var thisSObject = serializedObject;
            var thisObject = target as CylindricalDecal;


            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(thisObject));

            var sCylindricalCoordinatesSystem = thisSObject.FindProperty("CylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(sCylindricalCoordinatesSystem);

            AbstractDecalEditor.DrawerDecalEditor(thisSObject);

            if (targets.Length == 1)
            {
                var tf_sObg = new SerializedObject(thisObject.transform);
                SimpleDecalEditor.DrawerScale(thisSObject, tf_sObg, thisObject.DecalTexture);
                tf_sObg.ApplyModifiedProperties();
            }



            EditorGUILayout.LabelField("CullingSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var sSideCulling = thisSObject.FindProperty("SideCulling");
            EditorGUILayout.PropertyField(sSideCulling);
            var sFarCulling = thisSObject.FindProperty("OutDistanceCulling");
            EditorGUILayout.PropertyField(sFarCulling, new GUIContent("Far Culling OffSet"));
            var sNearCullingOffSet = thisSObject.FindProperty("InDistanceCulling");
            EditorGUILayout.PropertyField(sNearCullingOffSet, new GUIContent("Near Culling OffSet"));
            EditorGUI.indentLevel -= 1;


            AbstractDecalEditor.DrawerAdvancedOption(thisSObject);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(thisObject);

            thisSObject.ApplyModifiedProperties();
        }


    }


}
#endif

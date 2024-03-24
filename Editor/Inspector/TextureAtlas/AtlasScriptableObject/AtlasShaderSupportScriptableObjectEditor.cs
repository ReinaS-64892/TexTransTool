using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using net.rs64.TexTransTool.Editor;
using System;
namespace net.rs64.TexTransTool.TextureAtlas.Editor
{

    [CustomEditor(typeof(AtlasShaderSupportScriptableObject))]
    public class AtlasShaderSupportScriptableObjectEditor : UnityEditor.Editor
    {
        string[] comparers = new[] { "ContainsName", "ShaderReference" };
        Func<ISupportedShaderComparer>[] comparerGet = new Func<ISupportedShaderComparer>[] { () => new ContainsName(), () => new ShaderReference() };

        string[] protProses = new[] { "TextureReferenceCopy" };
        Func<IAtlasMaterialPostProses>[] protProsesGet = new Func<IAtlasMaterialPostProses>[] { () => new TextureReferenceCopy() };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            TextureTransformerEditor.DrawerWarning("AtlasShaderSupportScriptableObject");

            var sSupportedShaderComparer = serializedObject.FindProperty("SupportedShaderComparer");
            var reSelect = EditorGUILayout.Popup("Comparer Re Select", -1, comparers);
            if (reSelect != -1) { sSupportedShaderComparer.managedReferenceValue = comparerGet[reSelect].Invoke(); }
            EditorGUILayout.PropertyField(sSupportedShaderComparer);


            var sAtlasTargetDefines = serializedObject.FindProperty("AtlasTargetDefines");
            EditorGUILayout.PropertyField(sAtlasTargetDefines);


            var sAtlasMaterialPostProses = serializedObject.FindProperty("AtlasMaterialPostProses");
            var addPP = EditorGUILayout.Popup("Add AtlasMaterialPostProses", -1, protProses);
            if (addPP != -1)
            {
                var lastIndex = sAtlasMaterialPostProses.arraySize;
                sAtlasMaterialPostProses.arraySize += 1;
                sAtlasMaterialPostProses.GetArrayElementAtIndex(lastIndex).managedReferenceValue = protProsesGet[addPP].Invoke();
            }
            EditorGUILayout.PropertyField(sAtlasMaterialPostProses);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

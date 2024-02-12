using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.MatAndTexUtils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor.MatAndTexUtils
{
    [UnityEditor.CustomEditor(typeof(MatAndTexAbsoluteSeparator))]
    internal class MatAndTexAbsoluteSeparatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MatAndTexAbsoluteSeparator");

            var thisSObject = serializedObject;
            var thisObject = target as MatAndTexAbsoluteSeparator;

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(thisObject));

            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, "CommonDecal:prop:TargetRenderer".Glc(), sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode);

            if (_tempMaterial == null || GUILayout.Button("Refresh Materials")) { RefreshMaterials(sTargetRenderers, ref _tempMaterial); }
            var sSeparateTarget = thisSObject.FindProperty("SeparateTarget");
            MaterialSelectEditor(sSeparateTarget, _tempMaterial);

            if (thisObject.SeparateTarget.Any(I => I == null)) { Undo.RecordObject(thisObject, "SeparateTarget Remove Null"); thisObject.SeparateTarget.RemoveAll(I => I == null); }

            var sIsTextureSeparate = thisSObject.FindProperty("IsTextureSeparate");
            EditorGUILayout.PropertyField(sIsTextureSeparate);

            var sPropertyName = thisSObject.FindProperty("PropertyName");
            EditorGUILayout.PropertyField(sPropertyName);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(thisObject);

            thisSObject.ApplyModifiedProperties();
        }

        List<Material> _tempMaterial;
        public static void RefreshMaterials(SerializedProperty sTargetRenderers, ref List<Material> tempMaterial)
        {
            var renderer = new List<Renderer>();
            for (var i = 0; sTargetRenderers.arraySize > i; i += 1)
            {
                var rendererValue = sTargetRenderers.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;
                if (rendererValue != null) { renderer.Add(rendererValue); }
            }
            tempMaterial = RendererUtility.GetMaterials(renderer).Distinct().ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty targetMaterials, List<Material> tempMaterial, string Label = "Separate?         Material")
        {
            EditorGUI.indentLevel += 1;
            GUILayout.Label(Label);
            foreach (var mat in tempMaterial)
            {
                var sMatSelector = FindMatSelector(targetMaterials, mat);
                EditorGUILayout.BeginHorizontal();

                var isTarget = sMatSelector != null;

                var editIsTarget = EditorGUILayout.Toggle(isTarget);
                if (isTarget != editIsTarget)
                {
                    if (editIsTarget)
                    {
                        var index = targetMaterials.arraySize;
                        targetMaterials.arraySize += 1;
                        targetMaterials.GetArrayElementAtIndex(index).objectReferenceValue = mat;
                    }
                    else
                    {
                        targetMaterials.DeleteArrayElementAtIndex(FindMatSelectorIndex(targetMaterials, mat));
                    }
                }


                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MaxWidth(1000));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

        }
        public static SerializedProperty FindMatSelector(SerializedProperty targetMaterialArray, Material material)
        {
            for (int i = 0; targetMaterialArray.arraySize > i; i += 1)
            {
                var materialElement = targetMaterialArray.GetArrayElementAtIndex(i);
                if (materialElement.objectReferenceValue == material)
                {
                    return materialElement;
                }
            }
            return null;
        }
        public static int FindMatSelectorIndex(SerializedProperty targetMaterialArray, Material material)
        {
            for (int i = 0; targetMaterialArray.arraySize > i; i += 1)
            {
                var materialElement = targetMaterialArray.GetArrayElementAtIndex(i);
                if (materialElement.objectReferenceValue == material)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void DrawerSummary(MatAndTexAbsoluteSeparator target)
        {
            var sObj = new SerializedObject(target);
            var sTargetRenderers = sObj.FindProperty("TargetRenderers");
            TextureTransformerEditor.DrawerTargetRenderersSummary(sTargetRenderers,sTargetRenderers.name.Glc());
            sObj.ApplyModifiedProperties();
        }
    }
}

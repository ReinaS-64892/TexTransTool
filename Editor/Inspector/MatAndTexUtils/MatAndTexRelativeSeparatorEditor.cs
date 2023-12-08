#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.MatAndTexUtils;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor.MatAndTexUtils
{
    [UnityEditor.CustomEditor(typeof(MatAndTexRelativeSeparator))]
    internal class MatAndTexRelativeSeparatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MatAndTexRelativeSeparator");

            var This_S_Object = serializedObject;
            var ThisObject = target as MatAndTexRelativeSeparator;

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(ThisObject));

            var s_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var s_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(s_TargetRenderers, s_MultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(s_MultiRendererMode);

            var s_SeparateTarget = This_S_Object.FindProperty("SeparateTarget");
            if (s_SeparateTarget.arraySize != ThisObject.TargetRenderers.Count) { s_SeparateTarget.arraySize = ThisObject.TargetRenderers.Count; }

            EditorGUILayout.LabelField("---");

            int rendererIndex = 0;
            foreach (var renderer in ThisObject.TargetRenderers)
            {
                if (renderer == null) { continue; }
                var s_selectRd = s_SeparateTarget.GetArrayElementAtIndex(rendererIndex).FindPropertyRelative("Bools");
                var materials = renderer.sharedMaterials;
                if (s_selectRd.arraySize != materials.Length) { s_selectRd.arraySize = materials.Length; }
                int slotIndex = 0;
                foreach (var MatSlot in materials)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(s_selectRd.GetArrayElementAtIndex(slotIndex));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(MatSlot, typeof(Material), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    slotIndex += 1;
                }
                rendererIndex += 1;

                EditorGUILayout.LabelField("---");
            }

            var s_IsTextureSeparate = This_S_Object.FindProperty("IsTextureSeparate");
            EditorGUILayout.PropertyField(s_IsTextureSeparate);

            var s_PropertyName = This_S_Object.FindProperty("PropertyName");
            PropertyNameEditor.DrawInspectorGUI(s_PropertyName);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }

        List<Material> TempMaterial;
        void RefreshMaterials(SerializedProperty s_TargetRenderers)
        {
            var renderer = new List<Renderer>();
            for (var i = 0; s_TargetRenderers.arraySize > i; i += 1)
            {
                var rendererValue = s_TargetRenderers.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;
                if (rendererValue != null) { renderer.Add(rendererValue); }
            }
            TempMaterial = RendererUtility.GetMaterials(renderer).Distinct().ToList();
        }

        public static void MaterialSelectEditor(SerializedProperty TargetMaterials, List<Material> TempMaterial)
        {
            EditorGUI.indentLevel += 1;
            GUILayout.Label("Separate?         Material");
            foreach (var mat in TempMaterial)
            {
                var S_MatSelector = FindMatSelector(TargetMaterials, mat);
                EditorGUILayout.BeginHorizontal();

                var isTarget = S_MatSelector != null;

                var editIsTarget = EditorGUILayout.Toggle(isTarget);
                if (isTarget != editIsTarget)
                {
                    if (editIsTarget)
                    {
                        var index = TargetMaterials.arraySize;
                        TargetMaterials.arraySize += 1;
                        TargetMaterials.GetArrayElementAtIndex(index).objectReferenceValue = mat;
                    }
                    else
                    {
                        TargetMaterials.DeleteArrayElementAtIndex(FindMatSelectorIndex(TargetMaterials, mat));
                    }
                }


                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MaxWidth(1000));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

        }
        public static SerializedProperty FindMatSelector(SerializedProperty TargetMaterialArray, Material material)
        {
            for (int i = 0; TargetMaterialArray.arraySize > i; i += 1)
            {
                var materialElement = TargetMaterialArray.GetArrayElementAtIndex(i);
                if (materialElement.objectReferenceValue == material)
                {
                    return materialElement;
                }
            }
            return null;
        }
        public static int FindMatSelectorIndex(SerializedProperty TargetMaterialArray, Material material)
        {
            for (int i = 0; TargetMaterialArray.arraySize > i; i += 1)
            {
                var materialElement = TargetMaterialArray.GetArrayElementAtIndex(i);
                if (materialElement.objectReferenceValue == material)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void DrawerSummary(MatAndTexRelativeSeparator target)
        {
            var s_obj = new SerializedObject(target);
            var s_TargetRenderers = s_obj.FindProperty("TargetRenderers");
            TextureTransformerEditor.DrawerTargetRenderersSummary(s_TargetRenderers);
            s_obj.ApplyModifiedProperties();
        }
    }
}
#endif

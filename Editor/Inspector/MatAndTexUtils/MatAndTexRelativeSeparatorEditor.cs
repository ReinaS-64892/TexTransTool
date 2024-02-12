using net.rs64.TexTransTool.MatAndTexUtils;
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

            var thisSObject = serializedObject;
            var thisObject = target as MatAndTexRelativeSeparator;

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(thisObject));

            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, "CommonDecal:prop:TargetRenderer".Glc(), sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode);

            var sSeparateTarget = thisSObject.FindProperty("SeparateTarget");
            if (sSeparateTarget.arraySize != thisObject.TargetRenderers.Count) { sSeparateTarget.arraySize = thisObject.TargetRenderers.Count; }

            EditorGUILayout.LabelField("---");

            int rendererIndex = 0;
            foreach (var renderer in thisObject.TargetRenderers)
            {
                if (renderer == null) { continue; }
                var sSelectRd = sSeparateTarget.GetArrayElementAtIndex(rendererIndex).FindPropertyRelative("Bools");
                var materials = renderer.sharedMaterials;
                if (sSelectRd.arraySize != materials.Length) { sSelectRd.arraySize = materials.Length; }
                int slotIndex = 0;
                foreach (var MatSlot in materials)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sSelectRd.GetArrayElementAtIndex(slotIndex));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(MatSlot, typeof(Material), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    slotIndex += 1;
                }
                rendererIndex += 1;

                EditorGUILayout.LabelField("---");
            }

            var sIsTextureSeparate = thisSObject.FindProperty("IsTextureSeparate");
            EditorGUILayout.PropertyField(sIsTextureSeparate);

            var sPropertyName = thisSObject.FindProperty("PropertyName");
            EditorGUILayout.PropertyField(sPropertyName);


            EditorGUI.EndDisabledGroup();

            PreviewContext.instance.DrawApplyAndRevert(thisObject);

            thisSObject.ApplyModifiedProperties();
        }



        public static void DrawerSummary(MatAndTexRelativeSeparator target)
        {
            var sObj = new SerializedObject(target);
            var sTargetRenderers = sObj.FindProperty("TargetRenderers");
            TextureTransformerEditor.DrawerTargetRenderersSummary(sTargetRenderers, sTargetRenderers.name.Glc());
            sObj.ApplyModifiedProperties();
        }
    }
}

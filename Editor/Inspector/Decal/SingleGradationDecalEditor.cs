using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using System.Collections.Generic;
using net.rs64.TexTransCore.Utils;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SingleGradationDecal))]
    internal class SingleGradationDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(target.GetType().Name);

            var sTargetMaterials = serializedObject.FindProperty("TargetMaterials");
            foreach (var mat2mat in EnumPair(_materialSelectionCandidates))
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.width *= 0.5f;
                DrawAtMaterial(sTargetMaterials, mat2mat.Item1, rect);
                rect.x += rect.width;
                if (mat2mat.Item2 != null) { DrawAtMaterial(sTargetMaterials, mat2mat.Item2, rect); }
            }

            EditorGUILayout.PropertyField(sTargetMaterials, "SingleGradationDecal:prop:SelectedMaterialView".Glc());

            var tf = new SerializedObject((serializedObject.targetObject as SingleGradationDecal).transform);
            var sLocalScale = tf.FindProperty("m_LocalScale");
            var length = sLocalScale.vector3Value.y;
            var lengthRect = EditorGUILayout.GetControlRect();
            var propGUIContent = EditorGUI.BeginProperty(lengthRect, "SingleGradationDecal:prop:GradationLength".Glc(), sLocalScale);
            length = EditorGUI.FloatField(lengthRect, propGUIContent, length);
            sLocalScale.vector3Value = new Vector3(length, length, length);
            EditorGUI.EndProperty();


            var sGradient = serializedObject.FindProperty("Gradient");
            EditorGUILayout.PropertyField(sGradient, "SingleGradationDecal:prop:Gradient".Glc());

            var sAlpha = serializedObject.FindProperty("Alpha");
            EditorGUILayout.PropertyField(sAlpha, "SingleGradationDecal:prop:Alpha".Glc());

            var sGradientClamp = serializedObject.FindProperty("GradientClamp");
            EditorGUILayout.PropertyField(sGradientClamp, "SingleGradationDecal:prop:GradientClamp".Glc());

            var sIslandSelector = serializedObject.FindProperty("IslandSelector");
            EditorGUILayout.PropertyField(sIslandSelector, "SingleGradationDecal:prop:IslandSelector".Glc());

            var sBlendTypeKey = serializedObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendTypeKey, "SingleGradationDecal:prop:BlendTypeKey".Glc());

            var sTargetPropertyName = serializedObject.FindProperty("TargetPropertyName");
            EditorGUILayout.PropertyField(sTargetPropertyName, "SingleGradationDecal:prop:TargetPropertyName".Glc());


            AbstractDecalEditor.DrawerAdvancedOption(serializedObject);

            TextureTransformerEditor.DrawerRealTimePreviewEditorButton(target as TexTransRuntimeBehavior);
            serializedObject.ApplyModifiedProperties();
            tf.ApplyModifiedProperties();
        }

        private static IEnumerable<(T, T)> EnumPair<T>(IEnumerable<T> values)
        {
            T bv = default;
            bool isBv = false;
            foreach (var v in values)
            {
                if (isBv)
                {
                    yield return (bv, v);
                    isBv = false;
                }
                else
                {
                    bv = v;
                    isBv = true;
                }
            }
            if (isBv) { yield return (bv, default); }
        }

        private void DrawAtMaterial(SerializedProperty sTargetMaterials, Material mat, Rect rect)
        {
            var fullWidth = rect.width;
            var val = FindMaterial(sTargetMaterials, mat);
            rect.width = rect.height;
            var editVal = EditorGUI.Toggle(rect, val);
            if (editVal != val) { EditMaterial(sTargetMaterials, mat, editVal); }
            rect.x += rect.width;
            var prevTex2D = AssetPreview.GetAssetPreview(mat);
            if (prevTex2D != null) { EditorGUI.DrawTextureTransparent(rect, prevTex2D, ScaleMode.ScaleToFit); }
            rect.x += rect.width;
            rect.width = fullWidth - rect.width - rect.width;
            EditorGUI.ObjectField(rect, mat, typeof(Material), false);
        }

        bool FindMaterial(SerializedProperty matList, Material material)
        {
            for (var i = 0; matList.arraySize > i; i += 1)
            {
                if (matList.GetArrayElementAtIndex(i).objectReferenceValue == material) { return true; }
            }
            return false;
        }
        void EditMaterial(SerializedProperty matList, Material material, bool val)
        {
            if (val)
            {
                var newIndex = matList.arraySize;
                matList.arraySize += 1;
                matList.GetArrayElementAtIndex(newIndex).objectReferenceValue = material;
            }
            else
            {
                for (var i = 0; matList.arraySize > i; i += 1)
                { if (matList.GetArrayElementAtIndex(i).objectReferenceValue == material) { matList.DeleteArrayElementAtIndex(i); break; } }
            }
        }


        private void OnEnable() { FinedMaterialSelectionCandidates(); }

        List<Material> _materialSelectionCandidates;
        void FinedMaterialSelectionCandidates()
        {
            var ep = target as SingleGradationDecal;
            var marker = DomainMarkerFinder.FindMarker(ep.gameObject);
            if (marker == null) { return; }
            _materialSelectionCandidates = RendererUtility.GetFilteredMaterials(marker.GetComponentsInChildren<Renderer>(true), _materialSelectionCandidates);
        }
    }
}

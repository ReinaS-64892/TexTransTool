#nullable enable
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using System.Collections.Generic;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using net.rs64.TexTransTool.Utils;
using System.Linq;

namespace net.rs64.TexTransTool.Editor.Decal
{
    [CustomPropertyDrawer(typeof(DecalRendererSelector))]
    public class DecalRendererSelectorEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var preVal = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var indentWith = preVal * 18f;
            position.x += indentWith;
            position.width -= indentWith;


            position.height = 18f;
            var sMode = property.FindPropertyRelative("Mode");
            var sUseMaterialFilteringForAutoSelect = property.FindPropertyRelative("UseMaterialFilteringForAutoSelect");
            var sIsAutoIncludingDisableRenderers = property.FindPropertyRelative("IsAutoIncludingDisableRenderers");
            var sAutoSelectFilterMaterials = property.FindPropertyRelative("AutoSelectFilterMaterials");
            var sManualSelections = property.FindPropertyRelative("ManualSelections");
            if (_materialSelectionCandidates is null) { FinedMaterialSelectionCandidates(property.serializedObject.targetObject as Component); }

            EditorGUI.PropertyField(position, sMode, "Common:RendererSelectMode".Glc());
            position.y += position.height;

            switch ((RendererSelectMode)sMode.enumValueIndex)
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        EditorGUI.PropertyField(position, sUseMaterialFilteringForAutoSelect, "CommonDecal:prop:UseMaterialFiltering".Glc());
                        position.y += position.height;
                        if (sUseMaterialFilteringForAutoSelect.boolValue)
                        {
                            if (_materialSelectionCandidates is not null)
                                foreach (var mat2mat in EnumPair(_materialSelectionCandidates))
                                {
                                    var rect = position;

                                    rect.width *= 0.5f;
                                    DrawAtMaterial(sAutoSelectFilterMaterials, mat2mat.Item1, rect);
                                    rect.x += rect.width;
                                    DrawAtMaterial(sAutoSelectFilterMaterials, mat2mat.Item2, rect);

                                    position.y += position.height;
                                }
                            position.height = EditorGUI.GetPropertyHeight(sAutoSelectFilterMaterials);
                            EditorGUI.PropertyField(position, sAutoSelectFilterMaterials, "CommonDecal:prop:AutoSelectFilterMaterials".Glc());
                            position.y += position.height;
                            position.height = 18f;
                        }
                        EditorGUI.PropertyField(position, sIsAutoIncludingDisableRenderers, "CommonDecal:prop:IncludingDisableRenderers".Glc());
                        position.y += position.height;
                        break;
                    }
                case RendererSelectMode.Manual:
                    {
                        position.height = EditorGUI.GetPropertyHeight(sManualSelections);
                        EditorGUI.PropertyField(position, sManualSelections, "CommonDecal:prop:TargetRenderer".Glc());
                        position.y += position.height;
                        position.height = 18f;
                        break;
                    }
            }

            EditorGUI.indentLevel = preVal;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var h = 18f;
            var sMode = property.FindPropertyRelative("Mode");
            var sUseMaterialFilteringForAutoSelect = property.FindPropertyRelative("UseMaterialFilteringForAutoSelect");
            var sIsAutoIncludingDisableRenderers = property.FindPropertyRelative("IsAutoIncludingDisableRenderers");
            var sAutoSelectFilterMaterials = property.FindPropertyRelative("AutoSelectFilterMaterials");
            var sManualSelections = property.FindPropertyRelative("ManualSelections");
            if (_materialSelectionCandidates is null) { FinedMaterialSelectionCandidates(property.serializedObject.targetObject as Component); }
            switch ((RendererSelectMode)sMode.enumValueIndex)
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        h += 18f;
                        h += 18f;
                        if (sUseMaterialFilteringForAutoSelect.boolValue)
                        {
                            h += 18f * (((_materialSelectionCandidates?.Count ?? 0) + 1) / 2);
                            h += EditorGUI.GetPropertyHeight(sAutoSelectFilterMaterials);
                        }
                        ;
                        break;
                    }
                case RendererSelectMode.Manual:
                    {
                        h += EditorGUI.GetPropertyHeight(sManualSelections);
                        break;
                    }
            }
            return h;
        }

        List<Material>? _materialSelectionCandidates;
        void FinedMaterialSelectionCandidates(Component? findPoint)
        {
            if (findPoint == null) { return; }

            _materialSelectionCandidates = null;
            if (PreviewUtility.IsPreviewContains) { return; }

            var marker = DomainMarkerFinder.FindMarker(findPoint.gameObject);
            if (marker == null) { return; }

            _materialSelectionCandidates = RendererUtility.GetFilteredMaterials(marker.GetComponentsInChildren<Renderer>(true).Where(r => r is SkinnedMeshRenderer or MeshRenderer));
        }

        internal static IEnumerable<(T?, T?)> EnumPair<T>(IEnumerable<T> values)
        {
            T? bv = default;
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

        internal static bool DrawAtMaterial(SerializedProperty sTargetMaterials, Material? mat, Rect rect)
        {
            if (mat == null) { return false; }
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
            return editVal;
        }

        static bool FindMaterial(SerializedProperty matList, Material material)
        {
            for (var i = 0; matList.arraySize > i; i += 1)
            {
                if (matList.GetArrayElementAtIndex(i).objectReferenceValue == material) { return true; }
            }
            return false;
        }
        static void EditMaterial(SerializedProperty matList, Material material, bool val)
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



    }
}

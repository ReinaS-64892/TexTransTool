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
                            position.height = EditorGUI.GetPropertyHeight(sAutoSelectFilterMaterials);
                            EditorGUI.PropertyField(position, sAutoSelectFilterMaterials, "CommonDecal:prop:AutoSelectFilterMaterials".Glc());
                            position.y += position.height;

                            if (sAutoSelectFilterMaterials.isExpanded is false && _materialSelectionCandidates is not null)
                            {
                                position.height = TargetObjectSelector.GetRequireHeightSlim(_materialSelectionCandidates.Count, position.width);
                                TargetObjectSelector.DrawTargetSelectionSlim(position, sAutoSelectFilterMaterials, _materialSelectionCandidates);
                                position.y += position.height;
                            }
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
                            h += EditorGUI.GetPropertyHeight(sAutoSelectFilterMaterials);

                            if (sAutoSelectFilterMaterials.isExpanded is false && _materialSelectionCandidates is not null)
                            {
                                var viewWidth = EditorGUIUtility.currentViewWidth - 18f;
                                h += TargetObjectSelector.GetRequireHeightSlim(_materialSelectionCandidates.Count, viewWidth);
                            }
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
    }
}

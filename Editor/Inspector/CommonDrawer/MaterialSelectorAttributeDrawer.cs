using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(MaterialSelectorAttribute))]
    internal class MaterialSelectorAttributeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, AdvancedDropdownState> _dropdownStates = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var selectorAttribute = (MaterialSelectorAttribute)attribute;
            
            var labelRect = EditorGUI.PrefixLabel(position, label);
            var objectFieldRect = labelRect;
            var dropdownRect = labelRect;
            
            dropdownRect.width = 18f;
            bool buttonOnLeft = selectorAttribute.Button == MaterialSelectorAttribute.Side.Left;
            if (buttonOnLeft)
            {
                dropdownRect.x = labelRect.xMin;
                objectFieldRect.x = dropdownRect.xMax + 2f;
                objectFieldRect.width = labelRect.xMax - objectFieldRect.x;
            }
            else
            {
                dropdownRect.x = labelRect.xMax - dropdownRect.width;
                objectFieldRect.width = labelRect.width - dropdownRect.width - 2f;
            }

            EditorGUI.BeginProperty(objectFieldRect, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(objectFieldRect, property.objectReferenceValue, typeof(Material), true);
            EditorGUI.EndProperty();

            var component = property.serializedObject.targetObject as Component;
            if (component != null)
            {
                var stateKey = property.propertyPath;
                if (!_dropdownStates.ContainsKey(stateKey))
                {
                    _dropdownStates[stateKey] = new AdvancedDropdownState();
                }

                if (EditorGUI.DropdownButton(dropdownRect, GUIContent.none, FocusType.Keyboard))
                {
                    var dropdown = new DomainMaterialAdvancedDropdown(component, m => OnMaterialSelected(property, m), _dropdownStates[stateKey]);
                    var showRect = dropdownRect;
                    if (selectorAttribute.Popup == MaterialSelectorAttribute.Side.Left)
                    {
                        var offset = Mathf.Max(0f, dropdown.PopupSize.x - dropdownRect.width);
                        showRect.x -= offset;
                    }
                    dropdown.Show(showRect);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        private static void OnMaterialSelected(SerializedProperty property, Material material)
        {
            property.serializedObject.Update();
            property.objectReferenceValue = material;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    internal class DomainMaterialAdvancedDropdown : MaterialAdvancedDropdown
    {
        public DomainMaterialAdvancedDropdown(Component component, Action<Material> onSelected, AdvancedDropdownState state)
            : base(CollectDomainMaterials(component), onSelected, state)
        {
        }

        private static List<Material> CollectDomainMaterials(Component component)
        {
            var domainObject = DomainMarkerFinder.FindMarker(component.gameObject);
            if (domainObject == null) return new List<Material>();

            return domainObject.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct().ToList();
        }
    }

    internal class MaterialAdvancedDropdown : AdvancedDropdown
    {
        private readonly List<Material> _materials;
        private readonly Action<Material> _onSelected;
        public Vector2 PopupSize { get; private set; }

        const float minWidth = 260f;
        const float minHeight = 280f;

        public MaterialAdvancedDropdown(List<Material> materials, Action<Material> onSelected, AdvancedDropdownState state) : base(state)
        {
            _materials = materials;
            _onSelected = onSelected;
            PopupSize = new Vector2(minWidth, minHeight);
            minimumSize = PopupSize;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Material");
            int itemId = 0;

            foreach (var mat in _materials)
            {
                var materialItem = new MaterialAdvancedDropdownItem(mat, mat.name)
                {
                    id = itemId++,
                    icon = AssetPreview.GetAssetPreview(mat)
                };
                root.AddChild(materialItem);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is MaterialAdvancedDropdownItem materialItem)
            {
                _onSelected(materialItem.Material);
            }
        }

        class MaterialAdvancedDropdownItem : AdvancedDropdownItem
        {
            public Material Material { get; }

            public MaterialAdvancedDropdownItem(Material material, string name) : base(name)
            {
                Material = material;
            }
        }
    }
}
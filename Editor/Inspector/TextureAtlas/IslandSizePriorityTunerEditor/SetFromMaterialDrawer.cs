using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner;
using net.rs64.TexTransTool.Editor.Decal;
using System.Collections.Generic;
using net.rs64.TexTransTool.Editor;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(SetFromMaterial))]
    internal class SetFromMaterialDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            var sPriorityValue = property.FindPropertyRelative(nameof(SetFromMaterial.PriorityValue));
            var sMaterials = property.FindPropertyRelative(nameof(SetFromMaterial.Materials));

            EditorGUI.PropertyField(position, sPriorityValue, "IslandSizePriorityTuner:prop:PriorityValue".GlcV());
            position.y += 18;

            position.height = EditorGUI.GetPropertyHeight(sMaterials);
            EditorGUI.PropertyField(position, sMaterials, "IslandSizePriorityTuner:prop:Materials".GlcV());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(SetFromMaterial.Materials)));
        }


        public static void DrawNow(SerializedProperty sfm, IEnumerable<Material> materials)
        {
            var sPriorityValue = sfm.FindPropertyRelative(nameof(SetFromMaterial.PriorityValue));
            var sMaterials = sfm.FindPropertyRelative(nameof(SetFromMaterial.Materials));

            EditorGUILayout.PropertyField(sPriorityValue, "IslandSizePriorityTuner:prop:PriorityValue".GlcV());

            TargetObjectSelector.DrawTargetSelectionSlimLayout(sMaterials, materials);
        }

    }
}

#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
using System.Linq;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(PhotoshopGradationMapLayer), true)]
    [CanEditMultipleObjects]
    internal class PhotoshopGradationMapLayerEditor : AbstractLayerEditor
    {
        private static readonly string[] _gradientInteropMethodKeys = new string[] {
            "PhotoshopGradationMapLayer:GradientInteropMethod:Classic",
            "PhotoshopGradationMapLayer:GradientInteropMethod:Perceptual",
            "PhotoshopGradationMapLayer:GradientInteropMethod:Linear",
            "PhotoshopGradationMapLayer:GradientInteropMethod:Smooth",
            "PhotoshopGradationMapLayer:GradientInteropMethod:Stripes",
        };

        protected override void DrawInnerProperties()
        {
            var isGradientReversed = serializedObject.FindProperty(nameof(PhotoshopGradationMapLayer.IsGradientReversed));
            EditorGUILayout.PropertyField(isGradientReversed, "PhotoshopGradationMapLayer:prop:IsGradientReversed".Glc());
            var isGradientDithered = serializedObject.FindProperty(nameof(PhotoshopGradationMapLayer.IsGradientDithered));
            EditorGUILayout.PropertyField(isGradientDithered, "PhotoshopGradationMapLayer:prop:IsGradientDithered".Glc());

            var interopMethod = serializedObject.FindProperty(nameof(PhotoshopGradationMapLayer.InteropMethod));
            var interopMethodContents = _gradientInteropMethodKeys.Select(i => i.Glc()).ToArray();
            interopMethod.enumValueIndex = EditorGUILayout.Popup("PhotoshopGradationMapLayer:prop:InteropMethod".Glc(), interopMethod.enumValueIndex, interopMethodContents);

            var smoothens = serializedObject.FindProperty(nameof(PhotoshopGradationMapLayer.Smoothens));
            EditorGUILayout.PropertyField(smoothens, "PhotoshopGradationMapLayer:prop:Smoothens".Glc());
            var colorKeys = serializedObject.FindProperty(nameof(PhotoshopGradationMapLayer.ColorKeys));
            EditorGUILayout.PropertyField(colorKeys, "PhotoshopGradationMapLayer:prop:ColorKeys".Glc());
            var transparencyKeys = serializedObject.FindProperty(nameof(PhotoshopGradationMapLayer.TransparencyKeys));
            EditorGUILayout.PropertyField(transparencyKeys, "PhotoshopGradationMapLayer:prop:TransparencyKeys".Glc());
        }
    }

    [CustomPropertyDrawer(typeof(PhotoshopGradationMapLayer.ColorKey))]
    internal class ColorKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var keyLocation = property.FindPropertyRelative(nameof(PhotoshopGradationMapLayer.ColorKey.KeyLocation));
            var midLocation = property.FindPropertyRelative(nameof(PhotoshopGradationMapLayer.ColorKey.MidLocation));
            var color = property.FindPropertyRelative(nameof(PhotoshopGradationMapLayer.ColorKey.Color));
            
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, keyLocation, "PhotoshopGradationMapLayer:prop:KeyLocation".Glc());
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, midLocation, "PhotoshopGradationMapLayer:prop:MidLocation".Glc());
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, color, "PhotoshopGradationMapLayer:prop:Color".Glc());
            position.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }

    [CustomPropertyDrawer(typeof(PhotoshopGradationMapLayer.TransparencyKey))]
    internal class TransparencyKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var keyLocation = property.FindPropertyRelative(nameof(PhotoshopGradationMapLayer.TransparencyKey.KeyLocation));
            var midLocation = property.FindPropertyRelative(nameof(PhotoshopGradationMapLayer.TransparencyKey.MidLocation));
            var transparency = property.FindPropertyRelative(nameof(PhotoshopGradationMapLayer.TransparencyKey.Transparency));

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, keyLocation, "PhotoshopGradationMapLayer:prop:KeyLocation".Glc());
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, midLocation, "PhotoshopGradationMapLayer:prop:MidLocation".Glc());
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, transparency, "PhotoshopGradationMapLayer:prop:Transparency".Glc());
            position.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }
}
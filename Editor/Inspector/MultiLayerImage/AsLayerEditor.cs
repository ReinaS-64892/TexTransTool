#nullable enable
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(AsLayer), true)]
    [CanEditMultipleObjects]
    internal class AsLayerEditor : TexTransMonoBaseEditor
    {
        ICanBehaveAsLayer? _layer;

        void OnEnable()
        {
            _layer = (target as Component)?.GetComponent<ICanBehaveAsLayer>();
        }
        protected override void OnTexTransComponentInspectorGUI()
        {
            var sObj = serializedObject;
            var drawBlKey = (_layer?.HaveBlendTypeKey ?? false) is false;
            var sOpacity = sObj.FindProperty(nameof(AsLayer.Opacity));
            var sClipping = sObj.FindProperty(nameof(AsLayer.Clipping));
            var sBlendTypeKey = sObj.FindProperty(nameof(AsLayer.BlendTypeKey));
            var sLayerMask = sObj.FindProperty(nameof(AsLayer.LayerMask));

            EditorGUILayout.PropertyField(sOpacity);
            EditorGUILayout.PropertyField(sClipping);
            if (drawBlKey) EditorGUILayout.PropertyField(sBlendTypeKey);
            EditorGUILayout.PropertyField(sLayerMask);

        }
    }
}

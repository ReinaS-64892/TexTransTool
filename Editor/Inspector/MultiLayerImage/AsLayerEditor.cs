#nullable enable
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(AsLayer), true)]
    [CanEditMultipleObjects]
    internal class AsLayerEditor : UnityEditor.Editor
    {
        ICanBehaveAsLayer? _layer;

        void OnEnable()
        {
            _layer = (target as Component)?.GetComponent<ICanBehaveAsLayer>();
        }
        public override void OnInspectorGUI()
        {
            var sObj = serializedObject;
            var drawBlKey = (_layer?.HaveBlendTypeKey ?? false) is false;
            var sOpacity = sObj.FindProperty("Opacity");
            var sClipping = sObj.FindProperty("Clipping");
            var sBlendTypeKey = sObj.FindProperty("BlendTypeKey");
            var sLayerMask = sObj.FindProperty("LayerMask");

            TextureTransformerEditor.DrawerWarning(nameof(AsLayer).GetLocalize());

            EditorGUILayout.PropertyField(sOpacity);
            EditorGUILayout.PropertyField(sClipping);
            if (drawBlKey) EditorGUILayout.PropertyField(sBlendTypeKey);
            EditorGUILayout.PropertyField(sLayerMask);

            sObj.ApplyModifiedProperties();
        }
    }
}

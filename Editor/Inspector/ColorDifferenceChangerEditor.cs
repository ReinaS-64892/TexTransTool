using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(ColorDifferenceChanger))]
    internal class ColorDifferenceChangerEditor : UnityEditor.Editor
    {
        CanBehaveAsLayerEditorUtil behaveLayerUtil;
        void BehaveUtilInit() { behaveLayerUtil = new(target as Component); }
        void OnEnable() { BehaveUtilInit(); EditorApplication.hierarchyChanged += BehaveUtilInit; }
        void OnDisable() { EditorApplication.hierarchyChanged -= BehaveUtilInit; }

        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(nameof(ColorDifferenceChanger));
            var thisSObject = serializedObject;

            if (behaveLayerUtil.ThisIsLayer is false) EditorGUILayout.PropertyField(thisSObject.FindProperty(nameof(ColorDifferenceChanger.TargetTexture)));

            EditorGUILayout.PropertyField(thisSObject.FindProperty(nameof(ColorDifferenceChanger.DifferenceSourceColor)));
            EditorGUILayout.PropertyField(thisSObject.FindProperty(nameof(ColorDifferenceChanger.TargetColor)));

            thisSObject.ApplyModifiedProperties();
            if (behaveLayerUtil.IsDrawPreviewButton) PreviewButtonDrawUtil.Draw(target as TexTransMonoBase);
            behaveLayerUtil.DrawAddLayerButton(target as Component);
        }
    }
}

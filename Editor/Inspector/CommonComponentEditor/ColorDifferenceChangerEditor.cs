using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(ColorDifferenceChanger))]
    internal class ColorDifferenceChangerEditor : TTCanBehaveAsLayerEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var thisSObject = serializedObject;
            if (IsLayerMode is false) EditorGUILayout.PropertyField(thisSObject.FindProperty(nameof(ColorDifferenceChanger.TargetTexture)));

            EditorGUILayout.PropertyField(thisSObject.FindProperty(nameof(ColorDifferenceChanger.DifferenceSourceColor)));
            EditorGUILayout.PropertyField(thisSObject.FindProperty(nameof(ColorDifferenceChanger.TargetColor)));
            PreviewButtonDrawUtil.DrawUnlitButton();
        }
    }
}

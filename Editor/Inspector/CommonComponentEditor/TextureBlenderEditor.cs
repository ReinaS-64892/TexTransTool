using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureBlender))]
    internal class TextureBlenderEditor : TTCanBehaveAsLayerEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var thisSObject = serializedObject;

            if (IsLayerMode is false)
            {
                var targetTexture = thisSObject.FindProperty(nameof(TextureBlender.TargetTexture));
                EditorGUILayout.PropertyField(targetTexture, "TextureBlender:prop:TargetTexture".Glc());
            }

            var sBlendTexture = thisSObject.FindProperty(nameof(TextureBlender.BlendTexture));
            EditorGUILayout.PropertyField(sBlendTexture, "TextureBlender:prop:BlendTexture".Glc());

            var sColor = thisSObject.FindProperty(nameof(TextureBlender.Color));
            EditorGUILayout.PropertyField(sColor, "TextureBlender:prop:Color".Glc());

            var sBlendTypeKey = thisSObject.FindProperty(nameof(TextureBlender.BlendTypeKey));
            EditorGUILayout.PropertyField(sBlendTypeKey, "TextureBlender:prop:BlendTypeKey".Glc());
        }
    }
}

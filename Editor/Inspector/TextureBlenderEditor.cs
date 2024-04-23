using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Preview;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureBlender))]
    internal class TextureBlenderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("TextureBlender");

            var thisTarget = target as TextureBlender;
            var thisSObject = serializedObject;

            EditorGUI.BeginDisabledGroup(OneTimePreviewContext.IsPreviewing(thisTarget));

            EditorGUILayout.PropertyField(thisSObject.FindProperty("TargetTexture"));

            var sBlendTexture = thisSObject.FindProperty("BlendTexture");
            EditorGUILayout.PropertyField(sBlendTexture, sBlendTexture.name.Glc());

            var sColor = thisSObject.FindProperty("Color");
            EditorGUILayout.PropertyField(sColor, sColor.name.Glc());

            var sBlendTypeKey = thisSObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendTypeKey, sBlendTypeKey.name.Glc());

            EditorGUI.EndDisabledGroup();


            OneTimePreviewContext.instance.DrawApplyAndRevert(thisTarget);
            thisSObject.ApplyModifiedProperties();
        }


        public static void DrawerSummary(TextureBlender target)
        {
            var sObj = new SerializedObject(target);
            var sTargetRenderer = sObj.FindProperty("TargetTexture").FindPropertyRelative("RendererAsPath");
            EditorGUILayout.PropertyField(sTargetRenderer);
            var sBlendTexture = sObj.FindProperty("BlendTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(sBlendTexture);

            sObj.ApplyModifiedProperties();
        }
    }
}

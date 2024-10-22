using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransCoreEngineForUnity;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(MultiLayerImageCanvas))]
    internal class MultiLayerImageCanvasEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MultiLayerImageCanvas".GetLocalize());

            var sTarget = serializedObject;

            EditorGUILayout.PropertyField(sTarget.FindProperty("TextureSelector"));

            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);


            sTarget.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI() { return false; }

        public override void DrawPreview(Rect previewArea)
        {
            var mlic = target as MultiLayerImageCanvas;
            DrawPreviewMLIC(previewArea, mlic);
        }

        public static void DrawPreviewMLIC(Rect previewArea, MultiLayerImageCanvas mlic)
        {
            var texManager = new TextureManager(true);
            var texTransUnityCoreEngine = new TTCE4UnityWithTTT4Unity(true, texManager);
            var canvasResult = mlic.EvaluateCanvas(texTransUnityCoreEngine, 1024, 1024).Unwrap();
            texManager.DestroyDeferred();

            EditorGUI.DrawTextureTransparent(previewArea, canvasResult, ScaleMode.ScaleToFit);
            TTRt.R(canvasResult);
        }


    }
}

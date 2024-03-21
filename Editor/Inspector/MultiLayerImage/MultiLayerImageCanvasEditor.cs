using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
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

            var thisTarget = target as MultiLayerImageCanvas;
            PreviewContext.instance.DrawApplyAndRevert(thisTarget);

            sTarget.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI() { return true; }

        public override void DrawPreview(Rect previewArea)
        {
            var mlic = target as MultiLayerImageCanvas;
            DrawPreviewMLIC(previewArea, mlic);
        }

        public static void DrawPreviewMLIC(Rect previewArea, MultiLayerImageCanvas mlic)
        {
            var texManager = new TextureManager(true);
            var canvasResult = mlic.EvaluateCanvas(texManager, 1024);
            texManager.DestroyTextures();

            EditorGUI.DrawTextureTransparent(previewArea, canvasResult, ScaleMode.ScaleToFit);
            RenderTexture.ReleaseTemporary(canvasResult);
        }


    }
}

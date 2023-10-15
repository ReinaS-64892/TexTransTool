using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(MultiLayerImageCanvas))]
    public class MultiLayerImageCanvasEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var Target = target as MultiLayerImageCanvas;
            PreviewContext.instance.DrawApplyAndRevert(Target);
        }
    }
}
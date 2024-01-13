using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(AbstractLayer), true)]
    [CanEditMultipleObjects]
    internal class AbstractLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MultiImageLayer".GetLocalize());
            base.OnInspectorGUI();
        }


    }
}

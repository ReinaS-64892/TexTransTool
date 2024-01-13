using UnityEditor;
using net.rs64.TexTransTool.Decal.Curve;
namespace net.rs64.TexTransTool.Editor.Decal.Curve
{

    [CustomEditor(typeof(CurveSegment))]
    internal class CurveSegmentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("CurveSegment");
            base.OnInspectorGUI();

        }
    }
}

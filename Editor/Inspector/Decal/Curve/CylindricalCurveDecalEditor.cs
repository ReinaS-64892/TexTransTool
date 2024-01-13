using UnityEditor;
using net.rs64.TexTransTool.Decal.Curve;

namespace net.rs64.TexTransTool.Editor.Decal.Curve.Cylindrical
{


    [CustomEditor(typeof(CylindricalCurveDecal), true)]
    internal class CylindricalCurveDecalEditor : CurveDecalEditor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("CylindricalCurveDecal");
            base.OnInspectorGUI();
            PreviewContext.instance.DrawApplyAndRevert(target as CylindricalCurveDecal);
        }

    }

}

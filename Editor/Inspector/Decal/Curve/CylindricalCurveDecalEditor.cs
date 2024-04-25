using UnityEditor;
using net.rs64.TexTransTool.Decal.Curve;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor.Decal.Curve.Cylindrical
{


    [CustomEditor(typeof(CylindricalCurveDecal), true)]
    internal class CylindricalCurveDecalEditor : CurveDecalEditor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("CylindricalCurveDecal");
            base.OnInspectorGUI();
            OneTimePreviewContext.instance.DrawApplyAndRevert(target as CylindricalCurveDecal);
        }

    }

}

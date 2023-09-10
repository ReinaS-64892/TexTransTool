#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal.Curve;

namespace net.rs64.TexTransTool.Editor.Decal.Curve.Cylindrical
{


    [CustomEditor(typeof(CylindricalCurveDecal), true)]
    public class CylindricalCurveDecalEditor : CurveDecalEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            PreviewContext.instance.DrawApplyAndRevert(target as CylindricalCurveDecal);
        }

    }

}
#endif

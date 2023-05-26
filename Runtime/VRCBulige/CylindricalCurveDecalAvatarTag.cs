#if UNITY_EDITOR
using UnityEngine;
using VRC.SDKBase;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Decal.Curve.Cylindrical;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/CylindricalCurveDecal")]

    public class CylindricalCurveDecalAvatarTag : CylindricalCurveDecal, IEditorOnly
    {
    }
}
#endif
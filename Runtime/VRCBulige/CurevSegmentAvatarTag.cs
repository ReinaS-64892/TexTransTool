#if UNITY_EDITOR
using UnityEngine;
using Rs64.TexTransTool.TexturAtlas;
using VRC.SDKBase;
using Rs64.TexTransTool.Decal.Curve;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/Experimental/CurevSegment")]
    public class CurevSegmentAvatarTag : CurevSegment, IEditorOnly
    {

    }
}
#endif
#if UNITY_EDITOR
using UnityEngine;
using Rs64.TexTransTool.TexturAtlas;
using VRC.SDKBase;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/AvatarTag/AtlasSet")]
    public class AtlasSetAvatarTag : AtlasSet, IEditorOnly
    {

    }
}
#endif
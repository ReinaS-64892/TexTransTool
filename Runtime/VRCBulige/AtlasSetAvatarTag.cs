#if UNITY_EDITOR
using UnityEngine;
using Rs64.TexTransTool.TexturAtlas;
using VRC.SDKBase;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/AtlasSet")]
    public class AtlasSetAvatarTag : AtlasSet, IEditorOnly
    {

    }
}
#endif
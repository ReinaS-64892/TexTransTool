#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using VRC.SDKBase;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/TexTransGroup")]
    public class TexTransGroupAvatarTag : TexTransGroup, IEditorOnly
    {

    }
}
#endif
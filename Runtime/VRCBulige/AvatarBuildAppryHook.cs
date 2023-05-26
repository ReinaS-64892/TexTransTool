#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using VRC.SDKBase;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/AvatarBuildAppryHook"), RequireComponent(typeof(TexTransGroupAvatarTag))]
    public class AvatarBuildAppryHook : MonoBehaviour, IEditorOnly
    {
        public TexTransGroupAvatarTag TTGAvatarTag;
        private void Reset()
        {
            TTGAvatarTag = GetComponent<TexTransGroupAvatarTag>();
        }
    }
}
#endif
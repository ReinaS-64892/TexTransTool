#if (UNITY_EDITOR && VRC_BASE)
using UnityEngine;
using System.Collections.Generic;
using VRC.SDKBase;

namespace Rs64.TexTransTool.VRCBulige
{
    [AddComponentMenu("TexTransTool/AvatarBuildAppryHook"), RequireComponent(typeof(TexTransGroup))]
    public class AvatarBuildAppryHook : MonoBehaviour, IEditorOnly
    {
        public TexTransGroup TexTransGroup;
        private void Reset()
        {
            TexTransGroup = GetComponent<TexTransGroup>();
        }
    }
}
#endif
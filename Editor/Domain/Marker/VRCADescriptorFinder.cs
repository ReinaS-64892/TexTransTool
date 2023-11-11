#if UNITY_EDITOR && VRC_BASE
using UnityEngine;
using VRC.SDKBase;

namespace net.rs64.TexTransTool
{
    public class VRCADescriptorFinder : IMarkerFinder
    {
        public GameObject FindMarker(GameObject StartPoint)
        {
            return StartPoint.GetComponentInParent<VRC_AvatarDescriptor>()?.gameObject;
        }
    }
}
#endif
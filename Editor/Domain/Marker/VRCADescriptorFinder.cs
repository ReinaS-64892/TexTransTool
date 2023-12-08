#if UNITY_EDITOR && VRC_BASE
using UnityEngine;
using VRC.SDKBase;

namespace net.rs64.TexTransTool
{
    internal class VRCADescriptorFinder : IDomainMarkerFinder
    {
        public GameObject FindMarker(GameObject StartPoint)
        {
            return StartPoint.GetComponentInParent<VRC_AvatarDescriptor>()?.gameObject;
        }
    }
}
#endif
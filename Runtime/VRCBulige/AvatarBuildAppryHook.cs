#if (UNITY_EDITOR && VRC_BASE)
using UnityEngine;
using System.Collections.Generic;
using VRC.SDKBase;
using System.Linq;

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

        public void Appry(GameObject Avatar)
        {
            if (TexTransGroup == null) Reset();
            TexTransGroup.Appry(GetDomain(Avatar));
        }
        public virtual MaterialDomain GetDomain(GameObject Avatar)
        {
            return new MaterialDomain(Avatar.GetComponentsInChildren<Renderer>(true).ToList());
        }
    }
}
#endif
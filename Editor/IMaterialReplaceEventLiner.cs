#if UNITY_EDITOR
using UnityEngine;
namespace net.rs64.TexTransTool
{
    internal interface IMaterialReplaceEventLiner
    {
        void MaterialReplace(Material Souse, Material Target);
    }
}
#endif
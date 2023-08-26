#if UNITY_EDITOR

using UnityEngine;

namespace net.rs64.TexTransTool
{
    public interface ITexTransToolTag
    {
        int SaveDataVersion { get; }
    }
}
#endif
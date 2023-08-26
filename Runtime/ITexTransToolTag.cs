#if UNITY_EDITOR

using UnityEngine;

namespace Rs64.TexTransTool
{
    public interface ITexTransToolTag
    {
        int SaveDataVersion { get; }
    }
}
#endif
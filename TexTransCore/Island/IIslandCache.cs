using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;

namespace net.rs64.TexTransCore.Island
{
    internal interface IIslandCache
    {
        bool TryCache(List<Vector2> UV, List<TriangleIndex> Triangle, out List<Island> island);
        void AddCache(List<Vector2> UV, List<TriangleIndex> Triangle, List<Island> island);
    }
}
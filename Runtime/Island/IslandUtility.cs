#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;
using Unity.Collections;
using net.rs64.TexTransCore;
using Color = UnityEngine.Color;
using net.rs64.TexTransTool.Decal;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore.UVIsland;

namespace net.rs64.TexTransTool.UVIsland
{
    internal static class UnityIslandUtility
    {
        public static List<Island> UVtoIsland(MeshData meshData, int subMeshIndex)
        {
            var triangle = meshData.TriangleIndex[subMeshIndex].AsSpan();
            var uvVertex = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(meshData.VertexUV.AsSpan());
            return IslandUtility.UVtoIsland(triangle, uvVertex);
        }
    }
}

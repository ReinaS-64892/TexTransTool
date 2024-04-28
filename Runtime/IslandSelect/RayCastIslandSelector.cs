using UnityEngine;
using net.rs64.TexTransTool;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using Unity.Collections;
using System.Collections;
using System;
using Unity.Jobs;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine.Profiling;
using net.rs64.TexTransTool.Utils;
using System.Linq;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RayCastIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT RayCastIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        public float IslandSelectorRange = 0.1f;
        internal override IEnumerable<UnityEngine.Object> GetDependency() { return transform.GetParents().Append(transform); }
        internal override int GetDependencyHash() { return 0; }
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            return RayCastIslandSelect(GetIslandSelectorRay(), islands, islandDescription);
        }

        internal static BitArray RayCastIslandSelect(IslandSelectorRay islandSelectorRay, Island[] islands, IslandDescription[] islandDescription)
        {
            var bitArray = new BitArray(islands.Length);
            var ray = islandSelectorRay;
            var rayMatrix = ray.GetRayMatrix();
            var jobs = new JobHandle[islands.Length];
            var hitResults = new NativeArray<bool>[islands.Length];
            var distances = new NativeArray<float>[islands.Length];

            for (var i = 0; jobs.Length > i; i += 1)
            {
                var triCount = islands[i].triangles.Count;
                var nativeTriangleIndex = new NativeArray<TriangleIndex>(triCount, Allocator.TempJob);
                for (var triIndex = 0; triCount > triIndex; triIndex += 1) { nativeTriangleIndex[triIndex] = islands[i].triangles[triIndex]; }
                var hitResult = hitResults[i] = new NativeArray<bool>(triCount, Allocator.TempJob);
                var distance = distances[i] = new NativeArray<float>(triCount, Allocator.TempJob);

                var rayCastJob = new RayCastJob2()
                {
                    rayMatrix = rayMatrix,
                    Triangles = nativeTriangleIndex,
                    Position = islandDescription[i].Position,
                    HitResult = hitResult,
                    Distance = distance,
                };
                jobs[i] = rayCastJob.Schedule(triCount, 64);
            }

            for (var i = 0; jobs.Length > i; i += 1)
            {
                jobs[i].Complete();

                using (var hRes = hitResults[i])
                using (var distance = distances[i])
                {
                    for (var ti = 0; hRes.Length > ti; ti += 1)
                    {
                        if (!hRes[ti]) { continue; }
                        if (distance[ti] < 0) { continue; }
                        if (distance[ti] > 1) { continue; }

                        bitArray[i] = true;
                        break;
                    }
                }
            }


            return bitArray;
        }

        internal IslandSelectorRay GetIslandSelectorRay() { return new IslandSelectorRay(new Ray(transform.position, transform.forward), transform.lossyScale.z * IslandSelectorRange); }



        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, IslandSelectorRange));
        }
    }

    internal class RayCastIslandSelectorClass : IIslandSelector
    {
        public IslandSelectorRay IslandSelectorRay;

        public RayCastIslandSelectorClass(IslandSelectorRay islandSelectorRay)
        {
            IslandSelectorRay = islandSelectorRay;
        }

        BitArray IIslandSelector.IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            return RayCastIslandSelector.RayCastIslandSelect(IslandSelectorRay, islands, islandDescription);
        }
    }
}

#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.UVIsland;
using Vector2Sys = System.Numerics.Vector2;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasIslandContext
    {
        public Dictionary<AtlasSubMeshIndexID, List<Island>> OriginIslandDict;
        public Dictionary<Island, AtlasSubMeshIndexID> ReverseOriginDict;

        public Dictionary<Island, IslandTransform> Origin2VirtualIsland;

        /*
        Virtual Island とは？

        一つの Island が複数 SubMesh に渡る Island を同一として参照する必要があるからそれを実現するため
        同一 SubMesh 内でも同一として参照したい機会も当然あるため、

        O1 ---> V1
        O2 -/
        O3 /

        みたいな状態を作る必要がある。

        */


        public AtlasIslandContext(HashSet<AtlasSubMeshIndexID> atlasSubMeshIndexIDHash, Func<int, MeshData> getMeshDataFromMeshID)
        {
            // Profiler.BeginSample("UVtoIsland");
            OriginIslandDict = new();
            ReverseOriginDict = new();
            foreach (var atSub in atlasSubMeshIndexIDHash)
            {
                var md = getMeshDataFromMeshID(atSub.MeshID);

                // var triangle = normalizedMesh[Meshes[atSub.MeshID]].GetSubTriangleIndex(atSub.SubMeshIndex);

                // なぜかこっちだと一部のモデルで正しくUVtoIslandができない...なぜ？
                // 今だったら治ってる説ある
                var triangle = md.TriangleIndex[atSub.SubMeshIndex].AsSpan();

                var island = UnityIslandUtility.UVtoIsland(triangle, md.VertexUV.AsSpan());
                OriginIslandDict[atSub] = island;
                foreach (var i in island) { ReverseOriginDict[i] = atSub; }
            }
            // Profiler.EndSample();

            Origin2VirtualIsland = new();
            foreach (var i in OriginIslandDict.SelectMany(kv => kv.Value)) { Origin2VirtualIsland[i] = i.Transform.Clone(); }
            // Profiler.BeginSample("Cross SubMesh Island Merge");
            //Cross SubMesh Island Merge
            SomeVertexCrossSubMeshUsedIslandMerge(atlasSubMeshIndexIDHash, getMeshDataFromMeshID, OriginIslandDict, Origin2VirtualIsland);
            // うまく動いてないから一旦退避
            // OverCrossIslandMerge(atlasSubMeshIndexIDHash, OriginIslandDict, Origin2VirtualIsland);

            // Profiler.EndSample();

            // Profiler.BeginSample("GetIslandSubData");
            // var atSubLinkList = new LinkedList<AtlasSubMeshIndexID>();
            // var IslandLinkList = new LinkedList<Island>();

            // foreach (var atKv in OriginIslandDict)
            // {
            //     var atlasSubData = atKv.Key;
            //     var islands = atKv.Value;

            //     var count = islands.Count;
            //     for (var ii = 0; count > ii; ii += 1)
            //     {
            //         atSubLinkList.AddLast(atlasSubData);
            //         IslandLinkList.AddLast(islands[ii]);
            //     }
            // }

            // Islands = IslandLinkList.ToArray();
            // IslandSubData = atSubLinkList.ToArray();
            // Profiler.EndSample();
        }

        private static IslandTransform MergeIslandTransform(IslandTransform islandTransform, IslandTransform islandTransform2)
        {
            var min = Vector2Sys.Min(islandTransform.Position, islandTransform2.Position);
            var max = Vector2Sys.Max(islandTransform.GetNotRotatedMaxPos(), islandTransform2.GetNotRotatedMaxPos());
            var newVirtualIsland = new IslandTransform();
            newVirtualIsland.Position = min;
            newVirtualIsland.Size = max - min;
            return newVirtualIsland;
        }
        static void SomeVertexCrossSubMeshUsedIslandMerge(
            HashSet<AtlasSubMeshIndexID> atlasSubMeshIndexIDHash
            , Func<int,MeshData> getMeshDataFromMeshID
            , Dictionary<AtlasSubMeshIndexID, List<Island>> originIslandDict
            , Dictionary<Island, IslandTransform> origin2VirtualIsland
            )
        {
            var uvLockUp = new Dictionary<int, (Dictionary<Vector2, int> uvToIndex, int uvIndexCount)>();
            foreach (var atSubGroup in atlasSubMeshIndexIDHash.GroupBy(i => (i.MeshID, i.MaterialGroupID)))
            {
                var atSubCrossTarget = atSubGroup.ToArray();
                Array.Sort(atSubCrossTarget, (l, r) => l.SubMeshIndex - r.SubMeshIndex);

                var meshData = getMeshDataFromMeshID(atSubGroup.Key.MeshID);
                var uv = meshData.VertexUV;

                Dictionary<Vector2, int> uvToIndex;
                int uvIndexCount;
                if (uvLockUp.TryGetValue(atSubGroup.Key.MeshID, out var uvToTuple))
                {
                    uvToIndex = uvToTuple.uvToIndex;
                    uvIndexCount = uvToTuple.uvIndexCount;
                }
                else
                {
                    uvToIndex = new();
                    var uvIndex = 0;
                    foreach (var uvVert in uv)
                    {
                        if (!uvToIndex.ContainsKey(uvVert))
                        {
                            uvToIndex[uvVert] = uvIndex;
                            uvIndex += 1;
                        }
                    }
                    uvIndexCount = uvIndex + 1;
                    uvLockUp[atSubGroup.Key.MeshID] = (uvToIndex, uvIndexCount);
                }


                var usedUVVertIndexUsed = new BitArray[atSubCrossTarget.Length];// BitArray はそれ自体が配列だからこれは配列の配列であることに注意してね
                for (var i = 0; atSubCrossTarget.Length > i; i += 1)
                {
                    var atSub = atSubCrossTarget[i];
                    var bitArray = new BitArray(uvIndexCount);
                    foreach (var tri in meshData.TriangleIndex[atSub.SubMeshIndex])
                    {
                        for (var ti = 0; 3 > ti; ti += 1)
                        {
                            bitArray[uvToIndex[uv[tri[ti]]]] = true;
                        }
                    }
                    usedUVVertIndexUsed[i] = bitArray;
                }

                var needMergeAtSubIndex = new Dictionary<int, int>();

                for (var fromIndex = 0; atSubCrossTarget.Length > fromIndex; fromIndex += 1)
                {
                    for (var toIndex = fromIndex + 1; atSubCrossTarget.Length > toIndex; toIndex += 1)
                    {
                        var fromBitArray = usedUVVertIndexUsed[fromIndex];
                        var toBitArray = usedUVVertIndexUsed[toIndex];
                        for (var vi = 0; usedUVVertIndexUsed[fromIndex].Length > vi; vi += 1)
                        {
                            if (fromBitArray[vi] && toBitArray[vi]) { needMergeAtSubIndex[fromIndex] = toIndex; break; }
                        }
                    }
                }

                foreach (var mki in needMergeAtSubIndex)
                {
                    var fromAtSub = atSubCrossTarget[mki.Key];
                    var toAtSub = atSubCrossTarget[mki.Value];


                    var islandToUseIndexBits = originIslandDict[fromAtSub].Concat(originIslandDict[toAtSub])
                        .Select(islandToUseBitArray)
                        .ToDictionary(i => i.Item1, i => i.Item2);
                    (Island, BitArray) islandToUseBitArray(Island i)
                    {
                        var islandBitArray = new BitArray(uvIndexCount);
                        foreach (var tri in i.Triangles)
                        {
                            for (var ti = 0; 3 > ti; ti += 1)
                            {
                                islandBitArray[uvToIndex[uv[tri[ti]]]] = true;
                            }
                        }
                        return (i, islandBitArray);
                    }


                    foreach (var fIsland in originIslandDict[fromAtSub])
                    {
                        foreach (var tIsland in originIslandDict[toAtSub])
                        {
                            var fBit = islandToUseIndexBits[fIsland];
                            var tBit = islandToUseIndexBits[tIsland];

                            var needMerge = false;
                            for (var vi = 0; fBit.Length > vi; vi += 1)
                            {
                                if (fBit[vi] && tBit[vi]) { needMerge = true; break; }
                            }
                            if (needMerge is false) { continue; }

                            var mergedVirtualIsland = MergeIslandTransform(origin2VirtualIsland[fIsland], origin2VirtualIsland[tIsland]);
                            origin2VirtualIsland[fIsland] = origin2VirtualIsland[tIsland] = mergedVirtualIsland;
                        }
                    }
                }
            }
        }
        static void OverCrossIslandMerge(
            HashSet<AtlasSubMeshIndexID> atlasSubMeshIndexIDHash
            , Dictionary<AtlasSubMeshIndexID, List<Island>> originIslandDict
            , Dictionary<Island, IslandTransform> origin2VirtualIsland
            )
        {
            foreach (var atSubGroup in atlasSubMeshIndexIDHash.GroupBy(i => (i.MeshID, i.MaterialGroupID)))
            {
                var islands = atSubGroup.SelectMany(a => originIslandDict[a]).ToArray();

                var mergeLoop = true;
                while (mergeLoop) mergeLoop = IntersectMerge(origin2VirtualIsland, islands);
            }

            static bool IntersectMerge(Dictionary<Island, IslandTransform> origin2VirtualIsland, Island[] islands)
            {
                for (var index1 = 0; islands.Length > index1; index1 += 1)
                {
                    for (var index2 = index1 + 1; islands.Length > index2; index2 += 1)
                    {
                        var island1 = islands[index1];
                        var island2 = islands[index2];

                        var vIsland1 = origin2VirtualIsland[island1];
                        var vIsland2 = origin2VirtualIsland[island2];

                        if (vIsland1 == vIsland2) { continue; }
                        if (vIsland1.Intersect(vIsland2) is false) { continue; }

                        var merged = MergeIslandTransform(vIsland1, vIsland2);
                        // マージしたあとの面積がどれだけ広がっているかや
                        // もともとの交差していた面積をを差し引きして
                        // マージしたときの増加分が交差している面積よりも小さい場合はマージする。

                        var intersected = vIsland1.IntersectBox(vIsland2);

                        var area1 = vIsland1.GetArea();
                        var area2 = vIsland2.GetArea();
                        var mergedArea = merged.GetArea();
                        var intersectedArea = intersected.GetArea();

                        var increaseArea = mergedArea - (area1 + area2 - intersectedArea);
                        if (intersectedArea <= increaseArea) { continue; }
                        origin2VirtualIsland[island1] = origin2VirtualIsland[island2] = merged;
                    }
                }
                return false;
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.AAOCode
{
    //これは MeshInfo2 を改造して簡易的に使えるようにしたもの
    internal static class MeshInfoUtility
    {
        public static List<Vertex> ReadVertex(Mesh mesh, out MeshDesc meshDesc)
        {
            meshDesc = new();
            Profiler.BeginSample($"Read Static Mesh Part");

            var Vertices = new List<Vertex>(mesh.vertexCount);
            for (var i = 0; i < mesh.vertexCount; i++) { Vertices.Add(new()); }

            CopyVertexAttr(mesh.vertices, (x, v) => x.Position = v);
            if (mesh.GetVertexAttributeDimension(VertexAttribute.Normal) != 0)
            {
                meshDesc.HasNormals = true;
                CopyVertexAttr(mesh.normals, (x, v) => x.Normal = v);
            }
            if (mesh.GetVertexAttributeDimension(VertexAttribute.Tangent) != 0)
            {
                meshDesc.HasTangent = true;
                CopyVertexAttr(mesh.tangents, (x, v) => x.Tangent = v);
            }
            if (mesh.GetVertexAttributeDimension(VertexAttribute.Color) != 0)
            {
                meshDesc.HasColor = true;
                CopyVertexAttr(mesh.colors32, (x, v) => x.Color = v);
            }

            var uv2 = new List<Vector2>(0);
            var uv3 = new List<Vector3>(0);
            var uv4 = new List<Vector4>(0);
            for (var index = 0; index <= 7; index++)
            {
                // ReSharper disable AccessToModifiedClosure
                switch (mesh.GetVertexAttributeDimension(VertexAttribute.TexCoord0 + index))
                {
                    case 2:
                        meshDesc.SetTexCoordStatus(index, TexCoordStatus.Vector2);
                        mesh.GetUVs(index, uv2);
                        CopyVertexAttrFromList(uv2, (x, v) => x.SetTexCoord(index, v));
                        break;
                    case 3:
                        meshDesc.SetTexCoordStatus(index, TexCoordStatus.Vector3);
                        mesh.GetUVs(index, uv3);
                        CopyVertexAttrFromList(uv3, (x, v) => x.SetTexCoord(index, v));
                        break;
                    case 4:
                        meshDesc.SetTexCoordStatus(index, TexCoordStatus.Vector4);
                        mesh.GetUVs(index, uv4);
                        CopyVertexAttrFromList(uv4, (x, v) => x.SetTexCoord(index, v));
                        break;
                }

                // ReSharper restore AccessToModifiedClosure
            }
            void CopyVertexAttr<T>(T[] attributes, Action<Vertex, T> assign)
            {
                for (var i = 0; i < attributes.Length; i++)
                    assign(Vertices[i], attributes[i]);
            }

            void CopyVertexAttrFromList<T>(List<T> attributes, Action<Vertex, T> assign)
            {
                for (var i = 0; i < attributes.Count; i++)
                    assign(Vertices[i], attributes[i]);
            }

            Profiler.BeginSample("Read Bones");
            meshDesc.Bones = new(mesh.bindposes.Length);
            meshDesc.Bones.AddRange(mesh.bindposes.Select(x => new Bone(x)));

            var bonesPerVertex = mesh.GetBonesPerVertex();
            var allBoneWeights = mesh.GetAllBoneWeights();
            var bonesBase = 0;
            for (var i = 0; i < bonesPerVertex.Length; i++)
            {
                int count = bonesPerVertex[i];
                Vertices[i].BoneWeights.Capacity = count;
                foreach (var boneWeight1 in allBoneWeights.AsReadOnlySpan().Slice(bonesBase, count))
                    Vertices[i].BoneWeights.Add((meshDesc.Bones[boneWeight1.boneIndex], boneWeight1.weight));
                bonesBase += count;
            }
            Profiler.EndSample();


            meshDesc.BlendShapes = new();
            Profiler.BeginSample("Prepare shared buffers");
            var maxFrames = 0;
            var frameCounts = new NativeArray<int>(mesh.blendShapeCount, Allocator.TempJob);
            var shapeNames = new string[mesh.blendShapeCount];
            for (var i = 0; i < mesh.blendShapeCount; i++)
            {
                var frames = mesh.GetBlendShapeFrameCount(i);
                shapeNames[i] = mesh.GetBlendShapeName(i);
                maxFrames = Math.Max(frames, maxFrames);
                frameCounts[i] = frames;
            }

            var deltaVertices = new Vector3[Vertices.Count];
            var deltaNormals = new Vector3[Vertices.Count];
            var deltaTangents = new Vector3[Vertices.Count];
            var allFramesBuffer = new NativeArray3<Vertex.BlendShapeFrame>(mesh.blendShapeCount, Vertices.Count,
                maxFrames, Allocator.TempJob);
            var meaningfuls = new NativeArray2<bool>(mesh.blendShapeCount, Vertices.Count, Allocator.TempJob);
            Profiler.EndSample();

            for (var blendShape = 0; blendShape < mesh.blendShapeCount; blendShape++)
            {
                meshDesc.BlendShapes.Add((shapeNames[blendShape], 0.0f));

                for (var frame = 0; frame < frameCounts[blendShape]; frame++)
                {
                    Profiler.BeginSample("GetFrameInfo");
                    mesh.GetBlendShapeFrameVertices(blendShape, frame, deltaVertices, deltaNormals, deltaTangents);
                    var weight = mesh.GetBlendShapeFrameWeight(blendShape, frame);
                    Profiler.EndSample();

                    Profiler.BeginSample("Copy to buffer");
                    for (var vertex = 0; vertex < deltaNormals.Length; vertex++)
                    {
                        var deltaVertex = deltaVertices[vertex];
                        var deltaNormal = deltaNormals[vertex];
                        var deltaTangent = deltaTangents[vertex];
                        allFramesBuffer[blendShape, vertex, frame] = new Vertex.BlendShapeFrame(weight, deltaVertex, deltaNormal, deltaTangent);
                    }
                    Profiler.EndSample();
                }
            }

            Profiler.BeginSample("Compute Meaningful with Job");
            new ComputeMeaningfulJob
            {
                vertexCount = Vertices.Count,
                allFramesBuffer = allFramesBuffer,
                frameCounts = frameCounts,
                meaningfuls = meaningfuls,
            }.Schedule(Vertices.Count * mesh.blendShapeCount, 1).Complete();
            Profiler.EndSample();

            for (var blendShape = 0; blendShape < mesh.blendShapeCount; blendShape++)
            {
                Profiler.BeginSample("Save to Vertices");
                for (var vertex = 0; vertex < Vertices.Count; vertex++)
                {
                    if (meaningfuls[blendShape, vertex])
                    {
                        Profiler.BeginSample("Clone BlendShapes");
                        var slice = allFramesBuffer[blendShape, vertex].Slice(0, frameCounts[blendShape]);
                        Vertices[vertex].BlendShapes[shapeNames[blendShape]] = slice.ToArray();
                        Profiler.EndSample();
                    }
                }
                Profiler.EndSample();
            }

            meaningfuls.Dispose();
            frameCounts.Dispose();
            allFramesBuffer.Dispose();


            Profiler.EndSample();

            return Vertices;
        }


        [BurstCompile]
        struct ComputeMeaningfulJob : IJobParallelFor
        {
            public int vertexCount;

            // allFramesBuffer[blendShape][vertex][frame]
            [ReadOnly]
            public NativeArray3<Vertex.BlendShapeFrame> allFramesBuffer;
            [ReadOnly]
            public NativeArray<int> frameCounts;
            // allFramesBuffer[blendShape][vertex]
            [WriteOnly]
            public NativeArray2<bool> meaningfuls;

            public void Execute(int index)
            {
                var blendShape = index / vertexCount;
                var vertex = index % vertexCount;
                var slice = allFramesBuffer[blendShape, vertex].Slice(0, frameCounts[blendShape]);
                meaningfuls[blendShape, vertex] = IsMeaningful(slice);
            }

            bool IsMeaningful(NativeSlice<Vertex.BlendShapeFrame> frames)
            {
                foreach (var (_, position, normal, tangent) in frames)
                {
                    if (position != Vector3.zero) return true;
                    if (normal != Vector3.zero) return true;
                    if (tangent != Vector3.zero) return true;
                }

                return false;
            }
        }

        public static void WriteVertex(Mesh destMesh, MeshDesc meshAttribute, List<Vertex> Vertices)
        {
            Profiler.BeginSample("Write to Mesh");

            Profiler.BeginSample("Vertices and Normals");
            // Basic Vertex Attributes: vertices, normals
            {
                var vertices = new Vector3[Vertices.Count];
                for (var i = 0; i < Vertices.Count; i++)
                    vertices[i] = Vertices[i].Position;
                destMesh.vertices = vertices;
            }

            // tangents
            if (meshAttribute.HasNormals)
            {
                var normals = new Vector3[Vertices.Count];
                for (var i = 0; i < Vertices.Count; i++)
                    normals[i] = Vertices[i].Normal;
                destMesh.normals = normals;
            }
            Profiler.EndSample();

            // tangents
            if (meshAttribute.HasTangent)
            {
                Profiler.BeginSample("Tangents");
                var tangents = new Vector4[Vertices.Count];
                for (var i = 0; i < Vertices.Count; i++)
                    tangents[i] = Vertices[i].Tangent;
                destMesh.tangents = tangents;
                Profiler.EndSample();
            }

            // UVs
            {
                var uv2 = new Vector2[Vertices.Count];
                var uv3 = new Vector3[Vertices.Count];
                var uv4 = new Vector4[Vertices.Count];
                for (var uvIndex = 0; uvIndex < 8; uvIndex++)
                {
                    Profiler.BeginSample($"UV#{uvIndex}");
                    switch (meshAttribute.GetTexCoordStatus(uvIndex))
                    {
                        case TexCoordStatus.NotDefined:
                            // nothing to do
                            break;
                        case TexCoordStatus.Vector2:
                            for (var i = 0; i < Vertices.Count; i++)
                                uv2[i] = Vertices[i].GetTexCoord(uvIndex);
                            destMesh.SetUVs(uvIndex, uv2);
                            break;
                        case TexCoordStatus.Vector3:
                            for (var i = 0; i < Vertices.Count; i++)
                                uv3[i] = Vertices[i].GetTexCoord(uvIndex);
                            destMesh.SetUVs(uvIndex, uv3);
                            break;
                        case TexCoordStatus.Vector4:
                            for (var i = 0; i < Vertices.Count; i++)
                                uv4[i] = Vertices[i].GetTexCoord(uvIndex);
                            destMesh.SetUVs(uvIndex, uv4);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    Profiler.EndSample();
                }
            }

            // color
            if (meshAttribute.HasColor)
            {
                Profiler.BeginSample($"Vertex Color");
                var colors = new Color32[Vertices.Count];
                for (var i = 0; i < Vertices.Count; i++)
                    colors[i] = Vertices[i].Color;
                destMesh.colors32 = colors;
                Profiler.EndSample();
            }


            // bones
            destMesh.bindposes = meshAttribute.Bones.Select(x => x.Bindpose).ToArray();

            // BoneWeights
            if (Vertices.Any(x => x.BoneWeights.Count != 0))
            {
                Profiler.BeginSample("BoneWeights");
                var boneIndices = new Dictionary<Bone, int>();
                for (var i = 0; i < meshAttribute.Bones.Count; i++)
                    boneIndices.Add(meshAttribute.Bones[i], i);

                var bonesPerVertex = new NativeArray<byte>(Vertices.Count, Allocator.Temp);
                var allBoneWeights =
                    new NativeArray<BoneWeight1>(Vertices.Sum(x => x.BoneWeights.Count), Allocator.Temp);
                var boneWeightsIndex = 0;
                for (var i = 0; i < Vertices.Count; i++)
                {
                    bonesPerVertex[i] = (byte)Vertices[i].BoneWeights.Count;
                    Vertices[i].BoneWeights.Sort((x, y) => -x.weight.CompareTo(y.weight));
                    foreach (var (bone, weight) in Vertices[i].BoneWeights)
                        allBoneWeights[boneWeightsIndex++] = new BoneWeight1
                        { boneIndex = boneIndices[bone], weight = weight };
                }

                destMesh.SetBoneWeights(bonesPerVertex, allBoneWeights);
                Profiler.EndSample();
            }

            // BlendShapes
            if (meshAttribute.BlendShapes.Count != 0)
            {
                Profiler.BeginSample("BlendShapes");
                for (var i = 0; i < meshAttribute.BlendShapes.Count; i++)
                {
                    Debug.Assert(destMesh.blendShapeCount == i, "Unexpected state: BlendShape count");
                    var (shapeName, _) = meshAttribute.BlendShapes[i];
                    var weightsSet = new HashSet<float>();

                    foreach (var vertex in Vertices)
                        if (vertex.BlendShapes.TryGetValue(shapeName, out var frames))
                            foreach (var frame in frames)
                                weightsSet.Add(frame.Weight);

                    // blendShape with no weights is not allowed.
                    if (weightsSet.Count == 0)
                        weightsSet.Add(100);

                    var weights = weightsSet.ToArray();
                    Array.Sort(weights);

                    var positions = new Vector3[Vertices.Count];
                    var normals = new Vector3[Vertices.Count];
                    var tangents = new Vector3[Vertices.Count];

                    foreach (var weight in weights)
                    {
                        for (var vertexI = 0; vertexI < Vertices.Count; vertexI++)
                        {
                            var vertex = Vertices[vertexI];

                            vertex.TryGetBlendShape(shapeName, weight,
                                out var position, out var normal, out var tangent,
                                getDefined: true);
                            positions[vertexI] = position;
                            normals[vertexI] = normal;
                            tangents[vertexI] = tangent;
                        }

                        destMesh.AddBlendShapeFrame(shapeName, weight, positions, normals, tangents);
                    }
                }
                Profiler.EndSample();
            }

            Profiler.EndSample();
        }




        public class MeshDesc
        {
            public List<(string name, float weight)> BlendShapes = new List<(string name, float weight)>(0);
            public List<Bone> Bones = new List<Bone>();

            // TexCoordStatus which is 3 bits x 8 = 24 bits
            private ushort _texCoordStatus;
            public bool HasColor { get; set; }
            public bool HasNormals { get; set; }
            public bool HasTangent { get; set; }

            private const int BitsPerTexCoordStatus = 2;
            private const int TexCoordStatusMask = (1 << BitsPerTexCoordStatus) - 1;

            public TexCoordStatus GetTexCoordStatus(int index)
            {
                return (TexCoordStatus)((_texCoordStatus >> (index * BitsPerTexCoordStatus)) & TexCoordStatusMask);
            }
            public void SetTexCoordStatus(int index, TexCoordStatus value)
            {
                _texCoordStatus = (ushort)(
                    (uint)_texCoordStatus & ~(TexCoordStatusMask << (BitsPerTexCoordStatus * index)) |
                    ((uint)value & TexCoordStatusMask) << (BitsPerTexCoordStatus * index));
            }
        }


    }

}

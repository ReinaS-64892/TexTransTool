using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransCore.Decal;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasReferenceData : IDisposable
    {
        public OrderedHashSet<Mesh> Meshes;
        public HashSet<Material> TargetMaterials;
        public OrderedHashSet<Material> Materials;
        public List<AtlasMeshData> AtlasMeshDataList;
        public AtlasReferenceData(List<Material> targetMaterials, List<Renderer> inputRenderers)
        {
            TargetMaterials = new HashSet<Material>(targetMaterials);
            Meshes = new(); Materials = new();
            var Renderers = new List<Renderer>();

            foreach (var renderer in inputRenderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (TargetMaterials.Contains(mat))
                    {
                        Meshes.Add(renderer.GetMesh());
                        Materials.AddRange(renderer.sharedMaterials);
                        Renderers.Add(renderer);
                        break;
                    }
                }
            }

            AtlasMeshDataList = new();

            foreach (var renderer in Renderers)
            {
                var mesh = renderer.GetMesh();
                var refMesh = Meshes.IndexOf(mesh);
                var materialIndex = renderer.sharedMaterials.Select(Mat => Materials.IndexOf(Mat)).ToArray();
                int index = GetAtlasMeshDataIndex(refMesh, materialIndex);
                if (index == -1)
                {
                    var uv = new List<Vector2>();
                    mesh.GetUVs(0, uv);

                    AtlasMeshDataList.Add(new(
                        refMesh,
                        mesh.GetSubTriangleIndex(),
                        uv,
                        materialIndex,
                        renderer
                        ));
                }
            }
        }



        internal int GetAtlasMeshDataIndex(int refMesh, int[] materialIndex)
        {
            return AtlasMeshDataList.FindIndex(AMD => AMD.ReferenceMesh == refMesh && AMD.MaterialIndex.SequenceEqual(materialIndex));
        }

        public class AtlasMeshData
        {
            //RefData
            public int ReferenceMesh;
            public int[] MaterialIndex;

            //for Generate
            public readonly List<List<TriangleIndex>> Triangles;
            public List<Vector2> UV;
            public List<Vector2> MovedUV;
            public Renderer Renderer;
            public MeshData MeshData;

            public AtlasMeshData(int referenceMesh, List<List<TriangleIndex>> triangles, List<Vector2> uv, int[] materialIndex, Renderer renderer)
            {
                ReferenceMesh = referenceMesh;
                Triangles = triangles;
                UV = uv;
                MaterialIndex = materialIndex;
                Renderer = renderer;
                MeshData = new MeshData(renderer);
            }
            public AtlasMeshData()
            {
                Triangles = new List<List<TriangleIndex>>();
                UV = new List<Vector2>();
            }
        }


        /// <summary>
        ///  すべてをアイランドにし、同一の物を指すアイランドは排除したものを返します。
        /// </summary>
        /// <param name="islandCache"></param>
        /// <returns></returns>
        public Dictionary<AtlasIslandID, Island> GeneratedIslandPool()
        {
            var islandPool = new Dictionary<AtlasIslandID, Island>();
            var amdCount = AtlasMeshDataList.Count;
            var islandIndex = 0;
            for (int amdIndex = 0; amdIndex < amdCount; amdIndex += 1)
            {
                var atlasMeshData = AtlasMeshDataList[amdIndex];

                for (var SlotIndex = 0; atlasMeshData.MaterialIndex.Length > SlotIndex; SlotIndex += 1)
                {
                    if (!TargetMaterials.Contains(Materials[atlasMeshData.MaterialIndex[SlotIndex]])) { continue; }
                    if (atlasMeshData.Triangles.Count <= SlotIndex) { continue; }

                    var islands = IslandUtility.UVtoIsland(atlasMeshData.Triangles[SlotIndex], atlasMeshData.UV);
                    foreach (var island in islands) { islandPool.Add(new AtlasIslandID(amdIndex, SlotIndex, islandIndex), island); islandIndex += 1; }
                }
            }

            var refsHash = new HashSet<MSMRef>();
            var deleteTags = new HashSet<AtlasSlotRef>();
            foreach (var tag in islandPool.Keys.Select(i => new AtlasSlotRef(i.AtlasMeshDataIndex, i.MaterialSlot)).Distinct())
            {
                var atlasMeshData = AtlasMeshDataList[tag.AtlasMeshDataIndex];

                var refMesh = atlasMeshData.ReferenceMesh;
                var materialSlot = tag.MaterialSlot;
                var refMat = atlasMeshData.MaterialIndex[tag.MaterialSlot];

                var refs = new MSMRef(refMesh, materialSlot, refMat);

                if (refsHash.Contains(refs)) { deleteTags.Add(tag); }
                else { refsHash.Add(refs); }
            }

            var filteredIslandPool = new Dictionary<AtlasIslandID, Island>(islandPool.Count);
            islandIndex = 0;
            foreach (var idPair in islandPool)
            {
                var atlasID = idPair.Key;
                var island = idPair.Value;

                if (deleteTags.Contains(new AtlasSlotRef(atlasID.AtlasMeshDataIndex, atlasID.MaterialSlot))) { continue; }

                atlasID.IslandIndex = islandIndex;
                filteredIslandPool.Add(atlasID, island);
                islandIndex += 1;
            }


            //サブメッシュ間で頂点を共有するアイランドのマージ
            var containsIdenticalIslandForMultipleSubMesh = false;
            for (var amdIndex = 0; AtlasMeshDataList.Count > amdIndex; amdIndex += 1)
            {
                var amd = AtlasMeshDataList[amdIndex];

                var beyondVert = amd.Triangles.Where(i => TargetMaterials.Contains(Materials[amd.MaterialIndex[amd.Triangles.IndexOf(i)]]))
                .Select(i => new HashSet<int>(i.SelectMany(i2 => i2))).SelectMany(i => i)
                .GroupBy(i => i).Select(i => (i.Key, i.Count())).Where(i => i.Item2 > 1).Select(i => i.Key).ToHashSet();

                if (beyondVert.Any()) { containsIdenticalIslandForMultipleSubMesh = true; }
                else { continue; }

                var needMerge = filteredIslandPool.Where(i => i.Key.AtlasMeshDataIndex == amdIndex).Where(i => i.Value.triangles.SelectMany(i => i).Any(i => beyondVert.Contains(i))).GroupBy(i => i.Key.MaterialSlot).ToList();
                needMerge.Sort((l, r) => l.Key - r.Key);

                var needMergeIslands = needMerge.Select(i => i.ToHashSet()).ToArray();
                var MargeKV = new Dictionary<AtlasIslandID, HashSet<AtlasIslandID>>();

                for (var toIndex = 0; needMergeIslands.Length > toIndex; toIndex += 1)
                {
                    foreach (var island in needMergeIslands[toIndex])
                    {
                        var vertSet = island.Value.triangles.SelectMany(i => i).ToHashSet();

                        for (var fromIndex = toIndex; needMergeIslands.Length > fromIndex; fromIndex += 1)
                        {
                            if (toIndex == fromIndex) { continue; }

                            var margeFrom = needMergeIslands[fromIndex].Where(il => il.Value.triangles.SelectMany(v => v).Any(v => vertSet.Contains(v)));
                            if (margeFrom.Any()) { MargeKV.Add(island.Key, margeFrom.Select(i => i.Key).ToHashSet()); }
                        }
                    }
                }

                foreach (var margeIdKV in MargeKV)
                {
                    var to = filteredIslandPool[margeIdKV.Key];

                    foreach (var formKey in margeIdKV.Value)
                    {
                        to.triangles.AddRange(filteredIslandPool[formKey].triangles);
                        filteredIslandPool.Remove(formKey);
                    }
                }

            }
            if (containsIdenticalIslandForMultipleSubMesh) { TTTRuntimeLog.Warning("AtlasTexture:error:IdenticalIslandForMultipleSubMesh"); }








            return filteredIslandPool;
        }

        public Material GetMaterialReference(AtlasIslandID indexTag)
        {
            return GetMaterialReference(indexTag.AtlasMeshDataIndex, indexTag.MaterialSlot);
        }
        private Material GetMaterialReference(int atlasMeshDataIndex, int materialSlot)
        {
            return Materials[AtlasMeshDataList[atlasMeshDataIndex].MaterialIndex[materialSlot]];
        }
        internal AtlasSlotRef? FindIdenticalSlotRef(HashSet<AtlasSlotRef> slotRefs, MSMRef findTarget)
        {
            foreach (var tag in slotRefs)
            {
                var tagAmd = AtlasMeshDataList[tag.AtlasMeshDataIndex];

                if (findTarget.RefMesh != tagAmd.ReferenceMesh) { continue; }
                if (findTarget.RefMatSlot != tag.MaterialSlot) { continue; }
                if (findTarget.RefMat != tagAmd.MaterialIndex[tag.MaterialSlot]) { continue; }

                return tag;
            }

            return null;
        }

        internal static IEnumerable<KeyValuePair<AtlasIslandID, IslandRect>> IslandFind(AtlasSlotRef atlasIdenticalTag, Dictionary<AtlasIslandID, IslandRect> islandRect)
        {
            foreach (var islandKV in islandRect)
            {
                if (islandKV.Key.AtlasMeshDataIndex != atlasIdenticalTag.AtlasMeshDataIndex) { continue; }
                if (islandKV.Key.MaterialSlot != atlasIdenticalTag.MaterialSlot) { continue; }

                yield return islandKV;
            }
        }

        public void Dispose()
        {
            foreach (var amd in AtlasMeshDataList) { amd.MeshData.Dispose(); }
        }
    }

    internal struct MSMRef
    {
        public int RefMesh;
        public int RefMatSlot;
        public int RefMat;

        public MSMRef(int refMesh, int matSlot, int refMat)
        {
            RefMesh = refMesh;
            RefMatSlot = matSlot;
            RefMat = refMat;
        }

        public override bool Equals(object obj)
        {
            return obj is MSMRef other &&
                   RefMesh == other.RefMesh &&
                   RefMatSlot == other.RefMatSlot &&
                   RefMat == other.RefMat;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RefMesh, RefMatSlot, RefMat);
        }
    }
    internal struct AtlasSlotRef
    {
        public int AtlasMeshDataIndex;
        public int MaterialSlot;

        public AtlasSlotRef(int atlasMeshDataIndex, int materialSlot)
        {
            AtlasMeshDataIndex = atlasMeshDataIndex;
            MaterialSlot = materialSlot;
        }

        public static bool operator ==(AtlasSlotRef a, AtlasSlotRef b)
        {
            return a.AtlasMeshDataIndex == b.AtlasMeshDataIndex && a.MaterialSlot == b.MaterialSlot;
        }
        public static bool operator !=(AtlasSlotRef a, AtlasSlotRef b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is AtlasSlotRef tag && this == tag;
        }
        public override int GetHashCode()
        {
            return AtlasMeshDataIndex.GetHashCode() ^ MaterialSlot.GetHashCode();
        }
        internal static HashSet<AtlasSlotRef> ToSlotRefHash(IEnumerable<AtlasIslandID> tags)
        {
            var indexTag = new HashSet<AtlasSlotRef>();
            foreach (var tag in tags) { indexTag.Add(new(tag.AtlasMeshDataIndex, tag.MaterialSlot)); }
            return indexTag;
        }


    }
    public struct AtlasIslandID
    {
        public int AtlasMeshDataIndex;
        public int MaterialSlot;
        public int IslandIndex;

        public AtlasIslandID(int atlasMeshDataIndex, int materialSlot, int islandIndex)
        {
            AtlasMeshDataIndex = atlasMeshDataIndex;
            MaterialSlot = materialSlot;
            IslandIndex = islandIndex;
        }

        public static bool operator ==(AtlasIslandID a, AtlasIslandID b)
        {
            return a.IslandIndex == b.IslandIndex && a.AtlasMeshDataIndex == b.AtlasMeshDataIndex && a.MaterialSlot == b.MaterialSlot;
        }
        public static bool operator !=(AtlasIslandID a, AtlasIslandID b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is AtlasIslandID tag && this == tag;
        }
        public override int GetHashCode()
        {
            return IslandIndex.GetHashCode() ^ AtlasMeshDataIndex.GetHashCode() ^ MaterialSlot.GetHashCode();
        }
    }
}

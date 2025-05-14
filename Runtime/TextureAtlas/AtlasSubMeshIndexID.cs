using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public struct AtlasSubMeshIndexID
    {
        public int MeshID;
        public int SubMeshIndex;
        public int MaterialGroupID;
        public AtlasSubMeshIndexID(int meshID, int subMeshIndex, int materialGroupID)
        {
            MeshID = meshID;
            SubMeshIndex = subMeshIndex;
            MaterialGroupID = materialGroupID;
        }
        public override bool Equals(object obj)
        {
            return obj is AtlasSubMeshIndexID other && this == other;
        }
        public override int GetHashCode() { return HashCode.Combine(MeshID, SubMeshIndex, MaterialGroupID); }

        public static bool operator ==(AtlasSubMeshIndexID l, AtlasSubMeshIndexID r)
        {
            if (l.MeshID != r.MeshID) { return false; }
            if (l.SubMeshIndex != r.SubMeshIndex) { return false; }
            if (l.MaterialGroupID != r.MaterialGroupID) { return false; }
            return true;
        }
        public static bool operator !=(AtlasSubMeshIndexID l, AtlasSubMeshIndexID r)
        {
            if (l.MeshID != r.MeshID) { return true; }
            if (l.SubMeshIndex != r.SubMeshIndex) { return true; }
            if (l.MaterialGroupID != r.MaterialGroupID) { return true; }
            return false;
        }

    }
    public static class AtlasSubMeshIndexIDUtility
    {
        // public static (AtlasSubMeshIndexID?[] subset, HashSet<AtlasSubMeshIndexID> allID) GenerateAtlasSubMeshIndexID(IRendererTargeting targeting, Renderer[] renderers, HashSet<Material> targetMaterialHash)
        // {
        //     var allID = new HashSet<AtlasSubMeshIndexID>();
        //     var atlasSubSets = new List<AtlasSubMeshIndexID?[]>();
        //     for (var ri = 0; renderers.Length > ri; ri += 1)
        //     {
        //         var renderer = renderers[ri];
        //         var mats = targeting.GetMaterials(renderer);
        //         var mesh = targeting.GetMesh(renderer);
        //         var meshID = Array.IndexOf(Meshes, mesh);
        //         var atlasSubSet = new AtlasSubMeshIndexID?[mats.Length];
        //         for (var si = 0; mats.Length > si; si += 1)
        //         {
        //             if (!targetMaterialHash.Contains(mats[si])) { continue; }
        //             var matID = Array.FindIndex(MaterialGroup, i => i.Contains(mats[si]));
        //             var atSubData = atlasSubSet[si] = new(meshID, si, matID);
        //             allID.Add(atSubData.Value);
        //         }
        //         atlasSubSets.Add(atlasSubSet);
        //     }

        //     IdenticalSubSetRemove(atlasSubSets);
        //     AtlasSubSets = atlasSubSets;
        // }
    }



}

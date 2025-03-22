#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using System;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class MaterialGroupingContext
    {
        public readonly GroupMaterial[] GroupMaterials;
        public readonly IReadOnlyDictionary<Material, IReadOnlyDictionary<string, Texture>> ContainsTextureDictionaries;

        public readonly string? PrimaryTexturePropertyOrMaximum;
        public MaterialGroupingContext(HashSet<Material> targetMaterials, UVChannel atlasingTargetUVChannel, string? primaryTexturePropertyOrMaximum)
        {
            ContainsTextureDictionaries = targetMaterials
                .Select(m => (m, TTShaderTextureUsageInformationUtil.GetContainsUVUsage(m)))
                .Select(kv => (
                    kv.m,
                    kv.Item2.Where(u => (((int)u.Value) - 1) == (int)atlasingTargetUVChannel)
                        .Select(u => (u.Key, kv.m.GetTexture(u.Key)))
                        .Where(u => u.Item2 != null)
                        .ToDictionary(
                            u => u.Key,
                            u => u.Item2
                        )
                    )
                ).ToDictionary(kv => kv.m, kv => kv.Item2 as IReadOnlyDictionary<string, Texture>);

            var groupMaterials = new List<GroupMaterial>();
            foreach (var (keyMat, containsTextures) in ContainsTextureDictionaries)
            {
                AddGroups(keyMat, containsTextures);
                void AddGroups(Material keyMat, IReadOnlyDictionary<string, Texture> containsTextures)
                {
                    foreach (var group in groupMaterials)
                    {
                        if (group.Add(keyMat, containsTextures) is GroupMaterial.AddResult.Success or GroupMaterial.AddResult.Already) { return; }
                    }
                    groupMaterials.Add(new GroupMaterial { { keyMat, containsTextures } });
                }
            }
            GroupMaterials = groupMaterials.ToArray();
            PrimaryTexturePropertyOrMaximum = primaryTexturePropertyOrMaximum;
        }


        public int GetMaterialGroupID(Material material)
        {
            return Array.FindIndex(GroupMaterials, i => i.Contains(material));
        }
        public Texture? GetPrimaryTexture(int groupID)
        {
            return GroupMaterials[groupID].GetPrimaryTexture(PrimaryTexturePropertyOrMaximum);
        }
        public HashSet<string> GetContainsAllProperties()
        {
            return GroupMaterials.SelectMany(i => i.GroupedTexture.Keys).ToHashSet();
        }


        public class GroupMaterial : IEnumerable<Material>
        {
            Dictionary<Material, IReadOnlyDictionary<string, Texture>> _containsTextures = new();
            Dictionary<string, Texture> _groupedTexture = new();

            public int Count => _containsTextures.Count;

            public AddResult Add(Material mat, IReadOnlyDictionary<string, Texture> textures)
            {
                if (_containsTextures.ContainsKey(mat)) { return AddResult.Already; } // already
                if (IsGroupCompatible(textures) is false) { return AddResult.NotCompatible; }

                _containsTextures[mat] = textures;
                foreach (var texKV in textures)
                {
                    if (texKV.Value == null) { continue; }
                    _groupedTexture[texKV.Key] = texKV.Value;
                }
                return AddResult.Success;
            }
            bool IsGroupCompatible(IReadOnlyDictionary<string, Texture> textures)
            {
                foreach (var propName in _groupedTexture.Keys)
                {
                    var groupTex = _groupedTexture[propName];
                    var addMaterialTex = textures.GetValueOrDefault(propName);

                    if (addMaterialTex == null) { continue; }
                    if (groupTex != addMaterialTex) { return false; }
                }
                return true;
            }

            public enum AddResult
            {
                Success,
                NotCompatible,
                Already,

            }
            public IEnumerator<Material> GetEnumerator() { return _containsTextures.Keys.GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return _containsTextures.Keys.GetEnumerator(); }

            public bool Contains(Material? mat)
            {
                if (mat == null) { return false; }
                return _containsTextures.ContainsKey(mat);
            }

            public Texture? GetPrimaryTexture(string? primaryTexturePropertyOrMaximum)
            {
                if (primaryTexturePropertyOrMaximum is null) { return GetMaximum(); }
                if (_groupedTexture.TryGetValue(primaryTexturePropertyOrMaximum, out var tex)) { return tex; }
                return null;

                Texture? GetMaximum()
                {
                    Texture? tex = null;
                    int maxValue = 0;
                    foreach (var m in _groupedTexture.Values)
                    {
                        if (m == null) { continue; }
                        if (tex == null)
                        {
                            tex = m;
                            maxValue = tex.height * tex.width;
                            continue;
                        }

                        var mSize = m.height * m.width;
                        if (maxValue < mSize)
                        {
                            tex = m;
                            maxValue = mSize;
                        }
                    }
                    return tex;
                }

            }

            public IReadOnlyDictionary<string, Texture> GroupedTexture => _groupedTexture;
        }

    }
}

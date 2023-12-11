#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
namespace net.rs64.TexTransTool.ReferenceResolver.MLIResolver
{
    [RequireComponent(typeof(MultiLayerImageCanvas))]
    [AddComponentMenu("TexTransTool/Resolver/TTT MultiLayerImageCanvas AbsoluteTextureResolver")]
    internal class AbsoluteTextureResolver : AbstractResolver
    {
        public Texture2D Texture;

        public override void Resolving(AvatarBuildUtils.ResolverContext avatar)
        {
            var relativeTexture = FindRelativeTexture(avatar.AvatarRoot, Texture);
            if (relativeTexture != null)
            {
                GetComponent<MultiLayerImageCanvas>().TextureSelector = relativeTexture;
            }
        }

        public static RelativeTextureSelector FindRelativeTexture(GameObject findRoot, Texture2D texture2D)
        {
            var searchedMaterial = new HashSet<Material>();
            foreach (var renderer in findRoot.GetComponentsInChildren<Renderer>())
            {
                if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
                {
                    var mats = renderer.sharedMaterials;
                    for (var i = 0; mats.Length > i; i += 1)
                    {
                        var mat = mats[i];
                        if (mat == null) { continue; }
                        if (searchedMaterial.Contains(mat)) { continue; }
                        var allTexture = mat.GetAllTexture2D();
                        if (allTexture.ContainsValue(texture2D))
                        {
                            var findKVP = allTexture.FirstOrDefault(I => I.Value == texture2D);
                            if (findKVP.Value != null)
                            {
                                return new () { TargetRenderer = renderer, MaterialSelect = i, TargetPropertyName = new (findKVP.Key) };
                            }
                        }
                        searchedMaterial.Add(mat);
                    }

                }
            }
            return null;
        }
    }
}
#endif
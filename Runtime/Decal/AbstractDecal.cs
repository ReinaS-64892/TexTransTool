#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Rs64.TexTransTool.Decal
{
    public abstract class AbstractDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public Texture2D DecalTexture;
        public BlendType BlendType = BlendType.Normal;
        public string TargetPropatyName = "_MainTex";
        public bool MultiRendereMode = false;
        public DecalDataContainer Container;

        protected Material[] GetMaterials()
        {
            return TargetRenderers.Select(i => i.sharedMaterial).Distinct().ToArray();
        }
        protected Material[] EditableClone(Material[] Souse)
        {
            return Souse.Select(i => i == null ? null : Instantiate<Material>(i)).ToArray();
        }

        [SerializeField] protected bool _IsAppry = false;
        public override bool IsAppry => _IsAppry;
        public override bool IsPossibleAppry => Container != null;
        public override bool IsPossibleCompile => DecalTexture != null && TargetRenderers.Any(i => i != null);
    }
}



#endif
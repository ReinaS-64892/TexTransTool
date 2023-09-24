#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    public class MaterialModifier : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        public override List<Renderer> GetRenderers => TargetRenderers;
        public override bool IsPossibleApply => ModifiedTarget.Any();
        public List<Material> ModifiedTarget = new List<Material>();
        public List<Material> GetContainsMatTarget
        {
            get
            {
                var hashSet = new HashSet<Material>(RendererUtility.GetMaterials(GetRenderers));
                return ModifiedTarget.Where(I => hashSet.Contains(I)).ToList();
            }
        }
        public List<MatMod> ChangeList = new List<MatMod>();

        public class MatMod
        {
            public ModTypeEnum ModType;
            public enum ModTypeEnum
            {
                Float,
                Texture,
                Color,
            }

            public string Float_PropertyName;
            public float Float_Value;


            public PropertyName Texture_PropertyName;
            public Texture Texture_Value;

            public bool SetBlankTexture;
            public Vector2Int TextureSize;


            public string Color_PropertyName;
            public Color Color_Value;

            public void Modified(Material material, List<Texture2D> BlankTexture2Ds)
            {
                switch (ModType)
                {
                    case ModTypeEnum.Float:
                        {
                            if (!material.HasProperty(Float_PropertyName)) { break; }
                            material.SetFloat(Float_PropertyName, Float_Value);
                            break;
                        }
                    case ModTypeEnum.Texture:
                        {
                            if (!material.HasProperty(Texture_PropertyName)) { break; }
                            if (!SetBlankTexture)
                            {
                                material.SetTexture(Texture_PropertyName, Texture_Value);
                            }
                            else
                            {
                                var newBlankTex = CoreUtility.CreateFillTexture(TextureSize, new Color(0, 0, 0, 0));
                                BlankTexture2Ds.Add(newBlankTex);
                                material.SetTexture(Texture_PropertyName, newBlankTex);
                            }
                            break;
                        }
                    case ModTypeEnum.Color:
                        {
                            if (!material.HasProperty(Color_PropertyName)) { break; }
                            material.SetColor(Color_PropertyName, Color_Value);
                            break;
                        }
                }
            }

        }
        public override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public override void Apply([NotNull] IDomain domain)
        {
            var newTexList = new List<Texture2D>();
            var newMatList = new List<Material>();
            foreach (var modTarget in GetContainsMatTarget)
            {
                var newMat = Instantiate(modTarget);
                newMatList.Add(newMat);
                foreach (var Modified in ChangeList)
                {
                    Modified.Modified(newMat, newTexList);
                }
            }
            domain.transferAssets(newTexList);
            domain.transferAssets(newMatList);
        }
    }
}
#endif
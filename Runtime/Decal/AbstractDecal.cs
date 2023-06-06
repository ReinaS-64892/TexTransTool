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
        public float DefaultPading = -1;

        protected Material[] GetMaterials()
        {
            return TargetRenderers.Select(i => i.sharedMaterial).Distinct().ToArray();
        }
        protected Material[] EditableClone(Material[] Souse)
        {
            return Souse.Select(i => i == null ? null : Instantiate<Material>(i)).ToArray();
        }

        [SerializeField] protected bool _IsApply = false;
        public override bool IsApply => _IsApply;
        public override bool IsPossibleApply => Container != null;
        public override bool IsPossibleCompile => DecalTexture != null && TargetRenderers.Any(i => i != null);

        public static Dictionary<Material, Texture2D> ZipAndBlendTextures(List<Dictionary<Material, List<Texture2D>>> DictCompiledTextures, BlendType BlendType = BlendType.AlphaLerp)
        {
            var ResultTexutres = Utils.ZipToDictionaryOnList(DictCompiledTextures);
            var RetDict = new Dictionary<Material, Texture2D>();
            foreach (var kvp in ResultTexutres)
            {
                var Mat = kvp.Key;
                var Texs = kvp.Value;
                var Tex = Texs.Count == 1 ? Texs.FirstOrDefault() : TextureLayerUtil.BlendTexturesUseComputeSheder(null, Texs, BlendType);
                RetDict.Add(Mat, Tex);
            }

            return RetDict;
        }
        protected virtual void SetContainer(List<Texture2D> Texs)
        {
            if (Container == null) { Container = ScriptableObject.CreateInstance<DecalDataContainer>(); AssetSaveHelper.SaveAsset(Container); }
            Container.DecalCompiledTextures = Texs;
        }

        public virtual List<DecalUtil.Filtaring> GetFiltarings() { return null; }

        public override void Apply(MaterialDomain avatarMaterialDomain)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            _IsApply = true;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new MaterialDomain(TargetRenderers);

            var DistMaterials = Utils.GetMaterials(TargetRenderers);
            var DecalTextures = Container.DecalCompiledTextures;
            var PeadMaterial = new Dictionary<Material, Material>();
            var GeneretaTex = new List<Texture2D>();
            foreach (var Index in Enumerable.Range(0, DecalTextures.Count))
            {
                var DistMat = DistMaterials[Index];
                var DecalTex = DecalTextures[Index];

                if (DistMat == null || DecalTex == null) continue;

                if (DistMat.GetTexture(TargetPropatyName) is Texture2D OldTex)
                {
                    var Newtex = TextureLayerUtil.BlendTextureUseComputeSheder(null, OldTex, DecalTex, BlendType);
                    var SavedTex = AssetSaveHelper.SaveAsset(Newtex);

                    var NewMat = Instantiate<Material>(DistMat);
                    NewMat.SetTexture(TargetPropatyName, SavedTex);

                    if (PeadMaterial.ContainsKey(DistMat))
                    {
                        PeadMaterial[DistMat] = NewMat;
                    }
                    else
                    {
                        PeadMaterial.Add(DistMat, NewMat);
                    }
                    GeneretaTex.Add(SavedTex);
                }
            }

            Container.DecaleBlendTexteres = GeneretaTex;
            Container.GenereatMaterials = MatPea.GeneratMatPeaList(PeadMaterial);

            avatarMaterialDomain.SetMaterials(PeadMaterial);
        }

        public override void Revart(MaterialDomain avatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new MaterialDomain(TargetRenderers);

            var MatsDict = MatPea.SwitchingdList(Container.GenereatMaterials);
            avatarMaterialDomain.SetMaterials(MatPea.GeneratMatDict(MatsDict));
        }

        public virtual List<Vector3> ComvartSpace(List<Vector3> varticals)
        {
            return DecalUtil.ConvartVerticesInMatlix(transform.worldToLocalMatrix, varticals, new Vector3(0.5f, 0.5f, 0));
        }

    }
}



#endif
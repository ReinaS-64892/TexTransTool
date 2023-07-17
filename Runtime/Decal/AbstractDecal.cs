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
                var Tex = Texs.Count == 1 ? Texs.FirstOrDefault() : TextureLayerUtil.BlendTextureUseComputeSheder(null, Texs, BlendType);
                RetDict.Add(Mat, Tex);
            }

            return RetDict;
        }
        protected virtual void SetContainer(List<Texture2D> Texs)
        {
            if (Container == null) { Container = ScriptableObject.CreateInstance<DecalDataContainer>(); Container.name = "DecalDataContainer"; AssetSaveHelper.SaveAsset(Container); }
            Container.DecalCompiledTextures = Texs;
        }

        public override void Apply(AvatarDomain avatarMaterialDomain)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            _IsApply = true;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new AvatarDomain(TargetRenderers);

            var DistMaterials = Utils.GetMaterials(TargetRenderers);
            var DecalTextures = Container.DecalCompiledTextures;
            var DistAndGeneretaTex = new Dictionary<Texture2D, Texture2D>();
            foreach (var Index in Enumerable.Range(0, DecalTextures.Count))
            {
                var DistMat = DistMaterials[Index];
                var DecalTex = DecalTextures[Index];

                if (DistMat == null || DecalTex == null) continue;

                if (DistMat.GetTexture(TargetPropatyName) is Texture2D OldTex)
                {
                    var TexName = $"DecalBlendTexture {DistMat.name}";
                    if (DistAndGeneretaTex.ContainsKey(OldTex))
                    {
                        var OldGenereatetex = DistAndGeneretaTex[OldTex];
                        var MoreBlendsTex = TextureLayerUtil.BlendTextureUseComputeSheder(null, OldGenereatetex, DecalTex, BlendType);
                        MoreBlendsTex.name = TexName;
                        var SavedTex = AssetSaveHelper.SaveAsset(MoreBlendsTex);
                        AssetSaveHelper.DeletAsset(OldGenereatetex);
                        DistAndGeneretaTex[OldTex] = SavedTex;
                    }
                    else
                    {
                        var BlendsTex = TextureLayerUtil.BlendTextureUseComputeSheder(null, OldTex, DecalTex, BlendType);
                        BlendsTex.name = TexName;
                        var SavedTex = AssetSaveHelper.SaveAsset(BlendsTex);
                        DistAndGeneretaTex.Add(OldTex, SavedTex);
                    }
                }
            }

            Container.DecaleBlendTexteres = DistAndGeneretaTex.Values.ToList();

            var NotSavedMats = avatarMaterialDomain.SetTexture(DistAndGeneretaTex);
            Container.GenereatMaterials = MatPea.GeneratMatPeaList(NotSavedMats);
        }

        public override void Revart(AvatarDomain avatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new AvatarDomain(TargetRenderers);

            var MatsDict = MatPea.SwitchingdList(Container.GenereatMaterials);
            avatarMaterialDomain.SetMaterials(MatPea.GeneratMatDict(MatsDict));
        }

        public virtual void ScaleApply() { throw new NotImplementedException(); }

        public void ScaleApply(Vector3 Scale, bool FixedAspect)
        {
            if (DecalTexture != null && FixedAspect)
            {
                transform.localScale = new Vector3(Scale.x, Scale.x * ((float)DecalTexture.height / (float)DecalTexture.width), Scale.z);
            }
            else
            {
                transform.localScale = new Vector3(Scale.x, FixedAspect ? Scale.x : Scale.y, Scale.z);
            }
        }


    }
}



#endif
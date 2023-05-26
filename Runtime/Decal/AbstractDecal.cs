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

        [SerializeField] protected bool _IsAppry = false;
        public override bool IsAppry => _IsAppry;
        public override bool IsPossibleAppry => Container != null;
        public override bool IsPossibleCompile => DecalTexture != null && TargetRenderers.Any(i => i != null);

        public static List<MatAndTex> ZipAndBlendTextures(List<Dictionary<Material, List<Texture2D>>> DictCompiledTextures, BlendType BlendType)
        {
            var ResultTexutres = Utils.ZipToDictionaryOnList(DictCompiledTextures);
            var MatAndTexs = new List<MatAndTex>();
            foreach (var kvp in ResultTexutres)
            {
                var Mat = kvp.Key;
                var Texs = kvp.Value;
                var Tex = TextureLayerUtil.BlendTexturesUseComputeSheder(null, Texs, BlendType);
                MatAndTexs.Add(new MatAndTex(Mat, Tex));
            }

            return MatAndTexs;
        }
        protected virtual void SetContainer(List<MatAndTex> MatAndTexs)
        {
            if (Container == null) { Container = ScriptableObject.CreateInstance<DecalDataContainer>(); AssetSaveHelper.SaveAsset(Container); }
            Container.DecalCompiledTextures = MatAndTexs;
            Container.DistMaterials = MatAndTexs.ConvertAll<Material>(i => i.Material);
        }

        public virtual List<DecalUtil.Filtaring> GetFiltarings() { return null; }

        public override void Appry(MaterialDomain avatarMaterialDomain)
        {
            if (!IsPossibleAppry) return;
            if (_IsAppry) return;
            _IsAppry = true;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new MaterialDomain(TargetRenderers);

            var MatAndTexs = Container.DecalCompiledTextures;
            var GeneretaMatAndTex = new List<MatAndTex>();
            foreach (var MatAndTex in MatAndTexs)
            {
                var Mat = MatAndTex.Material;
                var Tex = MatAndTex.Texture;
                if (Mat.GetTexture(TargetPropatyName) is Texture2D OldTex)
                {
                    var Newtex = TextureLayerUtil.BlendTextureUseComputeSheder(null, OldTex, Tex, BlendType);
                    var SavedTex = AssetSaveHelper.SaveAsset(Newtex);

                    var NewMat = Instantiate<Material>(Mat);
                    NewMat.SetTexture(TargetPropatyName, SavedTex);

                    GeneretaMatAndTex.Add(new MatAndTex(NewMat, SavedTex));
                }
            }

            Container.DecaleBlendTexteres = GeneretaMatAndTex;
            Container.GenereatMaterials = GeneretaMatAndTex.ConvertAll(i => i.Material);

            avatarMaterialDomain.SetMaterials(Container.DistMaterials, Container.GenereatMaterials);
            _IsAppry = true;
        }

        public override void Revart(MaterialDomain avatarMaterialDomain = null)
        {
            if (!_IsAppry) return;
            _IsAppry = false;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new MaterialDomain(TargetRenderers);

            avatarMaterialDomain.SetMaterials(Container.GenereatMaterials, Container.DistMaterials);
        }

        public virtual List<Vector3> ComvartSpace(List<Vector3> varticals)
        {
            return DecalUtil.ConvartVerticesInMatlix(transform.worldToLocalMatrix, varticals, new Vector3(0.5f, 0.5f, 0));
        }

    }
}



#endif
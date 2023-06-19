#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Rs64.TexTransTool.ShaderSupport;

namespace Rs64.TexTransTool.TexturAtlas
{
    public class TexturAtlasDataContenar : TTDataContainer
    {
        [SerializeField] List<Mesh> _DistMeshs = new List<Mesh>();
        [SerializeField] List<Mesh> _GenereatMeshs = new List<Mesh>();


        public List<Mesh> DistMeshs
        {
            set => _DistMeshs = value;
            get => _DistMeshs;
        }
        public List<Mesh> GenereatMeshs
        {
            set
            {
                if (_GenereatMeshs != null) AssetSaveHelper.DeletSubAssets(_GenereatMeshs);
                _GenereatMeshs = value;
                AssetSaveHelper.SaveSubAssets(this, _GenereatMeshs);
            }
            get => _GenereatMeshs;
        }
        public List<int[]> DistMeshsSloats = new List<int[]>();
        public List<PropAndTexture> PropAndTextures = new List<PropAndTexture>();

        private string ThisPath => AssetDatabase.GetAssetPath(this);

        public void SetSubAsset<T>(List<T> Assets) where T : UnityEngine.Object
        {
            AssetSaveHelper.SaveSubAssets(this, Assets);
        }


        public void DeletTexture()
        {
            AssetSaveHelper.DeletAssets(PropAndTextures.ConvertAll(x => x.Texture2D));
            PropAndTextures.Clear();
        }

        public void SetTexture(PropAndTexture Souse)
        {
            PropAndTextures.Add(Souse);
            Souse.Texture2D.name = "AtlasTexture" + Souse.PropertyName;
            PropAndTextures[PropAndTextures.IndexOf(Souse)].Texture2D = AssetSaveHelper.SaveAsset(Souse.Texture2D);
        }
        public void SetTextures(List<PropAndTexture> Souses)
        {
            foreach (var Souse in Souses)
            {
                SetTexture(Souse);
            }
        }

        public List<MatPea> GeneratCompileTexturedMaterial(List<Material> SouseMatrial, bool IsClearUnusedProperties, bool FocuseSetTexture = false)
        {
            List<MatPea> GeneratMats = new List<MatPea>();


            foreach (var SMat in SouseMatrial)
            {

                var Gmat = UnityEngine.Object.Instantiate<Material>(SMat);

                PropToMaterialTexApply(PropAndTextures, Gmat, FocuseSetTexture);

                if (IsClearUnusedProperties) MaterialUtil.RemoveUnusedProperties(Gmat);
                MaterialCustomSetting(Gmat);

                GeneratMats.Add(new MatPea(SMat, Gmat));

            }

            GenereatMaterials = GeneratMats;
            return GeneratMats;
        }
        public MatPea GeneratCompileTexturedMaterial(Material SouseMatrial, bool IsClearUnusedProperties, bool FocuseSetTexture = false)
        {
            var Gmat = UnityEngine.Object.Instantiate<Material>(SouseMatrial);

            PropToMaterialTexApply(PropAndTextures, Gmat, FocuseSetTexture);
            if (IsClearUnusedProperties) MaterialUtil.RemoveUnusedProperties(Gmat);
            MaterialCustomSetting(Gmat);

            var MatPea = new MatPea(SouseMatrial, Gmat);
            GenereatMaterials = new List<MatPea>() { MatPea };
            return MatPea;
        }

        public static void PropToMaterialTexApply(List<PropAndTexture> PropAndTextures, Material TargetMat, bool FocuseSetTexture = false)
        {
            foreach (var propAndTexture in PropAndTextures)
            {
                if (FocuseSetTexture || TargetMat.GetTexture(propAndTexture.PropertyName) is Texture2D)
                {
                    TargetMat.SetTexture(propAndTexture.PropertyName, propAndTexture.Texture2D);
                }
            }
        }

        public static void MaterialCustomSetting(Material material)
        {
            var SuppotShder = ShaderSupportUtil.GetSupprotInstans().Find(i => material.shader.name.Contains(i.SupprotShaderName));
            if (SuppotShder == null) return;
            SuppotShder.GenereatMaterialCustomSetting(material);
        }

        public TexturAtlasDataContenar()
        {

        }

    }
}
#endif
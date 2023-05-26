#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Rs64.TexTransTool.ShaderSupport;

namespace Rs64.TexTransTool.TexturAtlas
{
    public class AtlasSet : TextureTransformer
    {
        public GameObject TargetRoot;
        public List<Renderer> TargetRenderer;//MeshとMaterialの両方を持っているRenderer
        public List<MatSelect> TargetMaterial;
        public bool ForsedMaterialMarge = false;
        public bool UseRefarensMaterial = false;
        public bool FocuseSetTexture = false;
        public Material RefarensMaterial;
        public Vector2Int AtlasTextureSize = new Vector2Int(2048, 2048);
        public float Pading = -10;
        public PadingType PadingType;
        public IslandSortingType SortingType = IslandSortingType.NextFitDecreasingHeight;
        public bool GeneratMatClearUnusedProperties = true;
        [SerializeField] bool _IsAppry;
        public Action<CompileDataContenar> AtlasCompilePostCallBack = (i) => { };
        public CompileDataContenar Contenar;
        [SerializeField] List<Mesh> BackUpMeshs = new List<Mesh>();
        [SerializeField] List<Material> BackUpMaterial = new List<Material>();
        public List<AtlasPostPrcess> PostProcess = new List<AtlasPostPrcess>()
        {
            new AtlasPostPrcess(){
                Process = AtlasPostPrcess.ProcessEnum.SetTextureMaxSize,
                Select = AtlasPostPrcess.TargetSelect.NonPropertys,
                TargetPropatyNames = new List<string>{"_MainTex"}
            },
                        new AtlasPostPrcess(){
                Process = AtlasPostPrcess.ProcessEnum.SetNormalMapSetting,
                TargetPropatyNames = new List<string>{"_BumpMap"}
            }
        };

        public override bool IsAppry => _IsAppry;

        public override bool IsPossibleAppry => Contenar != null;

        public override bool IsPossibleCompile => TargetRoot;

        public MaterialDomain BAckUpMaterialDomain;
        public override void Appry(MaterialDomain AvatarMaterialDomain)
        {
            if (!IsPossibleAppry) return;
            if (_IsAppry == true) return;
            _IsAppry = true;
            if (AvatarMaterialDomain == null) { AvatarMaterialDomain = new MaterialDomain(TargetRenderer); BAckUpMaterialDomain = AvatarMaterialDomain; }
            else { BAckUpMaterialDomain = AvatarMaterialDomain.GetBackUp(); }

            var DistMats = GetSelectMats();
            Contenar.DistMaterial = DistMats;
            if (!ForsedMaterialMarge)
            {
                var GanaretaMat = Contenar.GeneratCompileTexturedMaterial(DistMats, true);

                AvatarMaterialDomain.SetMaterials(DistMats, GanaretaMat);
            }
            else
            {
                Material RefMat;
                if (UseRefarensMaterial && RefarensMaterial != null) RefMat = RefarensMaterial;
                else RefMat = DistMats.First();
                var GenereatMat = Contenar.GeneratCompileTexturedMaterial(RefMat, true, FocuseSetTexture);

                AvatarMaterialDomain.SetMaterials(DistMats, GenereatMat);
            }

            Utils.SetMeshs(TargetRenderer, Contenar.DistMeshs, Contenar.GenereatMeshs);
        }
        public override void Revart(MaterialDomain AvatarMaterialDomain)
        {
            if (!IsAppry) return;
            _IsAppry = false;

            BAckUpMaterialDomain.ResetMaterial();
            BAckUpMaterialDomain = null;

            Utils.SetMeshs(TargetRenderer, Contenar.GenereatMeshs, Contenar.DistMeshs);
        }
        public override void Compile()
        {
            AtlasCompilePostCallBack = (i) => { };
            if (PostProcess.Any())
            {
                foreach (var PostPrces in PostProcess)
                {
                    AtlasCompilePostCallBack += (i) => PostPrces.Processing(i);
                }
            }
            TexturAtlasCompiler.AtlasSetCompile(this);
        }

        public AtlasCompileData GetCompileData()
        {
            AtlasCompileData Data = new AtlasCompileData();
            var SelectMat = GetSelectMats();
            Data.TargetMeshIndex = GetTargetMeshIndexs();
            Data.SetPropatyAndTexs(TargetRenderer, SelectMat, ShaderSupportUtil.GetSupprotInstans());
            Data.DistMesh = Utils.GetMeshes(TargetRenderer);
            Data.meshes = Data.DistMesh.ConvertAll<Mesh>(i => UnityEngine.Object.Instantiate<Mesh>(i));
            Data.AtlasTextureSize = AtlasTextureSize;
            Data.Pading = Pading;
            Data.PadingType = PadingType;
            return Data;
        }

        public List<MeshIndex> GetTargetMeshIndexs()
        {
            var MeshIndexs = new List<MeshIndex>();
            var SelectMat = GetSelectMats();
            int MeshIndex = -1;
            foreach (var Rendera in TargetRenderer)
            {
                MeshIndex += 1;
                int SubMeshIndex = -1;
                foreach (var Mat in Rendera.sharedMaterials)
                {
                    SubMeshIndex += 1;

                    if (SelectMat.Contains(Mat))
                    {
                        MeshIndexs.Add(new MeshIndex(MeshIndex, SubMeshIndex));
                    }
                }
            }
            return MeshIndexs;
        }

        public List<Material> GetSelectMats()
        {
            return TargetMaterial.FindAll(I => I.IsSelect == true).ConvertAll<Material>(I => I.Mat);
        }


    }
    [System.Serializable]
    public class AtlasPostPrcess
    {
        public ProcessEnum Process;
        public TargetSelect Select;
        public List<string> TargetPropatyNames;
        public string ProsesValue;

        public enum ProcessEnum
        {
            SetTextureMaxSize,
            SetNormalMapSetting,
        }
        public enum TargetSelect
        {
            Property,
            NonPropertys,
        }

        public void Processing(CompileDataContenar Target)
        {
            switch (Process)
            {
                case ProcessEnum.SetTextureMaxSize:
                    {
                        ProcessingTextureResize(Target);
                        break;
                    }
                case ProcessEnum.SetNormalMapSetting:
                    {
                        ProcessingSetNormalMapSetting(Target);
                        break;
                    }
            }
        }

        void ProcessingTextureResize(CompileDataContenar Target)
        {
            switch (Select)
            {
                case TargetSelect.Property:
                    {
                        foreach (var PropName in TargetPropatyNames)
                        {
                            var TargetTex = Target.PropAndTextures.Find(i => i.PropertyName == PropName);
                            if (TargetTex != null && int.TryParse(ProsesValue, out var res))
                            {
                                AppryTextureSize(TargetTex.Texture2D, res);
                            }
                        }
                        break;
                    }
                case TargetSelect.NonPropertys:
                    {
                        var TargetList = new List<PropAndTexture>(Target.PropAndTextures);
                        foreach (var PropName in TargetPropatyNames)
                        {
                            TargetList.RemoveAll(i => i.PropertyName == PropName);
                        }
                        if (int.TryParse(ProsesValue, out var res))
                        {
                            foreach (var TargetTex in TargetList)
                            {
                                AppryTextureSize(TargetTex.Texture2D, res);
                            }
                        }
                        break;
                    }
            }

            void AppryTextureSize(Texture2D TargetTexture, int Size)
            {
                var TargetTexPath = AssetDatabase.GetAssetPath(TargetTexture);
                var TextureImporter = AssetImporter.GetAtPath(TargetTexPath) as TextureImporter;
                TextureImporter.maxTextureSize = Size;
                TextureImporter.SaveAndReimport();
            }
        }

        void ProcessingSetNormalMapSetting(CompileDataContenar Target)//これらはあまり多数に対して使用することはないであろうから多数の設定はできないようにする。EditorがわでTargetPropatyNamesをリスト的表示はしないようにする
        {
            var TargetTex = Target.PropAndTextures.Find(i => i.PropertyName == TargetPropatyNames[0]);
            if (TargetTex != null)
            {
                var TargetTexPath = AssetDatabase.GetAssetPath(TargetTex.Texture2D);
                var TextureImporter = AssetImporter.GetAtPath(TargetTexPath) as TextureImporter;
                TextureImporter.textureType = TextureImporterType.NormalMap;
                TextureImporter.SaveAndReimport();
            }
        }
    }
    [Serializable]
    public class MatSelect
    {
        public Material Mat;
        public bool IsSelect;

        public MatSelect(Material mat, bool isSelect)
        {
            Mat = mat;
            IsSelect = isSelect;
        }
    }
}

#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Rs64.TexTransTool.ShaderSupport;
#if VRC_BASE
using VRC.SDKBase;
#endif
namespace Rs64.TexTransTool.TexturAtlas
{
    [AddComponentMenu("TexTransTool/AtlasSet")]
    public class AtlasSet : TextureTransformer
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public GameObject TargetRoot;
        public List<Renderer> TargetRenderer;//MeshとMaterialの両方を持っているRenderer
        public List<MatSelectAndOffset> TargetMaterial;
        public bool ForsedMaterialMarge = false;
        public bool UseRefarensMaterial = false;
        public bool ForseSetTexture = false;
        public Material RefarensMaterial;
        public Vector2Int AtlasTextureSize = new Vector2Int(2048, 2048);
        public float Pading = -10;
        public PadingType PadingType;
        public IslandSortingType SortingType = IslandSortingType.NextFitDecreasingHeight;
        public bool GeneratMatClearUnusedProperties = true;
        [SerializeField] bool _IsApply;
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

        public override bool IsApply => _IsApply;

        public override bool IsPossibleApply => Contenar != null;

        public override bool IsPossibleCompile => TargetRoot;

        public AvatarDomain BAckUpMaterialDomain;

        public bool CompileLook = false;
        public override void Apply(AvatarDomain AvatarMaterialDomain)
        {
            if (!IsPossibleApply) return;
            if (_IsApply == true) return;
            _IsApply = true;
            if (AvatarMaterialDomain == null) { AvatarMaterialDomain = new AvatarDomain(TargetRenderer); BAckUpMaterialDomain = AvatarMaterialDomain; }
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
                var GenereatMat = Contenar.GeneratCompileTexturedMaterial(RefMat, true, ForseSetTexture);

                AvatarMaterialDomain.SetMaterials(DistMats, GenereatMat);
            }

            Utils.SetMeshs(TargetRenderer, Contenar.DistMeshs, Contenar.GenereatMeshs);
        }
        public override void Revart(AvatarDomain AvatarMaterialDomain)
        {
            if (!IsApply) return;
            _IsApply = false;

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
            var TargetRendererNullMeshDeletes = TargetRenderer.Where(i => i.GetMesh() != null).ToList();
            var ItiDict = ConvertSlotIndexAndFiltaling(GetIndexAndSlot(TargetRendererNullMeshDeletes), SelectMat);
            Data.TargetMeshIndex = ItiDict.Keys.ToList();
            Data.Offsets = GetOffsetDict(ItiDict, SelectMat);
            Data.SetPropatyAndTexs(TargetRenderer, SelectMat, ShaderSupportUtil.GetSupprotInstans());
            Data.DistMesh = Utils.GetMeshes(TargetRenderer);
            Data.meshes = Data.DistMesh.ConvertAll<Mesh>(i => UnityEngine.Object.Instantiate<Mesh>(i));
            Data.AtlasTextureSize = AtlasTextureSize;
            Data.Pading = Pading;
            Data.PadingType = PadingType;
            return Data;
        }
        private Dictionary<MeshIndex, int> ConvertSlotIndexAndFiltaling(Dictionary<MeshIndex, Material> IndexAndSlot, List<Material> SelectMats)
        {
            var FiltedDict = new Dictionary<MeshIndex, int>();

            foreach (var kvp in IndexAndSlot)
            {
                if (SelectMats.Contains(kvp.Value))
                {
                    FiltedDict.Add(kvp.Key, SelectMats.IndexOf(kvp.Value));
                }
            }

            return FiltedDict;
        }

        private Dictionary<MeshIndex, Material> GetIndexAndSlot(IEnumerable<Renderer> Renderers)
        {
            var IndexAndSlot = new Dictionary<MeshIndex, Material>();
            int MeshIndex = -1;
            foreach (var Rendera in Renderers)
            {
                MeshIndex += 1;
                int SubMeshIndex = -1;
                foreach (var Mat in Rendera.sharedMaterials)
                {
                    SubMeshIndex += 1;

                    IndexAndSlot.Add(new MeshIndex(MeshIndex, SubMeshIndex), Mat);
                }
            }
            return IndexAndSlot;
        }
        public List<Material> GetSelectMats()
        {
            return TargetMaterial.FindAll(I => I.IsSelect == true).ConvertAll<Material>(I => I.Mat);
        }
        public List<float> GetSelectMatsOffset()
        {
            return TargetMaterial.FindAll(I => I.IsSelect == true).ConvertAll<float>(I => I.Offset);
        }
        public Dictionary<MeshIndex, float> GetOffsetDict(Dictionary<MeshIndex, int> ItIDict, List<Material> Mats)
        {
            var OffsetDict = new Dictionary<MeshIndex, float>();
            var OffSets = GetSelectMatsOffset();
            foreach (var kvp in ItIDict)
            {
                OffsetDict.Add(kvp.Key, OffSets[kvp.Value]);
            }
            return OffsetDict;
        }

        public void AutomaticOffSetSetting()
        {
            var FiltedSelectMats = TargetMaterial.Where(i => i.IsSelect == true).ToList();
            var MaxTexPicelCount = 0;
            if(!FiltedSelectMats.Any()) return;
            foreach (var mat in FiltedSelectMats)
            {
                var MatTex = mat.Mat.mainTexture;
                MaxTexPicelCount = Mathf.Max(MaxTexPicelCount, MatTex.width * MatTex.height);
            }

            foreach (var mat in FiltedSelectMats)
            {
                var MatTex = mat.Mat.mainTexture;
                mat.Offset = (float)(MatTex.width * MatTex.height) / (float)MaxTexPicelCount;
            }
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
                                ApplyTextureSize(TargetTex.Texture2D, res);
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
                                ApplyTextureSize(TargetTex.Texture2D, res);
                            }
                        }
                        break;
                    }
            }

            void ApplyTextureSize(Texture2D TargetTexture, int Size)
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
    public class MatSelectAndOffset
    {
        public Material Mat;
        public bool IsSelect = false;
        public float Offset = 1;

        public MatSelectAndOffset(Material mat, bool isSelect, float offset = 1)
        {
            Mat = mat;
            IsSelect = isSelect;
            Offset = offset;
        }
    }
}

#endif
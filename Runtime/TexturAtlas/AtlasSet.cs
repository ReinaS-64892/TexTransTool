#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Rs64.TexTransTool.TexturAtlas
{
    [AddComponentMenu("TexTransTool/AtlasSet")]
    public class AtlasSet : TextureTransformer
    {
        public AtlasSetObject AtlasSetObject = new AtlasSetObject()
        {
            SortingType = IslandSortingType.NextFitDecreasingHeight,
        };
        public ExecuteClient ClientSelect = ExecuteClient.ComputeSheder;

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

        public override bool IsAppry => AtlasSetObject.IsAppry;

        public override bool IsPossibleAppry => AtlasSetObject.Contenar != null;

        public override bool IsPossibleCompile => AtlasSetObject.AtlasTargetMeshs.Any() || AtlasSetObject.AtlasTargetStaticMeshs.Any();

        public override void Appry()
        {
            AtlasSetObject.Appry();
        }
        public override void Revart()
        {
            AtlasSetObject.Revart();
        }
        public override void Compile()
        {
            AtlasSetObject.AtlasCompilePostCallBack = (i) => { };
            if (PostProcess.Any())
            {
                foreach (var PostPrces in PostProcess)
                {
                    AtlasSetObject.AtlasCompilePostCallBack += (i) => PostPrces.Processing(i);
                }
            }
            TexturAtlasCompiler.AtlasSetCompile(AtlasSetObject, ClientSelect);
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
}

#endif
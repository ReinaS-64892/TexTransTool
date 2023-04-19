#if UNITY_EDITOR
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;

namespace Rs.TexturAtlasCompiler.VRCBulige
{
    public class AtlasSetAvatarTag : MonoBehaviour, IEditorOnly
    {
        public AtlasSet AtlasSet;
        public ExecuteClient ClientSelect;

        public List<AtlasPostPrces> PostPrcess;
    }
    [System.Serializable]
    public class AtlasPostPrces
    {
        public ProcesEnum Proces;
        public TargetSelect Select;
        public string TargetPropatyNames;
        public string ProsesValue;

        public enum ProcesEnum
        {
            SetTextureMaxSize,
        }
        public enum TargetSelect
        {
            Property,
            NonPropertys,
        }

        public void Proses(CompileDataContenar Target)
        {
            switch (Proces)
            {
                case ProcesEnum.SetTextureMaxSize:
                    {
                        ProsesTextureResize(Target);
                        break;
                    }
            }
        }

        void ProsesTextureResize(CompileDataContenar Target)
        {
            switch (Select)
            {
                case TargetSelect.Property:
                    {
                        var TargetTex = Target.PropAndTextures.Find(i => i.PropertyName == TargetPropatyNames);
                        if (TargetTex != null && int.TryParse(ProsesValue, out var res))
                        {
                            AppryTextureSize(TargetTex.Texture2D, res);
                        }
                        break;
                    }
                case TargetSelect.NonPropertys:
                    {
                        var TargetList = new List<PropAndTexture>(Target.PropAndTextures);
                        foreach (var PropName in TargetPropatyNames.Split(' '))
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
    }
}

#endif
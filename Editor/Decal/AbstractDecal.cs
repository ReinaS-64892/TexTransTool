using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public BlendType BlendType = BlendType.Normal;
        public Color Color = Color.white;
        public PropertyName TargetPropertyName = new PropertyName("_MainTex");
        public bool MultiRendererMode = false;
        public float Padding = 0.5f;
        public bool FastMode = true;
        public bool IsSeparateMatAndTexture;

        public virtual TextureWrap GetTextureWarp { get => TextureWrap.NotWrap; }

        [SerializeField] protected bool _IsApply = false;
        public override bool IsApply => _IsApply;


        public override void Apply(AvatarDomain avatarDomain)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            Dictionary<Texture2D, Texture> decalCompiledTextures = CompileDecal();

            if (avatarDomain != null)
            {
                if (!IsSeparateMatAndTexture)
                {
                    foreach (var trp in decalCompiledTextures)
                    {
                        avatarDomain.AddTextureStack(trp.Key, new TextureLayerUtil.BlendTextures(trp.Value, BlendType));
                    }
                }
                else
                {
                    var decalBlendTextures = DecalBlend(decalCompiledTextures, BlendType);
                    var materials = RendererUtility.GetMaterials(TargetRenderers).Distinct();
                    CopyTexDescription(decalBlendTextures);

                    var dictMat = GetDecalTextureSetMaterial(decalBlendTextures, materials, TargetPropertyName);
                    RendererUtility.ChangeMaterialForRenderers(TargetRenderers, dictMat);
                }
            }
            else
            {
                var decalBlendTextures = DecalBlend(decalCompiledTextures, BlendType);
                var materials = RendererUtility.GetMaterials(TargetRenderers).Distinct();
                var dictMat = GetDecalTextureSetMaterial(decalBlendTextures, materials, TargetPropertyName);

                RendererUtility.ChangeMaterialForRenderers(TargetRenderers, dictMat);
                var listMatPea = MatPair.ConvertMatPairList(dictMat);
                localSave = new DecalDataContainer();
                localSave.GenerateMaterials = listMatPea;
                localSave.DecalBlendTextures = decalBlendTextures.Values.ToList();
            }
            _IsApply = true;
        }

        private static void CopyTexDescription(Dictionary<Texture2D, Texture2D> DecalBlendTextures)
        {
            foreach (var dist in DecalBlendTextures.Keys.ToArray())
            {
                DecalBlendTextures[dist] = DecalBlendTextures[dist].CopySetting(DecalBlendTextures[dist]);
            }
        }

        public static Dictionary<Material, Material> GetDecalTextureSetMaterial(Dictionary<Texture2D, Texture2D> DecalsBlendTextures, IEnumerable<Material> Materials, string TargetPropertyName)
        {
            var dictMat = new Dictionary<Material, Material>();
            foreach (var material in Materials)
            {
                if (!material.HasProperty(TargetPropertyName)) { continue; }
                var oldTex = material.GetTexture(TargetPropertyName) as Texture2D;

                if (oldTex == null) continue;
                if (!DecalsBlendTextures.ContainsKey(oldTex)) continue;

                var newMat = UnityEngine.Object.Instantiate(material);

                var NewTex = DecalsBlendTextures[oldTex];
                newMat.SetTexture(TargetPropertyName, NewTex);
                dictMat.Add(material, newMat);
            }

            return dictMat;
        }

        public static Dictionary<Texture2D, Texture2D> DecalBlend(Dictionary<Texture2D, Texture> DecalCompiledTextures, BlendType BlendType)
        {
            var decalBlendTextures = new Dictionary<Texture2D, Texture2D>();
            foreach (var texture in DecalCompiledTextures)
            {
                var blendTexture = TextureLayerUtil.BlendBlit(texture.Key, texture.Value, BlendType).CopyTexture2D();
                blendTexture.Apply();
                decalBlendTextures.Add(texture.Key, blendTexture);
            }

            return decalBlendTextures;
        }

        public abstract Dictionary<Texture2D, Texture> CompileDecal();

        DecalDataContainer localSave;

        public override void Revert(AvatarDomain avatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;

            if (avatarMaterialDomain != null)
            {
                //何もすることはない。
            }
            else
            {
                var revertList = MatPair.ConvertMatDict(MatPair.SwitchingList(localSave.GenerateMaterials));
                RendererUtility.ChangeMaterialForRenderers(TargetRenderers, revertList);
                localSave = null;
            }
            IsSelfCallApply = false;

        }

        [ContextMenu("ExtractDecalCompiledTexture")]
        public void ExtractDecalCompiledTexture()
        {
            if (!IsPossibleApply) { Debug.LogError("Applyできないためデカールをコンパイルできません。"); return; }


            var path = EditorUtility.OpenFolderPanel("ExtractDecalCompiledTexture", "Assets", "");
            if (string.IsNullOrEmpty(path) && !Directory.Exists(path)) return;

            var decalCompiledTextures = CompileDecal();
            foreach (var TexturePair in decalCompiledTextures)
            {
                var name = TexturePair.Key.name;
                Texture2D extractDCtex;
                switch (TexturePair.Value)
                {
                    case RenderTexture rt:
                        extractDCtex = rt.CopyTexture2D();
                        break;
                    case Texture2D tex:
                        extractDCtex = tex;
                        break;
                    default:
                        continue;
                }
                var pngByte = extractDCtex.EncodeToPNG();

                System.IO.File.WriteAllBytes(Path.Combine(path, name + ".png"), pngByte);

            }
        }

    }
}
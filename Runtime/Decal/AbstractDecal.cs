#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public BlendType BlendType = BlendType.Normal;
        public Color Color = Color.white;
        public PropertyName TargetPropertyName = new PropertyName("_MainTex");
        public bool MultiRendereMode = false;
        public float Pading = 0.5f;
        public bool FastMode = true;
        public bool IsSeparateMatAndTexture;

        public virtual Vector2? GetOutRangeTexture { get => Vector2.zero; }

        [SerializeField] protected bool _IsApply = false;
        public override bool IsApply => _IsApply;


        public override void Apply(AvatarDomain avatarDomain)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            Dictionary<Texture2D, Texture> DecalCompiledTextures = CompileDecal();

            if (avatarDomain != null)
            {
                if (!IsSeparateMatAndTexture)
                {
                    foreach (var trp in DecalCompiledTextures)
                    {
                        avatarDomain.AddTextureStack(trp.Key, new TextureLayerUtil.BlendTextures(trp.Value, BlendType));
                    }
                }
                else
                {
                    var DecaleBlendTexteres = DecalBlend(DecalCompiledTextures, BlendType);
                    var Materials = Utils.GetMaterials(TargetRenderers).Distinct();
                    CopyTexdiscription(DecaleBlendTexteres);

                    var DictMat = TextureSwapdMaterial(DecaleBlendTexteres, Materials, TargetPropertyName);
                    Utils.ChangeMaterialsRendereas(TargetRenderers, DictMat);
                }
            }
            else
            {
                var DecaleBlendTexteres = DecalBlend(DecalCompiledTextures, BlendType);
                var Materials = Utils.GetMaterials(TargetRenderers).Distinct();
                var DictMat = TextureSwapdMaterial(DecaleBlendTexteres, Materials, TargetPropertyName);

                Utils.ChangeMaterialsRendereas(TargetRenderers, DictMat);
                var ListMatpe = MatPair.GeneratMatPairList(DictMat);
                LocalSave = new DecalDataContainer();
                LocalSave.GenerateMaterials = ListMatpe;
                LocalSave.DecaleBlendTexteres = DecaleBlendTexteres.Values.ToList();
            }
            _IsApply = true;
        }

        private static void CopyTexdiscription(Dictionary<Texture2D, Texture2D> DecaleBlendTexteres)
        {
            foreach (var Dist in DecaleBlendTexteres.Keys.ToArray())
            {
                DecaleBlendTexteres[Dist] = DecaleBlendTexteres[Dist].CopySetting(DecaleBlendTexteres[Dist]);
            }
        }

        public static Dictionary<Material, Material> TextureSwapdMaterial(Dictionary<Texture2D, Texture2D> DecaleBlendTexteres, IEnumerable<Material> Materials, string TargetPropertyName)
        {
            var DictMat = new Dictionary<Material, Material>();
            foreach (var Material in Materials)
            {
                if (!Material.HasProperty(TargetPropertyName)) { continue; }
                var OldTex = Material.GetTexture(TargetPropertyName) as Texture2D;

                if (OldTex == null) continue;
                if (!DecaleBlendTexteres.ContainsKey(OldTex)) continue;

                var NewMat = UnityEngine.Object.Instantiate(Material);

                var NewTex = DecaleBlendTexteres[OldTex];
                NewMat.SetTexture(TargetPropertyName, NewTex);
                DictMat.Add(Material, NewMat);
            }

            return DictMat;
        }

        public static Dictionary<Texture2D, Texture2D> DecalBlend(Dictionary<Texture2D, Texture> DecalCompiledTextures, BlendType BlendType)
        {
            var DecaleBlendTexteres = new Dictionary<Texture2D, Texture2D>();
            foreach (var Texture in DecalCompiledTextures)
            {
                var BlendTexture = TextureLayerUtil.BlendBlit(Texture.Key, Texture.Value, BlendType).CopyTexture2D();
                BlendTexture.Apply();
                DecaleBlendTexteres.Add(Texture.Key, BlendTexture);
            }

            return DecaleBlendTexteres;
        }

        public abstract Dictionary<Texture2D, Texture> CompileDecal();

        DecalDataContainer LocalSave;

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
                var RevarList = MatPair.GeneratMatDict(MatPair.SwitchingList(LocalSave.GenerateMaterials));
                Utils.ChangeMaterialsRendereas(TargetRenderers, RevarList);
                LocalSave = null;
            }
            IsSelfCallApply = false;

        }

        [ContextMenu("ExtractDecalCompiledTexture")]
        public void ExtractDecalCompiledTexture()
        {
            if(!IsPossibleApply) {Debug.LogError("Applyできないためデカールをコンパイルできません。");return;}


            var path = EditorUtility.OpenFolderPanel("ExtractDecalCompiledTexture", "Assets", "");
            if (string.IsNullOrEmpty(path) && !Directory.Exists(path)) return;

            var DecalCompiledTextures = CompileDecal();
            foreach (var Texturepea in DecalCompiledTextures)
            {
                var Name = Texturepea.Key.name;
                Texture2D extractDCtex;
                switch (Texturepea.Value)
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
                var PngByte = extractDCtex.EncodeToPNG();

                System.IO.File.WriteAllBytes(Path.Combine(path, Name + ".png"), PngByte);

            }
        }

    }
}



#endif

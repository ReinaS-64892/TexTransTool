#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Rs64.TexTransTool.Decal
{
    public abstract class AbstractDecal<SpaseConverter> : TextureTransformer
    where SpaseConverter : DecalUtil.IConvertSpace
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public Texture2D DecalTexture;
        public BlendType BlendType = BlendType.Normal;
        public string TargetPropatyName = "_MainTex";
        public bool MultiRendereMode = false;
        public float DefaultPading = 0.5f;

        public abstract SpaseConverter GetSpaseConverter { get; }
        public abstract DecalUtil.ITraiangleFilter<SpaseConverter> GetTraiangleFilter { get; }
        public virtual Vector2? GetOutRengeTexture { get => Vector2.zero; }

        [SerializeField] protected bool _IsApply = false;
        public override bool IsApply => _IsApply;
        public override bool IsPossibleApply => IsPossibleCompile;
        public override bool IsPossibleCompile => DecalTexture != null && TargetRenderers.Any(i => i != null);


        public override void Apply(AvatarDomain avatarDomain)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            Dictionary<Texture2D, RenderTexture> DecalCompiledTextures = CompileDecal();

            if (avatarDomain != null)
            {
                foreach (var trp in DecalCompiledTextures)
                {
                    avatarDomain.AddTextureStack(trp.Key, new TextureLayerUtil.BlendRenderTarget(trp.Value, BlendType));
                }
            }
            else
            {
                var DecaleBlendTexteres = new Dictionary<Texture2D, Texture2D>();
                foreach (var Texture in DecalCompiledTextures)
                {
                    var BlendTexture = TextureLayerUtil.BlendBlit(Texture.Key, Texture.Value, BlendType).CopyTexture2D();
                    BlendTexture.Apply();
                    DecaleBlendTexteres.Add(Texture.Key, BlendTexture);
                }

                var Materials = Utils.GetMaterials(TargetRenderers).Distinct();
                var DictMat = new Dictionary<Material, Material>();
                foreach (var Material in Materials)
                {
                    var OldTex = Material.GetTexture(TargetPropatyName) as Texture2D;

                    if (OldTex == null) continue;
                    if (!DecaleBlendTexteres.ContainsKey(OldTex)) continue;

                    var NewMat = UnityEngine.Object.Instantiate(Material);

                    var NewTex = DecaleBlendTexteres[OldTex];
                    NewMat.SetTexture(TargetPropatyName, NewTex);
                    DictMat.Add(Material, NewMat);
                }
                Utils.ChangeMaterials(TargetRenderers, DictMat);
                var ListMatpe = MatPea.GeneratMatPeaList(DictMat);
                LocalSave = new DecalDataContainer();
                LocalSave.GenereatMaterials = ListMatpe;
                LocalSave.DecaleBlendTexteres = DecaleBlendTexteres.Values.ToList();
            }
            _IsApply = true;
        }

        public virtual Dictionary<Texture2D, RenderTexture> CompileDecal()
        {
            var DecalCompiledTextures = new Dictionary<Texture2D, RenderTexture>();
            foreach (var Rendarer in TargetRenderers)
            {
                DecalUtil.CreatDecalTexture(
                    Rendarer,
                    DecalCompiledTextures,
                    DecalTexture,
                    GetSpaseConverter,
                    GetTraiangleFilter,
                    TargetPropatyName,
                    GetOutRengeTexture,
                    DefaultPading
                );
            }

            return DecalCompiledTextures;
        }

        DecalDataContainer LocalSave;

        public override void Revart(AvatarDomain avatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;

            if (avatarMaterialDomain != null)
            {
                //何もすることはない。
            }
            else
            {
                var RevarList = MatPea.GeneratMatDict(MatPea.SwitchingdList(LocalSave.GenereatMaterials));
                Utils.ChangeMaterials(TargetRenderers, RevarList);
                LocalSave = null;
            }
            IsSelfCallApply = false;

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
                transform.localScale = Scale;
            }
        }
    }
}



#endif
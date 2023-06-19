#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace Rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TextureBlender")]
    public class TextureBlender : TextureTransformer
    {
        public Renderer TargetRenderer;
        public int MaterialSelect = 0;
        public Texture2D BlendTexture;
        public BlendType BlendType = BlendType.Normal;
        public string TargetPropatyName = "_MainTex";

        public TextureBlenderDataContainer Container;

        [SerializeField] protected bool _IsApply = false;
        public override bool IsApply => _IsApply;

        public override bool IsPossibleApply => TargetRenderer != null && BlendTexture != null;

        public override bool IsPossibleCompile => IsPossibleApply;

        public override void Apply(AvatarDomain avatarMaterialDomain = null)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            _IsApply = true;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new AvatarDomain(new List<Renderer> { TargetRenderer });
            ContainerCheck();

            var DistMaterials = TargetRenderer.sharedMaterials;

            if (DistMaterials.Length <= MaterialSelect) return;

            var DistMat = DistMaterials[MaterialSelect];

            var DistTex = DistMat.GetTexture(TargetPropatyName) as Texture2D;
            var AddTex = BlendTexture;
            if (DistTex == null) return;


            var DistSize = DistTex.NativeSize();
            if (DistSize != AddTex.NativeSize())
            {
                Compiler.NotFIlterAndReadWritTexture2D(ref AddTex);
                AddTex = TextureLayerUtil.ResizeTexture(AddTex, DistSize);
            }

            var Newtex = TextureLayerUtil.BlendTextureUseComputeSheder(null, DistTex, AddTex, BlendType);
            var SavedTex = AssetSaveHelper.SaveAsset(Newtex);


            Container.BlendTexteres = SavedTex;

            var ChangeDict = avatarMaterialDomain.SetTexture(DistTex, SavedTex);
            Container.GenereatMaterials = MatPea.GeneratMatPeaList(ChangeDict);
        }

        public override void Compile() { }

        public override void Revart(AvatarDomain avatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new AvatarDomain(new List<Renderer> { TargetRenderer });

            avatarMaterialDomain.SetMaterials(MatPea.SwitchingdList(Container.GenereatMaterials));
        }

        protected virtual void ContainerCheck()
        {
            if (Container == null) { Container = ScriptableObject.CreateInstance<TextureBlenderDataContainer>(); AssetSaveHelper.SaveAsset(Container); }
        }
    }

    public class TextureBlenderDataContainer : TTDataContainer
    {
        [SerializeField] Texture2D _BlendTexteres;

        public Texture2D BlendTexteres
        {
            set
            {
                if (_BlendTexteres != null) AssetSaveHelper.DeletAsset(_BlendTexteres);
                _BlendTexteres = value;
            }
            get => _BlendTexteres;
        }

    }
}
#endif
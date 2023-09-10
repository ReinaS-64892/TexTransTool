#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.TextureAtlas;

namespace net.rs64.TexTransTool.Deprecated.V0.TextureAtlas
{

    public class AtlasTexture : TextureTransformer
    {
        public GameObject TargetRoot;
        public List<Material> SelectReferenceMat;//OrderedHashSetにしたかったけどシリアライズの都合で
        public List<MatSelector> MatSelectors = new List<MatSelector>();
        public List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        public bool UseIslandCache = true;

        [SerializeField] bool _isApply = false;
        public override bool IsPossibleApply => TargetRoot != null && AtlasSettings.Count > 0;

        public override List<Renderer> GetRenderers => throw new NotImplementedException();

        public override bool IsApply { get => _isApply; set => _isApply = value; }

        public AvatarDomain RevertDomain;
        public List<MeshPair> RevertMeshes;

        public override void Apply(IDomain avatarMaterialDomain = null)
        {
        }

    }
    [Serializable]
    public class MatSelector
    {
        public Material Material;
        public bool IsTarget = false;
        public int AtlasChannel = 0;
        public float TextureSizeOffSet = 1;
    }
}
#endif

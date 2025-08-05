#nullable enable
using nadena.dev.ndmf;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using net.rs64.TexTransTool.NDMF.AdditionalMaterials;

namespace net.rs64.TexTransTool.NDMF
{
    internal class NDMFDomain : AvatarDomain, IDomainReferenceViewer
    {
        private class NDMFAssetSaver : IAssetSaver
        {
            private readonly BuildContext _buildContext;

            public NDMFAssetSaver(BuildContext buildContext)
            {
                _buildContext = buildContext;
            }

            public void SaveAsset(Object asset)
            {
                if (asset == null || AssetDatabase.Contains(asset)) return;
                _buildContext.AssetSaver.SaveAsset(asset);
            }
            public bool IsTemporaryAsset(UnityEngine.Object asset)
            {
                return _buildContext.AssetSaver.IsTemporaryAsset(asset);
            }
        }

        private readonly AdditionalMaterialsProvider _additionalMaterialsProvider;
        public NDMFDomain(BuildContext b) : base(b.AvatarRootObject, new NDMFAssetSaver(b))
        {
            _additionalMaterialsProvider = new AdditionalMaterialsProvider(b);
        }
        public NDMFDomain(BuildContext b, ITexTransUnityDiskUtil diskUtil, ITexTransToolForUnity ttt4u) : base(b.AvatarRootObject, new NDMFAssetSaver(b), diskUtil, ttt4u)
        {
            _additionalMaterialsProvider = new AdditionalMaterialsProvider(b);
        }

        public HashSet<Material> GetAllMaterials()
        {
            var matHash = new HashSet<Material>();

            foreach (var r in EnumerateRenderers()) { matHash.UnionWith(GetMaterials(r).SkipDestroyed());}
            matHash.UnionWith(_additionalMaterialsProvider.GetReferencedMaterials());
            return matHash;

            Material?[] GetMaterials(Renderer renderer) => ((IDomainReferenceViewer)this).GetMaterials(renderer);
        }
        public override void ReplaceMaterials(Dictionary<Material, Material> mapping)
        {
            base.ReplaceMaterials(mapping);
            _additionalMaterialsProvider.ReplaceReferencedMaterials(mapping);
        }
        public override void RegisterReplacement(Object oldObject, Object nowObject)
        {
            if (_genericReplaceRegistry.ReplaceMap.TryGetValue(nowObject, out var dictOld)) { if (dictOld == oldObject) { return; } }

            base.RegisterReplacement(oldObject, nowObject);
            if (oldObject is not RenderTexture && nowObject is not RenderTexture) ObjectRegistry.RegisterReplacedObject(oldObject, nowObject);
        }
        public override bool OriginalObjectEquals(Object? l, Object? r) { return ObjectRegistry.GetReference(l) == ObjectRegistry.GetReference(r); }

        public override bool IsActive(GameObject gameObject)
        {
            // TODO : Animation parse
            return base.IsActive(gameObject);
        }
        public override bool IsEnable(Component component)
        {
            // TODO : Animation parse
            return base.IsEnable(component);
        }
    }
}

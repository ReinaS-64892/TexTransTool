#nullable enable
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace net.rs64.TexTransTool.NDMF
{
    internal class NDMFDomain : AvatarDomain, IRendererTargeting
    {
        private readonly AnimatorServicesContext _animatorServicesContext;
        private class NDMFAssetSaver : IAssetSaver
        {
            private readonly BuildContext _buildContext;

            public NDMFAssetSaver(BuildContext buildContext)
            {
                _buildContext = buildContext;
            }

            public void TransferAsset(Object asset)
            {
                if (asset == null || AssetDatabase.Contains(asset)) return;
                _buildContext.AssetSaver.SaveAsset(asset);
            }
            public bool IsTemporaryAsset(UnityEngine.Object asset)
            {
                return _buildContext.AssetSaver.IsTemporaryAsset(asset);
            }
        }

        public NDMFDomain(BuildContext b) : base(b.AvatarRootObject, new NDMFAssetSaver(b))
        {
            _animatorServicesContext = b.Extension<AnimatorServicesContext>();
        }
        public NDMFDomain(BuildContext b, ITexTransUnityDiskUtil diskUtil, ITexTransToolForUnity ttt4u) : base(b.AvatarRootObject, new NDMFAssetSaver(b), diskUtil, ttt4u)
        {
            _animatorServicesContext = b.Extension<AnimatorServicesContext>();
        }

        public HashSet<Material> GetAllMaterials()
        {
            var matHash = new HashSet<Material>();
            foreach (var r in EnumerateRenderer()) { matHash.UnionWith(GetMaterials(r).Where(m => m != null).Cast<Material>()); }

            var animatedMaterials = _animatorServicesContext.AnimationIndex
                .GetPPtrReferencedObjects
                .OfType<Material>();
            matHash.UnionWith(animatedMaterials);
            return matHash;

            Material?[] GetMaterials(Renderer renderer) => ((IRendererTargeting)this).GetMaterials(renderer);
        }
        public override void ReplaceMaterials(Dictionary<Material, Material> mapping)
        {
            base.ReplaceMaterials(mapping);
            _animatorServicesContext.AnimationIndex.RewriteObjectCurves(obj => {
                if (obj is Material oldMat && mapping.TryGetValue(oldMat, out var newMat)) {
                    return newMat;
                }
                return obj;
            });
        }
        public override void RegisterReplace(Object oldObject, Object nowObject)
        {
            if (_genericReplaceRegistry.ReplaceMap.TryGetValue(nowObject, out var dictOld)) { if (dictOld == oldObject) { return; } }

            base.RegisterReplace(oldObject, nowObject);
            if (oldObject is not RenderTexture && nowObject is not RenderTexture) ObjectRegistry.RegisterReplacedObject(oldObject, nowObject);
        }
        public override bool OriginEqual(Object? l, Object? r) { return ObjectRegistry.GetReference(l) == ObjectRegistry.GetReference(r); }

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

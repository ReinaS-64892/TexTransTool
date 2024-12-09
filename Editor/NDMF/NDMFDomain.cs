using nadena.dev.ndmf;
using System;
using UnityEditor;
using Object = UnityEngine.Object;


namespace net.rs64.TexTransTool.NDMF
{
    internal class NDMFDomain : AvatarDomain
    {
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

#if NDMF_1_6_0_OR_NEWER
                _buildContext.AssetSaver.SaveAsset(asset);
#else
                AssetDatabase.AddObjectToAsset(asset, _buildContext.AssetContainer);
#endif
            }
            public bool IsTemporaryAsset(UnityEngine.Object asset)
            {
#if NDMF_1_6_0_OR_NEWER
                return _buildContext.AssetSaver.IsTemporaryAsset(asset);
#else
                return _buildContext.IsTemporaryAsset(asset);
#endif
            }
        }

        public NDMFDomain(BuildContext b) : base(b.AvatarRootObject, false, new NDMFAssetSaver(b)) { }

        public override void RegisterReplace(Object oldObject, Object nowObject)
        {
            if (_replaceMap.TryGetValue(nowObject, out var dictOld)) { if (dictOld == oldObject) { return; } }

            base.RegisterReplace(oldObject, nowObject);
            ObjectRegistry.RegisterReplacedObject(oldObject, nowObject);
        }
        public override bool OriginEqual(Object l, Object r) { return ObjectRegistry.GetReference(l) == ObjectRegistry.GetReference(r); }

    }
}

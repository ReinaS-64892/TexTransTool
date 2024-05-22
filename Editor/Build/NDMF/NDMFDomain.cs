using nadena.dev.ndmf;
using System;
using Object = UnityEngine.Object;


namespace net.rs64.TexTransTool.Build.NDMF
{
    internal class NDMFDomain : AvatarDomain
    {
        public NDMFDomain(BuildContext b) : base(b.AvatarRootObject, false, new AssetSaver(b.AssetContainer)) { }

        public override void RegisterReplace(Object oldObject, Object nowObject)
        {
            if (_replaceMap.TryGetValue(nowObject, out var dictOld)) { if (dictOld == oldObject) { return; } }

            base.RegisterReplace(oldObject, nowObject);
            ObjectRegistry.RegisterReplacedObject(oldObject, nowObject);
        }
        public override bool OriginEqual(Object l, Object r) { return ObjectRegistry.GetReference(l) == ObjectRegistry.GetReference(r); }

    }
}

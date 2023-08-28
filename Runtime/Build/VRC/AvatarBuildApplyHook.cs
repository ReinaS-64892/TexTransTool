#if (UNITY_EDITOR && VRC_BASE)
using UnityEngine;
using System.Collections.Generic;
using VRC.SDKBase;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace net.rs64.TexTransTool.Build.VRC
{
    [AddComponentMenu("TexTransTool/AvatarBuildApplyHook"), RequireComponent(typeof(AbstractTexTransGroup)), DefaultExecutionOrder(-11024)]
    public class AvatarBuildApplyHook : AvatarDomainDefinition, IEditorOnly
    {

        public override void Apply(UnityEngine.Object OverrideAssetContainer = null)
        {
            SetAvatar(null);
            base.Apply();
        }
        public void Apply(GameObject avatar)
        {
            SetAvatar(avatar);
            base.Apply();
        }
        public override void SetAvatar(GameObject Setavatar)
        {
            if (Setavatar == null)
            {
                var VRCAvatar = FindAvatarInParents(transform);
                if (VRCAvatar == null) return;
                else Avatar = VRCAvatar.gameObject;
            }
            else
            {
                Avatar = Setavatar;
            }
        }

        private void Awake()
        {
            AssetSaveHelper.IsTmplaly = true;
            if (TexTransGroup.IsApply) return;
            Apply(null);
        }
        private void OnDestroy()
        {
            AssetSaveHelper.IsTmplaly = false;
        }
        private void Start()
        {
            if (TexTransGroup.IsApply) return;
            Apply(null);
        }

        //https://github.com/bdunderscore/modular-avatar/blob/5ad6b58c7ffb1f809ed1b585989b8cad65002563/Packages/nadena.dev.modular-avatar/Runtime/RuntimeUtil.cs
        // Originally under MIT License
        // Copyright (c) 2022 bd_
        public static VRCAvatarDescriptor FindAvatarInParents(Transform target)
        {
            while (target != null)
            {
                var av = target.GetComponent<VRCAvatarDescriptor>();
                if (av != null) return av;
                target = target.parent;
            }

            return null;
        }
    }
}
#endif

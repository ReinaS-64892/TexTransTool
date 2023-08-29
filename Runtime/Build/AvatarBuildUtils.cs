#if UNITY_EDITOR
using System;
using UnityEngine;

namespace net.rs64.TexTransTool.Build
{
    public static class AvatarBuildUtils
    {

        public static bool ProcessAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool UseTemp = false)
        {
            try
            {
                if (OverrideAssetContainer == null && UseTemp) { AssetSaveHelper.IsTmplaly = true; }
                var aDDs = avatarGameObject.GetComponentsInChildren<AvatarDomainDefinition>();
                foreach (var aDD in aDDs)
                {
                    aDD.Apply(avatarGameObject, OverrideAssetContainer);
                }
                foreach (var aDD in aDDs) { RemoveAvatarDomainDefinition(aDD); }
                foreach (var tf in avatarGameObject.GetComponentsInChildren<ITexTransToolTag>(true)) { if (tf is MonoBehaviour mb && mb != null) MonoBehaviour.DestroyImmediate(mb.gameObject); }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        public static void RemoveAvatarDomainDefinition(AvatarDomainDefinition avatarDomainDefinition)
        {
            foreach (var tf in avatarDomainDefinition.TexTransGroup.Targets)
            {
                switch (tf)
                {
                    case AbstractTexTransGroup abstractTexTransGroup:
                        RemoveTexTransGroup(abstractTexTransGroup);
                        break;
                }
                MonoBehaviour.DestroyImmediate(tf.gameObject);
            }
            MonoBehaviour.DestroyImmediate(avatarDomainDefinition.gameObject);
        }
        public static void RemoveTexTransGroup(AbstractTexTransGroup texTransGroup)
        {
            foreach (var tf in texTransGroup.Targets)
            {
                switch (tf)
                {
                    case AbstractTexTransGroup abstractTexTransGroup:
                        RemoveTexTransGroup(abstractTexTransGroup);
                        break;

                }
                MonoBehaviour.DestroyImmediate(tf.gameObject);
            }
            MonoBehaviour.DestroyImmediate(texTransGroup.gameObject);
        }
    }
}
#endif

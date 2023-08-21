#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Rs64.TexTransTool.Bulige
{
    public static class AvatarBuligeUtili
    {

        public static bool ProcesAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool UseTemp = false)
        {
            try
            {
                if (OverrideAssetContainer == null && UseTemp) { AssetSaveHelper.IsTmplaly = true; }
                var aDDs = avatarGameObject.GetComponentsInChildren<AvatarDomainDefinition>();
                foreach (var aDD in aDDs)
                {
                    aDD.SetAvatar(avatarGameObject);
                    aDD.Apply(OverrideAssetContainer);
                }
                foreach (var aDD in aDDs) { RemoveAvatarDomainDefinition(aDD); }
                foreach (var tf in avatarGameObject.GetComponentsInChildren<TextureTransformer>(true)) { MonoBehaviour.DestroyImmediate(tf); }
                foreach (var tf in avatarGameObject.GetComponentsInChildren<TexTransToolTag>(true)) { MonoBehaviour.DestroyImmediate(tf); }
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
                MonoBehaviour.DestroyImmediate(tf);
            }
            MonoBehaviour.DestroyImmediate(avatarDomainDefinition);
            MonoBehaviour.DestroyImmediate(avatarDomainDefinition.TexTransGroup);
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
                MonoBehaviour.DestroyImmediate(tf);
            }
            MonoBehaviour.DestroyImmediate(texTransGroup);
        }
    }
}
#endif
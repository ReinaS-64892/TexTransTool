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
                var ADD = avatarGameObject.GetComponentsInChildren<AvatarDomainDefinition>();
                foreach (var ABAH in ADD)
                {
                    ABAH.SetAvatar(avatarGameObject);
                    ABAH.Apply(OverrideAssetContainer);
                    MonoBehaviour.DestroyImmediate(ABAH);
                }
                foreach (var TT in avatarGameObject.GetComponentsInChildren<TextureTransformer>(true)) { MonoBehaviour.DestroyImmediate(TT); }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}
#endif
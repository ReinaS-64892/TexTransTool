#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Rs64.TexTransTool.Bulige
{
    public static class AvatarBuligeUtili
    {

        public static bool ProcesAvatar(GameObject avatarGameObject)
        {
            try
            {
                AssetSaveHelper.IsTmplaly = true;
                var AvatarBuildApplyHooks = avatarGameObject.GetComponentsInChildren<AvatarDomainDefinition>();
                foreach (var ABAH in AvatarBuildApplyHooks)
                {
                    ABAH.SetAvatar(avatarGameObject);
                    ABAH.Apply();
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
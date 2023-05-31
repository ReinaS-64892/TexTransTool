#if (UNITY_EDITOR && VRC_BASE)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using System.Linq;
using Rs64.TexTransTool.TexturAtlas;

namespace Rs64.TexTransTool.VRCBulige
{
    [InitializeOnLoad]
    public class TTGAppryToVRCAvatarCallBack : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => -2048;//この値についてはもうすこし考えるべきだが -1024で IEditorOnlyは消滅するらしい。

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
                var AvatarBuildAppryHooks = avatarGameObject.GetComponentsInChildren<AvatarBuildAppryHook>();
                foreach (var ABAH in AvatarBuildAppryHooks)
                {
                    ABAH.Appry(avatarGameObject);
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
        public void OnPostprocessAvatar()
        {
        }

    }
}
#endif
#if (UNITY_EDITOR && VRC_BASE)
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
                var AvatarBuildAppryHooks = avatarGameObject.GetComponentsInChildren<AvatarBuildAppryHook>();
                var MaterialDomain = new MaterialDomain(avatarGameObject.GetComponentsInChildren<Renderer>(true).ToList());
                foreach (var ABAH in AvatarBuildAppryHooks)
                {
                    ABAH.TexTransGroup.Appry(MaterialDomain);
                    MonoBehaviour.DestroyImmediate(ABAH);
                }
                return true;
        }
        public void OnPostprocessAvatar()
        {
        }

    }
}
#endif
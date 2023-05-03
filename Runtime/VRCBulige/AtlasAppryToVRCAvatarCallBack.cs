#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using System.Linq;

namespace Rs64.TexTransTool.VRCBulige
{
    [InitializeOnLoad]
    public class AtlasAppryToVRCAvatarCallBack : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => -2048;//この値についてはもうすこし考えるべきだが-30だとコンポーネントが消滅していた

        public void OnPostprocessAvatar()
        {
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var AtlasSetAvatarTags = avatarGameObject.GetComponentsInChildren<AtlasSetAvatarTag>(true);
            foreach (var AtlasSetAvatarTag in AtlasSetAvatarTags)
            {
                AtlasSetAvatarTag.AtlasSet.Appry();
                MonoBehaviour.DestroyImmediate(AtlasSetAvatarTag);
            }
            return true;

        }
    }
}
#endif
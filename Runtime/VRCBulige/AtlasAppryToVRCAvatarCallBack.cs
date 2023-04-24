#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using System.Linq;

namespace Rs.TexturAtlasCompiler.VRCBulige
{
    [InitializeOnLoad]
    public class AtlasAppryToVRCAvatarCallBack : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => -9999;//この値についてはもうすこし考えるべきだが-30だとコンポーネントが消滅していた

        public void OnPostprocessAvatar()
        {
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
           // PrefabUtility.SaveAsPrefabAsset(avatarGameObject,"Assets/test.prefab");
           // Debug.Log(avatarGameObject.name);
            var AtlasSetAvatarTags = avatarGameObject.GetComponentsInChildren<AtlasSetAvatarTag>(true);
           // Debug.Log(AtlasSetAvatarTags.Length);
            foreach (var AtlasSetAvatarTag in AtlasSetAvatarTags)
            {
                //Debug.Log(AtlasSetAvatarTag.gameObject.name);
                AtlasSetAvatarTag.AtlasSet.Appry();
                MonoBehaviour.DestroyImmediate(AtlasSetAvatarTag);
            }
            return true;

        }
    }
}
#endif
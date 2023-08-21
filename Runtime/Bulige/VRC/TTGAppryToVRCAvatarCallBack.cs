#if (UNITY_EDITOR && VRC_BASE)
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Rs64.TexTransTool.Bulige.VRC
{

    [InitializeOnLoad]
    public class TTGApplyToVRCAvatarCallBack : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => -2048;//この値についてはもうすこし考えるべきだが -1024で IEditorOnlyは消滅するらしい。

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            return AvatarBuligeUtili.ProcesAvatar(avatarGameObject, null, true);

        }
        public void OnPostprocessAvatar()
        {
            AssetSaveHelper.IsTmplaly = false;
        }

    }
}
#endif
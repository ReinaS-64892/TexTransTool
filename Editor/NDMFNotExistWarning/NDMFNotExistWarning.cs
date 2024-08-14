using UnityEditor;
using UnityEngine;

static class NDMFNotExistWarning
{
#if VRC_AVATAR
#if !NDMF_DEPEND_VERSION
    [InitializeOnLoadMethod]
    static void Call()
    {
#if NDMF
        Debug.LogWarning("TexTransTool の対応している NDMF バージョンではありません！ NDMF をアップデートしてください！！！");
#else
        Debug.LogWarning("NDMF が環境に存在しません！！！ VRChat Avatar 用途では NDMF は必須なのでご注意ください！");
#endif
    }
#endif
#endif
}

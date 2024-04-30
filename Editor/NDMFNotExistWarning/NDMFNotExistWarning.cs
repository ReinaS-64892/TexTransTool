using UnityEditor;
using UnityEngine;

static class NDMFNotExistWarning
{
#if ぶいちゃあばたー && !なでもふっっ
    [InitializeOnLoadMethod]
    static void Call()
    {
        Debug.LogWarning("NDMF が環境に存在しません！！！ VRChat Avatar 用途では NDMF は必須なのでご注意ください！");
    }
#endif
}

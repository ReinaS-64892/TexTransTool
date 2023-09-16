#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Build
{
    public static class ManualBake
    {
        [MenuItem("Tools/TexTransTool/Manual Bake Avatar")]
        public static void ManualBakeSelected()
        {
            var targetAvatar = Selection.activeGameObject;
            var duplicate = UnityEngine.Object.Instantiate(targetAvatar);
            duplicate.transform.position = new Vector3(duplicate.transform.position.x, duplicate.transform.position.y, duplicate.transform.position.z + 2);
            AvatarBuildUtils.ProcessAvatar(duplicate, null, false);
        }
    }
}
#endif

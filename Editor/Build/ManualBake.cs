using net.rs64.TexTransTool.Editor.OtherMenuItem;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Build
{
    internal static class ManualBake
    {
        [MenuItem(TTTConfig.DEBUG_MENU_PATH + "/TexTransTool Only Manual Bake Avatar")]
        private static void ManualBakeSelected()
        {
            var targetAvatar = Selection.activeGameObject;
            if (targetAvatar == null)
            {
                Debug.LogError("選択中の Avatar が存在しないため、Manual Bake Avatarを実行できません。");
                return;
            }
            ManualBakeAvatar(targetAvatar);
        }

        public static void ManualBakeAvatar(GameObject targetAvatar)
        {
            PreviewUtility.ExitPreviews();

            var duplicate = UnityEngine.Object.Instantiate(targetAvatar);
            duplicate.transform.position = new Vector3(duplicate.transform.position.x, duplicate.transform.position.y, duplicate.transform.position.z + 2);
            AvatarBuildUtils.ProcessAvatar(duplicate, true);
        }
    }
}

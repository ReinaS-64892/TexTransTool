using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.TextureAtlas;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    public static class UnityLeakDetectionMode
    {
        const string MenuPath = "Tools/" + TexTransBehavior.TTTName + "/Debug/UnityLeakDetectionMode";
        [MenuItem(MenuPath)]
        public static void Toggle()
        {
            var mode = UnsafeUtility.GetLeakDetectionMode();
            switch (mode)
            {
                case NativeLeakDetectionMode.Disabled:
                    {
                        UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
                        Menu.SetChecked(MenuPath, true);
                        break;
                    }
                case NativeLeakDetectionMode.Enabled:
                case NativeLeakDetectionMode.EnabledWithStackTrace:
                    {
                        UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.Disabled);
                        Menu.SetChecked(MenuPath, false);
                        break;
                    }
            }
        }
    }
}

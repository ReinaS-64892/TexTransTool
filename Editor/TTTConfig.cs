using UnityEditor;

namespace net.rs64.TexTransTool
{
    internal static class TTTConfig
    {
        public const string TTT_MENU_PATH = "Tools/TexTransTool";
        public const string EXPERIMENTAL_MENU_PATH = TTT_MENU_PATH + "/Experimental";
        public const string DEBUG_MENU_PATH = TTT_MENU_PATH + "/Debug";


        internal static void SettingInitializer()
        {
            IsObjectReplaceInvoke = EditorPrefs.GetBool(OBJECT_REPLACE_INVOKE_PREFKEY);
            UseIslandCache = EditorPrefs.GetBool(USE_ISLAND_CACHE_PREFKEY, true);
        }
        #region UseIslandCache
        public const string USE_ISLAND_CACHE_MENU_PATH = TTT_MENU_PATH + "/UseIslandCache";
        public const string USE_ISLAND_CACHE_PREFKEY = "net.rs64.tex-trans-tool.UseIslandCache";
        private static bool s_useIslandCache;
        public static bool UseIslandCache { get => s_useIslandCache; private set { s_useIslandCache = value; UseIslandCacheConfigUpdate(); } }

        [MenuItem(USE_ISLAND_CACHE_MENU_PATH)]
        static void ToggleUseIslandCache()
        {
            UseIslandCache = !UseIslandCache;
        }
        private static void UseIslandCacheConfigUpdate()
        {
            EditorPrefs.SetBool(USE_ISLAND_CACHE_PREFKEY, UseIslandCache);
            Menu.SetChecked(USE_ISLAND_CACHE_MENU_PATH, UseIslandCache);
        }
        #endregion

        #region ObjectReplaceInvoke
        public const string OBJECT_REPLACE_INVOKE_MENU_PATH = EXPERIMENTAL_MENU_PATH + "/ObjectReplaceInvoke";
        public const string OBJECT_REPLACE_INVOKE_PREFKEY = "net.rs64.tex-trans-tool.ObjectReplaceInvoke";
        private static bool s_isObjectReplaceInvoke;
        public static bool IsObjectReplaceInvoke { get => s_isObjectReplaceInvoke; private set { s_isObjectReplaceInvoke = value; ObjectReplaceInvokeConfigUpdate(); } }

        [MenuItem(OBJECT_REPLACE_INVOKE_MENU_PATH)]
        static void ToggleObjectReplaceInvoke()
        {
            IsObjectReplaceInvoke = !IsObjectReplaceInvoke;
        }
        private static void ObjectReplaceInvokeConfigUpdate()
        {
            EditorPrefs.SetBool(OBJECT_REPLACE_INVOKE_PREFKEY, IsObjectReplaceInvoke);
            Menu.SetChecked(OBJECT_REPLACE_INVOKE_MENU_PATH, IsObjectReplaceInvoke);
        }
        #endregion
    }
}

using System;
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
            Localize.LocalizeInitializer();
        }
        #region Language


        public const string LANGUAGE_PREFKEY = "net.rs64.tex-trans-tool.language";
        public const string LANGUAGE_MENU_PATH = TTTConfig.TTT_MENU_PATH + "/Language";
        private static LanguageEnum s_language;
        public static LanguageEnum Language
        {
            get => s_language;
            internal set
            {
                Menu.SetChecked(LANGUAGE_MENU_PATH + "/" + s_language.ToString(), false);
                s_language = value;
                EditorPrefs.SetInt(LANGUAGE_PREFKEY, (int)s_language);
                Menu.SetChecked(LANGUAGE_MENU_PATH + "/" + s_language.ToString(), true);
                OnSwitchLanguage?.Invoke(s_language);
            }
        }
        public enum LanguageEnum
        {
            EN,
            JP,
        }
        public static Action<LanguageEnum> OnSwitchLanguage;

        [MenuItem(LANGUAGE_MENU_PATH + "/EN")]
        public static void SwitchEN() => Language = LanguageEnum.EN;
        [MenuItem(LANGUAGE_MENU_PATH + "/JP")]
        public static void SwitchJP() => Language = LanguageEnum.JP;

        [MenuItem(TTTConfig.DEBUG_MENU_PATH + "/ReloadLocalize")]
        public static void ReloadLocalize() => Localize.LoadLocalize();

        #endregion


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

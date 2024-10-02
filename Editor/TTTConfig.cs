using System;
using net.rs64.TexTransUnityCore;
using UnityEditor;

namespace net.rs64.TexTransTool
{
    internal static class TTTConfig
    {
        public const string TTT_MENU_PATH = "Tools/TexTransTool";
        public const string EXPERIMENTAL_MENU_PATH = TTT_MENU_PATH + "/Experimental";
        public const string DEBUG_MENU_PATH = TTT_MENU_PATH + "/Debug";


        [TexTransInitialize]
        internal static void SettingInitializer()
        {
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

    }
}

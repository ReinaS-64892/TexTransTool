using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
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
            InitInternalTextureFormat();
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
            JA,
        }
        public static Action<LanguageEnum> OnSwitchLanguage;

        [MenuItem(LANGUAGE_MENU_PATH + "/EN")]
        public static void SwitchEN() => Language = LanguageEnum.EN;
        [MenuItem(LANGUAGE_MENU_PATH + "/JA")]
        public static void SwitchJP() => Language = LanguageEnum.JA;

        [MenuItem(TTTConfig.DEBUG_MENU_PATH + "/ReloadLocalize")]
        public static void ReloadLocalize() => Localize.LoadLocalize();

        #endregion

        [MenuItem(DEBUG_MENU_PATH + "/ForceTempRtRelease")]
        static void ForceTempRtRelease() { TTRt.ForceLeakedRelease(); }

        public const string INTERNAL_TEXTURE_FORMAT_PREFKEY = "net.rs64.tex-trans-tool.internal-texture-format";
        public const string INTERNAL_TEXTURE_FORMAT_PATH = TTTConfig.TTT_MENU_PATH + "/InternalTextureFormat";

        const string INTERNAL_TEXTURE_FORMAT_MENU_NAME_BYTE = "0: Byte - 8bit unsigned integer";
        const string INTERNAL_TEXTURE_FORMAT_MENU_NAME_USHORT = "1: UShort - 16bit unsigned integer";
        const string INTERNAL_TEXTURE_FORMAT_MENU_NAME_HALF = "2: Half - 16bit signed float";
        const string INTERNAL_TEXTURE_FORMAT_MENU_NAME_FLOAT = "3: Float - 32bit signed float";
        static string GetFormatToMenuName(TexTransCoreTextureFormat format)
        {
            switch (format)
            {
                default:
                case TexTransCoreTextureFormat.Byte: { return INTERNAL_TEXTURE_FORMAT_MENU_NAME_BYTE; }
                case TexTransCoreTextureFormat.UShort: { return INTERNAL_TEXTURE_FORMAT_MENU_NAME_USHORT; }
                case TexTransCoreTextureFormat.Half: { return INTERNAL_TEXTURE_FORMAT_MENU_NAME_HALF; }
                case TexTransCoreTextureFormat.Float: { return INTERNAL_TEXTURE_FORMAT_MENU_NAME_FLOAT; }
            }
        }
        static void InitInternalTextureFormat()
        {
            InternalTextureFormat = (TexTransCoreTextureFormat)EditorPrefs.GetInt(INTERNAL_TEXTURE_FORMAT_PREFKEY, 0);
        }
        private static TexTransCoreTextureFormat s_internalTextureFormat;
        public static TexTransCoreTextureFormat InternalTextureFormat
        {
            get => s_internalTextureFormat;
            internal set
            {
                Menu.SetChecked(INTERNAL_TEXTURE_FORMAT_PATH + "/" + GetFormatToMenuName(s_internalTextureFormat), false);
                s_internalTextureFormat = value;
                EditorPrefs.SetInt(INTERNAL_TEXTURE_FORMAT_PREFKEY, (int)s_internalTextureFormat);
                Menu.SetChecked(INTERNAL_TEXTURE_FORMAT_PATH + "/" + GetFormatToMenuName(s_internalTextureFormat), true);

                TTRt2.SetRGBAFormat(s_internalTextureFormat);
            }
        }

        [MenuItem(INTERNAL_TEXTURE_FORMAT_PATH + "/" + INTERNAL_TEXTURE_FORMAT_MENU_NAME_BYTE)]
        static void SwitchByte() => InternalTextureFormat = TexTransCoreTextureFormat.Byte;

        [MenuItem(INTERNAL_TEXTURE_FORMAT_PATH + "/" + INTERNAL_TEXTURE_FORMAT_MENU_NAME_USHORT)]
        static void SwitchUShort() => InternalTextureFormat = TexTransCoreTextureFormat.UShort;
        [MenuItem(INTERNAL_TEXTURE_FORMAT_PATH + "/" + INTERNAL_TEXTURE_FORMAT_MENU_NAME_HALF)]
        static void SwitchHalf() => InternalTextureFormat = TexTransCoreTextureFormat.Half;
        [MenuItem(INTERNAL_TEXTURE_FORMAT_PATH + "/" + INTERNAL_TEXTURE_FORMAT_MENU_NAME_FLOAT)]
        static void SwitchFloat() => InternalTextureFormat = TexTransCoreTextureFormat.Float;



    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class Localize
    {
        const string JA_GUID = "42db3dbd5755c844984648836a49629f";
        const string EN_GUID = "b6008be0d5fa3d242ba93f9a930df3c3";
        public static void LoadLocalize()
        {
            LocalizationAssets = new();
            LocalizationAssets["ja-JP"] = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(JA_GUID));
            LocalizationAssets["en-US"] = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(EN_GUID));

            foreach (var exLang in TexTransTool.PublicAPI.TexTransToolExternalLocalize.s_ExternalLocalizationAssets)
            {
                if (LocalizationAssets.ContainsKey(exLang.Key)) { continue; }//通常在りえないコードパス
                LocalizationAssets[exLang.Key] = exLang.Value;
            }

            Languages = LocalizationAssets.Keys.ToArray();
        }
        [TexTransCoreEngineForUnity.TexTransInitialize]
        public static void ListenExternalLocalizeAdded()
        {
            TexTransTool.PublicAPI.TexTransToolExternalLocalize.s_OnAddLocalization = LoadLocalize;
        }

        internal static Dictionary<string, LocalizationAsset> LocalizationAssets = null;
        internal static string[] Languages = null;


        static TTTGlobalConfig s_config;
        static TTTProjectConfig s_projectConfig;
        public static string GetLocalize(this string str)
        {
            s_config ??= TTTGlobalConfig.instance;
            var lang = s_config.Language;

            if (LocalizationAssets is null) { LoadLocalize(); }

            if (LocalizationAssets.TryGetValue(lang, out var langAssets))
                return langAssets.GetLocalizedString(str);
            else
                return str;
        }
        public static GUIContent GlcV(this string str)
        {
            s_projectConfig ??= TTTProjectConfig.instance;

            var c = str.Glc();
            if (s_projectConfig.DisplayVRAMIcon) c.image = TTTImageAssets.VramICon;
            return c;
        }

        public static GUIContent Glc(this string str)
        {
            var tooltipKey = str + ":tooltip";
            var tooltipStr = tooltipKey.GetLocalize();
            if (tooltipKey == tooltipStr) { tooltipStr = ""; }
            return new GUIContent(str.GetLocalize(), tooltipStr);
        }
        public static GUIContent Glf(this string str, params object[] objects)
        {
            var cuiContent = str.Glc();
            cuiContent.text = string.Format(cuiContent.text, objects);
            cuiContent.tooltip = string.Format(cuiContent.tooltip, objects);
            return cuiContent;
        }
    }
}


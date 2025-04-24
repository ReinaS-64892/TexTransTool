using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.PublicAPI
{
    [TexTransToolStablePublicAPI]
    public static class TexTransToolExternalLocalize
    {
        /// <summary>
        /// TexTransTool に 日本語(ja-JP) と 英語(en-US) 以外の言語を追加することができます。
        /// </summary>
        /// <param name="localeIsoCode">言語を識別する code となります。これは グローバル設定に保存されるものになります。</param>
        /// <param name="localization">言語ファイル、書き込む msgid と msgstr の組み合わせは 日本語や英語のファイルを参考にしてください。</param>
        /// <returns>日本語や英語またはすでに登録されている言語だった場合に false になり、正常に登録できた場合は true になります。(ただし obsolete になった場合は false を常に返すようになるでしょう。)</returns>
        [TexTransToolStablePublicAPI]
        public static bool RegisterLocalization(string localeIsoCode, LocalizationAsset localization)
        {
            if (string.IsNullOrWhiteSpace(localeIsoCode)) { return false; }
            if (localeIsoCode is "ja-JP" or "en-US") return false;
            if (s_ExternalLocalizationAssets.ContainsKey(localeIsoCode)) { return false; }

            s_ExternalLocalizationAssets[localeIsoCode] = localization;
            s_OnAddLocalization?.Invoke();
            return true;
        }

        internal static Dictionary<string, LocalizationAsset> s_ExternalLocalizationAssets = new();
        internal static Action s_OnAddLocalization = null;
    }
}

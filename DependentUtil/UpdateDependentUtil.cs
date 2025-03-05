using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.DestructiveTextureUtilities
{
    static class UpdateDependentUtil
    {
        const string TEX_TRANS_TOOL_PACKAGE_DOT_JSON_PATH = "Packages/TexTransTool/package.json";
        const string TEX_TRANS_CORE_PACKAGE_DOT_JSON_PATH = "Packages/TexTransCore/package.json";
        const string DEPENDENCY_PACKAGE = "net.rs64.tex-trans-core";

        [InitializeOnLoadMethod]
        static async void UpdateNow()
        {
            await Task.Run(() =>
                        {
                            try
                            {
                                string tttVersion = GetTTCVersion();
                                writeTTTVersion("^" + tttVersion);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        });
        }
        // ここにあるコードは何一つ JsonParser ではない。
        private static string GetTTCVersion()
        {
            var ttt = File.ReadAllText(TEX_TRANS_CORE_PACKAGE_DOT_JSON_PATH).Split("\n");
            var vstr = "\"version\":";
            var tttVersionLine = ttt.First(str => str.Contains(vstr));
            var tttVersion = GetString(tttVersionLine);
            return tttVersion;
        }

        private static void writeTTTVersion(string tttVersion)
        {
            var ttt = File.ReadAllText(TEX_TRANS_TOOL_PACKAGE_DOT_JSON_PATH).Split("\n");
            foreach (var i in FindIndexAll(ttt, str => str.Contains($"\"{DEPENDENCY_PACKAGE}\":")))
            {
                ttt[i] = ttt[i].Replace(GetString(ttt[i]), tttVersion);
            }
            File.WriteAllText(TEX_TRANS_TOOL_PACKAGE_DOT_JSON_PATH, string.Join("\n", ttt));
        }

        private static string GetString(string tttVersionLine)
        {
            var spIndex = tttVersionLine.IndexOf(":");
            var stringStart = tttVersionLine.IndexOf("\"", spIndex + 1);
            var stringEnd = tttVersionLine.LastIndexOf("\"");

            var stringIndex = stringStart + 1;
            var stringLength = stringEnd - stringIndex;

            return tttVersionLine.Substring(stringIndex, stringLength);
        }
        private static IEnumerable<int> FindIndexAll<T>(T[] array, Predicate<T> predicate)
        {
            for (var i = 0; i < array.Length; i += 1)
            {
                if (predicate.Invoke(array[i])) yield return i;
            }
        }
    }
}

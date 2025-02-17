using System.IO;
using UnityEditor;

namespace net.rs64.TexTransTool.PSDImporter
{
    static class PSDImporterMigration
    {
        const string ScriptedImporterTypeCode = "nativeImporterType: 2089858483";
        public static void PSDImporterReSetting()
        {
            foreach (var metaPath in Directory.GetFiles("Assets", "*.psd.meta", SearchOption.AllDirectories))
            {
                if (File.ReadAllText(metaPath).Contains(ScriptedImporterTypeCode) is false) { continue; }
                var psdPath = metaPath.Substring(0, metaPath.Length - 5);

                AssetDatabase.SetImporterOverride<TexTransToolPSDImporter>(psdPath);
            }
        }
    }
}

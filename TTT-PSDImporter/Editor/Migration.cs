using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    static class PSDImporterMigration
    {
        const string OldAssemblyPSDImporterTypeCode = "nativeImporterType: 2089858483";
        public static void PSDImporterReSetting()
        {
            foreach (var metaPath in Directory.GetFiles("Assets", "*.psd.meta", SearchOption.AllDirectories))
            {
                if (File.ReadAllText(metaPath).Contains(OldAssemblyPSDImporterTypeCode) is false) { continue; }
                var psdPath = metaPath.Substring(0, metaPath.Length - 5);

                AssetDatabase.SetImporterOverride<TexTransToolPSDImporter>(psdPath);
            }
        }
    }
}

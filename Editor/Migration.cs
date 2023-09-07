#if UNITY_EDITOR

using System;
using System.IO;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;


namespace net.rs64.TexTransTool
{
    [InitializeOnLoad]
    internal static class Migration
    {
        public static string SaveDataVersionPath = "ProjectSettings/net.rs64.TexTransTool-Version.json";

        static Migration()
        {
            if (!File.Exists(SaveDataVersionPath))
            {
                var NawSaveDataVersion = new SaveDataVersionJson();
                NawSaveDataVersion.SaveDataVersion = ToolUtils.ThiSaveDataVersion;
                var jsonStr = JsonUtility.ToJson(NawSaveDataVersion);

                File.WriteAllText(SaveDataVersionPath, jsonStr);
            }
        }


        [Serializable]
        private class SaveDataVersionJson
        {
            public int SaveDataVersion;
        }
    }
}
#endif
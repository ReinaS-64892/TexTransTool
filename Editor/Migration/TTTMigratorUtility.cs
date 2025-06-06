using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration
{
    interface IMigrator
    {
        int MigrateTarget { get; }
        bool Migration(ITexTransToolTag texTransToolTag);
    }
    interface IMigratorUseFinalize
    {
        bool MigrationFinalize(ITexTransToolTag texTransToolTag);
    }
    internal static class MigrationUtility
    {
        public static string SaveDataVersionPath = "ProjectSettings/net.rs64.TexTransTool-Version.json";

        public static void SetSaveDataVersion(ITexTransToolTag texTransToolTag, int value)
        {
            if (!(texTransToolTag is UnityEngine.Object unityObject)) { return; }
            if (unityObject == null) { return; }
            var sObj = new SerializedObject(unityObject);
            var saveDataProp = sObj.FindProperty("_saveDataVersion");
            if (saveDataProp == null) { Debug.LogWarning(texTransToolTag.GetType() + " : SaveDataVersionの書き換えができませんでした。"); }
            saveDataProp.intValue = value;
            sObj.ApplyModifiedPropertiesWithoutUndo();
        }



    }
}

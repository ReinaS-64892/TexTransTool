using System;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class TTTImageAssets
    {
        internal static Texture2D Icon;
        internal static Texture2D Logo;
        internal static Texture2D VramICon;


        [TexTransInitialize]
        internal static void Init()
        {
            Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("846ba4dba0267cf4187be80bb6577627"));
            Logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("85d97058f00c6f44a85833650996ea43"));
            VramICon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("99307eb41226073488a7a2dc4e67f4a1"));
        }

    }
}

using System;
using net.rs64.TexTransUnityCore;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class TTTImageAssets
    {
        internal static Texture2D Icon;
        internal static Texture2D Logo;


        [TexTransInitialize]
        internal static void Init()
        {
            Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("846ba4dba0267cf4187be80bb6577627"));
            Logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("85d97058f00c6f44a85833650996ea43"));
        }

    }
}

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using Island = net.rs64.TexTransCore.Island.Island;
using static net.rs64.TexTransCore.TransTextureCore.TransTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.EditorIsland;
using net.rs64.TexTransTool.TextureAtlas.FineSetting;
using UnityEditor;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public interface IAtlasIslandSorter
    {
        string SorterName { get; }
        bool RectTangleMove { get; }
        Dictionary<AtlasIslandID, AtlasIsland> Sorting(Dictionary<AtlasIslandID, AtlasIsland> atlasIslands, float Padding);
    }

    public static class AtlasIslandSorterUtility
    {
        static Dictionary<string, IAtlasIslandSorter> Sorters;
        static string[] SortersNames;
        [InitializeOnLoadMethod]
        static void Init()
        {
            var interfaces = InterfaceUtility.GetInterfaceInstance<IAtlasIslandSorter>();
            Sorters = new Dictionary<string, IAtlasIslandSorter>();
            foreach (var sorter in interfaces) { Sorters.Add(sorter.SorterName, sorter); }
            SortersNames = Sorters.Keys.ToArray();
        }
        public static IAtlasIslandSorter GetSorter(string SorterName)
        {
            if (Sorters == null) { Debug.LogError("Not Init"); return null; }
            if (!Sorters.ContainsKey(SorterName)) { Debug.LogError("Sorter Is not Exist"); return null; }
            return Sorters[SorterName];
        }

        public static string[] GetSorterName() => SortersNames;
    }
}
#endif
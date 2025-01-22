#nullable enable
using nadena.dev.ndmf;
using net.rs64.TexTransTool.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace net.rs64.TexTransTool.NDMF
{
    internal static class NDMFPreviewMaterialPool
    {
        static Dictionary<Shader, List<PooledMaterial>> s_pool = new();
        static Dictionary<Material, PooledMaterial> s_reverseDict = new();

        public static Material Get(Material source)
        {
            var s = source.shader;
            if (s_pool.ContainsKey(s) is false) { s_pool[s] = new(); }
            var sPool = s_pool[s];

            foreach (var m in sPool)
            {
                if (m.IsUsed) { continue; }
                MaterialUtility.PropertyCopy(source, m.Material);
                m.IsUsed = true;
                return m.Material;
            }

            var nm = Material.Instantiate(source);
            nm.parent = null;
            var pooledMat = new PooledMaterial(nm) { IsUsed = true };
            sPool.Add(pooledMat);
            s_reverseDict[nm] = pooledMat;
            return nm;
        }
        public static bool Ret(Material r)
        {
            if (s_reverseDict.ContainsKey(r) is false) { return false; }

            var pm = s_reverseDict[r];
            if (pm.IsUsed is false) { return false; }

            if (pm.Shader != r.shader) { r.shader = pm.Shader; }
            pm.IsUsed = false;

            return true;
        }
        class PooledMaterial
        {
            public Shader Shader;
            public Material Material;
            public bool IsUsed;

            public PooledMaterial(Material mat) { Material = mat; Shader = Material.shader; }
        }
    }
}

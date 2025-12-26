using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class ShaderTempMaterialManager
    {
        static Dictionary<Shader, Material> s_shader2TempMaterial = new();
        public static Material GetOrCreateTempMaterial(Shader shader)
        {
            if(!s_shader2TempMaterial.TryGetValue(shader, out Material tempMat) || tempMat == null)
            {
                tempMat = new Material(shader);
                s_shader2TempMaterial[shader] = tempMat;
            }
            return tempMat;
        }
    }
}

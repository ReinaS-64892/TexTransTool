using System.Collections.Generic;
using UnityEngine;

internal static class MatTemp
{
    static Dictionary<Shader, Material> s_shader2TempMaterial = new();
    public static Material GetTempMatShader(Shader shader)
    {
        if (s_shader2TempMaterial.ContainsKey(shader) is false || s_shader2TempMaterial[shader] == null) { s_shader2TempMaterial[shader] = new Material(shader); }
        return s_shader2TempMaterial[shader];
    }
}

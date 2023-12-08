using UnityEngine;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.BlendTexture
{
    internal interface TexBlendExtension
    {
        (HashSet<string> ShaderKeywords, Shader shader) GetExtensionBlender();
    }
}
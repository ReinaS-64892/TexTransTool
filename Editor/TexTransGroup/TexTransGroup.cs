using UnityEngine;
using System.Collections.Generic;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TexTransGroup")]
    public class TexTransGroup : AbstractTexTransGroup
    {
        public List<TextureTransformer> TextureTransformers = new List<TextureTransformer>();
        public override IEnumerable<TextureTransformer> Targets => TextureTransformers;
    }
}
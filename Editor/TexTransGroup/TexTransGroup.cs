#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TexTransGroup")]
    public class TexTransGroup : AbstractTexTransGroup
    {
        public override IEnumerable<TextureTransformer> Targets => transform.GetChildren().ConvertAll(x => x.GetComponent<TextureTransformer>()).Where(x => x != null);
    }
}
#endif
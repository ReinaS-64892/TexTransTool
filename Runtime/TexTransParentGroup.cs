#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Rs64.TexTransTool
{
    public class TexTransParentGroup : AbstractTexTransGroup
    {
        public override IEnumerable<TextureTransformer> Targets => transform.GetChilds().ConvertAll(x => x.GetComponent<TextureTransformer>());
    }
}
#endif
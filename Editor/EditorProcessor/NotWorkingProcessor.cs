using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(PreviewGroup))]
    internal class NotWorkingProcessor : IEditorProcessor
    {

        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain domain)
        {
            //何もしない
        }
        public IEnumerable<Renderer> ModificationTargetRenderers(TexTransCallEditorBehavior texTransCallEditorBehavior, IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            yield break;
        }
    }
}

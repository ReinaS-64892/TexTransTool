using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(TexTransGroup))]
    [EditorProcessor(typeof(PhaseDefinition))]
    internal class TexTransGroupProcessor : IEditorProcessor
    {
        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain domain)
        {
            var ttg = texTransCallEditorBehavior as TexTransGroup;

            // editorCallDomain.ProgressStateEnter("TexTransGroup");

            var targetList = TexTransGroup.TextureTransformerFilter(ttg.Targets).ToArray();
            var count = 0;
            foreach (var tf in targetList)
            {
                count += 1;
                TTTLog.ReportingObject(tf, () => tf.Apply(domain));
                // editorCallDomain.ProgressUpdate(tf.name + " Apply", (float)count / targetList.Length);
            }
            // editorCallDomain.ProgressStateExit();
        }
        public IEnumerable<Renderer> ModificationTargetRenderers(TexTransCallEditorBehavior texTransCallEditorBehavior, IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var ttg = texTransCallEditorBehavior as TexTransGroup;

            var targetList = TexTransGroup.TextureTransformerFilter(ttg.Targets).ToArray();
            return targetList.SelectMany(i => i.ModificationTargetRenderers(domainRenderers, replaceTracking));
        }
    }
}

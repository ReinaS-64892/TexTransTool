using System.Linq;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(TexTransGroup))]
    [EditorProcessor(typeof(PhaseDefinition))]
    internal class TexTransGroupProcessor : IEditorProcessor
    {
        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain)
        {
            var ttg = texTransCallEditorBehavior as TexTransGroup;

            if (!ttg.IsPossibleApply) { TTTLog.Error("Not executable"); return; }
            editorCallDomain.ProgressStateEnter("TexTransGroup");

            var targetList = TexTransGroup.TextureTransformerFilter(ttg.Targets).ToArray();
            var count = 0;
            foreach (var tf in targetList)
            {
                count += 1;
                tf.Apply(editorCallDomain);
                editorCallDomain.ProgressUpdate(tf.name + " Apply", (float)count / targetList.Length);
            }
            editorCallDomain.ProgressStateExit();
        }
    }
}

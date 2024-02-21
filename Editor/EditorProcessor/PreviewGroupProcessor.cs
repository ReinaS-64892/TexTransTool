using System.Linq;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(PreviewGroup))]
    internal class PreviewGroupProcessor : IEditorProcessor
    {
        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain)
        {
            //できることはない
        }
    }
}

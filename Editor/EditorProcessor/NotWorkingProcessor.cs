namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(PreviewGroup))]
    [EditorProcessor(typeof(PreviewRenderer))]
    internal class NotWorkingProcessor : IEditorProcessor
    {
        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain)
        {
            //何もしない
        }
    }
}

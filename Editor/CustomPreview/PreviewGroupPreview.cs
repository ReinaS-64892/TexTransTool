using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.CustomPreview
{
    [TTTCustomPreview(typeof(PreviewGroup))]
    internal class PreviewGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransBehavior texTransBehavior, IEditorCallDomain editorCallDomain)
        {
            if (texTransBehavior is not PreviewGroup previewGroup) { return; }

            var phaseOnTf = AvatarBuildUtils.FindAtPhase(previewGroup.gameObject);
            TexTransGroupPreview.ExecutePhases(editorCallDomain, phaseOnTf);
        }


    }
}

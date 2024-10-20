using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.Preview.Custom
{
    [TTTCustomPreview(typeof(PreviewGroup))]
    internal class PreviewGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransMonoBase texTransBehavior, RenderersDomain domain)
        {
            if (texTransBehavior is not PreviewGroup previewGroup) { return; }

            var phaseOnTf = AvatarBuildUtils.FindAtPhase(previewGroup.gameObject);
            var session = new TexTransBuildSession(domain, phaseOnTf);
            AvatarBuildUtils.ExeCuteAllPhase(session);
        }


    }
}

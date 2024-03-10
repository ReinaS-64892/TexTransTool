using System;
using nadena.dev.ndmf;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.Build.NDMF
{
    internal class TexTransToolContext : IExtensionContext
    {
        internal TexTransBuildSession TTTBuildContext { get; private set; }
        public void OnActivate(BuildContext context)
        {
            if (context != null)
            {
                TTTBuildContext = new TexTransBuildSession(new AvatarDomain(context.AvatarRootObject, false, new AssetSaver(context.AssetContainer)));
            }
        }

        public void OnDeactivate(BuildContext context)
        {
            if (TTTBuildContext != null)
            {
                try { TTTBuildContext.TTTSessionEnd(); } catch (Exception e) { TTTLog.Exception(e); }
                TTTBuildContext = null;
            }
        }
    }
}

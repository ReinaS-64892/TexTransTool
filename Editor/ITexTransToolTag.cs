
#if VRC_BASE
using VRC.SDKBase;
#endif

namespace net.rs64.TexTransTool
{
    public interface ITexTransToolTag

#if VRC_BASE
: IEditorOnly
#endif

    {
        int SaveDataVersion { get; }
    }
}
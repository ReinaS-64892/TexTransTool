#nullable enable
namespace net.rs64.TexTransCore
{
    /// <summary>
    /// なぜ Key じゃないのかというと、特殊なパラメーターを持つから
    /// </summary>
    public interface ITTGrabBlending
    {
        void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler;
    }


}

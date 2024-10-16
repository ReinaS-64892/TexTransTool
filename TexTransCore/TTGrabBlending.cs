#nullable enable
namespace net.rs64.TexTransCore
{
    public abstract class TTGrabBlending
    {
        public ITTComputeKey ComputeKey;

        protected TTGrabBlending(ITTComputeKey computeKey)
        {
            ComputeKey = computeKey;
        }
    }


}

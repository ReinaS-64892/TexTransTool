#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LevelAdjustment : ITTGrabBlending
    {
        public LevelData RGB;
        public LevelData R;
        public LevelData G;
        public LevelData B;
        public LevelAdjustment(LevelData rgb, LevelData r, LevelData g, LevelData b)
        {
            RGB = rgb;
            R = r;
            G = g;
            B = b;
        }

        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture) where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.GrabBlend[nameof(LevelAdjustment)]);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            computeHandler.SetTexture(texID, grabTexture);
            Span<float> gvBuf = stackalloc float[8];


            RGB.WriteToConstantsBuffer(gvBuf);
            gvBuf[5] = gvBuf[6] = gvBuf[7] = 1f;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.DispatchWithTextureSize(grabTexture);


            R.WriteToConstantsBuffer(gvBuf);
            gvBuf[5] = 1f;
            gvBuf[6] = gvBuf[7] = 0f;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.DispatchWithTextureSize(grabTexture);


            G.WriteToConstantsBuffer(gvBuf);
            gvBuf[6] = 1f;
            gvBuf[5] = gvBuf[7] = 0f;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.DispatchWithTextureSize(grabTexture);


            B.WriteToConstantsBuffer(gvBuf);
            gvBuf[7] = 1f;
            gvBuf[5] = gvBuf[6] = 0f;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.DispatchWithTextureSize(grabTexture);
        }
    }
    [Serializable]
    public class LevelData
    {
        [Range(0, 0.99f)] public float InputFloor = 0;
        [Range(0.01f, 1)] public float InputCeiling = 1;

        [Range(0.1f, 9.9f)] public float Gamma = 1;

        [Range(0, 1)] public float OutputFloor = 0;
        [Range(0, 1)] public float OutputCeiling = 1;

        internal void WriteToConstantsBuffer(Span<float> floats)
        {
            floats[0] = InputFloor;
            floats[1] = InputCeiling;
            floats[2] = Gamma;
            floats[3] = OutputFloor;
            floats[4] = OutputCeiling;
        }
    }
}

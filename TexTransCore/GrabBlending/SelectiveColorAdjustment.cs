#nullable enable
using System;
using System.Numerics;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class SelectiveColorAdjustment : ITTGrabBlending
    {
        public Vector4 RedsCMYK;
        public Vector4 YellowsCMYK;
        public Vector4 GreensCMYK;
        public Vector4 CyansCMYK;
        public Vector4 BluesCMYK;
        public Vector4 MagentasCMYK;
        public Vector4 WhitesCMYK;
        public Vector4 NeutralsCMYK;
        public Vector4 BlacksCMYK;
        public bool IsAbsolute;
        public SelectiveColorAdjustment(Vector4 red, Vector4 yellow, Vector4 green, Vector4 cyan, Vector4 blue, Vector4 magenta, Vector4 white, Vector4 neutral, Vector4 black, bool isAbsolute)
        {
            RedsCMYK = red;
            YellowsCMYK = yellow;
            GreensCMYK = green;
            CyansCMYK = cyan;
            BluesCMYK = blue;
            MagentasCMYK = magenta;
            WhitesCMYK = white;
            NeutralsCMYK = neutral;
            BlacksCMYK = black;
            IsAbsolute = isAbsolute;
        }


        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture) where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.GrabBlend[nameof(SelectiveColorAdjustment)]);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[40];
            WriteVector4(gvBuf.Slice(0, 4), RedsCMYK);
            WriteVector4(gvBuf.Slice(4, 4), YellowsCMYK);
            WriteVector4(gvBuf.Slice(8, 4), GreensCMYK);
            WriteVector4(gvBuf.Slice(12, 4), CyansCMYK);
            WriteVector4(gvBuf.Slice(16, 4), BluesCMYK);
            WriteVector4(gvBuf.Slice(20, 4), MagentasCMYK);
            WriteVector4(gvBuf.Slice(24, 4), WhitesCMYK);
            WriteVector4(gvBuf.Slice(28, 4), NeutralsCMYK);
            WriteVector4(gvBuf.Slice(32, 4), BlacksCMYK);
            gvBuf[36] = IsAbsolute ? 1f : 0f;

            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);
            computeHandler.SetTexture(texID, grabTexture);

            computeHandler.DispatchWithTextureSize(grabTexture);


            static void WriteVector4(Span<float> buf, Vector4 value)
            {
                buf[0] = value.X;
                buf[1] = value.Y;
                buf[2] = value.Z;
                buf[3] = value.W;
            }
        }


    }
}

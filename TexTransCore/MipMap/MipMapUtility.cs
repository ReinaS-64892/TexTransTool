using UnityEngine;

namespace net.rs64.TexTransCore.MipMap
{
    internal static class MipMapUtility
    {
        public static ComputeShader MipMapShader;
        const string WTex = "WTex";
        const string RTex = "RTex";
        const string PixelRatio = "PixelRatio";
        public static bool Average(RenderTexture renderTexture)
        {
            if (!renderTexture.useMipMap || !renderTexture.enableRandomWrite) { return false; }
            var kernel32ID = MipMapShader.FindKernel("Average32");
            var kernel1ID = MipMapShader.FindKernel("Average1");

            var width = renderTexture.width;
            var height = renderTexture.height;

            bool useOne = false;
            for (var mipIndex = 0; renderTexture.mipmapCount - 1 > mipIndex; mipIndex += 1)
            {
                width /= 2;
                height /= 2;

                if (width < 32 || height < 32) { useOne = true; }

                var kernelID = useOne ? kernel1ID : kernel32ID;
                var kernelSize = useOne ? 1 : 32;

                MipMapShader.SetTexture(kernelID, RTex, renderTexture, mipIndex);
                MipMapShader.SetTexture(kernelID, WTex, renderTexture, mipIndex + 1);
                MipMapShader.SetInts(PixelRatio, 2, 2);
                MipMapShader.Dispatch(kernelID, width / kernelSize, height / kernelSize, 1);
            }

            return true;
        }
    }
}

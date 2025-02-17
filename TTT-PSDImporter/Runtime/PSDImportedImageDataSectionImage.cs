using System;
using System.Buffers.Binary;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using net.rs64.ParserUtility;
using net.rs64.PSDParser;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.PSDParser;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.PSDImporter
{
    public class PSDImportedImageDataSectionImage : TTTImportedImage
    {
        public PSDLowLevelParser.PSDImageDataSectionData ImageDataSectionData;
        protected override void LoadImage(ITTImportedCanvasSource importSource, Span<byte> writeTarget)
        {
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            using var readBuffer = new NativeArray<byte>((int)ImageDataSectionData.ImageDataBinaryAddress.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            psdBinary.PSDByteArray.LongCopyTo(ImageDataSectionData.ImageDataBinaryAddress.StartAddress, readBuffer.AsSpan());

            var singleChannelPixelByte = ChannelImageDataParser.BitDepthToByteCount(ImageDataSectionData.BitDepth);
            var decodedDataLength = ChannelImageDataParser.GetImageByteCount((int)ImageDataSectionData.Width, (int)ImageDataSectionData.Height, ImageDataSectionData.BitDepth) * ImageDataSectionData.Channels;
            var decodeBuffer = new NativeArray<byte>(decodedDataLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            PSDLowLevelParser.LoadImageDataSectionChannelImageArray(readBuffer, ImageDataSectionData, decodeBuffer);
            switch (ImageDataSectionData.BitDepth, ImageDataSectionData.Channels)
            {
                default:
                    throw new InvalidOperationException();
                case (1, 1):
                    {
                        PSDLowLevelParser.ExpandChannelRToRGBPlusFillA(writeTarget, decodeBuffer);
                        break;
                    }
                case (8, 3):
                    {
                        PSDLowLevelParser.PackingR8ImageArray3ToRGBA32(ImageDataSectionData, writeTarget, decodeBuffer);
                        break;
                    }
                case (8, 4):
                    {
                        PSDLowLevelParser.PackingR8ImageArray4ToRGBA32(ImageDataSectionData, writeTarget, decodeBuffer);
                        break;
                    }
                case (16, 4):
                    {
                        PSDLowLevelParser.PackingRUshortImageArray4ToRGBAUshort(ImageDataSectionData, writeTarget, decodeBuffer);
                        break;
                    }
                case (32, 4):
                    {
                        PSDLowLevelParser.PackingRFloatImageArray4ToRGBAFloat(ImageDataSectionData, writeTarget, decodeBuffer);

                        var floatArray = MemoryMarshal.Cast<byte, float>(writeTarget);
                        for (var i = 0; floatArray.Length > i;)
                        {
                            floatArray[i] = TTMath.LinearToGamma(floatArray[i]);
                            i += 1;
                            floatArray[i] = TTMath.LinearToGamma(floatArray[i]);
                            i += 1;
                            floatArray[i] = TTMath.LinearToGamma(floatArray[i]);
                            i += 1;

                            i += 1;
                        }
                        break;
                    }
            }
            ChannelImageDataParser.HeightInvert(writeTarget, (int)(singleChannelPixelByte * 4 * ImageDataSectionData.Width), (int)ImageDataSectionData.Height);
        }
    }
}

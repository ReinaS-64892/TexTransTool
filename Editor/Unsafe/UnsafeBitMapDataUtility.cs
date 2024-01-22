using System;
using System.Drawing.Imaging;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool.Unsafe
{
    internal static class UnsafeBitMapDataUtility
    {

        public static unsafe void WriteBitMapData(NativeArray<byte> argbValue, BitmapData bmd)
        {
            if (bmd.PixelFormat != PixelFormat.Format32bppArgb) { throw new ArgumentException("BitmapDataのピクセルフォーマットが 32bppArgb 以外は非対応です。");  }
            if (argbValue.Length != bmd.Height * bmd.Width * 4) { throw new ArgumentException("BitmapDataのサイズとNativeArrayの長さが一致しません。"); }
            var to = new Span<byte>((void*)bmd.Scan0, argbValue.Length);
            Span<byte> form = argbValue;
            form.CopyTo(to);
        }


    }


}
using NUnit.Framework;
using net.rs64.TexTransTool;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TestCode
{
    public class TransTextureTest
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(null)]
        public void TransTest(int? TestSeed)
        {
            var sourcetex = new UnityEngine.Texture2D(512, 512, UnityEngine.TextureFormat.ARGB32, false);

            var sourcetexpixsel = sourcetex.GetPixels();
            var rundami = TestSeed.HasValue ? new System.Random(TestSeed.Value) : new System.Random();

            for (var i = 0; sourcetexpixsel.Length > i; i += 1)
            {
                sourcetexpixsel[i] = new UnityEngine.Color(rundami.Next(0, 255) / 255f, rundami.Next(0, 255) / 255f, rundami.Next(0, 255) / 255f, rundami.Next(0, 255) / 255f);
            }
            sourcetex.SetPixels(sourcetexpixsel);
            sourcetex.Apply();

            var targetrt = new UnityEngine.RenderTexture(512, 512, 0, UnityEngine.RenderTextureFormat.ARGB32);

            var trandata = new TransTexture.TransUVData(
                new List<TraiangleIndex>()
                {
                    new TraiangleIndex(0,1,2),
                    new TraiangleIndex(0,2,3),
                },
                new List<Vector2>()
                {
                    new Vector2(0,0),
                    new Vector2(1,0),
                    new Vector2(1,1),
                    new Vector2(0,1),
                },
                new List<Vector2>()
                {
                    new Vector2(0,0),
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(1,0),
                }
            );

            TransTexture.TransTextureToRenderTexture(targetrt, sourcetex, trandata, 0f, null);
            var target2d = targetrt.CopyTexture2D();

            for (var x = 0; x < 512; x += 1)
            {
                for (var y = 0; y < 512; y += 1)
                {
                    var S = sourcetex.GetPixel(y, x);
                    var T = target2d.GetPixel(x, y);

                    Assert.That(S == T, Is.True);
                }
            }


        }
    }

}
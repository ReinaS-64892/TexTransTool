using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.TestCode
{
    internal class TransTextureTest
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void TransTest(int? testSeed)
        {
            var sourcesTex = new UnityEngine.Texture2D(512, 512, UnityEngine.TextureFormat.ARGB32, false);

            var sourceTexPixel = sourcesTex.GetPixels();
            var randomI = testSeed.HasValue ? new System.Random(testSeed.Value) : new System.Random();

            for (var i = 0; sourceTexPixel.Length > i; i += 1)
            {
                sourceTexPixel[i] = new UnityEngine.Color(randomI.Next(0, 255) / 255f, randomI.Next(0, 255) / 255f, randomI.Next(0, 255) / 255f, randomI.Next(0, 255) / 255f);
            }
            sourcesTex.SetPixels(sourceTexPixel);
            sourcesTex.Apply();

            var targetRt = new UnityEngine.RenderTexture(512, 512, 0, UnityEngine.RenderTextureFormat.ARGB32);

            var transData = new TransTexture.TransData(
                new List<TriangleIndex>()
                {
                    new TriangleIndex(0,1,2),
                    new TriangleIndex(0,2,3),
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

            TransTexture.ForTrans(targetRt, sourcesTex, transData, 0f, null);
            var target2d = targetRt.CopyTexture2D();

            for (var x = 0; x < 512; x += 1)
            {
                for (var y = 0; y < 512; y += 1)
                {
                    var S = sourcesTex.GetPixel(y, x);
                    var T = target2d.GetPixel(x, y);

                    Assert.That(S == T, Is.True);
                }
            }


        }
    }

}

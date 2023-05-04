#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public static class TextureLayerUtil
{
    public const string PileTextureCSPaht = "Packages/rs64.tex-trans-tool/Runtime/ComputeShaders/PileTexture.compute";

    public enum PileType
    {
        Normal,
        mul,
    }
    public static async Task<Texture2D> PileTexture(Texture2D Base, Texture2D Add, PileType PileType)
    {
        if (Base.width != Add.width && Base.height != Add.height) throw new System.ArgumentException("Textureの解像度が同一ではありません。。");

        var BaesPixels = Base.GetPixels();
        var AddPixels = Add.GetPixels();
        var ResultTexutres = new Texture2D(Base.width, Base.height);
        var ResultPixels = new Color[BaesPixels.Length];

        var PileTasks = new ConfiguredTaskAwaitable<Color>[BaesPixels.Length];

        var indexEnumretor = Enumerable.Range(0, BaesPixels.Length);
        foreach (var Index in indexEnumretor)
        {
            var PileTask = Task.Run<Color>(() => PileColor(BaesPixels[Index], AddPixels[Index], PileType)).ConfigureAwait(false);
            PileTasks[Index] = PileTask;
        }
        foreach (var Index in indexEnumretor)
        {
            ResultPixels[Index] = await PileTasks[Index];
        }

        ResultTexutres.SetPixels(ResultPixels);
        ResultTexutres.Apply();

        return ResultTexutres;
    }
    public static Color PileColor(Color Base, Color Add, PileType pileType)
    {
        switch (pileType)
        {
            default:
            case PileType.Normal:
                {
                    float Alpha = Mathf.Clamp01(Base.a + Add.a);
                    var PileColor = Color.Lerp(Base, Add, Add.a);
                    PileColor.a = Alpha;
                    return PileColor;
                }
            case PileType.mul:
                {
                    return Base * Add;
                }
        }
    }

    public static Texture2D PileTextureUseComputeSheder(ComputeShader CS, Texture2D Base, Texture2D Add, PileType PileType)
    {
        if (Base.width != Add.width && Base.height != Add.height) throw new System.ArgumentException("Textureの解像度が同一ではありません。。");
        if (CS == null) CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(PileTextureCSPaht);

        var BaesPixels = Base.GetPixels();
        var AddPixels = Add.GetPixels();
        var ResultTexutres = new Texture2D(Base.width, Base.height);
        int KarnelId;
        switch (PileType)
        {
            default:
            case PileType.Normal:
                {
                    KarnelId = CS.FindKernel("Normal");
                    break;
                }
            case PileType.mul:
                {
                    KarnelId = CS.FindKernel("Mul");
                    break;
                }
        }

        CS.SetInt("Size", Base.width);

        var BaseTexCB = new ComputeBuffer(BaesPixels.Length, 16);
        var AddTexCB = new ComputeBuffer(BaesPixels.Length, 16);
        BaseTexCB.SetData(BaesPixels);
        AddTexCB.SetData(AddPixels);
        CS.SetBuffer(KarnelId, "BaseTex", BaseTexCB);
        CS.SetBuffer(KarnelId, "AddTex", AddTexCB);

        CS.Dispatch(KarnelId, Base.width / 32, Base.height / 32, 1);

        BaseTexCB.GetData(BaesPixels);

        ResultTexutres.SetPixels(BaesPixels);
        ResultTexutres.Apply();

        BaseTexCB.Release();
        AddTexCB.Release();

        return ResultTexutres;
    }
}

#endif
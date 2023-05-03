#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class TextureLayerUtil
{
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
                    float Alpha = Add.a;
                    Add.a = 1f;
                    return Color.Lerp(Base, Add, Alpha);
                }
            case PileType.mul:
                {
                    return Base * Add;
                }
        }
    }
}

#endif
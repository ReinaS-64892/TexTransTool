using System;
using System.Collections.Generic;

namespace net.rs64.TexTransUnityCore
{
    internal static class FrameMemoEvents
    {
        internal static event Action OnClearMemo;

        [TexTransInitialize]
        internal static void Init()
        {
            TexTransCoreRuntime.Update += () =>
            {
                OnClearMemo?.Invoke();
                OnClearMemo = default;
            };
        }
    }

    public static class Memoize
    {
        private static Dictionary<(object, object), (object, Action)> MemoData = new();

        /// <summary>
        /// 変換関数の結果を一フレームだけ記憶するヘルパーです。同じoriginalとtransformを再度渡せば、計算結果を使いまわします。
        /// </summary>
        /// <param name="original">変換前のデータ</param>
        /// <param name="transform">変換関数</param>
        /// <param name="destroy">変換結果を破棄するデストラクター</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public static U Memo<T, U>(this T original, Func<T, U> transform, Action<U> destroy = null)
        {
            var memoKey = (original, transform);

            if (MemoData.TryGetValue(memoKey, out var value))
            {
                return (U)value.Item1;
            }
            else
            {
                if (MemoData.Count == 0)
                {
                    FrameMemoEvents.OnClearMemo += () =>
                    {
                        foreach (var entry in MemoData)
                        {
                            entry.Value.Item2();
                        }
                        MemoData.Clear();
                    };
                }

                var output = transform(original);
                MemoData[memoKey] = (output, () => destroy?.Invoke(output));
                return output;
            }
        }
    }
}

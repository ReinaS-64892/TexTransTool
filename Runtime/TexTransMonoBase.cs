#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    /*
    TexTransMonoBase を継承した TTT のコンポーネントは一応ほかツールが AddComponent などの連携をできるようにすることを完全に禁止したくないので Public になっています。

    ただし、

    - **API に対する要望は基本的に受け付けない可能性があり、削除された物を復活させてほしいなどの要望はすべて受け付けません。**
    - 実験的なコンポーネント(ITexTransToolStableComponent が実装されていないコンポーネント)については一切保証しません。
    - TTT の実験的ではない(ITexTransToolStableComponent が実装されている)コンポーネントのフィールドに対しては、同一のマイナーバージョン間でのみ保証します。（つまり、マイナーが変わるときには破壊される可能性があります。）
      - v0.x の間は同一マイナーバージョンでも保証されません。（つまり v0.x の間はパッチバージョンでも破壊される可能性があります。）
    - .asmdef などで適切にエラーにならないように使用してください。

    なお TexTransTool にはコンポーネント以外にも Public されている関数や API などが存在しますが、それらははすべて保証されません。
    それらに触れる場合はパッチバージョンレベルで asmdef で指定してください。

    ですが、[TexTransToolStablePublicAPI] が付与された Public API や Public な関数については、特別に、メジャーバージョンが同一な間は保証されます。
    (つまり、メジャーバージョンが変わる場合は破壊される可能性が在ります。ただし、v0.x.x は v1.x.x と同一のものとみなし、v0.x.x にて追加された [TexTransToolStablePublicAPI] の API は v1.x.x の間まで保証します。)


    **もし、これら API 関係で TexTransTool および Reina_Sakiria に不利益を及ばせるものが見受けられた場合、[TexTransToolStablePublicAPI] を除いたすべての API が予告なく internal に戻る可能性があります。**
    */
    [ExecuteInEditMode]
    public abstract class TexTransMonoBase : MonoBehaviour, ITexTransToolTag
    {
        //v0.3.x == 0
        //v0.4.x == 1
        //v0.5.x == 2
        //v0.6.x == 3
        //v0.7.x == 4
        //v0.8.x == 5
        //v0.9.x == 6
        //v0.10.x == 7
        //v1.0.x == 7
        internal const int TTTDataVersion_0_10_X = 7;
        internal const int TTTDataVersion = 7;

        [HideInInspector, SerializeField] int _saveDataVersion = TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;


        internal void OnDestroy() => MonoBehaviourCallProvider.DestroyThis(this);
    }
    [DisallowMultipleComponent]
    public abstract class TexTransMonoBaseGameObjectOwned : TexTransMonoBase { }
    internal static class MonoBehaviourCallProvider
    {
        public static event Action<TexTransMonoBase>? OnDestroy;
        public static void DestroyThis(TexTransMonoBase destroy) => OnDestroy?.Invoke(destroy);
    }

    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class TexTransToolStablePublicAPIAttribute : System.Attribute
    {
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    /*
    TexTransBehavior を継承した TTT のコンポーネントは一応ほかツールが AddComponent などの連携をできるようにすることを完全に禁止したくないので公開されています。

    ただし、

    - これら API に対する要望は基本的に受け付けない可能性があり、削除したものを復活させてほしいなどの要望はすべて受け付けません。
    - これら API　の互換性はパッチバージョン内でしか保証しません。
      - v0.x の間はパッチも保証しません。
      - 実験的機能や実験的なコンポーネントは一切保証しません。
      - .asmdef などで適切にエラーにならないように使用してください。

    **もし、これら API 関係で TTT および Reina_Sakiria に不利益を及ばせるものが見られた場合、これらコンポーネントは予告なく internal に戻します。**
    */

    public abstract class TexTransBehavior : TexTransMonoBase
    {
        internal bool ThisEnable => gameObject.activeInHierarchy;
        internal abstract TexTransPhase PhaseDefine { get; }
        internal const string TTTName = "TexTransTool";
    }

    public enum TexTransPhase
    {
        BeforeUVModification = 1,
        UVModification = 2,
        AfterUVModification = 3,
        UnDefined = 0,
        Optimizing = 4,
    }
}

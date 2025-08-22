#nullable enable
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using System;
using System.Reflection;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    /*
    こういうケースに当たる場合はこのインターフェイスを実装する前に TTT の作者 (Reina_Sakiria) に相談してください。

    - フォトショップやクリスタなどの外部ペイントツールの再現を行う

    これらケースの場合は TTT PSD Importer などにもインポートが可能になるべきで、外部ツールとして実装せず
    TexTransTool 、また TexTransCore に対して PullRequest として、 コントリビューションをお持ちしております！
    */

    // [TexTransToolStablePublicAPI] MLIC が stable になったら付与される予定
    // レイヤーとして振る舞える外部ツールに共通して実装されるインターフェイス。
    // レイヤーとして立ち振る舞うためには AsImageLayer か AsGrabLayer のどちらかを実装してね
    // 両方実装したときの振る舞いは未定義です。未定義動作を踏みたくない場合はどちらか片方のみを実装してください。
    public interface IExternalToolCanBehaveAsLayer
    {
        /*
            これら TTT MLIC のレイヤーとして立ち振る舞うときには下記のような立ち振舞が要求されます。

            ExternalToolAsLayer (FullName:net.rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer)
            がコンポーネントが同じ GameObject に存在するとき

            - そのツールの元々の振る舞いをすべて止める必要があります。
                - 止めなかった場合、二重適用などが発生してしまい予期せぬ動作を発生させます！

            - (optional) ツールにもよりますが対象となるテクスチャなどのプロパティを UI からグレーアウト or 非表示にしたほうが良いです。
                - 行わなくても問題はないですが、対象は MLIC が持つため、無意味なプロパティとなってしまいます。
                - ユーザーに混乱を招くかもしれないので、親切にするならば 無効になったかどうかを適切に表示しましょう。

            - (optional) MultiLayerImageCanvas の配下にいた場合に、ExternalToolAsLayer を追加するボタンなどを表示しましょう。
                - できると、ユーザーにとっては便利です！

        */
    }

    // 画像を出力するだけのコンポーネントならば
    // [TexTransToolStablePublicAPI]
    public interface IExternalToolCanBehaveAsImageLayerV1 : IExternalToolCanBehaveAsLayer
    {
        void LoadImage(RenderTexture writeDistentionTexture);
    }
    // 特殊な色変換などを行いたい場合に
    // [TexTransToolStablePublicAPI]
    public interface IExternalToolCanBehaveAsGrabLayerV1 : IExternalToolCanBehaveAsLayer
    {
        void GrabBlending(RenderTexture readWiteCanvasTexture);
    }


    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    [RequireComponent(typeof(IExternalToolCanBehaveAsLayer))]// これ意味がないっぽい ... えぇ そんな
    /*
        上の Interface に [TexTransToolStablePublicAPI] が付与されたとき、このコンポーネントは

        `net.rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer, net.rs64.tex-trans-tool.runtime`

        という名前空間とアセンブリ名と名前から変更されなくなり、型名は安全に使用できます。
    */
    public sealed class ExternalToolAsLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT " + nameof(ExternalToolAsLayer);
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            var lm = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;

            domain.Observe(this);
            domain.Observe(gameObject);
            domain.ObserveToChildeComponents<MonoBehaviour>(gameObject);//治安悪いが ... Interface が NDMF の都合でねじ込めないので仕方がない ...
            var externalToolComponent = gameObject.GetComponent<IExternalToolCanBehaveAsLayer>();

            if (engine is not TTCEUnity)
            {
                TTLog.Warning("MultiLayerImage:warning:ExternalToolAsLayerIsRunOnlyOnUnityBackend");
                return new EmptyLayer<ITexTransToolForUnity>(Visible, lm, alphaOperator, Clipping, blKey);
            }
            if (externalToolComponent == null || externalToolComponent is not MonoBehaviour)
            {
                TTLog.Warning("MultiLayerImage:warning:ExternalToolComponentNotFound");
                return new EmptyLayer<ITexTransToolForUnity>(Visible, lm, alphaOperator, Clipping, blKey);
            }

            domain.Observe((MonoBehaviour)externalToolComponent);

            switch (externalToolComponent)
            {
                case IExternalToolCanBehaveAsImageLayerV1 imageLayerV1:
                    {
                        return new ExternalToolImageLayerAsImageLayer<ITexTransToolForUnity>(Visible, lm, alphaOperator, Clipping, blKey, imageLayerV1);
                    }
                case IExternalToolCanBehaveAsGrabLayerV1 grabLayerV1:
                    {
                        var wrapper = new ExternalToolGrabBlendAsGrabBlending(grabLayerV1);
                        return new GrabBlendingAsLayer<ITexTransToolForUnity>(Visible, lm, Clipping, blKey, wrapper);
                    }
                default:
                    {
                        TTLog.Warning("MultiLayerImage:warning:ExternalToolIsNotImplement");
                        return new EmptyLayer<ITexTransToolForUnity>(Visible, lm, alphaOperator, Clipping, blKey);
                    }
            }
        }
        class ExternalToolImageLayerAsImageLayer<TTCE> : ImageLayer<TTCE>
        where TTCE : ITexTransCreateTexture
            , ITexTransLoadTexture
            , ITexTransCopyRenderTexture
            , ITexTransComputeKeyQuery
            , ITexTransGetComputeHandler
            , ITexTransDriveStorageBufferHolder
        {
            private readonly IExternalToolCanBehaveAsImageLayerV1 externalToolImageLayer;

            public ExternalToolImageLayerAsImageLayer(bool visible, AlphaMask<TTCE> alphaMask, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, IExternalToolCanBehaveAsImageLayerV1 externalToolImageLayer) : base(visible, alphaMask, alphaOperation, preBlendToLayerBelow, blendTypeKey)
            {
                this.externalToolImageLayer = externalToolImageLayer;
            }

            public override void GetImage(TTCE engine, ITTRenderTexture writeTarget)
            {
                var ttceUnity = engine as TTCEUnity;
                if (ttceUnity is null) { return; }// unsupported

                if (writeTarget is not UnityRenderTexture urt) { return; }
                externalToolImageLayer.LoadImage(urt.RenderTexture);
            }
        }
        class ExternalToolGrabBlendAsGrabBlending : ITTGrabBlending
        {
            private readonly IExternalToolCanBehaveAsGrabLayerV1 externalTool;

            public ExternalToolGrabBlendAsGrabBlending(IExternalToolCanBehaveAsGrabLayerV1 externalTool)
            {
                this.externalTool = externalTool;
            }
            public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture)
            where TTCE : ITexTransCreateTexture
            , ITexTransComputeKeyQuery
            , ITexTransGetComputeHandler
            , ITexTransDriveStorageBufferHolder
            {
                var ttceUnity = engine as TTCEUnity;
                if (ttceUnity is null) { return; }// unsupported

                if (grabTexture is not UnityRenderTexture urt) { return; }
                externalTool.GrabBlending(urt.RenderTexture);
            }
        }
    }
}

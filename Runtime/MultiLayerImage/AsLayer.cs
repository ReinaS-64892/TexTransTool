#nullable enable
using System;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal interface ICanBehaveAsLayer
    {
        LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx, AsLayer asLayer);
        bool HaveBlendTypeKey => false;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ICanBehaveAsLayer))]
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class AsLayer : TexTransMonoBase, IMultiLayerImageCanvasLayer
    {
        internal const string ComponentName = "TTT AsLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;


        internal bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        [SerializeReference][SubclassSelector] public ILayerMask? LayerMask = new LayerMask();

        LayerObject<ITexTransToolForUnity> IMultiLayerImageCanvasLayer.GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;
            domain.LookAt(this);
            domain.LookAt(gameObject);

            var cdc = GetComponent<ICanBehaveAsLayer>();

            if (cdc != null) { return cdc.GetLayerObject(ctx, this); }
            else
            {
                var alphaMask = GetAlphaMaskObject(ctx);
                var blKey = engine.QueryBlendKey(BlendTypeKey);
                return new EmptyLayer<ITexTransToolForUnity>(Visible, alphaMask, AlphaOperation.Normal, Clipping, blKey);
            }
        }

        internal AlphaMask<ITexTransToolForUnity> GetAlphaMaskObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            Func<UnityEngine.Object, ILayerMask?> getLayerMask = o => (o as AsLayer)!.LayerMask;
            var lm = domain.LookAtGet(this, getLayerMask);

            var innerMask = lm?.GetAlphaMaskObject(ctx, this, getLayerMask);

            if (innerMask is not null) return new MaskAndSolid<ITexTransToolForUnity>(innerMask, Opacity);
            else return new SolidToMask<ITexTransToolForUnity>(Opacity);
        }
    }
}

#nullable enable
using System;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using System.Collections.Generic;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal class GenerateLayerObjectContext
    {
        public readonly IDomain Domain;
        public readonly ITexTransToolForUnity Engine;
        public readonly (int x, int y) CanvasSize;
        public HashSet<Material>? TargetContainedMaterials;

        public GenerateLayerObjectContext(IDomain domain, (int x, int y) canvasSize, HashSet<Material>? targetContainedMaterials = null)
        {
            Domain = domain;
            Engine = Domain.GetTexTransCoreEngineForUnity();
            CanvasSize = canvasSize;
            TargetContainedMaterials = targetContainedMaterials;
        }
    }
    internal interface IMultiLayerImageCanvasLayer
    {
        LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx);
    }
    public abstract class AbstractLayer : TexTransMonoBaseGameObjectOwned, IMultiLayerImageCanvasLayer
    {
        internal bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        [SerializeReference][SubclassSelector] public ILayerMask? LayerMask = new LayerMask();

        internal abstract LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx);
        LayerObject<ITexTransToolForUnity> IMultiLayerImageCanvasLayer.GetLayerObject(GenerateLayerObjectContext ctx) => GetLayerObject(ctx);

        internal virtual AlphaMask<ITexTransToolForUnity> GetAlphaMaskObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            Func<UnityEngine.Object, ILayerMask?> getLayerMask = o => (o as AbstractLayer)!.LayerMask;
            var lm = domain.ObserveToGet(this, getLayerMask);

            var innerMask = lm?.GetAlphaMaskObject(ctx, this, getLayerMask);

            if (innerMask is not null) return new MaskAndSolid<ITexTransToolForUnity>(innerMask, Opacity);
            else return new SolidToMask<ITexTransToolForUnity>(Opacity);
        }

    }
    public interface ILayerMask
    {
        internal AlphaMask<ITexTransToolForUnity> GetAlphaMaskObject(GenerateLayerObjectContext ctx, UnityEngine.Object thisObj, Func<UnityEngine.Object, ILayerMask?> getThisToLayerMask);
    }

    [Serializable]
    public class LayerMask : ILayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D? MaskTexture;

        AlphaMask<ITexTransToolForUnity> ILayerMask.GetAlphaMaskObject(GenerateLayerObjectContext ctx, UnityEngine.Object thisObj, Func<UnityEngine.Object, ILayerMask?> getThisToLayerMask)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            var thisLayerMask = domain.ObserveToGet(thisObj, o => getThisToLayerMask(o) as LayerMask);
            if (thisLayerMask is null) { return new NoMask<ITexTransToolForUnity>(); }

            var layerMaskDisabled = domain.ObserveToGet(thisObj, o => (getThisToLayerMask(o) as LayerMask)!.LayerMaskDisabled);
            if (layerMaskDisabled) { return new NoMask<ITexTransToolForUnity>(); }

            var maskTexture = domain.ObserveToGet(thisObj, o => (getThisToLayerMask(o) as LayerMask)!.MaskTexture);
            if (maskTexture == null) { return new NoMask<ITexTransToolForUnity>(); }

            domain.Observe(maskTexture);
            return new DiskOnlyToMask<ITexTransToolForUnity>(engine.Wrapping(maskTexture!));
        }
    }

}

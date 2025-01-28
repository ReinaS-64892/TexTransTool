#nullable enable
using System;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    internal interface IMultiLayerImageCanvasLayer
    {
        LayerObject<ITexTransToolForUnity> GetLayerObject(IDomain domain, ITexTransToolForUnity engine);
    }
    public abstract class AbstractLayer : TexTransMonoBaseGameObjectOwned, IMultiLayerImageCanvasLayer
    {
        internal bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        [SerializeReference] public ILayerMask? LayerMask = new LayerMask();

        internal abstract LayerObject<ITexTransToolForUnity> GetLayerObject(IDomain domain, ITexTransToolForUnity engine);
        LayerObject<ITexTransToolForUnity> IMultiLayerImageCanvasLayer.GetLayerObject(IDomain domain, ITexTransToolForUnity engine) => GetLayerObject(domain, engine);

        internal virtual AlphaMask<ITexTransToolForUnity> GetAlphaMask(IDomain domain, ITexTransToolForUnity engine)
        {
            Func<UnityEngine.Object, ILayerMask?> getLayerMask = o => (o as AbstractLayer)!.LayerMask;
            var lm = domain.LookAtGet(this, getLayerMask);

            var innerMask = lm?.GetAlphaMask(domain, engine, this, getLayerMask);

            if (innerMask is not null) return new MaskAndSolid<ITexTransToolForUnity>(innerMask, Opacity);
            else return new SolidToMask<ITexTransToolForUnity>(Opacity);
        }

    }
    public interface ILayerMask
    {
        internal AlphaMask<ITexTransToolForUnity> GetAlphaMask(IDomain domain, ITexTransToolForUnity engine, UnityEngine.Object thisObj, Func<UnityEngine.Object, ILayerMask?> getThisToLayerMask);
    }

    [Serializable]
    public class LayerMask : ILayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D? MaskTexture;

        AlphaMask<ITexTransToolForUnity> ILayerMask.GetAlphaMask(IDomain domain, ITexTransToolForUnity engine, UnityEngine.Object thisObj, Func<UnityEngine.Object, ILayerMask?> getThisToLayerMask)
        {
            var thisLayerMask = domain.LookAtGet(thisObj, o => getThisToLayerMask(o) as LayerMask);
            if (thisLayerMask is null) { return new NoMask<ITexTransToolForUnity>(); }

            var layerMaskDisabled = domain.LookAtGet(thisObj, o => (getThisToLayerMask(o) as LayerMask)!.LayerMaskDisabled);
            if (layerMaskDisabled) { return new NoMask<ITexTransToolForUnity>(); }

            var maskTexture = domain.LookAtGet(thisObj, o => (getThisToLayerMask(o) as LayerMask)!.MaskTexture);
            if (maskTexture == null) { return new NoMask<ITexTransToolForUnity>(); }

            domain.LookAt(maskTexture);
            return new DiskOnlyToMask<ITexTransToolForUnity>(engine.Wrapping(maskTexture!));
        }
    }

}

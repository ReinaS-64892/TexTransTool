#nullable enable
using System;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [DisallowMultipleComponent]
    public abstract class AbstractLayer : MonoBehaviour, ITexTransToolTag
    {
        internal bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;
        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        [SerializeReference] public ILayerMask LayerMask = new LayerMask();

        internal abstract TexTransCore.MultiLayerImageCanvas.LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler;

        internal virtual TexTransCore.MultiLayerImageCanvas.AlphaMask<TTCE4U> GetAlphaMask<TTCE4U>(TTCE4U engine)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            switch (LayerMask)
            {
                default: { return new TexTransCore.MultiLayerImageCanvas.SolidToMask<TTCE4U>(Opacity); }
                case TTTImportedLayerMask importedLayerMask:
                    {
                        if (importedLayerMask.ContainedMask is false) { return new TexTransCore.MultiLayerImageCanvas.SolidToMask<TTCE4U>(Opacity); }
                        var importedDiskTex = engine.Wrapping(importedLayerMask.MaskTexture);
                        return new TexTransCore.MultiLayerImageCanvas.DiskToMask<TTCE4U>(importedDiskTex, Opacity);
                    }
                case MultiLayerImage.LayerMask layerMask:
                    {
                        if (layerMask.ContainedMask is false) { return new TexTransCore.MultiLayerImageCanvas.SolidToMask<TTCE4U>(Opacity); }
                        var importedDiskTex = engine.Wrapping(layerMask.MaskTexture!);
                        return new TexTransCore.MultiLayerImageCanvas.DiskToMask<TTCE4U>(importedDiskTex, Opacity);
                    }
            }
        }
        internal virtual void LookAtCalling(ILookingObject lookingObject)
        {
            lookingObject.LookAt(gameObject);
            lookingObject.LookAt(this);
            LayerMask.LookAtCalling(lookingObject);
        }
    }
    [Serializable]
    public class LayerMask : ILayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D? MaskTexture;

        public bool ContainedMask => LayerMaskDisabled is false && MaskTexture != null;

        public void LookAtCalling(ILookingObject lookingObject) { if (MaskTexture != null) lookingObject.LookAt(MaskTexture); }

        public void WriteMaskTexture<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            if (MaskTexture == null) { throw new InvalidOperationException(); }
            using var lm = engine.Wrapping(MaskTexture);
            engine.LoadTextureWidthAnySize(renderTexture, lm);
        }
    }

    public interface ILayerMask
    {
        bool ContainedMask { get; }
        void LookAtCalling(ILookingObject lookingObject);
    }


}

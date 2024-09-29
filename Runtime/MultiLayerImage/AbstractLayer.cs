using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

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
        internal abstract void EvaluateTexture(CanvasContext canvasContext);

        internal virtual LayerAlphaMod GetLayerAlphaMod(CanvasContext canvasContext)
        {
            if (LayerMask.ContainedMask)
            {
                var rt = TTRt.G(canvasContext.CanvasSize, canvasContext.CanvasSize, true);
                rt.name = $"GetLayerAlphaMod.TempRt-{rt.width}x{rt.height}";

                LayerMask.WriteMaskTexture(rt, canvasContext.TextureManager);
                return new LayerAlphaMod(rt, Opacity);
            }
            else
            {
                return new LayerAlphaMod(null, Opacity);
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
        public Texture2D MaskTexture;

        public bool ContainedMask => !LayerMaskDisabled && MaskTexture != null;

        public void LookAtCalling(ILookingObject lookingObject) { lookingObject.LookAt(MaskTexture); }

        void ILayerMask.WriteMaskTexture(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            originTexture.WriteOriginalTexture(MaskTexture, renderTexture);
        }
    }

    public interface ILayerMask
    {
        bool ContainedMask { get; }
        void WriteMaskTexture(RenderTexture renderTexture, IOriginTexture originTexture);
        void LookAtCalling(ILookingObject lookingObject);
    }


}

using System;
using System.Collections.Generic;
using net.rs64.TexTransUnityCore;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransUnityCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
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

        internal abstract TexTransCore.MultiLayerImageCanvas.LayerObject GetLayerObject(ITextureManager textureManager);
        internal virtual TexTransCore.MultiLayerImageCanvas.AlphaMask GetAlphaMask(ITextureManager textureManager)
        { return new LayerAlphaMod(textureManager, Opacity, LayerMask); }
        class LayerAlphaMod : TexTransCore.MultiLayerImageCanvas.AlphaMask
        {
            private ITextureManager _textureManager;
            float _opacity;
            ILayerMask _layerMask;
            public LayerAlphaMod(ITextureManager textureManager, float opacity, ILayerMask layerMask)
            {
                _textureManager = textureManager;
                _opacity = opacity;
                _layerMask = layerMask;
            }
            public override void Masking(ITTEngine engine, ITTRenderTexture maskTarget)
            {
                engine.MulAlpha(maskTarget, _opacity);

                if (_layerMask.ContainedMask)
                    using (TTRt.U(out var rt, maskTarget.Width, maskTarget.Hight))
                    {
                        _layerMask.WriteMaskTexture(rt, _textureManager);
                        TextureBlend.MaskDrawRenderTexture(maskTarget.ToUnity(), rt);
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

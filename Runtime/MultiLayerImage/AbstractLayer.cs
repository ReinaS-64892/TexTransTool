using System;
using net.rs64.TexTransCoreEngineForUnity;
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

        internal abstract TexTransCore.MultiLayerImageCanvas.LayerObject<TTT4U> GetLayerObject<TTT4U>(TTT4U engine)
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending;

        internal virtual TexTransCore.MultiLayerImageCanvas.AlphaMask<TTCE4U> GetAlphaMask<TTCE4U>(TTCE4U engine)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending
        { return new LayerAlphaMod<TTCE4U>(engine, Opacity, LayerMask); }

        class LayerAlphaMod<TTT4U> : TexTransCore.MultiLayerImageCanvas.AlphaMask<TTT4U>
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        {
            private TTT4U _ttt4u;
            float _opacity;
            ILayerMask _layerMask;
            public LayerAlphaMod(TTT4U forUnity, float opacity, ILayerMask layerMask)
            {
                _ttt4u = forUnity;
                _opacity = opacity;
                _layerMask = layerMask;
            }
            public override void Masking(TTT4U engine, ITTRenderTexture maskTarget)
            {
                System.Diagnostics.Debug.Assert(_ttt4u.Equals(engine));

                engine.MulAlpha(maskTarget, _opacity);

                if (_layerMask.ContainedMask)
                    using (var rt = _ttt4u.CreateRenderTexture(maskTarget.Width, maskTarget.Hight))
                    {
                        _layerMask.WriteMaskTexture(_ttt4u, rt);
                        _ttt4u.MulAlpha(maskTarget, rt);
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

        public void WriteMaskTexture<TTT4U>(TTT4U engine, ITTRenderTexture renderTexture)
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        {
            engine.LoadTextureWidthAnySize(engine.Wrapping(MaskTexture), renderTexture);
        }
    }

    public interface ILayerMask
    {
        bool ContainedMask { get; }
        void WriteMaskTexture<TTT4U>(TTT4U texTransCoreEngine, ITTRenderTexture renderTexture)
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler;

        void LookAtCalling(ILookingObject lookingObject);
    }


}

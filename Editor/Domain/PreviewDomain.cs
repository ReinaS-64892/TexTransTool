#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;

namespace net.rs64.TexTransTool
{
    public class PreviewDomain : IDomain, IDisposable
    {
        [SerializeField] List<Renderer> _renderers;
        [SerializeField] TextureStacks _textureStacks = new TextureStacks();

        [SerializeField] RenderersBackup _renderersBackup;

        public PreviewDomain(List<Renderer> previewRenderers)
        {
            _renderers = previewRenderers;
            _renderersBackup = new RenderersBackup(previewRenderers);
        }
        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
        {
            _textureStacks.AddTextureStack(Dist, SetTex);
        }

        public void SetMaterial(Material Target, Material SetMat, bool isPaired)
        {
            RendererUtility.ChangeMaterialForRenderers(_renderers, Target, SetMat);
        }

        public void transferAsset(UnityEngine.Object Asset)
        {
            //なにもしなくていい
        }
        public void SetTexture(Texture2D Target, Texture2D SetTex)
        {
            RendererUtility.SetTexture(_renderers, Target, SetTex);
        }
        public void EditFinish()
        {
            foreach (var MargeResult in _textureStacks.MargeStacks())
            {
                SetTexture(MargeResult.FirstTexture, MargeResult.MargeTexture);
            }
        }

        public void Dispose()
        {
            _renderersBackup.Dispose();
        }

    }
}
#endif
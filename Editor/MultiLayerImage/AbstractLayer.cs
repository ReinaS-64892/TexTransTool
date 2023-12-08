#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [DisallowMultipleComponent]
    internal abstract class AbstractLayer : MonoBehaviour, ITexTransToolTag
    {
        public bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        [BlendTypeKey]public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        public LayerMask LayerMask;
        public abstract void EvaluateTexture(CanvasContext layerStack);



    }
    [Serializable]
    internal class LayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D MaskTexture;
    }


}
#endif
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
    public abstract class AbstractLayer : MonoBehaviour, ITexTransToolTag
    {
        internal bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;
        [Range(0, 1)] public float Opacity = 1;
        [SerializeField]internal bool Clipping;
        [BlendTypeKey]public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        [SerializeField]internal LayerMask LayerMask;
        internal abstract void EvaluateTexture(CanvasContext layerStack);



    }
    [Serializable]
    internal class LayerMask
    {
        [SerializeField]internal bool LayerMaskDisabled;
        [SerializeField]internal Texture2D MaskTexture;
    }


}
#endif
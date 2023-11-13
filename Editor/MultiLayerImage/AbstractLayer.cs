#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
using LayerMaskData = net.rs64.MultiLayerImageParser.LayerData.LayerMaskData;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [DisallowMultipleComponent]
    public abstract class AbstractLayer : MonoBehaviour, ITexTransToolTag
    {
        public bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        public BlendType BlendMode;
        public LayerMask LayerMask;
        public abstract void EvaluateTexture(CanvasContext layerStack);



    }
    [Serializable]
    public class LayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D MaskTexture;
    }


}
#endif
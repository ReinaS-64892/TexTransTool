using System;
using net.rs64.TexTransCore;


namespace net.rs64.TexTransTool.MultiLayerImage.LayerData
{
    public class PhotoshopGradationMapLayerData : AbstractLayerData, IGrabTag
    {
        public bool IsGradientReversed;
        public bool IsGradientDithered;
        public GradientInteropMethod InteropMethod;
        public float Smoothens;
        public ColorKey[] ColorKeys;
        public TransparencyKey[] TransparencyKeys;
    }
    public struct ColorKey
    {
        public float KeyLocation;
        public float MidLocation;
        public ColorWOAlpha Color;
    }
    public struct TransparencyKey
    {
        public float KeyLocation;
        public float MidLocation;
        public float Transparency;
    }
    public enum GradientInteropMethod
    {
        Classic,
        Perceptual,
        Linear,
        Smooth,
        Stripes,
    }

}

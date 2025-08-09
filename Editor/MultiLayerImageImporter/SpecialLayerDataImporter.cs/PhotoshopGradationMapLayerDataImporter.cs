using System;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using System.Linq;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [SpecialDataOf(typeof(PhotoshopGradationMapLayerData))]
    public class PhotoshopGradationMapLayerDataImporter : ISpecialLayerDataImporter
    {
        void ISpecialLayerDataImporter.CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData)
        {
            var photoshopGradationMapLayerData = specialData as PhotoshopGradationMapLayerData;
            var photoshopGradationMapLayer = newLayer.AddComponent<PhotoshopGradationMapLayer>();
            copyFromData(photoshopGradationMapLayer, photoshopGradationMapLayerData);

            photoshopGradationMapLayer.IsGradientReversed = photoshopGradationMapLayerData.IsGradientReversed;
            photoshopGradationMapLayer.IsGradientDithered = photoshopGradationMapLayerData.IsGradientDithered;
            photoshopGradationMapLayer.GradientInteropMethodKey = photoshopGradationMapLayerData.GradientInteropMethodKey;
            photoshopGradationMapLayer.Smoothens = photoshopGradationMapLayerData.Smoothens;
            photoshopGradationMapLayer.ColorKeys = photoshopGradationMapLayerData.ColorKeys.Select(
                c => new PhotoshopGradationMapLayer.ColorKey()
                {
                    KeyLocation = c.KeyLocation,
                    MidLocation = c.MidLocation,
                    Color = c.Color.ToUnity(),
                }
            ).ToArray();
            photoshopGradationMapLayer.TransparencyKeys = photoshopGradationMapLayerData.TransparencyKeys.Select(
                c => new PhotoshopGradationMapLayer.TransparencyKey()
                {
                    KeyLocation = c.KeyLocation,
                    MidLocation = c.MidLocation,
                    Transparency = c.Transparency,
                }
            ).ToArray();
        }
    }
}

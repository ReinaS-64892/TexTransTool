using System;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [SpecialDataOf(typeof(SelectiveColorLayerData))]
    public class SelectiveColorLayerDataImporter : ISpecialLayerDataImporter
    {
        void ISpecialLayerDataImporter.CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData)
        {
            var selectiveColorLayerData = specialData as SelectiveColorLayerData;
            var selectiveColoringAdjustmentLayer = newLayer.AddComponent<SelectiveColoringAdjustmentLayer>();
            copyFromData(selectiveColoringAdjustmentLayer, selectiveColorLayerData);

            selectiveColoringAdjustmentLayer.RedsCMYK = selectiveColorLayerData.RedsCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.YellowsCMYK = selectiveColorLayerData.YellowsCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.GreensCMYK = selectiveColorLayerData.GreensCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.CyansCMYK = selectiveColorLayerData.CyansCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.BluesCMYK = selectiveColorLayerData.BluesCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.MagentasCMYK = selectiveColorLayerData.MagentasCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.WhitesCMYK = selectiveColorLayerData.WhitesCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.NeutralsCMYK = selectiveColorLayerData.NeutralsCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.BlacksCMYK = selectiveColorLayerData.BlacksCMYK.ToUnity();
            selectiveColoringAdjustmentLayer.IsAbsolute = selectiveColorLayerData.IsAbsolute;
        }
    }
}

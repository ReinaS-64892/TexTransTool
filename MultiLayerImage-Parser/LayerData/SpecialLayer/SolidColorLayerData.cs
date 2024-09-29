using UnityEngine;

namespace net.rs64.MultiLayerImage.LayerData
{
    internal class SolidColorLayerData : AbstractLayerData
    {
        [ColorUsage(false)] public Color Color;
    }
}

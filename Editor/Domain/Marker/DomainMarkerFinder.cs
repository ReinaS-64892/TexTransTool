#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class DomainMarkerFinder
    {
        static List<IDomainMarkerFinder> Finders;
        public static GameObject FindMarker(GameObject StartPoint)
        {
            if (Finders == null) { Finders = InterfaceUtility.GetInterfaceInstance<IDomainMarkerFinder>(); }
            foreach (var finder in Finders)
            {
                var marker = finder.FindMarker(StartPoint);
                if (marker != null) { return marker; }
            }
            return null;
        }
    }

    internal interface IDomainMarkerFinder
    {
        GameObject FindMarker(GameObject StartPoint);
    }
}
#endif
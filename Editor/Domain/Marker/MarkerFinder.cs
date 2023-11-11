#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public static class MarkerFinder
    {
        static List<IMarkerFinder> Finders;
        public static GameObject FindMarker(GameObject StartPoint)
        {
            if (Finders == null) { Finders = InterfaceUtility.GetInterfaceInstance<IMarkerFinder>(); }
            foreach (var finder in Finders)
            {
                var marker = finder.FindMarker(StartPoint);
                if (marker != null) { return marker; }
            }
            return null;
        }
    }

    public interface IMarkerFinder
    {
        GameObject FindMarker(GameObject StartPoint);
    }
}
#endif
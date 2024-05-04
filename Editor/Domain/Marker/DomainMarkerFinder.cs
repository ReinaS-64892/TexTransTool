using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class DomainMarkerFinder
    {
        static List<IDomainMarkerFinder> s_finders;
        public static GameObject FindMarker(GameObject StartPoint)
        {
            if (s_finders == null) { s_finders = InterfaceUtility.GetInterfaceInstance<IDomainMarkerFinder>().ToList(); }
            foreach (var finder in s_finders)
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

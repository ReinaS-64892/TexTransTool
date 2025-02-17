using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class TTTDomainDefinitionFinder : IDomainMarkerFinder
    {
        public GameObject FindMarker(GameObject startPoint)
        {
            return startPoint.GetComponentInParent<DomainDefinition>(true)?.gameObject;
        }
    }
}

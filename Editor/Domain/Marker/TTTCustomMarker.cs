#if UNITY_EDITOR
using UnityEngine;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    internal class TTTCustomMarker : MonoBehaviour, ITexTransToolTag
    {
        [SerializeField, HideInInspector] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        public int SaveDataVersion => _saveDataVersion;
    }
    internal class TTTCustomMarkerFinder : IDomainMarkerFinder
    {
        public GameObject FindMarker(GameObject startPoint)
        {
            return startPoint.GetComponentInParent<TTTCustomMarker>()?.gameObject;
        }
    }
}
#endif
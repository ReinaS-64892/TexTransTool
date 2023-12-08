#if UNITY_EDITOR
using UnityEngine;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    internal class TTTCustomMarker : MonoBehaviour, ITexTransToolTag
    {
        [SerializeField, HideInInspector] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
    }
    internal class TTTCustomMarkerFinder : IDomainMarkerFinder
    {
        public GameObject FindMarker(GameObject StartPoint)
        {
            return StartPoint.GetComponentInParent<TTTCustomMarker>()?.gameObject;
        }
    }
}
#endif
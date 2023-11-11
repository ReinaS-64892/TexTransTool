#if UNITY_EDITOR
using UnityEngine;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    public class TTTCustomMarker : MonoBehaviour, ITexTransToolTag
    {
        [SerializeField, HideInInspector] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
    }
    public class TTTCustomMarkerFinder : IMarkerFinder
    {
        public GameObject FindMarker(GameObject StartPoint)
        {
            return StartPoint.GetComponentInParent<TTTCustomMarker>()?.gameObject;
        }
    }
}
#endif
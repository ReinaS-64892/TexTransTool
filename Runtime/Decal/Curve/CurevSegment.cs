#if UNITY_EDITOR
using UnityEngine;
#if VRC_BASE
using VRC.SDKBase;
#endif


namespace net.rs64.TexTransTool.Decal.Curve
{
    [AddComponentMenu("TexTransTool/Experimental/CurveSegment")]
    public class CurveSegment : MonoBehaviour, ITexTransToolTag
#if VRC_BASE
    , IEditorOnly
#endif
    {
        [HideInInspector,SerializeField] int _saveDataVersion = Utils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public Vector3 position => transform.position;
        public float Roll = 0f;

    }
}
#endif

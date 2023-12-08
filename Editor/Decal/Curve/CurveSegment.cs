#if UNITY_EDITOR
using net.rs64.TexTransTool.Utils;
using UnityEngine;


namespace net.rs64.TexTransTool.Decal.Curve
{
    [AddComponentMenu("TexTransTool/Experimental/TTT CurveSegment")]
    internal class CurveSegment : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector,SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public Vector3 position => transform.position;
        public float Roll = 0f;

    }
}
#endif

using UnityEngine;
#if VRC_BASE
using VRC.SDKBase;
#endif


namespace Rs64.TexTransTool.Decal.Curve
{
    [AddComponentMenu("TexTransTool/Experimental/CurevSegment")]
    public class CurevSegment : MonoBehaviour , ITexTransToolTag
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public Vector3 position => transform.position;
        public float Rool = 0f;

    }
}
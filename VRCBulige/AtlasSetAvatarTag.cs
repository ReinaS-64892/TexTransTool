#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Rs.TexturAtlasCompiler.VRCBulige
{

    public class AtlasSetAvatarTag : MonoBehaviour, IEditorOnly
    {
        public AtlasSet AtlasSet;
        public ExecuteClient ClientSelect;
    }
}
#endif
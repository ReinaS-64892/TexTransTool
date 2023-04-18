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

        public List<AtlasPostPrces> PostPrcess;
    }
    [System.Serializable]
    public class AtlasPostPrces
    {
        public ProcesEnum ProcesType;
        public string TargetPropatyName;
        public string ProsesValue;

        public enum ProcesEnum
        {
            TextureResize,
        }
    }
}
#endif
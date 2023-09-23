#if UNITY_EDITOR
using UnityEngine;
using System;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Build
{
    [AddComponentMenu("TexTransTool/TTT PhaseDefinition")]
    public class PhaseDefinition : TexTransGroup
    {
        public TexTransPhase TexTransPhase;
    }
}
#endif

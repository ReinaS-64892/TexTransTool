using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + PDMenuPath)]
    public sealed class PhaseDefinition : TexTransGroup
    {
        internal const string PDName = "TTT PhaseDefinition";
        internal const string PDMenuPath = TexTransGroup.FoldoutName + "/" + PDName;
        public TexTransPhase TexTransPhase;
    }
}

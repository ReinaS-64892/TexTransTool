#nullable enable
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + PDMenuPath)]
    public sealed class PhaseDefinition : TexTransMonoBaseGameObjectOwned , ITexTransToolStableComponent
    {
        internal const string PDName = "TTT PhaseDefinition";
        internal const string FoldoutName = "Group";
        internal const string PDMenuPath = FoldoutName + "/" + PDName;
        public int StabilizeSaveDataVersion => TTTDataVersion_0_10_X;
        public TexTransPhase TexTransPhase;
    }
}

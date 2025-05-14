using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class DomainDefinition : TexTransAnnotation
    {
        internal const string Name = "TTT DomainDefinition";
        internal const string MenuPath = PhaseDefinition.FoldoutName + "/" + Name;
    }
}

using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class IsActiveInheritBreaker : TexTransAnnotation
    {
        internal const string Name = "TTT IsActiveInheritBreaker";
        internal const string MenuPath = TexTransGroup.FoldoutName + "/" + Name;
    }
}

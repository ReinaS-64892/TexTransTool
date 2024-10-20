using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class IsActiveInheritBreaker : TexTransAnnotation, IActivenessChanger
    {
        internal const string Name = "TTT IsActiveInheritBreaker";
        internal const string MenuPath = TexTransGroup.FoldoutName + "/" + Name;
        public bool IsActive => this.gameObject.activeSelf;
    }
}

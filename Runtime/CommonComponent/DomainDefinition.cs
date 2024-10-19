using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class DomainDefinition : TexTransAnnotation, IActivenessChanger
    {
        internal const string Name = "TTT DomainDefinition";
        internal const string MenuPath = TexTransGroup.FoldoutName + "/" + Name;

        public bool IsActive => true;
    }
}

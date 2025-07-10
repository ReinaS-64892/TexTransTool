using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class ModificationOnlyTag : TexTransAnnotation
    {
        internal const string ComponentName = "TTT ModificationOnlyTag";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
    }
}

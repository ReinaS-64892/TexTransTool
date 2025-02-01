using UnityEditor;
using net.rs64.TexTransTool.IslandSelector;
namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AbstractIslandSelector), true)]
    internal class AbstractIslandSelectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if ((target as AbstractIslandSelector)?.IsExperimental ?? true) TextureTransformerEditor.DrawerWarning(target.GetType().Name);
            base.OnInspectorGUI();
        }
    }
}

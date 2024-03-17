using UnityEditor;
using net.rs64.TexTransTool.Decal.Cylindrical;
using net.rs64.TexTransTool.IslandSelector;
namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AbstractIslandSelector), true)]
    internal class AbstractIslandSelectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(target.GetType().Name);
            base.OnInspectorGUI();
        }
    }
}

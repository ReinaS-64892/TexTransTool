using UnityEditor;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AbstractTexTransGroup), true)]
    public class AbstractTexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thisTarget = target as AbstractTexTransGroup;
            TextureTransformerEditor.DrawerApplyAndRevert(thisTarget);
        }
    }
}
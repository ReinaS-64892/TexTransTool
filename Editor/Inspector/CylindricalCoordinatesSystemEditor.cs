using UnityEditor;
using net.rs64.TexTransTool.Decal.Cylindrical;
namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(CylindricalCoordinatesSystem))]
    internal class CylindricalCoordinatesSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("CylindricalCoordinatesSystem");
            base.OnInspectorGUI();
        }
    }
}
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal.Curve;

namespace Rs64.TexTransTool.Editor.Decal.Curve.Cylindrical
{


    [CustomEditor(typeof(CylindricalCurveDecal), true)]
    public class CylindricalCurveDecalEditor : CurveDecalEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TextureTransformerEditor.TextureTransformerEditorDrow(target as CylindricalCurveDecal);
        }

    }

}
#endif
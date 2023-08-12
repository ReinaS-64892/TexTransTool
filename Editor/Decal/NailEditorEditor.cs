#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;

namespace Rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(NailEditor), true)]
    public class NailEditorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var This_S_Object = serializedObject;
            var ThisObject = target as NailEditor;





            TextureTransformerEditor.DrowApplyAndRevart(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }


    }


}
#endif
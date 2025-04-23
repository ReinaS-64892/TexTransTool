using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor
{
    internal class TTCanBehaveAsLayerEditor : TexTransMonoBaseEditor
    {
        protected virtual void OnEnable() { CanBehaveAsLayerEditorUtilInit(); EditorApplication.hierarchyChanged += CanBehaveAsLayerEditorUtilInit; }
        protected virtual void OnDisable() { EditorApplication.hierarchyChanged -= CanBehaveAsLayerEditorUtilInit; }
        public bool ThisIsLayer;
        public bool ThisIsMLICChilde;
        public bool IsLayerMode => ThisIsLayer && ThisIsMLICChilde;
        public bool IsDrawPreviewButton => ThisIsLayer is false && ThisIsMLICChilde is false;
        void CanBehaveAsLayerEditorUtilInit()
        {
            var component = target as Component;
            if (component == null) { return; }

            ThisIsLayer = component?.GetComponent<AsLayer>() != null;
            ThisIsMLICChilde = component?.GetComponentInParent<MultiLayerImageCanvas>(true);
        }
        void DrawAddLayerButton(Component component)
        {
            if (ThisIsLayer is false && ThisIsMLICChilde)
                if (GUILayout.Button("AsLayer:button:AddLayer".Glc()))
                    if (component != null)
                        Undo.AddComponent<AsLayer>(component.gameObject);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var targetComponent = target as Component;
            if (targetComponent != null) DrawAddLayerButton(targetComponent);
        }
        protected override bool DrawPreviewButton => IsDrawPreviewButton;

    }
}

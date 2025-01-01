using UnityEditor;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;
namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AimIslandSelector))]
    internal class AimIslandSelectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(target.GetType().Name);
            base.OnInspectorGUI();

            if (_isAimEntered is false) { if (GUILayout.Button("IslandSelector:button:AimStart".Glc())) { AimStart(); } }
            else { if (GUILayout.Button("IslandSelector:button:AimExit".Glc())) { AimExit(); } }
        }

        bool _isAimEntered = false;

        private void OnDisable() { AimExit(); }

        void AimStart() { _isAimEntered = true; EditorApplication.update += AimUpdate; }

        void AimExit() { _isAimEntered = false; }

        void AimUpdate()
        {
            if (_isAimEntered is false) { EditorApplication.update -= AimUpdate; }

            var t = target as AimIslandSelector;
            if (t == null) { _isAimEntered = false; return; }
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) { return; }

            var cameraTf = sceneView.camera.transform;
            var tTf = t.transform;

            Undo.RecordObject(tTf, "AimUpdate");
            tTf.position = cameraTf.position;
            tTf.rotation = cameraTf.rotation;
        }
    }
}

#if UNITY_EDITOR
using net.rs64.TexTransTool.Decal;
using UnityEditor;

namespace net.rs64.TexTransTool
{
    internal class DecalGrabManager : ScriptableSingleton<DecalGrabManager>
    {

        AbstractDecal GrabTarget;
        public AbstractDecal NowGrabDecal => GrabTarget;
        SceneView GrabView;

        DecalGrabManager()
        {
            EditorApplication.update -= UpdateGrab;
            EditorApplication.update += UpdateGrab;
        }
        public void Grab(AbstractDecal abstractDecal)
        {
            GrabTarget = abstractDecal;
            GrabView = SceneView.lastActiveSceneView;
        }
        public void Drop(AbstractDecal abstractDecal)
        {
            if (abstractDecal != GrabTarget) { return; }
            GrabTarget = null;
            GrabView = null;
            EditorUtility.SetDirty(abstractDecal);
        }

        private void UpdateGrab()
        {
            if (GrabTarget == null || GrabView == null) { return; }
            GrabTarget.transform.position = GrabView.camera.transform.position;
            GrabTarget.transform.rotation = GrabView.camera.transform.rotation;
        }
    }
}
#endif
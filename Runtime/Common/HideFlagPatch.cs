#if UNITY_EDITOR
using UnityEngine;


namespace net.rs64.TexTransTool
{
    [AddComponentMenu("")]
    [ExecuteAlways]
    internal class HideFlagPatch : MonoBehaviour, ITexTransToolTag
    {
        internal const HideFlags flag = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

        public int SaveDataVersion => -1;

        private void RuntimePatch()
        {
            if (gameObject.TryGetComponent<Renderer>(out var renderer)) { renderer.enabled = false; }
            gameObject.hideFlags = flag;
        }
        private void Awake() { RuntimePatch(); }

        private void Start() { RuntimePatch(); }
        private void Reset() { RuntimePatch(); }
        private void Update() { RuntimePatch(); }
    }


    [UnityEditor.CustomEditor(typeof(HideFlagPatch))]
    class HideFlagPatchEditor : UnityEditor.Editor
    {
        private void OnEnable() { InspectorPatch(); }
        public override void OnInspectorGUI() { InspectorPatch(); }

        private void InspectorPatch()
        {
            var patch = target as HideFlagPatch;
            patch.hideFlags = HideFlagPatch.flag;
            foreach (var cm in patch.gameObject.GetComponents<Component>()) { cm.hideFlags = patch.hideFlags; }
            patch.gameObject.hideFlags = HideFlagPatch.flag;
        }
    }


}
#endif

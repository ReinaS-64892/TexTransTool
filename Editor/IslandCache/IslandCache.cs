using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.EditorIsland
{
    internal class IslandCache : ScriptableObject
    {
        public IslandCacheObject CacheObject;
    }
    [CustomEditor(typeof(IslandCache))]
    [CanEditMultipleObjects]
    internal class IslandCacheEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
            {
                var thisTarget = (target as IslandCache).CacheObject;
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Hash", GUILayout.Width(50));
                    EditorGUILayout.Space();
                    foreach (var hash in thisTarget.Hash) { EditorGUILayout.LabelField(hash.ToString(), GUILayout.Width(25)); }
                }
                EditorGUILayout.Space();
            }
            var rect = EditorGUILayout.GetControlRect();
            rect.height = rect.width;
            var drawSize = rect.height;
            var origin = rect.position;
            EditorGUI.DrawRect(rect, Color.white);
            var drawColor = new Color(0, 0, 0, 0.75f);
            foreach (var islandCache in targets)
            {
                var islandTargets = (islandCache as IslandCache).CacheObject;
                foreach (var island in islandTargets.Islands)
                {
                    var pivot = island.Pivot;
                    pivot.y = 1 - pivot.y;
                    pivot *= drawSize;
                    var size = island.Size;
                    size.y *= -1;
                    size *= drawSize;

                    rect.position = origin + pivot;
                    rect.size = size;

                    EditorGUI.DrawRect(rect, drawColor);
                }
            }
        }
    }
}

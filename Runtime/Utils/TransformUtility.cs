using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    internal static class TransformUtility
    {
        public static IEnumerable<Transform> GetChildren(this Transform Parent)
        {
            foreach (Transform child in Parent) { yield return child; }
        }
        public static IEnumerable<Transform> GetParents(this Transform transform)
        {
            while (transform?.parent != null)
            {
                transform = transform?.parent;
                yield return transform;
            }
        }

        public static IEnumerable<GameObject> GetParents(this GameObject gameObject)
        {
            while (gameObject?.transform?.parent != null)
            {
                gameObject = gameObject?.transform?.parent?.gameObject;
                yield return gameObject;
            }
        }
    }
}

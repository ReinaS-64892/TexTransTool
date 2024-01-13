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
    }
}
#nullable enable
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
namespace net.rs64.TexTransTool.Editor
{
    internal static class TargetObjectSelector
    {
        internal static void DrawTargetSelectionLayout<T>(SerializedProperty mg, IEnumerable<T> refObject, float imgSize = 84f)
        where T : UnityEngine.Object
        {
            var viewWidth = EditorGUIUtility.currentViewWidth - 18f;
            var getReqHeight = GetRequireHeight(refObject.Count(), viewWidth, imgSize);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(viewWidth), GUILayout.Height(getReqHeight));
            DrawTargetSelection(rect, mg, refObject, imgSize);
        }
        internal static float GetRequireHeight(int refCount, float viewWidth, float imgSize = 84f)
        {
            var widthCount = Math.Max(1, (int)Math.Floor(viewWidth / imgSize));
            var heightCount = (refCount + (widthCount - 1)) / widthCount;
            return (imgSize + 18f) * heightCount;
        }
        internal static void DrawTargetSelection<T>(Rect rect, SerializedProperty targetObjects, IEnumerable<T> refObject, float imgSize = 84f)
        where T : UnityEngine.Object
        {
            var xInit = rect.x;
            var widthCount = Math.Max(1, (int)Math.Floor(rect.width / imgSize));
            var elementExpandedWidth = rect.width / widthCount;
            foreach (var mm in EnumPair(refObject, widthCount))
            {
                rect.width = elementExpandedWidth;
                rect.height = imgSize + 18f;
                foreach (var m in mm)
                {
                    DrawTargetSelectorRow(rect, targetObjects, m, imgSize);
                    rect.x += rect.width;
                }

                rect.y += rect.height;
                rect.x = xInit;
            }
        }

        static void DrawTargetSelectorRow<T>(Rect rect, SerializedProperty targetObjects, T? referenceObject, float imgSize = 64f)
        where T : UnityEngine.Object
        {
            if (referenceObject == null) { return; }
            var initX = rect.x;
            rect.height = 18f;
            rect.width = 18f;

            var bVal = FindTarget(targetObjects, referenceObject);
            var aVal = EditorGUI.Toggle(rect, GUIContent.none, bVal);
            if (aVal != bVal) { EditTarget(targetObjects, referenceObject, aVal); }

            rect.x += rect.width;
            rect.width = imgSize - rect.width;
            EditorGUI.ObjectField(rect, referenceObject, typeof(T), false);

            rect.height = imgSize;
            rect.width = imgSize;
            rect.x = initX;
            rect.y += 18f;
            
            var previewTex = AssetPreview.GetAssetPreview(referenceObject);
            if (previewTex != null) EditorGUI.DrawPreviewTexture(rect, previewTex, null, ScaleMode.ScaleToFit);
        }

        internal static IEnumerable<IEnumerable<T?>> EnumPair<T>(IEnumerable<T> values, int count)
        {
            var pair = new T?[count];
            var index = 0;
            foreach (var v in values)
            {
                pair[index] = v;
                index += 1;

                if (index >= count)
                {
                    index = 0;
                    yield return pair.ToArray();
                }
            }
            if (index is not 0) { yield return pair[..index].ToArray(); }
        }

        static bool FindTarget<T>(SerializedProperty targetList, T refObject)
        where T : UnityEngine.Object
        {
            for (var i = 0; targetList.arraySize > i; i += 1)
            {
                if (targetList.GetArrayElementAtIndex(i).objectReferenceValue == refObject) { return true; }
            }
            return false;
        }
        static void EditTarget<T>(SerializedProperty targetList, T refObject, bool val)
        where T : UnityEngine.Object
        {
            if (val)
            {
                var newIndex = targetList.arraySize;
                targetList.arraySize += 1;
                targetList.GetArrayElementAtIndex(newIndex).objectReferenceValue = refObject;
            }
            else
            {
                for (var i = 0; targetList.arraySize > i; i += 1)
                { if (targetList.GetArrayElementAtIndex(i).objectReferenceValue == refObject) { targetList.DeleteArrayElementAtIndex(i); break; } }
            }
        }
        internal static void DrawTargetSelectionSlimLayout<T>(SerializedProperty mg, IEnumerable<T> refObject, float elementWidth = 128f)
        where T : UnityEngine.Object
        {
            var viewWidth = EditorGUIUtility.currentViewWidth - 18f;
            var getReqHeight = GetRequireHeightSlim(refObject.Count(), viewWidth, elementWidth);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(viewWidth), GUILayout.Height(getReqHeight));
            DrawTargetSelectionSlim(rect, mg, refObject, elementWidth);
        }
        internal static float GetRequireHeightSlim(int refCount, float viewWidth, float elementWidth = 128f)
        {
            var widthCount = Math.Max(1, (int)Math.Floor(viewWidth / elementWidth));
            var heightCount = (refCount + (widthCount - 1)) / widthCount;
            return 18f * heightCount;
        }
        internal static void DrawTargetSelectionSlim<T>(Rect rect, SerializedProperty targetObjects, IEnumerable<T> refObject, float elementWidth = 128f)
        where T : UnityEngine.Object
        {
            var xInit = rect.x;
            var widthCount = Math.Max(1, (int)Math.Floor(rect.width / elementWidth));
            var elementExpandedWidth = rect.width / widthCount;
            foreach (var mm in EnumPair(refObject, widthCount))
            {
                rect.width = elementExpandedWidth;
                rect.height = 18f;
                foreach (var m in mm)
                {
                    DrawAtTarget(rect, targetObjects, m);
                    rect.x += rect.width;
                }

                rect.y += rect.height;
                rect.x = xInit;
            }
        }


        internal static bool DrawAtTarget<T>(Rect rect, SerializedProperty sTargetMaterials, T? mat)
        where T : UnityEngine.Object
        {
            if (mat == null) { return false; }
            var fullWidth = rect.width;

            var val = FindTarget(sTargetMaterials, mat);
            rect.width = rect.height;
            var editVal = EditorGUI.Toggle(rect, val);
            if (editVal != val) { EditTarget(sTargetMaterials, mat, editVal); }

            rect.x += rect.width;
            var prevTex2D = AssetPreview.GetAssetPreview(mat);
            if (prevTex2D != null) { EditorGUI.DrawTextureTransparent(rect, prevTex2D, ScaleMode.ScaleToFit); }
            rect.x += rect.width;
            rect.width = fullWidth - rect.width - rect.width;
            EditorGUI.ObjectField(rect, mat, typeof(T), false);
            return editVal;
        }



    }

}

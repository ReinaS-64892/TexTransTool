#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace net.rs64.TexTransTool
{
    public readonly struct SerializedObjectCrawler : IEnumerable<UnityEngine.Object>
    {
        public readonly GameObject CrawlRoot;

        public SerializedObjectCrawler(GameObject crawlRoot)
        {
            CrawlRoot = crawlRoot;
        }
        public IEnumerator<UnityEngine.Object> GetEnumerator()
        {
            return EnumerateSerializedObjects(CrawlRoot);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnumerateSerializedObjects(CrawlRoot);
        }

        public static IEnumerator<UnityEngine.Object> EnumerateSerializedObjects(GameObject avatarRootObject)
        {
            foreach (var component in avatarRootObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;

                using (var serializeObj = new SerializedObject(component))
                {
                    var iter = serializeObj.GetIterator();
                    while (iter.Next(true))
                    {
                        if (iter.propertyType != SerializedPropertyType.ObjectReference) { continue; }
                        yield return iter.objectReferenceValue;
                    }
                }
            }
        }
        public static void ReplaceSerializedObjects(GameObject avatarRootObject, Dictionary<UnityEngine.Object, UnityEngine.Object> replaceDict)
        {
            foreach (var component in avatarRootObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;

                using (var serializeObj = new SerializedObject(component))
                {
                    serializeObj.Update();
                    var iter = serializeObj.GetIterator();
                    var applyFrag = false;
                    while (iter.Next(true))
                    {
                        if (iter.propertyType != SerializedPropertyType.ObjectReference) { continue; }
                        if (iter.objectReferenceValue == null) { continue; }
                        if (!replaceDict.ContainsKey(iter.objectReferenceValue)) { continue; }
                        iter.objectReferenceValue = replaceDict[iter.objectReferenceValue];
                        applyFrag = true;
                    }
                    if (applyFrag) { serializeObj.ApplyModifiedPropertiesWithoutUndo(); }
                }
            }
        }
    }

}
#endif

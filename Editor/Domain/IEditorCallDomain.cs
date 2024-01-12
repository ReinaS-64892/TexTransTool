#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal interface IEditorCallDomain : IDomain
    {
        /// <summary>
        /// Sets the value to specified property with recording for revert
        /// </summary>
        void SetSerializedProperty(SerializedProperty property, Object value);
    }
}
#endif
using UnityEngine;
using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(TextureSelector))]
    public class TextureSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18;

            var labelRect = position;
            var buttonRect = position;
            labelRect.width = 160;
            buttonRect.width -= labelRect.width;
            buttonRect.x += labelRect.width;
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

            var sTexture2D = property.FindPropertyRelative("SelectTexture");
            var component = sTexture2D.serializedObject.targetObject;
            EditorGUI.BeginDisabledGroup(component is not Component);
            if (GUI.Button(buttonRect, "TextureSelector:prop:OpenSelector".Glc()))
            {
                DomainTextureSelector.OpenSelector(sTexture2D, component as Component);
            }
            EditorGUI.EndDisabledGroup();


            position.y += position.height;
            using var indentScope = new EditorGUI.IndentLevelScope(1);

            var texRect = position;
            texRect.height = 64f;
            var prop = EditorGUI.BeginProperty(texRect, "TextureSelector:prop:SelectTexture".Glc(), sTexture2D);
            sTexture2D.objectReferenceValue = EditorGUI.ObjectField(texRect, prop, sTexture2D.objectReferenceValue, typeof(Texture2D), true);
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 18f + 64f; }

    }
}

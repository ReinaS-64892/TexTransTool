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
            var sMode = property.FindPropertyRelative("Mode");
            var labelRect = position;
            var enumRect = position;
            labelRect.width = 160;
            enumRect.width -= labelRect.width;
            enumRect.x += labelRect.width;
            EditorGUI.BeginProperty(position, GUIContent.none, sMode);
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            EditorGUI.PropertyField(enumRect, sMode, GUIContent.none);
            EditorGUI.EndProperty();
            position.y += position.height;


            EditorGUI.indentLevel += 1;

            switch (sMode.enumValueIndex)
            {
                case 0://Absolute
                    {
                        var sTexture2D = property.FindPropertyRelative("SelectTexture");
                        var component = sTexture2D.serializedObject.targetObject;
                        EditorGUI.BeginDisabledGroup(component is not Component);
                        if (GUI.Button(position, "TextureSelector:prop:OpenSelector".Glc()))
                        {
                            DomainTextureSelector.OpenSelector(sTexture2D, component as Component);
                        }
                        EditorGUI.EndDisabledGroup();

                        position.y += position.height;

                        var texRect = position;
                        texRect.height = 64f;
                        var prop = EditorGUI.BeginProperty(texRect, "TextureSelector:prop:SelectTexture".Glc(), sTexture2D);
                        sTexture2D.objectReferenceValue = EditorGUI.ObjectField(texRect, prop, sTexture2D.objectReferenceValue, typeof(Texture2D), true);
                        EditorGUI.EndProperty();
                        break;
                    }
                case 1://Relative
                    {

                        var sTargetRenderer = property.FindPropertyRelative("RendererAsPath");

                        EditorGUI.BeginChangeCheck();
                        EditorGUI.PropertyField(position, sTargetRenderer, "TextureSelector:prop:RendererAsPath".Glc());
                        position.y += position.height;
                        if (EditorGUI.EndChangeCheck() && sTargetRenderer.objectReferenceValue != null)
                        { sTargetRenderer.objectReferenceValue = TextureTransformerEditor.RendererFiltering(sTargetRenderer.objectReferenceValue as Renderer); }



                        var sMaterialSelect = property.FindPropertyRelative("SlotAsPath");

                        var TargetRenderer = sTargetRenderer.objectReferenceValue as Renderer;
                        var TargetMaterials = TargetRenderer?.sharedMaterials;
                        ArraySelector(sMaterialSelect, TargetMaterials, ref position);

                        var sTargetPropertyName = property.FindPropertyRelative("PropertyNameAsPath");
                        EditorGUI.PropertyField(position, sTargetPropertyName, "TextureSelector:prop:PropertyNameAsPath".Glc());
                        position.y += position.height;

                        if (TargetMaterials != null && TargetMaterials.Length > sMaterialSelect.intValue && sMaterialSelect.intValue >= 0)
                        {
                            var texture = TargetMaterials[sMaterialSelect.intValue]?.GetTexture(sTargetPropertyName.FindPropertyRelative("_propertyName").stringValue) as Texture2D;
                            if (texture != null)
                            {
                                var previewTexRect = position;
                                previewTexRect.height = 64f;
                                EditorGUI.ObjectField(previewTexRect, "TextureSelector:prop:SelectTexturePreview".GetLocalize(), texture, typeof(Texture2D), true);
                            }
                        }

                        break;
                    }
            }

            EditorGUI.indentLevel -= 1;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var sMode = property.FindPropertyRelative("Mode");
            switch (sMode.enumValueIndex)
            {
                case 0://Absolute
                    {
                        return 18f + 18f + 64f;
                    }
                case 1://Relative
                    {
                        var sTargetRenderer = property.FindPropertyRelative("RendererAsPath");
                        if (sTargetRenderer.objectReferenceValue == null) { return 18f * 3; }

                        var TargetRenderer = sTargetRenderer.objectReferenceValue as Renderer;
                        var TargetMaterials = TargetRenderer?.sharedMaterials;

                        if (TargetMaterials == null) { return 18f * 3; }

                        var sMaterialSelect = property.FindPropertyRelative("SlotAsPath");
                        var sTargetPropertyName = property.FindPropertyRelative("PropertyNameAsPath");
                        var selectTex = 0 <= sMaterialSelect.intValue && sMaterialSelect.intValue < TargetMaterials.Length ? TargetMaterials[sMaterialSelect.intValue]?.GetTexture(sTargetPropertyName.FindPropertyRelative("_propertyName").stringValue) as Texture2D : null;
                        if (selectTex == null) { return 18f * (TargetMaterials.Length + 3); }
                        else { return 18f * (TargetMaterials.Length + 3) + 64f; }
                    }
                default: return 18f;
            }
        }

        public static void ArraySelector<T>(SerializedProperty Select, T[] Array, ref Rect position) where T : UnityEngine.Object
        {
            if (Array == null) return;
            int SelectCount = 0;
            int DistSelect = Select.intValue;
            int NewSelect = Select.intValue;

            var offset = 160;
            var propRect = position; propRect.height *= Array.Length;
            var labelRect = position; labelRect.width = offset;
            EditorGUI.LabelField(labelRect, EditorGUI.BeginProperty(propRect, "TextureSelector:prop:SlotAsPath".Glc(), Select));

            foreach (var ArrayValue in Array)
            {
                var toggleRect = position;
                toggleRect.width = 30;
                toggleRect.x += offset;
                var objectRect = position;
                objectRect.width -= 30 + offset;
                objectRect.x += 30 + offset;

                if (EditorGUI.Toggle(toggleRect, SelectCount == Select.intValue) && DistSelect != SelectCount) NewSelect = SelectCount;
                EditorGUI.ObjectField(objectRect, ArrayValue, typeof(T), true);

                position.y += position.height;

                SelectCount += 1;
            }
            if (DistSelect != NewSelect) { Select.intValue = NewSelect; }

            EditorGUI.EndProperty();
        }

    }
}

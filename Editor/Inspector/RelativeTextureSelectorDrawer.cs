using UnityEngine;
using UnityEditor;
namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(RelativeTextureSelector))]
    public class RelativeTextureSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18;
            EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
            position.y += position.height;

            EditorGUI.indentLevel += 1;

            var sTargetRenderer = property.FindPropertyRelative("TargetRenderer");

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, sTargetRenderer, "TargetRenderer".GetLC());
            position.y += position.height;
            if (EditorGUI.EndChangeCheck())
            { sTargetRenderer.objectReferenceValue = TextureTransformerEditor.RendererFiltering(sTargetRenderer.objectReferenceValue as Renderer); }



            var sMaterialSelect = property.FindPropertyRelative("MaterialSelect");

            var TargetRenderer = sTargetRenderer.objectReferenceValue as Renderer;
            var TargetMaterials = TargetRenderer?.sharedMaterials;

            sMaterialSelect.intValue = ArraySelector(sMaterialSelect.intValue, TargetMaterials, ref position);

            var sTargetPropertyName = property.FindPropertyRelative("TargetPropertyName");
            EditorGUI.PropertyField(position, sTargetPropertyName, "TargetPropertyName".GetLC());
            position.y += position.height;

            if (TargetMaterials != null)
            {
                var texture = TargetMaterials[sMaterialSelect.intValue]?.GetTexture(sTargetPropertyName.FindPropertyRelative("_propertyName").stringValue) as Texture2D;
                if (texture != null)
                {
                    EditorGUI.LabelField(position, "ReplaceTexturePreview".GetLocalize());
                    var previewTexRect = position;
                    previewTexRect.height = 64f;
                    EditorGUI.DrawTextureTransparent(previewTexRect, texture, ScaleMode.ScaleToFit);
                }
            }

            EditorGUI.indentLevel -= 1;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var sTargetRenderer = property.FindPropertyRelative("TargetRenderer");
            if (sTargetRenderer.objectReferenceValue == null) { return 18f * 3; }

            var TargetRenderer = sTargetRenderer.objectReferenceValue as Renderer;
            var TargetMaterials = TargetRenderer?.sharedMaterials;

            if (TargetMaterials == null) { return 18f * 3; }

            var sMaterialSelect = property.FindPropertyRelative("MaterialSelect");
            var sTargetPropertyName = property.FindPropertyRelative("TargetPropertyName");
            var selectTex = TargetMaterials[sMaterialSelect.intValue]?.GetTexture(sTargetPropertyName.FindPropertyRelative("_propertyName").stringValue) as Texture2D;
            if (selectTex == null) { return 18f * (TargetMaterials.Length + 3); }
            else { return 18f * (TargetMaterials.Length + 3) + 64f; }

        }

        public static int ArraySelector<T>(int Select, T[] Array, ref Rect position) where T : UnityEngine.Object
        {
            if (Array == null) return Select;
            int SelectCount = 0;
            int DistSelect = Select;
            int NewSelect = Select;

            foreach (var ArrayValue in Array)
            {
                var toggleRect = position;
                toggleRect.width = 30;
                var objectRect = position;
                objectRect.width -= 30;
                objectRect.x += 30;

                if (EditorGUI.Toggle(toggleRect, SelectCount == Select) && DistSelect != SelectCount) NewSelect = SelectCount;

                EditorGUI.ObjectField(objectRect, ArrayValue, typeof(Material), true);

                position.y += position.height;

                SelectCount += 1;
            }
            return NewSelect;
        }
    }
}
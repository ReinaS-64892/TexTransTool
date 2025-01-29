using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureBlender))]
    internal class TextureBlenderEditor : UnityEditor.Editor
    {
        CanBehaveAsLayerEditorUtil behaveLayerUtil;
        void BehaveUtilInit() { behaveLayerUtil = new(target as Component); }
        void OnEnable() { BehaveUtilInit(); EditorApplication.hierarchyChanged += BehaveUtilInit; }
        void OnDisable() { EditorApplication.hierarchyChanged -= BehaveUtilInit; }

        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("TextureBlender");

            var thisTarget = target as TextureBlender;
            var thisSObject = serializedObject;

            if (behaveLayerUtil.ThisIsLayer is false) EditorGUILayout.PropertyField(thisSObject.FindProperty("TargetTexture"));

            var sBlendTexture = thisSObject.FindProperty("BlendTexture");
            EditorGUILayout.PropertyField(sBlendTexture, sBlendTexture.name.Glc());

            var sColor = thisSObject.FindProperty("Color");
            EditorGUILayout.PropertyField(sColor, sColor.name.Glc());

            var sBlendTypeKey = thisSObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendTypeKey, sBlendTypeKey.name.Glc());

            thisSObject.ApplyModifiedProperties();
            if (behaveLayerUtil.IsDrawPreviewButton) PreviewButtonDrawUtil.Draw(thisTarget);
            behaveLayerUtil.DrawAddLayerButton(target as Component);
        }


        public static void DrawerSummary(TextureBlender target)
        {
            var sObj = new SerializedObject(target);
            var sTargetRenderer = sObj.FindProperty("TargetTexture").FindPropertyRelative("RendererAsPath");
            EditorGUILayout.PropertyField(sTargetRenderer);
            var sBlendTexture = sObj.FindProperty("BlendTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(sBlendTexture);

            sObj.ApplyModifiedProperties();
        }



    }
    internal class CanBehaveAsLayerEditorUtil
    {
        public readonly bool ThisIsLayer;
        public readonly bool ThisIsMLICChilde;
        public bool IsDrawPreviewButton => ThisIsLayer is false && ThisIsMLICChilde is false;
        public CanBehaveAsLayerEditorUtil(Component component)
        {
            ThisIsLayer = component?.GetComponent<AsLayer>() != null;
            ThisIsMLICChilde = component?.GetComponentInParent<MultiLayerImageCanvas>(true);
        }
        public void DrawAddLayerButton(Component component)
        {
            if (ThisIsLayer is false && ThisIsMLICChilde)
                if (GUILayout.Button("AsLayer:button:AddLayer".Glc()))
                    if (component != null)
                        Undo.AddComponent<AsLayer>(component.gameObject);
        }
    }
}

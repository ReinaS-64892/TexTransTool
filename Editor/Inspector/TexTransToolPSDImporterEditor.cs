using UnityEditor;
using UnityEditor.AssetImporters;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [CustomEditor(typeof(TexTransToolPSDImporter))]
    class TexTransToolPSDImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("TexTransToolPSDImporter" + " " + "Common:ExperimentalWarning".GetLocalize(), MessageType.Warning);

            base.ApplyRevertGUI();
        }
        public override bool HasPreviewGUI() { return false; }
        // public override void DrawPreview(Rect previewArea)
        // {
        //     var importer = target as TexTransToolPSDImporter;
        //     var previewTex = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).FirstOrDefault(i => i.name == "TTT-CanvasPreviewResult") as Texture2D;
        //     if (previewTex == null) { return; }

        //     EditorGUI.DrawTextureTransparent(previewArea, previewTex, ScaleMode.ScaleToFit);
        // }

    }







}

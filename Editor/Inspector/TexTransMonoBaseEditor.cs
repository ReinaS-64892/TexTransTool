using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.Preview.RealTime;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TexTransMonoBase), true)]
    internal class TexTransMonoBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var ttMonoBase = target as TexTransMonoBase;
            if (ttMonoBase == null) { return; }// 通常ありえないコードパス
            DrawOldSaveDataVersionWarning(ttMonoBase);
            DrawerWarning(ttMonoBase);

            serializedObject.Update();
            OnTexTransComponentInspectorGUI();
            serializedObject.ApplyModifiedProperties();

            if (DrawPreviewButton) PreviewButtonDrawUtil.Draw(ttMonoBase);
        }
        protected virtual bool DrawPreviewButton => true;

        protected virtual void OnTexTransComponentInspectorGUI()
        {
            base.OnInspectorGUI();
        }
        static TTTProjectConfig s_projectConfig;
        public static void DrawerWarning(TexTransMonoBase ttMonoBase)
        {
            if (ttMonoBase is ITexTransToolStableComponent) { return; }

            s_projectConfig ??= TTTProjectConfig.instance;
            if (s_projectConfig.DisplayExperimentalWarning is false) { return; }

            var typeName = ttMonoBase.GetType().Name;
            EditorGUILayout.HelpBox(typeName + " " + "Common:ExperimentalWarning".GetLocalize(), MessageType.Warning);
        }
        public static void DrawOldSaveDataVersionWarning(TexTransMonoBase ttMonoBase)
        {
            if (ttMonoBase is ITexTransToolStableComponent texTransToolStableComponent)
            {
                if ((ttMonoBase as ITexTransToolTag).SaveDataVersion < texTransToolStableComponent.StabilizeSaveDataVersion)
                    DrawMigratorWindowButton();
            }
            else
            {
                if ((ttMonoBase as ITexTransToolTag).SaveDataVersion < TexTransMonoBase.TTTDataVersion)
                    DrawMigratorWindowButton();
            }

            void DrawMigratorWindowButton()
            {
                if (GUILayout.Button("Common:button:ThisComponentSaveDataIsOldOpenMigratorWindow".Glc()))
                    Migration.MigratorWindow.ShowWindow();
            }
        }
    }
}

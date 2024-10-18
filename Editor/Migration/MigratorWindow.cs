using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.IO;

namespace net.rs64.TexTransTool.Migration
{

    internal class MigratorWindow : EditorWindow
    {
        [MenuItem("Tools/TexTransTool/Migrator")]
        internal static void ShowWindow()
        {
            var window = GetWindow<MigratorWindow>();
            window.initialize();
            window.titleContent = new GUIContent("Migrator");
            window.Show();
        }

        Dictionary<GameObject, AAOMigrator.PrefabInfo> PrefabInfo;
        Dictionary<GameObject, int> PrefabMinimumSaveDataVersion;
        Dictionary<GameObject, bool> MigrationTarget;
        Dictionary<GameObject, Toggle> PrefabToToggle;
        Dictionary<Toggle, VisualElement> ToggleVisualBox;
        Dictionary<string, Toggle> SceneToToggle;
        Dictionary<string, bool> Scene;

        void initialize()
        {
            rootVisualElement.Clear();
            var rootScroll = new ScrollView();
            rootVisualElement.Add(rootScroll);
            var rootScrollContainer = rootScroll.contentContainer;


            var prefabsLabel = new Label("Prefab");
            prefabsLabel.style.fontSize = 24f;
            rootScrollContainer.hierarchy.Add(prefabsLabel);

            var selectUtilBox = new VisualElement();
            selectUtilBox.style.flexDirection = FlexDirection.Row;
            var prefabSelectAll = new Button(() => { foreach (var toggle in PrefabToToggle.Values) toggle.value = true; });
            prefabSelectAll.text = "PrefabSelectAll";
            var prefabSelectInvert = new Button(() => { foreach (var toggle in PrefabToToggle.Values) toggle.value = !toggle.value; });
            prefabSelectInvert.text = "PrefabSelectInvert";
            selectUtilBox.hierarchy.Add(prefabSelectAll);
            selectUtilBox.hierarchy.Add(prefabSelectInvert);
            rootScrollContainer.hierarchy.Add(selectUtilBox);

            var prefabTargetSelectorToggle = new VisualElement();
            rootScrollContainer.hierarchy.Add(prefabTargetSelectorToggle);

            var sceneLabel = new Label("Scene");
            sceneLabel.style.fontSize = 24f;
            rootScrollContainer.hierarchy.Add(sceneLabel);

            var sceneSelectUtilBox = new VisualElement();
            sceneSelectUtilBox.style.flexDirection = FlexDirection.Row;
            var sceneSelectAll = new Button(() => { foreach (var toggle in SceneToToggle.Values) toggle.value = true; });
            sceneSelectAll.text = "SceneSelectAll";
            var sceneSelectInvert = new Button(() => { foreach (var toggle in SceneToToggle.Values) toggle.value = !toggle.value; });
            sceneSelectInvert.text = "SceneSelectInvert";
            sceneSelectUtilBox.hierarchy.Add(sceneSelectAll);
            sceneSelectUtilBox.hierarchy.Add(sceneSelectInvert);
            rootScrollContainer.hierarchy.Add(sceneSelectUtilBox);

            var sceneTargetSelectorToggle = new VisualElement();
            rootScrollContainer.hierarchy.Add(sceneTargetSelectorToggle);

            var migrate = new Button(Migration);
            migrate.text = "Migrate Selected!";
            rootScrollContainer.hierarchy.Add(migrate);

            // var allMigrate = new Button(() => { Migrator.MigrateEverything(); this.Close(); });
            // allMigrate.text = "All Migration!";
            // allMigrate.style.marginTop = 18f;
            // rootScrollContainer.hierarchy.Add(allMigrate);

            PrefabInfo = AAOMigrator.GetPrefabInfo(AAOMigrator.GetAllPrefabRoots<ITexTransToolTag>()).ToDictionary(i => i.Prefab, i => i);
            PrefabMinimumSaveDataVersion = PrefabInfo.ToDictionary(kv => kv.Key, kv => kv.Key.GetComponentsInChildren<ITexTransToolTag>(true).Min(t => t.SaveDataVersion));
            MigrationTarget = PrefabInfo.ToDictionary(kv => kv.Key, kv => false);
            PrefabToToggle = PrefabInfo.ToDictionary(kv => kv.Key, kv => new Toggle(kv.Value.Prefab.name) { value = false, tooltip = AssetDatabase.GetAssetPath(kv.Value.Prefab) });
            ToggleVisualBox = PrefabToToggle.ToDictionary(kv => kv.Value, kv =>
            {
                var vi = new VisualElement();
                vi.hierarchy.Add(kv.Value);
                return vi;
            });
            foreach (var toggleKV in PrefabToToggle)
            {
                var prefab = toggleKV.Key;
                toggleKV.Value.RegisterValueChangedCallback(mv => { MigrationTarget[prefab] = mv.newValue; });
            }
            foreach (var toggleKV in PrefabToToggle)
            {
                if (PrefabMinimumSaveDataVersion[toggleKV.Key] < TexTransBehavior.TTTDataVersion) { continue; }
                toggleKV.Value.SetEnabled(false);
            }
            foreach (var prefabKV in PrefabInfo)
            {
                var toggle = PrefabToToggle[prefabKV.Key];
                var parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefabKV.Value.Prefab);
                if (parentPrefab != null && PrefabToToggle.ContainsKey(parentPrefab))
                {
                    ToggleVisualBox[PrefabToToggle[parentPrefab]].hierarchy.Add(ToggleVisualBox[toggle]);
                    toggle.style.marginLeft = 18;
                }
                else
                {
                    prefabTargetSelectorToggle.hierarchy.Add(ToggleVisualBox[toggle]);
                }
            }

            Scene = AssetDatabase.FindAssets("t:scene").Select(AssetDatabase.GUIDToAssetPath).Where(path => !AAOMigrator.IsReadOnlyPath(path)).ToDictionary(i => i, i => false);
            SceneToToggle = Scene.ToDictionary(s => s.Key, s => new Toggle(Path.GetFileNameWithoutExtension(s.Key)) { value = false, tooltip = s.Key });
            foreach (var toggleKV in SceneToToggle)
            {
                var path = toggleKV.Key;
                toggleKV.Value.RegisterValueChangedCallback(mv => Scene[path] = mv.newValue);
                sceneTargetSelectorToggle.hierarchy.Add(toggleKV.Value);
            }


            var MigratePSDImporterRegistering = new Button();
            MigratePSDImporterRegistering.text = "PSD Importer のマイグレーション(再設定)をする";
            MigratePSDImporterRegistering.clicked += ReflectCallPSDMigration;
            rootScrollContainer.hierarchy.Add(MigratePSDImporterRegistering);
        }

        void CreateGUI()
        {
            if (rootVisualElement.childCount != 0) { return; }
            var findMigrationTarget = new Button(initialize);
            findMigrationTarget.text = "FindMigrationTarget";
            rootVisualElement.Add(findMigrationTarget);
        }


        void Migration()
        {
            var prefabTarget = MigrationTarget.Where(kv => kv.Value).Select(kv => kv.Key).ToHashSet();

            var saveDataVersionValues = PrefabMinimumSaveDataVersion.Where(kv => prefabTarget.Contains(kv.Key)).Select(i => i.Value);
            if (saveDataVersionValues.Any() is false) { saveDataVersionValues = saveDataVersionValues.Append(0); }

            AAOMigrator.MigratePartial(saveDataVersionValues.Min(), prefabTarget, Scene.Where(kv => kv.Value).Select(kv => kv.Key).ToHashSet());

            this.Close();
        }

        internal static void ReflectCallPSDMigration()
        {
            try
            {
                var PSDImporterMigrationType = Type.GetType("net.rs64.TexTransTool.MultiLayerImage.Importer.PSDImporterMigration,net.rs64.ttt-psd-importer.editor", true, false);
                var migrationMethod = PSDImporterMigrationType.GetMethod("PSDImporterReSetting", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod);

                migrationMethod.Invoke(null, null);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }
}


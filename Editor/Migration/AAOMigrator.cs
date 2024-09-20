//https://github.com/anatawa12/AvatarOptimizer/blob/25bf2e68f93705808d8d2cc6b7c4449f57c990a8/Editor/Migration/Migration.cs
/* Origin AAO License MIT
MIT License

Copyright (c) 2022 anatawa12

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using net.rs64.TexTransTool.Migration.V0;
using net.rs64.TexTransTool.Migration.V1;
using net.rs64.TexTransTool.Migration.V2;
using net.rs64.TexTransTool.Migration.V3;
using net.rs64.TexTransTool.Migration.V4;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VRC.Dynamics;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool.Migration
{
    /*
    これは AAO のマイクグレーターにあった便利な関数などを引き抜いていじった何か
    */
    [InitializeOnLoad]
    internal class AAOMigrator
    {


        static AAOMigrator()
        {
            if (!File.Exists(MigrationUtility.SaveDataVersionPath))
            {
                MigrationUtility.WriteVersion(TexTransBehavior.TTTDataVersion);
            }

            EditorApplication.update += Update;
        }
        static bool InProgress = false;
        private static void Update()
        {
            if (InProgress) return; // try next tick
            try
            {
                var SaveDataVersionJsonI = MigrationUtility.GetSaveDataVersion;

                if (SaveDataVersionJsonI.SaveDataVersion < TexTransBehavior.TTTDataVersion)
                {
                    DoMigrate();
                }
                else if (SaveDataVersionJsonI.SaveDataVersion > TexTransBehavior.TTTDataVersion)
                {
                    var result = EditorUtility.DisplayDialog("ダウングレードは保証されていません!!",
@"以前インストールされていたTexTransToolの新バージョンと、現在インストールされているTexTransToolの旧バージョンの間に互換性がありません。
データを保護するには、保存せずにUnityを終了してください。保存した場合、TexTransTool関連のデータが消失する可能性があります。"
#if !UNITY_EDITOR_OSX

+
@"


強制的に古いバージョンにする: 強制的に古いバージョンにし、このウィンドウを今回を非表示にします。
セーブデータが消える可能性の高い非常に推奨されない選択肢なので、ご注意ください。
"
#endif
                      , "強制的に古いバージョンにする", "閉じる(Close)");

                    if (result)
                    {
                        MigrationUtility.WriteVersion(TexTransBehavior.TTTDataVersion);
                    }
                }
            }
            finally
            {
                EditorApplication.update -= Update;
            }
        }

        private static bool DoMigrate()
        {
            InProgress = true;
            var result = EditorUtility.DisplayDialogComplex("マイグレーションが必要です!",
#if !UNITY_EDITOR_OSX
@"以前インストールされていたTexTransToolの旧バージョンと、現在インストールされているTexTransToolの新バージョンの間に互換性がありません!
TexTransToolを正常に動作させるためには、すべてのシーンとプレハブをこのバージョン用にマイグレーションする必要があります。
マイグレーションには長い時間がかかり、プロジェクトが壊れる可能性もあります。
バックアップをしていない場合はバックアップを行ってからマイグレーションしてください。
マイグレーションしない場合、Unityを再起動するたびにこのウィンドウが表示されます。

" +
#endif
"プロジェクトをマイグレーションしますか?"
#if !UNITY_EDITOR_OSX
+
@"


上級者向け: このマイグレーション通知を今回非表示にし、個別選択可能なマイグレーションウィンドウにて行う。
データが壊れる可能性がある危険な選択肢です、バックアップを行ってから選択してください。
ウィンドウ上部分のツールバー「 Tools / TexTransTool / Migrator 」 から手動で開くことも可能です。
"
#endif
                ,
                "マイグレーションする (Migrate)",
                "キャンセル (Cancel)",
                "上級者向け (Advanced)"
                );

            if (result == 0) // Do Migrate!!!
            {
                MigrateEverything();
            }
            else if (result == 1) { } // Cancel
            else // Advanced!
            {
                MigratorWindow.ShowWindow();
                MigrationUtility.WriteVersion(TexTransBehavior.TTTDataVersion);
            }
            InProgress = false;
            return result != 1;
        }







        /// <returns>List of prefab assets. parent prefab -> child prefab</returns>
        private static List<GameObject> GetPrefabs()
        {
            return FlattenToGameObject(GetPrefabInfo(GetAllPrefabRoots<ITexTransToolTag>()));
        }
        public static GameObject[] GetAllPrefabRoots<ContainsType>()
        {
            return AssetDatabase.FindAssets("t:prefab")
                  .Select(AssetDatabase.GUIDToAssetPath)
                  .Where(s => !IsReadOnlyPath(s))
                  .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                  .Where(x => x != null)
                  .Where(x => CheckPrefabType(PrefabUtility.GetPrefabAssetType(x)))
                  .Where(x => x.GetComponentsInChildren<ContainsType>(true).Length != 0)
                  .ToArray();
        }
        public static LinkedList<PrefabInfo> GetPrefabInfo(GameObject[] allPrefabRoots)
        {
            var vertices = new LinkedList<PrefabInfo>(allPrefabRoots.Select(prefabRoot => new PrefabInfo(prefabRoot)));

            // assign Parents and Children here.
            var vertexLookup = vertices.ToDictionary(x => x.Prefab, x => x);
            foreach (var vertex in vertices)
            {
                foreach (var parentPrefab in vertex.Prefab
                             .GetComponentsInChildren<Transform>(true)
                             .Select(x => x.gameObject)
                             .Where(PrefabUtility.IsAnyPrefabInstanceRoot)
                             .Select(PrefabUtility.GetCorrespondingObjectFromSource)
                             .Select(x => x.transform.root.gameObject))
                {
                    if (vertexLookup.TryGetValue(parentPrefab, out var parent))
                    {
                        vertex.Parents.Add(parent);
                        parent.Children.Add(vertex);
                    }
                }
            }

            return vertices;
        }
        public static List<GameObject> FlattenToGameObject(LinkedList<PrefabInfo> vertices)
        {
            var sortedVertices = new List<GameObject>();

            // Orphaned nodes with no parents or children go first
            {
                var it = vertices.First;
                while (it != null)
                {
                    var cur = it;
                    it = it.Next;
                    if (cur.Value.Children.Count != 0 || cur.Value.Parents.Count != 0) continue;
                    sortedVertices.Add(cur.Value.Prefab);
                    vertices.Remove(cur);
                }
            }

            var openSet = new Queue<PrefabInfo>();

            // Find root nodes with no parents
            foreach (var vertex in vertices.Where(vertex => vertex.Parents.Count == 0))
                openSet.Enqueue(vertex);

            var visitedVertices = new HashSet<PrefabInfo>();
            while (openSet.Count > 0)
            {
                var vertex = openSet.Dequeue();

                if (visitedVertices.Contains(vertex))
                {
                    continue;
                }

                if (vertex.Parents.Count > 0)
                {
                    var neededParentVisit = false;

                    foreach (var vertexParent in vertex.Parents.Where(vertexParent => !visitedVertices.Contains(vertexParent)))
                    {
                        neededParentVisit = true;
                        openSet.Enqueue(vertexParent);
                    }

                    if (neededParentVisit)
                    {
                        // Re-queue to visit after we have traversed the node's parents
                        openSet.Enqueue(vertex);
                        continue;
                    }
                }

                visitedVertices.Add(vertex);
                sortedVertices.Add(vertex.Prefab);

                foreach (var vertexChild in vertex.Children)
                    openSet.Enqueue(vertexChild);
            }

            // Sanity check
            foreach (var vertex in vertices.Where(vertex => !visitedVertices.Contains(vertex)))
                throw new Exception($"Invalid DAG state: node '{vertex.Prefab}' was not visited.");

            return sortedVertices;
        }


        static bool CheckPrefabType(PrefabAssetType type) => type != PrefabAssetType.MissingAsset && type != PrefabAssetType.Model && type != PrefabAssetType.NotAPrefab;
        internal static bool IsReadOnlyPath(string path)
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
            return packageInfo != null && packageInfo.source != PackageSource.Embedded && packageInfo.source != PackageSource.Local;
        }
        internal class PrefabInfo
        {
            public readonly GameObject Prefab;
            public readonly List<PrefabInfo> Children = new List<PrefabInfo>();
            public readonly List<PrefabInfo> Parents = new List<PrefabInfo>();

            public PrefabInfo(GameObject prefab)
            {
                Prefab = prefab;
            }
        }
#pragma warning disable CS0612
        [MenuItem(TTTConfig.DEBUG_MENU_PATH + "/Migration/Migrate Project")]
        internal static void MigrateEverything()
        {
            PreMigration();
            for (var version = MigrationUtility.GetSaveDataVersion.SaveDataVersion; TexTransBehavior.TTTDataVersion > version; version += 1)
            {
                switch (version)
                {
                    case 0:
                        {
                            MigrateEverythingFor<TTTV0Migrator>(true);
                            break;
                        }
                    case 1:
                        {
                            MigrateEverythingFor<TTTV1Migrator>(true);
                            break;
                        }
                    case 2:
                        {
                            MigrateEverythingFor<TTTV2Migrator>(true);
                            break;
                        }
                    case 3:
                        {
                            MigrateEverythingFor<TTTV3Migrator>(true);
                            break;
                        }
                    case 4:
                        {
                            MigrateEverythingFor<TTTV4Migrator>(true);
                            break;
                        }
                }
            }
            PostMigration();
        }
        internal static void MigratePartial(int minimumSaveDataVersion, HashSet<GameObject> targetPrefabs, HashSet<string> targetScenePath)
        {
            PreMigration();
            for (var version = minimumSaveDataVersion; TexTransBehavior.TTTDataVersion > version; version += 1)
            {
                switch (version)
                {
                    case 0:
                        {
                            MigratePartialFor<TTTV0Migrator>(targetPrefabs, targetScenePath, true);
                            break;
                        }
                    case 1:
                        {
                            MigratePartialFor<TTTV1Migrator>(targetPrefabs, targetScenePath, true);
                            break;
                        }
                    case 2:
                        {
                            MigratePartialFor<TTTV2Migrator>(targetPrefabs, targetScenePath, true);
                            break;
                        }
                    case 3:
                        {
                            MigratePartialFor<TTTV3Migrator>(targetPrefabs, targetScenePath, true);
                            break;
                        }
                    case 4:
                        {
                            MigratePartialFor<TTTV4Migrator>(targetPrefabs, targetScenePath, true);
                            break;
                        }
                }
            }
            PostMigration();
        }
#pragma warning restore CS0612
        private static void MigratePartialFor<Migrator>(HashSet<GameObject> targetPrefabs, HashSet<string> targetScenePath, bool continuesMigrate = false)
        where Migrator : IMigrator, new()
        {
            try
            {
                if (!continuesMigrate) PreMigration();

                var migrator = new Migrator();
                var prefabs = GetPrefabs().Where(targetPrefabs.Contains).ToList();
                var scenePaths = AssetDatabase.FindAssets("t:scene").Select(AssetDatabase.GUIDToAssetPath).Where(targetScenePath.Contains).ToList();
                float totalCount = prefabs.Count + scenePaths.Count;

                MigratePrefabsImpl(prefabs, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 1)",
                    $"{name} (Prefabs) ({i} / {totalCount})",
                    i / totalCount),
                    migrator.Migration
                    );

                MigrateAllScenesImpl(scenePaths, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 1)",
                    $"{name} (Scenes) ({prefabs.Count + i} / {totalCount})",
                    (prefabs.Count + i) / totalCount)
                    , migrator.Migration
                    );

                if (migrator is IMigratorUseFinalize migratorUseFinalize)
                {
                    MigratePrefabsImpl(prefabs, (name, i) => EditorUtility.DisplayProgressBar(
                        "Migrating Everything (pass 2)",
                        $"{name} (Prefabs) ({i} / {totalCount})",
                        i / totalCount)
                        , migratorUseFinalize.MigrationFinalize
                        );

                    MigrateAllScenesImpl(scenePaths, (name, i) => EditorUtility.DisplayProgressBar(
                        "Migrating Everything (pass 2)",
                        $"{name} (Scenes) ({prefabs.Count + i} / {totalCount})",
                        (prefabs.Count + i) / totalCount)
                        , migratorUseFinalize.MigrationFinalize
                        );
                }
            }
            catch
            {
                EditorUtility.DisplayDialog("Error!", "Error in migration process!", "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (!continuesMigrate) PostMigration();
            }
        }
        private static void MigrateEverythingFor<Migrator>(bool continuesMigrate = false)
        where Migrator : IMigrator, new()
        {
            try
            {
                if (!continuesMigrate) PreMigration();

                var migrator = new Migrator();
                var prefabs = GetPrefabs();
                var scenePaths = AssetDatabase.FindAssets("t:scene").Select(AssetDatabase.GUIDToAssetPath).ToList();
                float totalCount = prefabs.Count + scenePaths.Count;

                MigratePrefabsImpl(prefabs, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 1)",
                    $"{name} (Prefabs) ({i} / {totalCount})",
                    i / totalCount),
                    migrator.Migration
                    );

                MigrateAllScenesImpl(scenePaths, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 1)",
                    $"{name} (Scenes) ({prefabs.Count + i} / {totalCount})",
                    (prefabs.Count + i) / totalCount)
                    , migrator.Migration
                    );

                if (migrator is IMigratorUseFinalize migratorUseFinalize)
                {
                    MigratePrefabsImpl(prefabs, (name, i) => EditorUtility.DisplayProgressBar(
                        "Migrating Everything (pass 2)",
                        $"{name} (Prefabs) ({i} / {totalCount})",
                        i / totalCount)
                        , migratorUseFinalize.MigrationFinalize
                        );

                    MigrateAllScenesImpl(scenePaths, (name, i) => EditorUtility.DisplayProgressBar(
                        "Migrating Everything (pass 2)",
                        $"{name} (Scenes) ({prefabs.Count + i} / {totalCount})",
                        (prefabs.Count + i) / totalCount)
                        , migratorUseFinalize.MigrationFinalize
                        );
                }

                MigrationUtility.WriteVersion(migrator.MigrateTarget + 1);
            }
            catch
            {
                EditorUtility.DisplayDialog("Error!", "Error in migration process!", "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (!continuesMigrate) PostMigration();
            }
        }

        private static void MigratePrefabsImpl(List<GameObject> prefabAssets, Action<string, int> progressCallback, Func<ITexTransToolTag, bool> migrator)
        {
            for (var i = 0; i < prefabAssets.Count; i++)
            {
                var prefabPath = AssetDatabase.GetAssetPath(prefabAssets[i]);
                var prefabAsset = PrefabUtility.LoadPrefabContents(prefabPath);
                progressCallback(prefabAsset.name, i);
                AssetDatabase.OpenAsset(prefabAsset.GetInstanceID());

                try
                {
                    foreach (var component in prefabAsset.GetComponentsInChildren<ITexTransToolTag>(true))
                        migrator(component);
                }
                catch (Exception e)
                {
                    throw new Exception($"Migrating Prefab {prefabAsset.name}: {e.Message}", e);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabAsset, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabAsset);
            }
            progressCallback("finish Prefabs", prefabAssets.Count);
        }

        private static void MigrateAllScenesImpl(List<string> scenePaths, Action<string, int> progressCallback,
            Func<ITexTransToolTag, bool> migrator)
        {
            // load each scene and migrate scene
            for (var i = 0; i < scenePaths.Count; i++)
            {
                var scenePath = scenePaths[i];
                if (IsReadOnlyPath(scenePath))
                    continue;

                if (Path.GetExtension(scenePath) != ".unity") { Debug.LogWarning($"{scenePath} is invalid file extension"); continue; }
                var scene = EditorSceneManager.OpenScene(scenePath);

                progressCallback(scene.name, i);

                var modified = false;

                try
                {
                    foreach (var rootGameObject in scene.GetRootGameObjects())
                        foreach (var component in rootGameObject.GetComponentsInChildren<ITexTransToolTag>(true))
                            modified |= migrator(component);
                }
                catch (Exception e)
                {
                    throw new Exception($"Migrating Scene {scene.name}: {e.Message}", e);
                }

                if (modified)
                    EditorSceneManager.SaveScene(scene);
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            progressCallback("finish Scenes", scenePaths.Count);
        }


        private static (string Path, bool IsLoaded)[] _openingSceneInfos;
        private static void PreMigration()
        {
            var scenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
            if (scenes.Any(x => x.isDirty))
                EditorSceneManager.SaveScenes(scenes);
            _openingSceneInfos = scenes.Select(x => (x.path, x.isLoaded)).ToArray();
            if (_openingSceneInfos.Any(x => string.IsNullOrEmpty(x.Path)))
                _openingSceneInfos = null;
        }
        private static void PostMigration()
        {

            if (_openingSceneInfos != null
                && EditorUtility.DisplayDialog("Reopen?", "以前に開いたシーンを開きなおしますか?(Do you want to reopen previously opened scenes?)", "Yes",
                    "No"))
            {
                var scenes = _openingSceneInfos;
                for (int i = 0; i < scenes.Length; i++)
                {
                    EditorSceneManager.OpenScene(scenes[i].Path, i == 0 ? OpenSceneMode.Single : scenes[i].IsLoaded ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
                }
            }
        }






    }
}

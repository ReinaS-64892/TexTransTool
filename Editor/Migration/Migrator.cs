#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using net.rs64.TexTransTool.Migration.V0;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


[assembly: InternalsVisibleTo("net.rs64.tex-trans-tool.Inspector")]

namespace net.rs64.TexTransTool.Migration
{
    [InitializeOnLoad]
    internal static class Migrator
    {
        public static string SaveDataVersionPath = "ProjectSettings/net.rs64.TexTransTool-Version.json";

        static Migrator()
        {
            if (!File.Exists(SaveDataVersionPath))
            {
                MigrationUtility.WriteVersion(ToolUtils.ThiSaveDataVersion);
            }
        }
        [Serializable]
        internal class SaveDataVersionJson
        {
            public int SaveDataVersion;
        }




        public static bool MigrationITexTransToolTagV0ToV1(ITexTransToolTag texTransToolTag)
        {
#pragma warning disable CS0612
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV0.MigrationAtlasTextureV0ToV1(atlasTexture, false);
                        return true;
                    }
                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 1);
                        return true;
                    }
            }
#pragma warning restore CS0612
        }
        public static bool RemoveSaveDataVersionV0(ITexTransToolTag texTransToolTag)
        {
            if (texTransToolTag.SaveDataVersion == 0)
            {
                UnityEngine.Object.DestroyImmediate(texTransToolTag as MonoBehaviour);
                return true;
            }
            else
            {
                return false;
            }
        }


        //https://github.com/anatawa12/AvatarOptimizer/blob/25bf2e68f93705808d8d2cc6b7c4449f57c990a8/Editor/Migration/Migration.cs#L841C43-L841C43
        //Originally under MIT License
        //Copyright (c) 2022 anatawa12
        #region CopyFromAAOCode


        [MenuItem("Tools/TexTransTool/Migrate Everything v0.3.x to v0.4x")]
        private static void MigrateEverything()
        {
            try
            {
                PreMigration();
                var prefabs = GetPrefabs();
                var scenePaths = AssetDatabase.FindAssets("t:scene").Select(AssetDatabase.GUIDToAssetPath).ToList();
                float totalCount = prefabs.Count + scenePaths.Count;

                MigratePrefabs(prefabs, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 1)",
                    $"{name} (Prefabs) ({i} / {totalCount})",
                    i / totalCount));

                MigrateAllScenes(scenePaths, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 1)",
                    $"{name} (Scenes) ({prefabs.Count + i} / {totalCount})",
                    (prefabs.Count + i) / totalCount));

                MigratePrefabsPass2(prefabs, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 2)",
                    $"{name} (Prefabs) ({i} / {totalCount})",
                    i / totalCount));

                MigrateAllScenesPass2(scenePaths, (name, i) => EditorUtility.DisplayProgressBar(
                    "Migrating Everything (pass 2)",
                    $"{name} (Scenes) ({prefabs.Count + i} / {totalCount})",
                    (prefabs.Count + i) / totalCount));

                MigrationUtility.WriteVersion(1);

            }
            catch
            {
                EditorUtility.DisplayDialog("Error!", "Error in migration process!", "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                PostMigration();
            }
        }
        private static string[] _openingScenePaths;

        private static void PreMigration()
        {
            var scenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
            if (scenes.Any(x => x.isDirty))
                EditorSceneManager.SaveScenes(scenes);
            _openingScenePaths = scenes.Select(x => x.path).ToArray();
            if (_openingScenePaths.Any(string.IsNullOrEmpty))
                _openingScenePaths = null;
        }

        private static void PostMigration()
        {

            if (_openingScenePaths != null
                && EditorUtility.DisplayDialog("Reopen?", "以前に開いたシーンを開きなおしますか?(Do you want to reopen previously opened scenes?)", "Yes",
                    "No"))
            {
                EditorSceneManager.OpenScene(_openingScenePaths[0]);
                foreach (var openingScenePath in _openingScenePaths.Skip(1))
                    EditorSceneManager.OpenScene(openingScenePath, OpenSceneMode.Additive);
            }
        }

        /// <returns>List of prefab assets. parent prefab -> child prefab</returns>
        private static List<GameObject> GetPrefabs()
        {
            bool CheckPrefabType(PrefabAssetType type) =>
                type != PrefabAssetType.MissingAsset && type != PrefabAssetType.Model &&
                type != PrefabAssetType.NotAPrefab;

            var allPrefabRoots = AssetDatabase.FindAssets("t:prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(s => !IsReadOnlyPath(s))
                .Select(path => (PrefabUtility.LoadPrefabContents(path), path))
                .Where(x => x.Item1)
                .Where(x => CheckPrefabType(PrefabUtility.GetPrefabAssetType(x.Item1)))
                .Where(x => x.Item1.GetComponentsInChildren<ITexTransToolTag>(true).Length != 0)
                .ToArray();

            var sortedVertices = new List<GameObject>();

            var vertices = new LinkedList<PrefabInfo>(allPrefabRoots.Select(prefabRoot => new PrefabInfo(prefabRoot.Item1)));

            // assign Parents and Children here.
            {
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
            }

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
        private static bool IsReadOnlyPath(string path)
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);

            return packageInfo != null && packageInfo.source != PackageSource.Embedded && packageInfo.source != PackageSource.Local;
        }
        private class PrefabInfo
        {
            public readonly GameObject Prefab;
            public readonly List<PrefabInfo> Children = new List<PrefabInfo>();
            public readonly List<PrefabInfo> Parents = new List<PrefabInfo>();

            public PrefabInfo(GameObject prefab)
            {
                Prefab = prefab;
            }
        }
        private static void MigratePrefabs(List<(GameObject, string)> prefabAssets, Action<string, int> progressCallback)
        {
            MigratePrefabsImpl(prefabAssets, progressCallback, MigrationITexTransToolTagV0ToV1);
        }

        private static void MigratePrefabsPass2(List<(GameObject, string)> prefabAssets, Action<string, int> progressCallback)
        {
            MigratePrefabsImpl(prefabAssets, progressCallback, RemoveSaveDataVersionV0);
        }

        private static void MigratePrefabsImpl(List<(GameObject, string)> prefabAssets, Action<string, int> progressCallback, Func<ITexTransToolTag, bool> migrator)
        {
            for (var i = 0; i < prefabAssets.Count; i++)
            {
                var (prefabAsset, path) = prefabAssets[i];
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

                PrefabUtility.SaveAsPrefabAsset(prefabAsset, path);
                PrefabUtility.UnloadPrefabContents(prefabAsset);
            }
            progressCallback("finish Prefabs", prefabAssets.Count);
        }

        private static void MigrateAllScenes(List<string> scenePaths, Action<string, int> progressCallback)
        {
            MigrateAllScenesImpl(scenePaths, progressCallback, MigrationITexTransToolTagV0ToV1);
        }

        private static void MigrateAllScenesPass2(List<string> scenePaths, Action<string, int> progressCallback)
        {
            MigrateAllScenesImpl(scenePaths, progressCallback, RemoveSaveDataVersionV0);
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
        #endregion


    }
    internal static class MigrationUtility
    {
        public static void WriteVersion(int value)
        {
            var NawSaveDataVersion = new Migrator.SaveDataVersionJson();
            NawSaveDataVersion.SaveDataVersion = value;
            var newJsonStr = JsonUtility.ToJson(NawSaveDataVersion);

            File.WriteAllText(Migrator.SaveDataVersionPath, newJsonStr);
        }
        public static void SetSaveDataVersion(ITexTransToolTag texTransToolTag, int value)
        {
            var sObj = new SerializedObject(texTransToolTag as UnityEngine.Object);
            var saveDataProp = sObj.FindProperty("_saveDataVersion");
            if (saveDataProp == null) { Debug.LogWarning(texTransToolTag.GetType() + " : SaveDataVersionの書き換えができませんでした。"); }
            saveDataProp.intValue = value;
            sObj.ApplyModifiedPropertiesWithoutUndo();
        }



    }
}
#endif
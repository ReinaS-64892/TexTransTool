using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.Preview.RealTime
{
    internal class RealTimePreviewContext : ScriptableSingleton<RealTimePreviewContext>
    {
        RealTimePreviewDomain _previewDomain = null;
        Dictionary<TexTransRuntimeBehavior, int> _texTransRuntimeBehaviorPriority = new();

        HashSetQueue<TexTransRuntimeBehavior> _updateQueue = new();

        Dictionary<int, HashSet<TexTransRuntimeBehavior>> _dependencyAndContainsMap = new();




        protected RealTimePreviewContext()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitRealTimePreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitRealTimePreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
            DestroyCall.OnDestroy -= DestroyObserve;
            DestroyCall.OnDestroy += DestroyObserve;
        }


        public void EnterRealtimePreview(GameObject previewRoot)
        {
            if (_previewDomain != null) { UnityEngine.Debug.Log("Already preview !!!"); return; }
            if (AnimationMode.InAnimationMode()) { UnityEngine.Debug.Log("Other preview now !!!"); return; }

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();

            _previewDomain = new RealTimePreviewDomain(previewRoot);

            var texTransBehaviors = AvatarBuildUtils.PhaseDictFlatten(AvatarBuildUtils.FindAtPhaseAll(previewRoot));
            var priority = 0;
            foreach (var ttb in texTransBehaviors)
            { if (ttb is TexTransRuntimeBehavior texTransRuntimeBehavior) { _texTransRuntimeBehaviorPriority[texTransRuntimeBehavior] = priority; priority += 1; } }


            ObjectChangeEvents.changesPublished += ListenChangeEvent;
            EditorApplication.update += UpdatePreview;

            foreach (var ttb in texTransBehaviors) { AddPreviewBehavior(ttb as TexTransRuntimeBehavior); }
        }

        void AddPreviewBehavior(TexTransRuntimeBehavior texTransRuntimeBehavior)
        {
            switch (texTransRuntimeBehavior)
            {
                default: { return; }
                case SimpleDecal:
                case MultiLayerImageCanvas:
                    { break; }
            }
            if (ContainsBehavior(texTransRuntimeBehavior)) { return; }

            foreach (var dependInstanceID in texTransRuntimeBehavior.GetDependency().Append(texTransRuntimeBehavior).Select(g => g.GetInstanceID()))
            {
                if (!_dependencyAndContainsMap.ContainsKey(dependInstanceID)) { _dependencyAndContainsMap[dependInstanceID] = new(); }
                _dependencyAndContainsMap[dependInstanceID].Add(texTransRuntimeBehavior);
            }
            _updateQueue.Enqueue(texTransRuntimeBehavior);
        }
        void RemovePreviewBehavior(TexTransRuntimeBehavior texTransRuntimeBehavior) { foreach (var depend in _dependencyAndContainsMap) { depend.Value.Remove(texTransRuntimeBehavior); } }
        bool ContainsBehavior(TexTransRuntimeBehavior texTransRuntimeBehavior)
        {
            foreach (var dependKey in _dependencyAndContainsMap)
            {
                if (dependKey.Value.Contains(texTransRuntimeBehavior)) { return true; }
            }
            return false;
        }

        void ListenChangeEvent(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; stream.length > i; i += 1)
            {
                switch (stream.GetEventType(i))
                {
                    case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                        {
                            stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var data);
                            DependEnqueueOfInstanceID(data.instanceId);
                            break;
                        }
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                    case ObjectChangeKind.ChangeChildrenOrder:
                    case ObjectChangeKind.ChangeGameObjectParent:
                        { RealTimePreviewRestart(); break; }
                    case ObjectChangeKind.ChangeScene:
                        { ExitRealTimePreview(); break; }

                }
            }

            void DependEnqueueOfInstanceID(int instanceId)
            {
                if (_dependencyAndContainsMap.ContainsKey(instanceId))
                {
                    foreach (var t in _dependencyAndContainsMap[instanceId]) { _updateQueue.Enqueue(t); }
                }
            }
            void RealTimePreviewRestart()
            {

            }
        }

        void UpdatePreview()
        {
            if (_updateQueue.TryDequeue(out var texTransRuntimeBehavior) && _texTransRuntimeBehaviorPriority.TryGetValue(texTransRuntimeBehavior, out var priority))
            {
                _previewDomain.NowPriority = priority;
                _previewDomain.PreviewStackManager.ReleaseStackOfPriority(priority);

                texTransRuntimeBehavior.Apply(_previewDomain);

                _previewDomain.UpdateNeeded();
                _previewDomain.NowPriority = -1;
            }
        }



        private void ExitPreview(Scene scene, bool removingScene) => ExitRealTimePreview();
        public void ExitRealTimePreview()
        {
            if (_previewDomain == null) { return; }
            ObjectChangeEvents.changesPublished -= ListenChangeEvent;
            EditorApplication.update -= UpdatePreview;

            _texTransRuntimeBehaviorPriority.Clear();
            _dependencyAndContainsMap.Clear();
            _updateQueue.Clear();
            _previewDomain.PreviewExit();
            _previewDomain = null;
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();
        }

        private void DestroyObserve(TexTransBehavior behavior)
        {
            if (_previewDomain == null) { return; }
            var runtimeBehavior = behavior as TexTransRuntimeBehavior;
            if (runtimeBehavior == null) { return; }
            if (!ContainsBehavior(runtimeBehavior)) { return; }

            var stackManager = _previewDomain.PreviewStackManager;
            var updateTarget = stackManager.FindAtPriority(_texTransRuntimeBehaviorPriority[runtimeBehavior]);
            stackManager.ReleaseStackOfPriority(_texTransRuntimeBehaviorPriority[runtimeBehavior]);
            foreach (var tex in updateTarget) { stackManager.UpdateStack(tex); }
        }

        public bool IsPreview() => _previewDomain != null;
    }

    internal class HashSetQueue<T> : IEnumerable<T>
    {
        HashSet<T> _hash = new();
        Queue<T> _queue = new();

        public bool Enqueue(T item)
        {
            if (_hash.Contains(item)) return false;
            _hash.Add(item);
            _queue.Enqueue(item);
            return true;
        }

        public T Dequeue()
        {
            var item = _queue.Dequeue();
            _hash.Remove(item);
            return item;
        }
        public bool TryDequeue(out T item)
        {
            if (_queue.TryDequeue(out item)) { _hash.Remove(item); return true; }
            return false;
        }

        public T Peek() { return _queue.Peek(); }

        public void Clear()
        {
            _hash.Clear();
            _queue.Clear();
        }

        public IEnumerator<T> GetEnumerator() { return _queue.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return _queue.GetEnumerator(); }
    }

}

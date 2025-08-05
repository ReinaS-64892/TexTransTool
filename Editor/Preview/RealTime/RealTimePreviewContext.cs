#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace net.rs64.TexTransTool.Preview.RealTime
{
    internal class RealTimePreviewContext : ScriptableSingleton<RealTimePreviewContext>
    {
        RealTimePreviewDomain? _previewDomain = null;
        Dictionary<TexTransBehavior, int> _PriorityMap = new();
        Dictionary<int, HashSet<TexTransBehavior>> _dependencyMap = new();
        HashSetQueue<TexTransBehavior> _updateQueue = new();


        public event Action<GameObject>? OnPreviewEnter;
        public event Action? OnPreviewExit;

        public static bool IsPreviewPossibleType(TexTransBehavior ttr)
        {
            switch (ttr)
            {
                default: { return false; }
                case SimpleDecal:
                case MultiLayerImageCanvas:
                case SingleGradationDecal:
                    { return true; }
            }
        }


        protected RealTimePreviewContext()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitRealTimePreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitRealTimePreview;
            EditorSceneManager.sceneClosing -= ExitRealTimePreview;
            EditorSceneManager.sceneClosing += ExitRealTimePreview;
        }


        public void EnterRealtimePreview(GameObject previewRoot)
        {
            if (_previewDomain != null) { UnityEngine.Debug.Log("Already preview !!!"); return; }
            if (AnimationMode.InAnimationMode()) { UnityEngine.Debug.Log("Other preview now !!!"); return; }

            OnPreviewEnter?.Invoke(previewRoot);

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();

            _previewDomain = new RealTimePreviewDomain(previewRoot, RegisterDependency);

            var texTransBehaviors = AvatarBuildUtils.PhaseDictFlatten(TexTransBehaviorSearch.FindAtPhase(previewRoot)).Where(b => TexTransBehaviorSearch.CheckIsActiveBehavior(b, previewRoot));
            var priority = 0;
            foreach (var ttb in texTransBehaviors)
            { if (ttb is TexTransBehavior texTransRuntimeBehavior) { _PriorityMap[texTransRuntimeBehavior] = priority; priority += 1; } }


            ObjectChangeEvents.changesPublished += ListenChangeEvent;
            EditorApplication.update += UpdatePreview;

            foreach (var ttb in texTransBehaviors) { AddPreviewBehavior(ttb as TexTransBehavior); }
        }
        void RegisterDependency(TexTransBehavior texTransRuntimeBehavior, int dependInstanceID)
        {
            if (!_dependencyMap.ContainsKey(dependInstanceID)) { _dependencyMap[dependInstanceID] = new(); }
            _dependencyMap[dependInstanceID].Add(texTransRuntimeBehavior);
        }

        void AddPreviewBehavior(TexTransBehavior texTransRuntimeBehavior)
        {
            if (ContainsBehavior(texTransRuntimeBehavior)) { return; }
            if (IsPreviewPossibleType(texTransRuntimeBehavior) is false) { return; }

            _updateQueue.Enqueue(texTransRuntimeBehavior);
        }

        void UnRegisterDependency(TexTransBehavior texTransRuntimeBehavior) { foreach (var depend in _dependencyMap) { depend.Value.Remove(texTransRuntimeBehavior); } }
        bool ContainsBehavior(TexTransBehavior texTransRuntimeBehavior)
        {
            foreach (var dependKey in _dependencyMap)
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
                    case ObjectChangeKind.ChangeAssetObjectProperties:
                        {
                            stream.GetChangeAssetObjectPropertiesEvent(i, out var data);
                            DependEnqueueOfInstanceID(data.instanceId);
                            break;
                        }

                    case ObjectChangeKind.CreateGameObjectHierarchy:
                    case ObjectChangeKind.ChangeGameObjectParent:
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                    case ObjectChangeKind.ChangeChildrenOrder:
                        { RealTimePreviewRestart(); break; }
                    case ObjectChangeKind.ChangeScene:
                        { ExitRealTimePreview(); break; }

                }
            }

            void DependEnqueueOfInstanceID(int instanceId)
            {
                if (_dependencyMap.ContainsKey(instanceId))
                {
                    foreach (var t in _dependencyMap[instanceId]) { _updateQueue.Enqueue(t); }
                }
            }
        }

        internal void RealTimePreviewRestart()
        {
            if (_previewDomain is null) { Debug.Assert(false); return; }
            var domainRoot = _previewDomain.DomainRoot;
            ExitRealTimePreview();
            EnterRealtimePreview(domainRoot);
        }

        void UpdatePreview()
        {
            if (_previewDomain is null) { Debug.Assert(false); return; }
            Profiler.BeginSample("UpdatePreview");
            if (_updateQueue.TryDequeue(out var texTransRuntimeBehavior) && _PriorityMap.TryGetValue(texTransRuntimeBehavior, out var priority))
            {
                Profiler.BeginSample("UnRegisterDependency");
                UnRegisterDependency(texTransRuntimeBehavior);
                Profiler.EndSample();

                Profiler.BeginSample("Apply");
                _previewDomain.SetNowBehavior(texTransRuntimeBehavior, priority);
                try { if (texTransRuntimeBehavior.gameObject.activeInHierarchy) { texTransRuntimeBehavior.Apply(_previewDomain); } }
                catch (Exception ex) { Debug.LogException(ex); }
                Profiler.EndSample();

                Profiler.BeginSample("NeededStackUpdate");
                _previewDomain.UpdateNeeded();
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }



        private void ExitRealTimePreview(Scene scene, bool removingScene) => ExitRealTimePreview();
        public void ExitRealTimePreview()
        {
            if (_previewDomain == null) { return; }
            ObjectChangeEvents.changesPublished -= ListenChangeEvent;
            EditorApplication.update -= UpdatePreview;

            _PriorityMap.Clear();
            _dependencyMap.Clear();
            _updateQueue.Clear();
            _previewDomain.PreviewExit();
            _previewDomain = null;
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();

            OnPreviewExit?.Invoke();
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

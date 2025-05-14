#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class TexTransBehaviorSearch
    {
        public interface IGameObjectWakingTool
        {
            C[] GetComponentsInChildren<C>(GameObject gameObject, bool includeInactive) where C : Component;
            C GetComponent<C>(GameObject gameObject) where C : Component;
            int GetChilesCount(GameObject gameObject);
            GameObject? GetChilde(GameObject gameObject, int index);
        }
        public interface IGameObjectActivenessWakingTool
        {
            C GetComponent<C>(GameObject gameObject) where C : Component;
            GameObject? GetParent(GameObject gameObject);
            bool ActiveSelf(GameObject gameObject);
        }
        public struct DefaultGameObjectWakingTool : IGameObjectWakingTool, IGameObjectActivenessWakingTool
        {

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public GameObject? GetChilde(GameObject gameObject, int index)
            {
                return gameObject.transform.GetChild(index)?.gameObject;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetChilesCount(GameObject gameObject)
            {
                return gameObject.transform.childCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public C GetComponent<C>(GameObject gameObject) where C : Component
            {
                return gameObject.GetComponent<C>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public C[] GetComponentsInChildren<C>(GameObject gameObject, bool includeInactive) where C : Component
            {
                return gameObject.GetComponentsInChildren<C>(includeInactive);
            }

            public GameObject? GetParent(GameObject gameObject)
            {
                return gameObject.transform.parent?.gameObject;
            }
            public bool ActiveSelf(GameObject gameObject)
            {
                return gameObject.activeSelf;
            }
        }
        /*
            PhaseDefinition は常に最上段にあるものが有効な扱いになる。
            後は上から順。
        */
        public static Dictionary<TexTransPhase, List<TexTransBehavior>> FindAtPhase(GameObject rootDomainObject)
        { return FindAtPhase(rootDomainObject, new DefaultGameObjectWakingTool()); }
        public static Dictionary<TexTransPhase, List<TexTransBehavior>> FindAtPhase<WakingTool>(GameObject rootDomainObject, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var behavior = Correct(rootDomainObject, wakingTool);
            var phasedBehaviour = TexTransPhaseUtility.GeneratePhaseDictionary<List<TexTransBehavior>>();

            foreach (var pd in behavior.PhaseDefinitions) GroupedComponentsCorrect(phasedBehaviour[pd.TexTransPhase], pd.gameObject, wakingTool);
            foreach (var ttb in behavior.OtherBehaviors) phasedBehaviour[ttb.PhaseDefine].Add(ttb);

            return phasedBehaviour;
        }
        class CorrectingResult
        {
            public List<PhaseDefinition> PhaseDefinitions = new();
            public List<TexTransBehavior> OtherBehaviors = new();
        }

        static CorrectingResult Correct<WakingTool>(GameObject wakingPoint, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var wakingResult = new CorrectingResult();
            Correct(wakingResult, wakingPoint, wakingTool);
            return wakingResult;
        }
        static void Correct<WakingTool>(CorrectingResult wakingResult, GameObject wakingPoint, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var chilesCount = wakingTool.GetChilesCount(wakingPoint);
            for (var i = 0; chilesCount > i; i += 1)
            {
                var cObject = wakingTool.GetChilde(wakingPoint, i)!;
                var ownedComponent = wakingTool.GetComponent<TexTransMonoBaseGameObjectOwned>(cObject);

                if (ownedComponent != null)
                    switch (ownedComponent)
                    {
                        default: break;
                        case PhaseDefinition pd: wakingResult.PhaseDefinitions.Add(pd); break;
                        case TexTransBehavior ttb: wakingResult.OtherBehaviors.Add(ttb); break;
                    }
                else
                    Correct(wakingResult, cObject, wakingTool);
            }
        }

        internal static void GroupedComponentsCorrect<WakingTool>(List<TexTransBehavior> behaviors, GameObject wakingPoint, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var chilesCount = wakingTool.GetChilesCount(wakingPoint);
            for (var i = 0; chilesCount > i; i += 1)
            {
                var cObject = wakingTool.GetChilde(wakingPoint, i)!;
                var component = wakingTool.GetComponent<TexTransBehavior>(cObject);

                if (component != null) behaviors.Add(component);
                else GroupedComponentsCorrect(behaviors, cObject, wakingTool);
            }
        }
        public static bool CheckIsActiveBehavior(TexTransBehavior behavior, GameObject? domainRoot = null)
        { return CheckIsActive(behavior.gameObject, new DefaultGameObjectWakingTool(), domainRoot); }
        public static bool CheckIsActive(GameObject entry, GameObject? domainRoot = null)
        { return CheckIsActive(entry, new DefaultGameObjectWakingTool(), domainRoot); }
        public static bool CheckIsActive<WakingTool>(GameObject entry, WakingTool wakingTool, GameObject? domainRoot = null)
        where WakingTool : IGameObjectActivenessWakingTool
        {
            var wakingPoint = entry;
            while (wakingPoint != domainRoot)
            {
                if (wakingTool.GetComponent<IsActiveInheritBreaker>(wakingPoint) != null) { return true; }
                if (wakingTool.ActiveSelf(wakingPoint) is false) { return false; }

                wakingPoint = wakingTool.GetParent(wakingPoint);

                if (wakingPoint == null) { break; }// safety
            }
            return true;
        }
    }
}

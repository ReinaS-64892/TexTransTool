using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Profiling;

namespace net.rs64.TexTransUnityCore
{
    public static class TexTransCoreRuntime
    {
        public static Action Update = () => { NextUpdateCall?.Invoke(); NextUpdateCall = null; };
        public static Action NextUpdateCall;
        public static Func<string, Type, UnityEngine.Object> LoadAsset;
        public static Func<Type, IEnumerable<UnityEngine.Object>> LoadAssetsAtType;
        public static Dictionary<Type, Action> NewAssetListen = new();

    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class TexTransInitialize : System.Attribute
    {
        public TexTransInitialize()
        {
        }

        public static void CallInitialize()//シェーダー等がロードさている状態を想定している。
        {
            Profiler.BeginSample("FindInitializers");
            var initializers = TexTransToolAssembly().SelectMany(i => i.GetTypes().SelectMany(t => t.GetRuntimeMethods()))
            .Where(i => i.IsStatic && i.GetCustomAttribute<TexTransInitialize>() is not null)
            .Select(i => (Action)i.CreateDelegate(typeof(Action))).ToArray();
            Profiler.EndSample();

            Profiler.BeginSample("CallInitializers");
            foreach (var method in initializers)
            {
                Profiler.BeginSample("Call:" + method.Method.Name);
                method();
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }

        internal static IEnumerable<Assembly> TexTransToolAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(i => i.FullName.Contains("net.rs64"));
        }
    }

}

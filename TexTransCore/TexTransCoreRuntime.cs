using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransCore
{
    public static class TexTransCoreRuntime
    {
        public static Action Update = () => { NextUpdateCall?.Invoke(); NextUpdateCall = null; };
        public static Action NextUpdateCall;
        public static Func<string, Type, UnityEngine.Object> LoadAsset;
        public static Func<Type, IEnumerable<UnityEngine.Object>> LoadAssetsAtType;

    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class TexTransInitialize : System.Attribute
    {
        public TexTransInitialize()
        {
        }

        public static void CallInitialize()//シェーダー等がロードさている状態を想定している。
        {
            var initializers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(i => i.GetTypes())
            .SelectMany(i => i.GetRuntimeMethods())
            .Where(i => i.IsStatic)
            .Where(i => i.GetCustomAttribute<TexTransInitialize>() is not null)
            .Select(i => (Action)i.CreateDelegate(typeof(Action)));

            foreach (var method in initializers) { method(); }
        }


    }

}

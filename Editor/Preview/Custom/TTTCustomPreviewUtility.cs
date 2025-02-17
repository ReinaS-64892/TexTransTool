using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.Preview.Custom
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class TTTCustomPreviewAttribute : System.Attribute// TODO : このあたりの属性とインターフェイスで特定の型に対して特定のインターフェースを当てる部分を共通の仕組みにしてもいいかもな～
    {
        public Type PreviewTargetType { get; }
        public TTTCustomPreviewAttribute(Type processorTargetType)
        {
            PreviewTargetType = processorTargetType;
        }
    }
    internal interface ITTTCustomPreview
    {
        void Preview(TexTransMonoBase texTransBehavior, GameObject domainRoot, RenderersDomain editorCallDomain);
    }
    internal static class TTTCustomPreviewUtility
    {
        static Dictionary<Type, ITTTCustomPreview> s_processor;

        [InitializeOnLoadMethod]
        public static void InitProcessor()
        {
            var processorTypes = AppDomain.CurrentDomain.GetAssemblies()
             .SelectMany(t => t.GetTypes())
             .Where(type => type.GetInterfaces().Any(i => i == typeof(ITTTCustomPreview)))
             .Where(i => !i.IsAbstract)
             .Where(i => i.GetCustomAttributes<TTTCustomPreviewAttribute>().Any())
             .SelectMany(type => type.GetCustomAttributes<TTTCustomPreviewAttribute>().Select(customAttribute => (customAttribute, type)));

            var processorDict = new Dictionary<Type, ITTTCustomPreview>();

            foreach (var processorType in processorTypes)
            {
                if (processorDict.ContainsKey(processorType.customAttribute.PreviewTargetType))
                { Debug.LogWarning(processorType.customAttribute.PreviewTargetType.FullName + " is Duplicate for " + processorType.type.FullName); return; }

                try
                {
                    processorDict[processorType.customAttribute.PreviewTargetType] = (ITTTCustomPreview)Activator.CreateInstance(processorType.type);
                }
                catch (Exception e) { Debug.LogError(processorType.type.ToString()); throw e; }
            }

            s_processor = processorDict;
        }


        internal static bool TryExecutePreview(TexTransMonoBase texTransBehavior, GameObject domainRoot, RenderersDomain editorCallDomain)//trueだったらカスタムプレビューしたよってこと、そうでなければカスタムプレビューはないってこと。
        {
            if (s_processor is null) { InitProcessor(); }

            var targetType = texTransBehavior.GetType();
            if (!s_processor.ContainsKey(targetType)) { return false; }

            s_processor[targetType].Preview(texTransBehavior, domainRoot, editorCallDomain);
            return true;
        }
    }
}

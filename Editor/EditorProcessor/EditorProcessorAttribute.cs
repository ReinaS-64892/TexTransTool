using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.EditorProcessor
{

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class EditorProcessorAttribute : System.Attribute
    {
        public Type ProcessorTargetType { get; }
        public EditorProcessorAttribute(Type processorTargetType)
        {
            ProcessorTargetType = processorTargetType;
        }
    }

    internal interface IEditorProcessor
    {
        void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain domain);
        IEnumerable<Renderer> ModificationTargetRenderers(TexTransCallEditorBehavior texTransCallEditorBehavior, IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking);
    }

    internal static class EditorProcessorUtility
    {
        static Dictionary<Type, IEditorProcessor> s_processor;

        [InitializeOnLoadMethod]
        public static void InitProcessor()
        {
            var processorTypes = AppDomain.CurrentDomain.GetAssemblies()
             .SelectMany(t => t.GetTypes())
             .Where(type => type.GetInterfaces().Any(i => i == typeof(IEditorProcessor)))
             .Where(i => !i.IsAbstract)
             .Where(i => i.GetCustomAttributes<EditorProcessorAttribute>().Any())
             .SelectMany(type => type.GetCustomAttributes<EditorProcessorAttribute>().Select(customAttribute => (customAttribute, type)));

            var processorDict = new Dictionary<Type, IEditorProcessor>();

            foreach (var processorType in processorTypes)
            {
                if (processorDict.ContainsKey(processorType.customAttribute.ProcessorTargetType))
                { Debug.LogWarning(processorType.customAttribute.ProcessorTargetType.FullName + " is Duplicate for " + processorType.type.FullName); return; }

                try
                {
                    processorDict[processorType.customAttribute.ProcessorTargetType] = (IEditorProcessor)Activator.CreateInstance(processorType.type);
                }
                catch (Exception e) { Debug.LogError(processorType.type.ToString()); throw e; }
            }

            s_processor = processorDict;
        }


        private static IEditorProcessor GetPresserType(Type texTransCallEditorBehaviorType)
        {
            if (s_processor is null) { InitProcessor(); }

            var targetType = texTransCallEditorBehaviorType;
            if (!s_processor.ContainsKey(targetType)) { throw new EditorProcessorNotFound(targetType); }

            return s_processor[targetType];
        }
        public static void CallProcessorApply(this TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain editorCallDomain)
        {
            GetPresserType(texTransCallEditorBehavior.GetType()).Process(texTransCallEditorBehavior, editorCallDomain);
        }

        public static IEnumerable<Renderer> CallProcessorModificationTargetRenderers(this TexTransCallEditorBehavior texTransCallEditorBehavior, IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            return GetPresserType(texTransCallEditorBehavior.GetType()).ModificationTargetRenderers(texTransCallEditorBehavior, domainRenderers, replaceTracking);
        }

    }


    public sealed class EditorProcessorNotFound : TTTException
    {
        public Type TargetType { get; }
        public EditorProcessorNotFound(Type type, params object[] additionalMessage) : base("Processor Not Found : " + type.FullName, additionalMessage) { TargetType = type; }
        public EditorProcessorNotFound(params object[] additionalMessage) : base("Processor Not Found", additionalMessage) { }
        public EditorProcessorNotFound(string message, params object[] additionalMessage) : base(message, additionalMessage) { }
        public EditorProcessorNotFound(string message, System.Exception inner) : base(message, inner) { }
    }
}

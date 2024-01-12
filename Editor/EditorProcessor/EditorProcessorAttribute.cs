#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
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
        void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain);
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


        public static void CallProcessorApply(this TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain)
        {
            if (s_processor is null) { InitProcessor(); }

            var targetType = texTransCallEditorBehavior.GetType();
            if (!s_processor.ContainsKey(targetType)) { throw new EditorProcessorNotFound(targetType); }

            s_processor[targetType].Process(texTransCallEditorBehavior, editorCallDomain);
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
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.TexTransUnityCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal interface ISpecialLayerDataImporter
    {
        void CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData);
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class SpecialDataOfAttribute : Attribute
    {
        public Type Type;
        public SpecialDataOfAttribute(Type type)
        {
            Type = type;
        }
    }

    internal static class SpecialLayerDataImporterUtil
    {
        static Dictionary<Type, ISpecialLayerDataImporter> s_specialLayerDataImporters;
        public static Dictionary<Type, ISpecialLayerDataImporter> SpecialLayerDataImporters
        {
            get { s_specialLayerDataImporters ??= GetAdditionalLayerInfoParsersTypes(); return s_specialLayerDataImporters; }
        }
        static Dictionary<Type, ISpecialLayerDataImporter> GetAdditionalLayerInfoParsersTypes()
        {
            var dict = new Dictionary<Type, ISpecialLayerDataImporter>();

            foreach (var dataOf in AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(I => I.GetTypes())
                 .Where(I => I.GetCustomAttribute<SpecialDataOfAttribute>() is not null))
            {
                var instants = Activator.CreateInstance(dataOf) as ISpecialLayerDataImporter;
                var attr = dataOf.GetCustomAttribute<SpecialDataOfAttribute>();
                if (dict.ContainsKey(attr.Type) is false) { dict.Add(attr.Type, instants); }
            }

            return dict;
        }
    }
}

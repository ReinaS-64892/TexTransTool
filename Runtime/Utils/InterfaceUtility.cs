#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace net.rs64.TexTransTool.Utils
{

    internal static class InterfaceUtility
    {
        /// <summary>
        /// <typeparamref name="T"/>型の変数に代入可能な、<paramref name="excludeTypes"/>に含まれない全ての具象型の既定のコンストラクターを使用して、インスタンスを作成し、列挙します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excludeTypes">生成対象から除外する型。</param>
        /// <returns></returns>
        public static IEnumerable<T> CreateConcreteAssignableTypeInstances<T>(params Type[] excludeTypes)
        {
            return GetConcreteAssignableTypes<T>().Except(excludeTypes).Select(type =>
            {
                try
                {
                    return (T)Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    Debug.Log(type.ToString());
                    throw;
                }
            });
        }
        /// <summary>
        /// <typeparamref name="T"/>型の変数に代入可能な全ての具象型の<see cref="Type"/>を列挙します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetConcreteAssignableTypes<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(T).IsAssignableFrom(type));
        }
    }
}

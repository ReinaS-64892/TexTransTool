using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace net.rs64.TexTransTool.Utils
{
    internal static class NativeArrayListView
    {
        /// <summary>
        /// コピーせずに、NativeArrayをIListとして扱うための拡張メソッドです。アイテムを削除・追加する関数は実装されていないため、
        /// NotImplementedExceptionが発生します。
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IList<T> AsList<T>(this NativeArray<T> array) where T : struct => new NativeArrayAsList<T>(array);
        
        internal class NativeArrayAsList<T> : IList<T> where T : struct
        {
            private NativeArray<T> _array;
            
            public NativeArrayAsList(NativeArray<T> array)
            {
                _array = array;
            }
            
            public IEnumerator<T> GetEnumerator()
            {
                return _array.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(T item)
            {
                throw new System.NotImplementedException();
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains(T item)
            {
                return _array.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                for (int i = 0; i < _array.Length; i++)
                {
                    array[i + arrayIndex] = _array[i];
                }
            }

            public bool Remove(T item)
            {
                throw new System.NotImplementedException();
            }

            public int Count => _array.Length;
            public bool IsReadOnly => true;
            
            public int IndexOf(T item)
            {
                return _array.Select((v, i) => (v, i))
                    .Where(pair => pair.v.Equals(item))
                    .Select(pair => pair.i)
                    .Append(-1)
                    .First();
            }

            public void Insert(int index, T item)
            {
                throw new System.NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new System.NotImplementedException();
            }

            public T this[int index]
            {
                get => _array[index];
                set => _array[index] = value;
            }
        }
    }
}
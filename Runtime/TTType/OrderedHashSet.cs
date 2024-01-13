using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    [Serializable]
    internal class OrderedHashSet<T> : IReadOnlyList<T>, IEnumerable<T>
    {
        [SerializeField] List<T> List;
        [SerializeField] HashSet<T> Hash;

        public T this[int index] => List[index];
        public int Count => List.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public bool Add(T item)
        {
            if (!Hash.Contains(item))
            {
                List.Add(item);
                Hash.Add(item);
                return true;
            }
            return false;
        }
        public int AddAndIndexOf(T item)
        {
            if (Add(item))
            {
                return List.Count - 1;
            }
            else
            {
                return IndexOf(item);
            }
        }

        public int IndexOf(T item)
        {
            return List.IndexOf(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void HashRefresh()
        {
            Hash.Clear();
            Hash.UnionWith(List);
        }



        public OrderedHashSet(IEnumerable<T> enumerate)
        {
            List = new List<T>();
            Hash = new HashSet<T>();
            List.AddRange(enumerate);
        }

        public OrderedHashSet()
        {
            List = new List<T>();
            Hash = new HashSet<T>();
        }

        public List<T> ToList(bool deepClone = false)
        {
            if (deepClone)
            {
                return new List<T>(List);
            }
            else
            {
                return List;
            }
        }
    }
}
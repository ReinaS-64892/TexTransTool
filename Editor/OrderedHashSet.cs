using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    [Serializable]
    public class OrderedHashSet<T> : IReadOnlyList<T>, IEnumerable<T>
    {
        [SerializeField] List<T> List;
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

        public void Add(T item)
        {
            var Index = IndexOf(item);
            if (Index == -1)
            {
                List.Add(item);
            }
        }
        public int AddAndIndexOf(T item)
        {
            var Index = List.IndexOf(item);
            if (Index == -1)
            {
                List.Add(item);
                return List.Count - 1;
            }
            return Index;
        }

        public int IndexOf(T item)
        {
            return List.IndexOf(item);
        }

        public void AddRange(IEnumerable<T> Items)
        {
            foreach (var item in Items)
            {
                Add(item);
            }
        }




        public OrderedHashSet(IEnumerable<T> enumreat)
        {
            List = new List<T>();
            List.AddRange(enumreat);
        }

        public OrderedHashSet()
        {
            List = new List<T>();
        }

        public List<T> ToList(bool DeepClone = false)
        {
            if (DeepClone)
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
using System.Collections.Generic;
using UnityEngine;

namespace ZooTycoon.Core
{
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        public readonly List<T> Items = new List<T>();

        public void Add(T item) { if (!Items.Contains(item)) Items.Add(item); }
        public void Remove(T item) => Items.Remove(item);
        public List<T> GetAll() => Items;
    }
}

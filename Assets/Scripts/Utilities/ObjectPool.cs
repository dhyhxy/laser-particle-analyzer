using System.Collections.Generic;

namespace LaserParticleAnalyzer.Utilities
{
    public class ObjectPool<T> where T : class, new()
    {
        private Stack<T> available;
        private HashSet<T> inUse;
        private System.Func<T> factory;
        private int capacity;

        public ObjectPool(System.Func<T> objectFactory, int poolCapacity)
        {
            factory = objectFactory;
            capacity = poolCapacity;
            available = new Stack<T>(poolCapacity);
            inUse = new HashSet<T>();

            for (int i = 0; i < poolCapacity; i++)
            {
                available.Push(factory());
            }
        }

        public T Get()
        {
            T obj;
            if (available.Count > 0)
            {
                obj = available.Pop();
            }
            else
            {
                obj = factory();
            }
            inUse.Add(obj);
            return obj;
        }

        public void Release(T obj)
        {
            if (obj != null && inUse.Remove(obj))
            {
                available.Push(obj);
            }
        }

        public void Clear()
        {
            available.Clear();
            inUse.Clear();
        }

        public int CountInUse => inUse.Count;
    }
}

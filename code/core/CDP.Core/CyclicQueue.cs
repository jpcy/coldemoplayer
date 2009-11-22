using System;
using System.Collections.Generic;
using System.Collections;

namespace CDP.Core
{
    public class CyclicQueue<T> : IEnumerable<T>, IEnumerable
    {
        private readonly int maximumSize;
        private readonly List<T> list = new List<T>();

        public int Count
        {
            get { return list.Count; }
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public CyclicQueue(int maximumSize)
        {
            this.maximumSize = maximumSize;
        }

        public void Enqueue(T item)
        {
            list.Add(item);

            if (list.Count > maximumSize)
            {
                list.RemoveAt(0);
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}

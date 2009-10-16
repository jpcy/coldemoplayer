using System;
using System.Collections.Generic;
using System.Collections;

namespace CDP.Core
{
    public class CyclicQueue<T> : IEnumerable<T>, IEnumerable
    {
        private readonly int maximumSize;
        private readonly Queue<T> queue = new Queue<T>();

        public int Count
        {
            get { return queue.Count; }
        }

        public CyclicQueue(int maximumSize)
        {
            this.maximumSize = maximumSize;
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item);

            if (queue.Count > maximumSize)
            {
                queue.Dequeue();
            }
        }

        public void Clear()
        {
            queue.Clear();
        }

        public Queue<T>.Enumerator GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }
    }
}

using System.Collections.Generic;

namespace Wave.Common
{
    public class AsyncQueue<T> where T : class
    {
        public int Count
        {
            get
            {
                lock (queueLock)
                    return queue.Count;
            }
        }

        public bool CanDequeue
        {
            get { return !isDequeueing; }
        }

        private Queue<T> queue = new Queue<T>();

        private object queueLock = new object();
        private InterlockedBool isDequeueing = false;

        public AsyncQueue()
        {
        }

        public T BeginDequeue()
        {
            if (!isDequeueing)
            {
                lock (queueLock)
                {
                    if (queue.Count > 0)
                    {
                        isDequeueing = true;

                        return queue.Peek();
                    }
                }
            }

            return null;
        }

        public void EndDequeue()
        {
            if (isDequeueing)
            {
                lock (queueLock)
                {
                    queue.Dequeue();

                    isDequeueing = false;
                }
            }
        }

        public void CancelDequeue()
        {
            isDequeueing = false;
        }

        public void Enqueue(T item)
        {
            lock (queueLock)
                queue.Enqueue(item);
        }

        public bool Contains(T item)
        {
            lock (queueLock)
                return queue.Contains(item);
        }

        public void Clear()
        {
            lock (queueLock)
                queue.Clear();
        }
    }
}

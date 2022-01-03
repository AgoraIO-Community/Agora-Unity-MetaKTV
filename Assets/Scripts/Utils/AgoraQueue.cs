using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace agora.KTV
{
    public class AgoraQueue<T>
    {
        private Queue<T> _queue;

        public AgoraQueue()
        {
            _queue = new Queue<T>();
        }

        ~AgoraQueue()
        {
            ClearQueue();
        }

        public void ClearQueue()
        {
            lock (_queue)
            {
                _queue.Clear();
            }
        }

        public void EnQueue(T info)
        {
            lock (_queue)
            {
                if (_queue.Count >= 250)
                {
                    _queue.Dequeue();
                }

                _queue.Enqueue(info);
            }
        }

        public T DeQueue()
        {
            T info = default(T);
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    info = _queue.Dequeue();
                }

                return info;
            }
        }
    }
}
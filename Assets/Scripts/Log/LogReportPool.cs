using System.Collections.Generic;

namespace LogModule
{
    public static class LogReportPool
    {
        public static T Acquire<T>() where T : class, new()
             => LogReportPool<T>.Acquire();

        public static void Release<T>(T item) where T : class, new()
        => LogReportPool<T>.Release(item);

    }

    public static class LogReportPool<T> where T : class, new()
    {

        private static readonly Queue<T> cache = new();

        public static T Acquire()
        {
            if (cache.Count > 0)
            {
                return cache.Dequeue();
            }

            return new T();
        }

        public static void Release(T item)
        {
            cache.Enqueue(item);
        }
    }
}

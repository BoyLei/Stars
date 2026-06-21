using System.Collections.Generic;

namespace Yoka.UnityString.Core
{
    public static class StringPool
    {
        private static Dictionary<int, Queue<UString>> map = new Dictionary<int, Queue<UString>>();

        public static void InitPools()
        {
            for (int i = 0; i < StrDefine.InitCacheNum; i++)
            {
                PreAlloc(i, StrDefine.InitSingleCacheNum);
            }
        }

        static void PreAlloc(int size, int count)
        {
            if (size <= 0)
            {
                return;
            }

            Queue<UString> queue = null;

            if (map.TryGetValue(size, out queue))
            {
                for (int i = queue.Count; i < count; i++)
                {
                    queue.Enqueue(new UString(StrDefine.NEW_ALLOC_CHAR, size));
                }
            }
            else
            {
                queue = new Queue<UString>();
                map[size] = queue;

                for (int i = 0; i < count; i++)
                {
                    queue.Enqueue(new UString(StrDefine.NEW_ALLOC_CHAR, size));
                }
            }
        }

        static public int Count(int size)
        {
            if (size == 0)
            {
                return -1;
            }
            if (map.TryGetValue(size, out var queue))
            {
                return queue.Count;
            }
            return -1;
        }

        static public UString Get(int size)
        {
            if (size == 0)
            {
                return null;
            }
            Queue<UString> queue = null;

            if (map.TryGetValue(size, out queue))
            {
                if (queue.Count > 0)
                {
                    return queue.Dequeue();
                }
            }
            else
            {
                queue = new Queue<UString>();
                map[size] = queue;
            }
            return new UString((char)0xCC, size);
        }

        static public void Release(UString str)
        {
            if (string.IsNullOrEmpty(str.Value))
            {
                return;
            }

            int size = str.Value.Length;

            if (size > 0)
            {
                Queue<UString> queue = null;

                if (!map.TryGetValue(str.Length, out queue))
                {
                    queue = new Queue<UString>();
                    map[size] = queue;
                }

                queue.Enqueue(str);
            }
        }
    }
}

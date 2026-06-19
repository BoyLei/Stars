using System.Collections.Generic;

namespace CollectionUtils
{
    public class DictionaryUtils
    {
        /// <summary>
        /// 将 一个 listItem 添加到 一个  Dictionary<T1, List<T2>> 的 list 数据里面
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="listItem"></param>
        public static void AddListItem<T1, T2>(Dictionary<T1, List<T2>> dictionary, T1 key, T2 listItem)
        {
            if (!dictionary.TryGetValue(key, out List<T2> arr))
            {
                arr = new List<T2>();
                dictionary.Add(key, arr);
            }
            ArrayUtils.Add<T2>(arr, listItem);
        }
    }

    public class ArrayUtils
    {
        public static void Add<T>(List<T> list, T v)
        {
            if (!list.Contains(v))
            {
                list.Add(v);
            }
        }
    }
}


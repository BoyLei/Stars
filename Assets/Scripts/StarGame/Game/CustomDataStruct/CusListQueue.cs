using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.CustomDataStruct
{
    /// <summary>
    /// 2023/3/6
    /// 使用 list 封装了 一部分queue 接口的 数据结构,用来实现 能够 操作 Queue 中间数据的结构
    /// note:
    ///     使用 list, 所以 队列的增加 和删除 涉及数组所有数据的 拷贝问题,效率不太高, 看情况使用。
    ///     后面有空 看看 要不要用其它的 数据结构 优化.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CusListQueue<T> : List<T>
    {
        public void Enqueue(T item)
        {
            this.Add(item);
        }
        public T Dequeue()
        {
            T result = default(T);
            if (this.Count > 0)
            {
                result = this[0];
                RemoveAt(0);
            }
            return result;
        }

        public T Peek()
        {
            T result = default(T);
            if (this.Count > 0)
            {
                result = this[0];
            }
            return result;
        }
    }
}

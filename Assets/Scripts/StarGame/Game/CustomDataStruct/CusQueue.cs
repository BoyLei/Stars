using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.CustomDataStruct
{
    /// <summary>
    /// 封装 的 Queue 的拓展 数据结构, 用来 实现 一些方便 的操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueExtends<T> : Queue<T>
    {

        /// <summary>
        /// 从 queue 的头部插入 items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="clearInputQueue"></param>
        public void HeadEnqueue(IEnumerable<T> items)
        {
            var lists = this.KToList();

            List<T> itemLists = items.KToList();

            lists.ForEach((v) =>
            {
                itemLists.Add(v);
            });

            this.Clear();
            lists.Clear();

            itemLists.ForEach((v) =>
            {
                this.Enqueue(v);
            });

            itemLists.Clear();
        }

        /// <summary>
        /// 从 queue中 删除 一组 元素
        /// </summary>
        /// <param name="items"></param>
        public void Remove(List<T> items)
        {
            var lists = this.KToList();
            items.ForEach((item) =>
            {
                lists.Remove(item);
            });
            this.Clear();
            lists.ForEach((item) =>
            {
                this.Enqueue(item);
            });
        }
    }

}

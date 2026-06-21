// -----------------------------------------------------------------------
// This file is part of framework.
// 
// (c) rick <ninglei@dobest.com>
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Yoka.Galaxy.Framework
{
    /// <summary>
    /// 所有需要监听的接口都需要从这个类派生， 不然下面的泛型不好写
    /// </summary>
    public interface IAspect
    {

    }

    public interface IAspectContainer
    {
        void Recieve(IModule module);

        void AssginPriority(bool undefinedToTail, List<Type> inputPriorityList);
    }

    /// <summary>
    /// 行为切片
    /// </summary>
    public class AspectContainer<I> : IAspectContainer where I: IAspect
    {
        List<I> _accepted = new List<I>();
        List<I> _modified = null;
        List<Type> _typePriorityList = null;
        Dictionary<Type, int> _typeToPriorityDict = null;
        bool _undefinedToTail = false;

        /// <summary>
        /// 监听接口Resolve
        /// </summary>
        /// <param name="module"></param>
        public void Recieve(IModule module)
        {
            if (module is I)
            {
                //防止Fetch后迭代器工作中的修改，在这里备份出来，下次Fetch再更新
                if(_modified==null)
                {
                    _modified = new List<I>(_accepted);
                }
                _modified.Add((I)module);
            }
        }

        /// <summary>
        /// 赋排序列表
        /// </summary>
        /// <param name="inputPriorityList">传入排序类型优先级表</param>
        public void AssginPriority(bool undefinedToTail, List<Type> inputPriorityList)
        {
            _undefinedToTail = undefinedToTail;
            _typePriorityList = inputPriorityList;
            _typeToPriorityDict = new Dictionary<Type, int>();
            for (int i = 0; i < inputPriorityList.Count; i++)
            {
                Type type = inputPriorityList[i];
                _typeToPriorityDict[type] = i;
            }
            //这里等于置脏标记，触发下次Fetch时的排序
            if (_modified == null)
            {
                _modified = new List<I>(_accepted);
            }
        }

        /// <summary>
        /// 获取监听的列表
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <returns></returns>
        public List<I> Fetch()
        {
            if(_modified!=null)
            {
                _accepted = ConvertModifiedToAccepted(_modified);
                _modified = null;
            }
            return _accepted;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="modifiedList"></param>
        /// <returns></returns>
        private List<I> ConvertModifiedToAccepted(List<I> modifiedList)
        {
            if(_typePriorityList!=null && _typeToPriorityDict!=null)
            {
                SortList(modifiedList);
            }
            return modifiedList;
        }

        /// <summary>
        /// 排序函数
        /// </summary>
        /// <param name="inputList">传入模块列表</param>
        private void SortList(List<I> inputList)
        {
            inputList.Sort((left, right) =>
            {
                return GetPriority(left) - GetPriority(right);
            });
        }

        /// <summary>
        /// 获取每个类型的优先级
        /// </summary>
        /// <param name="aspect">切片</param>
        /// <returns>优先级</returns>
        private int GetPriority(I aspect)
        {
            var t = aspect.GetType();
            foreach(var kv in _typeToPriorityDict)
            {
                var type = kv.Key;
                var intfc = t.GetInterface(type.Name);
                if(intfc!=null)
                {
                    Debug.LogFormat("GetPriority found, t: {0}, type: {1}, intfc: {2}", t, type, intfc);
                    return kv.Value;
                }
            }
            if(_undefinedToTail)
            {
                return _typePriorityList.Count;
            }
            else
            {
                return -1;
            }
        }
    }

}
#endif
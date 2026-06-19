using System;
using System.Collections.Generic;

//你这里这么规划，以后就3大类 循环器

//现在所有显示类型View：恰好都是unity，实体View

//所有实体生命周期：实体logic

//实体按照什么方式生活【服务器给了公式化运行的结果】：实体DataQueue公式化演绎实体的生命周期

namespace StarProject.Game.Entity.Factory
{
    public interface IRecyclableObject
    {
        /// <summary>
        /// 池化字典_分组类型
        /// </summary>
        /// <returns></returns>
        string GetRecycleType();
        /// <summary>
        /// 释放
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// 回收期管理是一个抽象概念,是对回收器规范(接口),的抽象管理
    /// 1,Gob的复用--位置和现/隐，放在非渲染空间
    /// 2,【资源组】的复用加载，归纳现隐-GPU，归纳GC-内存,数据创建计算-CPU
    /// 3,对象的复用,逻辑节省,性能节省
    /// -------------------------------------
    /// 依赖大量反复必然出现的实体：延迟释放也具备叠加式内存峰值：需依赖设计+ 控制上限
    /// ------------------------------------
    /// 1,基于规则逻辑相似
    /// 2,减少I/O
    /// 3,减少内存分配
    /// 4,数量的控制
    /// 5,频率和规则和缓存：
    /// 6,时效性，替换性|进入：频率--缓存：上限，复用--销毁：GC+VFrame|如基于实体本身为重的原则则不设置替换上限
    /// </summary>
    /// ps：非单例原因：考虑可以被复用：【因为洗衣机概念设定，是随着工厂而new出来的，多少工厂多少套管理模式】
    /// 回收循环：Unuse = Release + Hide + CanUseTag | 可重新初始化 + 全属性重生
    public class Recycler
    {
        /// <summary>
        /// 被工厂回收的空闲对象列表
        /// 类型，栈池接口
        /// </summary>
        private static DictionaryEx<string, Stack<IRecyclableObject>> m_poolIdleObject;
        /// <summary>
        /// 构建
        /// </summary>
        public Recycler()
        {
            m_poolIdleObject = new DictionaryEx<string, Stack<IRecyclableObject>>();
        }

        /// <summary>
        /// 倒闭
        /// GameOver，Edit，Application.Exit
        /// Excel逐Cel.Release()
        /// View:Destroy-显示层次，unity+Draw渲染层
        /// Module层次：则GC；尽量不人为干预unityGC
        /// </summary>
        public void Release()
        {
            foreach (var pair in m_poolIdleObject)
            {
                foreach (var obj in pair.Value)
                {
                    obj.Dispose();
                }
                pair.Value.Clear();
            }

        }
        /// <summary>
        /// 洗衣机，回收洗涤的，也是暂时缓存待利用的
        /// 1，来源定为不用
        /// （2，初始化），策略层交给工厂来指导，保证逻辑内循环可复用
        /// </summary>
        /// <param name="obj"></param>
        public void Push(IRecyclableObject obj)
        {
            string type = obj.GetRecycleType();
            Stack<IRecyclableObject> stackIdleObject = m_poolIdleObject[type];
            if (stackIdleObject == null)
            {
                stackIdleObject = new Stack<IRecyclableObject>();
                m_poolIdleObject.Add(type, stackIdleObject);
            }
            stackIdleObject.Push(obj);
        }

        /// <summary>
        ///  洗衣机，拿出使用的，也是暂时缓存待利用的
        /// 1，来源定为不用
        /// （2，初始化），策略层交给工厂来指导，保证逻辑内循环可复用
        /// 3，这里初始化不给值得原因是，1设计不同有些为空*有些跟二级工厂要*有些自己new让上层自己处理 ，2方式不同都在外层实体具体拓展如何处理数据，3作用不同如果类可以new而mono需要加载和Addcomp
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IRecyclableObject Pop(string type)
        {
            Stack<IRecyclableObject> stackIdleObject = m_poolIdleObject[type];
            if (stackIdleObject != null && stackIdleObject.Count > 0)
            {
                return stackIdleObject.Pop();
            }
            return null;
        }

        public int GetCount(string type)
        {
            int count = 0;

            Stack<IRecyclableObject> stackIdleObject = m_poolIdleObject[type];
            if (stackIdleObject != null)
            {
                count = stackIdleObject.Count;
            }

            return count;
        }
       
    }
}

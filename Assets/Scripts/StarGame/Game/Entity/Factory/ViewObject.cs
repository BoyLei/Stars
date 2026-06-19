using System;
using UnityEngine;

//项目,游戏,实体,工厂  管理
namespace StarProject.Game.Entity.Factory
{
    /// <summary>
    /// 显示层通常基于
    /// 1,unity----挂点设计----所以具备Mono驱动力, IRecyclableObject
    /// </summary>
    public abstract class ViewObject : MonoBehaviour, IRecyclableObject
    {
        //类名为,子类集合
        //但是同类型,会因为设计意义不同导致名称不同
        //例如:士兵,加强士兵,变身士兵;敌方士兵|右方|被我控制的士兵----------------------------------------------------------------------
        private string m_recycleType;


        //工厂创建,和子类具体实现----------------------------------------------------------------------
        /// <summary>
        /// Creates the in factory.
        /// 在被工厂控制时候,
        /// 具备特定名称在池化时的key标记
        /// [工厂招聘方式]
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="recycleType">有些View的类型是资源名，有些是类名.</param>
        internal void CreateInFactory(EntityObject entity, string recycleType)
        {
            //显示,enity,对于工厂招聘,首先做的就是自己记录自己的类型
            m_recycleType = recycleType;

            Create(entity);
        }

        internal void CreateInFactory(EntityObject entity, string recycleType, string modelPath)
        {
            //显示,enity,对于工厂招聘,首先做的就是自己记录自己的类型
            m_recycleType = recycleType;

            Create(entity, modelPath);
        }

        /// <summary>
        /// 所有在unityView层的子类,需具备的创建细节
        /// [子公司附加招聘方式]
        /// </summary>
        /// <param name="entity"></param>
        protected abstract void Create(EntityObject entity);

        /// <summary>
        /// 根据 modelPath 模型路径 创建一个 模型实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modelPath"></param>
        protected abstract void Create(EntityObject entity, string modelPath);




        //工厂移除,和子类移除----------------------------------------------------------------------
        //[工厂解雇方式]
        internal void ReleaseInFactory()
        {
            Release();
        }
        //[子公司附加解雇方式]:通常是左值置空等待GC
        protected abstract void Release();


        //----------------------------------------------------------------------
        //类型获取 和 池化删除(池化控制的删除)意味着游戏结束,工厂倒闭时显示物品如何处理
        //回收:显示隐藏,属性:   倒闭:移除
        //---------------------------------------------------------------------

        public string GetRecycleType()
        {
            return m_recycleType;
        }
        /// <summary>
        /// [社会移除掉这个实体-_-!!!]
        /// </summary>
        public void Dispose()
        {
            try
            {
                //Gob挂点,的Mono驱动对象
                GameObject.Destroy(this.gameObject);
            }
            catch (Exception e)
            {

            }
        }
    }

}
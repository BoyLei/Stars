/// <summary>
/// 简单的 数据.
/// </summary>
namespace StarProject.Game.Entity.Factory
{
    /// <summary>
    /// 一般 需要频繁 实例化的 数据类型, 都可以继承这个 类. 比如 技能 skillInfo这些 需要频繁创建和销毁的数据
    /// 此处 独立出 DynamicDataObject 的原因 是 DynamicDataObject 存在了 额外的一些 类似于 M_EntityID 这种无效的 字段.
    /// </summary>
    public abstract class SimpleDataObject : IRecyclableObject
    {
        //释放标记----------------------------------------------------------------------
        //虽然服务器会标脏数据，他有指数级通知不同玩家，我们虽然没有
        //但是依然具有，多人 * 属性 * 时间队列
        //【毫无用处】，因为会立刻释放，【不会对其进行检测是否可以pop】
        private bool m_isReleased = false;
        public bool IsReleased { get { return m_isReleased; } }

        /// <summary>
        /// [工厂录用]
        /// </summary>
        internal void InstanceInFactory()
        {
            m_isReleased = false;//beingUseing
        }


        protected abstract void Release();


        //不是帧尾释放，是直接释放
        //通过具体工厂释放：引用----------------------------------------------------------------------
        /// <summary>
        /// [工厂解雇]
        /// </summary>
        internal void ReleaseInFactory()
        {
            //初始化 = 运行用  ===转换为===> 准备释放标记 + 处理子类Release
            if (!m_isReleased)
            {
                Release();
                //这里的释放就是标记，和子类的释放，重新变为内存模板占位符
                m_isReleased = true;
            }
        }



        //innerface----------------------------------------------------------------------

        public string GetRecycleType()
        {
            return this.GetType().FullName;
        }
        /// <summary>
        /// [社会移除掉这个Data-_-!!!]
        /// </summary>
        public void Dispose()
        {
            //由系统的GC机制来处理
            //Do nothing!
        }
        //----------------------------------------------------------------------
    }
}
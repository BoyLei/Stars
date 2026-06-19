/// <summary>
/// 数据基类
/// 这里一定是动态数据
/// 静态：本地Json，通过关卡加载，拆表切换数据
/// 动态：数据虽不强制依赖时间，但循环实体本质就是驱动动态数据
/// </summary>
namespace StarProject.Game.Entity.Factory
{   /// <summary>
    //TODO：        三个子类型：伤害演绎，buff效果，属性底层
    //TODO：那怎么管理一组黑板数据呢
    //TODO：这里应该是一个单元，是所有单元的子集
    public abstract class DynamicDataObject : IRecyclableObject
    {
        public ulong M_EntityID;

        //由于网络拆包，是否合并完成，遇到End标签就是true
        public bool IsFullMessage = false;


        //public ulong RunTimeId;
        ///// <summary>
        ///// 属性没有时间
        ///// </summary>
        //public double Time;
        /// <summary>
        /// 1,属性同步直接获取最后是true
        /// 2，动画，伤害，都是False
        /// </summary>
        /// <returns></returns>
        public virtual bool IsGetLastData()
        {
            return true;
        }
        //释放标记----------------------------------------------------------------------
        //虽然服务器会标脏数据，他有指数级通知不同玩家，我们虽然没有
        //但是依然具有，多人 * 属性 * 时间队列
        //【毫无用处】，因为会立刻释放，【不会对其进行检测是否可以pop】
        private bool m_isReleased = false;
        public bool IsReleased { get { return m_isReleased; } }

        /// <summary>
        /// [工厂录用]
        /// </summary>
        internal void InstanceInFactory(ulong entityId)
        {
            m_isReleased = false;//beingUseing
            Create(entityId);
        }


        #region 快照组数据
        /// <summary>
        /// [工厂录用]:快照一组数据不需要具体的nttid
        /// </summary>
        internal void InstanceInFactory()
        {
            m_isReleased = false;//beingUseing
            Create();
        }
        protected abstract void Create();
        #endregion

        /// <summary>
        /// 所有在unityView层的子类,需具备的创建细节
        /// [子公司附加招聘方式]
        /// </summary>
        /// <param name="entity"></param>
        protected abstract void Create(ulong entity);


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
        ~DynamicDataObject()
        {

        }

        /// <summary>
        /// 无定规则的抽象者
        /// 如有共有逻辑则修改为虚函数
        /// [解雇附加条件]
        /// </summary>
        protected virtual void Release()
        {

            IsFullMessage = false;
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
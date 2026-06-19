using System;
using StarProjectDef;



namespace StarProject.Service.SystemOpen
{
    /// <summary>
    /// 系统开放自定义条件检查的 基类, 各个系统 可以 自己定义 自己需要的 检查条件.
    /// 
    /// 添加步骤 如下:
    ///     1.继承 SystemCustomCondition, 重写 CheckCondion 条件检查接口. 
    ///     2.在 类的 构造函数里面 注册 自己需要的事件监听, 监听到 事件后, MarkDirty() 触发 对应的 条件检查
    ///     3.在  SystemOpenManagerCusConditions.cs 的 InitCustomConditions 中, 
    ///       通过 CreateCustomCondition<T>() 创建 自定义类型的实例.
    ///     
    ///     Over!!!
    /// </summary>
    [XLua.Hotfix]
    public abstract class SystemCustomCondition
    {
        /// <summary>
        /// 系统 开放的类型, 需要子类 重写
        /// </summary>
        public abstract SystemOpenType systemOpenType { get; }

        /// <summary>
        ///  触发系统 
        /// </summary>
        public Action<SystemOpenType> ActionOnTriggerCheckSystemCusCondition;

        public SystemCustomCondition()
        {
            // 声明一个默认的 无参的构造函数
        }
        /// <summary>
        /// 定义 一个 抽象的 检查接口
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckCondion();

        /// <summary>
        /// 标脏接口, 当 系统类型 收到自己监听 事件的时候, 触发一次标脏检查
        /// </summary>
        public void MarkDirty()
        {
            if (SGF.Unity.DelayInvoker.ContainInvoke(this))
            {
                return;
            }
            SGF.Unity.DelayInvoker.DelayInvoke(0, DirtyRefresh, null);
        }

        /// <summary>
        /// 触发一次 刷新逻辑
        /// </summary>
        /// <param name="args"></param>
        private void DirtyRefresh(object[] args)
        {
            ActionOnTriggerCheckSystemCusCondition?.Invoke(systemOpenType);
        }
    }
}


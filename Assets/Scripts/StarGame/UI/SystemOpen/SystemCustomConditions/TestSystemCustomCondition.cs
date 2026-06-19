
using StarProject.Game;
using StarProjectDef;

namespace StarProject.Service.SystemOpen
{


    /// <summary>
    /// 添加步骤 如下:
    ///     1.继承 SystemCustomCondition, 重写 CheckCondion 条件检查接口. 
    ///     2.在 类的 构造函数里面 注册 自己需要的事件监听, 监听到 事件后, MarkDirty() 触发 对应的 条件检查
    ///     3.在  SystemOpenManagerCusConditions.cs 的 InitCustomConditions 中, 
    ///       通过 CreateCustomCondition<T>() 创建 自定义类型的实例.
    ///     
    ///     Over!!!
    /// </summary>
    [XLua.Hotfix]
    public class TestSystemCustomCondition : SystemCustomCondition
    {

        public override SystemOpenType systemOpenType => SystemOpenType.None;

        /// <summary>
        /// 在 构造函数里面, 可以声明一些 自己需要监听的 事件类型, 依此来触发自定义类型的条件检查
        /// </summary>
        public TestSystemCustomCondition()
        {
            // 目前 演示的 是 监听 玩家 等级变化的时候, 刷新 系统开放
            GlobalEvent.OnPlayerDataChange.AddListener(OnPlayerDataChange);
        }

        public override bool CheckCondion()
        {
            /// 测试 演示的 是 >10级的时候, 系统 自定义条件满足, 开放系统
            /// 其它的 类型按 各自自己需要的 条件判断
            {
                int curRoleLevel = 0;
                if (GameManager.Instance.M_MainPlayerCtrlBase != null)
                {
                    curRoleLevel = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);
                }

                if (curRoleLevel > 10)
                {
                    return true;
                }

                return false;
            }

        }

        private void OnPlayerDataChange(string key, object value)
        {
            switch (key)
            {
                case AOIAttrDefine.PlayerLevel:
                    {
                        MarkDirty();
                    }
                    break;
                default: break;
            }
        }
    }
}

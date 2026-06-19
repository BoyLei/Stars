using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using SGF.Time;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProjectDef;
using System;
/// <summary>
/// 技能控制器，用来管理 “技能位” 技能的状态。 如LOL中 QWER 四个技能状态。
/// 
/// 技能的设计分为3大块。
/// 1.技能摇杆模块，摇杆的UI刷新、cd时间等，都是依赖于技能控制器中的数据。
/// 2.技能运行时模块。当使用技能后，根据服务器的技能协议返回，创建的技能运行时。
///     技能运行时由VitalSignsEntityBase的EnterFrame每帧驱动，到了配置的帧事件时，创建对应的特效、动画显示。
///     技能运行时达到最大时长时，自行销毁。
///     技能运行时由服务器下发控制销毁时，立即销毁此运行时，同事销毁所有动画、特效（一般动画技能被打断等会有这个）。
///     技能运行时同时可能有多个，但同一个技能同时只能有一个，因为一个人物同时只能有一个动作，只有主动技能有动作（技能运行时，代表的是技能）
/// 3.状态机模块。
///     技能能否释放，主要依赖状态机来判断。
///     比如 ： EZ 的R ，释放技能有个读条，此时应该进入了读条 状态，屏蔽 QWER，但召唤师技能可以使用；
///             金身，进入金身状态，屏蔽所有技能。
///             以上几种状态都是到了时间之后，状态自动迁移。
///     所以状态机的状态迁移，具有自主性。它能够记录进入状态B之前的LastState A，当状态B结束后，
///             恢复状态A。或者由于某个条件condition，进入State C。
///     目前状态机这块有点疑惑，比如技能的释放会有多个阶段，那在技能释放的不同阶段去 switch State？ 分散的写法太难受了
///     然后，状态机的控制，一部分由客户端维护，一部分根据服务器返回的协议来切换
/// 
/// 
/// </summary>
using System.Collections.Generic;
using UnityEngine;

using StarProject.Game.SnapShot;

namespace StarProject.Game.Skill
{
    [XLua.LuaCallCSharp]
    public class SkillDispatcher
    {
        private string TagFlag => $"[{EntityId}] [SkillDispatcher]";

        public ulong EntityId => playerData == null ? 0 : playerData.M_EntityID;
        /// <summary>
        /// 创建技能时，玩家的数据层
        /// </summary>
        private VitalSignData playerData;
        private Transform parent;

        private SkillController skillController;

        public SkillController SkillController
        {
            get { return skillController; }
        }

        private SkillUnitController skillUnitController;

        public SkillUnitController SkillUnitController
        {
            get { return skillUnitController; }
        }

        /// <summary>
        /// 技能控制器的创建，需要做以下几件事：
        /// 1.玩家技能位的创建，每个技能位，可以填充多个技能。
        /// 2.初始化技能运行时Dictionary，虽然服务器目前认为同时只能有一个运行时，我想通过Dic来存储，防止以后有变化
        /// 3.根据技能位，去创建技能指示器（需要判断是否是主角）。
        /// 4.根据playerData身上挂在的数据，判断哪些技能有CD之类，防止断线重连登录，技能冷却重置的问题。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="container"></param>
        public void Create(VitalSignData data, Transform container)
        {
            playerData = data;
            parent = container;

            skillController = new SkillController();
            skillController.Create(this, data, container);

            skillUnitController = new SkillUnitController();
            skillUnitController.Create(this, data, container);
        }


        internal void EnterFrame()
        {
            skillController.EnterFrame();
            skillUnitController.EnterFrame();
        }

        /// <summary>
        /// 技能控制器reset的接口，并不会清理action
        /// note：
        ///     只清理当前运行的技能,但 不会清理action 和 对应的 控制器
        /// </summary>
        public void Reset()
        {
            skillController.Reset();

            skillUnitController.Reset();
        }

        /// <summary>
        /// SkillDispatcher销毁的时候,才需要调用Release
        /// note:
        ///     如果只需要恢复SkillDispatcher原始状态,同时,不清理注册的Action,
        ///     调用 Reset(); 接口
        /// </summary>
        public void Release()
        {
            Reset();

            skillController.Release();

            skillController = null;

            skillUnitController.Release();

            skillUnitController = null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using ProtoMsg;
using SGF.Time;
using SGF.Unity;
using StarProject.Service.Battle;

namespace StarProject.Game.Skill
{
    // 技能控制器  关于 技能的部分
    public partial class SkillController
    {
        /// <summary>
        /// 2023/3/24
        ///     新增 的 所有 技能实体list , 对于 技能控制器来说, 对技能实体 执行行为 频率最高的 是 enterFrame 中的遍历.
        ///     如果 按 曾经 的 dictionary 的方式 遍历, 由于 enterFrame 途中可能出现 技能实体的 增加/销毁,  所以需要每次
        ///     将 dictionary 转变为 tempList 的方式 处理.
        ///     同时, skillEntities 的 实体数量 并不是很多， 所以 直接查找某个技能实体用 find 也没有太多的性能消耗 .
        /// </summary>
        /// <typeparam name="SkillEntity"></typeparam>
        /// <returns></returns>
        private List<SkillEntity> skillEntities = new List<SkillEntity>(8);


        /// <summary>
        /// 活跃技能
        /// </summary>
        private SkillEntity activeSkillEntity;

        public SkillEntity ActiveSkillEntity
        {
            get { return activeSkillEntity; }
        }

        public bool IsSkillActive => activeSkillEntity != null;

        /// <summary>
        /// 当前 服务器同步的 技能实体
        /// </summary>
        private SkillEntity curServerSkillEntity;

        /// <summary>
        /// 当技能槽使用技能时，执行的Action，此时参数为技能位
        /// </summary>
        public Action<SkillContainer> ActionOnSkillUnitUseSkill;

        /// <summary>
        /// 技能开始的时候，通知外面
        /// </summary>
        public StartSkillStage ActionOnStartSkillStage;

        /// <summary>
        /// 技能结束的时候，通知外面
        /// </summary>
        public EndSkillStage ActionOnEndSkillStage;

        /// <summary>
        /// 蓄力开始的时候，通知外面的 回调
        /// </summary>
        public EnergyStart ActionOnEnergyStart;

        /// <summary>
        ///  当蓄力层数发生改变时，通知外面(skillDispatcher)
        ///  <energyedCount, curStageRunningTime>
        /// </summary>
        public Action<int, double> ActionOnEnergyCountChange;

        /// <summary>
        /// 蓄力结束的时候，通知外面的 回调
        /// </summary>
        public EnergyEnd ActionOnEnergyEnd;

        /// <summary>
        /// 技能黑板数据同步的action
        /// 之前好像有需求需要从技能黑板数据中同步到人物身上
        /// 目前好像不需要了
        /// TODO: dl
        ///     看看后面需不需要干掉
        /// </summary>
        public Action<string, object> ActionOnBlackBord;


        /// <summary>
        /// 技能播放Animation的 action
        /// </summary>
        public Action<E_ULayerSubState> ActionOnSkillAnimation;

        public bool IsPrePlayClientSkill => BattleManager.Instance.IsPrePlayClientSkill;

        private List<SkillEntity> tempLists = new List<SkillEntity>(8);

        private int tempCount = 0;
        private SkillEntity tempSkillEntity = null;

        private void UpdateSkillEntities()
        {
            try
            {
                tempCount = skillEntities.Count;
                int count = skillEntities.Count;
                for (int i = 0; i < skillEntities.Count; i++)
                {
                    /// 2023/4/3
                    /// 如果 当前 实体的 长度 已经大于了 缓存 记录的 实体的数量,
                    /// 说明 在 enterFrame 过程中, 实体 数量增加了.
                    /// 目前 在 执行 客户端 用户预输入的时候, 客户端会提前预先 一个技能,
                    /// 此时, 会 增加 一个实体
                    if (tempCount < skillEntities.Count)
                    {
                        tempCount = skillEntities.Count;
                        //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory]  enterFrame 中增加了 skillEntity");
                        // Debug.Break();
                    }
                    else if (tempCount > skillEntities.Count)
                    {
                        /// 如果 当前的数量 小于 缓存的 数量, 说明 技能实体被 删除了
                        /// 此时 其实需要 区分 删的 是 哪一个, 如果删的是之前的 , 那就需要 执行 i--
                        /// 如果 删的 是  后面的, 那啥都不用改
#if (UNITY_EDITOR && BATTLE_DEBUG)
                        LogUtils.LogError(LogUtils.LogEnum.Skill,
                            $"{TagFlag} [memory]  enterFrame 中删除了 skillEntity, 但是不知道当前是哪个");
#endif
                        SGF.Debuger.LogWarning("技能阶段 enterFrame 过程中删除了 技能实体, 这种情况应该不存在的");
#if UNITY_EDITOR
                        UnityEngine.Debug.Break();
#endif
                    }

                    tempSkillEntity = skillEntities[i];
                    if (tempSkillEntity != null && tempSkillEntity.IsClientRunning)
                    {
                        tempSkillEntity.EnterFrame();
                    }

                    tempSkillEntity = null;
                }
            }
            catch (System.Exception e)
            {
#if (UNITY_EDITOR && BATTLE_DEBUG)
                LogUtils.LogError(LogUtils.LogEnum.Skill,
                    $"{TagFlag} [memory]  enterFrame 中删除了 skillEntity: {e.Message} ");
#endif

#if UNITY_EDITOR
                //UnityEngine.Debug.Break();
#endif
            }
        }

        private void ReleaseReadyRemoveSkillEntities()
        {
            tempLists.Clear();

            // 遍历所有的 技能实体，找出 需要 释放的 实体
            skillEntities.ForEach((SkillEntity skillEntity) =>
            {
                if (skillEntity.ReadyRelease)
                {
                    tempLists.Add(skillEntity);
                }
            });

            // 按倒序 的方式, 从 skillEntities 中移除 技能实体
            for (int i = tempLists.Count - 1; i >= 0; i--)
            {
                SkillEntity skillEntity = tempLists[i];
                ReleaseSkillEntity(skillEntity);
            }

            tempLists.Clear();
        }

        private void ReleaseSkillEntity(SkillEntity skillEntity)
        {
            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [memory]  [release] skillEntity_[{skillEntity.RuntimeID}_{skillEntity.skillId}__{skillEntity.GetHashCode()}], isRunning: {skillEntity.IsClientRunning}, readyRelease: {skillEntity.ReadyRelease}");

            skillEntities.Remove(skillEntity);
            EntityFactory.ReleaseEntity(skillEntity);
            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [memory]  [release] 剩余: {skillEntities.Count} ");
        }

        private void ResetSkillEntities()
        {
            activeSkillEntity = null;

            curServerSkillEntity = null;

            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [memory]  reset start============= ");

            skillEntities.ForEach((SkillEntity skillEntity) =>
            {
                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [memory]  [reset] skillEntity_[{skillEntity.RuntimeID}_{skillEntity.skillId}__{skillEntity.GetHashCode()}], isRunning: {skillEntity.IsClientRunning}, readyRelease: {skillEntity.ReadyRelease}");

                BreakSkillEntity(skillEntity, E_ClientSkillEndType.Reset);
                EntityFactory.ReleaseEntity(skillEntity);
            });

            skillEntities.Clear();
            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [memory]  reset end============= ");
        }

        private SkillEntity CreateSkillEntity(SkillInfo skillInfo, ulong runtimeID)
        {
            SkillEntity skillEntity = EntityFactory.InstanceEntity<SkillEntity>();
            skillEntity.Create(skillInfo, this, runtimeID);
            RegisterSkillEntityAction(skillEntity);

            return skillEntity;
        }

        private void RegisterSkillEntityAction(SkillEntity skillEntity)
        {
            skillEntity.ActionOnEnergyStart = OnSkillEnergyStartCall;
            skillEntity.ActionOnEnergyCountChange = OnEnergyCountChange;
            skillEntity.ActionOnEnergyFull = OnSkillEnergyFullCall;
            skillEntity.ActionOnEnergyEnd = OnSkillEnergyEndCall;

            skillEntity.ActionOnBlackBord = OnActionOnBlackBord;

            skillEntity.ActionOnSyncSkillUse = OnActionUseSkill;
            skillEntity.ActionOnServerCreateStage = OnActionServerCreateStage;
            skillEntity.ActionOnSyncPlayerViewRota = OnActionSyncViewRota;

            #region 新的技能逻辑

            skillEntity.FuncOnSkillTrySetActiveSkill = TrySetActiveSkill;
            skillEntity.ActionOnSkillExit = OnActionSkillExit;
            skillEntity.ActionOnRefreshStates = OnActionRefreshStates;
            skillEntity.ActionOnPlayAnim = OnActionPlayAnim;
            skillEntity.ActionOnPlayFx = OnActionPlayFx;
            skillEntity.ActionOnPlayAudio = OnActionPlayAudio; //目前假设音频一个事件对应多个音频的制作方式
            skillEntity.ActionOnStopAudio = OnActionStopAudio; //目前假设音频一个事件对应多个音频的制作方式

            skillEntity.FuncOnTryPlayClientEffect = TryPlayClientEffect;
            skillEntity.FuncOnTryPlayServerEffect = OnFuncTryPlayServerEffect;
            skillEntity.FuncOnTryStopEffect = TryStopEffect;


            skillEntity.ActionOnStopFx = OnActionStopFx;

            skillEntity.ActionOnPlayCamera = OnActionPlayCamera;
            skillEntity.ActionOnPlayCameraShake = OnActionPlayCameraShake;
            // skillEntity.ActionTriggerExecuteCacheUserInput = TriggerExecuteCacheUserInput;

            #endregion
        }


        /// <summary>
        /// 检查 skillUseRet 是否时客户端主动使用的技能,
        /// 主动使用的技能,在本地都有自己的 客户端技能运行时
        /// </summary>
        /// <param name="skillUseRet"></param>
        /// <returns></returns>
        public bool CheckIsClientUseSkill(SkillUseRet skillUseRet)
        {
            SkillEntity curSkillEntity = ActiveSkillEntity;

            if (curSkillEntity == null)
            {
                return false;
            }

            if (curSkillEntity.IsClientSimulating && curSkillEntity.RuntimeID == skillUseRet.RuntimeID)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 检查技能能否使用，需要区分目前是否 存在主动技能 以及当前的需要使用的技能是否时主动技能
        /// 
        ///  1.如果当前不存在主动技能, 那只需要满足技能的释放条件就都可以释放
        ///  2.如果存在主动技能，就需要区分 技能第一阶段是否需要设置为主动
        ///        如果不设置主动,那可以立即释放
        ///        如果  设置主动，那需要比较技能的优先级，满足才能释放
        /// </summary>
        /// <param name="skillContainer"></param>
        /// <param name="usingSkillType">使用技能的类型</param>
        public E_UseSkillResult CheckCanUseSkill(SkillContainer skillContainer, int skillId,
            E_UseSkillType usingSkillType)
        {
            //先做 技能条件的判断
            E_UseSkillResult result = skillContainer.CheckCanUseSkill(skillId, usingSkillType);

            // 是否是执行效果(类似于蓄力的抬起,走的就是执行效果)
            bool isExecuteEffect = usingSkillType == E_UseSkillType.ExecuteEffect;
            if (result != E_UseSkillResult.Succeed && !isExecuteEffect)
            {
                return result;
            }

            // 如果是宠物技能,那就直接发送协议即可,同时 runtimeID 为0
            if (skillContainer.CurSkillInfo != null && skillContainer.CurSkillInfo.SkillTag == SkillTag.PetSkill)
            {
                return E_UseSkillResult.Send_Use_Skill;
            }

            // 就需要判断当前使用的技能是否存在
            bool hasSkillIDSkill = HasRunningSkillID(skillId);

            // 是否有活跃技能
            bool hasActiveSkill = ActiveSkillEntity != null;

            // 如果有活跃技能, 且当前的遥感的朝向 跟主角的朝向过大, 那么不能释放这个技能
            // 2022/11/16
            // 因为存在 网络延迟, 所以当技能由 活跃 ---> 非活跃时, 服务器的原子锁 并不一定解开
            // 此时, 仍旧禁止 朝向的偏转。
            // 如果此时 摇杆跟 人的朝向 偏差太大,此时 不允许使用这个技能（遥感 会取消技能的预输入）

            // 2023/1/17
            // gl 需要 临时 关闭 技能朝向偏差太多，禁止使用这个技能的逻辑
            // if (playerData.Is___ForbidDir && CheckJoyStickCanCancelUseSkill())
            // {
            //     return E_UseSkillResult.Cur_JoyStick_Driction_Error;
            // }

            // 获得当前这个技能id是否由正在等待用户输入的skillEntity
            SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);
            bool hasSkillUserInput = runningInputSkill != null;


            /// 2023/11/3
            /// gl 有如下几个需求:
            /// 1. 按钮CD的过程中, 如果有 有效的输入轴, 那按钮CD 不拦, 仍可以输入预输入;
            /// 2. 只要 拖动摇杆，就可以使输入轴 效果失效。 摇杆停止移动后， 输入轴效果恢复;
            /// 3. 对于 秒放的蓄力效果， 客户端会 有个最小输入时间。 秒放后， 输入轴效果要被认为已经执行.


            // 是否是使用技能
            bool useSkill = usingSkillType == E_UseSkillType.UsingSkill;


            //SGF.Debuger.Log($"{TagFlag}  [CheckCanUseSkill] [input] skillID  {skillId} , hasSkillIDSkill {hasSkillIDSkill} , hasActiveSkill {hasActiveSkill} , hasSkillUserInput {hasSkillUserInput} , useSkill {useSkill} , isExecuteEffect {isExecuteEffect}");


            // 执行效果的逻辑,例如蓄力的取消逻辑,
            // 执行的条件需要判断当前是否存在这个技能,如果技能不存在(技能被结束),那就不需要执行
            if (isExecuteEffect)
            {
                if (!hasSkillIDSkill || !hasSkillUserInput)
                {
                    // 当要执行结束 效果逻辑但又没有这个技能的时候,返回没有这个技能效果
                    // note:
                    //     如果当前存在优先级较高的活跃技能,此时使用蓄力技能,
                    //     技能的使用也是预输入施法流程,而对于蓄力取消的效果执行,
                    //     此时因为没有存在这个蓄力技能,所以会返回没有技能效果.
                    // note1:
                    //     此时,效果执行失败,应该缓存这个操作.
                    return E_UseSkillResult.No_Skill_Effect;
                }
            }

            // 如果存在 用户输入效果, 但是 用户输入效果 此时不在 设置的 网络安全范围内(预输入效果接近效果尾部)
            // 此时，就不能接受 用户输入
            if (hasSkillUserInput && !runningInputSkill.CheckIsInNetSafeAreaUserInput())
            {
                LogUtils.Log(LogUtils.LogEnum.Skill, "输入轴效果快结束了,不允许用户输入了！！！", skillContainer.IsNormalSkill());
                // 如果 此时在 预输入轴的 非安全区 且当前技能正在 CD, 就 走技能槽的 cd 拦截使用技能的逻辑

                if (skillContainer.IsCD())
                {
                    return E_UseSkillResult.CD_Not_Enough;
                }
                return E_UseSkillResult.Input_At_Forbid;
            }

            // 新增一个 用户输入轴 在 技能阶段尾部 屏蔽用户输入的逻辑
            // 如果 存在用户输入, 且是在 技能阶段的 尾部,那么 此时就不执行
            if (hasSkillUserInput && runningInputSkill.IsAtEndOfStage())
            {
                /// <summary>
                /// 2024/3/18
                /// fixed issue:
                ///     在普攻预输入的非法输入区域点击，客户端会缓存下来预输入,等到下一次 活跃--->非活跃 时候, 开始提前预播.
                ///     但是 此时 策划又配置了普攻技能的 cd, 从而导致 服务器判定cd 会打断客户端的预播动作.
                /// 
                /// gl需求:
                /// ----------------------------------------
                /// 如果有事你再叫我 (高磊), [2024/3/18 11:42]
                /// @段磊 这里加入判断CD逻辑
                /// 如果有事你再叫我 (高磊), [2024/3/18 11:52]
                /// 如果处于CD，就等到CD结束再预播放
                /// ----------------------------------------
                /// </summary>
                /// 
                /// 基于上述gl 需求, 需要:
                /// 1. 预输入还是会发送;
                /// 2. 在客户端预播之前, 先判断是否技能槽被cd, 如果被cd, 那就不预播
                return E_UseSkillResult.Input_At_StageEnd;
            }

            // 如果当前不存在skillID对应的技能实体,那就是走使用技能的逻辑       
            if (!hasSkillIDSkill)
            {
                // 如果没有活跃技能， 那就是 使用新的技能(效果不存在的情况之前已经判定) 
                if (!hasActiveSkill)
                {
                    // 执行成功,即立即使用这个新技能
                    return E_UseSkillResult.Succeed;
                }
                else
                {
                    /// 2023/3/3
                    /// 如果当前存在 活跃技能, 就需要判定 当前新使用技能的优先级.
                    /// 如果优先级 比 当前活跃技能要高, 则应该 走的是 打断 当前活跃技能的流程.
                    /// 
                    /// 目前 跟 kl 沟通 服务器的 逻辑 如下:
                    ///     服务器 收到客户端的 预释放技能的时候,会先去 做优先级比较, 如果优先级高, 则会执行先打断
                    ///     当前活跃,再执行后续技能的流程。
                    ///     如果 优先级低， 则会将 这个技能 设置为 后续技能,等 技能走入非活跃再执行.
                    /// 
                    /// 所以 基于上, 客户端 只需要发送 技能预输入即可, 并不需要由客户端本地 提前比较优先级打断逻辑。
                    /// 
                    // 如果当前存在一个活跃技能,那就
                    // 等到阶段由 活跃--->非活跃的时候执行
                    return E_UseSkillResult.Cache_User_Input;
                }
            }

            // 如果存在技能id对应的技能运行时,那就判断是否开启了用户输入轴
            // 如果输入轴都没开启，那其实还是走用户使用技能的逻辑
            if (!hasSkillUserInput)
            {
                // 如果存在这个技能正在运行,且这个技能还没有用户输入效果,
                // 那就需要判断 这个技能是否 有CD, 如果技能有CD,则返回技能即将进入CD
                // 如果没有CD, 那执行 是否有活跃技能之后的逻辑
                //bool hasCD = skillContainer.CurSkillInfo != null && skillContainer.CurSkillInfo.HasCfgCD();
                //if (hasCD)
                //{
                //    // 技能正在运行, 将会进入CD
                //    return E_UseSkillResult.CD_Will_Running;
                //}

                if (!hasActiveSkill)
                {
                    // 执行成功,即立即使用这个新技能
                    return E_UseSkillResult.Succeed;
                }
                else
                {
                    // 等到阶段由 活跃--->非活跃的时候执行
                    return E_UseSkillResult.Cache_User_Input;
                }
            }

            // 如果开启了用户输入轴了,那就是走用户输入轴的判断逻辑

            // 如果有输入轴效果，但是没有活跃技能，那不管是预输入还是立即输入，都是立即执行输入轴逻辑
            if (!hasActiveSkill)
            {
                return E_UseSkillResult.Run_User_Input_On_No_Active;
            }

            // 如果存在输入轴，且存在活跃技能，就需要判断活跃技能是否就是自己
            bool isSkillActive = runningInputSkill.RuntimeID == activeSkillEntity.RuntimeID;

            // 检查 用户输入轴配置的 是否是预输入
            bool isPrepare = runningInputSkill.CheckIsPrepareUserInput();

            // 如果活跃技能就是输入轴技能，需要区分是否是预输入
            //     预输入:   存下输入cache,等到下一次 变为非活跃时,执行输入轴逻辑
            //     立即输入: 立即执行当前输入轴的逻辑
            if (isSkillActive)
            {
                if (isPrepare)
                {
                    return E_UseSkillResult.Cache_User_Input;
                }
                else
                {
                    // 执行 立即输入
                    return E_UseSkillResult.Run_User_Immediate_Input;
                }
            }
            else
            {
                // 如果存在活跃技能,且 要使用的技能, 不是活跃技能
                // 对于 立即输入还是 预输入,都需要比较优先级
                // 如果优先级
                //      低: 存下输入cache,等到下一次 技能由活跃 ---> 非活跃时,执行输入逻辑
                //      高: 打断当前活跃技能,使用当前技能
                if (runningInputSkill.Priority <= ActiveSkillEntity.Priority)
                {
                    return E_UseSkillResult.Cache_User_Input;
                }
                else
                {
                    return E_UseSkillResult.Break_Active_Run_User_input;
                }
            }
        }

        /// <summary>
        /// 检查 自动战斗 是否 可以使用 这个技能. 
        /// 由于 技能 使用, 客户端 有预输入 和 技能优先级的 打断关系, 所以在模拟 自动战斗的点击的时候, 需要 忽略 预输入和 技能优先级打断的情况.
        /// 如果 每次 简单的 按 固定时间 去 点击按钮, 我担心 会出现 客户端 预播 跟服务器返回结果不一致的情况(网络延迟导致技能预播不一致,会触发客户端技能的 打断重播).
        /// 所以 在自动战斗 检查使用 技能的时候, 就 按照 玩家目前处于 非活跃 阶段的方式判断 是否可以使用技能. 
        /// 这样 客户端 可以避免 预输入 和 预播的问题.
        /// </summary>
        /// <returns></returns>
        public bool CheckCanAutoBattleUseSkill(SkillContainer skillContainer)
        {
            int skillId = skillContainer.CurSkillId;

            if (skillContainer.IsAutoBattleCD())
            {
                return false;
            }

            E_UseSkillResult result = CheckCanUseSkill(skillContainer, skillId, E_UseSkillType.UsingSkill);

            //SGF.Debuger.Log($" CheckCanAutoBattleUseSkill 检查自动战斗 是否可以使用 result: {result}");

            switch (result)
            {
                case E_UseSkillResult.Succeed:
                case E_UseSkillResult.Run_User_Input_On_No_Active:
                    {
                        return true;
                    }
                case E_UseSkillResult.Cache_User_Input:
                    {
                        // 如果这个技能是 普攻并且 存在正在使用的输入轴的时候, 则认为自动战斗可以使用这个 普攻技能
                        if (skillContainer.IsNormalSkill() && HasRunningSkillUserInput(skillId))
                        {
                            return true;
                        }

                        return false;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取 正在运行的 技能 轮盘数据, 如果这个skillID 运行的技能不存在,那就返回 null
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public SkillWheelInfo GetRunningSkillWheelInfo(int skillId)
        {
            // 获得当前这个技能id是否由正在等待用户输入的skillEntity
            SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);
            bool hasSkillUserInput = runningInputSkill != null;

            // 如果有 正在运行的 用户输入轴效果, 那就返回这个技能的 用户输入轴里配置的 轮盘配置
            if (hasSkillUserInput)
            {
                return runningInputSkill.GetSkillWheelConfig();
            }

            // 如果没有, 那就返回 null
            return null;
        }

        public SkillWheelInfo GetSkillWheelInfo(int skillId)
        {
            // 获得当前这个技能id是否由正在等待用户输入的skillEntity
            SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);
            bool hasSkillUserInput = runningInputSkill != null;

            // 如果有 正在运行的 用户输入轴效果, 那就返回这个技能的 用户输入轴里配置的 轮盘配置
            if (hasSkillUserInput)
            {
                return runningInputSkill.GetSkillWheelConfig();
            }

            SkillContainer skillContainer = GetSkillContainer(skillId);
            if (skillContainer != null)
            {
                SkillInfo skillInfo = skillContainer.FindSkillInfo(skillId);
                if (skillInfo != null && skillInfo.cfg != null)
                {
                    SkillWheelInfo skillWheelInfo = new SkillWheelInfo();
                    return skillWheelInfo.Init(skillInfo.cfg);
                }
            }

            return null;
        }

        private E_UseSkillResult UseSkill(SkillContainer skillContainer, SkillUseReq skillUseReq,
            E_UseSkillType usingSkillType, int enterStageTime, float arg = -999)
        {
            if (usingSkillType == E_UseSkillType.None)
            {
                return E_UseSkillResult.Failed;
            }


            bool useSkill = usingSkillType == E_UseSkillType.UsingSkill;

            int skillId = skillUseReq.SkillID;

            //SGF.Debuger.LogError($"{TagFlag} 客户端使用技能: {skillUseReq}");

            E_UseSkillResult result = CheckCanUseSkill(skillContainer, skillId, usingSkillType);
            // SGF.Debuger.LogError($"{TagFlag} [UseSkill] 使用技能 skillID  {skillId} , 检查 result {result} ");
#if (UNITY_EDITOR && BATTLE_DEBUG)
            LogUtils.Log(LogUtils.LogEnum.Skill, $"{TagFlag} [UseSkill] 使用技能 skillID  {skillId} , 检查 result {result} ", playerData.isMainPlayer);
#endif
            // CurSkillInfo 如果为 null, 上面的 CheckCanUseSkill 会是失败;
            SkillInfo skillInfo = skillContainer.CurSkillInfo;
            switch (result)
            {
                case E_UseSkillResult.Send_Use_Skill:
                    {
                        // 直接发送 使用技能,客户端不做预播
                        skillUseReq.RuntimeID = 0;
                        SendPreUseSkillReq(skillUseReq);
                        return E_UseSkillResult.Succeed;
                    }
                    break;

                case E_UseSkillResult.Succeed:
                    {
                        ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能 check: [Succeed], UseNewSkill  skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");

                        // 直接可以使用技能
                        return UseNewSkill(skillInfo, skillUseReq, enterStageTime);
                    }

                case E_UseSkillResult.Input_At_Forbid:
                    {
                        ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能时 check: [Input_At_Forbid], UseNewSkill skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");

                        // 在输入轴的禁止输入范围输入技能,gl需要直接使用新技能
                        return UseNewSkill(skillInfo, skillUseReq, enterStageTime);
                    }
                case E_UseSkillResult.Cache_User_Input:
                    {
                        ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能时 存在活跃阶段,处缓存用户输入 skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");

                        // 需要缓存用户操作,等待进入非活跃时，触发
                        return CacheUserInput(skillUseReq, usingSkillType);
                    }
                case E_UseSkillResult.Run_User_Input_On_No_Active:
                    {
                        SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);
                        //LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 此时没有活跃阶段, 所以立即执行 用户预输入 skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ", playerData.isMainPlayer);

                        // 因为没有活跃阶段,所以此处立即执行用户输入
                        var result1 = PrePlayUserInput(runningInputSkill, skillUseReq);
                        // LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} PrePlayUserInput 用户预输入 skillID: {skillUseReq.SkillID} , result1: {result1} ", playerData.isMainPlayer);
                        return result1;
                    }
                case E_UseSkillResult.Run_User_Immediate_Input:
                    {
                        SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);
                        ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 此时没有活跃阶段, 所以立即执行 用户立即输入 skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");

                        // 没有活跃阶段,所以 立即执行 立即用户输入
                        return PrePlayUserInput(runningInputSkill, skillUseReq, arg);
                    }
                case E_UseSkillResult.Break_Active_Run_User_input:
                    {
                        SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);
                        BreakSkillEntity(activeSkillEntity, E_ClientSkillEndType.UseSkill);

                        // //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 存在活跃技能,但活跃技能的优先级较低, 此时打断活跃技能 [{activeSkillEntity.skillId}] 执行用户输入 skillID: {skillUseReq.SkillID}");

                        // 打断 当前 的活跃技能的 时候, 提前预播 的 用户输入
                        return PrePlayUserInput(runningInputSkill, skillUseReq);
                    }
                case E_UseSkillResult.No_Skill_Effect:
                    {
                        // 当需要执行这个效果但是又没有这个效果的时候,需要缓存这个操作。
                        // 而对于类似蓄力这个技能来说,目前它的执行分为两段,使用技能和 使用效果(结束蓄力).
                        // 对于使用技能,走的是技能的预输入,而对于蓄力结束的使用效果,发给服务器走的是用户操作.
                        // note:
                        //     因为对于技能的预输入和用户操作的预输入,走的是同一个黑板数据(它们相互覆盖),
                        //     如果对执行效果做用户操作的预输入,会覆盖使用技能的预输入.
                        //     所以对于 使用效果的 预输入，此处会单独存放在一边,当收到服务器使用技能的返回的时候,
                        //     判断是否正是自己需要执行效果的 技能:
                        //      如果是: 执行这个 效果的预输入,并且清除这个缓存;
                        //        不是: 说明技能的预输入执行被服务器判断不可释放或者被客户端后续的技能预输入
                        //              给清除掉,释放了其它的技能,那此时,清除此处的 缓存.
                        // //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能但是没有啥技能效果, 所以此处缓存用户输入 skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");
                        return result;
                        // return CacheUserInput(skillUseReq, usingSkillType);
                    }
                case E_UseSkillResult.Input_At_StageEnd:
                    {
                        bool hasSkillUserCache = HasSkillUserInputCache(skillId);

                        if (!hasSkillUserCache)
                        {
                            // 如果 在阶段的尾部 并且之前没有输入过 预输入, 那此时不管预播还是不预播
                            // 其实 都是应该给 服务器发送 预输入操作
                            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能 在阶段的尾部,但是之前没有 预输入过, 所以此处 直接发送预输入操作 skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");
                            return CacheUserInput(skillUseReq, usingSkillType);
                        }
                        else
                        {
                            // 如果 在 阶段的尾部 并且有之前 预输入过 操作的,那此处 可以直接 return
                            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能但是 在阶段的尾部, 所以此处使用新的技能 skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");
                            return result;
                        }

                        // return E_UseSkillResult.Succeed;
                    }
                case E_UseSkillResult.CD_Will_Running:
                    {
                        return E_UseSkillResult.CD_Not_Enough;
                    }
                default:
                    {
                        ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用技能 失败, result: {result} skillID: {skillUseReq.SkillID} ,usingSkillType: {usingSkillType} ");

                        return result;
                    }
            }
        }


        /// <summary>
        /// 客户端使用技能调用的接口
        /// note:
        ///     技能改为抢活跃的多运行时 方式 , 技能是否需要打断,
        ///     只需要在技能 阶段抢占活跃 时处理,使用技能只需要 判定是否可以使用技能即可
        /// </summary>
        public E_UseSkillResult ClientUseSkill(SkillContainer skillContainer, SkillUseReq skillUseReq,
            CastMethodType castMethodType, E_BtnInputType e_BtnInputType)
        {

            E_UseSkillType usingSkillType = GetUseSkillType(castMethodType, e_BtnInputType);

            return UseSkill(skillContainer, skillUseReq, usingSkillType, 0);
        }

        public E_UseSkillResult ClientUseSkill(SkillContainer skillContainer, SkillUseReq skillUseReq,
          CastMethodType castMethodType, E_BtnInputType e_BtnInputType, float arg)
        {

            E_UseSkillType usingSkillType = GetUseSkillType(castMethodType, e_BtnInputType);

            return UseSkill(skillContainer, skillUseReq, usingSkillType, 0, arg);
        }

        public E_UseSkillResult ClientUseSkill(SkillUseReq skillUseReq, CastMethodType castMethodType,
            E_BtnInputType e_BtnInputType, out bool isNormalSkill)
        {

            SkillContainer skillContainer = GetSkillContainer(skillUseReq.SkillID);

            // 返回这个 技能是 普通技能
            isNormalSkill = skillContainer.IsNormalSkill();

            E_UseSkillType usingSkillType = GetUseSkillType(castMethodType, e_BtnInputType);

            return UseSkill(skillContainer, skillUseReq, usingSkillType, 0);
        }

        private E_UseSkillType GetUseSkillType(CastMethodType castMethodType, E_BtnInputType e_BtnInputType)
        {
            E_UseSkillType useSkillType = E_UseSkillType.None;
            switch (castMethodType)
            {
                case CastMethodType.DirectCast:
                    {
                        if (e_BtnInputType == E_BtnInputType.PointerDown)
                        {
                            useSkillType = E_UseSkillType.UsingSkill;
                        }
                    }
                    break;
                case CastMethodType.DirectCastOnLoosen:
                    {
                        if (e_BtnInputType == E_BtnInputType.PointerUp)
                        {
                            useSkillType = E_UseSkillType.UsingSkill;
                        }
                    }
                    break;
                case CastMethodType.WheelCast:
                    {
                        if (e_BtnInputType == E_BtnInputType.PointerUp)
                        {
                            useSkillType = E_UseSkillType.UsingSkill;
                        }
                    }
                    break;
                case CastMethodType.GatherWheelCast:
                    {
                        if (e_BtnInputType == E_BtnInputType.PointerUp)
                        {
                            useSkillType = E_UseSkillType.ExecuteEffect;
                        }
                        else
                        {
                            useSkillType = E_UseSkillType.UsingSkill;
                        }
                    }
                    break;
                case CastMethodType.GatherWheelCastNoIndicator:
                    {
                        if (e_BtnInputType == E_BtnInputType.PointerUp)
                        {
                            useSkillType = E_UseSkillType.ExecuteEffect;
                        }
                        else
                        {
                            useSkillType = E_UseSkillType.UsingSkill;
                        }
                    }
                    break;
                default: break;
            }

            return useSkillType;
        }

        private long clientUseSkillTime = 0;
        private long serverUseSkillTime = 0;

        private ulong c_startTime = 0;

        /// <summary>
        /// 发送使用一个新的技能
        /// </summary>
        /// <param name="skillInfo"></param>
        /// <param name="skillUseReq"></param>
        public E_UseSkillResult UseNewSkill(SkillInfo skillInfo, SkillUseReq skillUseReq, int enterStageTime)
        {
            tempTime = TimeUtils.ClientNowStampMilli;
            ulong useSkillRuntimeID = skillUseReq.RuntimeID;
            bool newSkillID = HasRunningSkill(useSkillRuntimeID) || useSkillRuntimeID == 0;
            FormatSkillUseReq(skillUseReq, newSkillID);

            // 当gm 不设置 提前预播放 的时候, 客户端 就不 提前预播
            if (!IsPrePlayClientSkill)
            {
                SendPreUseSkillReq(skillUseReq);
                return E_UseSkillResult.Succeed;
            }


            // 客户端主动使用技能的时候, 先提前同步一下技能的朝向

            var t1 = TimeUtils.ClientNowStampMilli;

#if (UNITY_EDITOR && BATTLE_DEBUG)
            LogUtils.Log(LogUtils.LogEnum.Skill, $"{TagFlag}  使用技能 [UseNewSkill] skillID: {skillUseReq.SkillID} , runtimeID: {skillUseReq.RuntimeID} , 客户端发送 协议  ", playerData.isMainPlayer);
#endif
            {
                // 发送预播的时间在 技能资源加载之后,防止给服务器发的时间 提前了
                skillUseReq.RunTime = (ulong)TimeUtils.ServerNowStampMilli;
                c_startTime = (ulong)TimeUtils.ServerNowStampMilli;
                SendPreUseSkillReq(skillUseReq);
            }

            SkillEntity skillEntity = CreateSkillEntity(skillInfo, skillUseReq.RuntimeID);
            SetRunningSkill(skillEntity);
            now = TimeUtils.ClientNowStampMilli;

            skillEntity.ClientUseSkill(skillUseReq, enterStageTime);


            var t2 = TimeUtils.ClientNowStampMilli;


            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] 客户端使用技能 skillEntity_[{skillEntity.RuntimeID}  C_{TimeUtils.ClientNowStampMilli} ,S_{TimeUtils.ServerNowStampMilli}, reqTime: {skillUseReq.RunTime} , 客户端开始预播服务器时间: {c_startTime} , 技能的补帧时间: {enterStageTime}ms ,技能预播消耗: {t2 - t1}ms ");

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] 客户端使用技能 skillEntity_[{skillEntity.RuntimeID}_{skillEntity.skillId}__{skillEntity.GetHashCode()}], isRunning: {skillEntity.IsClientRunning}, readyRelease: {skillEntity.ReadyRelease}");

            ActionOnStartSkillStage?.Invoke(skillEntity);

            //如果需要打断技能，并且还需要发送打断协议请求
            //那客户端可以直接发送 useSkillReq
            //SGF.Debuger.Log($"{TagFlag} [memory] [client] [input] [Time] UseNewSkill skillID  {skillUseReq.SkillID} , runtimeID {skillUseReq.RuntimeID} ");
            if (clientUseSkillTime > 0)
            {
                long cost = TimeUtils.ClientNowStampMilli - clientUseSkillTime;
                clientUseSkillTime = TimeUtils.ClientNowStampMilli;

                //   SGF.Debuger.Log($"{TagFlag} [client] UseNewSkill [CurStageTime] clientNow {clientUseSkillTime} , 距离上次客户端 使用技能 相差 : {cost} ms ");
            }
            else
            {
                clientUseSkillTime = TimeUtils.ClientNowStampMilli;
            }


            /*  // 如果配置了自动转向,就通知 表现层 使用技能前,先转向
              if (skillInfo.IsAutoTurnToTarget)
              {
                  ActionOnUseSkill?.Invoke(skillUseReq);
              }*/

            return E_UseSkillResult.Succeed;
        }


        private void FormatSkillUseReq(SkillUseReq skillUseReq, bool useNewID)
        {
            //note:
            //  播放人物动作需要加载资源，所以存在卡帧的问题
            //  所以先create 运行时，加载对应的资源，然后再去发送 useSkillReq
            //  先给skillUseReq 设置 runTime，因为 sendSkillReq在 create之后，create收到的时间为0
            skillUseReq.RunTime = (ulong)TimeUtils.ServerNowStampMilli;
            //note:
            //  1.攻速只对抬手阶段产生影响
            //  2.攻速只在技能释放开始时确定,技能释放过程改变也不会影响
            //      所以客户端需要给服务器传攻速
            skillUseReq.CAttackSpeed = playerData.CurAttackSpeed;
            skillUseReq.Pos = playerData.myOwnerNtt.PbPosition();
            if (useNewID)
            {
                skillUseReq.RuntimeID = GetNextID();
            }
        }

        /// <summary>
        /// 服务器返回 SkillUseRet 的接口
        /// </summary>
        /// <param name="skillUseRet"></param>
        public void OnServerUseSkill(SkillUseRet skillUseRet)
        {
            int skillId = skillUseRet.SkillID;
            ulong runtimeID = skillUseRet.RuntimeID;
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [zz-x] 服务器回复使用技能 runtimeID: {runtimeID}");

            long cost = TimeUtils.ClientNowStampMilli - now;

            //LogUtils.LogError(LogUtils.LogEnum.Skill, $" {TagFlag}  [server] OnServerUseSkill SkillID: {skillId}, time: {skillUseRet.CreateTime}, now cli: {TimeUtils.ClientNowStampMilli} ,ser: {TimeUtils.ServerNowStampMilli},  runtimeID {skillUseRet.RuntimeID} ,cost : {cost}  ");

            // 收到服务器技能的回复后,不管本地是否有什么 技能或者操作的预输入,都清空.
            CleanCacheUserInput();

            //// 清空 waitSendInputCache;
            //CleanWaitSendInputCache();
            //LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 收到 服务器使用回复,清除 waitSendInputCache");


            if (activeSkillEntity != null && activeSkillEntity.RuntimeID != skillUseRet.RuntimeID)
            {
                //   SGF.Debuger.Log($" {TagFlag}  [server] [input]  OnServerUseSkill SkillID {skillId} runtimeID {skillUseRet.RuntimeID} has activeSkillEntity[{activeSkillEntity.GetHashCode()}] runtimeID {activeSkillEntity.RuntimeID} , skillID : {activeSkillEntity.skillId} , IsClientSimulating : {activeSkillEntity.IsClientSimulating} ");
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 收到 服务器使用技能 skillID: {skillId} 时存在 活跃技能: {activeSkillEntity.skillId}, curStage: {activeSkillEntity.CurSkillStage.StageIDStr}, 设置 阶段为 非活跃");


                // 2023/4/19
                // 目前 发现 快速点击技能,会存在 由于客户端 本地计算的 runtimeID 变化,发送了 (1001/1002) 两个技能, 
                // 服务器 实际能够响应的 是 前面 1001 但 客户端 响应 1002
                // 此时，客户端就可以直接杀掉当前预播的技能即可
                // TODO: DL
                //     最终的解决方案 还是 runtimeID 变化的问题,后面要想个法子 处理
                if (activeSkillEntity.skillId == skillId && activeSkillEntity.IsClientSimulating)
                {
                    // 还是应该采用客户端退出,像 普工的第四段就会进入这种情况,此时 只是结束动作,
                    // 等待服务器 返回结束消息,结束技能
                    BreakSkillEntity(activeSkillEntity, E_ClientSkillEndType.ClientQuit);
                }

                // 更高磊 约定的临时方案,
                // 如果服务器协议提前到了,本地还有技能,
                // 此时本地的技能活跃设置为null,优先播服务器反馈的技能.
                // note:
                //  目前这种方案还是有问题的,比如如果存在一个技能,第一段的活跃为false,这样这个技能其实在
                //  存在活跃技能时,也是能释放的.
                // note1:
                //   目前的也是临时方案！！！！
                // BreakSkillEntity(activeSkillEntity, E_ClientSkillEndType.ServerBreak);
                activeSkillEntity = null;
                /// 2023/4/6
                /// 女枪炮 会配置 不同优先级的 技能, 这样,就可能出现 不同技能 优先级的打断.
                /// 而阶段 是否恶意打断, 由 服务器 同步通知,所以 此处 不需要再去 debug.Break.
                //Debug.Break();
            }


            SkillContainer skillContainer = dispatcher.SkillUnitController.GetSkillUnit(skillId);
            //服务器使用的技能id 可能并不一定等于 skillContainer.CurSkillInfo数据
            SkillInfo skillInfo = skillContainer.ServerUseSkill(skillUseRet);

            /// 因为改为多运行时结构,所以 activeSkillEntity 活跃运行时。
            ///     收到SkillUseRet 的时候，需要先查找 本地是否有 相同runtimeID的 运行时,
            ///     如果没有，就相应的创建新的运行时实体

            SkillEntity serverUseSkillEntity;

            bool hasRunningSkillEntity = HasRunningSkill(runtimeID);
            if (!hasRunningSkillEntity)
            {
                serverUseSkillEntity = CreateSkillEntity(skillInfo, runtimeID);
            }
            else
            {
                serverUseSkillEntity = GetRunningSkill(runtimeID);
            }
            // SGF.Debuger.Log($" {TagFlag}  [server] 收到 OnServerUseSkill[{serverUseSkillEntity.GetHashCode()}] SkillID {skillId} runtimeID {skillUseRet.RuntimeID} , entityHashCode {serverUseSkillEntity.GetHashCode()}  , has running :{hasRunningSkillEntity} ");
#if (UNITY_EDITOR && BATTLE_DEBUG)
            LogUtils.Log(LogUtils.LogEnum.Skill,
                $" {TagFlag} 服务器使用技能 [OnServerUseSkill] SkillID {skillId} runtimeID {skillUseRet.RuntimeID},  has running :{hasRunningSkillEntity} ",
                playerData.isMainPlayer);
#endif
            SetRunningSkill(serverUseSkillEntity);
            // ulong? c_time = serverUseSkillEntity.clientSkillUseReq?.RunTime;
            // ulong s_time = skillUseRet.CreateTime;
            // ulong? s_costTime = s_time > c_time ? s_time - c_time : c_time - s_time;

            // var xxxx111 = s_time > c_startTime ? s_time - c_startTime : c_startTime - s_time;
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] [收到服务器使用技能] C_{TimeUtils.ClientNowStampMilli} ,S_{TimeUtils.ServerNowStampMilli} , skillReqTime: {c_time}, skillRetTime: {s_time}, 从skillReq.runtime到skillRet.CreateTime: {s_costTime} , 从预播到服务器回复: {TimeUtils.ClientNowStampMilli - clientUseSkillTime}, 服务器回复CreateTime-客户端开始预播时间: {xxxx111}  ");
            // SGF.Debuger.LogError($"{TagFlag} [收到服务器使用技能] skillUseRet: {skillUseRet} C_{TimeUtils.ClientNowStampMilli} ,S_{TimeUtils.ServerNowStampMilli} , 从预播到服务器回复: {TimeUtils.ClientNowStampMilli - clientUseSkillTime}");

            curServerSkillEntity = serverUseSkillEntity;

            if (serverUseSkillTime > 0)
            {
                // long serverCost = TimeUtils.ClientNowStampMilli - serverUseSkillTime;
                serverUseSkillTime = TimeUtils.ClientNowStampMilli;

                //   SGF.Debuger.Log($"{TagFlag} [server] serverUseSkill [CurStageTime] now ClientTime : {serverUseSkillTime} , 距离上次服务器 使用技能 协议间 相差 : {serverCost} ms ");
            }
            else
            {
                serverUseSkillTime = TimeUtils.ClientNowStampMilli;
            }

            // 收到 服务器技能返回时,同步 人物 和技能的朝向


            serverUseSkillEntity.ServerUseSkill(skillUseRet);

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] 服务器使用技能 skillEntity_[{serverUseSkillEntity.RuntimeID}_{serverUseSkillEntity.skillId}__{serverUseSkillEntity.GetHashCode()}], isRunning: {serverUseSkillEntity.IsClientRunning}, readyRelease: {serverUseSkillEntity.ReadyRelease}");


            //执行Action，参数为技能位
            ActionOnSkillUnitUseSkill?.Invoke(skillContainer);
        }


        int count = 0;

        /// <summary>
        /// 服务器提前打断技能进入cd
        /// </summary>
        /// <param name="skillEndRet"></param>
        public void OnServerEndSkill(SkillEndRet skillEndRet)
        {
            ulong runtimeID = skillEndRet.RuntimeID;
            int skillId = skillEndRet.SkillID;

            if (skillEndRet.EndType != SKillEndType.Default
                //  && skillEndRet.EndType != SKillEndType.SameSkill
                && skillEndRet.EndType != SKillEndType.Outtime
               )
            {
                //Debug.Break();
                //return;
            }
            LogUtils.LogError(LogUtils.LogEnum.Skill, $" {TagFlag} [zz-x]  服务器结束技能 SkillID {skillId} runtimeID {runtimeID} EndType: {skillEndRet.EndType} ");

            // 如果收到技能结束的时候, 存在这个 runtimeID 的 用户输入缓存, 那 就直接清除
            if (HasRuntimeIDUserInputCache(runtimeID))
            {
                CleanCacheUserInput();
            }

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$" {TagFlag}  服务器结束技能 SkillID {skillId} runtimeID {runtimeID} EndType: {skillEndRet.EndType} ");

            // //LogUtils.LogError(LogUtils.LogEnum.Skill,$" {TagFlag}  [server] OnServerEndSkill SkillID {skillId} runtimeID {runtimeID} EndType: {skillEndRet.EndType} ");

            bool hasRunningSkillEntity = HasRunningSkill(runtimeID);
            if (!hasRunningSkillEntity)
            {
                return;
            }

            SkillEntity skillEntity = GetRunningSkill(runtimeID);

            // 收到服务器技能 结束的时候,就去 清除 curServerSkillEntity 的记录
            if (skillEntity == curServerSkillEntity)
            {
                curServerSkillEntity = null;
            }

            bool hasActiveSkillEntity = activeSkillEntity != null;
            bool isActiveSkillEntity = skillEntity == activeSkillEntity;


            // SGF.Debuger.Log($"{TagFlag}  OnServerEndSkill runtimeID={runtimeID},skillId={skillId},EndType={skillEndRet.EndType}");
            //使用 PreSkillReq后，服务器会自动将技能放入后续施法队列
            //同时，服务器会返回一个 restart类型的 SkillEndRet
            //此时客户端不需要重新发送 skillUseReq
            //所以 reSendFunc 不需要调用
            Action reSendFunc = () =>
            {
                if (ActiveSkillEntity == null)
                {
                    return;
                }

                //当不是客户端模拟运行时 的时候
                if (!ActiveSkillEntity.IsClientSimulating)
                {
                    return;
                }

                if (ActiveSkillEntity.clientSkillUseReq == null)
                {
                    return;
                }

                SkillUseReq skillUseReq = ActiveSkillEntity.clientSkillUseReq;
                if (skillUseReq.SkillID == skillEndRet.SkillID)
                {
                    count++;
                    // // SGF.Debuger.Log($"{TagFlag}  OnServerEndSkill SkillID {skillId} runtimeID = {runtimeID}  SKillEndType {(SKillEndType)skillEndRet.EndType} serverSkill not end ,so ---> reSend {count}");
                    SendUseSkillReq(skillUseReq);
                    return;
                }
            };

            //首先干掉 客户端模拟的SkillEntity
            Action cleanClientRuntime = () =>
            {
                //如果能 根据runTimeID找到运行时，那就直接释放
                //BreakSkillEntity会向外面 发送 ActionOnSkillEnd 事件
                BreakSkillEntity(skillEntity, E_ClientSkillEndType.ServerBreak);
                return;
            };

            //服务器认为位置不对，技能需要重启，然后服务器会重新发送 skillUseRet。
            //这个时候，对于客户端来说，其实并不需要清理客户度运行时
            Action restartRuntime = () =>
            {
                cleanClientRuntime();
                // SGF.Debuger.Log($"{TagFlag}  OnServerEndSkill runtimeID {runtimeID} , skillId {skillId} , SKillEndType {skillEndRet.EndType} , curSkillEntity skillID {skillId} ");
            };

            //TODO: dl
            //补丁：
            //  网络太顺畅了，客户端结束了，但是服务器技能还没结束，这个时候服务器判定技能释放失败
            //note:
            //  后续改为客户端 使用技能预释放的方式，但那样，技能的抬手阶段就需要等到服务器回复后，
            //  才能开始进行。
            switch (skillEndRet.EndType)
            {
                case SKillEndType.CdNotEnough:
                    {
                        //cd不足，那就不重发
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.StatusError:
                    {
                        //禁用技能，那就不重发
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.MpNotEnough:
                    {
                        //蓝量不足，那就不重发
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.Outtime:
                    {
                        //输入超时
                        /// 2023/2/3
                        /// 如果因为 输入轴 超时结束,  客户端收到技能回复后,不会立即关闭这个技能,而是等技能播放结束后,
                        /// 才会关闭这个技能.
                        BreakSkillEntity(skillEntity, E_ClientSkillEndType.ServerDefault);
                    }
                    break;
                case SKillEndType.Break:
                    {
                        //被打断
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.Cancel:
                    {
                        //06/20 跟夏哥沟通 
                        //技能的取消 分为
                        //  1.主动 取消 ，skillQuitReq
                        //使用技能主动取消
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.OwnerDead:
                    {
                        //所有者死亡
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.SkillCondNotEnough:
                    {
                        //技能施法条件不满足
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.SpellFailed:
                    {
                        //服务器正在使用技能，所以此时释放技能失败
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.ReStart:
                    {
                        // restart 服务器只会强同步客户端位置
                        // 2022/10/16 
                        // 目前跟kl的约定是 技能不重播,而是强同步坐标

                        //增加了预施法之后，就不需要重发技能了
                        // restartRuntime();
                        return;
                    }
                case SKillEndType.TargetNil:
                    {
                        // 目标对象不存在
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.Default:
                    {
                        // 正常的结束,客户端收到后不做处理,正常播后续逻辑

                        BreakSkillEntity(skillEntity, E_ClientSkillEndType.ServerDefault);
                    }
                    break;
                case SKillEndType.SameSkill:
                    {
                        // 服务器返回 技能结束类型 sameSkill,表明 客户端 可能发送了 多次 一样 runtimeID的 技能使用请求, 此时客户端清除 这个 runtimeID的 预输入缓存就行了
                        if (HasRuntimeIDUserInputCache(skillEndRet.RuntimeID))
                        {
                            CleanCacheUserInput();
                        }
                    }
                    break;
                case SKillEndType.NoConfig:
                    {
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.NoRuntime:
                    {
                        /// 2023/3/23
                        /// 客户端发送了 预输入的操作,但是 服务器没有这个 技能运行时. 那客户端就杀
                        cleanClientRuntime();
                    }
                    break;
                case SKillEndType.NoRunLine:
                    {
                        /// 2023/3/23
                        /// 服务器 收到 客户端的 预输入操作, 服务器 有运行时,但是没有 输入轴效果，此时 会给客户端 返回 NoRunLine
                        /// 目前 客户端不处理这种情况
                        //DB_Close       SGF.Debuger.Log($"{TagFlag}  OnServerEndSkill SkillID {skillId} runtimeID = {runtimeID}  SKillEndType {skillEndRet.EndType} 客户端不处理");
                    }
                    break;
                default:
                    {
                        //DB_Close       SGF.Debuger.Log($"{TagFlag}  OnServerEndSkill SkillID {skillId} runtimeID = {runtimeID}  SKillEndType {skillEndRet.EndType} no handle");
                    }
                    break;
            }

            /// 2023/4/4
            /// 收到 技能 结束之后, 检查一下 结束的技能是否就是当前的活跃技能,
            /// 如果是 活跃技能,那就触发 一下 TriggerWaitSendInputCache();
            /// note:
            ///     收到技能结束可能 存在以下情况:
            ///     1.如果 当前不存在 活跃技能, 那就 可以直接去触发;
            ///     2.如果 当前存在活跃 技能, 那就检查 是否是活跃技能;
            if (!hasActiveSkillEntity || isActiveSkillEntity)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 收到服务器的技能结束 RuntimeID: {runtimeID}, hasActiveSkillEntity: {hasActiveSkillEntity}, isActiveSkillEntity:{isActiveSkillEntity}");
                TriggerWaitSendInputCache();
            }
        }


        /// <summary>
        /// 1.如果存在 活跃技能阶段, 活跃技能阶段 动画不会被移动打断;
        /// 2.如不存在 活跃技能阶段, 那移动应该 打断当前所有 非活跃技能 动画
        /// </summary>
        /// <returns></returns>
        public bool CheckIsMoveBreakAnimation()
        {
            if (ActiveSkillEntity != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 客户端摇杆移动(移动 跟 技能 其实不相关，可以在释放技能中，移动。
        /// 原子锁可以锁定移动和方向，技能收尾阶段可以被移动打断)
        /// 
        /// 移动目前设计改为 不打断阶段，只打断非 活跃技能的阶段
        /// </summary>
        public void OnClientMove()
        {
            if (CheckIsMoveBreakAnimation())
            {
                if (_curAnimParam != null)
                {
                    LogUtils.LogError(LogUtils.LogEnum.Skill, $"[动作] 移动打断非活跃 技能动作, 设置状态: SingleMoving"); ;

                    SetCurAnimParam(null);
                    //直接向外播放 移动指令
                    ActionOnSkillAnimation.Invoke(E_ULayerSubState.SingleMoving);
                }
            }
        }
        /// <summary>
        /// 摇杆一拖动就会 调用的接口. 不关系是否被原子锁锁住, 都会触发这个接口
        /// </summary>
        /// <param name="targetPos"></param>
        public void OnClientJoyStickChange(UnityEngine.Vector3 targetPos)
        {
            if (targetPos == UnityEngine.Vector3.zero)
            {
                MarkUserInputIsValid(true);
                return;
            }
            if (CheckJoyStickCanCancelUseSkill())
            {
                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] 摇杆 执行 取消用户预输入 缓存");

                CancelUserInputReq();
            }

            /// 摇杆拖动 终止 配置了 摇杆输入 中止 预输入的技能
            MarkUserInputIsValid(false);

            if (CheckJoyStickCancleBuff())
            {
                JoyStickCancleBuff();
            }
        }

        private bool CheckJoyStickCanCancelUseSkill()
        {
            // 如果不存在 技能的预输入,那就 不需要取消
            if (!HasUserInputCache())
            {
                return false;
            }

            SkillInputCache userInputCache = GetUserInputCache();

            /// 2023/2/27
            /// 跟 gl 确定, 摇杆取消 技能预输入采用的是 技能 skillUseReq.Rot, 不是用 黑板中的inputRota
            // 先拿到 预输入 缓存的 角度
            int cacheRot = userInputCache.SkillUseReq.Rot;

            // 拿到 当前左摇杆 的 角度
            int leftJoyStickAngle = playerData.myOwnerNttGroup.GetCurLeftJoyStickAngle();

            int joyStickAngle = leftJoyStickAngle - cacheRot;
            // SGF.Debuger.Log($"{TagFlag}  JoyStickAngle : {joyStickAngle} , cacheRot : {cacheRot} ,leftJoyStickAngle : {leftJoyStickAngle}   ");

            if (joyStickAngle <= 30 || joyStickAngle >= 330)
            {
                return false;
            }

            // SGF.Debuger.Log($"{TagFlag}  JoyStickAngle : {joyStickAngle} , cacheRot : {cacheRot} ,leftJoyStickAngle : {leftJoyStickAngle}   ");
            return true;
        }

        private void MarkUserInputIsValid(bool isValid)
        {
            /// 1.检查是否有 缓存的预输入
            ///   如果有缓存, 检查缓存的 预输入技能是否能够 被摇杆指令 清除/
            /// note:
            ///     此处需要跟 高磊确定细节。 目前高磊的 要求是 站着不动的时候，技能能放多段. 移动的话，只能放 第一段
            ///     考虑网络延迟的情况, 可能 在 拖动摇杆的时候, 服务器 已经收到了 普工的预输入。
            ///     这个时候服务器 会让使用技能 第二段。 但是 客户端 新的技能的第一段. 
            ///     所有会有 一定概率的 客户端表现 跟服务器的不一致的情况.
            /// 
            if (!isValid)
            {
                SkillInputCache userInputCache = GetUserInputCache();
                if (userInputCache != null)
                {

                    SkillEntity runningSkill = GetRunningSkill(userInputCache.RuntimeID); ;

                    if (runningSkill != null && runningSkill.GetUserInputCanMarkInvalid())
                    {
                        // 1. 判定 技能缓存 是否需要被摇杆取消;
                        // runningSkill.MarkUserInputInvalid();

                        // 2. 如果配置了 摇杆会 中止数据，那就 取消这个预输入;
                        CancelUserInputReq();
                    }

                }
            }


            // 3.遍历 玩家身上的 所有的 正在运行 技能输入 效果, 标记 所有 可以摇杆取消的输入效果
            skillEntities.ForEach((skill) =>
            {
                skill.MarkUserInputInvalid(isValid);

            });
        }

        /// <summary>
        /// 客户端主动结束技能调用的接口
        /// </summary>
        public void ClientBreakActiveSkill(E_ClientSkillEndType skillEndType)
        {
            BreakSkillEntity(activeSkillEntity, skillEndType);
            // // SGF.Debuger.Log($"{TagFlag}  ClientBreakSkill  runTimeId {runtimeID}");
        }

        private void BreakSkillEntity(SkillEntity skillEntity, E_ClientSkillEndType clientSkillEndType)
        {
            /// 技能的打断分为 几种情况
            /// 1.服务器打断:
            ///     服务器打断分为 正常结束Default 和 异常类型打断,
            ///     Default类型客户端 可以不处理，等客户端技能正常结束即可,所有也不应该调用 BreakSkillEntity;
            ///     异常打断，客户端应该立即结束运行时;
            /// 2.技能取消的打断:
            ///     直接打断整个技能运行时,发送技能运行时取消逻辑;
            /// 3.主动技能抢占导致的打断:
            ///     需要在阶段配置中处理阶段被技能打断,是只打断阶段 还是 打断技能运行时，此逻辑存在于阶段中处理;
            ///
            ///  10.技能移动的打断:
            ///     移动 不打断 阶段, 但是移动可以打断 非活跃技能动画
            if (skillEntity == null)
            {
                return;
            }
            // SGF.Debuger.Log($" {TagFlag}  BreakSkillEntity skillEntity={skillEntity.cfg.ID} clientSkillEndType {clientSkillEndType}1111  ");
            // SGF.Debuger.Log($"{TagFlag} BreakSkillEntity clientSkillEndType {clientSkillEndType}");
            //LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] 准备打断 skillEntity_[{skillEntity.RuntimeID}_{skillEntity.skillId}__{skillEntity.GetHashCode()}], isRunning: {skillEntity.IsClientRunning}, readyRelease: {skillEntity.ReadyRelease} , clientSkillEndType: {clientSkillEndType}");

            switch (clientSkillEndType)
            {
                case E_ClientSkillEndType.ServerDefault:
                    {
                        skillEntity.OnExit(E_SkillExitType.ServerDefault);
                    }
                    break;
                case E_ClientSkillEndType.ServerBreak:
                    {
                        skillEntity.OnExit(E_SkillExitType.ServerBreakSkill);
                    }
                    break;
                case E_ClientSkillEndType.ClientQuit:
                    {
                        skillEntity.OnExit(E_SkillExitType.ClientBreakSkill);

                        //客户端取消, 发送取消协议,等服务器返回
                        SendSkillQuit(skillEntity);
                    }
                    break;
                case E_ClientSkillEndType.UseSkill:
                    {
                        /// 2023/3/3
                        /// 当 技能A 抢占 活跃 打断了 活跃技能B 的时候, 技能 被设置为 活跃, 由客户端自己 演绎。
                        /// 活跃技能 B 被 设置为非活跃, 阶段 打断 不再由 客户端 预播, 而是 由服务器 下发通知.
                        /// 所以 此处 不再直接打断技能, 而是 仅仅 打断 活跃的 标志
                        // skillEntity.OnBreakStage(E_SkillStageExitType.Broken);
                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用新技能打断 SkillEntity_[{skillEntity.RuntimeID}] , skillID: {skillEntity.skillId}, 仅仅设置 打断标志,等待服务器 打断 ");
                        skillEntity.SetBreakActiveTag();
                    }
                    break;
                case E_ClientSkillEndType.CommonBreakSkill:
                    {
                        // 通用的打断类型, 也不管 外面是为啥打断, 此处直接 打断这个技能
                        skillEntity.OnExit(E_SkillExitType.Reset);
                    }
                    break;
                case E_ClientSkillEndType.Reset:
                    {
                        skillEntity.OnExit(E_SkillExitType.Reset);
                    }
                    break;
                default:
                    {
                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 使用新技能打断 SkillEntity_[{skillEntity.RuntimeID}] , skillID: {skillEntity.skillId}, 执行阶段打断 OnBreakStage endType: {clientSkillEndType}");
                        skillEntity.OnBreakStage(E_SkillStageExitType.Broken);
                    }
                    break;
            }
        }

        /// <summary>
        /// skillEntity 结束流程执行的Action
        /// </summary>
        /// <param name="skillEntity"></param>
        public void OnActionSkillExit(SkillEntity skillEntity, E_SkillExitType exitType)
        {
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [zz-x] [client] OnActionSkillExit skillID  {skillEntity.skillId} , runtimeID {skillEntity.RuntimeID}  ");

            ulong runtimeID = skillEntity.RuntimeID;

            //如果删除的 skillEntity是当前运行时, 那么当前运行时引用先移除
            if (ActiveSkillEntity != null && ActiveSkillEntity.RuntimeID == runtimeID)
            {
                SetActiveSkillEntity(null, 0);
            }

            // 技能结束的 时候, 就去同步一次 view层的 服务器朝向
            //SyncServerRotate();

            ActionOnEndSkillStage?.Invoke(skillEntity, exitType);
        }

        public void OnActionRefreshStates(string stageID, List<int> states, bool regist)
        {
            playerData.HandleClientBattleStates(states, regist);
        }


        /// <summary>
        /// 检查是否有 正在运行的 skillID 技能,目前的设定是 非活跃阶段 能够使用相同的技能,
        /// 所以可以存在多个相同的skillID 技能运行时.
        /// </summary>
        /// <param name="skillID"></param>
        public bool HasRunningSkillID(int skillID)
        {
            return GetRunningSkillID(skillID) != null;
        }

        public bool HasRunningSkill(ulong runtimeID)
        {
            return GetRunningSkill(runtimeID) != null;
        }

        /// <summary>
        /// 是否有正在运行是 技能实体
        /// </summary>
        public bool HasRunningSkill()
        {
            var skill = skillEntities.Find((SkillEntity skillEntity) =>
            {
                return !skillEntity.ReadyRelease;
            });
            return skill != null;
        }


        public SkillEntity GetRunningSkillID(int skillID)
        {
            return skillEntities.Find(((SkillEntity skillEntity) =>
            {
                return !skillEntity.ReadyRelease && skillEntity.skillId == skillID;
            }));
        }


        private SkillEntity GetRunningSkill(ulong runtimeID)
        {
            return skillEntities.Find((SkillEntity skillEntity) =>
            {
                return !skillEntity.ReadyRelease && skillEntity.RuntimeID == runtimeID;
            });
        }


        /// <summary>
        /// 技能阶段 的抢位 接口,每个技能阶段都会设置自己是否抢位。
        /// 如果抢位成功,需要打断之前的 技能/技能阶段 (根据阶段的配置 是否打断技能 还是打断阶段)
        /// 如果抢位失败, 那就打断自己整个技能
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="enterStageTime">进入下一个阶段的补偿时间</param>
        /// <param name="isActive"></param>
        /// <param name="force">是否强制设置 活跃, 这个只有 同步服务器技能时,才会使用</param>
        /// <returns></returns>
        public bool TrySetActiveSkill(SkillEntity skillEntity, double enterStageTime, bool isActive, bool force)
        {
            bool hasActiveSkill = activeSkillEntity != null;

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 设置活跃 TrySetActiveSkill skillEntity[{skillEntity.GetHashCode()}] , runtimeID {skillEntity.RuntimeID}, isActive {isActive} , hasActive {hasActiveSkill}, force {force}");

            //如果之前没有活跃技能,那此时我想设置啥就是啥
            if (!hasActiveSkill)
            {
                if (isActive)
                {
                    SetActiveSkillEntity(skillEntity, enterStageTime);
                }
                else
                {
                    SetActiveSkillEntity(null, enterStageTime);
                }

                return true;
            }

            bool setSkillIsActive = hasActiveSkill && activeSkillEntity.RuntimeID == skillEntity.RuntimeID;
            // SGF.Debuger.Log($"{TagFlag}  TrySetActiveSkill skillEntity[{skillEntity.GetHashCode()}] , runtimeID {skillEntity.RuntimeID}, setSkillIsActive {setSkillIsActive}");

            //如果之前存在主动技能,且当前设置的技能就是 之前设置的技能
            //那 这个技能 设置啥就是啥
            if (setSkillIsActive)
            {
                if (isActive)
                {
                    SetActiveSkillEntity(skillEntity, enterStageTime);
                }
                else
                {
                    SetActiveSkillEntity(null, enterStageTime);
                }

                return true;
            }
            //   SGF.Debuger.Log($"{TagFlag}  TrySetActiveSkill skillEntity[{skillEntity.GetHashCode()}] , runtimeID {skillEntity.RuntimeID}, activeSkillEntity[{activeSkillEntity.GetHashCode()}] , activeRuntimeID {activeSkillEntity.RuntimeID}");

            //否则的话,那主动技能 和 当前需要设置的技能就不一样
            //如果 需要抢占主技能,那就需要 比较优先级
            if (isActive)
            {
                // 2022/10/14
                // 如果服务器发的技能 需要抢占活跃,但是优先级又不够,此时切换活跃技能
                // 跟孔磊的 约定是, 客户端 不干掉技能,但是切换活跃技能
                if (force)
                {
                    SetActiveSkillEntity(skillEntity, enterStageTime);
                    return true;
                }

                //compare priority
                if (skillEntity.Priority > activeSkillEntity.Priority)
                {
                    //1. send break activeSkillEntity 
                    BreakSkillEntity(activeSkillEntity, E_ClientSkillEndType.UseSkill);

                    //2. set activeSkillEntity
                    SetActiveSkillEntity(skillEntity, enterStageTime);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //目前存在一个不同的主技能位,且本次不需要抢占主技能位，那其实啥都不用干
                //dont do anything
            }

            return true;
        }

        /// <summary>
        /// 设置 活跃的 技能
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="enterStageTime">进入下一个阶段的时间</param>
        private void SetActiveSkillEntity(SkillEntity skillEntity, double enterStageTime)
        {
            activeSkillEntity = skillEntity;
            string skillId = skillEntity != null ? skillEntity.skillId.ToString() : "null";

            bool isNoneActiveSkill = skillEntity == null;
            // 如果 活跃技能 设置位空 并且 是主角的时候, 发送 主角没有活跃技能的通知
            if (playerData.isMainPlayer && isNoneActiveSkill)
            {
                GameManager.Instance.TriggerEvent(TriggerEventType.M_NoneActiveSkill, isNoneActiveSkill);
            }

            if (skillEntity == null)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [zz-x] [memory]  Set Active SkillEntity null  ");
            }
            else
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag}  [zz-x] Set Active SkillEntity[{skillEntity.GetHashCode()}] , skillID :{skillEntity.skillId} , runtimeID : {skillEntity.RuntimeID} ");
            }

            // 如果 活跃技能变为null,检查是否有用户输入的 cache,如果有,执行
            TriggerExecuteCacheUserInput(enterStageTime);
        }

        /// <summary>
        /// 得到当前的活跃阶段
        /// </summary>
        public SkillStage GetActiveStage()
        {
            if (activeSkillEntity == null)
            {
                return null;
            }

            return activeSkillEntity.CurSkillStage;
        }

        public void SetRunningSkill(SkillEntity skillEntity)
        {
            ulong runtimeID = skillEntity.RuntimeID;
            // int skillId = skillEntity.skillId;
            // int newHash = skillEntity.GetHashCode();

            //1. 先确定 当前 技能实体中 是否存在 相同runtimeID的 技能实体, 是否一致.
            SkillEntity oldSkillEntity = GetRunningSkill(runtimeID);

            if (oldSkillEntity == null)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] SetRunningSkill skillEntity_[{skillEntity.RuntimeID}_{skillEntity.skillId}__{skillEntity.GetHashCode()}], isRunning: {skillEntity.IsClientRunning}, readyRelease: {skillEntity.ReadyRelease}");
                skillEntities.Add(skillEntity);
                return;
            }

            if (oldSkillEntity == skillEntity)
            {
                // 如果 完全一样, 那就说明里面已经有了这个 技能实体
                return;
            }

            // 当 新老 skillEntity不一致,打断 旧的 skillEntity 
            BreakSkillEntity(oldSkillEntity, E_ClientSkillEndType.CommonBreakSkill);
            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [memory] SetRunningSkill skillEntity_[{skillEntity.RuntimeID}_{skillEntity.skillId}__{skillEntity.GetHashCode()}], isRunning: {skillEntity.IsClientRunning}, readyRelease: {skillEntity.ReadyRelease} ");

            skillEntities.Add(skillEntity);
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] SetRunningSkill skillEntity[{newHash}] runtimeID {runtimeID} , skillID {skillId}, count: {skillEntities.Count} ");
        }


        public void OnSkillRuntimeSyncRet(RuntimeSyncRet runtimeSyncRet)
        {
            ulong runtimeID = runtimeSyncRet.RuntimeID;

            SkillEntity skillEntity = GetRunningSkill(runtimeID);
            if (skillEntity == null)
            {
                //服务器说要取消技能运行时，但是客户端没有，咋办
                //还能咋办，客户端除了打个日志，就那样了。
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag}  OnSkillRuntimeSyncRet runtimeID {runtimeID} , not find,error!!! ");
                return;
            }

            skillEntity.OnRuntimeSyncRet(runtimeSyncRet);
        }

        /// <summary>
        /// 客户端蓄力满了之后，告诉服务器蓄力满了
        /// </summary>
        /// <param name="energyedCount"></param>
        public void OnSkillEnergyFullCall(SkillEntity skillEntity)
        {
            SendEnergyEndNotice(skillEntity);
        }

        /// <summary>
        /// 收到服务器通知技能运行时 蓄力结束后的回调。
        /// </summary>
        /// <param name="energyedCount"></param>
        public void OnSkillEnergyStartCall(SkillEntity skillEntity)
        {
            ActionOnEnergyStart?.Invoke(skillEntity);
        }

        /// <summary>
        /// 收到服务器通知技能运行时 蓄力结束后的回调。
        /// </summary>
        /// <param name="energyedCount"></param>
        public void OnSkillEnergyEndCall(SkillEntity skillEntity)
        {
            ActionOnEnergyEnd?.Invoke(skillEntity);
        }

        public void OnEnergyCountChange(int energyedCount, double curStageRunningTime)
        {
            ActionOnEnergyCountChange?.Invoke(energyedCount, curStageRunningTime);
        }

        public void OnActionOnBlackBord(string key, object value)
        {
            ActionOnBlackBord?.Invoke(key, value);
        }

        public void OnActionServerCreateStage(SkillEntity skillEntity, SkillStage skillStage)
        {
            if (curServerSkillEntity != null && curServerSkillEntity == skillEntity)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 当前阶段:{curServerSkillEntity.CurSkillStage.StageIDStr},技能RuntimeID: {skillEntity.RuntimeID}, 服务器通知 阶段创建: {skillStage.StageIDStr}, skill: {skillEntity.skillId} runtimeID: {skillEntity.RuntimeID}");
                TriggerWaitSendInputCache();
            }
        }
    }
}
using ProtoMsg;
using SGF.Time;
using SGF.Unity;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Skill.Utils;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 技能控制器，用来控制和处理:
    ///     1: 主动技能
    ///     2：被动技能
    /// </summary>

    public partial class SkillController
    {
        protected string TagFlag
        {
            get => $"[{EntityId}] [SkillController]";
        }


        #region 通用类型的属性和 Action
        public Action<E_ULayerSubState, I_AnimParam> ActionOnSkillPlayAnim;

        public Action<I_FxParam> ActionOnSkillPlayFx;

        public Action<I_AudioParam> ActionOnSkillPlayAudio;
        public Action<I_AudioParam> ActionOnSkillStopAudio;

        /// <summary>
        /// 技能执行 客户端效果线 的 Func
        /// </summary>
        public Func<ulong, I_EffectParam, BaseBlackBoard, bool, ulong, ulong, int, bool, bool> FuncOnPlayClientSkillEffect;

        /// <summary>
        /// 技能执行 服务器效果线 的 Action
        /// </summary>
        public Func<ulong, I_EffectParam, CustomBlackBoardNode, BaseBlackBoard, int, bool> FuncOnTryPlayServerSkillEffect;
        /// <summary>
        /// 终止 技能效果的 Action
        /// </summary>
        public Action<I_EffectParam, BaseBlackBoard, ulong, ulong> ActionOnSkillStopEffect;

        public Action<I_FxParam> ActionOnSkillStopFx;

        public Action<int, E_CameraEffectType> ActionOnPlayCamera;

        public Action<int, bool, bool, bool, float> ActionOnSkillUpdateRotate;

        /// <summary>
        /// 同步一次 服务器 通知的 最新的角度
        /// </summary>
        public Action ActionOnSyncServerRotate;


        public Action<float> ActionOnBulletOffectY;


        public Action<bool> ActionOnHidden;

        private long now = 0;

        #endregion

        public void EnterFrame()
        {
            ReleaseReadyRemoveSkillEntities();

            UpdateSkillEntities();

            EnterFrameBuff();

            EnterFrameBullet();

            EnterFramePassive();
        }



        /// <summary>
        /// 技能控制器reset的接口，并不会清理action
        /// note：
        ///     只清理当前运行的技能,但 不会清理action 和 对应的 控制器
        /// </summary>
        public void Reset()
        {
            ResetPassive();

            ResetSkillEntities();

            ResetBuff();

            ResetBullet();
        }

        public void Release()
        {
            Reset();

            ldm = null;

            dispatcher = null;

            nextID = 0;
            clientRuntimeIDRecord.Clear();

            ReleaseAction();
            ReleaseBuffAction();
            ReleaseBulletAction();
            ReleasePassiveAction();
            playerData = null;
        }

        public void ReleaseAction()
        {
            ActionOnSkillStopEffect = null;
            ActionOnSkillPlayAnim = null;
            ActionOnSkillPlayFx = null;
            ActionOnSkillPlayAudio = null;
            ActionOnSkillStopAudio = null;
            FuncOnPlayClientSkillEffect = null;
            FuncOnTryPlayServerSkillEffect = null;
            ActionOnSkillStopFx = null;

            ActionOnSkillUnitUseSkill = null;

            ActionOnStartSkillStage = null;
            ActionOnEndSkillStage = null;

            ActionOnEnergyStart = null;
            ActionOnEnergyCountChange = null;
            ActionOnEnergyEnd = null;


            ActionOnBlackBord = null;


            ActionOnSkillAnimation = null;

            ActionOnSkillUpdateRotate = null;

            ActionOnSyncServerRotate = null;

            ActionOnBulletOffectY = null;
            ActionOnHidden = null;
        }

        #region 技能相关逻辑


        public void OnServerRunStage(RunStageRet runStageRet)
        {
            switch (runStageRet.RuntimeType)
            {
                case RuntimeEnumType.Buff:
                    {
                        OnBuffRunStage(runStageRet);
                    }
                    break;
                case RuntimeEnumType.ActiveSkill:
                    {
                        ulong UID = runStageRet.UID;
                        ulong runtimeID = runStageRet.RuntimeID;
                        int stageID = runStageRet.StageID;

                        SkillEntity skillEntity = GetRunningSkill(runtimeID);
                        if (skillEntity == null)
                        {
                            //DB_Close   SGF.Debuger.LogError($"{TagFlag} [server]  OnServerRunStage runtimeID {runtimeID},stageID={stageID},StageLoop={runStageRet.StageLoop} not find,error!!! ");
                            return;
                        }

                        skillEntity.OnServerRunStage(runStageRet, false);
                    }
                    break;
                case RuntimeEnumType.Bullet:
                    {
                        OnBulletRunStage(runStageRet);
                    }
                    break;
                case RuntimeEnumType.PassiveSkill:
                    {
                        OnPassiveRunStageRet(runStageRet);
                    }
                    break;
                default: break;
            }
        }

        /// <summary>
        ///  服务器 通知 客户端 哪些 技能的 阶段 被 恶意打断, 正常的技能自己的 效果打断阶段, 不会给客户端发
        /// </summary>
        /// <param name="runStageForceEndRet"></param>
        public void OnServerRunStageForceEndRet(RunStageForceEndRet runStageForceEndRet)
        {
            SkillEntity skillEntity = GetRunningSkill(runStageForceEndRet.RuntimeID);
            if (skillEntity != null)
            {
                skillEntity.OnServerRunStageForceEndRet(runStageForceEndRet);
            }
        }


        public void OnSkillRuntimeSync(RuntimeSyncRet runtimeSyncRet)
        {
            switch (runtimeSyncRet.RuntimeType)
            {
                case RuntimeEnumType.ActiveSkill:
                    {
                        OnSkillRuntimeSyncRet(runtimeSyncRet);
                    }
                    break;
                case RuntimeEnumType.Buff:
                    {
                        OnBuffRuntimeSync(runtimeSyncRet);
                    }
                    break;
                case RuntimeEnumType.Bullet:
                    {
                        OnBulletRuntimeSync(runtimeSyncRet);
                    }
                    break;
                case RuntimeEnumType.PassiveSkill:
                    {
                        OnPassiveRuntimeSync(runtimeSyncRet);
                    }
                    break;
                default: break;
            }
        }




        private bool CheckCanPlayStageAnim(SkillStage skillStage, I_AnimParam param)
        {

            /// 收到动画的起始帧,需要判断当前是否有活跃阶段. 
            /// 如果不存在活跃阶段: (那当前的动画阶段一定也是非活跃(如果是活跃,被打断阶段后应该也是跳转了))
            ///     判断当前动画阶段的优先级, 如果优先级 > = , 则播放；
            /// 
            /// 如果存在活跃阶段:
            ///     判断活跃阶段是否是 当前动画阶段:
            ///         是: 直接播放;
            ///         否: 那就不播(因为活跃阶段只有一个,如果不是自己,那自己一定非活跃)

            SkillStage activeStage = GetActiveStage();
            // 1.如果存在活跃阶段
            if (activeStage != null)
            {
                if (activeStage == skillStage)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                /// 2023/2/27
                /// gl 新增需求: buff / 被动 的动作 优先级 要大于 技能的非活跃阶段的动作
                // 如果是非活跃的技能阶段,动画优先级最低,那就直接播
                if (_curAnimType == E_StageType.Skill || _curAnimType == E_StageType.None)
                {
                    return true;
                }
                // 如果当前 不是技能的非活跃阶段动画
                // 1.先判断 新波的动画 阶段是不是活跃阶段,如果活跃,那必定要播
                if (skillStage.Active)
                {
                    return true;
                }
                // 2.如果 新播动画阶段也不是活跃阶段, 判断这个动画 是不是 技能阶段动画,
                //   如果是技能 非活跃阶段的动画, 优先级最低,此时,这个 技能动画就不能播
                if (param.AnimType == E_StageType.Skill)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 当前播放 动画的 类型, 可能是 技能/被动/buff/子弹 等等各种类型
        /// </summary>
        private E_StageType _curAnimType = E_StageType.None;
        /// <summary>
        /// 当前的动画 记录, 替代之前的 _curAnimKey , 在 动画开始帧 播放 时记录, 在动画的结束帧 清空
        /// 所以 此处不需要 去监听动画 是否播放完成, 只需要保证逻辑层的 动画事件帧 顺序对,就可以保证 _curAnimKey 的正确性.
        /// </summary>
        private AnimParam _curAnimParam = null;
        /// <summary>
        /// 一份 _curAnimParam 的 缓存拷贝, 增加这个的目的是 为了 不必每次都 copy 生成一个新的 动画 animParam
        /// </summary>
        private AnimParam _curAnimParamCache = new AnimParam();

        /// <summary>
        /// 播放 条件动画的 记录
        /// </summary>
        private AnimAndCondition _curAnimAndCondition = null;

        /// <summary>
        /// SkillController 最终播放动画的接口,理论上调用到此接口时，播放动画一定成功(动画能否播放在上层逻辑中已经拦截).
        /// </summary>
        /// <param name="param"></param>
        /// <param name="isFrameStart">是动画帧的 帧首还是帧尾</param>
        private void OnActionPlayAnim(SkillStage skillStage, I_AnimParam param, bool isFrameStart)
        {
            //如果是 动画首帧,开始播放动画
            if (isFrameStart)
            {
                bool canPlay = CheckCanPlayStageAnim(skillStage, param);
                //SGF.Debuger.LogError($"xxxx--{_curAnimParam?.AnimationPath} ,next {param.Key} , isFrameStart {isFrameStart} , canPlay : {canPlay}");
#if (UNITY_EDITOR && BATTLE_DEBUG)
#endif
                //LogUtils.Log(LogUtils.LogEnum.Skill, $"播放 技能动画 xxxx--{_curAnimParam?.AnimationPath} ,next {param.Key} , isFrameStart {isFrameStart} , canPlay : {canPlay}", playerData.isMainPlayer);
                if (canPlay)
                {
                    // SGF.Debuger.LogError($"xxxx--[动作] anim: {param.Key} , play skill_common "); ;

                    ActionOnSkillPlayAnim.Invoke(E_ULayerSubState.Skill_Common, param);
                    _curAnimType = param.AnimType;
                    SetCurAnimParam(param);
                }
            }
            else
            {
                //如果是动画结束帧,判断是否需要中断状态
                // SGF.Debuger.LogError($"xxxx--{_curAnimParam?.AnimationPath} ,curKey: {_curAnimParam?.Key}, next {param.Key} , isFrameStart {isFrameStart} ");

                // _curAnimKey 可能为 null 或者新的 key
                // 所以,如果动画变了,那这个动画的结束帧就没用了
                if (_curAnimParam == null || (_curAnimParam != null && _curAnimParam.Key != param.Key))
                {
                    // SGF.Debuger.LogError($"xxxx--{_curAnimParam?.AnimationPath},curKey: {_curAnimParam?.Key}, param.Key {param.Key}, return ");
                    return;
                }
                // 清除 动画记录
                SetCurAnimParam(null);
                //SGF.Debuger.LogError($"xxxx--[动作] anim: {param.AnimationPath} --- > null , play BattleIdle "); ;

                ActionOnSkillPlayAnim.Invoke(E_ULayerSubState.BattleIdle, null);
            }
        }

        private void SetCurAnimParam(I_AnimParam animParam)
        {

            // 每次设置 当前 动画参数的时候, 先 取消之前动画参数的条件监听
            UnRegAnimConditionsListener(_curAnimParam);
            if (animParam != null)
            {
                RegAnimConditionsListener(animParam);
            }

            if (animParam == null)
            {
                _curAnimParam = null;
            }
            else
            {
                // 将 animParam 的数据 拷贝到  _curAnimParamCache 中, 解出 _curAnimParam 对 animParam 的直接引用
                animParam.CopyTo(_curAnimParamCache);
                _curAnimParam = _curAnimParamCache;
            }
            // SGF.Debuger.LogError($"xxxx--set Cur {_curAnimParam?.AnimationPath} ,curKey: {_curAnimParam?.Key} ");

            RefreshConditionAnim(null);
        }

        private string GetAnimRegConditionKey()
        {
            // key 需要保证 [每个人] 都是唯一的, 所以 此处直接 使用 hashcode.
            string animRegKey = $"animRegKey_{this.GetHashCode()}";
            return animRegKey;
        }

        private void RegAnimConditionsListener(I_AnimParam animParam)
        {
            string animRegKey = GetAnimRegConditionKey();
            if (animParam.AnimatorSp == null)
            {
                return;
            }
            animParam.AnimatorSp.ForEach((AnimAndCondition animAndCondition) =>
            {
                animAndCondition.Condition.ForEach((AnimConditionTypeSerialize condition) =>
                {
                    switch (condition.AnimConditionType)
                    {
                        case AnimConditionType.Move:
                            {
                                if (condition.Move.IsMove)
                                {
                                    // SGF.Debuger.Log($"[regAnim] reg anim: {animAndCondition.Anim} , reg Move , key : {animRegKey} ");
                                    playerData.RegChangeListener("event_on_Move", RefreshConditionAnim, animRegKey);
                                }
                            }
                            break;
                        default:
                            {
                                //DB_Close   SGF.Debuger.LogError($"reg anim: {animAndCondition.Anim} no handle conditionType: {condition.AnimConditionType} error!!!");
                            }
                            break;
                    }

                });
            });
        }

        private void UnRegAnimConditionsListener(I_AnimParam animParam)
        {
            if (animParam == null)
            {
                return;
            }
            string animRegKey = GetAnimRegConditionKey();
            playerData.UnRegAllTagChangeListeners(animRegKey);
        }

        private void RefreshConditionAnim(object v)
        {
            //SGF.Debuger.Log($"[regAnim] 准备刷新 条件 动画 , _curAnimParam 存在 : {_curAnimParam == null} ");

            if (_curAnimParam == null)
            {
                return;
            }

            AnimAndCondition animAndCondition = CheckAnimConditions(_curAnimParam.AnimatorSp);

            // 如果没有找到 满足条件的 动画, 就需要 判断 当前 正在播放的动画 是不是 _curAnimParam 的默认动画:
            //     如果是 默认动画,那就不用管.   
            //     如果不是,说明 动画发生了 改变, 需要 还原 到 默认的动画.
            if (animAndCondition == null)
            {
                //SGF.Debuger.Log($"[regAnim] 检查不到满足条件的动画");

                if (_curAnimAndCondition == null)
                {
                    //SGF.Debuger.Log($"[regAnim] 历史记录中 也没有 播放过 条件动画 , 所以直接 return");
                    return;
                }
                _curAnimAndCondition = null;
                _curAnimParam.SetCurAnimAndCondition(null);
                //SGF.Debuger.Log($"[regAnim] 历史记录中  播放过 条件动画 , 重新播放 这个 技能状态的 动画");
                //SGF.Debuger.Log($"[动作] 还原条件动画为默认动画: {_curAnimParam.AnimationPath}, startTime: {_curAnimParam.StartTime}"); ;

                _curAnimParam.SetUseLastStateTimeAsStartTime(true);
                // 重新 播放 这个动画
                ActionOnSkillPlayAnim.Invoke(E_ULayerSubState.Skill_Common, _curAnimParam);

                return;
            }

            //SGF.Debuger.Log($"[regAnim] 检查到满足条件的动画 , 播放 满足条件的 动画: {animAndCondition.Anim}");

            _curAnimAndCondition = animAndCondition;
            _curAnimParam.SetCurAnimAndCondition(animAndCondition);

            //SGF.Debuger.Log($"[动作] 刷新默认动画: {_curAnimParam.AnimationPath} 为条件动画: {animAndCondition.Anim} , startTime: {_curAnimParam.StartTime} "); ;
            _curAnimParam.SetUseLastStateTimeAsStartTime(true);


            // 如果 有 新的 满足条件的 动画, 那就播放 这个动画,并做好 记录
            ActionOnSkillPlayAnim.Invoke(E_ULayerSubState.Skill_Common, _curAnimParam);
        }

        private AnimAndCondition CheckAnimConditions(List<AnimAndCondition> animAndConditions)
        {
            foreach (AnimAndCondition animAndCondition in animAndConditions)
            {
                foreach (AnimConditionTypeSerialize condition in animAndCondition.Condition)
                {
                    switch (condition.AnimConditionType)
                    {
                        case AnimConditionType.Move:
                            {
                                if (playerData.M_Is_Moveing == condition.Move.IsMove)
                                {
                                    return animAndCondition;
                                }
                            }
                            break;
                        default:
                            {
                                //DB_Close   SGF.Debuger.LogError($"reg anim: {animAndCondition.Anim} no handle conditionType: {condition.AnimConditionType} error!!!");
                            }
                            break;
                    }
                }
            }
            return null;
        }



        private void OnActionPlayFx(I_FxParam fxParam)
        {
            // 设置特效的 角度 为 逻辑层角度
            // 2022/11/17
            // 高磊说： 播放特效,使用技能配置中的 自动转向 来决定
            // 此处 播放技能特效时, 根据配置,先直接转向 .  所以技能特效的朝向,不需要再单独设置 为逻辑层的朝向
            // fxParam.SetCustomRotate(playerData.myOwnerNtt.EulerAngles);
            if (fxParam.EffectName == "")
            {
                return;
            }
            //SGF.Debuger.LogWarning($"{TagFlag} [x-x] OnActionPlayFx  EffectName {fxParam.EffectName} Key {fxParam.Key}");

            ActionOnSkillPlayFx?.Invoke(fxParam);
        }

        private void OnActionPlayAudio(I_AudioParam audioParam)
        {
            ActionOnSkillPlayAudio?.Invoke(audioParam);
        }

        private void OnActionStopAudio(I_AudioParam audioParam)
        {
            ActionOnSkillStopAudio?.Invoke(audioParam);
        }

        private void OnActionStopFx(I_FxParam fxParam)
        {
            if (fxParam.EffectName == "")
            {
                return;
            }
            // SGF.Debuger.LogWarning($"{TagFlag} [x-x] OnActionStopFx  EffectName {fxParam.EffectName} Key {fxParam.Key}");
            // SGF.Debuger.Log($"[Buff] PlayCreateLoopEffect stop : {fxParam.EffectName}");

            ActionOnSkillStopFx?.Invoke(fxParam);
        }

        private void OnActionPlayCamera(SkillEntity skillEntity, int cameraEffectID, E_CameraEffectType e_CameraEffectType)
        {
            return;
            if (cameraEffectID > 0)
            {
                // SGF.Debuger.Log($"{TagFlag} OnActionPlayCamera  cameraEffectID {cameraEffectID} cameraEffctType {e_CameraEffectType}");

                E_CameraEffectBroadCastType broadCastType = E_CameraEffectBroadCastType.NotBroadCast;

                switch (e_CameraEffectType)
                {
                    case E_CameraEffectType.Shake:
                        {
                            CameraShakeDataCell cfg = LocalDataManager.Instance.GetCameraShakeDataCell(cameraEffectID);
                            broadCastType = (E_CameraEffectBroadCastType)cfg.GetBroadCastType();
                        }
                        break;
                    case E_CameraEffectType.Zoom:
                        {
                            CameraZoomDataCell cfg = LocalDataManager.Instance.GetCameraZoomDataCell(cameraEffectID);
                            broadCastType = (E_CameraEffectBroadCastType)cfg.GetBroadCastType();
                        }
                        break;
                    default:
                        {
                            // SGF.Debuger.Log($"{TagFlag} HandleCameraEffect no handle!!!");
                        }
                        break;

                }

                bool isMainPlayer = playerData.isMainPlayer;
                bool isMonster = playerData.EntityType == E_EntityType.Monster;

                //如果不是怪物,也不是自己
                //说明时其他人,其他人的震屏不会对我自己产生效果
                if (!isMonster && !isMainPlayer)
                {
                    return;
                }

                bool isEmitCameraEvent = false;

                //接下来只考虑 自己 和 怪物 产生的特效的屏幕效果
                switch (broadCastType)
                {
                    case E_CameraEffectBroadCastType.NotBroadCast:
                        {
                            //不广播的特效 只 针对自己播放 技能效果: 比如 砸地
                            //只有在视野范围内才播放震屏
                            //视野判断放在 CamerManager中,HandleCameraEffect 执行的时候,特效还没显示  
                            if (isMainPlayer)
                            {
                                isEmitCameraEvent = true;
                            }

                        }
                        break;
                    case E_CameraEffectBroadCastType.BroadCast:
                        {
                            //广播的 特效, 所有人(怪和其他人) 播放广播特效,都会广播给所有人
                            //不需要考虑谁播放,策划只要配了是广播类型,那就播
                            isEmitCameraEvent = true;
                        }
                        break;

                    default:
                        break;
                }

                if (isEmitCameraEvent)
                {
                    ActionOnPlayCamera.Invoke(cameraEffectID, e_CameraEffectType);
                }

            }
        }


        public void OnActionPlayCameraShake(bool start, CameraShakeJson cameraShake)
        {
            GlobalEvent.OnVirtualCameraShakeEvent?.Invoke(start, cameraShake);
        }

        private void OnActionOnBulletOffectY(float offectY)
        {
            ActionOnBulletOffectY?.Invoke(offectY);
        }

        private void OnActionHidden(bool isHidden)
        {
            ActionOnHidden?.Invoke(isHidden);
        }

        public void OnActionSyncViewRota(int rota, bool isUpdateClient, bool isUpdateServer, bool isSyncView, float maxTime)
        {
            ActionOnSkillUpdateRotate.Invoke(rota, isUpdateServer, isUpdateClient, isSyncView, maxTime);
        }

        /// <summary>
        /// 通知 实体 view层 同步一次 服务器朝向
        /// </summary>
        public void SyncServerRotate()
        {
            ActionOnSyncServerRotate.Invoke();
        }

        public void OnActionUseSkill(SyncSkillUseType syncSkillUseType)
        {
            // 使用技能的通知, 之前 是 在此处同步 view 的 朝向, 现在 改到 技能自己去同步
        }

        #endregion


        #region LUA 层绑定技能事件

        [XLua.LuaCallCSharp]
        public void AddEnergyStartLuaAction(EnergyStart action)
        {
            ActionOnEnergyStart += action;
        }

        [XLua.LuaCallCSharp]
        public void DelEnergyStartLuaAction(EnergyStart action)
        {
            ActionOnEnergyStart -= action;
        }

        [XLua.LuaCallCSharp]

        public void AddEnergyEndLuaAction(EnergyEnd action)
        {
            ActionOnEnergyEnd += action;
        }

        [XLua.LuaCallCSharp]
        public void DelEnergyEndLuaAction(EnergyEnd action)
        {
            ActionOnEnergyEnd -= action;
        }
        [XLua.LuaCallCSharp]

        public void AddEndSkillStageLuaAction(EndSkillStage action)
        {
            ActionOnEndSkillStage += action;
        }

        [XLua.LuaCallCSharp]
        public void DelEndSkillStageLuaAction(EndSkillStage action)
        {
            ActionOnEndSkillStage -= action;
        }
        #endregion


    }
}

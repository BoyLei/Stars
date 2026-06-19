using ProtoMsg;
using SGF.Time;
using SGF.Unity;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Game.Skill
{
    public partial class SkillController
    {

        /// <summary>
        /// 检查现在是否可以执行用户输入轴响应
        /// note:
        ///     此处都是单一职责的接口,只做是否能够执行输入轴逻辑的判断
        ///     至于是否开启用户输入轴,应该在调用此接口之前处理！！！
        /// </summary>
        /// <returns></returns>
        public bool CheckCanExecuteUserInput(SkillEntity skillEntity)
        {

            // 是否是预输入,如果是立即输入,那么就可以立即执行
            bool isPrepare = skillEntity.CheckIsPrepareUserInput();

            // 如果是预输入,需要区分是否由活跃技能,如果不存在活跃,那就是缓存,等待非活跃执行
            // 如果存在活跃,区分是否是自己,如果是自己,等非活跃执行;
            // 如果不是自己,比较优先级,如果优先级不够,缓存下来，如果优先级满足,打断当前技能立即执行.
            bool isSkillActive = false;

            bool hasActiveSkill = activeSkillEntity != null;
            // 如果存在输入轴，且存在活跃技能，就需要判断活跃技能是否就是自己
            if (hasActiveSkill)
            {
                isSkillActive = skillEntity.RuntimeID == activeSkillEntity.RuntimeID;
            }


            // 如果活跃技能就是输入轴技能，需要区分是否是预输入
            //     预输入:   存下输入cache,等到下一次 变为非活跃时,执行输入轴逻辑
            //     立即输入: 立即执行当前输入轴的逻辑
            if (!hasActiveSkill || isSkillActive)
            {
                return true;
            }
            else
            {
                // 如果存在活跃技能,且 要使用的技能, 不是活跃技能
                // 对于 立即输入还是 预输入,都需要比较优先级
                // 如果优先级
                //      低: 存下输入cache,等到下一次 技能由活跃 ---> 非活跃时,执行输入逻辑
                //      高: 打断当前活跃技能,使用当前技能
                if (skillEntity.Priority <= ActiveSkillEntity.Priority)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }





        /// <summary>
        /// 计算服务器黑板数据 是否缓存还是执行, 如果缓存，返回true；
        /// 默认为 返回 False
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="effectParam"></param>
        /// <param name="baseBlackBoard"></param>
        /// <returns></returns>
        private bool CheckIsCacheServerEffect(E_StageType playEffectType, ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard baseBlackBoard)
        {
            bool result = false;

            /// 判断是否需要缓存,有两种情况:
            /// 1.服务器数据到了,但是客户端还未开始执行这个效果;
            /// 2.类似于 预输入轴效果这种,收到了服务器的效果数据,但是还不满足执行条件的情况；

            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.UserInput:
                    {
                        /// 1.检查现在是否开启用户输入,如果连预输入都没开启,那么表示客户端还未执行这个效果,需要先缓存 
                        /// note:
                        ///     1: 此处不能直接判断效果轴是否正在开启(CheckIsOpenUserInput)，因为客户端可以提前触发输入轴
                        ///        效果,关闭输入轴;
                        ///     2: 此处需要判断 效果的 outPutKey 对应的黑板数据是否存在,如果不存在,说明客户端效果还没开始执行;
                        ///     3: 如果存在客户端 黑板数据,需要判断是否能够立即执行这个效果，如果可以,那就立即执行;
                        ///     4: 如果是预输入的效果轴,需要缓存当前的服务器效果数据


                        string outputKey = effectParam.OutputKey;

                        //DB_Close       SGF.Debuger.Log($"{TagFlag} [server] [Input] CheckIsCacheServerEffect effectParam {effectParam.OutputKey}");
                        bool clientHasOutPutKey = EffectUtils.CheckIsBlackBoardHasKey(baseBlackBoard, outputKey, E_BlackBoardTag.Client);
                        // 服务器数据先到,那理论上应该缓存
                        if (!clientHasOutPutKey)
                        {
                            return true;
                        }
                        bool isSkill = playEffectType == E_StageType.Skill;

                        SkillEntity skillEntity = GetRunningSkill(runtimeID);

                        // 服务器下发了 输入轴的效果数据,但此时客户端 已经不存在这个输入轴效果的技能
                        // 那就不执行这个服务器效果，而是直接缓存
                        if (skillEntity == null)
                        {
                            //SGF.Debuger.LogError($"{TagFlag} [server] [Input] CheckIsCacheServerEffect not find skill[{runtimeID}] , do cache server blackBoard , error!!!");
                            return true;
                        }

                        // 如果客户端的输入轴效果存在,检查输入轴是否被客户端关闭
                        bool isOpenUserInput = skillEntity.CheckIsOpenUserInput();

                        // 如果输入轴关闭,如果是 客户端关闭, 那么就要去比较 客户端服务器的效果是否一致,不一致就要执行 recover
                        // 如果 不是客户端关闭，而是还没开启, 那就是先缓存.
                        // 而本处都不需要考虑缓存的问题,所以 直接返回 true
                        if (!isOpenUserInput)
                        {
                            //DB_Close       SGF.Debuger.Log($"{TagFlag} [server] [Input] CheckIsCacheServerEffect  isOpenUserInput : [{isOpenUserInput}] , do cache server blackBoard ");
                            return true;
                        }

                        // 如果效果轴还是开启的,那就交给skillEntity判断当前的服务器效果是否需要执行
                        bool canExecuteUserInput = CheckCanExecuteUserInput(skillEntity);
                        // 如果当前不可以执行用户输入轴效果,那就缓存
                        if (!canExecuteUserInput)
                        {
                            //DB_Close       SGF.Debuger.Log($"{TagFlag} [server] [Input] CheckIsCacheServerEffect  canExecuteUserInput : [{canExecuteUserInput}] , do cache server blackBoard ");
                            return true;
                        }

                        return false;
                    }
                    break;

                default:
                    {
                        // 默认的情况,收到服务器的效果数据,都是立即执行
                        result = false;
                    }
                    break;
            }


            return result;
        }

        /// <summary>
        /// 服务器效果跟客户端预演不一致时的效果恢复
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        private void RecoverServerEffect(E_StageType playEffectType, ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            // TODO: DL
            //SGF.Debuger.LogError($"{TagFlag} [server] [Input] RecoverServerEffect effectParam {effectParam.OutputKey}");

        }

        /// <summary>
        /// 阶段 尝试去 播放 服务器同步过来的黑板数据
        /// </summary>
        /// <param name="playEffectType"></param>
        /// <param name="runtimeID"></param>
        /// <param name="effectParam"></param>
        /// <param name="serverCustomBlackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        /// <returns></returns>
        private bool OnFuncStageTryPlayServerEffect(E_StageType playEffectType, ulong runtimeID,
            I_EffectParam effectParam, CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            return OnFuncTryPlayServerEffect(playEffectType, runtimeID, effectParam, serverCustomBlackBoardNode, baseBlackBoard, 0);
        }

        /// <summary>
        /// 阶段 注册 服务器效果线的接口
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="skillStage">效果注册时的阶段</param>
        /// <param name="regEffectParam">本地创建的封装的效果数据</param>
        /// <param name="serverCustomBlackBoardNode">服务器传过来的黑板数据</param>
        /// <param name="baseBlackBoard">黑板</param>
        /// <param name="effectRecoverTime">这个效果 自己的恢复时间</param>
        private bool OnFuncTryPlayServerEffect(E_StageType playEffectType, ulong runtimeID,
            I_EffectParam regEffectParam, CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard, int effectRecoverTime)
        {

            string outPutKey = regEffectParam.OutputKey;
            string serverKey = serverCustomBlackBoardNode.Key;

            /// 2023/3/8
            ///  服务器效果线的 触发 已经由 单个 outputKey 触发 变为 由 inputKeys 和 outputKey 共同触发的逻辑.
            ///  当 由 inputKeys 触发 这个效果的时候, 服务器黑板中 并不存在 outputKey 对应的 数据.
            ///  所以 此处 serverCustomBlackBoardNode 传过来的 可能是 outputKey 的黑板值  也可能是 inputKeys传过来的值.
            ///  如果要 跑 以下的 结果流程 比较, 则需要 比较 serverCustomBlackBoardNode 是否就是 effectParam 需要的数据
            {
                /// 2022/10/21 
                /// 新增一套服务器效果执行流程，用来统一处理收到服务器效果数据后,是立即执行/不执行/缓存/还是执行回滚恢复逻辑.
                /// 判断流程如下:
                /// 
                /// 效果是否已经客户端执行: 
                ///     Y: 客户端服务器结果 是否一致:
                ///          一致: return;
                ///          不一致: 回滚恢复；
                ///     N: 判断效果是否立即执行
                ///          立即执行: run Now;
                ///          否则: 存入缓存，等待其它逻辑触发    


                if (outPutKey != serverKey)
                {
                    //SGF.Debuger.LogError($"{TagFlag} 效果监听outPutKey: {outPutKey} 由 key: {serverKey} 触发, 所以不走效果比较流程 ");
                }
                else
                {
                    CustomBlackBoardNode clientCustomBlackBoardNode = (CustomBlackBoardNode)baseBlackBoard.Get(outPutKey, E_BlackBoardTag.Client);

                    /// 2023/5/12
                    /// 新增一个 预输入操作 同步 服务器朝向的问题. 
                    /// 目前 客户端 收到服务器的 黑板效果后, 会检查 一次 客户端 和服务器的结果是否一致. 
                    /// 对于 输入轴的 效果来说, 检查到 客户端 已经 提前执行, 并且结果一致, 那就不触发 输入轴的 服务器线(输入轴效果 只需要触发一次即可).
                    /// 
                    /// 所以, 对于输入轴效果, 不管客户端有没有提前触发, 在收到了服务求这个效果之后, 都立即去同步一次 输入轴效果里面的坐标和  朝向即可
                    if (regEffectParam.SkillEffectType == E_SkillEffect.UserInput)
                    {
                        UpdateServerUserInputEffct(runtimeID, regEffectParam, serverCustomBlackBoardNode, baseBlackBoard, effectRecoverTime);
                    }


                    // 客户端已经执行了这个效果
                    bool HasRunClientEffect = clientCustomBlackBoardNode != null && clientCustomBlackBoardNode.state == E_BlackBoardNodeState.ClientUsed;

                    bool serverEffectResult = serverCustomBlackBoardNode.OutputResult;


                    //DB_Close       SGF.Debuger.Log($"{TagFlag} [server] [Input] OnFuncTryPlayServerEffect key : {outPutKey} , HasRunClientEffect : {HasRunClientEffect} , serverEffectResult : {serverEffectResult}");

                    if (HasRunClientEffect)
                    {
                        // 如果服务器效果 和 客户端效果执行的结果不一致,那就需要做服务器效果的恢复
                        // 需要根据不同的效果类型,做不同的恢复逻辑
                        // 比如 类似一些 判断效果, 客户端 判断的 结果 为false, 服务器 为 true, 那么 结果就不一致, 需要等服务器数据过来后 做恢复逻辑
                        if (clientCustomBlackBoardNode.OutputResult != serverEffectResult)
                        {
                            // 更新 黑板结果为  服务器的结果
                            clientCustomBlackBoardNode.OutputResult = serverEffectResult;
                            // TODO: DL
                            // Do Recover
                            // 执行效果的恢复逻辑
                            RecoverServerEffect(playEffectType, runtimeID, regEffectParam, serverCustomBlackBoardNode, baseBlackBoard);
                            return true;
                        }
                        else
                        {
                            // 如果这个效果,客户端已经执行且跟服务器结果一致,需要区分是什么样的效果
                            // 对于普通的服务器效果,客户端效果执行和服务器效果执行不冲突.
                            // 但对于输入轴 这种 客户端和服务器共同控制的效果，服务器和客户端 一个执行后《
                            // 另一个就不需要执行了
                            if (regEffectParam.SkillEffectType == E_SkillEffect.UserInput)
                            {
                                //SGF.Debuger.LogError($"{TagFlag} [server] [Input] OnFuncTryPlayServerEffect effectParam {regEffectParam.OutputKey} , client run ,return");

                                return true;
                            }
                        }
                    }
                }
            }


            //SGF.Debuger.Log($"{TagFlag} [server] [Input] OnFuncTryPlayServerEffect HandleServerEffect");
            bool needCacheServerEffect = CheckIsCacheServerEffect(playEffectType, runtimeID, regEffectParam, baseBlackBoard);

            if (needCacheServerEffect)
            {
                // Do Cache Server Effect
                // 执行 缓存当前的服务器效果
                //DB_Close       SGF.Debuger.Log($"{TagFlag} [server] [Input] OnFuncTryPlayServerEffect effectParam {regEffectParam.OutputKey} , need do cache!!");
                //SGF.Debuger.LogError($"{TagFlag} 触发监听效果key: {outPutKey} , 服务器黑板key: {serverKey} , need do cache!!");

                return false;
            }
            bool result = HandleServerEffect(playEffectType, runtimeID, regEffectParam, serverCustomBlackBoardNode, baseBlackBoard, effectRecoverTime);

            return result;
        }

        public void UpdateServerUserInputEffct(ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard, int effectRecoverTime)
        {
            SkillEntity skillEntity = GetRunningSkill(runtimeID);
            if (skillEntity == null)
            {
                return;
            }
            /// 预输入的角度 同步, 可能分为 :
            ///     1. 玩家自己的 恢复阶段 中的 恢复数据,那此时 就不用管这个数据;
            ///     2. 玩家自己的 正在使用技能中的 输入轴数据, 此时需要使用这个数据;
            ///     3. 其它玩家 同步的数据轴的数据, 此时其实不太好 去区分这个效果是不是恢复的数据(不确定服务器会不会把恢复数据也给客户端)

            /// 目前线 按 effectRecoverTim >500ms 来粗略判定一下, 如果是 延迟很大, 就认为它是恢复逻辑, 不予处理
            if (effectRecoverTime > 500)
            {
                SGF.Debuger.LogWarning($"{TagFlag}  key: {effectParam.OutputKey} , 服务器黑板key: {blackBoardNode.Key} 的朝向/坐标 超时失败, effectRecoverTime: {effectRecoverTime} ");
                return;
            }
            PreUserInputData preUserInputData = (PreUserInputData)blackBoardNode.Value;
            // 服务器 回复超时, 则不处理
            if (!preUserInputData.IsExcute)
            {
                return;
            }

            /// 2023/6/26
            /// gl 加了个 技能转向 最大时间的配置, 所以 顺便着把 技能的朝向 梳理了一遍.
            /// 目前 对于 朝向这块, 客户端 的想法 是 客户端 先不提前 预播 朝向(可能发现前后端不一致的情况).
            /// 朝向的 变化 由服务器 通知下来后 进行同步.
            /// 
            /// 那相应的 , 客户端 就完全 信赖服务器的 朝向变化(认为 服务器发过来的朝向即是最终朝向).
            /// 此处 朝向 直接 同步到技能.
            skillEntity.UpdateSkillUserInput(effectParam, preUserInputData);

            // TODO
            // 服务器 还会传个坐标过来, 后面再看要不要加, 加的话 应该也是只同步 服务器 数据
        }

        /// <summary>
        /// 统一处理 技能 服务器效果的接口
        /// </summary>
        /// <param name="runtimeID"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="effectRecoverTime">效果 自己的恢复时间</param>
        /// <returns></returns>
        private bool HandleSkillServerEffect(ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard, int effectRecoverTime)
        {
            SkillEntity skillEntity = GetRunningSkill(runtimeID);
            if (skillEntity == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} [server] [Input] HandleSkillServerEffect effect[{effectParam.EffectID}] , cant find skill[{runtimeID}] error!!!");
                return false;
            }

            bool result = false;
            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.SetSkillCD:
                    break;
                case E_SkillEffect.SetSkillCDPercent:
                    break;
                case E_SkillEffect.TarGroup:
                    break;
                case E_SkillEffect.WaitInput:
                    break;
                case E_SkillEffect.IsInput:
                    break;
                case E_SkillEffect.IsStage:
                    break;
                case E_SkillEffect.ChangeCD:
                    break;
                case E_SkillEffect.BreakCurRuntime:
                    break;
                case E_SkillEffect.AddBuff:
                    break;
                case E_SkillEffect.RemoveBuff:
                    break;

                case E_SkillEffect.UserInput:
                    {
                        HandleServerInputEffect(skillEntity, effectParam, blackBoardNode, baseBlackBoard);
                    }
                    break;
                case E_SkillEffect.Energy:
                    {
                        HandleServerChargeInputEffect(skillEntity, effectParam, blackBoardNode, baseBlackBoard);
                    }
                    break;
                default:
                    {
                        // 目前的设定,对人 的操作,由人处理;
                        // 对技能的效果,由技能自己处理
                        result = FuncOnTryPlayServerSkillEffect.Invoke(runtimeID, effectParam, blackBoardNode, baseBlackBoard, effectRecoverTime);
                    }
                    break;
            }

            return result;
        }

        private bool HandleBuffServerEffect(ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            SkillBuff skillBuff = GetSkillBuff(runtimeID);
            if (skillBuff == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} [server] [Input] HandleBuffServerEffect effect[{effectParam.EffectID}] , cant find skill[{runtimeID}] error!!!");
                return false;
            }

            // 目前的设定,对人 的操作,由人处理;
            // 对技能的效果,由技能自己处理
            bool result = FuncOnTryPlayServerSkillEffect.Invoke(runtimeID, effectParam, blackBoardNode, baseBlackBoard, 0);

            return result;
        }

        /// <summary>
        /// 处理服务器对于的 效果
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="effectRecoverTime">这个 效果 它自己的 恢复时间</param>
        private bool HandleServerEffect(E_StageType playEffectType, ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard, int effectRecoverTime)
        {
            switch (playEffectType)
            {
                case E_StageType.Skill:
                    {
                        return HandleSkillServerEffect(runtimeID, effectParam, blackBoardNode, baseBlackBoard, effectRecoverTime);
                    }
                case E_StageType.Buff:
                    {
                        return HandleBuffServerEffect(runtimeID, effectParam, blackBoardNode, baseBlackBoard);
                    }
                default:
                    {
                        // 被动 子弹的效果 处理 也考虑恢复时间.
                        return FuncOnTryPlayServerSkillEffect.Invoke(runtimeID, effectParam, blackBoardNode, baseBlackBoard, effectRecoverTime);
                    }
            }
        }

        private bool TryPlaySkillClientEffect(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, ulong builderID, ulong ownerEntityID, int stageRecoverTime, bool isRecover)
        {
            SkillEntity skillEntity = GetRunningSkill(runtimeID);
            if (skillEntity == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} [server] [Input] TryPlaySkillClientEffect effect[{effectParam.EffectID}] , cant find skill[{runtimeID}] error!!!");
                return false;
            }
            // 先判断这个效果 的outputkey 服务器有没有发送过来,如果发送过来了,客户端就不需要执行了
            bool findAll = EffectUtils.CheckIsBlackBoardHasKey(blackBoard, effectParam.OutputKey, E_BlackBoardTag.Server);
            // 客户端效果线执行逻辑,并返回这个效果 根据本地计算得出的效果是否能够执行的结果
            bool result = true;

            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.SetSkillCD:
                    break;
                case E_SkillEffect.SetSkillCDPercent:
                    break;
                case E_SkillEffect.TarGroup:
                    break;
                case E_SkillEffect.WaitInput:
                    break;
                case E_SkillEffect.IsInput:
                    break;
                case E_SkillEffect.IsStage:
                    break;
                case E_SkillEffect.ChangeCD:
                    break;
                case E_SkillEffect.BreakCurRuntime:
                    break;
                case E_SkillEffect.AddBuff:
                    break;
                case E_SkillEffect.RemoveBuff:
                    break;

                case E_SkillEffect.UserInput:
                    {
                        HandleClientUserInputEffect(skillEntity, effectParam, blackBoard, isFrameStart, findAll);
                    }
                    break;
                case E_SkillEffect.Energy:
                    {
                        HandleClientChangeInputEffect(skillEntity, effectParam, blackBoard, isFrameStart, findAll);
                    }
                    break;
                case E_SkillEffect.BreakCurRuntimeInBullet:
                    {
                        /// 2024/06/13
                        /// 技能 新接入 打断阶段效果, 只在 主动的技能运行时中处理
                        /// 
                        HandleClientBreakCurRuntimeInBullet(skillEntity, effectParam, blackBoard, isFrameStart, findAll);
                    }
                    break;
                default:
                    {
                        // 目前的设定,对人 的操作,由人处理;
                        // 对技能的效果,由技能自己处理
                        //if (effectParam.EffectArgs.Damage != null)
                        //{
                        //    // SGF.Debuger.LogWarning($"受击动作时间  GroupID={effectParam.EffectArgs.Damage.GroupID},name={effectParam.EffectArgs.Damage.ActorFile},time={SGF.Time.TimeUtils.TimeLogString()}");
                        //}
                        result = FuncOnPlayClientSkillEffect.Invoke(runtimeID, effectParam, blackBoard, isFrameStart, builderID, ownerEntityID, stageRecoverTime, isRecover);
                    }
                    break;
            }
            return result;
        }

        private bool TryPlayBuffClientEffect(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, ulong builderID, ulong ownerEntityID, bool isRecover)
        {
            SkillBuff skillBuff = GetSkillBuff(runtimeID);
            if (skillBuff == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} [server] [Input] TryPlayBuffClientEffect effect[{effectParam.EffectID}] , cant find buff[{runtimeID}] error!!!");
                return false;
            }
            // 先判断这个效果 的outputkey 服务器有没有发送过来,如果发送过来了,客户端就不需要执行了
            bool findAll = EffectUtils.CheckIsBlackBoardHasKey(blackBoard, effectParam.OutputKey, E_BlackBoardTag.Server);
            // 客户端效果线执行逻辑,并返回这个效果 根据本地计算得出的效果是否能够执行的结果
            // 目前的设定,对人 的操作,由人处理;
            // 对技能的效果,由技能自己处理
            //if (effectParam.EffectArgs.Damage != null)
            //{
            //    // SGF.Debuger.LogWarning($"受击动作时间  GroupID={effectParam.EffectArgs.Damage.GroupID},name={effectParam.EffectArgs.Damage.ActorFile},time={SGF.Time.TimeUtils.TimeLogString()}");
            //}
            bool result = FuncOnPlayClientSkillEffect.Invoke(runtimeID, effectParam, blackBoard, isFrameStart, builderID, ownerEntityID, 0, isRecover);

            return result;
        }

        private bool StageTryPlayClientEffect(E_StageType playEffectType, ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, ulong builderID, ulong ownerEntityID)
        {
            return TryPlayClientEffect(playEffectType, runtimeID, effectParam, blackBoard, isFrameStart, builderID, ownerEntityID, 0, false);
        }

        private bool TryPlayClientEffect(E_StageType playEffectType, ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, ulong builderID, ulong ownerEntityID, int stageRecoverTime, bool isRecover)
        {
            /// <summary>
            /// 执行效果的逻辑目前做了以下的分层处理:
            ///     1.事件帧播放了效果帧,通知Stage阶段;
            ///     2.Stage阶段 并不需要知道自己播放效果细节,所以通知 技能SkillEntity
            ///     3.SkillEntity 也不需要关心效果执行的细节,但是需要把控效果执行的串行逻辑,执行Next分支，
            ///       之后，请求SkillController 统一处理效果
            ///     4.而对于 player来说,只需要提供最简单的PlayServerEffect 和 PlayClientEffect 接口即可,
            ///       PlayClientEffect对具体的每种效果类型做处理,产生黑板数据,并且返回黑板执行的result
            ///     
            ///     5.所以 skillController 只需要调度 分别执行 客户端/服务器效果线逻辑即可
            /// 
            /// </summary>

            // 客户端效果线执行逻辑,并返回这个效果 根据本地计算得出的效果是否能够执行的结果
            bool result = true;
            switch (playEffectType)
            {
                case E_StageType.Skill:
                    {
                        return TryPlaySkillClientEffect(runtimeID, effectParam, blackBoard, isFrameStart, builderID, ownerEntityID, stageRecoverTime, isRecover);
                    }
                case E_StageType.Buff:
                    {
                        return TryPlayBuffClientEffect(runtimeID, effectParam, blackBoard, isFrameStart, builderID, ownerEntityID, isRecover);
                    }
                default:
                    {
                        result = FuncOnPlayClientSkillEffect.Invoke(runtimeID, effectParam, blackBoard, isFrameStart, builderID, ownerEntityID, 0, isRecover);
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// 效果 的 停止 逻辑
        /// </summary>
        /// <param name="playEffectType"></param>
        /// <param name="runtimeID"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="builderID"></param>
        /// <param name="ownerEntityID"></param>
        /// <returns></returns>
        private bool TryStopEffect(E_StageType playEffectType, ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, ulong builderID, ulong ownerEntityID)
        {
            // 客户端效果线执行逻辑,并返回这个效果 根据本地计算得出的效果是否能够执行的结果
            bool result = true;
            switch (playEffectType)
            {
                case E_StageType.Skill:
                    {
                        return TryStopSkillEffect(runtimeID, effectParam, blackBoard, builderID, ownerEntityID);
                    }
                case E_StageType.Buff:
                    {
                        // TODO DL 
                        return true;
                    }
                default:
                    {
                        // TODO DL
                        return true;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// 尝试去 终止 效果, 一般来说都应该终止成功,但是 如果这个 技能实体都不存在了,就会返回 false
        /// </summary>
        /// <param name="runtimeID"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="builderID"></param>
        /// <param name="ownerEntityID"></param>
        /// <returns></returns>
        private bool TryStopSkillEffect(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, ulong builderID, ulong ownerEntityID)
        {
            SkillEntity skillEntity = GetRunningSkill(runtimeID);
            if (skillEntity == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} [server] [Input] TryStopSkillEffect effect[{effectParam.EffectID}] , cant find skill[{runtimeID}] error!!!");
                return false;
            }

            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.UserInput:
                    {
                        // 不确定后续 用户输入 是否 会加入 提前stop 的逻辑
                    }
                    break;
                case E_SkillEffect.Energy:
                    {
                        // 不确定后续 蓄力输入 是否 会加入 提前stop 的逻辑
                    }
                    break;
                default:
                    {
                        ActionOnSkillStopEffect.Invoke(effectParam, blackBoard, builderID, ownerEntityID);
                    }
                    break;
            }
            return true;
        }

        private void HandleClientBreakCurRuntimeInBullet(SkillEntity skillEntity, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, bool findAll)
        {
            skillEntity.OnBreakCurRuntimeInBullet(effectParam, blackBoard, isFrameStart, findAll);
        }

        /// <summary>
        /// 处理用户输入的效果
        /// </summary>
        /// <param name="skillEntity"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="isFrameStart"></param>
        /// <param name="findAll"></param>
        private void HandleClientUserInputEffect(SkillEntity skillEntity, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, bool findAll)
        {
            skillEntity.OnClientUserInputEffect(effectParam, blackBoard, isFrameStart, findAll);
        }

        private void HandleServerInputEffect(SkillEntity skillEntity, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            // 先更新 服务器 同步的用户输入
            UpdateServerInputCache(skillEntity.RuntimeID, effectParam, blackBoardNode);

            bool isActive = activeSkillEntity == skillEntity;
            bool result = skillEntity.OnServerInputEffect(effectParam, blackBoardNode, baseBlackBoard, isActive);

            // 如果 服务器线 执行成功了 服务器黑板中的 用户输入, 那直接清空 这个缓存
            if (result)
            {
                ClearServerInputCache();
            }
        }

        private void HandleClientChangeInputEffect(SkillEntity skillEntity, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, bool findAll)
        {
            skillEntity.OnClientChangeInput(effectParam, blackBoard, isFrameStart, findAll);
        }

        private void HandleServerChargeInputEffect(SkillEntity skillEntity, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            skillEntity.OnServerCahrgeInput(effectParam, blackBoardNode, baseBlackBoard);
        }

    }
}

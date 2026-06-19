using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SGF.Time;
using StarProject.Game.Entity;
using SkillEditor;
using StarProjectDef;
using ProtoMsg;
using SGF.Unity;
using StarProject.Service.Battle;

namespace StarProject.Game.Skill
{
    public partial class SkillEntity : EntityRemoteStatic
    {
        private void HandleStageEvent(StageEvent stageEvent, E_SkipToStageType skipToStageType)
        {
            //DB_Close       SGF.Debuger.Log($"{TagFlag} HandleStageEvent , {stageEvent}");
            switch (stageEvent)
            {
                case StageEvent.None:
                    {
                        //dont do anything
                    }
                    break;
                case StageEvent.JumpStage1:
                    {
                        /// 2023/2/28
                        /// Tick 结束时跳转
                        /// 跟gl 约定如下:
                        ///     当前阶段无论如何都会跑完,跑完后 跳转到下一阶段. 当前阶段以后的循环 都不会再跑.
                        if (CurSkillStage != null)
                        {
                            // 当前阶段 继续执行,只是 设置一个 skipLoopStage 标签,等到阶段结束时,根据这个标签
                            // 跳转 到下一个非循环阶段
                            CurSkillStage.SetSkipLoopStage();
                        }
                    }
                    break;
                case StageEvent.JumpStage2:
                    {
                        /// 立即跳转
                        /// TODO : DL
                        /// 缺少一个立即跳转到对应阶段的配置
                        /// note:
                        ///    夏哥的意思,目前服务器是在此处只发送一个打断信号,
                        ///    阶段走正常的打断后的逻辑.当进入阶段且没有 活跃技能时,
                        ///    执行此处的跳过阶段逻辑,此时跳过本阶段,进入下一阶段,
                        ///    从而达到策划要求的 跳过 非活跃的等待阶段需求.
                        /// 
                        /// note2:
                        /// 但是此时很明显的存在几个问题:
                        ///    1.逻辑不清晰. 正常的逻辑,其实就是收到输入轴信号号,
                        ///      打断当前阶段,进入策划指定的阶段即可.但上述方式,绕了一大圈;
                        ///    2.跟主位技能抢占逻辑混杂在一起,导致逻辑更加混乱.
                        ///      阶段输入轴的 阶段逻辑 优先级大于阶段自己的阶段结束逻辑.
                        ///      此时只需要 处理阶段退出时，到底是走 输入轴的逻辑还是走阶段自己的阶段退出逻辑.
                        ///      而对于阶段的抢位,由后续阶段自己处理。
                        ///      总结: 就是阶段输入轴只关系策划配置的阶段逻辑,而 活跃阶段的抢占,
                        ///            在新的阶段进入的时候,自己抢占处理。
                        ///    3.输入轴 是预输入还是 立即输入的问题.
                        ///      对于立即输入:
                        ///          其实就是立即处理轴的逻辑,首先比较优先级，优先级满足了,就立即打断当前阶段,
                        ///          执行跳段逻辑;
                        ///          优先级如果不满足,则记录下cache,等阶段结束时,处理cache的缓存逻辑(当预输入技能处理);
                        ///     对于预输入:
                        ///         预输入 需要考虑当前活跃技能是否存在 以及是否时自身.
                        ///         如果 不存在活跃技能, 那就直接执行当前的输入轴逻辑,打断当前的阶段,并跳转到对应的阶段.
                        ///         如果存在活跃技能,且不是自己的时候,比较优先级,如果 满足优先级了,
                        ///         那就立即执行逻辑.
                        ///         如果优先级不足,则缓存当前操作,等到 技能从 活跃-->非活跃时,激活缓存的预输入。
                        /// 
                        ///  总结: 
                        ///     1.预输入 和 立即输入 + 活跃技能,共同决定了 输入轴的效果是立即执行还是 缓存下来等到活跃--->非活跃时执行;
                        ///     2.缓存 下来的执行流程 与 夏哥说的一致,在阶段进入时执行;
                        ///     3.输入轴不管是立即输入还是预输入,执行的时候,都是立即处理配置的阶段逻辑,
                        ///       所以此处客户端是在 阶段被打断是,立即处理输入轴的效果. 
                        ///       此处与夏哥的不一样,夏哥的是此处依旧走2.
                        ///       但我认为此处应该是在阶段结束时立即处理的逻辑,需要与夏哥讨论一下.

                        // 目前相当于时写死的 跳转到当前阶段的后2个阶段,后续要改为策划配置
                        // int skipStageID = GetCurOffsetStageID(2, false);


                        // ExecuteSkipToStage(skipStageID, E_BlackBoardTag.Client, skipToStageType);

                        /// 2023/2/28
                        /// gl 确定 立即跳转 功能暂时不做(目前没这种 技能需求)
                        /// 立即跳转:
                        ///     直接开始跑 下个非循环阶段, 当前阶段继续跑完. 如果这个阶段是 循环阶段, 后续的循环阶段跳过不执行
                        /// note:
                        ///     相当于 同时开了 2个 正在运行的 阶段. 目前客户端 也可以 实现这种需求, 但是 这种情况下,由于服务器
                        ///     同步的 数据 缺失,无法 做 恢复.
#if (UNITY_EDITOR && BATTLE_DEBUG)
                        LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} HandleStageEvent  JumpStage2 error!!!");
#endif
#if UNITY_EDITOR
                        //UnityEngine.Debug.Break();
#endif
                    }
                    break;
                case StageEvent.KillSkill:
                    {
                        // 结束技能
                        OnExit(E_SkillExitType.ClientBreakSkill);
                    }
                    break;
                case StageEvent.KillStage:
                    {
                        GetNextStageIDStr(false, out int stageID, out int loopIdx);
                        SetSkipToStage(stageID, loopIdx, E_BlackBoardTag.Client, E_SkipToStageType.OnCurStageExit);

                        // 结束阶段, 如果是循环阶段,会 结束 整个循环阶段
                        OnBreakStage(E_SkillStageExitType.Broken);
                    }
                    break;

                default:
                    {
                        //dont do anything
                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} HandleStageEvent stageEvent: {stageEvent} no handle, error!!!");
                    }
                    break;
            }
        }

        public void OnBreakCurRuntimeInBullet(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, bool findAll)
        {
            SkipToNextStage(0, E_SkillStageEnterType.Default);
        }

        /// <summary>
        /// 技能实体自己处理 输入效果的逻辑
        /// note1 :
        ///     1.用户输入的效果, 客户端 需要做的,应该只有 在本地 设置 KEY_USER_INPUT,
        ///       用来标识 本地 已经进入 了 用户输入轴效果.
        ///     2.对于服务器 用户效果 输出的 outputkey,服务器发送过来的数据,客户端需要
        ///       判定这个数据服务器是否有即可.
        /// note2 :
        ///     跟服务器约定输入轴的同步数据如下:
        ///     1.TRUE 表示正常结束(点击了)
        ///     2.FALSE 表示超时结束
        /// note3:
        ///     1.服务器的预输入 约定: A--->B--->C(输入轴横杠 ABC3个阶段)
        ///       服务器预输入的处理逻辑在阶段进入时处理.如A阶段为活跃阶段,接收到A的点击,
        ///       进入到B阶段的时候,判定是否有预输入,如果有,进入C.
        ///     2.预输入触发在活跃阶段到非活跃阶段的时候,如A,B都是活跃阶段,C,D为非活跃阶段.如果A处响应点击,A--->B,
        ///       发现B为主位技能且有预输入记录,此时不执行预输入逻辑.B--->C,进入非活跃阶段,此时触发预输入,进入D.
        ///     3.预输入的检测逻辑,首先判定 当前是否有 活跃技能.如果没有,立即执行这个预输入.
        ///       如果 有活跃技能,当由活跃----> 非活跃时,执行预输入逻辑.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="isFrameStart"></param>
        /// <param name="findAll"></param>
        public void OnClientUserInputEffect(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, bool findAll)
        {
            // 用户输入效果,如果服务器数据 到了,只能说明 服务器执行了 用户输入这个效果(也就是做个标记,同时在黑板中写入一份数据)
            // 而对于客户端来说,客户端需要做一些额外的本地 黑板标记key,用来标识这个效果已经执行了
            // note:
            //  自定义的用户输入KEY 肯定存在于技能黑板中,而并不存在于策划配置的 黑板中
            string userInputKey = BaseBlackBoard.KEY_USER_INPUT;

            string outputKey = effectParam.OutputKey;
            // 只有主角 才有 输入轴的 完整结束逻辑, 其他人的话 应该走 服务器同步 阶段
            if (!isFrameStart && playerData.isMainPlayer)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [client] [input] OnClientUserInputEffect , end : {outputKey}");

                // 如果是帧尾,其实就要执行效果结束 都没等待到 输入的逻辑
                // 处理完帧尾逻辑,结束
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 客户端关闭用户输入效果");
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] OnClientUserInputEffect 尾帧结束预输入, effectID: {effectParam.EffectID}");

                /// fixed issue:
                ///     在普攻连击的过程中,如果按钮点击落在了 非活跃阶段,并且此处 离预输入结束 很短.
                ///     此时客户端为了防止 客户端提前预播,但是 网络延迟服务器收到时 输入轴已经结束的情况,
                ///     所以此时 客户端不会预播,而是只给服务器发送 预输入操作。等到 服务器返回回来后,再去触发 预输入操作。
                /// 但:
                ///     会出现一个问题， 服务器收到了这个效果并且触发。但是客户端已经提前触发了 输入轴的 结束效果.
                ///     此时 服务器播放第二段，但是客户端技能已经 被输入轴效果 kill.
                /// 
                /// 基于上, 合理的处理方式是:
                ///     1.客户端发送 预输入操作后, 标记 已经发送的状态;
                ///     2.在客户端触发这个效果的时候, 检查是否是否已经发送;
                ///     3.如果收到了服务器的效果，那直接触发;
                ///     4.如果没收到，但是客户端已经发送预输入, 客户端只是
                ///        标记这个输入轴的状态为 客户端已经发送;
                ///     5.等到服务器输入轴效果 返回成功/超时 再去触发对应的输入轴逻辑;
                /// 
                /// 先用临时的方式,给客户端的输入轴结束效果加个 延时.
                {
                    bool isServerContainEffect = skillBlackBoard.Contain(outputKey, E_BlackBoardTag.Server);
                    if (!isServerContainEffect)
                    {
                        DelayInvoker.DelayInvoke($"userInput_[{RuntimeID}]", 0.1f, (object[] args) =>
                        {
                            ExecuteUserInput(false, E_BlackBoardNodeState.ClientClosed);
                        });
                        return;
                    }
                }

                ExecuteUserInput(false, E_BlackBoardNodeState.ClientClosed);
                return;
            }

            StartUserInput(effectParam, blackBoard);
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] OnClientUserInputEffect 客户端 开启 用户输入轴, effectID: {effectParam.EffectID}");

            TriggerServerInputOnInputStart(effectParam, blackBoard);
        }

        /// <summary>
        /// 开启用户输入 轴
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        private void StartUserInput(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            string userInputKey = BaseBlackBoard.KEY_USER_INPUT;
            string outputKey = effectParam.OutputKey;


            CustomBlackBoardNode effectBlackBoardNode = InitEffectBoardNode(userInputKey, effectParam);

            // 帧首逻辑,所以直接 设置用户输入黑板,标识用户输入效果开启
            skillBlackBoard.Set(userInputKey, effectBlackBoardNode, E_BlackBoardTag.Client);

            // 客户端执行效果存在对应outPutKey对应的客户端黑板中
            skillBlackBoard.Set(outputKey, effectBlackBoardNode, E_BlackBoardTag.Client);

            RecordUserInputStart(effectParam);

        }

        /// <summary>
        /// 提前检查 服务器 黑板数据是否可以触发 用户输入轴, 如果是存在 服务器数据,就需要检查是否是预输入, 
        /// 如果是预输入, 则等到能够触发的时候触发
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        private void TriggerServerInputOnInputStart(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            string outputKey = effectParam.OutputKey;
            // 客户端执行 输入轴效果,首先判断服务器的预输入效果是否已经提前到
            bool isContainSserverInputEffect = skillBlackBoard.Contain(outputKey, E_BlackBoardTag.Server);
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [client] [input] OnClientUserInputEffect,isContainSserverInputEffect: {isContainSserverInputEffect},IsPrepare: {effectParam.IsPrepare} start : {outputKey}");

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 客户端开启用户输入效果, 是否收到服务器用户输入效果: {isContainSserverInputEffect}, IsPrepare: {effectParam.IsPrepare} start : {outputKey}");
            // 如果未提前到: 那啥都不干
            if (!isContainSserverInputEffect)
            {
                // dont do anything
            }
            else
            {
                // 如果提前到了: 检查是否时预输入,如果是预输入,啥都不干;
                if (effectParam.IsPrepare)
                {
                    // dont do anything
                    //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 此时是 用户预输入效果, 客户端提前收到服务器数据,但是 不立即执行");
                }
                else
                {
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] OnClientUserInputEffect 客户端提前执行用户预输入, effectID: {effectParam.EffectID}");

                    //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 此时是 用户立即输入效果, 客户端提前收到服务器数据,立即 按 ServerUsed 类型 执行");
                    //如果不是预输入,立即执行服务器的预输入效果;
                    ExecuteUserInput(true, E_BlackBoardNodeState.ServerUsed);
                }
            }
        }

        /// <summary>
        /// 服务器 用户输入的效果线.
        /// 收到服务器用户输入的效果线,True表示正常结束,False表示超时结束.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="baseBlackBoard"></param>
        public bool OnServerInputEffect(I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard, bool isActive)
        {

            bool inputResult = blackBoardNode.OutputResult;

            string outputKey = effectParam.OutputKey;

            bool isUserInputEffect = CheckIsUserInputEffect(effectParam);

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [server] [Input] OnServerInputEffect  outputKey {outputKey} , effectID {effectParam.EffectID} , isUserInputEffect {isUserInputEffect} , inputResult {inputResult} ");

            // 如果不是 当前用户输入轴的效果,那什么都不干
            if (!isUserInputEffect)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [server] [Input] isUserInputEffect : {isUserInputEffect} , do nothing!! ");
                return false;
            }

            // 如果是预输入,收到服务器的效果数据后,先看本地是否非活跃,只有非活跃才能播放服务器的效果. 这个效果需要由 活跃 ----> 非活跃 触发
            if (effectParam.IsPrepare && isActive)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [server] [Input]  IsPrepare : {true} && isActive : {isActive} , do nothing!!");
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 处理服务器预输入效果  outputKey {outputKey} , effectID {effectParam.EffectID} , isUserInputEffect {isUserInputEffect} , inputResult {inputResult} , 但是当前输入轴技能 活跃,等非活跃 后触发");
                return false;
            }

            // 如果是 True,表示效果正常结束(按钮触发了)
            // 如果是False,表示超时结束

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [server] 准备执行 服务器的 用户输入  outputKey {outputKey} , effectID {effectParam.EffectID} , isUserInputEffect {isUserInputEffect} , inputResult {inputResult} ");
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] OnServerInputEffect 服务器执行用户预输入, effectID: {effectParam.EffectID}");

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [server] [Input] OnServerInputEffect : TimeLogic {effectParam.EffectArgs.UserInput.OnTimeLogic} ");
            /// 1.收到服务器输入轴效果数据后,标识服务器的输入轴效果已经结束;
            /// 2.判断客户端本地输入轴效果是否结束,如果结束,直接return; 
            /// 3.判断输入轴是否是 立即输入, 如果是立即输入,立即执行输入轴逻辑;
            /// 4.如果是预输入, 缓存服务器数据,同时标记输入轴效果的状态为服务器使用,等待由活跃--->非活跃,执行用户输入轴逻辑
            /// 5.删除预输入数据
            /// 6。客户端预输入 和服务器返回的预输入,应该可以走相同的接口逻辑
            ExecuteUserInput(inputResult, E_BlackBoardNodeState.ServerUsed);

            return true;
        }

        /// <summary>
        /// 效果是否是缓存中的的用户输入轴, 后面看看是否还必要
        /// </summary>
        /// <param name="effectParam"></param>
        /// <returns></returns>
        private bool CheckIsUserInputEffect(I_EffectParam effectParam)
        {
            string key = BaseBlackBoard.KEY_USER_INPUT;
            CustomBlackBoardNode effectBlackBoardNode = skillBlackBoard.GetKey<CustomBlackBoardNode>(key, false);
            if (effectBlackBoardNode == null)
            {
                return false;
            }
            I_EffectParam cacheEffectParam = (I_EffectParam)effectBlackBoardNode.Value;
            if (cacheEffectParam.EffectID != effectParam.EffectID)
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// 检查 当前是否开起用户输入轴效果
        /// </summary>
        public bool CheckIsOpenUserInput()
        {
            // 如果输入轴 已经被关闭
            if (!IsUserInputValid)
            {
                return false;
            }

            if (!skillBlackBoard.ContainKey(BaseBlackBoard.KEY_USER_INPUT, false))
            {
                return false;
            }

            CustomBlackBoardNode effectBlackBoardNode = skillBlackBoard.GetKey<CustomBlackBoardNode>(BaseBlackBoard.KEY_USER_INPUT, false);
            return effectBlackBoardNode.IsOpen;
        }


        /// <summary>
        /// 检查 effectID 对应 输入轴效果有没有开启
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        public bool CheckIsOpenEffectInput(int effectID)
        {
            // 先检查 这个技能是否开启 输入轴,如果输入轴都没开启,那肯定返回false
            if (!CheckIsOpenUserInput())
            {
                return false;
            }

            EffectParam effectParam = GetUserInput();
            // 然后检查 技能当前的输入轴 是否就是 effectID 对应的输入轴
            if (effectParam.EffectID != effectID)
            {
                return false;
            }

            return true;
        }

        private long userInputStartTime = 0;
        private int userInputDuration = 0;
        private int stopInputBeforeEnd = 0;




        /// <summary>
        /// 用户输入 这个效果是否是 有效的. 如果被标记为 无效, 那用户无法通过 点击按钮 触发这个用户输入
        /// 只有在 输入轴 这个效果 isOpen 并且 IsUserInputValid 有效的时候, 这个输入轴 才是可以被 输入触发的
        /// </summary>
        // public bool IsUserInputValid => !IsServerEndSkill() && cur_userInputInvalidState == E_UserInputInvalidState.None;
        public bool IsUserInputValid => cur_userInputInvalidState == E_UserInputInvalidState.None;


        /// <summary>
        /// 是否 秒放 提前静止了朝向
        /// </summary>
        private bool hasPreForbidDir = false;

        /// <summary>
        /// 当前用户输入的非法状态
        /// </summary>
        private E_UserInputInvalidState cur_userInputInvalidState = E_UserInputInvalidState.None;

        /// <summary>
        /// 设置 输入轴状态
        /// </summary>
        /// <param name="inputInvalidState">设置的非法状态</param>
        /// <param name="isSet">是设置还是取消这个状态</param>
        /// <param name="forceSet">是否是强制设置这个状态</param>
        private void SetUserInputState(E_UserInputInvalidState inputInvalidState, bool isSet, bool forceSet = false)
        {
            if (forceSet)
            {
                cur_userInputInvalidState = inputInvalidState;
            }
            else
            {
                if (isSet)
                {
                    cur_userInputInvalidState |= inputInvalidState;
                }
                else
                {
                    cur_userInputInvalidState &= ~inputInvalidState;
                }
            }
            OnInputStageChange();
        }

        private void OnInputStageChange()
        {
            if (!playerData.isMainPlayer)
            {
                return;
            }

            // 蓄力不需要通知, 理论上是需要通知的
            if (Flag_StartEnergy)
            {
                return;
            }

            EffectParam userInput = GetUserInput();

            // 说明数据轴 已经关闭
            if (userInput == null)
            {
                // 主角的话, 输入轴状态变化需要通知到 表现层
                skillContainer.OnUserInputSteteChange(this, 0, false, 0, 0);
                return;
            }

            int totalTime = userInputDuration - stopInputBeforeEnd;
            int curTime = (int)(TimeUtils.ClientNowStampMilli - userInputStartTime);

            // 输入轴还存在, 那就通知 当前的状态和 对应的时间
            skillContainer.OnUserInputSteteChange(this, userInput.EffectID, IsUserInputValid, totalTime, curTime);
        }

        /// <summary>
        /// 标记 输入轴效果 为 invalid. 此时 用户无法通过 按钮点击触发
        /// note:
        ///     只有在 输入轴 这个效果 isOpen 并且 有效的时候, 这个输入轴 才是可以被 输入触发的
        /// </summary>
        public void MarkUserInputInvalid(bool isValid)
        {
            if (!GetUserInputCanMarkInvalid())
            {
                return;
            }

            SetUserInputState(E_UserInputInvalidState.MarkInvalid, !isValid);

            // SGF.Debuger.LogError($" skill 技能预输入 mark Invalid ---> {isValid} , state: {cur_userInputInvalidState}");
        }

        /// <summary>
        /// 标记 输入效果关闭, 这个关闭只是因为同一个技能槽其它技能的开启而关闭.
        /// 高磊需求:
        /// noet:
        ///     1.输入轴关闭后, 输入轴自己的 阶段结束逻辑不受影响;
        ///     2.输入轴关闭后, 这个输入轴 不可以再次输入
        /// </summary>
        public void MarkUserInputClose()
        {
            SetUserInputState(E_UserInputInvalidState.MarkClose, true);
        }

        /// <summary>
        /// 检查 用户输入效果 是否可以被 标记为 invalid.
        /// </summary>
        /// <returns></returns>
        public bool GetUserInputCanMarkInvalid()
        {
            EffectParam userInputEffectParam = GetUserInput();
            if (userInputEffectParam == null)
            {
                return false;
            }

            return userInputEffectParam.CanMarkInvalid;
        }

        /// <summary>
        /// 预输入 效果 有没有被提前 执行。 对于蓄力 这种效果，有个秒放 的逻辑,在秒放后, 这个输入效果 不可再次被按钮触发
        /// </summary>
        public void MarkUserInputExecuted()
        {
            SetUserInputState(E_UserInputInvalidState.MarkPreExecute, true);
            // SGF.Debuger.LogError($"[CheckCanUseSkill] ---> hasPreExecuted : true");
        }

        private void RecordUserInputStart(I_EffectParam effectParam)
        {
            userInputStartTime = TimeUtils.ClientNowStampMilli;
            userInputDuration = effectParam.EffectEndTime;
            stopInputBeforeEnd = (effectParam.BaseEffect as EffectTypeUserInput).StopInputBeforeEnd;



            if (playerData.isMainPlayer)
            {
                skillController.TriggerUserInputStart(this, effectParam);
            }

            SetUserInputState(E_UserInputInvalidState.None, true, true);

        }


        /// <summary>
        /// 检查 是否是 网络时间安全范围内的 用户输入.
        /// note:
        ///     (用户输入效果在 快结束的一小段时间,为不安全的网络范围时间,此时,需要频闭 用户输入)
        /// </summary>
        public bool CheckIsInNetSafeAreaUserInput()
        {
            // 如果用户输入 未开启,那就 不在用户输入的网络安全范围内
            // note:
            //      (用户输入效果在 快结束的一小段时间,为不安全的网络范围时间,此时,需要频闭 用户输入)
            EffectParam effectParam = GetUserInput();
            if (effectParam == null)
            {
                return false;
            }

            long cost = TimeUtils.ClientNowStampMilli - userInputStartTime;
            long leastTime = userInputDuration - cost;

            if (effectParam.SkillEffectType != E_SkillEffect.UserInput)
            {
                return true;
            }

            if (stopInputBeforeEnd > 0)
            {
                return leastTime > stopInputBeforeEnd;
            }

            // 如果没有配置 输入轴的 提前stop 输入轴 时间, 那就用 默认的 网络延迟安全时间范围
            bool result = false;
            result = !SkillEntity.CheckInForbidenUseSkillTime(leastTime);

            return result;
        }

        /// <summary>
        /// 检查输入轴是否是 预输入
        /// </summary>
        public bool CheckIsPrepareUserInput()
        {
            CustomBlackBoardNode effectBlackBoardNode = skillBlackBoard.GetKey<CustomBlackBoardNode>(BaseBlackBoard.KEY_USER_INPUT, false);

            I_EffectParam i_EffectParam = (I_EffectParam)effectBlackBoardNode.Value;
            if (i_EffectParam == null)
            {
                return false;
            }
            return i_EffectParam.IsPrepare;
        }

        /// <summary>
        /// 触发用户输入
        /// </summary>
        /// <param name="inputSkillUseReq"></param>
        /// <param name="resultResult"></param>
        /// <param name="e_BlackBoardNodeState"></param>
        /// <returns></returns>
        public E_UseSkillResult OnTriggerUserInput(SkillUseReq inputSkillUseReq, bool resultResult, E_BlackBoardNodeState e_BlackBoardNodeState, float arg = -999)
        {
            if (playerData.IsForbidAttack)
            {

                return E_UseSkillResult.ForbidAttack;
            }
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] OnTriggerUserInput 触发用户输入, runtimeID: {inputSkillUseReq.RuntimeID} ");

            E_UseSkillResult result = ExecuteUserInput(resultResult, e_BlackBoardNodeState, inputSkillUseReq, arg);
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 触发 用户输入: skillID: {inputSkillUseReq.SkillID} , result {result}");

            // 如果失败 表示 优先级 不够
            return result;
        }

        public E_UseSkillResult CheckCanPreTriggerUserInput(SkillUseReq inputSkillUseReq, E_BlackBoardNodeState e_BlackBoardNodeState)
        {
            if (playerData.IsForbidAttack)
            {

                return E_UseSkillResult.ForbidAttack;
            }

            string key = BaseBlackBoard.KEY_USER_INPUT;
            //取用户输入轴数据
            CustomBlackBoardNode userInput = (CustomBlackBoardNode)skillBlackBoard.Get(key, E_BlackBoardTag.Client);

            if (userInput == null)
            {
                return E_UseSkillResult.User_Input_Not_Open;
            }

            if (!userInput.IsOpen)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [Input] ExecuteUserInput IsOpen : False , state : {userInputBlackBoardNode.state}");
                return E_UseSkillResult.User_Input_Not_Open;
            }

            EffectParam clientEffectParam = (EffectParam)userInput.Value;
            string outputKey = clientEffectParam.OutputKey;
            EffectParam effectParam = clientEffectParam;

            // 执行输入轴效果的时候,首先判断服务器的输入轴效果有没有到
            bool isServerContainEffect = skillBlackBoard.Contain(outputKey, E_BlackBoardTag.Server);

            // 如果本地包含服务器 的黑板数据,那就直接返回成功            
            if (isServerContainEffect)
            {
                return E_UseSkillResult.Succeed;
            }

            // 如果没有服务器数据, 那就检查 是否是 立即输入, 如通过是立即输入,现在预输入 暂时还是客户端直接预播,不等服务器
            if (!clientEffectParam.IsPrepare)
            {
                EffectTypeChargeInput chargeInput = effectParam.BaseEffect as EffectTypeChargeInput;

                /// 2023/2/23
                /// gl 增加 蓄力 最小时间的配置,用来实现 秒放蓄力 或者在蓄力时间小于 最小蓄力时间的时候, 能够继续蓄力知道 达到配置的最小时间.
                /// note1:
                ///     需要注意的点是在 蓄力 定时器 还未触发的时候, 如果 技能被 另外一个活跃技能打断,此时需要取消这个定时器
                /// note2:
                ///     如果存在 服务器的 输入轴效果数据, 那就要优先使用 服务器的 结果

                //SGF.Debuger.Log($"[energy] 检查是否可以触发的 立即输入 isServerContainEffect: {isServerContainEffect}, e_BlackBoardNodeState: {e_BlackBoardNodeState}, energyTotalTime: {energyTotalTime}, MinTime: {chargeInput.MinTime}");
                if (!isServerContainEffect && e_BlackBoardNodeState == E_BlackBoardNodeState.ClientUsed && energyTotalTime < chargeInput.MinTime)
                {
                    int leastTime = chargeInput.MinTime - (int)energyTotalTime;
                    return E_UseSkillResult.DealyInvokeSkill;
                }

                return E_UseSkillResult.Succeed;
            }

            // 如果是 预输入, 目前 跟服务器的约定 是 每次的预输入操作,
            // 服务器都 会返回结果, 那客户端 能否预播 这个预输入操作, 需要 根据 服务器是否返回过 一次 成功.
            // 只要成功过一次, 那客户端就 认为 这个 预输入轴 可以提前触发
            bool canTriggle = skillController.CheckCanTrigglePreUserInput(RuntimeID, clientEffectParam);
            if (!canTriggle)
            {
                return E_UseSkillResult.Server_Not_Ret_Input_Result;
            }

            return E_UseSkillResult.Succeed;
        }

        /// <summary>
        /// 执行 输入轴效果的输入效果
        /// note:
        ///     用户输入目前两种:
        ///     1.普通的用户输入轴的用户输入;
        ///     2.蓄力 服务器也认为走 的是用户输入;
        /// 
        /// 所以此处用户输入需要处理 用户输入轴 和蓄力 两种情况
        /// </summary>
        /// <param name="result">输入轴效果的结果,超时: false,正常点击: True</param>
        /// <param name="effectBlackNodeState"> 黑板节点的状态 </param>
        /// <param name="inputSkillUseReq">执行 用户输入轴 时 使用的 输入操作请求,对于客户端主动触发的,需要发送给服务器 </param>
        /// <returns></returns>
        public E_UseSkillResult ExecuteUserInput(bool result, E_BlackBoardNodeState effectBlackNodeState, SkillUseReq inputSkillUseReq = null, float arg = -999)
        {
            // 执行输入轴效果的时候先 取消之前 预输入轴的 延迟用户输入
            DelayInvoker.CancelInvoke($"userInput_[{RuntimeID}]");

            // 客户端延迟执行 输入轴的效果帧后,  ExecuteUserInput 的时候可能技能 已经被干掉, 所以此时需要处理 没有的情况
            if (skillBlackBoard == null)
            {
                return E_UseSkillResult.No_Skill_Effect;
            }
            string key = BaseBlackBoard.KEY_USER_INPUT;
            //取用户输入轴数据
            CustomBlackBoardNode userInput = (CustomBlackBoardNode)skillBlackBoard.Get(key, E_BlackBoardTag.Client);

            if (userInput == null)
            {
                return E_UseSkillResult.User_Input_Not_Open;
            }

            if (!userInput.IsOpen)
            {
                SGF.Debuger.LogError($"{TagFlag} [Input] 客户端执行 输入效果 已经关闭, state: {userInput.state} ");
                return E_UseSkillResult.User_Input_Not_Open;
            }

            EffectParam clientEffectParam = (EffectParam)userInput.Value;
            string outputKey = clientEffectParam.OutputKey;
            EffectParam effectParam = clientEffectParam;

            // 执行输入轴效果的时候,首先判断服务器的输入轴效果有没有到
            bool isServerContainEffect = skillBlackBoard.Contain(outputKey, E_BlackBoardTag.Server);

            bool finalResult = result;

            //  如有 服务器已经存在了 这个效果的结果值, 那么就要优先使用服务器的 值
            {
                // 如果服务器效果还未到,那就是执行客户端的效果轴逻辑
                // 如果服务器效果已经提前到了,那执行的时候,采用的是服务器的效果
                if (isServerContainEffect)
                {
                    // executeEffectKey = "[Server]";
                    effectBlackNodeState = E_BlackBoardNodeState.ServerUsed;
                    CustomBlackBoardNode serverEffectBlackBoardNode = (CustomBlackBoardNode)skillBlackBoard.Get(outputKey, E_BlackBoardTag.Server);

                    // 使用服务器的 result 来执行效果
                    finalResult = serverEffectBlackBoardNode.OutputResult;
                }

                // 更新客户端效果的执行结果
                userInput.OutputResult = finalResult;
            }
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 准备执行 用户输入效果 IsOpen : True ,result: {result}, result: {finalResult} , isServerContainEffect: {isServerContainEffect}");


            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} {executeEffectKey} [Input]  ExecuteUserInput isServerContainEffect: {isServerContainEffect}, outPutKey: {effectParam.OutputKey},effectID: {clientEffectParam.EffectID} , result : {result} , finalResult : {finalResult}  ");
            E_UseSkillResult executeResult = E_UseSkillResult.Succeed;
            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.UserInput:
                    {
                        executeResult = ExecuteSkillUserInput(effectParam, finalResult, effectBlackNodeState, isServerContainEffect);
                    }
                    break;
                case E_SkillEffect.Energy:
                    {
                        executeResult = ExecuteEnergyUserInput(effectParam, finalResult, effectBlackNodeState, inputSkillUseReq, arg);
                    }
                    break;
            }

            return executeResult;
        }

        public E_UseSkillResult ExecuteServerUserInput(ServerInputCache serverInputCache, E_BlackBoardNodeState effectBlackNodeState, SkillUseReq inputSkillUseReq = null)
        {
            string key = BaseBlackBoard.KEY_USER_INPUT;
            //取用户输入轴数据
            CustomBlackBoardNode userInput = (CustomBlackBoardNode)skillBlackBoard.Get(key, E_BlackBoardTag.Client);


            if (userInput == null)
            {
                return E_UseSkillResult.User_Input_Not_Open;
            }

            if (!userInput.IsOpen)
            {
                //SGF.Debuger.LogError($"{TagFlag} [Input] 服务端端执行 输入效果 已经关闭, state: {userInput.state} ");

                return E_UseSkillResult.User_Input_Not_Open;
            }

            EffectParam clientEffectParam = (EffectParam)userInput.Value;
            string outputKey = clientEffectParam.OutputKey;

            // 服务器同步过来的 用户输入 结果不再这个轴上
            if (outputKey != serverInputCache.EffectParam.OutputKey)
            {
                return E_UseSkillResult.User_Input_Not_Open;
            }
            EffectParam effectParam = clientEffectParam;



            bool result = serverInputCache.IsExcute;

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 准备执行 服务器触发的 用户输入效果 outputKey: {outputKey}, result: {result}");


            E_UseSkillResult executeResult = E_UseSkillResult.Succeed;
            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.UserInput:
                    {
                        executeResult = ExecuteSkillUserInput(effectParam, result, effectBlackNodeState, true);
                    }
                    break;
                case E_SkillEffect.Energy:
                    {
                        executeResult = ExecuteEnergyUserInput(effectParam, result, effectBlackNodeState, inputSkillUseReq);
                    }
                    break;
            }

            return executeResult;
        }


        private void DelayInvokeExecuteUserInput(object[] args)
        {
            if (hasPreForbidDir)
            {
                playerData.HandleClientBattleStates(new List<E_BattleStateType>() { E_BattleStateType.BattleState_ForbidDir }, false);
            }

            bool result = (bool)args[0];
            E_BlackBoardNodeState effectBlackNodeState = (E_BlackBoardNodeState)args[1];
            SkillUseReq inputSkillUseReq = (SkillUseReq)args[2];
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] DelayInvokeExecuteUserInput 延迟 一段事件后, 开始触发用户输入, runtimeID: {inputSkillUseReq.RuntimeID} ,skillID: {inputSkillUseReq.SkillID}");

            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"[energy]  DelayInvokeExecuteUserInput");

            EffectParam userInput = GetUserInput();
            if (userInput == null)
            {
                return;
            }
            SkillMsgUtils.SendPreSkillUseInput(RuntimeID, inputSkillUseReq, userInput.EffectID);

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 延迟 一段事件后, 开始触发用户输入: skillID: {inputSkillUseReq.SkillID}");

            ExecuteUserInput(result, effectBlackNodeState, inputSkillUseReq);
        }

        private void CancelInvokeExecuteUserInput()
        {
            if (hasPreForbidDir)
            {
                playerData.HandleClientBattleStates(new List<E_BattleStateType>() { E_BattleStateType.BattleState_ForbidDir }, false);
            }

            DelayInvoker.CancelInvoke($"energy_[{RuntimeID}]");
        }


        /// <summary>
        /// 执行 用户 输入轴的 效果
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="effectResult"></param>
        /// <param name="effectBlackNodeState"></param>
        /// <param name="isServer"> 是否是 执行 服务器黑板的 预输入效果, 如果是 客户端提前 执行了 服务器效果,会关闭 这个预输入效果, 从而 后面服务器线不会再去执行 这个 预输入效果  </param>
        private E_UseSkillResult ExecuteSkillUserInput(EffectParam effectParam, bool effectResult, E_BlackBoardNodeState effectBlackNodeState, bool isServer)
        {
            EffectTypeUserInput userInput = effectParam.BaseEffect as EffectTypeUserInput;
            // 如果result       为true,表示是点击按钮执行的效果;
            // 如果             为false,表示超时的效果
            StageEvent stageEvent = effectResult ? userInput.OnTimeLogic : userInput.OutTimeLogic;
            E_SkipToStageType skipToStageType = E_SkipToStageType.OnNoneActiveStageEnter;

            //SGF.Debuger.LogError($"{TagFlag} [Input] 执行 输入效果-----  ");

            string key = BaseBlackBoard.KEY_USER_INPUT;
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 执行用户输入轴效果 ExecuteSkillUserInput runtimeID: {inputSkillUseReq?.RuntimeID} ,skillID: {inputSkillUseReq?.SkillID}");

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} {executeEffectKey} [Input] ExecuteUserInput  key {outputKey} , Open {effectBlackNodeState} , ready execute stageEvent {stageEvent} , end");
            // 结束 用户输入轴,执行用户输入轴对应的逻辑
            ExecuteEndUserInputEffect(effectParam, stageEvent, true, skipToStageType, effectBlackNodeState);

            {
                //SGF.Debuger.LogWarning($"{TagFlag} 实体角度调试 客户端 预输入 ,inputSkillUseReq.Rot={inputSkillUseReq.Rot}");
                SyncSkillUseType syncSkillUseType = isServer ? SyncSkillUseType.ServerUserInput : SyncSkillUseType.ClientPreUserInput;

                ActionOnSyncSkillUse.Invoke(syncSkillUseType);
            }
            return E_UseSkillResult.Succeed;
        }

        /// <summary>
        /// 执行 蓄力的 用户输入逻辑
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="effectResult"></param>
        /// <param name="effectBlackNodeState"></param>
        private E_UseSkillResult ExecuteEnergyUserInput(EffectParam effectParam, bool result, E_BlackBoardNodeState effectBlackNodeState, SkillUseReq inputSkillUseReq, float arg = -999)
        {
            string outputKey = effectParam.OutputKey;

            EffectTypeChargeInput chargeInput = effectParam.BaseEffect as EffectTypeChargeInput;
            // 蓄力的输入轴 超时和点击目前是一样的逻辑,不做区分
            StageEvent stageEvent = chargeInput.OnTimeLogic;

            E_SkipToStageType skipToStageType = E_SkipToStageType.OnCurStageExit;

            // 执行输入轴效果的时候,首先判断服务器的输入轴效果有没有到
            bool isServerContainEffect = skillBlackBoard.Contain(outputKey, E_BlackBoardTag.Server);


            /// 2023/2/23
            /// gl 增加 蓄力 最小时间的配置,用来实现 秒放蓄力 或者在蓄力时间小于 最小蓄力时间的时候, 能够继续蓄力知道 达到配置的最小时间.
            /// note1:
            ///     需要注意的点是在 蓄力 定时器 还未触发的时候, 如果 技能被 另外一个活跃技能打断,此时需要取消这个定时器
            /// note2:
            ///     如果存在 服务器的 输入轴效果数据, 那就要优先使用 服务器的 结果
            {
                // 如果 客户端秒放蓄力, 同时蓄力时间 要小于策划配置的 最小蓄力时间,此时,需要 继续蓄力直到 达到蓄力最小时间为止
                if (!isServerContainEffect && effectBlackNodeState == E_BlackBoardNodeState.ClientUsed && energyTotalTime < chargeInput.MinTime)
                {
                    var chargeInputMinTime = chargeInput.MinTime;

                    // 如果是自动战斗状态下， 蓄力时间改为 最大蓄力时间
                    if (BattleManager.Instance.IsAutoBattling && arg == 2)
                    {
                        if (chargeInput.SegmentationTime.Count > 0)
                        {
                            chargeInputMinTime = chargeInput.SegmentationTime[chargeInput.SegmentationTime.Count - 1];
                        }
                        else
                        {
                            chargeInputMinTime = chargeInput.MaxTime;
                        }

                    }

                    int leastTime = chargeInputMinTime - (int)energyTotalTime;

                    // fix leastTime 小于 1帧时, 无限循环触发的bug
                    // 此处 如果 leastTime 小于1帧,那进入下一帧的时候，就会立即触发此处的 DelayInvokeExecuteUserInput。 
                    // 但是 此时 DelayInvoker 的执行顺序 在 本地的 EnterFrame 之前, 导致 energyTotalTime 永远都没变，从而进入死循环
                    if (leastTime <= TimeUtils.FixedDeltaTime)
                    {
                        leastTime += (int)TimeUtils.FixedDeltaTime;
                    }
                    MarkUserInputExecuted();

                    /// 2023/11/3
                    /// 对于蓄力技能, gl 的要求 有2点:
                    /// 1. 秒放之后, 蓄力技能 即被认为已经使用, 不能再次点击
                    /// 2. 秒放之后， 在最小蓄力时间内，左摇杆的朝向 需要被客户端禁止(gl 要求客户端自己增加 朝向的原子锁)
                    /// 3. 秒放 期间， 策划保证 位移的原子锁， 如果没锁柱， 那就是 策划配置的问题(gl 原话)
                    if (curSkillInfo.IsAutoTurnToTarget && !hasPreForbidDir)
                    {
                        hasPreForbidDir = true;
                        // SGF.Debuger.LogError($"[CheckCanUseSkill] ---> mark hasPreForbidDir : {hasPreForbidDir} , IsUserInputValid: {IsUserInputValid}");

                        playerData.HandleClientBattleStates(new List<E_BattleStateType>() { E_BattleStateType.BattleState_ForbidDir }, true);
                    }
                    // LogUtils.LogError(LogUtils.LogEnum.Skill,$"[energy] DelayInvoke leastTime: {leastTime}");
                    DelayInvoker.DelayInvoke($"energy_[{RuntimeID}]", leastTime / 1000f, DelayInvokeExecuteUserInput, new object[] { result, effectBlackNodeState, inputSkillUseReq });
                    return E_UseSkillResult.DealyInvokeSkill;
                }
            }
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 执行 蓄力 用户输入轴效果 ExecuteSkillUserInput runtimeID: {RuntimeID} ");

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} {executeEffectKey} [Input] ExecuteUserInput  key {outputKey} , Open {effectBlackNodeState} , ready execute stageEvent {stageEvent} , end");
            // 结束 用户输入轴,执行用户输入轴对应的逻辑
            ExecuteEndUserInputEffect(effectParam, stageEvent, true, skipToStageType, effectBlackNodeState);

            {
                //SGF.Debuger.LogWarning($"{TagFlag} 技能 自动转向 ExecuteUserInput effectBlackNodeState={effectBlackNodeState},,skillUseReq.Rot={skillUseReq.Rot}");
                SyncSkillUseType syncSkillUseType = isServerContainEffect ? SyncSkillUseType.ServerUserInput : SyncSkillUseType.ClientPreUserInput;

                ActionOnSyncSkillUse.Invoke(syncSkillUseType);
            }
            return E_UseSkillResult.Succeed;
        }


        /// <summary>
        /// 用户输入的效果, 这里 只判定黑板中是否有这个 输入轴， 而不判断 它是否被摇杆移动关闭.
        /// </summary>
        /// <returns></returns>
        public EffectParam GetUserInput()
        {
            string key = BaseBlackBoard.KEY_USER_INPUT;

            CustomBlackBoardNode customBlackBoardNode = skillBlackBoard.GetKey<CustomBlackBoardNode>(key, false);

            // 如果用户输入效果 已经关闭, 那就返回 null
            if (customBlackBoardNode == null || !customBlackBoardNode.IsOpen)
            {
                return null;
            }
            EffectParam effectParam = (EffectParam)customBlackBoardNode.Value;
            return effectParam;
        }

        /// <summary>
        /// 蓄力的客户端效果
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="isFrameStart"></param>
        /// <param name="findAll"></param>
        public void OnClientChangeInput(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, bool findAll)
        {
            // 对于蓄力来说,效果的结束,有两种情况
            // 1.蓄力超时结束(接收到了效果的结束帧)
            // 2.服务器 通知了 outputKey
            string key = BaseBlackBoard.KEY_USER_INPUT;

            if (!isFrameStart)
            {
                // 如果是帧尾,其实就要执行效果结束 都没等待到 输入的逻辑
                // 处理完帧尾逻辑,结束
                ExecuteEndUserInputEffect(effectParam, (effectParam.BaseEffect as EffectTypeChargeInput).OnTimeLogic, false, E_SkipToStageType.OnCurStageExit, E_BlackBoardNodeState.ClientClosed);
                return;
            }

            // 蓄力首帧 只是开启蓄力的标签,客户端和服务器到了蓄力效果执行的时候

            CustomBlackBoardNode customBlackBoardNode = InitEffectBoardNode(key, effectParam.Clone());

            skillBlackBoard.Set(key, customBlackBoardNode, E_BlackBoardTag.Client);
            StartEnergy(effectParam);
        }

        private void StartEnergy(I_EffectParam effectParam)
        {
            Flag_StartEnergy = true;
            energyTotalTime = 0;
            energyUITotalTime = 0;
            energyedCount = 0;
            EffectTypeChargeInput chargeInput = effectParam.BaseEffect as EffectTypeChargeInput;
            energyItemTime = chargeInput.LayerTime;
            energyMinCount = chargeInput.MinLayer;
            energyMaxCount = chargeInput.MaxLayer;

            if (chargeInput.SegmentationTime.Count > 0)
            {
                energyStageMaxTime = chargeInput.SegmentationTime[chargeInput.SegmentationTime.Count - 1];
            }
            else
            {
                energyStageMaxTime = chargeInput.MaxTime;
            }

            energyMaxTime = chargeInput.MaxTime;


            hasPreForbidDir = false;
            // 开启蓄力效果的时候, 也标记 这个输入轴效果状态 为 valid
            SetUserInputState(E_UserInputInvalidState.None, true, true);

            ActionOnEnergyStart?.Invoke(this);
        }

        private void OnEnergyUpdate()
        {
            if (!Flag_StartEnergy)
            {
                return;
            }
            energyTotalTime += TimeUtils.FixedDeltaTime;

            // 蓄力最大时间
            {
                if (energyTotalTime >= energyStageMaxTime)
                {
                    ActionOnEnergyFull?.Invoke(this);
                }
            }

            // 蓄力层数刷新逻辑
            {
                // 策划说,现在只计算蓄力阶段 蓄力的时间就好   【不需要加默认层数】
                int energyingCount = (int)(energyTotalTime / energyItemTime);

                //限定蓄力层数为最大层数,但不切换装
                if (energyingCount >= energyMaxCount)
                {
                    energyingCount = energyMaxCount;
                }
                else
                {
                    energyUITotalTime += TimeUtils.FixedDeltaTime;
                }

                //如果蓄力的层数大于了当前已经蓄力的层数
                if (energyingCount > energyedCount)
                {
                    energyedCount = energyingCount;
                    // SGF.Debuger.Log($"{TagFlag}  energyedCount {energyedCount}, baseEnergyCount {energyItemTime} ");
                    Fire.Utils.SafeRunAction(() =>
                    {
                        ActionOnEnergyCountChange?.Invoke(energyedCount, 0);
                    });
                }

            }
        }

        private void StopEnergy()
        {
            Flag_StartEnergy = false;
            energyTotalTime = 0;
            ActionOnEnergyEnd?.Invoke(this);

            //TODO : 曲
            //蓄力结束 通知 ui 层
        }


        public void OnServerCahrgeInput(I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            // 如果是 True,表示效果正常结束(按钮触发了)
            // 如果是False,表示超时结束
            bool inputResult = blackBoardNode.OutputResult;

            EffectTypeChargeInput chargeInput = effectParam.BaseEffect as EffectTypeChargeInput;
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 服务器 执行用户输入轴效果 ExecuteSkillUserInput runtimeID: {RuntimeID} ");

            ExecuteEndUserInputEffect(effectParam, chargeInput.OnTimeLogic, inputResult, E_SkipToStageType.OnCurStageExit, E_BlackBoardNodeState.ServerClosed);
        }


        /// <summary>
        /// 执行 用户输入结束的 统一的效果接口
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="stageEvent"></param>
        /// <param name="nextValue"></param>
        /// <param name="skipToStageType"></param>
        /// <param name="state"></param>
        private void ExecuteEndUserInputEffect(I_EffectParam effectParam, StageEvent stageEvent, bool nextValue, E_SkipToStageType skipToStageType, E_BlackBoardNodeState state)
        {
            string key = BaseBlackBoard.KEY_USER_INPUT;

            ExecuteEndEffect(key, effectParam, stageEvent, nextValue, skipToStageType, state);

            // 执行蓄力结束效果后，通知 技能槽 结束
            skillContainer.OnUserInputSteteChange(this, effectParam.EffectID, false, 0, 0);

        }

    }
}

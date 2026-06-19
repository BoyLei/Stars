using System;
using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Time;
using SGF.Unity;
using SkillEditor;
using StarProject;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;
using UVec3 = UnityEngine.Vector3;

namespace EditorModeTest
{
    public abstract class LocalServerRuntime
    {
        public ulong UID;
        public ulong RuntimeID;
        public ulong OwnerEntityID;

        public ulong BuilderID;

        /// <summary>
        /// 运行时的所属类型
        /// </summary>
        public RuntimeEnumType runtimeEnumType;

        public RuntimeEnumType RuntimeEnumType => runtimeEnumType;

        public long CreateTime;

        // 黑板
        public BaseBlackBoard blackBoard;

        /// <summary>
        /// 抽象的 onTick 接口, 子类自己去实现 他需要的 onTick 逻辑
        /// </summary>
        public abstract void OnTick();

    }

    /// <summary>
    /// 阶段运行时
    /// </summary>
    public class SkillStageRuntime : LocalServerRuntime
    {
        public int StageID => StageJson.StageID;
        /// <summary>
        /// 阶段的循环次数，默认从0开始
        /// </summary>
        public int StageLoop;

        // 效果线先不管
        // StageLines

        public ulong ParentRuntimeID;


        /// <summary>
        /// 阶段的 类型枚举
        /// </summary>
        public SkillStageEnum skillStageEnum;

        public Action<SkillStageRuntime> ActionOnStageExit;

        public StageJson StageJson;

        protected bool isNotCreateNext = false;
        /// <summary>
        /// 不创建 next 阶段的 表示
        /// </summary>
        public bool IsNotCreateNext => isNotCreateNext;
        /// <summary>
        /// 标记 结束 loop 循环
        /// </summary>
        protected bool isMarkEndLoop = false;
        public bool isNextStageLoop = false;

        /// <summary>
        /// 下个阶段是否 循环loop. 
        /// 如果 阶段的 配置循环次数 为 -1 或者 当前的 StageLoop(阶段从0开始) + 1 < 配置循环次数 , 则下次创建还需要循环
        /// </summary>
        public bool IsNextStageLoop => !isMarkEndLoop && isNextStageLoop;

        public int CfgLoopCount => StageJson.GetLoopCount();

        /// <summary>
        /// 阶段时长
        /// </summary>
        public int stageTime => StageJson.Duration;

        /// <summary>
        /// 阶段 同步的 消息
        /// </summary>
        public static RunStageRet runStageRet = new();
        /// <summary>
        /// 阶段强制 结束的 通知.
        /// 本地服 没有 单独的 阶段过程逻辑， 所以也没有打断逻辑.
        /// 此处为了方便，就在 阶段 结束的时候，就通知 给客户端.
        /// </summary>
        public static RunStageForceEndRet runStageForceEndRet = new RunStageForceEndRet();
        public SkillStageRuntime(StageJson stageJson, int loopIdx, BaseBlackBoard createrBlackBoard)
        {
            // 阶段创建的时候 分配它的唯一 UID
            UID = EditorMode.Instance.localServer.localServerIDManager.GetUID();
            // 每个阶段 都由自己的 唯一的 runtimeID
            RuntimeID = EditorMode.Instance.localServer.localServerIDManager.GetRuntimeID();
            StageJson = stageJson;
            StageLoop = loopIdx;
            isNextStageLoop = CfgLoopCount == -1 || (loopIdx + 1) < CfgLoopCount;

            SGF.Debuger.LogWarning($"{LocalServer.TagFlag} : 阶段_[{StageID}_{StageLoop}] 创建 ");

            DelayInvoker.DelayInvoke(this, stageTime / 1000f, (object[] args) =>
            {
                OnStageExit(false);
            }, null);

            // 本地服 的时间 用客户端本地时间
            CreateTime = TimeUtils.ClientNowStampMilli;

            // 创建这个阶段对应的 阶段黑板
            CreateStageBlackBoard(createrBlackBoard);
        }

        /// <summary>
        /// 阶段进入, 主要是为了 传几个参数,同时 通知 客户端 阶段创建的通知
        /// 阶段中执行的效果 由 客户端的 客户端线自己去执行， 本地服由于 没有效果线的逻辑。
        /// 所以 目前只关系 阶段的 开启/关闭 这些逻辑
        /// </summary>
        public void OnEnter(SkillStageEnum stageEnum, ulong parentRuntimeID, ulong ownerID, RuntimeEnumType runtimeType)
        {
            skillStageEnum = stageEnum;
            OwnerEntityID = ownerID;
            runtimeEnumType = runtimeType;

            // 阶段 的 父 运行时的 runtimeID
            ParentRuntimeID = parentRuntimeID;

            SendRunStage();
        }

        /// <summary>
        /// 阶段结束的调用接口
        /// </summary>
        /// <param name="isBreakExitCB">是否打断 退出阶段的 callBack</param>
        public void OnStageExit(bool isBreakExitCB)
        {
            DelayInvoker.CancelInvoke(this);

            SGF.Debuger.LogWarning($"{LocalServer.TagFlag} : 阶段_[{StageID}_{StageLoop}] 退出, 是否打断退出: {isBreakExitCB}");
            SendStageEnd();

            if (!isBreakExitCB)
            {
                ActionOnStageExit?.Invoke(this);
            }
        }

        /// <summary>
        /// 外部退出阶段的调用接口
        /// </summary>
        /// <param name="isBreakExitCB">是否打断 退出阶段的 callBack</param>
        public void ExitStage(bool isBreakExitCB)
        {
            OnStageExit(isBreakExitCB);
        }

        public void MarkSkipLoop(bool endLoop)
        {
            isMarkEndLoop = endLoop;
        }

        /// <summary>
        /// 表示 不创建下个 阶段
        /// </summary>
        /// <param name="notCreateNext"></param>
        public void MarkNotCreateNextstage(bool notCreateNext)
        {
            isNotCreateNext = notCreateNext;
        }

        public override void OnTick()
        {

        }

        public void CreateStageBlackBoard(BaseBlackBoard createrBlackBoard)
        {
            blackBoard = new BaseBlackBoard();

            // 指定阶段黑板的 上级运行时黑板
            blackBoard.parent = createrBlackBoard;


            // 根据父运行时的黑板，对阶段黑板的数据赋值
            // TODO 黑板赋值
        }

        public void SendRunStage()
        {
            runStageRet.UID = UID;
            runStageRet.BlackType = skillStageEnum;

            // 这个值服务器暂时没用
            runStageRet.TriggerGroupID = 0;

            // 阶段 runStageRet 发送的运行时id  是 主运行时 id
            runStageRet.RuntimeID = ParentRuntimeID;
            runStageRet.OwnerEntityID = OwnerEntityID;
            runStageRet.RuntimeType = runtimeEnumType;
            runStageRet.StageID = StageID;

            runStageRet.CreateTime = CreateTime;
            runStageRet.StageLoop = StageLoop;

            // 效果线先不管, 目前服务器应该也没有给客户端发送效果线
            runStageRet.StageLines.Clear();

            // 黑板数据 
            runStageRet.BlackList.Clear();

            // TODO: 黑板数据 后面填充

            EditorMode.Instance.localServer.SendMsg((object msgData, object otherData) =>
            {
                SGF.Debuger.LogWarning($"{LocalServer.TagFlag} : 阶段创建 runStageRet: [{msgData}] ");

                GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.RunStageRet, LocalServer.RPCMsgPacker(msgData, OwnerEntityID));
            }, runStageRet, MsgType.Delay);


        }

        public void SendStageEnd()
        {
            runStageForceEndRet.ID = UID;
            runStageForceEndRet.BreakType = BreakStageEnum.BreakStageDefault;
            runStageForceEndRet.RuntimeID = RuntimeID;

            EditorMode.Instance.localServer.SendMsg((object msgData, object otherData) =>
            {
                SGF.Debuger.LogWarning($"{LocalServer.TagFlag} : 阶段关闭 runStageForceEndRet: [{msgData}] ");

                GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.RunStageForceEndRet, LocalServer.RPCMsgPacker(msgData, OwnerEntityID));

            }, runStageForceEndRet, MsgType.Delay);
        }
    }

    /// <summary>
    /// 基础的 主运行时 的基类
    /// </summary>
    public abstract class BaseParentRuntime : LocalServerRuntime
    {
        /// <summary>
        /// 阶段运行时
        /// </summary>
        public DictionaryEx<ulong, SkillStageRuntime> stageRuntimes = new();
        /// <summary>
        /// 触发器
        /// </summary>
        public DictionaryEx<TriggerStageEnum, List<LocalServerTrigger>> triggers = new();

        public abstract List<StageJson> NormalStages { get; }

        /// <summary>
        /// 当前的 普通阶段运行时. 同时只应该有一个 普通阶段运行时
        /// </summary>
        public SkillStageRuntime normalStageRuntime;

        public abstract List<StageJson> BulletStages { get; }
        public abstract List<StageJson> OtherStages { get; }

        /// <summary>
        /// 当前的子弹阶段运行时
        /// </summary>
        public SkillStageRuntime bulletStageRuntime;

        public UVec3 TargetPos = UVec3.zero;


        public abstract void OnExitRuntime();

        public void AddStage(SkillStageRuntime skillStageRuntime)
        {
            stageRuntimes[skillStageRuntime.UID] = skillStageRuntime;
        }

        public void RemoveStage(SkillStageRuntime skillStageRuntime)
        {
            if (stageRuntimes.ContainsKey(skillStageRuntime.UID))
            {
                stageRuntimes.Remove(skillStageRuntime.UID);
            }
        }

        public List<LocalServerTrigger> GetEnumTriggers(TriggerStageEnum triggerStageEnum)
        {
            if (triggers.ContainsKey(triggerStageEnum))
            {
                return triggers[triggerStageEnum];
            }
            return null;
        }

        public void AddTrigger(StageJson stageJson)
        {
            var trigger = new LocalServerTrigger(stageJson);

            var enumTriggers = GetEnumTriggers(trigger.StageEnum);
            if (enumTriggers == null)
            {
                enumTriggers = new();
            }
            enumTriggers.Add(trigger);

            trigger.ActionOnTriggerStageTrigger = OnActionTriggerStageTrigger;
            trigger.FuncOnCheckCanTrigger = OnFuncCheckCanTrigger;

            triggers[trigger.StageEnum] = enumTriggers;
        }



        /// <summary>
        /// 触发 这种类型 对应的 触发器
        /// </summary>
        /// <param name="triggerStageEnum"></param>
        public void Trigger(TriggerStageEnum triggerStageEnum, object triggerData)
        {
            var enumTriggers = GetEnumTriggers(triggerStageEnum);
            if (enumTriggers == null || enumTriggers.Count == 0)
            {
                return;
            }
            for (int i = 0; i < enumTriggers.Count; i++)
            {
                var trigger = enumTriggers[i];
                if (trigger.CheckCanTrigger(triggerData))
                {
                    trigger.Trigger(triggerData);
                }

                // 检查触发器是否有效
                if (!trigger.Active)
                {
                    // 删除 这个触发器
                    enumTriggers.RemoveAt(i);
                    --i;
                }
            }

        }

        public void TriggerEvent(TriggerEvent triggerEvent, object triggerData)
        {
            switch (triggerEvent)
            {
                case StarProjectDef.TriggerEvent.PathMoveEnd:
                    {
                        Trigger(TriggerStageEnum.Stage_AtTargetPos, triggerData);
                    }
                    break;
                default: break;
            }
        }

        public void OnActionTriggerStageTrigger(LocalServerTrigger localServerTrigger, object triggerData)
        {
            // 阶段 loopIdx 从 0 开始
            CreateTriggerStage(localServerTrigger.TriggerStageJson, localServerTrigger.TriggeredCount - 1);
        }
        public void CreateTriggerStage(StageJson TriggerStageJson, int loopIdx)
        {
            var stage = CreateStage(TriggerStageJson, loopIdx);
            stage.OnEnter(SkillStageEnum.StageTrigger, RuntimeID, OwnerEntityID, RuntimeEnumType);

        }

        public SkillStageRuntime CreateStage(StageJson stageJson, int loopIdx)
        {
            SkillStageRuntime stageRuntime = new SkillStageRuntime(stageJson, loopIdx, blackBoard);

            AddStage(stageRuntime);

            return stageRuntime;
        }

        /// <summary>
        /// 创建 普通的阶段
        /// </summary>
        public void CreateNormalStage()
        {
            normalStageRuntime = GetNextStageRuntime(NormalStages, normalStageRuntime);

            if (normalStageRuntime == null)
            {
                OnExitRuntime();
                return;
            }

            normalStageRuntime.ActionOnStageExit = (SkillStageRuntime endStageRuntime) =>
            {
                if (endStageRuntime.IsNotCreateNext && (normalStageRuntime == null || normalStageRuntime.StageID != endStageRuntime.StageID))
                {
                    Debug.LogError($"stage: {endStageRuntime.StageID}_{endStageRuntime.StageLoop} not createNext");
                    return;
                }
                CreateNormalStage();
            };

            normalStageRuntime.OnEnter(SkillStageEnum.StageDefault, RuntimeID, OwnerEntityID, RuntimeEnumType);
        }


        /// <summary>
        /// 创建 子弹阶段
        /// </summary>
        public void CreateBulletStage()
        {
            bulletStageRuntime = GetNextStageRuntime(BulletStages, bulletStageRuntime);

            if (bulletStageRuntime == null)
            {
                return;
            }

            bulletStageRuntime.ActionOnStageExit = (SkillStageRuntime endStageRuntime) =>
            {
                if (endStageRuntime.IsNotCreateNext && (bulletStageRuntime == null || bulletStageRuntime.StageID != endStageRuntime.StageID))
                {
                    Debug.LogError($"bullet stage: {endStageRuntime.StageID}_{endStageRuntime.StageLoop} not createNext");
                    return;
                }
                CreateBulletStage();
            };

            bulletStageRuntime.OnEnter(SkillStageEnum.StageBulletMove, RuntimeID, OwnerEntityID, RuntimeEnumType);
        }

        public void CreateOhterStage()
        {
            OtherStages.ForEach((otherStage) =>
            {
                switch (otherStage.StageType)
                {
                    case StageType.TriggerStage:
                        {
                            AddTrigger(otherStage);
                        }
                        break;

                    default: break;
                }

            });
        }

        public void CreateNextStage(SkillStageRuntime skillStageRuntime)
        {
            if (skillStageRuntime == null)
            {
                return;
            }
            if (skillStageRuntime == normalStageRuntime)
            {
                CreateNormalStage();
                return;
            }
            if (skillStageRuntime == bulletStageRuntime)
            {
                CreateBulletStage();
                return;
            }
            // TODO
            // 其它阶段 
        }

        /// <summary>
        /// 获取 skillStageRuntime 对应的下一个 阶段配置
        /// </summary>
        /// <param name="stageJsons"></param>
        /// <param name="skillStageRuntime"></param>
        /// <param name="skillStageRuntime"></param>
        /// <returns></returns>
        public SkillStageRuntime GetNextStageRuntime(List<StageJson> stageJsons, SkillStageRuntime skillStageRuntime)
        {
            if (stageJsons.Count == 0)
            {
                return null;
            }

            // 如果阶段运行时 不存在, 那就说明是 第一次创建 , 就直接返回第一个
            if (skillStageRuntime == null)
            {
                return CreateStage(stageJsons[0], 0);
            }

            var stageJson = skillStageRuntime.StageJson;

            // 如果 skillStageRuntime 存在, 就需要判断这个运行时是否是 循环阶段
            if (skillStageRuntime.IsNextStageLoop && skillStageRuntime.StageLoop < 10)
            {
                return CreateStage(skillStageRuntime.StageJson, skillStageRuntime.StageLoop + 1);
            }
            // 当阶段 被标识不创建下个阶段的时候, 就不创建后续阶段
            else if (skillStageRuntime.IsNotCreateNext)
            {
                return null;
            }
            else
            {
                int idx = stageJsons.FindIndex((json) =>
                {
                    return json == stageJson;
                });
                // 找不到下一个阶段，就返回 null
                if (idx == -1 || idx == stageJsons.Count - 1)
                {
                    return null;
                }

                return CreateStage(stageJsons[idx + 1], 0);
            }

        }

        public void SetTargetPos(UVec3 targetPos)
        {
            TargetPos = targetPos;
        }

        public bool OnFuncCheckCanTrigger(LocalServerTrigger localServerTrigger, object triggerData)
        {
            switch (localServerTrigger.StageEnum)
            {
                case TriggerStageEnum.Stage_AtTargetPos:
                    {
                        if (TargetPos == UVec3.zero)
                        {
                            return false;
                        }

                        return TargetPos == (UVec3)triggerData;
                    }
                    break;

                default: break;
            }
            return false;
        }

        public bool BreakCurRuntimeInBullet(EffectTypeBreakCurRuntimeInBullet effectTypeBreakCurRuntimeInBullet)
        {
            SkillStageRuntime targetStage = null;

            switch (effectTypeBreakCurRuntimeInBullet.StageType)
            {
                case StageType.NormalStage:
                    {
                        targetStage = normalStageRuntime;
                    }
                    break;
                case StageType.BulletStage:
                    {
                        targetStage = bulletStageRuntime;
                    }
                    break;
                case StageType.AddBuffStage:
                case StageType.EndBuffStage:
                case StageType.EndPassiveStage:
                case StageType.TriggerStage:
                    {
                        SGF.Debuger.LogError($"{LocalServer.TagFlag} 无法打断阶段类型: {effectTypeBreakCurRuntimeInBullet.StageType}");
                    }
                    break;

                default: break;
            }

            // 没啥可以打断的阶段, 直接 return
            if (targetStage == null)
            {
                return true;
            }


            switch (effectTypeBreakCurRuntimeInBullet.OnTimeLogic)
            {
                case StageEvent.None:
                    {
                        return true;
                    }
                    break;
                case StageEvent.JumpStage1:
                    {
                        // 阶段结束后， 跳到下一个阶段(会跳过循环阶段)
                        targetStage.MarkSkipLoop(true);
                    }
                    break;
                case StageEvent.JumpStage2:
                    {
                        // 立即跳转 阶段
                        // 不结束当前阶段, 立即跳转下一阶段

                        // 首先标识 这个阶段跳过循环
                        targetStage.MarkSkipLoop(true);
                        // 开始创建 下一个阶段
                        CreateNextStage(targetStage);
                        // 标识这个 阶段 不创建 next 阶段
                        targetStage.MarkNotCreateNextstage(true);

                    }
                    break;
                case StageEvent.KillSkill:
                    {
                        // 结束运行时
                        BreakRuntime();
                    }
                    break;
                case StageEvent.KillStage:
                    {
                        // 结束 循环
                        // 打断当前阶段, 同时 跳过这个阶段的循环, 进入下一个阶段
                        targetStage.MarkSkipLoop(true);
                        // 立即结束这个阶段, 同时不打断 这个阶段的结束 cb
                        targetStage.ExitStage(false);
                    }
                    break;
                case StageEvent.ReSet:
                    {
                        // 跳转到下次循环
                        // 相当于 不结束当前阶段, 但是 立即 开启下一次循环阶段

                        // 开始创建 下一个阶段
                        CreateNextStage(targetStage);
                        // 标识这个 阶段 不创建 next 阶段
                        targetStage.MarkNotCreateNextstage(true);

                    }
                    break;
                case StageEvent.EndStageReSet:
                    {
                        // 结束当前阶段 跳转下次循环
                        targetStage.ExitStage(false);
                    }
                    break;
                default: break;
            }
            return true;
        }

        public void BreakRuntime()
        {
            OnExitRuntime();
        }
    }



    /// <summary>
    /// 同步的自定义黑板数据
    /// </summary>
    public class CusBlackBoardData
    {
        public CusUpdateBlackType CusBlackType;
        public ulong RuntimeID;

        public BaseBlackBoard blackBoard;
    }

}


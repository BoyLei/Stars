using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
/*using StarProject.Game.SnapShot.Buff;
using StarProject.Game.SnapShot.Skill;*/
using UnityEngine;
using System.Collections.Generic;

namespace StarProject.Game.SnapShot
{
    /// <summary>
    /// 黑板的类型
    /// </summary>
    public enum EnumBlackBoardType
    {
        Default,
        //技能黑板----------------------
        RunTimeAllEffect,
        SkillUse,
        RuntimeAllData,//技能会触发其他

        //buff黑板----------------------
        BuffCreate,



        Attribute,          //属性
        PassiveSkills,      //被动
        halo,                //光环

    }

    public enum RunTimeState
    {
        /// <summary>
        /// 运行时还未创建
        /// </summary>
        None,
        /// <summary>
        /// 运行时正在运行
        /// </summary>
        Running,
        /// <summary>
        /// 运行时已经结束
        /// note:
        ///     对于技能而言,技能运行时正常结束服务器并不会通知客户端.
        ///     所以客户端的运行时结束,由客户端自己控制
        /// </summary>
        Destroy,
    }

    /// <summary>
    /// 此处的设计经历过几个阶段:
    /// 1.原始阶段,此处是装入所有黑板数据进行统计,然后统一分发出去,此处只做统计和 分发;
    /// 
    /// 2.技能开发阶段:
    ///     想采用数据快照的模式,此处将所有的黑板数据进行归类,然后pop出去。
    /// 
    /// 3.技能重构阶段1:
    ///     此处只做数据的缓存,技能效果到了需要的时候,过来取. 但是对于非当前技能阶段的效果数据,
    ///     或者技能运行时结束,但是效果没结束的 黑板效果数据, 客户端目前没办法处理.
    /// 
    /// 4.技能重构阶段2:
    ///     黑板效果数据重新划分:
    ///     a. 无当前技能RuntimeID记录;
    ///     b. 存在RuntimeID记录,且 正在runing;
    ///     c. 存在RuntimeID记录,且 skill已经销毁。
    ///     
    ///     对于以上a/b/c类型,需要分别处理如下:
    ///     a: 
    ///         对于主动技能的效果数据而言,正常情况下,技能的创建会提前于效果数据.
    ///         所有如果有数据在技能运行时之前存在,报error,同时立即执行.
    ///         note1:
    ///         对于被动技能,可能存在人物刚上线,但是客户端主角还未创建,就收到被动效果伤害数据。
    ///         这种异常数据,暂时先忽略,不处理.
    ///         note2:
    ///         不处理的前提是 客户端收到技能使用回复后,立即使用技能,不执行异步逻辑(比如使用技能前慢慢走到技能点)
    ///     b:
    ///         存在当前的技能运行时,那存入cache,执行正常的技能取数据的流程.
    ///     
    ///     c:
    ///         有些主动技能,技能运行时结束,但技能的效果线未结束(服务器的技能运行时与效果运行时分离,效果运行时是多段效果串联),
    ///         所有会出现 客户端技能运行时结束了,但是依旧可以收到 服务器的效果数据.
    ///         
    /// 
    ///         此时,收到了效果数据,如果此时有等待效果队列,就执行等待效果队列逻辑,没有,客户端就立即显示
    ///         
    ///     【暂时用不到】
    /// </summary>
    public class SerSnapshotSeqManager : ServiceModule<SerSnapshotSeqManager>
    {
        private string TagFlag = "[SerSnapshotSeqManager]";

        /// <summary>
        /// 运行时所处的阶段
        /// </summary>



        /// <summary>
        /// key : EntityID , value : RuntimeID--->RunTimeState
        /// note:
        ///     在 EntityID实体销毁的时候,减少本地数据记录的量
        ///     如果不做分类,每次添加即存在本地, EntityID2RuntimeRecord 可能会因为游戏时长过多,内存爆掉。
        ///     
        ///     但目前数据的设计,必须要存在一个runtime的record.
        ///     所以目前的做法,是子要entity干掉之后,干掉本地存在的相应record运行时数据.
        /// </summary>
        /// <typeparam name="ulong">EntityID</typeparam>
        /// <typeparam name="ulong">RuntimeID</typeparam>
        /// <returns></returns>
    /*    private Dictionary<ulong, Dictionary<ulong, RunTimeState>> EntityID2RuntimeRecord = new Dictionary<ulong, Dictionary<ulong, RunTimeState>>();*/

        public void Init()
        {
            /* SGF.Debuger.Log($"{TagFlag} : Init ");
             //消息注册
             {
                 //buf1
                 NetworkManager.Instance.OnMessageEnum(MsgIDEnum.BuffCreateRetID, OnBuffCreateRet, this, true);
             }

             {
                 //技能使用
                 NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SkillUseRetID, OnSkillUseRet, this, true);
             }
             {
                 //全局效果
                 NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RunStageRetID, OnEffectSnap, this, true);

                 //全局数据
                 NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RuntimeSyncRetID, OnAllDataSnap, this, true);
             }*/

            /*//给buf模块增加update的功能
            MonoHelper.AddFixedUpdateListener(OnMyUpdate);*/
        }

        /* public bool CheckHasRecordRuntime(ulong RuntimeID, ulong EntityID)
         {
             bool hasRecord = false;

             if (EntityID2RuntimeRecord.ContainsKey(EntityID))
             {
                 Dictionary<ulong, RunTimeState> runtimeRecord = EntityID2RuntimeRecord[EntityID];
                 if (runtimeRecord.ContainsKey(RuntimeID))
                 {
                     hasRecord = true;
                 }
             }
             return hasRecord;
         }

         public void RecordRuntime(ulong RuntimeID, ulong entityID, RunTimeState runTimeState)
         {
             Dictionary<ulong, RunTimeState> runtimeRecord;
             if (!EntityID2RuntimeRecord.ContainsKey(entityID))
             {
                 runtimeRecord = new Dictionary<ulong, RunTimeState>();
             }
             else
             {
                 runtimeRecord = EntityID2RuntimeRecord[entityID];
             }
             runtimeRecord[RuntimeID] = runTimeState;
             EntityID2RuntimeRecord[entityID] = runtimeRecord;

             if (runTimeState == RunTimeState.Destroy)
             {
                 DelayInvoker.DelayInvoke(15, (object[] ags) =>
                 {
                     DeleteRecordRuntime((ulong)ags[0], (ulong)ags[1]);
                 }, new object[] { RuntimeID, entityID });
             }
         }

         public void DeleteRecordRuntime(ulong RuntimeID, ulong entityID)
         {
             Dictionary<ulong, RunTimeState> runtimeRecord;
             if (!EntityID2RuntimeRecord.ContainsKey(entityID))
             {
                 return;
             }
             runtimeRecord = EntityID2RuntimeRecord[entityID];

             if (runtimeRecord.ContainsKey(RuntimeID))
             {
                 runtimeRecord.Remove(RuntimeID);
             }
         }

         /// <summary>
         /// 移除与entityID 相关的 运行时记录数据
         /// </summary>
         /// <param name="entityID"></param>
         public void ReleaseEntityRuntimeRecord(ulong entityID)
         {
             if (EntityID2RuntimeRecord.ContainsKey(entityID))
             {
                 EntityID2RuntimeRecord.Remove(entityID);
             }
         }

         public override void Release()
         {
             MonoHelper.RemoveFixedUpdateListener(OnMyUpdate);
             DynamicDataFactory.Release();

             EntityID2RuntimeRecord.Clear();

             base.Release();
             SGF.Debuger.Log($"{TagFlag} Release() ");
         }

         /// <summary>
         /// 所有主动技能效果的入口
         /// 直接从EnQueueBlackInfo 拷贝过来的代码
         /// </summary>
         /// <param name="RuntimeID"></param>
         /// <param name="EntityID"></param>
         /// <param name="BlackList"></param>
         /// <param name="runtimeType"></param>
         /// <param name="BlackBoardType"></param>
         /// <param name="data"></param>
         void EnQueueSkillEffect(ulong RuntimeID, ulong EntityID, RepeatedField<BlackBoardNode> BlackList, RuntimeEnumType runtimeType, EnumBlackBoardType BlackBoardType, object data)
         {
             //SGF.Debuger.Log($" {TagFlag}   Add RuntimeAllEffect RuntimeID {RuntimeID}");

             /// <summary>
             /// 如果不包含当前 RuntimeID 的运行时,只有几种情况 
             /// 1.可能不同效果的 RuntimeEnumType (被动/主动/buff 类型) 的运行时漏了 record,所以加个error日志,排除遗漏;
             /// 2.如果真的存在 运行时效果 提前过来,同时本地还没有对应的运行时记录,那这个时候就直接执行这个运行时。
             /// </summary>
             bool hasRecord = CheckHasRecordRuntime(RuntimeID, EntityID);

             if (!hasRecord)
             {
                 SGF.Debuger.LogError($"{TagFlag} EnQueueSkillEffect runtimeID {RuntimeID} , EntityID {EntityID} , runtimeType {runtimeType} , BlackBoardType {BlackBoardType} , no runtime record.");
                 //GlobalEvent.onNoRecordRunBlack.Invoke((RunStageRet)data);
                 return;
             }


             //多次效果数据是多个RuntimeAllEffect（一次数据集合服务器会提前发来很多数据集合），里面是每一个效果数据的idList
             RuntimeAllEffect skillRuntimeSnap = DynamicDataFactory.GetData<RuntimeAllEffect>(RuntimeID, EntityID, runtimeType, BlackBoardType);
             if (skillRuntimeSnap == null)
             {
                 skillRuntimeSnap = DynamicDataFactory.InstanceData<RuntimeAllEffect>(RuntimeID);
             }
             skillRuntimeSnap.Init(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);

         }

         void EnQueueBlackInfo(ulong RuntimeID, ulong EntityID, RepeatedField<BlackBoardNode> BlackList, RuntimeEnumType runtimeType, EnumBlackBoardType BlackBoardType, object data)
         {
             switch (runtimeType)
             {
                 case RuntimeEnumType.DefaultRuntime:
                     //1用源类型以后，2就没有default了除了老字段，3或者bug
                     break;
                 case RuntimeEnumType.PassiveSkill:
                     {
                         //note:
                         //被动技能黑板效果
                         //被动技能 黑板效果RunBlackRet ，客户端收到 被动的 RunBlackRet后,知道运行时 tick了
                         //所以对于 被动技能 的运行时 和 效果来说，都应该是 收到数据后,由此处主动将数据 推出去 
                         if (BlackBoardType == EnumBlackBoardType.RunTimeAllEffect)//这里告诉你扣除500
                         {
                             //TODO: dl
                             //算了,先不用 DynamicDataFactory的方式了
                             //note:
                             //      目前发现 DynamicDataFactory存在如下几个问题
                             //      1.PopEarliestData  会在内部 release(ServiceSnapshotData 类型),导致外部无法获取pop了什么数据
                             //      2.GetData 只是获取了数据,并没有被弹出, 相当于这份数据 同时被 Factory 和外部持有
                             //
                             //      对于需要主动推的数据,runtimeAllEffect的方式 使用都很麻烦
                             //      目前先用简单的 GlobalEvent 方式通知外面

                             //首先将黑板效果数据存入
                             // EnQueueRuntimeEffect(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);
                             //通知外面被动技能黑板效果数据收到了,让外面来处理
                             // GlobalEvent.onPassiveSkillBoardInfo.Invoke((RunStageRet)data);
                         }

                     }
                     break;
                 case RuntimeEnumType.ActiveSkill:
                     //用表格自己缓存嘛：不必因为分类了，我要看什么协议
                     if (BlackBoardType == EnumBlackBoardType.RuntimeAllData)//这里告诉你剩余多少血
                     {
                         //虽然技能定义，但是是运行时的运行时
                         RuntimeAllData runtime = DynamicDataFactory.InstanceData<RuntimeAllData>(EntityID);
                         *//*bool success = *//*
                         runtime.Init(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);
                     }
                     else if (BlackBoardType == EnumBlackBoardType.RunTimeAllEffect)//这里告诉你扣除500
                     {
                         //EnQueueSkillEffect(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);
                     }
                     else
                     {
                         SkillRet skillUseSnap = DynamicDataFactory.InstanceData<SkillRet>(EntityID);
                         *//*bool success = *//*
                         skillUseSnap.Init(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);
                     }

                     break;
                 case RuntimeEnumType.Buff:
                     {
                         BuffCreateSnap buffCreate = DynamicDataFactory.InstanceData<BuffCreateSnap>(EntityID);
                         *//*bool success = *//*
                         buffCreate.Init(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);
                         // TODO: 曲  技能4 持续的BUFF伤害，会在这里告诉伤害黑板数据
                         //SGF.Debuger.Log($" {TagFlag} [Snap]   Buff RuntimeID {RuntimeID} , EntityID {EntityID} , runtimeType {runtimeType} , BlackBoardType {BlackBoardType}");
                     }
                     break;
                 case RuntimeEnumType.Bullet:
                     {
                         BulletRunBlackSnap bulletRunbackSnap = DynamicDataFactory.InstanceData<BulletRunBlackSnap>(EntityID);
                         *//*bool success = *//*
                         bulletRunbackSnap.Init(RuntimeID, EntityID, BlackList, runtimeType, BlackBoardType, data);
                     }
                     break;
                 default:
                     break;
             }
             //SGF.Debuger.Log($"{TagFlag}: EnQueueBlackInfo {runtimeType}");

         }

         void OnBuffCreateRet(MessageHandleData data)
         {
             SGF.Debuger.Log($"{TagFlag}: OnBuffCreateRet");
             //1运行时id，再组内是不重复的如buff1和buff2，组间也是不重复的如buff1和skill1
             //2运行时id，是动态生成的99999之下的递增数值,组内组间都再这里取
             //问题：浪费ulong，没有利用数值的分析逻辑，不确定系统


             BuffCreateRet buffCreateRet = (BuffCreateRet)data.data;

             EnQueueBlackInfo(buffCreateRet.RuntimeID, buffCreateRet.OwnerEntityID, buffCreateRet.BlackList, RuntimeEnumType.Buff, EnumBlackBoardType.BuffCreate, buffCreateRet);
         }


         /// <summary>
         /// 技能的使用协议
         /// </summary>
         /// <param name="data"></param>
         void OnSkillUseRet(MessageHandleData data)
         {
             // SGF.Debuger.Log($"{TagFlag}: OnSkillUseRet , {TimeUtils.ClientNowStampMilli} ms");

             // SkillUseRet skillUseRet = (SkillUseRet)data.data;

             // RecordRuntime(skillUseRet.RuntimeID, skillUseRet.OwnerEntityID, RunTimeState.Running);
             // //Skillid是同一个
             // EnQueueBlackInfo(skillUseRet.RuntimeID, skillUseRet.OwnerEntityID, skillUseRet.BlackList, RuntimeEnumType.ActiveSkill, EnumBlackBoardType.SkillUse, skillUseRet);
         }

         /// <summary>
         /// 全部的效果解析
         /// </summary>
         /// <param name="data"></param>
         void OnEffectSnap(MessageHandleData data)
         {
             SGF.Debuger.Log($"{TagFlag}: OnRunBlackRet -> OnEffectSnap");

             RunStageRet runStageRet = (RunStageRet)data.data;
             //Skillid是同一个

             EnQueueBlackInfo(runStageRet.RuntimeID, runStageRet.OwnerEntityID, runStageRet.BlackList, runStageRet.RuntimeType, EnumBlackBoardType.RunTimeAllEffect, runStageRet);
         }

         /// <summary>
         /// 全部的数据解析
         /// </summary>
         /// <param name="data"></param>
         void OnAllDataSnap(MessageHandleData data)
         {
             SGF.Debuger.Log($"{TagFlag}: OnRuntimeSyncRet -> OnAllDataSnap");

             RuntimeSyncRet runtimeSyncRet = (RuntimeSyncRet)data.data;
             //Skillid是同一个
             EnQueueBlackInfo(runtimeSyncRet.RuntimeID, runtimeSyncRet.OwnerEntityID, runtimeSyncRet.BlackList, runtimeSyncRet.RuntimeType, EnumBlackBoardType.RuntimeAllData, runtimeSyncRet);
         }

         /// <summary>
         /// 伤害面板统计
         /// </summary>
         void BlackBoardStatistics(ServiceSnapshotData blackBoardInfo)
         {
             //TODO: dl
             //统计伤害
             //SGF.Debuger.Log($"{TagFlag}: BlackBoardStatistics xxx");
         }

         private ImmediateRuntimeData immediateRuntimeData;

         private RuntimeAllEffect immediateRuntimeAllEffect;
         private BulletRunBlackSnap tempBulletRunBlackSnap;
         private BuffCreateSnap tempBuffCreateSnap;


         void HandleWithRunBlackSnap<T>(ServiceSnapshotData data) where T : ServiceSnapshotData
         {
             bool cont = true;
             do
             {
                 data = DynamicDataFactory.PopEarliestData<T>();
                 if (data == null)
                 {
                     cont = false;//不处理且不循环
                     continue;
                 }
                 if (data.runtimeEnumType == RuntimeEnumType.Bullet)
                 {
                     //如果是子弹类型，让子弹飞一会
                     GlobalEvent.onBulletBoardInfo.Invoke(data);
                 }
                 else if (data.runtimeEnumType == RuntimeEnumType.Buff)
                 {
                     GlobalEvent.onBuffBlackBoardInfo.Invoke(data);
                 }
                 else if (data.runtimeEnumType == RuntimeEnumType.PassiveSkill)
                 {

                 }

                 //TODO: dl
                 //后续其它主动抛出的数据类型有需要继续填充

                 SGF.Debuger.Log($"{TagFlag} HandleWithRunBlackSnap type {data.runtimeEnumType}");

                 //在此处可以做具体的伤害统计
                 BlackBoardStatistics(data);

                 double exeTime = data.ExecuteTime;
                 //所有中断条件
                 //需要检测，且时间未至
                 if (exeTime != 0 && exeTime < TimeUtils.GetTotalSeconds())
                 {
                     cont = false;
                 }
                 data = null;

             } while (cont);

         }


         void OnMyUpdate()
         {
             ///TODO:       整体是一个Mgr(下文）
             ///TODO:       分帧处理(下文）:
             ///TODO：    分组处理：广度优先：设置组优先级;剔除低优先级
             ///TODO：    分数据处理：数据数量上限：剔除30条
             ///TODO：    条件处理：上下关系
             ///Done：条件处理：时间处理
             ///说明：整体是多个Queue，每个Queue再自己的Mgr里处理，现在先处理自己的


             //如果分类处理：如SkillSnapshotData ：ServiceSnapshotData
             //DynamicDataFactory.EarliestData<如SkillSnapshotData>();
             //在代码前端优先处理：目前是全部按照基类处理
             //如需考虑属性：再分一个代码片段

             //HandleWithRunBlackSnap<RuntimeAllData>(tempRuntimeAllData);

             //处理立即需要pop 的数据
             HandleWithRunBlackSnap<ImmediateRuntimeData>(immediateRuntimeData);

             HandleWithRunBlackSnap<BulletRunBlackSnap>(tempBulletRunBlackSnap);

             HandleWithRunBlackSnap<BuffCreateSnap>(tempBuffCreateSnap);

         }
 */


    }
}

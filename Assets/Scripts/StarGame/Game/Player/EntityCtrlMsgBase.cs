using SGF.Network;
using StarProject.Game.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StarProject.Game.Player
{
    /// <summary>
    /// 抽象的 实体 消息处理基类
    /// </summary>
    public abstract class EntityCtrlMsgBase
    {
        private StringBuilder msgSb = new();

        /// <summary>
        /// 玩家的 数据, 目前主要用来 在消息执行出现异常时的 日志 提示
        /// </summary>
        protected virtual EntityBaseData EntityData { get; }

        string rpcMsgName = "";
        Dictionary<string, Action<MessageHandleData>> rpcMsgHandles = new();

        /// <summary>
        /// 初始化 entityCtrl 的 消息监听
        /// </summary>
        public void InitMsgListen()
        {
            rpcMsgHandles.Add("BuffCreateRet", OnBuffCreateRet);
            rpcMsgHandles.Add("BuffEndRet", OnBuffEndRet);
            rpcMsgHandles.Add("RuntimeSyncRet", OnRuntimeSyncRet);
            rpcMsgHandles.Add("SkillUseRet", OnSkillUseRet);
            rpcMsgHandles.Add("RunBlackRet", OnRunBlackRet);
            rpcMsgHandles.Add("SkillEndRet", OnSkillEndRet);
            rpcMsgHandles.Add("SkillQuitReq", OnSkillQuitReq);
            rpcMsgHandles.Add("BulletCreateRet", OnBulletCreateRet);
            rpcMsgHandles.Add("BulletEndRet", OnBulletEndRet);
            rpcMsgHandles.Add("ChangeDeadState", OnChangeDeadState);
            rpcMsgHandles.Add("RoleReviveRet", OnRoleReviveRet);
            rpcMsgHandles.Add("PassiveSkillUseRet", OnPassiveSkillUseRet);
            rpcMsgHandles.Add("PassiveSkillEndRet", OnPassiveSkillEndRet);
            rpcMsgHandles.Add("InterRet", OnInterRet);
            rpcMsgHandles.Add("RunStageRet", OnRunStageRet);
            rpcMsgHandles.Add("CDUpdateNotice", OnCDUpdateNotice);
            rpcMsgHandles.Add("RunStageForceEndRet", OnRunStageForceEndRet);
            rpcMsgHandles.Add("ServerSetPosNTF", OnServerSetPosNTF);
            rpcMsgHandles.Add("ReadyDeadNtf", OnReadyDeadNtf);
        }

        public void ClearMsgListen()
        {
            rpcMsgHandles.Clear();
        }

        public void HandleMessage(MessageHandleData messageHandleData)
        {
            rpcMsgName = messageHandleData.messageName;

            if (!rpcMsgHandles.ContainsKey(rpcMsgName))
            {
                SGF.Debuger.LogWarning($"HandleMessage messageName : {rpcMsgName} lost in EnumPBMsgName ");
                return;
            }

            try
            {
                // SGF.Debuger.Log($"[xxxx] rpcMsgName {rpcMsgName} , {messageHandleData.data}");
                rpcMsgHandles[rpcMsgName].Invoke(messageHandleData);
            }
            catch (Exception e)
            {
                SGF.Debuger.LogError($" HandleMessage messageName : {rpcMsgName} ,{e.Message}, {e.StackTrace}, \\n {e.InnerException},\\n {e.InnerException?.StackTrace} ");

                // SGF.Debuger.LogError($" HandleMessage messageName : {rpcMsgName} , id={EntityData?.M_EntityID},type={EntityData?.EntityType} ");
            }
        }

        #region RPC协议处理
        //=================================================================
        //增加一层RPC处理层，收到RPC消息后，只有对应的PlayerCtrlGroup去处理对应的消息
        /// <summary>
        /// 分层：AOI，【玩家控制群】，玩家信息
        ///  rpcMsg统一的控制playerCtrlGrop 的接口，理论上所有的pbMsg都需要在里面处理
        ///  注：
        ///  由于rpcMsg 包裹的message都是pb结构，所以只在rpc的 messageHandleData中存储了messageName， 如果后面rpcMsg有自定义结构，再添加
        /// </summary>
        /// <param name="messageHandleData"></param>
        /// 
        private Type typeOfEnumPBMsgName = typeof(EnumPBMsgName);

        private object[] args = new object[1] { null };

        /// <summary>
        /// rpc 消息的 缓存 队列
        /// note:
        ///      如果在处理rpc消息的工程中,需要 异步暂停消息处理, 
        ///      此时，就需要将 新收到的 消息 存入 消息缓存队列中, 等 异步消息锁解除之后, 重现按顺序处理
        /// </summary>
        private Queue<MessageHandleData> rpcMessageCacheQueue = new();

        /// <summary>
        /// 异步消息 锁
        /// note:
        ///     异步消息 锁 同时间应该 只有一个, 因为 同一时间应该只会处理 一个消息. 
        ///     当 消息队列 A-B-C 均为 需要锁的异步消息的时候, 处理 A , 消息锁住. 
        ///     只有等到 A 处理完成 解锁, 才会 依次处理 B 和 C.
        ///     所以 此处 消息所 采用 bool , 而不是采用 计数 的方式.
        /// </summary>
        private bool rpcMessageLock = false;

        public void LockRPCMessage(bool isLockMessage)
        {
            rpcMessageLock = isLockMessage;

            // 如果是 解除 异步消息锁, 那就同时处理 之前锁住的所有 rpc 消息
            if (!rpcMessageLock)
            {
                HandleRpcMessageCacheQueue();
            }
        }

        private void HandleRpcMessageCacheQueue()
        {
            // 如果 消息缓存队列 有消息, 且 消息锁 未锁住, 那就依次处理消息队列中的消息
            while (rpcMessageCacheQueue.Count > 0 && !rpcMessageLock)
            {
                MessageHandleData messageHandleData = rpcMessageCacheQueue.Dequeue();
                HandleRpcMsg(messageHandleData);
            }
        }

        public void HandleRpcMsg(MessageHandleData messageHandleData)
        {
            if (rpcMessageLock)
            {
                rpcMessageCacheQueue.Enqueue(messageHandleData);
                return;
            }
            HandleMessage(messageHandleData);


            // string msgName = messageHandleData.messageName;
            // bool hasPbEnum = Enum.IsDefined(typeOfEnumPBMsgName, msgName);
            // //if (M_Curr.EnityId == GameManager.Instance.mainPlayerId)
            // //{
            // //    SGF.Debuger.LogError($" RPC协议顺序 HandleRpcMsg messageName : {msgName} ");
            // //}
            // if (!hasPbEnum)
            // {
            //     SGF.Debuger.LogError($" HandleRpcMsg messageName : {msgName} lost in EnumPBMsgName ");
            // }
            // else
            // {
            //     EnumPBMsgName msgEnum = (EnumPBMsgName)Enum.Parse(typeOfEnumPBMsgName, msgName);

            //     msgSb.Clear();
            //     msgSb.AppendFormat("On{0}", msgEnum.ToString());

            //     args[0] = messageHandleData;

            //     HandleMessage(msgSb.ToString(), args);
            // }
        }

        public void HandleMessage(string msgName, object[] args)
        {
            //SGF.Debuger.Log($" HandleMessage() msg:{msgName}, args:{args}");

            MethodInfo mi = this.GetType().GetMethod(msgName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null)
            {
                try
                {
                    mi.Invoke(this, BindingFlags.NonPublic, null, args, null);
                }
                catch (Exception e)
                {
                    SGF.Debuger.LogError($" HandleMessage messageName : {msgName} , id={EntityData?.M_EntityID},type={EntityData?.EntityType} ,{e.InnerException},{e.InnerException.StackTrace} ");
                }
            }
            else
            {
                SGF.Debuger.LogError($" HandleMessage messageName : {msgName} , cant find {msgSb}  or method has overload !!!");
            }
        }
        #endregion

        public virtual void OnBuffCreateRet(MessageHandleData data)
        {

        }
        public virtual void OnBuffEndRet(MessageHandleData data) { }
        public virtual void OnRuntimeSyncRet(MessageHandleData data) { }
        public virtual void OnSkillUseRet(MessageHandleData data) { }
        public virtual void OnRunBlackRet(MessageHandleData data) { }
        public virtual void OnSkillEndRet(MessageHandleData data) { }
        public virtual void OnSkillQuitReq(MessageHandleData data) { }
        public virtual void OnBulletCreateRet(MessageHandleData data) { }
        public virtual void OnBulletEndRet(MessageHandleData data) { }
        public virtual void OnChangeDeadState(MessageHandleData data) { }
        public virtual void OnRoleReviveRet(MessageHandleData data) { }
        public virtual void OnPassiveSkillUseRet(MessageHandleData data) { }
        public virtual void OnPassiveSkillEndRet(MessageHandleData data) { }
        public virtual void OnInterRet(MessageHandleData data) { }
        public virtual void OnRunStageRet(MessageHandleData data) { }
        public virtual void OnCDUpdateNotice(MessageHandleData data) { }
        public virtual void OnRunStageForceEndRet(MessageHandleData data) { }

        public virtual void OnServerSetPosNTF(MessageHandleData data) { }
        public virtual void OnReadyDeadNtf(MessageHandleData data)
        {
            // SGF.Debuger.LogError($"[Ready-Dead]");
            ProtoMsg.ReadyDeadNtf serverSetPosMsg = (ProtoMsg.ReadyDeadNtf)data.data;

        }


    }

}

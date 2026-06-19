using Google.Protobuf;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using ProtoMsg;
using StarProject.Game.SnapShot;
using StarProject.Game.Entity.Factory;
using System.Threading;

namespace SGF.Network
{

    public class MessageData
    {
        public int cmd;
        public byte[] data;
        public byte serializeType;
        public string socketName;


        public byte[] allData;
        public int buffLength = 0;
        public int dataLength;
        public int curBuffPosition;
        //自动大小数据缓存器
        public int minBuffLen;
    }

    [XLua.LuaCallCSharp]
    public class MessageHandleData
    {
        /// <summary>
        /// 消息体数据
        /// </summary>
        public object data;
        /// <summary>
        /// 消息体 完整解析出来的结构, 如果是 rpc消息, 则会包含这个 rpc消息的结构
        /// </summary>
        public object DeserializedData;
        /// <summary>
        /// scoket 对应的名字
        /// </summary>
        public string socketName;

        public bool isRPC;

        /// <summary>
        /// rpc 消息的 entityID 或者 默认 0 (0代表这条消息是发给自己)
        /// 这条消息的实体id，只有rpc的消息才带有enityID。
        /// </summary>
        public ulong enityId;
        /// <summary>
        /// 消息对应的cmd
        /// </summary>
        public int messageCmd;
        /// <summary>
        /// 解析后的消息对应的名字
        /// </summary>
        public string messageName;
    }

    /// <summary>
    /// data 数据类型转换的问题，先不考虑，因为自定义消息的msg 数据结构不是pb类型，所以先用object来替换
    /// 后面如果考虑对象转换性能开心，再去把接口分开
    /// </summary>
    /// <param name="data"></param>
    [XLua.LuaCallCSharp]
    public delegate void MessageHandle(MessageHandleData data);



    [XLua.LuaCallCSharp]
    public class NetworkManager : ServiceModule<NetworkManager>
    {
        public void Init()
        {
            //检查本机ip
            IPUtils.CheckSelfIPAddress();
            // Debug.Log($"{flagKey} : Init ");

            //给NetworkManager增加mono的update功能，此单例不需要释放
            MonoHelper.AddUpdateListener(OnMyUpdate, MonoHelper.E_ModuleType.Net);

            gameSocket.OnSocketClose = OnGameSocketClose;


            Application.quitting += OnQuitGame;
        }

        public void Close()
        {
            gameSocket.End();
            battleSocket.End();
        }

        public override void Release()
        {
            MonoHelper.RemoveUpdateListener(OnMyUpdate, MonoHelper.E_ModuleType.Net);
            base.Release();
            this.Log("Release() NetWork Manager");
        }


        public string flagKey = "[MessageCtr]";


        /// <summary>
        /// 第一个tcp连接，负责系统方面的，装备啥的
        /// </summary>
        public SocketBase gameSocket = new SocketBase("game");

        /// <summary>
        /// 2023/5/3
        /// note:
        ///     服务器 现在 已经改为只有 game,没有 battle , 客户端 已经不需要再再使用 battleSocket
        /// 
        /// 第二个tcp连接，战斗之类的
        /// </summary>
        public SocketBase battleSocket = new SocketBase("battle");

        /// <summary>
        /// key 由 注册监听的条件组成，如
        /// 1.普通的 cmd消息监听 ， key =  cmd.toString();
        /// 2.rpc结构的cmd消息监听, key =  rpcCmd+'_'+messageCmd+'_'+enityId;
        /// </summary>
        private Dictionary<string, MessageHandle> key2MessageHandleDic = new Dictionary<string, MessageHandle>();

        /// <summary>
        /// 由 target 映射的 <key,MessageHandle> 键值对, 用来 通过 target 统一移除 target 所有的消息监听
        /// </summary>
        /// <returns></returns>
        private Dictionary<object, List<KeyValuePair<string, MessageHandle>>> target2KeyMessage = new Dictionary<object, List<KeyValuePair<string, MessageHandle>>>();

        public Queue<MessageData> messageDataQueue = new();

        private Queue<MessageHandleData> messageHandleDataQueue = new();

        private readonly object mLock = new object();

        private int _rpcCmd = 0;
        /// <summary>
        /// 弄一个RpcCmd的get属性，免得每次都去查找
        /// </summary>
        public int RPCCmd
        {
            get
            {
                if (_rpcCmd == 0)
                {
                    _rpcCmd = (int)MsgIDEnum.RpcMsgID;
                }
                return _rpcCmd;
            }
        }


        private int _fixCmd = 0;
        public int FixCmd
        {
            get
            {
                if (_fixCmd == 0)
                {
                    _fixCmd = (int)MsgIDEnum.DBUpUserDatasReqID;
                }
                return _fixCmd;
            }
        }

        // 第三方指定的协议号
        private int _thirdCmd = 0;
        public int ThirdCmd
        {
            get
            {
                if (_thirdCmd == 0)
                {
                    _thirdCmd = (int)MsgIDEnum.Client2ThirdRetID;
                }
                return _thirdCmd;
            }
        }

        // private string

        private string FormatKey(int messageCmd, bool isGroup = false)
        {
            sb.Clear();
            sb.Append(messageCmd).Append("_").Append(isGroup);
            return sb.ToString();
            // return messageCmd.ToString() + '_' + isGroup.ToString();
        }

        private string FormatKey(int rpcCmd, int messageCmd, ulong enityId)
        {
            sb.Clear();
            sb.Append(rpcCmd).Append("_").Append(messageCmd).Append("_").Append(enityId); ;
            return sb.ToString();
            // return rpcCmd.ToString() + '_' + messageCmd.ToString() + '_' + enityId.ToString();
        }

        public void AddMessageData(MessageData messageData)
        {
            return;
            // bool isRpc = messageData.cmd == RPCCmd;
            // if (isRpc)
            // {
            //Debug.LogWarning($"{flagKey} socket:{messageData.socketName},AddMessageDataCMD:{messageData.cmd},serializeType:{messageData.serializeType},buffLength:{messageData.buffLength},dataLength:{messageData.dataLength},curBuffPosition:{messageData.curBuffPosition},minBuffLen:{messageData.minBuffLen},allData:{string.Join(",", messageData.allData)}");
            // }
            //else
            //{
            //    Debug.Log($"{flagKey} socket:{messageData.socketName},AddMessageDataCMD:{messageData.cmd},serializeType:{messageData.serializeType},buffLength:{messageData.buffLength},dataLength:{messageData.dataLength},curBuffPosition:{messageData.curBuffPosition},minBuffLen:{messageData.minBuffLen},allData:{string.Join(",", messageData.allData)}");
            //}

            var messageHandleData = DeserializeMessageData(messageData);
            if (messageHandleData == null)
            {
                return;
            }

            // 如果不是 rpc 并且一些特殊逻辑, 就不放入 主线程处理的 messageDataQueue, 而是在其它网络线程收到时就立即处理
            // 比如时间同步, 收到后立即同步 服务器时间, 那这样就不会应该 主线程阻塞,而导致 客户端处理时间同步 延后
            switch (messageHandleData.messageCmd)
            {
                case (int)MsgIDEnum.ServerTimeRetID:
                case (int)CustomMsgID.HeartBeat:
                    {
                        PreHandleMessageHandleData(messageHandleData);
                        return;
                    }
                    break;
                default: break;
            }

            lock (mLock)
            {
                // 不是特殊消息, 那就放入 messageHandleDataQueue 队列, 等待后续执行
                messageHandleDataQueue.Enqueue(messageHandleData);
            }


        }

        public void AddMessageHandleData(MessageHandleData messageHandleData)
        {

            if (messageHandleData == null)
            {
                return;
            }

            //lock (messageHandleDataQueue)
            //{

            // 如果不是 rpc 并且一些特殊逻辑, 就不放入 主线程处理的 messageDataQueue, 而是在其它网络线程收到时就立即处理
            // 比如时间同步, 收到后立即同步 服务器时间, 那这样就不会应该 主线程阻塞,而导致 客户端处理时间同步 延后
            switch (messageHandleData.messageCmd)
            {
                case (int)MsgIDEnum.ServerTimeRetID:
                case (int)CustomMsgID.HeartBeat:
                    {
                        PreHandleMessageHandleData(messageHandleData);
                        return;
                    }
                    break;
                default: break;
            }


            lock (mLock)
            {
                // Debug.Log($"[message_{messageHandleData.enityId}] [{messageHandleData.messageCmd}] , {messageHandleData.messageName} , DeserializedData: {messageHandleData.DeserializedData} , data: {messageHandleData.data}");

                // 不是特殊消息, 那就放入 messageHandleDataQueue 队列, 等待后续执行
                messageHandleDataQueue.Enqueue(messageHandleData);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="target"></param> 消息注册的target，cocos的习惯，预留一个target，后续有需要在拓展
        private void OnMessageKey(string key, MessageHandle callback, object target)
        {
            if (key2MessageHandleDic.ContainsKey(key))
            {
                key2MessageHandleDic[key] += callback;
            }
            else
            {
                key2MessageHandleDic.Add(key, callback);
            }

            if (target != null)
            {

                if (!target2KeyMessage.TryGetValue(target, out List<KeyValuePair<string, MessageHandle>> messageHandles))
                {
                    messageHandles = new List<KeyValuePair<string, MessageHandle>>();
                    target2KeyMessage.Add(target, messageHandles);
                }

                messageHandles.Add(new KeyValuePair<string, MessageHandle>(key, callback));
            }
            // Debug.Log($"{flagKey} OnMessageKey : {key} ");
        }

        /// <summary>
        /// 【服务器自定义响应】
        /// target没用到
        /// </summary>
        /// <param name="cmd">自定义协议号/区别于PB的MSGID</param>
        /// <param name="callback"></param>
        /// <param name="target"></param>
        /// <param name="isGroup"></param> 是否监听消息组，就是不管服务器传过来enityId，都会响应
        public void OnMessageCmd(int cmd, MessageHandle callback, object target, bool isGroup = false)
        {
            string key = FormatKey(cmd, isGroup);
            OnMessageKey(key, callback, target);
        }

        public void OnMessageEnum(MsgIDEnum messageEnum, MessageHandle callback, object target, bool isGroup = false)
        {
            OnMessageCmd((int)messageEnum, callback, target, isGroup);
        }

        public void OffTargetMessage(object target)
        {
            if (target == null)
            {
                return;
            }

            // 如果没有这个 target 数据的时候,就不管
            if (!target2KeyMessage.TryGetValue(target, out List<KeyValuePair<string, MessageHandle>> messageHandles))
            {
                return;
            }

            messageHandles.ForEach((KeyValuePair<string, MessageHandle> messageHandleKeyPaire) =>
            {
                OffMessageKey(messageHandleKeyPaire.Key, messageHandleKeyPaire.Value, target);
            });

        }

        private void OffMessageKey(string key, MessageHandle callback, object target)
        {
            if (key2MessageHandleDic.ContainsKey(key))
            {
                key2MessageHandleDic[key] -= callback;
                if (key2MessageHandleDic[key] == null)
                {
                    key2MessageHandleDic.Remove(key);
                }
            }

            if (target == null)
            {
                return;
            }

            if (!target2KeyMessage.TryGetValue(target, out List<KeyValuePair<string, MessageHandle>> messageHandles))
            {
                return;
            }

            var idx = messageHandles.FindIndex((KeyValuePair<string, MessageHandle> messageHandleKeyPairs) =>
            {
                return messageHandleKeyPairs.Key == key && messageHandleKeyPairs.Value == callback;
            });

            if (idx != -1)
            {
                messageHandles.RemoveAt(idx);
            }
        }

        /// <summary>
        /// 取消cmd消息监听
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="callback"></param>
        /// <param name="target"></param>
        /// <param name="isGroup"></param>是否是消息组
        public void OffMessageCmd(int cmd, MessageHandle callback, object target, bool isGroup = false)
        {
            string key = FormatKey(cmd);
            OffMessageKey(key, callback, target);
        }

        public void OffMessageEnum(MsgIDEnum msgIDEnum, MessageHandle callback, object target, bool isGroup = false)
        {
            OffMessageCmd((int)msgIDEnum, callback, target, isGroup);
        }

        //反序列化自定义消息
        private bool DeserializeCustomMsg(MessageData messageData, MessageHandleData messageHandleData)
        {
            int cmd = messageData.cmd;

            Type type = CustomMsg.Instance.GetStructByCmd(cmd);
            ByteStream byteStream = new ByteStream();

            bool result = false;
            object data = byteStream.DeSerializeType(type, messageData.data, out result);

            if (result)
            {
                messageHandleData.data = data;
                messageHandleData.messageCmd = cmd;
                // 对于自定义的数据结构, 目前应该都是一层数据结构
                messageHandleData.DeserializedData = data;
            }
            else
            {
                Debug.LogError($"{flagKey} DeserializeCustomMsg cmd : {cmd} error!!");
            }
            return result;
        }


        private QAProto qAProto = new QAProto();
        /// <summary>
        /// sb 设置位 64 位, 目前最大长度 为 rpcCmd_messageCmd_enityId = 10 + 1 +10 +1 +20 = 42 位
        /// 其中 enityId 的字符串长度 20位 ulong.MaxValue = 18,446,744,073,709,551,615
        /// rpcCmd 和 messageCmd 的 字符串长度位 10 位: int.MaxValue = 2,147,483,647
        /// </summary>
        private StringBuilder sb = new StringBuilder(64);



        public void LockMessage(bool isLockMessage)
        {
            messageLock = isLockMessage;

            // Debug.LogError($"[Time] 锁消息 {messageLock}");
        }

        //资源都没准备好，处理没意义，所以锁定
        private bool messageLock = false;

        public bool IsMessageLock => messageLock;

        private bool IsFixMessage(int cmdID)
        {
            return cmdID == FixCmd;
        }

        private bool IsThirdMessage(int cmdID)
        {
            return cmdID == ThirdCmd;
        }

        private void ProcessMessageHandleQueue()
        {
            MessageHandleData messageHandleData = null;

            while (true)
            {
                if (messageHandleDataQueue.Count > 0 && !IsMessageLock)
                {
                    lock (mLock)
                    {
                        messageHandleData = messageHandleDataQueue.Dequeue();
                    }
                }
                else
                {
                    // 没有消息 或者锁住了, 那就直接跳出 这个循环
                    break;
                }
                if (messageHandleData != null)
                {
                    HandleMessageIntoSnapData(messageHandleData);
                }
            }
        }

        private MessageHandleData DeserializeMessageData(MessageData messageData)
        {
            byte[] buf = messageData.data;

            int encryptFlag = messageData.serializeType & 0x02;
            //表示字段已经加密，则需要解密
            if (encryptFlag > 0)
            {
                // Debug.Log($"{flagKey} handleMessageCS encryptFlag：{encryptFlag} need DecryptData: { KTool.BytesArrToString(buf)}");
                buf = MsgEncode.DecryptData(buf);
                // Debug.Log($"{flagKey} handleMessageCS encryptFlag：{encryptFlag} after DecryptData ---> {KTool.BytesArrToString(buf)}");
            }
            messageData.data = buf;

            int compressFlag = messageData.serializeType & 0x01;

            // Debug.Log($"{flagKey} handleMessageCS CMD : {messageData.cmd} compressFlag : {compressFlag} ");
            // Debug.Log($"{flagKey} handleMessageCS data : buf { KTool.BytesArrToString(buf)} ");

            MessageHandleData messageHandleData = new MessageHandleData();
            messageHandleData.socketName = messageData.socketName;

            //compressFlag == 0 有两层，一个走pb，一个走自定义。
            //服务器通过统一接口的方式，通过一个msgContent.Unmarshal 做到了统一解析
            //我这边跟服务器沟通出现了偏差，以为pb 和自定义会走两个 compressFlag 值
            //所以先通过cmd 是否在自定义协议集合，先把协议调通。

            //TODO: dl
            //服务器的通过多态来统一解的方式，我不太确实是否最优。
            //按我的想法，其实就应该根据 compressFlag 的不同值，来做不同的解析方式
            //但服务器的方式，代码更加整洁

            int cmd = messageData.cmd;
            bool isCustomMsg = CustomMsg.Instance.Contain(cmd);


            bool result = false;

            //表示自定义的msg 结构
            if (compressFlag == 0)
            {
                if (isCustomMsg)
                {
                    result = DeserializeCustomMsg(messageData, messageHandleData);
                }
                else
                {
                    result = ProtoUtils.DeserializePbMsg(messageData, messageHandleData);

                    if (result)
                    {
                        QASDKMsgUtils.SendServerMessage2QASDK(messageHandleData.isRPC, true, false, messageHandleData.messageCmd, (IMessage)messageHandleData.data);
                    }
                }
            }
            else if (compressFlag == 1)
            {
                //听海星的说法，此处服务器为冗余代码，serializeType只可能为0 或者 2
                //compressFlag 不可能为1
            }
            else
            {
                Debug.LogError($"{flagKey} handleMessageCS CMD : {messageData.cmd} has error compression flag");
                return null;
            }

            if (!result)
            {
                Debug.LogError($"{flagKey} handleMessageCS CMD : {messageData.cmd} 解析失败: {messageData.allData}");

                return null;
            }

            return messageHandleData;
        }

        private void HandleMessageIntoSnapData(MessageHandleData messageHandleData)
        {
            // 将收到的消息 数据作为快照数据,存入动态快照数据中

            {
                // 等待GameManager 在 update 中分帧处理。 
                // GameManager  在登录且地图后 还没startGame,导致不能再 update 执行. 所以 还是放在netWrokMessage中
                // GameManager.Instance.CacheMessageSnapData(messageHandleData);
            }
            // 缓存到本地 的 快照数据中
            CacheMessageSnapData(messageHandleData);
        }

        /// <summary>
        /// 处理 每一个 messageHandleData, 将数据 按不同的方式 推送到外部
        /// </summary>
        /// <param name="messageHandleData"></param>
        /// <param name="immediately">立即执行,不考虑消息锁, 一般只有 特殊的类似时间同步这种协议才需要立即执行</param>
        public void HandleMessageHandleData(MessageHandleData messageHandleData, bool immediately)
        {

            bool isRpc = messageHandleData.isRPC;
            int messageCmd = messageHandleData.messageCmd;

            //根据是否是 rpc 计算对应的 消息的key
            string key = isRpc ? FormatKey(RPCCmd, messageCmd, messageHandleData.enityId) : FormatKey(messageCmd);

            bool containKey = key2MessageHandleDic.ContainsKey(key);
            bool isFix = IsFixMessage(messageCmd);

            bool isThird = IsThirdMessage(messageCmd);
            if (isFix)
            {
                //差量更新
                FixMessageManager.Instance.HandleFixMessageCS(messageHandleData.data as DBUpUserDatasReq);
            }
            if (isThird)
            {
                // 服务器中转的第三方协议
                Client2ThirdMsgManager.Instance.HandleMessageCS(messageHandleData.data as Client2ThirdRet);
            }
            if (containKey)
            {
                if (key2MessageHandleDic.ContainsKey(key))
                {
                    key2MessageHandleDic[key]?.Invoke(messageHandleData);
                }

                else
                {
                    Debug.LogError($"key2MessageHandleDic not find key{key} {messageHandleData.messageName} {messageHandleData.messageCmd}");
                }
            }
            // Debug.Log($"{flagKey} entityID: {messageHandleData.enityId} handleMessageCS messageCmd : {messageHandleData.messageCmd} , messageName: {messageHandleData.messageName} , {messageHandleData.DeserializedData}");
            //如实是rpc消息的时候，判断是否有消息组，如果有消息组，则执行消息组的消息监听
            if (isRpc)
            {
                string groupKey = FormatKey(messageHandleData.messageCmd, true);
                bool contaionGroupKey = key2MessageHandleDic.ContainsKey(groupKey);

                //Debug.Log($"{flagKey} handleMessageCS CMD : {cmd} ,isRpc : {isRpc} , messageCmd : {messageHandleData.messageCmd} , groupKey : {groupKey} run group cb : {contaionGroupKey} time {TimeUtils.ServerNowStampMilli}");
                if (contaionGroupKey)
                {
                    key2MessageHandleDic[groupKey](messageHandleData);
                }
                GameManager.Instance.HandleRPCMsg(messageHandleData);

            }
            GameManager.Instance.OnRetMsg(messageHandleData.messageCmd);
        }

        #region 在收线程立即提前处理的 消息

        /// <summary>
        /// 收线程就立即处理的 消息
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="MessageHandle"></typeparam>
        /// <returns></returns>
        private Dictionary<int, MessageHandle> preStaticKey2MessageHandleDic = new();

        private StringBuilder preSb = new StringBuilder(64);

        /// <summary>
        /// 注册 收线程立即处理的 静态的消息监听,不需要取消监听
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="callback"></param>
        public void OnPreStaticMessage(int cmd, MessageHandle callback)
        {
            if (preStaticKey2MessageHandleDic.ContainsKey(cmd))
            {
                preStaticKey2MessageHandleDic[cmd] += callback;
            }
            else
            {
                preStaticKey2MessageHandleDic.Add(cmd, callback);
            }
        }
        /// <summary>
        /// 提前处理的几个消息
        /// </summary>
        /// <param name="messageHandleData"></param>
        public void PreHandleMessageHandleData(MessageHandleData messageHandleData)
        {

            int messageCmd = messageHandleData.messageCmd;

            bool containKey = preStaticKey2MessageHandleDic.ContainsKey(messageCmd);

            if (preStaticKey2MessageHandleDic.ContainsKey(messageCmd))
            {
                preStaticKey2MessageHandleDic[messageCmd]?.Invoke(messageHandleData);
            }
        }

        #endregion

        public Action<bool> OnSocketClose;

        public void OnGameSocketClose(bool isForceClose)
        {
            OnSocketClose?.Invoke(isForceClose);
        }


        public void OnGameSocketWeak()
        {
        }

        public void OnQuitGame()
        {
            Close();
        }


        public void OnMyUpdate()
        {
            try
            {
                // // 先将所有的 messageDataQueue 解析出来
                // PreDeserializeMessageDatas();

                // note:
                // 如果消息锁住之后, 缓存队列中存在了 一堆的消息, 服务器没有新的协议过来.
                // 此时, 如果通   while (messageDataQueue.Count > 0) 来处理， 由于 新的消息始终数量为 0, 虽然 messageLock 解开了, 缓存的数据也没法执行

                //var now = DateTime.Now;
                //Debug.LogError($"网络 帧 开始 :");
                //lock (messageDataQueue)
                //{
                ProcessMessageHandleQueue();
                //}
                //Debug.LogError($"网络 帧 结束 cost: {(now-lastTime).TotalMilliseconds}ms");
                //lastTime = DateTime.Now;

                // 处理快照数据
                InvokeMessageSnapDataCache();
            }
            catch (System.Exception e)
            {
                //Debug.LogError($"{flagKey} [OnMyUpdate] {messageData.cmd} {messageData.socketName} error!!!");
                // Debug.LogError($"{flagKey} [OnMyUpdate] Exception {e.Message}");
                // messageDataQueue.Clear();
                LockMessage(false);
                Debug.LogError($"{flagKey} [OnMyUpdate] Exception 清除消息队列,同时 解除锁定 {e.Message} , stack: {e.StackTrace}");
            }

        }

        /// <summary>
        /// 收到服务求消息的临时变量
        /// </summary>
        private SerMessageSnapData tempSerMessageSnapData;


        /// <summary>
        /// 缓存 消息数据作为 快照数据
        /// </summary>
        /// <param name="messageHandleData"></param> <summary>
        public void CacheMessageSnapData(MessageHandleData messageHandleData)
        {
            // 工厂创建缓存 收到的 消息快照数据
            tempSerMessageSnapData = DynamicDataFactory.InstanceData<SerMessageSnapData>();

            // 标记这个快照数据为 全数据.
            // note: 
            // 2024/1/2
            // 跟夏哥沟通如下:
            //     AOI 消息中的 IsEnd 变量用来标识 这一条 AOI消息是否完全发送完.
            //     看服务器代码, 如果 AOI 一条消息UpdateAOI.count>=20 条就会 分多条给客户端发.
            //     夏哥的意思, 这个变量 客户端不需要处理. 对于客户端来说, 消息的顺序是能够保障的。
            //     那基于此, 如果 一条消息分为 3条, 没必要分3帧处理。 收到了立即处理就可以了.
            tempSerMessageSnapData.IsFullMessage = true;

            // 快照数据赋值
            tempSerMessageSnapData.SnapMessageData = messageHandleData;

            // 移除引用，全部给工厂管理就可以了
            tempSerMessageSnapData = null;
        }

        private void InvokeMessageSnapDataCache()
        {
            int DealCount = GameManager.Instance.GetDealCountByAddSpeed<SerMessageSnapData>();
            if (DealCount == 0)
            {
                return;
            }
            // LogUtils.LogWarning(LogUtils.LogEnum.GameManagerTest, $"[GameManager] 准备 处理消息 count: {DealCount}-----------", DealCount > 10);
            for (int c = 0; c < DealCount && !NetworkManager.Instance.IsMessageLock; c++)
            {
                tempSerMessageSnapData = DynamicDataFactory.PopEarliestDataByRecorde<SerMessageSnapData>();
                if (tempSerMessageSnapData != null && tempSerMessageSnapData.SnapMessageData != null)
                {
                    // LogUtils.LogWarning(LogUtils.LogEnum.GameManagerTest, $"[GameManager] _rPCMsgList count: {SerMRSnapDataReaderRPC._rPCMsgList.Count}", SerMRSnapDataReaderRPC._rPCMsgList.Count > 20);
                    HandleMessageHandleData(tempSerMessageSnapData.SnapMessageData, false);
                    // LogUtils.LogWarning(LogUtils.LogEnum.GameManagerTest, $"[GameManager] _rPCMsgList 处理完成 cost: {TimeUtils.ClientNowStampMilli - now}", SerMRSnapDataReaderRPC._rPCMsgList.Count > 20);
                }
            }
            //Debug.Log($"属性同步 网络层下发 -===-=-=-=-=-=--=-=-=-=-=-=-=-=-=-=-=-==-=-=-= 处理数据  count={DealCount}");
        }
    }
}


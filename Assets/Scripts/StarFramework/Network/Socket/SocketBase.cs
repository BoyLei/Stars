using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using System;
using SGF.Network;
using System.Net.Sockets;
using SGF.Time;
using UnityEngine;
using Google.Protobuf;
using Unity.Mathematics;
using StarProject.Service.User;
using StarProjectDef;
using ZXing;

namespace SGF.Network
{
    public enum SocketViewState
    {
        None,
        /// <summary>
        /// 正在连接的菊花
        /// </summary>
        Connecting,
        /// <summary>
        /// 尝试重连的弹窗
        /// </summary>
        ReconnectBox,
        /// <summary>
        /// ping 弱网时，显示尝试重连的假弹窗, 只 做弹窗显示, 不关闭socket连接
        /// </summary>
        PingReconnectBox,
        /// <summary>
        /// 返回重新登录的弹窗
        /// </summary>
        RetrunLoginBox
    }

    public enum E_SocketState
    {
        None,
        VerifyFailed,
        NormalConnect,
        TryReConnecting,
        ConnnectFail,

        ReConnnectFail, // 重连失败

    }



    /// <summary>
    /// socket网络的中间层。主要是为了在断线重连的构成中，提取中间层控制网络切换
    /// 主要是为了避免 断线重连过程中，多线程切换导致状态管理十分麻烦的问题
    /// </summary>
    public class SocketBase
    {

        private string TagFlag => $"{TimeUtils.ClientUtcNow.ToString("mm:ss.fff")} [SocketBaseNew_{GetHashCode()}]";

        public Action<SocketViewState> OnSocketViewStateChange;

        private SocketViewState curSocketViewState = SocketViewState.None;


        /// <summary>
        /// socket 断开的通知, 每次socket 断开后， 会自动开启重连
        /// </summary>
        public Action<bool> OnSocketClose;


        public Action<E_SocketState> OnSocketStateChange;
        private E_SocketState curSocketState = E_SocketState.None;


        public bool IsConnceted
        {
            get
            {
                if (_curSocketItem != null && _curSocketItem.IsConnected)
                {
                    return true;
                }
                return false;
            }
        }


        private ulong _pid;
        private string _token;
        private string _ip;
        private int _port;
        private ProtocolType _protocolType;

        /// <summary>
        /// 当前的 socket 连接对象
        /// </summary>
        private SocketItem _curSocketItem;

        Queue<SendMsgData> sendMsgCache = new();  //未发送缓存

        Ping ping;
        public Ping CurPing => ping;

        public string SocketName;
        public SocketBase(string name)
        {
            SocketName = name;

            ping = new(this);
        }

        public void Start(string ip, int port, ulong pid, string token, ProtocolType protocolType = ProtocolType.Tcp)
        {
            _ip = ip;
            _port = port;
            _pid = pid;
            _token = token;
            _protocolType = protocolType;

            ConnectSocket();

            ping.Start();
        }
        public void Start(string url, ulong pid, string token, ProtocolType protocolType = ProtocolType.Tcp)
        {
            // 192.168.0.1:8080
            // 192.168.0.1:8080 // [2001:db8::1]:8080
            string[] ipPort = url.Split(':');
            if (ipPort.Length > 2)
            {
                //遇到IPV6的情况
                //正常就是0.1 两个
                //V6，我SOCKET选的是域名方式
                Debug.LogError($"[Socket] connect url: {url} 服务器给的 地址错误!!!");
            }

            Start(ipPort[0], Convert.ToInt32(ipPort[1]), pid, token, protocolType);
        }

        /// <summary>
        /// 重新开始网络连接
        /// </summary>
        public void ReStart()
        {
            _reconnectTimes = 0;
            _totalReconectTimes = 0;

            ReConnect();
        }

        /// <summary>
        /// 重置 ping 的状态显示
        /// </summary>
        public void ReStartPing()
        {
            curSocketViewState = SocketViewState.None;
            ping.Reset();
            ping.StartSendPing();
        }

        public void ReSetPing()
        {
            curSocketViewState = SocketViewState.None;
            ping.Reset();
        }

        private void ReConnect()
        {
            // 先关闭当前的 socketItem
            EndSocket(false);

            // 重新连接
            TryReConnect();
        }

        public void TestReconnect()
        {
            // 先关闭当前的 socketItem
            EndSocket(false);
            // 重新拿到 socket 的token 去重连
            TryReconnectSocketWithToken();
        }

        public void TestPing()
        {
            ping.TestPing = !ping.TestPing;
        }

        public void End()
        {
            EndSocket(false);

            ping.End();

            SwitchSocketState(E_SocketState.None);
        }



        private void ConnectSocket(bool isReConnect = false)
        {
            // 结束当前的 socket
            EndSocket();

            // 创建新的 socket
            _curSocketItem = SocketItem.Start(_pid, _token, _ip, _port, _protocolType, isReConnect);

            _curSocketItem.SocketName = _totalReconectTimes.ToString();
            // 注册状态 监听
            _curSocketItem.OnSocketClose += OnActionSocketClose;
            // _curSocketItem.OnSocketConnected += OnSocketConnected;

            _curSocketItem.OnReceiveMessage += OnReceiveMessage;
            _curSocketItem.OnTryGetSendMsgData += OnTryGetSendMsgData;


        }

        /// <summary>
        /// 主动关闭 socket. 那此时 不需要关心 socket 关闭后的装填通知
        /// </summary>
        private void EndSocket(bool reConnect = false)
        {
            // 如果当前的 socket 存在, 那先关闭之前的 socket
            if (_curSocketItem == null)
            {
                return;
            }

            var socketItem = _curSocketItem;

            // 先清空 _curSocketItem 的引用, 防止 End() 的回调过来的时候, _curSocketItem还存在
            _curSocketItem = null;

            // 结束当前的socket
            socketItem.End(reConnect);

            socketItem = null;

        }

        private void ClearOnrSocketEnd()
        {
            // 如果当前的 socket 存在, 那先关闭之前的 socket
            if (_curSocketItem == null)
            {
                return;
            }

            // 先清空 _curSocketItem 的引用, 防止 End() 的回调过来的时候, _curSocketItem还存在
            _curSocketItem = null;
        }

        private void SendMsgData(SendMsgData sendData)
        {
            lock (sendMsgCache)
            {
                sendMsgCache.Enqueue(sendData);
            }
        }

        private SendMsgData OnTryGetSendMsgData()
        {
            SendMsgData msg = null;
            lock (sendMsgCache)
            {
                while (sendMsgCache.Count > 0)
                {
                    msg = sendMsgCache.Dequeue();

                    // 如果数据解析失败了, 那就 msg设置为null, 继续找下一个
                    // 如果解析成功, 那 就找到了 需要发送的数据, 直接break
                    if (!msg.result)
                    {
                        msg = null;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return msg;
        }


        /// <summary>
        /// socket 收线程收到数据的处理逻辑
        /// note:
        ///     此处 不是主线程!!!
        /// </summary>
        /// <param name="messageHandleData"></param> 
        private void OnReceiveMessage(MessageHandleData messageHandleData, string tag)
        {
            CustomMsgID cmdEnum = CustomMsg.Instance.GetEnumByCmd(messageHandleData.messageCmd);
            switch (cmdEnum)
            {
                case CustomMsgID.ClientVerifySucceedRet:
                    {
                        OnVerifyResult(true);
                        NetworkManager.Instance.AddMessageHandleData(messageHandleData);
                    }
                    break;
                case CustomMsgID.ClientVerifyFailedRet:
                    {
                        OnVerifyResult(false);
                        NetworkManager.Instance.AddMessageHandleData(messageHandleData);
                    }
                    break;
                case CustomMsgID.HeartBeat:
                    {
                        ping.OnHeartBeatMsgID(messageHandleData, tag);
                    }
                    break;

                default:
                    {

                        NetworkManager.Instance.AddMessageHandleData(messageHandleData);
                    }
                    break;
            }
        }

        /// <summary>
        /// 收到socket 的验证回复消息
        /// </summary>
        private void OnVerifyResult(bool result)
        {
            // 验证失败, 说明服务器要关闭客户端的 socket, 客户端 不需要执行后续的重连逻辑
            // 需要跟服务器确认，如果服务器 发送给客户端 验证失败的通知,客户端 是否会收到 客户端被踢下线的 协议通知
            Loom.QueueOnMainThread((v) =>
            {
                if (!result)
                {
                    Debug.Log($"{TagFlag} OnVerifyResult 验证失败, 准备 断开 socket");

                    SwitchSocketState(E_SocketState.VerifyFailed);
                    return;
                }
                Debug.Log($"{TagFlag} OnVerifyResult 验证成功, 准备 发送");


                // StarProject.GlobalEvent.OnSocketVerifyResult?.Invoke(result);

                // 验证成功了, 就认为网络正常连接
                SwitchSocketState(E_SocketState.NormalConnect);
            }, null);

            /// 验证可能失败(未成年研制可能直接被服务器踢下线)
            /// 所以 心跳的开启 放在 socket 连接的回调中处理

        }

        private void SwitchSocketState(E_SocketState newState)
        {
            if (curSocketState == newState)
            {
                return;
            }
            curSocketState = newState;

            OnSocketStateChange?.Invoke(curSocketState);

            switch (curSocketState)
            {
                case E_SocketState.VerifyFailed:
                    {
                        End();
                        // 验证失败, 弹出 返回登录弹出
                        SwithSocketViewState(SocketViewState.RetrunLoginBox);
                    }
                    break;
                case E_SocketState.NormalConnect:
                    {
                        _curSocketItem.IsReConnect = false;
                        // 切换到正常网路状态后,表示连接已经成功, 重连次数可以清空
                        _reconnectTimes = 0;
                        SwithSocketViewState(SocketViewState.None);
                    }
                    break;
                case E_SocketState.TryReConnecting:
                    {
                        ping.ResetWeakPingCount();
                        SwithSocketViewState(SocketViewState.Connecting);
                    }
                    break;
                case E_SocketState.ConnnectFail:
                    {
                        SwithSocketViewState(SocketViewState.ReconnectBox);
                    }
                    break;

                case E_SocketState.ReConnnectFail:
                    {
                        ping.End();
                        SwithSocketViewState(SocketViewState.RetrunLoginBox);
                    }
                    break;

                default: break;
            }

        }

        /// <summary>
        /// 切换 socketViteState
        /// </summary>
        /// <param name="newState"></param>
        public void SwithSocketViewState(SocketViewState newState)
        {
            if (newState == curSocketViewState)
            {
                return;
            }

            curSocketViewState = newState;

            OnSocketViewStateChange?.Invoke(curSocketViewState);
        }




        #region  断线重连的相关处理
        private int _reconnectTimes = 0;
        /// <summary>
        /// 总的重连次数, 
        /// </summary>
        private int _totalReconectTimes = 0;

        /// <summary>
        /// 收到了 socket 断开的通知
        /// </summary>
        /// <param name="needReConnect">
        private void OnActionSocketClose(bool needReConnect, bool isReConnect)
        {
            if (!needReConnect)
            {
                // 如果是 自动重连途中, 在验证节点就发下连不上了, 那就
                if (isReConnect)
                {
                    OnReConnectFailed();
                }

                return;
            }

            // 重新拿到 socket 的token 去重连
            TryReconnectSocketWithToken();
        }

        /// <summary>
        /// 网络断开的监听, 断开后会尝试重连
        /// </summary>
        private void TryReConnect()
        {
            // 关闭 socket 的时候,先去 清除当前的 _curSocketItem
            ClearOnrSocketEnd();

            Debug.Log($"{TagFlag} TryReConnect 尝试重连, _reconnectTimes: {_reconnectTimes}, _totalReconectTimes: {_totalReconectTimes}");

            // 重连次数 达到最大次数, 关闭重连,
            if (_reconnectTimes >= SocketConstValue.MAX_RECONNECT_TIMES)
            {
                OnReConnectFailed();
                return;
            }

            SwitchSocketState(E_SocketState.TryReConnecting);

            // 重连次数加一
            _reconnectTimes++;
            _totalReconectTimes++;
            // ConnectSocket(true);

            DelayInvoker.DelayInvoke(0.5f, (args) =>
            {
                ConnectSocket(true);
            }, null);
        }

        private void OnReConnectFailed()
        {
            _reconnectTimes = 0;
            Debug.Log($"{TagFlag} TryReConnect 重连失败, 准备弹出 重连失败弹窗");

            // 重连失败的状态通知
            SwitchSocketState(E_SocketState.ConnnectFail);

        }

        #endregion


        private RPCMsg rpcMsg = new();

        /// <summary>
        /// // 【发送入口】【对服务器PB消息发送】【自定义消息发送】
        /// 发送RPC消息，服务器说除了自定义协议，所有的协议都要包装一层RPC（假的RPC, 是利用KV查找的，根本不是反射和通讯机制）
        /// </summary>
        /// <param name="serverType"></param>
        /// <param name="message"></param>
        /// <param name="isEncrypt"></param>
        /// <param name="oneOfCtrlEntityId">3位1体的，操作，其中某一个具体人的id，先留着</param>
        public void SendRPCMsg(ServerType serverType, IMessage message, Boolean isEncrypt = false, ulong oneOfCtrlEntityId = 0, bool isAutoChangeMsgTarget = true, bool isSend2QASDK = true)
        {
            ////Scene和Fb同一个级别的，Lobby不用动，互切
            if (isAutoChangeMsgTarget && (serverType == ServerType.ServerTypeInstance || serverType == ServerType.ServerTypeScene))
            {
                //服务器 已经处理了 没问题
                //serverType = SWM.IsCurrentInFB ? ServerType.ServerTypeInstance : ServerType.ServerTypeScene;
                serverType = ServerType.ServerTypeSpace;
                //lobby：
                //场景：你自己发也行，统一发3也行，都行
            }
            string messageName = message.GetType().Name;
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByName(messageName);
            if (!protoInfo.HasValue)
            {
                Debug.LogError($"{TagFlag} SendRPCMsg name : {messageName} has no protoinfo");
                return;
            }

            // byte serverType = protoInfo.Value.ServerType;
            int messageCmd = protoInfo.Value.cmd;

            rpcMsg.ServerType = (byte)serverType;
            rpcMsg.MethodName = protoInfo.Value.Name;
            rpcMsg.Data = SocketUtils.RPCMsgSerializer(message.ToByteArray(), messageCmd);

            //TODO:dl
            //发送实体id，目前使用pid暂时替换测试
            //那我发消息的时候，服务器知道（操作者本身的实体id）发送的移动。因为socket链接是唯一的呀
            //但是三位一体，控制其他宝宝移动的话。就需要发送我手下的被操纵者id了
            rpcMsg.SrcEntityID = 0;

            // Debug.Log($"{TagFlag}  SendRPCMsg name : {rpcMsg.MethodName}  ,cmd : {protoInfo.Value.cmd}");
            SendCustomMsg((int)CustomMsgID.RPCMsg, rpcMsg, isEncrypt, messageCmd);

            if (isSend2QASDK)
            {
                QASDKMsgUtils.SendClientRpcPb2QASDK(serverType, protoInfo.Value.cmd, message, isEncrypt, oneOfCtrlEntityId, isAutoChangeMsgTarget);
            }
        }


        private ProtoMsg.Client2CenterReq client2CenterReq = new();

        /// <summary>
        /// 发送 到 客户端中台中心的 消息
        /// </summary>
        /// <param name="message"></param>
        public void SendClient2CenterMsg(IMessage message)
        {
            string msgName = message.GetType().Name;

            client2CenterReq.MsgName = msgName;
            client2CenterReq.MsgData = message.ToByteString();

            SendEnumPbByteMsg(MsgIDEnum.Client2CenterReqID, client2CenterReq);
        }

        /// <summary>
        /// 通过消息枚举发送pb数据, 直接发送的是 byte[], 不需要再 内部 Serialize . 这个是发送第三方特定用的接口
        /// </summary>
        /// <param name="enumCmd"></param>
        /// <param name="message"></param>
        /// <param name="isEncrypt"></param>
        public void SendEnumPbByteMsg(MsgIDEnum enumCmd, IMessage message, Boolean isEncrypt = false)
        {
            int cmd = (int)enumCmd;
            // SendCustomMsg(cmd, message, isEncrypt);
            var sendData = SocketDataPools.Instance.GetMsgData(cmd, message.ToByteArray(), isEncrypt, cmd);

            SendMsgData(sendData);

        }

        public void SendCustomMsg(int cmd, object msg, Boolean isEncrypt = false, int messageCmd = 0)
        {

            SendMsgData sendData = SocketDataPools.Instance.GetMsgData(cmd, msg, isEncrypt, messageCmd);

            SendMsgData(sendData);
        }


        /// <summary>
        /// 尝试 获得新的 Token  重连socket
        /// </summary>
        private void TryReconnectSocketWithToken()
        {
            string GameServerIpAddress = GameLoginInfo.GameServerIpAddress;

            Debug.Log($"{TagFlag} TryReconnectSocketWithToken 尝试重连 获取 token");

            SwitchSocketState(E_SocketState.TryReConnecting);

            // HttpUtils.SendRetryLoginHeaderHttpReq<ChooseHeroReq, ChossHeroAck>(GameServerIpAddress, HttpUtils.CHOOSE_HERO_HANDLER, GameLoginInfo.M_ChooseHeroReq, OnChoseHeroAckCallBack, 2, 5);
            // 拿到角色数据, 开始重连socket
            TryReConnect();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="reqResult"></param>
        /// <param name="chossHeroAck"></param> 
        private void OnChoseHeroAckCallBack(ChossHeroAck chossHeroAck, bool reqResult)
        {
            // 请求 http 拿不到数据, 那弹窗返回登录
            if (!reqResult)
            {
                Debug.Log($"{TagFlag} TryReconnectSocketWithToken  获取 token 失败, 弹出返回登录 弹窗");

                SwitchSocketState(E_SocketState.ReConnnectFail);
                return;
            }

            GameLoginInfo.M_ChossHeroAck = chossHeroAck;
            // note: 2023/12/4 夏哥的 意思 result 返回 0 || 1 都表示成功, 不过此处 没有使用 result,所以先增加个备注
            bool result = GameLoginInfo.M_ChossHeroAck.Result > 1;

            // 角色数据获取失败, 也是放回登录界面
            if (result)
            {
                Debug.Log($"{TagFlag} TryReconnectSocketWithToken  获取 角色 失败, 弹出返回登录 弹窗");

                SwitchSocketState(E_SocketState.ReConnnectFail);
                return;
            }

            // 更新 token
            _token = chossHeroAck.Token;
            Debug.Log($"{TagFlag} TryReconnectSocketWithToken  获取 _token: {_token} , 准备重连");
            if (_token == null)
            {
                Debug.Log($"{TagFlag} TryReconnectSocketWithToken  获取 _token: {_token} 为null, 提示返回登录弹窗");

                SwitchSocketState(E_SocketState.ReConnnectFail);
                return;
            }
            // 拿到角色数据, 开始重连socket
            TryReConnect();
        }
    }
}

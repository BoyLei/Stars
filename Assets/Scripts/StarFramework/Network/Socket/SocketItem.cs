using System;
using System.Net;
using System.Net.Sockets;
using SGF.Network;
using SGF.Unity;
using UnityEngine;
using SGF.Time;
using ProtoMsg;
using System.Threading;
using System.Collections.Generic;

namespace SGF.Network
{
    public static class SocketConstValue
    {
        /// <summary>
        /// 连接超时时间
        /// </summary>
        public static float CONNECT_OVER_TIME = 3;

        /// <summary>
        /// 最大重连次数
        /// </summary>
        public static float MAX_RECONNECT_TIMES = 3;

        /// <summary>
        /// 最大总重连次数
        /// 用来防止 断线后连接上 重连次数被 清零后 socket 又断开的情况.限制了最大的总重连次数
        /// </summary>
        public static float MAX_TOTAL_RECONNECT_TIMES = 5;
    }

    public class SocketItem
    {
        private string TagFlag => $"{TimeUtils.ClientUtcNow.ToString("mm:ss.fff")} [SocketItem_{SocketName}] , curSocket: {curSocket?.GetHashCode()} ";

        /// <summary>
        /// 参数 标识 是否 需要重连
        ///     如果是 网络异常导致的关闭, 那需要走重连.
        ///     如果是 返回登录/服务器通知的 踢下线/验证消息失败, 都不需要重连
        /// </summary>
        public Action<bool, bool> OnSocketClose;
        public Action OnSocketConnected;

        public bool IsReConnect;
        /// <summary>
        /// socket 接收到的消息通知
        /// </summary>
        public Action<MessageHandleData, string> OnReceiveMessage;

        public Func<SendMsgData> OnTryGetSendMsgData;

        /// <summary>
        /// 由外部定义的 socketName
        /// </summary>
        public string SocketName;

        public bool IsConnected
        {
            get
            {
                if (curSocket == null)
                {
                    return false;
                }

                // socket 没 连接上
                if (!curSocket.Connected)
                {
                    return false;
                }

                // socket 如果线程关闭, 那也认为socket 关闭
                if (!socketThreadFlag)
                {
                    return false;
                }

                // 如果 连接还未走入回调, 那也不算连接成功
                if (!verifyFalg)
                {
                    return false;
                }

                return true;
            }
        }

        private Socket curSocket;
        private byte[] _tmpReceiveBuff = new byte[4096];
        private DataBuffer _databuffer = new();

        private int receiveLength = 0;
        private ClientVerifyReq clientVerifyReq = new();

        private string _ip = string.Empty;
        private int _port = 0;
        private ProtocolType _protocolType = ProtocolType.Tcp;

        private object _lock = new object();

        /// <summary>
        /// socket 线程的标志，由这个标志来标识 收发需要关闭 socket
        /// </summary>
        bool socketThreadFlag = false;

        /// <summary>
        /// 超时的标记
        /// </summary>
        bool overTimeFlag = false;

        /// <summary>
        /// 连接 回调的 标记
        /// </summary>


        /// <summary>
        /// 验证 成功的标志
        /// </summary>
        bool verifyFalg = false;

        /// <summary>
        /// 设置 socket 的验证数据, 每个 socket 连接后第一步都是先发送 验证消息。
        /// note:
        ///     只有验证通过后,才可以发送后续的协议. 
        ///     一旦收到服务器推送的验证失败,按夏哥的说法,表明客户端被服务器踢下线了。此时可以断就不需要重连了
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="token"></param> 
        private SocketItem(ulong pid, string token)
        {
            clientVerifyReq.Source = 0; //  Source: 消息来源, 分客户端或者服务器(ClientMSG/服务器类型)，客户端填0
            clientVerifyReq.PID = pid;
            clientVerifyReq.Token = token;
            clientVerifyReq.SessState = 1;  // SessState: 客户
        }

        private void Connect(string ip, int port, ProtocolType protocolType = ProtocolType.Tcp)
        {
            _ip = ip;
            _port = port;
            _protocolType = protocolType;
            StartSocket();
        }

        private void StartSocket()
        {

            socketThreadFlag = true;
            verifyFalg = false;
            overTimeFlag = false;

            // 设置连接成功 的标志之前, 先设置 消息验证的消息
            SetVerifyMsg();

            var socketThread = new Thread(new ThreadStart(OnSocketMainThread));
            socketThread.IsBackground = true;
            socketThread.Name = "socket_thread";
            socketThread.Start();


            // 开始一个超时关闭的定时器, 定时器需要在主线程设置
            DelayInvoker.DelayInvoke(GetHashCode(), SocketConstValue.CONNECT_OVER_TIME, OnConnectingOverTime, null);
        }

        public static SocketItem Start(ulong pid, string token, string ip, int port, ProtocolType protocolType = ProtocolType.Tcp, bool isReConnect = false)
        {
            // 创建新的 socket
            var _curSocketItem = new SocketItem(pid, token);

            _curSocketItem.IsReConnect = isReConnect;

            _curSocketItem.Connect(ip, port, protocolType);

            return _curSocketItem;
        }

        public void End(bool needReConnect = false)
        {
            Debug.Log($"{TagFlag} 由外部接口 End 关闭socket, 准备关闭socket, 需要重连: {needReConnect}");

            CloseSocket(needReConnect);
        }

        /// <summary>
        /// socket 的 创建/ 连接/ 接收/ 关闭 都放在 OnSocketMainThread 里面处理.
        /// 发送单独 放在 socketSend 线程中处理
        /// </summary>
        private void OnSocketMainThread()
        {
            // 首先开启连接
            BeginConnect();

            while (socketThreadFlag)
            {
                // 如果当前正在连接中, 那就 过段时间继续 看能不能收发
                if (!curSocket.Connected)
                {
                    Thread.Sleep(1);
                    continue;
                }

                // note: 
                //      收线程会阻塞 SocketMainThread 线程,所以如果 此处开启收线程, 那哪怕网络异常,  
                // 发线程中 关闭 socketThreadFlag, 此处也会因为 收线程阻塞, 而没办法进入后续的 CloseConnect 流程.
                // 所以此处 尝试 发送消息的 线程      
                if (!TrySendMsgData())
                {
                    // 发送失败, 可能没数据或者 发送消息异常, 那过一会再看能不能发
                    Thread.Sleep(20);
                    continue;
                }
                // 发送成功的话,那就每 1ms 去尝试下一次发送
                Thread.Sleep(1);
            }

            // 如果收发出现异常 或者 外部关闭 socketThreadFlag , 就会进入结束连接的阶段,里面会关闭socket
            CloseConnect();

        }

        private void BeginConnect()
        {
            curSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, _protocolType);//创建套接字
            Debug.Log($"{TagFlag} BeginConnect 准备开始 socket 异步连接,超时由 外部主线程设置");

            curSocket.SendTimeout = 1000;



            curSocket.BeginConnect(_ip, _port, new AsyncCallback(OnConnectCallBack), curSocket);//异步连接
        }

        private void CloseConnect()
        {
            Debug.Log($"{TagFlag} CloseConnect  start >>>>>>>>>>>>>>>>");

            var now = TimeUtils.ClientUtcNow;
            try
            {
                // 会确保所有的 
                curSocket.Shutdown(SocketShutdown.Both);
            }
            catch (System.Exception e)
            {
                Debug.Log($"{TagFlag} CloseConnect 关闭 Exception:  {e.Message}");
            }
            finally
            {
                Debug.Log($"{TagFlag} CloseConnect 准备释放 连接");


                curSocket.Close();
                curSocket.Dispose();
                Debug.Log($"{TagFlag} CloseConnect 释放 结束 , cost : {(TimeUtils.ClientUtcNow - now).Milliseconds} ms");
            }
            curSocket = null;

            Debug.Log($"{TagFlag} CloseConnect  end ------------------");

        }

        private bool TrySendMsgData()
        {
            SendMsgData msg = null;

            // 如果还未验证成功,  尝试发送 
            if (!verifyFalg)
            {
                // 如果已经发送了 验证消息, 但是还未收到验证成功的通知,那就 不发后续消息
                if (verifyMsg == null)
                {
                    return false;
                }

                // 如果有验证数据, 先发送验证数据
                msg = verifyMsg;
                verifyMsg = null;
            }
            else
            {

                msg = TryGetSendMsg();
            }

            // 没有需要发送的数据, 等一会再发
            if (msg == null)
            {
                return false;
            }

            try
            {
                // 尝试发送数据
                byte[] buf = SocketDataPools.Instance.FormateRetMsgData(msg);

                int sendCountResult = curSocket.Send(buf, buf.Length, SocketFlags.None);
                // Debug.Log($"{TagFlag} Send 发送线程 sendCountResult: {sendCountResult}, cmd: {msg.cmd} ,messageCmd: {msg.messageCmd} ");

                // 如果发送的字节数 为-1 ,表示发送失败
                if (sendCountResult < 0)
                {
                    Debug.Log($"{TagFlag} Send 发送线程 sendCountResult: {sendCountResult}, cmd: {msg.cmd} ,messageCmd: {msg.messageCmd} buf: {buf} 发送 消息失败, 准备断开 socket");
                    OnExceptionCloseConnect();

                    return false;
                }

                // Debug.Log($"{TagFlag} Send 发送线程 sendCountResult: {sendCountResult}, cmd: {msg.cmd}, messageCmd: {msg.messageCmd} 消息发送成功");

            }
            catch (System.Exception e)
            {
                // 如果发送发生了异常, 将数据
                Debug.Log($"{TagFlag}  Send 发送线程 cmd:{msg.cmd} 发生异常 , e: {e.Message}");
                OnExceptionCloseConnect();

                return false;
            }
            return true;
        }

        /// <summary>
        /// BeginConnect 连接的回调, 成功和 失败都会执行 这个回调.
        /// note:
        ///     1.基于.Net 异步编程模型机制，在主线程执行的 BeginConnect, 它的回调也会在 主线程执行;
        ///     2.成功和失败都会执行这个回调;
        ///     3.如果在 BeginConnect 的途中 取消,Socket关闭,  那么 回调也会执行. 同时 socket.EndConnect 会抛出一个异常,
        ///       可以用来识别这个 socket 连接是否已经被取消.
        /// </summary>
        private void OnConnectCallBack(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;


            Debug.Log($"{TagFlag} 进入 连接回调, 超时标志为 {overTimeFlag}, 先 结束 socket.BegineConnect");
            try
            {
                // BegineConnect 和 EndConnect 一一对应
                client.EndConnect(iar);
            }
            catch (System.Exception e)
            {
                // 如果 在 EndConnect 的时候发生异常, 那就直接 结束这个socket
                Debug.LogWarning($"{TagFlag} EndConnect 出现异常, 准备关闭 socket e: {e.Message}");
                OnExceptionCloseConnect();
                return;
            }

            if (overTimeFlag)
            {
                Debug.Log($"{TagFlag} 进入 连接回调超时, 因为不确定 连接回调何时执行,所有在超时 定时器中去关闭 socket, 此时回调不做任何事");
                return;
            }



            // 如果此时还未超时, 那就开启 send 线程 用来发送网络，进入了 socket连接成功的回调
            Debug.Log($"{TagFlag} 进入 连接成功的回调 , 准备开启 收 线程");

            var receiveThread = new Thread(new ThreadStart(OnReceiveThread));
            receiveThread.IsBackground = true;
            receiveThread.Name = "socket_ReceiveThread";
            receiveThread.Start();


            // 通知socket 连接成功, 此回调为 socket 的连接线程执行，不是主线程
            OnSocketConnected?.Invoke();
        }


        private void OnReceiveThread()
        {
            while (socketThreadFlag)
            {
                //如果在判断后 其他线程中cursocket被关闭 则会引发异常  所以此处需要加锁处理
                lock (_lock)
                {
                    // 如果当前正在连接中, 那就 过段时间继续 看能不能收发
                    if (curSocket == null || !curSocket.Connected)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    // 如果没有 接收数据发生了异常, 异常会关闭 socketThreadFlag 
                    // 此处可以直接 结束 ReceiveThread 
                    if (!TryReceiveMsgData())
                    {
                        return;
                    }
                }

            }

            Debug.LogWarning($"{TagFlag} 接收线程 结束!!!");
        }


        private bool TryReceiveMsgData()
        {
            try
            {
                receiveLength = curSocket.Receive(_tmpReceiveBuff);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"{TagFlag} 接收发生异常, receiveLength: {receiveLength}, 异常 e: {e.Message} ");

                OnExceptionCloseConnect();

                return false;
            }

            // 如果  Client.Receive（）== 0（简单便捷、资源不缺情况下可用该方法）, 也可以判断断开
            //  客户端连接状态有数据接收，Receive返回值 > 0。连接状态无数据接收，Receive会一直等待，不存在返回值的说明。
            //  断开状态下 Receive返回值 == 0。
            if (receiveLength == 0)
            {
                Debug.LogWarning($"{TagFlag} 接收receiveLength: {receiveLength}, Connected: {curSocket.Connected},准备断开 socket ");

                OnExceptionCloseConnect();

                return false;
            }

            // 如果接收正常, 继续执行后续逻辑
            if (receiveLength > 0)
            {
                _databuffer.AddBuffer(_tmpReceiveBuff, receiveLength);//将收到的数据添加到缓存器中
                                                                      //判断是否有一条完整数据
                while (_databuffer.ContainData())
                {
                    //取出一条完整数据
                    MessageHandleData messageHandleData = SocketUtils.GetMsgHandleData(_databuffer, SocketName);

                    // 没有数据的时候, 跳过本次 消息梳理循环
                    if (messageHandleData == null)
                    {
                        continue;
                    }
                    PreHandleMessage(messageHandleData);

                    // Debug.Log($"{TagFlag} receive 接收消息  messageHandleData: {messageHandleData.messageName}, cmd: {messageHandleData.messageCmd} 成功");
                    // 如果有数据, 那就预处理一次数据
                    OnReceiveMessage?.Invoke(messageHandleData, TagFlag);
                }
            }

            return true;
        }

        private void PreHandleMessage(MessageHandleData messageHandleData)
        {
            CustomMsgID cmdEnum = CustomMsg.Instance.GetEnumByCmd(messageHandleData.messageCmd);
            switch (cmdEnum)
            {
                case CustomMsgID.ClientVerifySucceedRet:
                    {
                        verifyFalg = true;
                    }
                    break;
                case CustomMsgID.ClientVerifyFailedRet:
                    {
                        // 验证失败 socketItem 不处理,交给上层
                        verifyFalg = false;
                    }
                    break;
            }

        }

        /// <summary>
        /// 主线程连接超时的回调
        /// </summary>
        private void OnConnectingOverTime(object[] args)
        {
            Debug.Log($"{TagFlag} 进入 OnConnectingOverTime 回调, 线程 标记 socketThreadFlag: {socketThreadFlag}");
            // 进入连接超时的 定时器callBack, 先判断 链接是否被关闭

            // 如果socket 线程的标志被关闭，所以socket 已经被执行了 关闭流程， 超时回调不需要执行
            if (!socketThreadFlag)
            {
                Debug.Log($"{TagFlag} 线程已经被关闭, 不需要做超时处理");
                return;
            }

            // 如果 在超时之前,先接收到 socket 连接的 回调, 那超时逻辑不需要处理任何逻辑
            if (verifyFalg)
            {
                Debug.Log($"{TagFlag} 在超时逻辑之前，已经收到了 验证成功的通知, 不需要做超时处理");
                return;
            }

            // 如果 超时逻辑执行时, 连接回调还未执行, 那就之前后续的超时断开逻辑
            overTimeFlag = true;

            Debug.Log($"{TagFlag} 连接超时, 准备 超时关闭socket");
            CloseSocket(true);
        }

        private SendMsgData TryGetSendMsg()
        {
            return OnTryGetSendMsgData?.Invoke();
        }

        /// <summary>
        /// 在异常的时候关闭 连接
        /// </summary>
        private void OnExceptionCloseConnect(bool needReConnect = true)
        {
            // 如果还未验证通 就收到了 接/收 的网络异常, 那只有两种情况:
            // 1. socket 连接被服务器踢掉,  或者未成年不让进？？ 客户端不重新连接socket
            // 2. 在等待验证的途中 断网, 理论上很难出现这种情况
            if (!verifyFalg)
            {
                Debug.LogWarning($"{TagFlag} 连接进入收发异常, 同时 验证消息 回复还未收到!!!");

                needReConnect = false;
            }
            Loom.QueueOnMainThread((v) =>
            {
                CloseSocket(needReConnect);
            }, null);
        }


        /// <summary>
        /// 关闭 socket 线程, 在收/发线程或者 玩家返回登录,都需要关闭 socket, 所以此处可能多个线程都进来
        /// </summary>
        private void CloseSocket(bool needReConnect)
        {
            if (!socketThreadFlag)
            {
                return;
            }

            // 关闭 socket 线程
            socketThreadFlag = false;

            OnSocketConnected = null;
            OnReceiveMessage = null;
            OnTryGetSendMsgData = null;

            // 先通知 外面socket 关闭, 再去 关闭 socket 线程
            OnSocketClose?.Invoke(needReConnect, IsReConnect);

            OnSocketClose = null;
            IsReConnect = false;
        }

        private SendMsgData verifyMsg = null;
        private void SetVerifyMsg()
        {
            // Debug.Log($"{TagFlag} 设置 clientVerifyReq token : {clientVerifyReq.Token}");

            verifyMsg = SocketDataPools.Instance.GetMsgData((int)CustomMsgID.ClientVerifyReq, clientVerifyReq, false);
        }
    }
}
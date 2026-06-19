using Google.Protobuf;
using SGF.Module.Framework;
using SGF.Time;
using SGF.Unity;
using StarProject.Module;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace SGF.Network
{

    public class UnSendCache
    {
        public int cmd;
        public byte[] data;

        public Boolean isEncrypt;

        public int messageCmd;

        public UnSendCache(int _cmd, byte[] _data, Boolean _isEncrypt, int _messageCmd = 0)
        {
            cmd = _cmd;
            data = _data;
            isEncrypt = _isEncrypt;
            messageCmd = _messageCmd;
        }
    }

    public enum SocketState
    {
        /// <summary>
        /// 默认状态
        /// </summary>
        Default,
        /// <summary>
        /// 尝试连接过程中
        /// </summary>
        TryConnecting = 10,         // 尝试连接socket, 其实还是无网络状态

        /// <summary>
        /// 连接超时
        /// </summary>
        ConnnectOutTime = 300,     // 连接超时状态, 后面连接失败或者 接收失败,都是连接失败的无网络状态
        /// <summary>
        /// 连接失败
        /// </summary>
        ConnnectFail,
        /// <summary>
        /// 接收线程 发生异常, 目前是数据 发生异常, 需要断开网络重连
        /// </summary>
        ReceiveError,


        /// <summary>
        /// 普通断开网络连接, 会执行自动断线重连
        /// </summary>
        DisConnected = 500,        // 网络断开状态,不管是 软断开还是强制断开,都是网络断开状态
        /// <summary>
        /// 强制断开网络连接,之后 不会自动断线重连
        /// </summary>
        ForceDisConnected,


        //------------接下来 都是连接状态，不管是不是弱网----------------------------------
        /// <summary>
        /// 连接成功
        /// </summary>
        Connected = 1000,           // 网络连接成功的状态, 对于后面 正常网络还是 弱网, 其实都是 网络连接状态
        /// <summary>
        /// 验证阶段
        /// </summary>
        Verify,                     // socket 连接后, 客户端会进入socket 的验证阶段, 如果直接从验证阶段 进入 socket 断开, 则需要弹出 返回登录弹出. 不需要后续的重新登录流程
        /// <summary>
        /// 验证成功阶段
        /// </summary>
        VerifySuccessed,
        /// <summary>
        /// 验证失败阶段
        /// </summary>
        VerifyFailed,
        /// <summary>
        /// 正常网络连接状态
        /// </summary>
        NormalConnect,

        /// <summary>
        /// 弱网连接 5s
        /// </summary>
        WeakConnect5s,
        /// <summary>
        /// 弱网连接 10s
        /// </summary>
        WeakConnect10s,
    }

    /// <summary>
    /// 发送的 socket 状态枚举
    /// </summary>
    public enum SocketNetStateEnum
    {
        /// <summary>
        /// 正常连接状态
        /// </summary>
        NormalConnect,
        /// <summary>
        /// 弱网状态
        /// </summary>
        WeakConnect,

        /// <summary>
        /// 重连状态
        /// </summary>
        ReConnect,
        /// <summary>
        /// 重连失败
        /// </summary>
        ReConnectFailed,
        /// <summary>
        /// 重连失败
        /// </summary>
        VerifyFailed,
    }

    public class SocketBase1
    {
        public string socketName;

        // private string TagFlag { get => $"[SocketBase] {TimeUtils.ClientUtcNowStr} 线程ID: {Thread.CurrentThread.ManagedThreadId} "; }
        private string TagFlag { get => $"[SocketBase]  线程ID: {Thread.CurrentThread.ManagedThreadId} "; }
        private string _currIP;
        private int _currPort;
        private ProtocolType _protocolType;

        private bool _isConnected = false;
        public bool IsConnceted { get { return _isConnected; } }
        private Socket curSocket = null;
        private Thread receiveThread = null;
        private Thread sendThread = null;

        private bool receiveFlag = false;
        private bool sendFlag = false;

        private DataBuffer _databuffer = new();

        byte[] _tmpReceiveBuff = new byte[4096];

        Queue<UnSendCache> msgCaches = new();  //未发送缓存

        /// <summary>
        /// socket 断开的通知, 每次socket 断开后， 会自动开启重连
        /// </summary>
        public Action<bool> OnSocketClose;
        /// <summary>
        /// socket 连接并且验证成功后的通知
        /// </summary>
        public Action OnSocketConnected;

        /// <summary>
        /// socket 网络状态变化的通知
        /// </summary>
        public Action<SocketNetStateEnum, object> OnSocketStateChanged;

        /// <summary>
        /// 发送消息 的成功/失败Action通知
        /// note:
        ///     发送成功只是意味着 数据被推入发送缓存区,并不意味着 数据发送给服务器.
        ///     所以服务器所谓的断线重连的差量同步,理论上是做不了的(夏哥1/5号也知道了)!!!
        /// </summary>
        public Action<int, bool> OnSendMsgAction;

        private SocketState curState = SocketState.Default;

        public bool CurIsConnect => (int)curState >= (int)SocketState.Connected;

        /// <summary>
        /// 总的重连次数, 
        /// </summary>
        private int totalReconectTimes = 0;
        /// <summary>
        /// 断线重连次数， 每次 重连成功后, 清零。
        /// </summary>
        private int reconnectTimes = 0;
        /// <summary>
        /// 最大的重连次数
        /// </summary>
        private int MAX_RECONNECT_TIMES = 3;


        /// <summary>
        /// 是否关闭自动重连, 正常 网络断开 都会自动重连, 
        /// 但是如果 重连次数 超过最大次数 或者 强制关闭 socket, 那就不 关闭自动重连
        /// </summary>
        private bool CloseAutoConnect = false;
        public SocketBase1(string name)
        {
            socketName = name;
            //TODO: dl
            //加一个游戏关闭的逻辑，后续要处理
            //  m_mgrFSP.onGameExit += DoClose
            UpdateSocketState(SocketState.Default);

        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="type"></param>
        public void Connect(string ip, int port, ProtocolType protocolType = ProtocolType.Tcp)
        {
            if (IsConnceted)
            {
                return;
            }

            _currIP = ip;
            _currPort = port;
            _protocolType = protocolType;
            ConnectScocket();
        }

        public void Connect(string url, ProtocolType protocolType = ProtocolType.Tcp)
        {
            //Debug.Log($"{TagFlag}  { TimeUtils.ClientUtcNowStr}   Connect url ${url}");
            string[] ipPort = url.Split(':');
            if (ipPort.Length > 2)
            {
                //遇到IPV6的情况
                //正常就是0.1 两个
                //V6，我SOCKET选的是域名方式
                Debug.LogError($"[Socket] connect url: {url} 服务器给的 地址错误!!!");
            }
            Connect(ipPort[0], Convert.ToInt32(ipPort[1]), protocolType);
        }

        public void DoReConnect()
        {
            reconnectTimes = 0;
            totalReconectTimes = 0;
            CloseAutoConnect = false;
            ReConnect();
        }

        /// <summary>
        /// 断线重连
        /// </summary>
        private void ReConnect()
        {

            if (CloseAutoConnect)
            {
                Debug.Log($"{TagFlag} 关闭了 自动重连, 不再继续重连");
                return;
            }
            // 重连次数, 每次 socket 断开后， 都会尝试断线重连, 重连次数 ++. 重连成功后, 重连次数清零.
            reconnectTimes++;
            // 总的重连次数, 重连成功后不会清理. 
            // note:
            //      增加最大重连次数 是因为可能出现一种情况 socket 断开， 自动重连成功. reconnectTimes 清零。
            //  然后服务器继续将 socket 断开， 导致客户端 又从 0 开始 断线重连.
            //  所以此处引入 总的 重连次数, 在 2倍 MAX_RECONNECT_TIMES 后, 断开 自动重连.  强制断开后, totalReconectTimes 清零

            totalReconectTimes++;

            /// 断开连接 目前 分为以下几种:
            /// 1.远程服务器断线，分为2中:
            ///     a.服务器将客户端 踢下线, 踢下线前会 通过协议告知, 这种情况下, 不需要 客户端重连;
            ///     b.网络线路断开的断线, 比如服务器重启, 服务器没有通过协议告知, 客户端需要 尝试重连;
            /// 2.客户端判断弱网超时 主动断线:
            ///     此时 客户端 需要重连;
            /// 3.socket 连接 失败/超时/收发消息异常 导致的 主动断线, 此时 客户端 需要重连

            // 如果重连次数 大于 最大重连次数, 那么断开连接, 同时 通知 外面自动重连无法连接
            if (reconnectTimes > MAX_RECONNECT_TIMES || totalReconectTimes > 4)
            {
                Debug.Log($"{TagFlag} 重连 reconnectTimes {reconnectTimes} ,totalReconectTimes: {totalReconectTimes} ,  后仍然失败, 强制断开 socket ");

                PostStateChangeEvent(SocketNetStateEnum.ReConnectFailed, 0);

                UpdateSocketState(SocketState.ForceDisConnected);
            }
            else
            {
                Debug.Log($"{TagFlag} 开启第 reconnectTimes {reconnectTimes}, totalReconectTimes: {totalReconectTimes} 次 重连 ");

                PostStateChangeEvent(SocketNetStateEnum.ReConnect, reconnectTimes);

                /// 执行重连时 都需要先 断开 socket(目前先不考虑 socket 复用)
                UpdateSocketState(SocketState.DisConnected);

                /// 如果在最大连接次数内, 那就 继续重连
                ConnectScocket();
            }

        }

        private void PostStateChangeEvent(SocketNetStateEnum socketNetStateEnum, object times)
        {
            Debug.Log($"{TagFlag} PostStateChangeEvent  {socketNetStateEnum}");

            OnSocketStateChanged?.Invoke(socketNetStateEnum, times);
        }

        public void Close()
        {
            UpdateSocketState(SocketState.ForceDisConnected);
        }

        /// <summary>
        /// 测试断线重连的 接口, 先主动断开socket,然后自动连接
        /// </summary>
        public void TestReConnect()
        {
            Close();
            DoReConnect();
        }

        /// <summary>
        /// 关闭 socket
        /// </summary>
        /// <param name="forceClose"></param>
        public void CloseSocket(bool forceClose = false)
        {

            Debug.Log($"{TagFlag} 执行 CloseSocket {curSocket?.GetHashCode()}");

            _isConnected = false;

            // 如果是强制关闭, 那就标记 自动重连 关闭
            if (forceClose)
            {
                CloseAutoConnect = true;
                // 总的 重连次数清零
                totalReconectTimes = 0;
            }

            // socket 断开, 清空 unsend 缓存
            ClearUnSendCache();

            receiveFlag = false;
            sendFlag = false;

            receiveThread = null;
            sendThread = null;


            if (curSocket != null)
            {
                Socket client = curSocket;
                Debug.Log($"{TagFlag} CloseSocket 关闭 开始 : {client.Connected}");
                var now = TimeUtils.ClientUtcNow;
                try
                {
                    // 会确保所有的 
                    client.Shutdown(SocketShutdown.Both);
                }
                catch (System.Exception e)
                {
                    Debug.Log($"{TagFlag} CloseSocket 关闭 socket Exception:  {e.Message}");
                }
                finally
                {
                    Debug.Log($"{TagFlag} CloseSocket 准备释放 socket 连接");


                    client.Close(0);
                    client.Dispose();
                    Debug.Log($"{TagFlag} CloseSocket {client.GetHashCode()} 关闭 结束 , cost : {(TimeUtils.ClientUtcNow - now).Milliseconds} ms");
                }
                curSocket = null;

                Debug.Log($"{TagFlag} CloseSocket  end ------------------");

                // 网络断开, 不管 是 强制断开还是 软断开
                OnSocketClose?.Invoke(forceClose);
            }

        }

        private void ConnectScocket()
        {
            try
            {
                if (curSocket != null)
                {
                    Debug.Log($"{TagFlag} 执行 ConnectScocket 时 curSocket: {curSocket.GetHashCode()} 不为null, 先关闭");

                    CloseSocket();
                }

                UpdateSocketState(SocketState.TryConnecting);

                curSocket = new Socket(SocketType.Stream, _protocolType);//创建套接字
                // 设置 receive 的超时时间
                // curSocket.ReceiveTimeout = 1000;

                Debug.Log($"{TagFlag} DoConnet ip: ${_currIP} , port {_currPort} , socket: {curSocket.GetHashCode()}");
                curSocket.BeginConnect(_currIP, _currPort, (IAsyncResult ar) =>
                {
                    OnLoomIAsyncResult(ar, OnConnectCallAsync);
                }, curSocket);//异步连接

                RegScocketOverTimeListener(curSocket);

            }
            catch (System.Exception e)
            {
                Debug.LogError($"{TagFlag} 连接 Socket 异常, Exception  error:  {e.Message}");
                OnConnectFail();
            }

        }

        /// <summary>
        /// 注册 socket 超时监听
        /// </summary>
        private void RegScocketOverTimeListener(Socket socket)
        {
            var hashCode = socket.GetHashCode();
            DelayInvoker.DelayInvoke(hashCode, 3, (args) =>
            {
                ConnectOutTimeCheck((Socket)args[0]);
            }, new object[] { curSocket });
        }

        /// <summary>
        /// 取消socket 的超时监听
        /// </summary>
        private void UnRegSocketOverTimeListener(Socket socket)
        {
            var socketHashCode = socket.GetHashCode();
            DelayInvoker.CancelInvoke(socketHashCode);
        }

        private void ConnectOutTimeCheck(Socket connectingSocket)
        {
            Debug.Log($"{TagFlag}    进入超时回调, checkSocket: {connectingSocket.GetHashCode()}, curSocket: {curSocket.GetHashCode()}");

            if (connectingSocket != curSocket)
            {
                Debug.LogError($"{TagFlag}    检查的socket 与 当前的 socket 不一致");
            }

            Debug.Log($"{TagFlag}    连接 Socket 超时检查 回调, 此时连接状态为: {connectingSocket.Connected} , 执行 socket 超时断开逻辑");
            OnConnectOuttime(connectingSocket);
        }


        /// <summary>
        /// 当 socket 的网络状态发生改变的时候
        /// </summary>
        public void ChangeSocketState(SocketState state)
        {

            if (curState == state)
            {
                return;
            }
            // 如果需要切换到 弱网状态, 需要判断 本地socket 是否是 连接状态, 如果不是连接状态, 就return
            if (state == SocketState.WeakConnect10s || state == SocketState.WeakConnect10s)
            {
                // 如果 此时 网络未连接， 那就return
                if ((int)curState < (int)SocketState.Connected)
                {
                    Debug.LogError($"{TagFlag} 改变 socket 状态 ChangeSocketState: curState: {curState} ---> {state} 失败, 此时网络断开");
                    return;
                }
            }

            // 如果当前网络已经被强制断开, 那么 切换到其它状态 需要 有一些特殊流程
            if (curState == SocketState.ForceDisConnected || curState == SocketState.DisConnected)
            {
                // 如果需要切换到  连接超时/连接失败/接收异常的状态, 同时, 此时又是已经被强制断开 ForceDisConnected, 则啥都不需要改
                if ((int)state >= (int)SocketState.ConnnectOutTime && (int)state <= (int)SocketState.ReceiveError)
                {
                    Debug.LogError($"{TagFlag} 改变 socket 状态 ChangeSocketState: curState: {curState} ---> {state} 不允许, 网络已经被强制断开");
                    return;
                }

                // 如果是由于心跳网络延迟, 此时 socket 已经被强制断开,  那么此时， 状态不能被心跳 设置为  NormalConnect

                if (state == SocketState.NormalConnect)
                {
                    Debug.LogError($"{TagFlag} 改变 socket 状态 ChangeSocketState: curState: {curState} ---> {state} 不允许, 网络已经被强制断开");

                    return;
                }
            }

            // 如果收到了 socket 的验证成功回调, 但是 当前又不是验证状态, 有可能
            if (state == SocketState.VerifySuccessed && curState != SocketState.Verify)
            {
                Debug.LogError($"{TagFlag} 改变 socket 状态 ChangeSocketState: curState: {curState} ---> {state} 不允许, 此时已经不是 验证状态");

                return;
            }

            Debug.Log($"{TagFlag} 改变socket 状态 ChangeSocketState: curState: {curState}  ----> {state}");
            UpdateSocketState(state);

        }

        private void UpdateSocketState(SocketState state)
        {
            if (curState == state)
            {
                return;
            }
            Debug.Log($"{TagFlag} 改变状态 {curState} ---> {state}");
            curState = state;

            switch (curState)
            {
                case SocketState.Connected:
                    {
                        /// 如果socket 连接成功, 那就自动发送 登录验证
                        /// 目前 服务器要求socket 连接成功之后 第一条消息必须为验证消息,
                        /// 所以第一条就发送 验证
                        if (IsConnceted)
                        {
                            Debug.Log($"{TagFlag} socket 连接成功, 准备切换到 verify 验证阶段");

                            CloseAutoConnect = false;
                            reconnectTimes = 0;
                            UpdateSocketState(SocketState.Verify);
                        }
                        else
                        {
                            Debug.Log($"{TagFlag} 进入 connected 状态, 但是 IsConnceted: {IsConnceted}, 有一丢丢问题");
                        }
                    }
                    break;
                case SocketState.Verify:
                    {
                        Debug.Log($"{TagFlag} 进入 Verify 状态,, 发送登录验证 ");

                        SendVerifyMsg();
                    }
                    break;
                case SocketState.VerifySuccessed:
                    {
                        Debug.Log($"{TagFlag} 进入 VerifySuccessed 状态, 准备切入 NormalConnect 状态 ");
                        UpdateSocketState(SocketState.NormalConnect);
                        OnSocketConnected?.Invoke();
                    }
                    break;
                case SocketState.VerifyFailed:
                    {
                        Debug.Log($"{TagFlag} 进入 VerifyFailed 状态, 准备断开socket, 弹出 返回登录 弹窗");
                        ChangeSocketState(SocketState.ForceDisConnected);

                        PostStateChangeEvent(SocketNetStateEnum.VerifyFailed, null);
                    }
                    break;
                case SocketState.NormalConnect:
                    {
                        PostStateChangeEvent(SocketNetStateEnum.NormalConnect, null);
                    }
                    break;
                case SocketState.WeakConnect5s:
                    {
                        // 如果是 超时5s 都没有收到回复, 说明网络拥堵, 通知外面
                        PostStateChangeEvent(SocketNetStateEnum.WeakConnect, 5);

                        // ReConnect();
                    }
                    break;
                case SocketState.WeakConnect10s:
                    {
                        // 2023/12/28
                        // 弱网超过10s,直接断掉socket 重连
                        ReConnect();

                        //        // 弱网 10s， 开启重连
                        //        OnSocketStateChanged?.Invoke(SocketNetStateEnum.WeakConnect, 10);

                    }
                    break;
                case SocketState.ConnnectFail:
                case SocketState.ConnnectOutTime:
                case SocketState.ReceiveError:
                    {
                        // 连接失败, 开启 重连, 多次重连 如果还是失败, 就强制断开连接
                        ReConnect();
                    }
                    break;
                case SocketState.DisConnected:
                    {
                        /// 断开连接 目前 分为以下几种:
                        /// 1.远程服务器断线，分为2中:
                        ///     a.服务器将客户端 踢下线, 踢下线前会 通过协议告知, 这种情况下, 不需要 客户端重连;
                        ///     b.网络线路断开的断线, 比如服务器重启, 服务器没有通过协议告知, 客户端需要 尝试重连;
                        /// 2.客户端判断弱网超时 主动断线:
                        ///     此时 客户端 需要重连;
                        /// 3.socket 连接 失败/超时/收发消息异常 导致的 主动断线, 此时 客户端 需要重连
                        CloseSocket(false);
                    }
                    break;
                case SocketState.ForceDisConnected:
                    {
                        CloseSocket(true);
                    }
                    break;

                default: break;
            };


        }

        public void OnSocketVerifyResult(bool result)
        {
            if (result)
            {
                ChangeSocketState(SocketState.VerifySuccessed);
            }
            else
            {
                ChangeSocketState(SocketState.VerifyFailed);
            }
        }

        /// <summary>
        /// socket 连接的回调
        /// note:
        ///     socket连接 超时或者 失败， 都会执行 连接回调， 因为他们不是一个 异步句柄. 所以在 连接回调中, 需要 防止重复处理 连接异常的情况
        /// </summary>
        /// <param name="iar"></param>
        private void OnConnectCallAsync(IAsyncResult iar)
        {
            // 取消socket 的超时监听 放在了 socket 的连接回调里

            Socket client = (Socket)iar.AsyncState;

            Debug.Log($"{TagFlag} OnConnectCallBack 进入 连接回调");

            UnRegSocketOverTimeListener(client);

            try
            {
                client.EndConnect(iar);
                Debug.Log($"{TagFlag} OnConnectCallBack 进入 连接成功的回调 ");

                if (curSocket != client)
                {
                    Debug.Log($"{TagFlag} OnConnectCallBack 进入 成功的回调,  callBack client: {client?.GetHashCode()}, curClient: {curSocket?.GetHashCode()}, 不对当前网络状态做处理");
                    return;
                }
                else
                {
                    Debug.Log($"{TagFlag} OnConnectCallBack 进入 连接成功的回调, 更新当前 socket 状态  ");
                    _isConnected = true;
                    UpdateSocketState(SocketState.Connected);

                    receiveThread = new Thread(new ThreadStart(() =>
                                          {
                                              receiveFlag = true;
                                              OnReceiveSocketThread(client);
                                          }));
                    receiveThread.IsBackground = true;
                    receiveThread.Start();
                    receiveThread.Name = "Socket-Receive";

                    sendThread = new Thread(new ThreadStart(() =>
                    {
                        sendFlag = true;
                        OnSendSocketThread(client);
                    }));
                    sendThread.IsBackground = true;
                    sendThread.Start();
                    sendThread.Name = "Socket-Send";
                }
            }
            catch (Exception e)
            {
                Debug.Log($"{TagFlag} OnConnectCallBack client : {client?.GetHashCode()} Exception:  {e.Message} ");

                /// note:
                ///     beginConnect 回调 在超时执行完 socket.Close 后依旧会执行,
                ///     所以 此处需要额外判断 当前的 curSocket == client .
                ///     以防止 失败回调 和 超时回调 都去重复执行 重连逻辑
                if (curSocket == client)
                {
                    Debug.Log($"{TagFlag} OnConnectCallBack 连接回调失败 client : {client?.GetHashCode()} , clientScoket: {curSocket?.GetHashCode()} ");

                    OnConnectFail();

                }
            }
        }

        private void OnLoomIAsyncResult(IAsyncResult result, Action<IAsyncResult> ac)
        {
            Loom.QueueOnMainThread((object v) =>
            {
                ac?.Invoke(result);
            }, null);
        }

        private void OnThreadExceptionChangeStage(int socketHashCode, SocketState socketState, string log)
        {
            Loom.QueueOnMainThread((object v) =>
            {
                // Debug.Log($"{TagFlag} {log} , loom 切换到 ----> {socketState}");
                if (curSocket != null && curSocket.GetHashCode() == socketHashCode)
                {
                    Debug.Log($"{TagFlag} {log} , loom 切换到 ----> {socketState}");
                    ChangeSocketState(socketState); //ReceiveFail
                }

            }, null);
        }



        /// <summary>
        /// 连接超时，目前是直接关闭连接
        /// </summary>
        private void OnConnectOuttime(Socket checkSocket)
        {

            Debug.Log($"{TagFlag} 进入超时回调, client: {checkSocket.GetHashCode()}, curClient: {curSocket?.GetHashCode()} ");

            if (curSocket != null && curSocket == checkSocket)
            {
                Debug.Log($"{TagFlag} OnConnectOuttime 超时, ChangeSocketState {SocketState.ConnnectOutTime} ");
                // 连接超时
                ChangeSocketState(SocketState.ConnnectOutTime);
            }
            else
            {
                Debug.LogError($"{TagFlag} OnConnectOuttime 超时不是当前 socket, 不做任何状态刷新 ");
            }
        }

        /// <summary>
        /// 连接失败，目前是直接关闭连接
        /// </summary>
        private void OnConnectFail()
        {

            Debug.Log($"{TagFlag} OnConnectFail , ChangeSocketState {SocketState.ConnnectFail}  ");
            // 连接失败, 直接切换socket 状态为 NoConnect,断开重新连接
            ChangeSocketState(SocketState.ConnnectFail); //Client Fail
        }


        /// <summary>
        /// 接受网络数据
        /// </summary>
        private void OnReceiveSocketThread(Socket client)
        {
            while (receiveFlag)
            {
                if (curSocket == null || client == null)
                {
                    Thread.Sleep(3);
                    return;
                }
                // socket 被关闭后, 接收线程就不处理了
                if (curSocket != client)
                {
                    Debug.LogError($"{TagFlag}   OnReceiveSocket client: {client.GetHashCode()}, curSocket: {curSocket?.GetHashCode()} 不一致, 不做 接收处理");
                    Thread.Sleep(3);
                    return;
                }

                try
                {
                    TryReceiveData(client);
                }
                catch (System.Exception e)
                {
                    // socket 的 接收是单独的 线程, 所以如果在 子线程中 直接改变socket 状态,会通知状态改变给 主线程.
                    // 所以 此处 先通知 NetworkManager socket 发生了异常, 由 它去主动关闭这个socket

                    // 由于 socket 的 receive 是单独的子线程, 直接在 子线程中改变 socket 的状态 可能会 调用断线重连的ui弹窗.
                    Debug.Log($"{TagFlag} OnReceiveSocketThread 接收线程发生异常, 准备断开socket  Exception:  {e.Message} , ");
                    // 将状态 更新为 接收数据异常状态
                    OnThreadExceptionChangeStage(client.GetHashCode(), SocketState.ReceiveError, "OnReceiveSocketThread 接收线程发生异常");

                    return;
                }
            }

            Debug.Log($"{TagFlag}   OnReceiveSocketThread 接收线程发生异常, 结束 while");
        }

        private void TryReceiveData(Socket socket)
        {
            var now = TimeUtils.ClientUtcNow;

            /// receive 默认 buffer 从0号位置 接收网络数据
            int receiveLength = socket.Receive(_tmpReceiveBuff);
            if (receiveLength > 0)
            {
                if (_databuffer != null)
                {

                    _databuffer.AddBuffer(_tmpReceiveBuff, receiveLength);//将收到的数据添加到缓存器中
                                                                          //判断是否有一条完整数据
                    while (_databuffer.ContainData())
                    {
                        var cost = (TimeUtils.ClientUtcNow - now).Milliseconds;
                        if (cost > 2)
                        {
                            Debug.Log($"{TagFlag}   OnReceiveSocketThread 收到一个 数据, cost {cost} ms");
                        }

                        //取出一条完整数据
                        MessageData msgData = SocketUtils.GetMsgData(_databuffer);
                        if (msgData != null)
                        {
                            msgData.socketName = socketName;
                            NetworkManager.Instance.AddMessageData(msgData);
                        }
                    }
                }
            }
            Thread.Sleep(1);
        }

        private bool TryGetSendMsg(Socket client, out UnSendCache sendMsg)
        {
            sendMsg = null;

            if (curSocket != client)
            {
                return false;
            }
            if (!IsConnceted)
            {
                return false;
            }
            if (curSocket == null || !curSocket.Connected)
            {
                return false;
            }
            lock (msgCaches)
            {
                // 如果取了第一个 msgCache 是null，那就继续往下找
                while (sendMsg == null && msgCaches.Count > 0)
                {
                    sendMsg = msgCaches.Dequeue();
                }
            }

            if (sendMsg == null)
            {
                return false;
            }

            return true;
        }

        private void OnSendSocketThread(Socket client)
        {
            while (sendFlag)
            {
                // 检查是否有 需要send 的msg, 如果没有的话就 continue, 同时 sleep 防止 因为lock(msgCaches) 而阻塞住主线程 心跳的发送
                if (!TryGetSendMsg(client, out UnSendCache msg))
                {
                    Thread.Sleep(3);
                    continue;
                }

                try
                {
                    var now = TimeUtils.ClientUtcNow;
                    byte[] buf = SocketUtils.FormateData(msg.cmd, msg.data, msg.isEncrypt);

                    int sendCount = client.Send(buf, buf.Length, SocketFlags.None);

                    OnSendMsgAction?.Invoke(msg.cmd, true);

                    var cost = (TimeUtils.ClientUtcNow - now).Milliseconds;
                    if (cost > 2)
                    {
                        Debug.Log($"{TagFlag}   OnSendSocketThread send {sendCount} 数据, cost {cost} ms");
                    }

                }
                catch (System.Exception e)
                {
                    OnSendMsgAction?.Invoke(msg.cmd, false);
                    Debug.Log($"{TagFlag} OnSendSocketThread 发送消息 发生异常 , e: {e.Message}");

                    OnThreadExceptionChangeStage(client.GetHashCode(), SocketState.ReceiveError, "OnSendSocketThread 发送消息异常");
                    return;
                }

                Thread.Sleep(1);
            }

            Debug.Log($"{TagFlag}   OnSendSocketThread 发送消息异常, 结束 while");

        }


        /// <summary>
        /// 发送消息的base方法，【自定义发送】
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <param name="isEncrypt"></param> 是否加密
        /// <param name="messageCmd">实际发送的 messageCmd , 对于自定义的rpcMsg, 所有协议都是通过rpcMsg封装, 所以cmd 一致, 而 messageCmd 是实际发送的消息号</param>
        private void SendMsgBase(int cmd, byte[] data, Boolean isEncrypt, int messageCmd = 0)
        {
            SaveMsgCache(cmd, data, isEncrypt, messageCmd);
        }

        private void SaveMsgCache(int cmd, byte[] data, Boolean isEncrypt, int messageCmd = 0)
        {
            lock (msgCaches)
            {
                UnSendCache unSendCache = new(cmd, data, isEncrypt, messageCmd);

                msgCaches.Enqueue(unSendCache);
            }
        }


        private void ClearUnSendCache()
        {
            lock (msgCaches)
            {
                msgCaches.Clear();
            }
        }

        /// <summary>
        /// 通过消息枚举发送pb数据，这个协议接口不暴露，因为所有的pb协议都会走一层RPC封装
        /// </summary>
        /// <param name="enumCmd"></param>
        /// <param name="message"></param>
        /// <param name="isEncrypt"></param>
        public void SendEnumPbMsg(MsgIDEnum enumCmd, IMessage message, Boolean isEncrypt = false)
        {
            int cmd = (int)enumCmd;
            SendMsgBase(cmd, message.ToByteArray(), isEncrypt);
        }


        /// <summary>
        /// 【对服务器自定义发送】
        /// 发送自定义协议，自定义的协议通过这个这个接口发送，如心跳、服务器登入确认等
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="msg"></param>
        /// <param name="isEncrypt"></param>
        /// <param name="isSocketVerify">是否是 socket 验证消息</param>
        public void SendCustomMsg(int cmd, object msg, Boolean isEncrypt = false, int messageCmd = 0)
        {

            ByteStream byteStream = new();
            bool result = byteStream.Serialize(msg);
            if (!result)
            {
                //Debug.Log($"{TagFlag}  { TimeUtils.ClientUtcNowStr}   SendCustomMsg cmd : {cmd} result {result}");
                return;
            }
            byte[] data = byteStream.data;
            //Debug.Log($"{TagFlag}  { TimeUtils.ClientUtcNowStr}   SendCustomMsg cmd : {cmd} ");

            if (data.Length == 0)
            {
                //Debug.Log($"{TagFlag}  { TimeUtils.ClientUtcNowStr}   SendCustomMsg cmd : {cmd} has no msg");
                return;
            }
            SendMsgBase(cmd, data, isEncrypt, messageCmd);
        }

        private object[] verifyMsg = new object[3];
        /// <summary>
        /// 验证消息,需要保证socket 第一条发送, 在socket 连接之前 设置, 连接之后 自动发送
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="msg"></param>
        /// <param name="isEncrypt"></param>
        public void SetSocketVerifyMsg(int cmd, object msg, Boolean isEncrypt = false)
        {
            verifyMsg[0] = cmd;
            verifyMsg[1] = msg;
            verifyMsg[2] = isEncrypt;
        }

        /// <summary>
        /// 发送socket 的第一条验证消息
        /// </summary>
        private void SendVerifyMsg()
        {
            if (verifyMsg[0] == null)
            {
                Debug.Log($"{TagFlag} 发送验证消息为空, 想想是不是没设置登录验证消息");
                return;
            }
            Debug.Log($"{TagFlag}  发送验证消息");
            ClearUnSendCache();
            SendCustomMsg((int)verifyMsg[0], verifyMsg[1], (Boolean)verifyMsg[2], 0);
        }

        private RPCMsg rpcMsg = new();
        public StarWorldModule SWM
        {
            get
            {
                return ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
            }
        }

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
                Debug.Log($"{TagFlag} SendRPCMsg name : {messageName} has no protoinfo");
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

        /// 【Lua对服务器PB消息发送】
        public void SendRPCMsgLua(int cmd, byte[] data, Boolean isEncrypt = false)
        {

            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(cmd);
            if (!protoInfo.HasValue)
            {
                //Debug.Log($"{TagFlag}  { TimeUtils.ClientUtcNowStr}   SendRPCMsg name : {name} has no protoinfo");
                return;
            }

            byte serverType = protoInfo.Value.ServerType;
            int messageCmd = protoInfo.Value.cmd;

            rpcMsg.ServerType = serverType;
            rpcMsg.MethodName = protoInfo.Value.Name;
            rpcMsg.Data = SocketUtils.RPCMsgSerializer(data, messageCmd);

            //TODO:dl
            //发送实体id，目前使用pid暂时替换测试
            rpcMsg.SrcEntityID = GameLoginInfo.M_ChooseHeroReq.PID;

            //Debug.Log($"{TagFlag}  { TimeUtils.ClientUtcNowStr}   SendRPCMsgLua name : {rpcMsg.MethodName}  ,cmd : {protoInfo.Value.cmd}");
            SendCustomMsg((int)CustomMsgID.RPCMsg, rpcMsg, isEncrypt, messageCmd);
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

            SendEnumPbMsg(MsgIDEnum.Client2CenterReqID, client2CenterReq);
        }

    }
}

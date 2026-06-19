using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using SGF.Network;
using SGF.Time;
using SGF.Unity;
using UnityEngine;

namespace SGF.Network
{
    /// <summary>
    /// ping 相关的几个静态常量
    /// </summary>
    public static class PingValue
    {

        /// <summary>
        /// ping 需要的样本数, 计算ping 的时候 采用 PING_VALID_COUNT 有效的 ping 计算
        /// 比如 样本数 20个, 但是只采用 最新的 5个
        /// </summary>
        public static int PING_COUNT = 20;

        /// <summary>
        /// 计算 平均ping 需要的有效样本数
        /// </summary>
        public static int PING_VALID_COUNT = 5;

        /// <summary>
        /// ping 的标准刷新间隔
        /// </summary>
        public static int PING_REFRESH_INTERVAL = 1000;
        /// <summary>
        /// ping 的 网络波动 的时候 刷新间隔
        /// </summary>
        public static int PING_REFRESH_INTERVAL_NET_CHANG = 50;

        /// <summary>
        /// ping 波动改变 刷新频率 的波动值, 现在 超过 50ms 就改变刷新频率
        /// </summary>
        public static int PING_WAVE_LIMIT = 50;

        /// <summary>
        /// ping 网络波动 最多的 累计次数， 超过这个次数， 判定网络异常，返回登录弹窗
        /// </summary>
        public static int PING_WAVE_MAX_COUNT = 1000;

        /// <summary>
        /// ping 进入 正常状态的次数
        /// </summary>
        public static int PING_ENTER_NORMAL_COUNT = 3;

        /// <summary>
        /// 弱网 ping 进入菊花状态的 次数
        /// </summary>
        public static int PING_ENTER_CONNECTING_COUNT = 5;

        /// <summary>
        /// ping 进入 网络连接断开状态的 次数
        /// </summary>
        public static int PING_ENTER_DISCONNECT_COUNT = 12;


        /// <summary>
        /// 弱网 定义的 ping-pong 基础值 
        /// </summary>
        public static int PING_PONG_WEAK_NET_TIME = 460 * 2;

        /// <summary>
        /// 弱网 等待ping 的单次 最大时间, 超过需要 提示 网络重连
        /// </summary>
        public static int PING_PONG_WAIT_TIME = 5000;

    }
    public enum E_SocketPingState
    {
        None,
        Weaking,
        TryReconnect,
        ReturnLogin
    }



    public class Ping
    {

        private string TagFlag => $"{TimeUtils.ClientUtcNow.ToString("mm:ss.fff")} [Socket_ping_{GetHashCode()}]";

        private SocketBase socketBase;

        /// <summary>
        /// 发送心跳的标脏值,对应如下:
        /// 1.0x1 标识 客户端 发送;
        /// 2.0x2 标识 收到心跳回复的 标脏;
        /// 3.0x4 标识 定时器触发
        /// 
        /// 综上: 只有 (dirtyFlag == 0x1 | 0x2 | 0x4) || (dirtyFlag== 0) 才表示 需要发送心跳
        /// </summary>
        private uint dirtyFlag = 0;

        /// <summary>
        /// 记录等待ping 时间的临时变量
        /// </summary>
        private float tempWaitPinMsgTime = 0;
        private int tempWaitPinCount = 0;

        private bool curIsWeekPing = false;

        private LinkedList<int> pingsList = new();


        private long sendPingTime = 0;

        private int AveragePing
        {
            get => GetCurAveragePing();
        }

        private long refreshPingTime = 0;

        private int refreshPingInterval = PingValue.PING_REFRESH_INTERVAL;

        private HeartBeat heartBeat = new HeartBeat();  // 自定义心跳

        /// <summary>
        /// 发送ping 的标志
        /// </summary>
        private bool sendPingFlag = false;

        private object _lock = new object();

        public Ping(SocketBase _socketBase)
        {
            socketBase = _socketBase;
            socketBase.OnSocketStateChange += OnSocketStateChange;
        }

        public void Start()
        {
            // 开启心跳的定时器
            StartScheduleHeartBeat();
        }

        public void Reset()
        {
            // Debug.LogError($"{TagFlag} ping Reset");

            sendPingFlag = true;
            cur_pingState = E_SocketPingState.None;

            // 清空发送ping 的时间, 重新开始新一轮 ping 超时计算
            SetSendPingTime(TimeUtils.ClientNowStampMilli);

            ResetTotalWeakPingCount();
            ClearPingList();
        }

        public void End()
        {
            // Debug.Log($"{TagFlag} ping end");
            sendPingFlag = false;

            SetDirtyFlag(0);
            SetSendPingTime(0);
            ResetTotalWeakPingCount();
            ClearPingList();
        }

        private void SetDirtyFlag(uint flag, bool isSet = true)
        {
            lock (_lock)
            {
                if (isSet)
                {
                    dirtyFlag = flag;
                }
                else
                {
                    dirtyFlag |= flag;
                }
            }
        }

        private uint GetDirtyFlag()
        {
            uint flag = 0;
            lock (_lock)
            {
                flag = dirtyFlag;
            }
            return flag;
        }

        private void SetSendPingTime(long time)
        {
            lock (_lock)
            {
                sendPingTime = time;
            }
        }

        private long GetSendPingTime()
        {
            long time = 0;
            lock (_lock)
            {
                time = sendPingTime;
            }
            return time;
        }
        public void ResetTotalWeakPingCount()
        {
            totalWeakPingCount = 0;
        }

        /// <summary>
        /// 只清除 ping 弱网次数 
        /// </summary> <summary>
        /// 
        /// </summary>
        public void ResetWeakPingCount()
        {
            sendPingFlag = false;
            SetDirtyFlag(0);
            SetSendPingTime(0);
            ClearPingList();
        }

        private void OnSocketStateChange(E_SocketState socketState)
        {
            switch (socketState)
            {
                case E_SocketState.None:
                case E_SocketState.VerifyFailed:
                case E_SocketState.ConnnectFail:
                    {
                        End();
                    }
                    break;
                case E_SocketState.NormalConnect:
                    {
                        StartSendPing();
                    }
                    break;

                default: break;
            }
        }

        public void StartSendPing()
        {
            sendPingFlag = true;
            // Debug.Log($"{TagFlag} ping start");

            SetDirtyFlag(0);
            SetSendPingTime(0);
            ClearPingList();

            // 一开始直接 立即发送一个心跳
            TrySendHeartBeatMsg();
        }

        private void StartScheduleHeartBeat()
        {
            // 增加定时器,定时发送
            MonoHelper.AddFixedUpdateListener(OnPingUpdate, MonoHelper.E_ModuleType.CommonService);

        }

        private void StopStartHeartBeat()
        {
            MonoHelper.RemoveFixedUpdateListener(OnPingUpdate, MonoHelper.E_ModuleType.CommonService);
        }

        /// <summary>
        /// 定时去发送心跳
        /// </summary>
        private void Schedule2SendHeartBeatMsg()
        {
            // 标识定时器 到了, 如果已经 发送并且收到了回复, 那就立即触发 心跳的发送
            SetDirtyFlag(0x4, false);

            // SGF.Debuger.Log($"[socket], 定时器启动 , dirtyFlag: {dirtyFlag} ");
            TrySendHeartBeatMsg();
        }

        /// <summary>
        /// 尝试 发送心跳, 在收到 服务器回复/固定间隔时间 发送的时候 都尝试去发送 心跳
        /// </summary>
        private void TrySendHeartBeatMsg()
        {

            /// 0 表示未发送过 socket , 0x3 表示 客户发送并且服务器回复
            uint curDirtyFlag = GetDirtyFlag();
            bool dirtySendHert = curDirtyFlag == 0 || curDirtyFlag == 0x7;
            //SGF.Debuger.Log($"[socket], 尝试发送 心跳, dirtyFlag: {dirtyFlag} , dirtySendHert: {dirtySendHert}");
            if (!dirtySendHert)
            {
                return;
            }
            // SGF.Debuger.Log($"[socket], 尝试发送 心跳, dirtyFlag: {dirtyFlag} , dirtySendHert: {dirtySendHert}");

            SendHeartBeatMsg();
        }

        // 直接发送心跳协议
        private void SendHeartBeatMsg()
        {
            // SGF.Debuger.Log($"[socket], 发送 心跳 ------>>>> {TimeUtils.ClientNowStampMilli}");
            // 发送完心跳后, 标识 状态未 0x1
            SetDirtyFlag(0x1);
            SetSendPingTime(TimeUtils.ClientNowStampMilli);
            socketBase.SendCustomMsg((int)CustomMsgID.HeartBeat, heartBeat);
        }

        public bool TestPing = false;

        /// <summary>
        /// 收到ping 的消息回复
        /// </summary>
        /// <param name="data"></param>
        public void OnHeartBeatMsgID(MessageHandleData data, string tag)
        {
            if (TestPing)
            {
                return;
            }
            // 标识 收到心跳回复

            SetDirtyFlag(0x2, false);


            // var cost = TimeUtils.ClientNowStampMilli - sendPingTime;

            //心跳包，时刻(1s)刷新平均的，网络延迟
            OnNewPingRet();
            // SGF.Debuger.Log($"{tag} ,收到 心跳回复 , dirtyFlag: {dirtyFlag} ---------<<<<<<<< {TimeUtils.ClientNowStampMilli} , cost {cost} ms , ping: {TimeUtils.AveragePing}");

            TrySendHeartBeatMsg();
        }

        private void OnNewPingRet()
        {
            var curSendPingTime = GetSendPingTime();
            int curPing = (int)(TimeUtils.ClientNowStampMilli - curSendPingTime);

            SetSendPingTime(0);

            UpdateCurPing(curPing);

            RefreshPing();
        }

        private int sumPing = 0;
        private int pingCount = 0;
        private int pingLast = 0;

        /// <summary>
        /// 弱网 ping 的次数
        /// </summary>
        private int totalWeakPingCount = 0;

        /// <summary>
        /// 当前总的 弱网 ping 数量
        /// </summary>
        public int CurTotalWeakPingCount => totalWeakPingCount;

        /// <summary>
        /// pingList 中之前连续弱网次数
        /// </summary>
        private int preContinueWeakCount = 0;
        public int PreContinueWeakCount => preContinueWeakCount;

        private int preContinueNomalPingCount = 0;
        /// <summary>
        /// pingList 中之前连续正常次数
        /// </summary>
        public int PreContinueNomalPingCount => preContinueNomalPingCount;

        /// <summary>
        /// 当前等待 ping 的时间
        /// </summary>    
        public float CurWaitTime => tempWaitPinMsgTime;

        /// <summary>
        /// 当前等待 ping 的时间换算的 弱网次数
        /// </summary>
        public int CurWaitWeakPingTimes => (int)(curIsWeekPing ? tempWaitPinMsgTime / PingValue.PING_PONG_WEAK_NET_TIME : 0);

        private void UpdateCurPing(int curPing)
        {
            lock (pingsList)
            {
                // 收到了 两次返回,  过滤
                if (curPing == 0)
                {
                    return;
                }
                // 上一次的 ping
                var lastPing = pingsList.Count > 0 ? pingsList.Last.Value : 0;


                if (pingsList.Count >= PingValue.PING_COUNT)
                {
                    pingsList.RemoveFirst();
                }
                pingsList.AddLast(curPing);


                var last = pingsList.Last;

                sumPing = 0;
                int validCount = 0;
                while (last != null && validCount < PingValue.PING_VALID_COUNT)
                {
                    sumPing += last.Value;
                    last = last.Previous;

                    validCount++;
                }

                // 更新 pingList 相关的数据
                pingCount = validCount;

                pingLast = curPing;

                // 当前 ping 超过弱网, 才需要将 连续弱网次数 +1 ， 否则的话直接清空
                if (curPing <= PingValue.PING_PONG_WEAK_NET_TIME)
                {
                    preContinueWeakCount = 0;

                    // 免得 preContinueNomalPingCount 爆炸
                    if (preContinueNomalPingCount > 50)
                    {
                        preContinueNomalPingCount = 50;
                    }
                    else
                    {
                        preContinueNomalPingCount++;
                    }
                }
                else
                {
                    preContinueWeakCount++;
                    preContinueNomalPingCount = 0;

                }
                // Debug.Log($"[ping] curPing: {curPing}, preContinueWeakCount: {preContinueWeakCount}, 刷新ping 间隔: {refreshPingInterval}");

                // 此次 ping 为 弱网, 那么 弱网ping 的次数 ++
                if (curPing >= PingValue.PING_PONG_WEAK_NET_TIME)
                {
                    totalWeakPingCount++;
                }

                // 根据 ping 的波动 刷新 ping 的频率
                {
                    // 如果 ping 样本数比较少, 那就快速发送
                    if (pingCount < PingValue.PING_VALID_COUNT)
                    {
                        refreshPingInterval = PingValue.PING_REFRESH_INTERVAL_NET_CHANG;
                        // Debug.Log($"[ping] 刷新ping 间隔: {refreshPingInterval}");
                        return;
                    }

                    // 上一次 ping 与这一次ping 的波动范围比较大, 那就重新采样 ping
                    if (Math.Abs(lastPing - curPing) >= PingValue.PING_WAVE_LIMIT || Math.Abs(sumPing / pingCount - curPing) > PingValue.PING_WAVE_LIMIT)
                    {
                        refreshPingInterval = PingValue.PING_REFRESH_INTERVAL_NET_CHANG;
                        // Debug.Log($"[ping] 刷新ping 间隔: {refreshPingInterval}");
                        return;
                    }

                    // 如果样本数量足够，并且网络波动范围小， 那ping 就按标准时间 发送
                    if (pingCount >= PingValue.PING_VALID_COUNT && Math.Abs(sumPing / pingCount - curPing) < PingValue.PING_WAVE_LIMIT)
                    {
                        refreshPingInterval = PingValue.PING_REFRESH_INTERVAL;
                        // Debug.Log($"[ping] 刷新ping 间隔: {refreshPingInterval}");
                    }
                }
            }
        }

        private void ClearPingList()
        {
            lock (pingsList)
            {
                pingsList.Clear();
                sumPing = 0;
                pingCount = 0;
                preContinueWeakCount = 0;
                preContinueNomalPingCount = 0;

                RefreshPing();
            }
        }

        private void RefreshPing()
        {
            TimeUtils.AveragePing = AveragePing;
        }



        private int GetCurAveragePing()
        {
            int curPingCount = pingCount;

            // 如果 sendPingTime = 0, 表示刚收到了 ping 的回复,还未开启新的一轮 ping,此时 averagePing 就等于 pingsList 的平均值
            var curSendPingTime = GetSendPingTime();
            if (curSendPingTime > 0)
            {
                // 当前 发送ping 后 等待的时间
                int curWaitPing = (int)(TimeUtils.ClientNowStampMilli - curSendPingTime);



                // 如果 curWaitPing <= lastPing，表明刚开始新的一轮ping-pong,此时 就不用计算curWaitPing,防止失真 
                // 只有 curWaitPing > lastPing, 计算才有意义
                if (curWaitPing > pingLast)
                {
                    sumPing += curWaitPing;
                    curPingCount = curPingCount + 1;
                }
            }

            if (curPingCount == 0)
            {
                return sumPing;
            }

            return (int)Math.Ceiling(sumPing * 1.0f / curPingCount);
        }

        /// <summary>
        /// 判断之前的 ping 是否有 count 次连续 弱网/正常 状态
        /// </summary>
        /// <param name="count"></param>
        /// <param name="isWeakPing"> 判断 是 弱网还是 正常网络</param>
        private bool IsPrevPingMatchTimes(int count, bool isWeakPing)
        {
            if (isWeakPing)
            {
                return preContinueWeakCount >= count;
            }
            else
            {
                return preContinueNomalPingCount >= count;
            }

        }

        /// <summary>
        /// 等待ping 的刷新逻辑
        /// </summary>
        private void OnPingUpdate()
        {
            // 开关 ping 的发送
            if (!sendPingFlag)
            {
                return;
            }
            // 定时刷新 ping, 正常情况 1s 发送一次ping, 网络波动情况下, 缩短发送间隔, 较快的计算出 ping 的真实情况
            if (refreshPingInterval > 0)
            {
                var time = TimeUtils.ClientNowStampMilli - refreshPingTime;
                if (time >= refreshPingInterval)
                {
                    refreshPingTime = TimeUtils.ClientNowStampMilli;
                    Schedule2SendHeartBeatMsg();
                }
            }

            var curSendPingTime = GetSendPingTime();

            // 当前帧 心跳刚回复
            if (curSendPingTime == 0)
            {
                tempWaitPinMsgTime = 0;
            }
            else
            {
                tempWaitPinMsgTime = TimeUtils.ClientNowStampMilli - curSendPingTime;
            }


            // 当前等待时间是否是 弱网ping
            curIsWeekPing = tempWaitPinMsgTime >= PingValue.PING_PONG_WEAK_NET_TIME;

            tempWaitPinCount = curIsWeekPing ? (int)(tempWaitPinMsgTime / PingValue.PING_PONG_WEAK_NET_TIME) : 0;



            // 连续三次 达到正常网络, 关闭 弱网
            if (!curIsWeekPing && IsPrevPingMatchTimes(PingValue.PING_ENTER_NORMAL_COUNT, false))
            {
                SwitchPingState(E_SocketPingState.None, "3次网络正常");
                return;
            }

            if (!curIsWeekPing)
            {
                return;
            }

            // 判定 总的 弱网次数 + 当前的 ping 的次数 是否 大于 总的 弱网次数限制, 如果满足, 那就 提示弱网，返回登录
            if ((totalWeakPingCount + tempWaitPinCount) >= PingValue.PING_WAVE_MAX_COUNT)
            {
                // 重置 总的弱网次数
                ResetTotalWeakPingCount();
                SwitchPingState(E_SocketPingState.ReturnLogin, $"超过总的 ping 弱网次数 {PingValue.PING_WAVE_MAX_COUNT},返回登录");
                return;
            }
            Debug.Log($"tempWaitPinMsgTime: {tempWaitPinMsgTime} , tempWaitPinCount: {tempWaitPinCount} ,  {tempWaitPinMsgTime >= PingValue.PING_PONG_WAIT_TIME} ");

            // 超时5s 关闭 socket 重连
            if (tempWaitPinMsgTime >= PingValue.PING_PONG_WAIT_TIME)
            {
                SwitchPingState(E_SocketPingState.TryReconnect, $"单次,等待ping: {tempWaitPinMsgTime}, 超过 {PingValue.PING_PONG_WAIT_TIME} ms, 弹出 断线重连弹窗");
                return;
            }


            // 如果连续 弱网 达到 10 次
            if (preContinueWeakCount + tempWaitPinCount >= PingValue.PING_ENTER_DISCONNECT_COUNT)
            {
                SwitchPingState(E_SocketPingState.TryReconnect, $"{PingValue.PING_ENTER_DISCONNECT_COUNT}次弱网, 弹出 断线重连弹窗");
                return;
            }

            // 如果当前等待时间 已经是 弱网等待ping 的状态, 判定 当前的ping 时间 为多少次的 弱网状态
            if (cur_pingState == E_SocketPingState.None && (preContinueWeakCount + tempWaitPinCount) >= PingValue.PING_ENTER_CONNECTING_COUNT)
            {
                // 切换到弱网状态
                SwitchPingState(E_SocketPingState.Weaking, $"{PingValue.PING_ENTER_CONNECTING_COUNT} 次弱网,  弹出 菊花, 当前等待时间: {tempWaitPinMsgTime}");
                return;
            }

        }
        private E_SocketPingState cur_pingState = E_SocketPingState.None;
        private void SwitchPingState(E_SocketPingState newState, string result)
        {
            if (cur_pingState == newState)
            {
                return;
            }
            Debug.Log($"{TagFlag} SwitchPingState {cur_pingState} ---> {newState}, : {result}");
            cur_pingState = newState;

            switch (cur_pingState)
            {
                case E_SocketPingState.None:
                    {
                        socketBase.SwithSocketViewState(SocketViewState.None);
                    }
                    break;
                case E_SocketPingState.Weaking:
                    {
                        socketBase.SwithSocketViewState(SocketViewState.Connecting);
                    }
                    break;
                case E_SocketPingState.TryReconnect:
                    {
                        socketBase.SwithSocketViewState(SocketViewState.PingReconnectBox);
                    }
                    break;
                case E_SocketPingState.ReturnLogin:
                    {
                        socketBase.SwithSocketViewState(SocketViewState.RetrunLoginBox);
                    }
                    break;

                default: break;
            }

        }


    }
}
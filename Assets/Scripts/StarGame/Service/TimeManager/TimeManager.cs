using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.Unity;
using StarProject.Service.Time.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace StarProject.Service.Time
{
    public class TimeManager : ServiceModule<TimeManager>
    {
        private List<StarTimer> timerList;



        private ServerTimeReq serverTimeReqMsg = new ServerTimeReq();    // 获取服务器时间


        /// <summary>
        /// 同步到的 服务器时间戳
        /// </summary>
        private long serverTimeStamp;
        private long syncClientTimeStamp;
        /// <summary>
        /// 收到服务器时间戳的时候, 客户端时间与服务器时间的时间差.
        /// </summary>
        private long syncTimeOffset;

        /// <summary>
        /// 同步的时间
        /// </summary>
        private DateTime syncTime = DateTime.UtcNow;

        public int AveragePing => TimeUtils.AveragePing;
        public static int SinglePing => TimeUtils.AveragePing / 2;

        public void SetTimeSync(long _syncStamp)
        {
            syncTime = DateTime.UtcNow;

            Thread.VolatileWrite(ref syncClientTimeStamp, DateTimeOffset.Now.ToUnixTimeMilliseconds());
            Thread.VolatileWrite(ref serverTimeStamp, _syncStamp);

            Thread.VolatileWrite(ref syncTimeOffset, DateTimeOffset.Now.ToUnixTimeMilliseconds() - _syncStamp);


            TimeUtils.OnServerTimeSync(serverTimeStamp);
        }

        /// <summary>
        /// 防止数据竞争,用的原子读写
        /// </summary>
        public long ServerSyncTimeStamp
        {
            get
            {
                return Thread.VolatileRead(ref serverTimeStamp);
            }
        }

        public long ClientSyncTimeStamp
        {
            get
            {
                return Thread.VolatileRead(ref syncClientTimeStamp);
            }
        }

        private long ClientSyncOffsetTime
        {
            get
            {
                return Thread.VolatileRead(ref syncTimeOffset);
            }
        }

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <returns></returns>
        public long GetServerTimeStamp()
        {
            return ServerSyncTimeStamp + Convert.ToInt64((DateTime.UtcNow - syncTime).TotalMilliseconds);
        }

        public DateTime GetServerTime()
        {
            if (ServerSyncTimeStamp == 0)
            {
                return DateTime.Now;
            }

            return DateTime.Now.AddMilliseconds(-ClientSyncOffsetTime).AddMilliseconds(SinglePing);
        }



        public void Init()
        {
            CheckSingleton();

            timerList = new List<StarTimer>();

            // 服务器时间 10s 同步一次
            NetworkManager.Instance.OnPreStaticMessage((int)MsgIDEnum.ServerTimeRetID, OnServerTimeRetMsg);

            MonoHelper.AddFixedUpdateListener(OnEnterFrame, MonoHelper.E_ModuleType.CommonService);

        }

        #region 服务器时间 10s 同步一次

        public void RegReqServerTime()
        {
            StopRegReqServerTime();

            // 请求服务器时间 同步还是用 10s 一次, 如果1s 一次, 那时服务求时间戳的变化会很平凡
            ReqSeversTime();

            MonoHelper.AddTenSecTimeUpdateListener(ReqSeversTime, MonoHelper.E_ModuleType.CommonService);
        }

        public void StopRegReqServerTime()
        {
            MonoHelper.RemoveTenSecTimeUpdateListener(ReqSeversTime, MonoHelper.E_ModuleType.CommonService);
        }

        public void ReqSeversTime()
        {
            if (NetworkManager.Instance.gameSocket.IsConnceted)
            {
                // SGF.Debuger.Log($"[Time] send 请求 ReqSeversTime ");

                //战斗服启动时间
                NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, serverTimeReqMsg, false);
            }
        }

        private void OnServerTimeRetMsg(MessageHandleData data)
        {

            ServerTimeRet serverTimeRetMsg = (ServerTimeRet)data.data;
            // SGF.Debuger.Log($" [Time] , 收到服务器同步, 服务器时间 = {serverTimeRetMsg.TimeStamp}");
            SetTimeSync(serverTimeRetMsg.TimeStamp);
        }

        #endregion
        public void AddTimer(StarTimer t)
        {
            timerList.Add(t);
        }

        //Update我觉得可能有问题
        //Fix相对好点，
        //TODO:不过一定是多线程
        private void OnEnterFrame()
        {
            for (int i = 0; i < timerList.Count;)
            {
                timerList[i].Run();

                //计时结束，且需要销毁
                if (!timerList[i].isActive && timerList[i].isDestroy)
                {
                    timerList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        public void OnGameRelease()
        {
            MonoHelper.RemoveFixedUpdateListener(OnEnterFrame, MonoHelper.E_ModuleType.CommonService);
        }

        public void OnTimeReset()
        {
            for (int i = 0; i < timerList.Count; i++)
            {
                timerList[i].Reset();
            }
        }

        public void GameAllPause()
        {
            for (int i = 0; i < timerList.Count; i++)
            {
                timerList[i].isPause = true;
            }
            UnityEngine.Time.timeScale = 0f;
        }

        public void GameAllStart()
        {
            for (int i = 0; i < timerList.Count; i++)
            {
                timerList[i].isPause = false;
            }
            UnityEngine.Time.timeScale = 0f;
        }
        //倒计时器
        //obj.
    }
}


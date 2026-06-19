using Google.Protobuf;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.UI.Framework;
using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game.Data; //子文件，依然封装
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.LocalDynamic;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Game.SnapShot;
using StarProject.Game.StarsCamera;
using StarProject.Module;
using StarProject.OffLine;
using StarProject.Service.Battle;
using StarProject.Service.Business;
using StarProject.Service.Cam;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProject.Service.ServerService;
using StarProject.Service.User;
using StarProjectDef;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game
{
    [XLua.LuaCallCSharp]
    /// <summary>
    /// GameManager是服务模块
    /// 1前面，有根据不同战场模式，启动参数，来控制中心模块中转
    /// 2他是中转控制所有模块的模块，也是交通的中间要塞，（转盘）【启动】【释放】
    /// 3下面他会联通，核心对象，玩家模块，地图模块，输入模块，【数据结构，上下文，事件】
    /// 
    /// 4，目前他的开启由大世界模块（business就是个系统模块）->大世界游戏（new因为有的战场就是需要时间就是会结束会有释放情况）--通知-->GameManager去联通所有模块加载构成游戏
    /// 
    /// 小地图有转角
    /// </summary>
    public class GameManager : ServiceModule<GameManager>
    {
        /// <summary>
        /// 是否锁游戏消息处理
        /// </summary>
        public bool IsLockGameMsgHandle = true;

        public bool IsLoginInitState = true;

        private string LOG_TAG = "GameManager";

        private bool m_isRunning;

        private EquipSlotControllerModule equipSlotModule;

        private InterActionModule _InterActionModule;

        private InterActionModule m_InterActionModule
        {
            get
            {
                //if (_InterActionModule == null)
                //{
                //    _InterActionModule = ModuleManager.Instance.GetModule(ModuleDef.Name.InterActionModule) as InterActionModule;
                //}

                //return _InterActionModule;

                return ModuleManager.Instance.GetModule(ModuleDef.Name.InterActionModule) as InterActionModule;
            }
        }

        private StarWorldModule starWorldModule;

        private StarWorldModule starWorld
        {
            get
            {
                //if (starWorldModule == null)
                //{
                //    starWorldModule = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
                //}
                //return starWorldModule;

                return ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
            }
        }

        public int testInt = 0;
        public ulong MainPlayerEnityId = 0;
        public ulong mainPlayerId
        {
            get
            {
                if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
                {
                    return M_MainPlayerCtrlBase.M_Curr.EntityId;
                }

                return MainPlayerEnityId;
            }
        }

        public ulong SpaceIDCache
        {
            get
            {
                if (starWorld != null)
                {
                    return starWorld.SpaceIDCache;
                }

                return 0;
            }
        }

        public int OldMapID
        {
            get
            {
                if (starWorld != null)
                {
                    return starWorld.OldMapID;
                }

                return 0;
            }
        }

        public ulong ServerIDCache
        {
            get
            {
                if (starWorld != null)
                {
                    return starWorld.ServerIDCache;
                }

                return 0;
            }
        }

        private EntityCtrlBase m_MainPlayerCtrlBase;

        public EntityCtrlBase M_MainPlayerCtrlBase
        {
            get { return m_MainPlayerCtrlBase; }
            set
            {
                m_MainPlayerCtrlBase = value;
                // 主角自己绑定 目标与自己移动时要计算的委托
                if (m_MainPlayerCtrlBase != null)
                {
                    (m_MainPlayerCtrlBase.M_Curr as NPCEntityBase).ActionOnCheckTargetMove += OnActionOnCheckTargetMove;
                    GlobalEvent.OnMainPlayerCreate.Invoke(0);
                }
                else
                {
                    // 主角如果没有了, 那就释放掉所有注册的 ac
                    GlobalEvent.OnMainPlayerCreate.RemoveAllListeners();
                }
            }
        }

        public bool IsSeverBattleState
        {
            get { return BattleManager.Instance.IsSeverBattleState; }
        }


        public GameObject M_GameCamera;

        /// <summary>
        /// 获取战斗摄像机
        /// </summary>
        /// <returns></returns>
        public Camera GetGameCamera()
        {
            if (M_GameCamera == null)
            {
                return null;
            }

            GameCamera gc = M_GameCamera.GetComponent<GameCamera>();
            return gc.Camera;
        }

        public GameCamera GetGameCameraComponent()
        {
            if (M_GameCamera == null)
            {
                return null;
            }

            GameCamera gc = M_GameCamera.GetComponent<GameCamera>();
            return gc;
        }

        //////--------------------- 本地实体缓存
        private List<EntityLocalDynamic> m_LocalEntitys = new();

        public List<EntityLocalDynamic> M_LocalEntitys
        {
            get { return m_LocalEntitys; }
        }

        public DictionaryEx<string, EntityLocalDynamic> m_LocalEntityDic = new();
        public DictionaryEx<ulong, EntityLocalStatic> m_LocalStaticEntityDic = new();
        /// <summary>
        /// 标签key 对应的本地实体 id
        /// </summary>
        public DictionaryEx<string, ulong> m_key2EntityIDDic = new();

        //////--------------------- 本地实体缓存

        //////--------------------- 主角系统玩法数据

        public int SeasonPassFloor = 1; // 秘境历史通过层数

        public bool IsShowPlayerWidget = false;

        //////--------------------- 主角系统数据

        public void Init()
        {
            //P=Server
            //开启携程，设定帧率1，帧率2
            //循环调用
            //检测合法性 = 输出操作
            //消息队列
            //处理结果
            //同步
            //逻辑
            //渲染
            OnEventMessage();
            SimpleDataFactory.Init();
            //全局服务AOI协议接收器，不依重新登陆而销毁，时刻准备类似Service
            //AOI,全量都在这里（主角在全量里面）。登录需要拿到全量信息
            DynamicDataFactory.Init();
            // InitTestData();
            EntityFactory.Init();

            ViewFactory.Init(EntityRoot.Instance.RemoveRoot.transform);
        }

        public override void Release()
        {
            OffEventMessage();


            SimpleDataFactory.Release();
            DynamicDataFactory.Release();
            MainPlayerEnityId = 0;
            DestroyNavMeshQuery();
            base.Release();
        }

        private void OnEventMessage()
        {
            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneChanged);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.AOIMsgID, OnAOIMsg, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PropSyncListID, OnPropSyncList, this);

            // 服务器同步坐标给客户端(客户端传的坐标，服务器验证不合法)
            // 1.服务器给不合法坐标周围最近的合法点坐标（优先）
            // 2.服务器给我们上次移动的合法坐标点
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.ServerSetPosNTFID, OnServerSetPosNTFMsg, this);
            // 强同步角度
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.ServerSetRotNTFID, OnServerSetRotNTFMsg, this);

            //服务器Flag
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.InsFlagNtfID, OnInsFlagNtfMsg, this);

            //服务器全局设置（包括开服时间啥的
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GloInfoSrvRetID, OnGloInfoSrvRetMsg, this);

            //////////-------------------------------- 客户端自己模拟创建的简单实体

            GlobalEvent.onCreateLocalEntity.AddListener(OnCreateLocalEntity);

            //////////-------------------------------- 客户端自己模拟创建的简单实体

            ////--- 通缉玩法
            {
                // 全量同步通缉信息
                NetworkManager.Instance.OnMessageEnum(MsgIDEnum.WTaskGetAllInfoRetID, OnWTaskGetAllInfoRet, this);
                // 单个点位信息发生改变同步
                NetworkManager.Instance.OnMessageEnum(MsgIDEnum.WTaskPointNtfID, OnWTaskPointNtf, this);
                // 某阶刷新同步
                NetworkManager.Instance.OnMessageEnum(MsgIDEnum.WTaskStageNtfID, OnWTaskStageNtf, this);
            }

            //充值信息
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.FirstPaySignInfoNtfID, OnFirstPaySignInfoNtf, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RechargeInfoNtfID, OnRechargeInfoNtf, this);




            //小月卡信息
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SmallMonthCardInfoNtfID, OnSmallMonthCardInfoNtf, this);

            //通行证信息
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.BattlePassInfoNtfID, OnBattlePassInfoNtf, this);

            //GVE活动开始
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GVEStartI2SID, OnGVEStartI2S, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GVEBossListNtfID, OnGVEBossListNtf, this);
            //GNG活动开始
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GNGPlayStartI2LID, OnGNGPlayStartI2L, this);

            //公会篝火
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.QAPlayStartNtfID, OnQAPlayStartNtf, this);

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                GlobalEvent.OnLocalServerEvent.AddListener(OnLocalServerEvent);
            }
#endif
        }

        private void OffEventMessage()
        {
            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneChanged);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.AOIMsgID, OnAOIMsg, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PropSyncListID, OnPropSyncList, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.ServerSetPosNTFID, OnServerSetPosNTFMsg,
                this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.ServerSetRotNTFID, OnServerSetRotNTFMsg,
                this);
            //服务器Flag
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.InsFlagNtfID, OnInsFlagNtfMsg, this);

            //服务器全局设置（包括开服时间啥的
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GloInfoSrvRetID, OnGloInfoSrvRetMsg, this);

            //////////-------------------------------- 客户端自己模拟创建的简单实体

            GlobalEvent.onCreateLocalEntity.RemoveListener(OnCreateLocalEntity);


            //////////-------------------------------- 客户端自己模拟创建的简单实体
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.FirstPaySignInfoNtfID, OnFirstPaySignInfoNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.RechargeInfoNtfID, OnRechargeInfoNtf, this);

            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.SmallMonthCardInfoNtfID, OnSmallMonthCardInfoNtf,
                this);

            //通行证信息
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.BattlePassInfoNtfID, OnBattlePassInfoNtf, this);


            //GVE活动
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GVEStartI2SID, OnGVEStartI2S, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GVEBossListNtfID, OnGVEBossListNtf, this);
            //GNG活动开始
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GNGPlayStartI2LID, OnGNGPlayStartI2L, this);


            //公会篝火
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.QAPlayStartNtfID, OnQAPlayStartNtf, this);

        }

        #region 强同步

        /// <summary>
        /// 服务器同步坐标给客户端(客户端传的坐标，服务器验证不合法)
        /// 1.服务器给不合法坐标周围最近的合法点坐标（优先）
        /// 2.服务器给我们上次移动的合法坐标点
        /// </summary>
        /// <param name="data"></param>
        private void OnServerSetPosNTFMsg(MessageHandleData data)
        {
            ProtoMsg.ServerSetPosNTF serverSetPosMsg = (ProtoMsg.ServerSetPosNTF)data.data;
            if (M_MainPlayerCtrlBase != null && serverSetPosMsg.GameEntityID == mainPlayerId)
            {
                // 打断当前的 寻路
                (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFindPath();
                (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFollowDynamicEnity();

                UnityEngine.Vector3 v3 = UnityEngine.Vector3.zero;
                v3.x = serverSetPosMsg.Pos.X;
                v3.y = serverSetPosMsg.Pos.Y;
                v3.z = serverSetPosMsg.Pos.Z;
                //SGF.Debuger.LogWarning($"OnServerSetPosNTFMsg PlayerEnityId=> {serverSetPosMsg.GameEntityID} posX:{v3.x},posY:{v3.y},posZ:{v3.z}");
                //SGF.Debuger.Log($"  [xx] [StarWorldGame] OnServerSetPosNTFMsg serverSetPosMsg {v3}");
                M_MainPlayerCtrlBase.ForceSyncPos(v3);
            }
        }

        /// <summary>
        /// 服务器同步角度给客户端【主角-强】
        /// </summary>
        /// <param name="data"></param>
        private void OnServerSetRotNTFMsg(MessageHandleData data)
        {
            ProtoMsg.ServerSetRotNTF serverSetRotMsg = (ProtoMsg.ServerSetRotNTF)data.data;
            if (M_MainPlayerCtrlBase != null && serverSetRotMsg.GameEntityID == mainPlayerId)
            {
                // 打断当前的 寻路
                (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFindPath();
                (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFollowDynamicEnity();

                int rot = serverSetRotMsg.Rot;
                // SGF.Debuger.LogError($"[Rotate]  entityID: {mainPlayerId}  收到服务器 强同步坐标: {rot}");
                M_MainPlayerCtrlBase.M_Curr.ServerSetRotation(rot, serverSetRotMsg.ShowRotAn, true, 0);
            }
        }

        #endregion

        #region 首充相关

        int GetDay(int sec)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(sec);
            return dateTimeOffset.LocalDateTime.Day;
        }

        public FirstPaySignInfoNtf FirstPaySignInfo;

        private void OnFirstPaySignInfoNtf(MessageHandleData data)
        {
            FirstPaySignInfo = (FirstPaySignInfoNtf)data.data;
            bool canGet = false;

            if (FirstPaySignInfo != null)
            {
                var HaveFirstPayAct = FirstPaySignInfo.HaveFirstPayAct;
                var GetFirstPayAwardNum = FirstPaySignInfo.GetFirstPayAwardNum;
                var LastGetFirstPayAwardTime = FirstPaySignInfo.LastGetFirstPayAwardTime;


                int MaxDay = LocalDataManager.Instance.GetFirstPayMaxDay();


                if (GetFirstPayAwardNum < MaxDay)
                {
                    if (GetFirstPayAwardNum == 0)
                    {
                        if (HaveFirstPayAct)
                        {
                            canGet = true;
                        }
                    }
                    else
                    {
                        int serverNowTime = Mathf.FloorToInt(GameManager.Instance.GetServerTimeStamp() / 1000);
                        int day1 = GetDay(serverNowTime - (5 * 3600));
                        int day2 = 0;
                        if (LastGetFirstPayAwardTime != 0)
                        {
                            day2 = GetDay(LastGetFirstPayAwardTime - (5 * 3600)); //凌晨五点刷新（扣除5小时）
                        }

                        if (day1 != day2)
                        {
                            canGet = true;
                        }
                    }
                }
            }

            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.FirstCharge, canGet);

            GlobalEvent.OnRefeshShopBtn.Invoke(false);
        }

        #endregion

        #region 小月卡相关

        public SmallMonthCardInfoNtf SmallMonthCardInfo;

        private void OnSmallMonthCardInfoNtf(MessageHandleData data)
        {
            SmallMonthCardInfo = (SmallMonthCardInfoNtf)data.data;

            bool canGet = false;

            if (SmallMonthCardInfo != null)
            {
                int serverNowTime = Mathf.FloorToInt(GameManager.Instance.GetServerTimeStamp() / 1000);
                var latestBuyTime = Mathf.FloorToInt(SmallMonthCardInfo.LatestBuyTime);
                var willFinishTime = Mathf.FloorToInt(SmallMonthCardInfo.WillFinishTime);
                var lastGetTime = Mathf.FloorToInt(SmallMonthCardInfo.LastSignTime);

                int leftDay = 0;
                int leftSecond = willFinishTime - serverNowTime;
                if (leftSecond > 0)
                {
                    leftDay = Mathf.CeilToInt(leftSecond / 60 / 60 / 24);
                }

                if (leftDay > 0)
                {
                    int day1 = GetDay(serverNowTime - (5 * 3600));
                    int day2 = 0;
                    if (lastGetTime != 0)
                    {
                        day2 = GetDay(lastGetTime - (5 * 3600)); //凌晨五点刷新（扣除5小时）
                    }

                    if (day1 != day2)
                    {
                        canGet = true;
                    }
                }
            }

            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.MoonCard, canGet);
        }

        #endregion

        #region 通行证相关

        public BattlePassInfoNtf BattlePassInfo;

        private void OnBattlePassInfoNtf(MessageHandleData data)
        {
            BattlePassInfo = (BattlePassInfoNtf)data.data;

            bool canGet = false;

            if (BattlePassInfo != null)
            {
                //判断是否有没领取的
                foreach (var kv in BattlePassInfo.OtherAwardLv)
                {
                    if (BattlePassInfo.BPType == BPTypeEnum.FreePass)
                    {
                        //没有付费
                        if (kv.Value == 1)
                        {
                            canGet = true;
                            break;
                        }
                    }
                    else if (BattlePassInfo.BPType == BPTypeEnum.PayJunior ||
                             BattlePassInfo.BPType == BPTypeEnum.PaySenior)
                    {
                        //付费了
                        if (kv.Value == 1 || kv.Value == 3)
                        {
                            canGet = true;
                            break;
                        }
                    }
                }
            }

            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.BattlePass, canGet);

            ModuleManager.Instance.SendMessage(ModuleDef.Name.CommercializationModule, "OnCheckBattlePassTask", null);
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ActivityModule, "OnCheckPartnerTaskTask", null);
        }

        #endregion

        #region 充值活动

        public RechargeInfoNtf RechargeInfo;

        private void OnRechargeInfoNtf(MessageHandleData data)
        {
            RechargeInfo = (RechargeInfoNtf)data.data;
        }

        #endregion

        #region 篝火活动


        public QAPlayStartNtf QAPlayStartNtf;//公会篝火的数据
        public bool IsInPartyArea;//判断是否在公会篝火区域

        private void OnQAPlayStartNtf(MessageHandleData data)
        {
            QAPlayStartNtf = (QAPlayStartNtf)data.data;
        }


        #endregion

        #region GNG活动

        public GNGPlayStartI2L GNGPlayStartI2L;

        private void OnGNGPlayStartI2L(MessageHandleData data)
        {
            GNGPlayStartI2L = (GNGPlayStartI2L)data.data;

            //刷新NPC显影
            UpdateAllNpcServiceState();

            long d1 = GNGPlayStartI2L.StartTime; //开始时间
            long d2 = GNGPlayStartI2L.Duration; //持续时间（不变
            long d3 = GNGPlayStartI2L.EndTime; //结束时间
            long d4 = GNGPlayStartI2L.BossID; //bossid
        }

        #endregion

        #region GVE活动

        public GVEStartI2S GVEStartI2S;
        public RepeatedField<ProtoMsg.WaitMonNode> BossList;

        /// <summary>
        /// GVE活动开始的数据
        /// </summary>
        /// <param name="data"></param>
        private void OnGVEStartI2S(MessageHandleData data)
        {
            GVEStartI2S = (GVEStartI2S)data.data;
            BossList = GVEStartI2S.BossList;

            bool isSameMap = false;
            foreach (var it in GVEStartI2S.MapList)
            {
                if (it.Key == GameManager.Instance.GetCurMapId())
                {
                    isSameMap = true;
                }
            }

            if (isSameMap)
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "OnStartGve", new object[] { });
                //Debug.LogError("相同地图");
            }
            else
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "OnEndGve", new object[] { });
                //Debug.LogError("不同地图");
            }

            //打开GVE面板
            //ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "OnStartGve", new object[] { });

            long d1 = GVEStartI2S.BossDelay; //boss初始刷新delay（不变
            long d2 = GVEStartI2S.BossInterval; //boss刷新间隔（不变
            int d3 = GVEStartI2S.Duration; //持续时间（不变
            int d4 = GVEStartI2S.PlayID; //玩法id（不变
            long d5 = GVEStartI2S.StartTime; //开始时间（不变

            var d6 = GVEStartI2S.BossList; //boss刷新序列（变
            var d7 = GVEStartI2S.CurWave; //当前已刷的波次（变
            var d8 = GVEStartI2S.MapList; //地图对应的怪物选择

            //刷新小地图
            StarProject.GlobalEvent.OnGVEBossRefesh.Invoke(-1);
        }


        private void OnGVEBossListNtf(MessageHandleData data)
        {
            BossList = ((GVEBossListNtf)data.data).BossList;

            //刷新小地图
            StarProject.GlobalEvent.OnGVEBossRefesh.Invoke(-1);
        }

        #endregion


        #region AOI逻辑

        //=========================================缓存=================================

        //创建变量的临时引用，无就是无数据，有就是有转向和路店的任意数据
        //赋值是空等于是需要接受新数据
        //原来设定是，【等于空，下次调用可以录入：录入完毕的特性==通过置空的方式】
        private SerNttAOIOneFrameSnapALLDataTHD serMoveRotSnapDataSaver = null;

        /// <summary>
        /// 请求的时候一定是需要的时候，没有则保证是新的，结束则关闭
        /// </summary>
        private SerNttAOIOneFrameSnapALLDataTHD SerMoveRotSnapDataSaver
        {
            get
            {
                //没有就创建新的
                if (serMoveRotSnapDataSaver == null)
                {
                    //缓存了这个类型，这个人？，所有路点角度数据
                    serMoveRotSnapDataSaver = DynamicDataFactory.InstanceData<SerNttAOIOneFrameSnapALLDataTHD>();
                }
                //有就用原来创建的；也可以手动清理
                else
                {
                    return serMoveRotSnapDataSaver;
                }

                return serMoveRotSnapDataSaver;
            }
        }

        /// <summary>
        /// 读写缓存：1，缓存本次信息。2，清理就是执行结束
        /// 1，重复内存池，2，直接属性直接释放掉，3，间接属性也给了别人
        /// </summary>
        private SerNttAOIOneFrameSnapALLDataTHD SerMRSnapDataReader; //SerMRSnapDataRuningTagReader

        //主角的---------------------------------------------------
        private SerNttAOIOneFrameSnapALLDataMainPlayer serMoveRotSnapDataSaverMainPlayer = null;

        private SerNttAOIOneFrameSnapALLDataMainPlayer SerMoveRotSnapDataSaverMainPlayer
        {
            get
            {
                if (serMoveRotSnapDataSaverMainPlayer == null)
                {
                    serMoveRotSnapDataSaverMainPlayer =
                        DynamicDataFactory.InstanceData<SerNttAOIOneFrameSnapALLDataMainPlayer>();
                }
                else
                {
                    return serMoveRotSnapDataSaverMainPlayer;
                }

                return serMoveRotSnapDataSaverMainPlayer;
            }
        }

        private SerNttAOIOneFrameSnapALLDataMainPlayer SerMRSnapDataReaderMainPlayer; //SerMRSnapDataRuningTagReader

        //RPC协议的---------------------------------------------------
        private SerRPCOneFrameSnapALLData rpcOneFrameSnapALLData = null;

        private SerRPCOneFrameSnapALLData RPCOneFrameSnapALLData
        {
            get
            {
                if (rpcOneFrameSnapALLData == null)
                {
                    rpcOneFrameSnapALLData = DynamicDataFactory.InstanceData<SerRPCOneFrameSnapALLData>();
                }
                else
                {
                    return rpcOneFrameSnapALLData;
                }

                return rpcOneFrameSnapALLData;
            }
        }

        private SerRPCOneFrameSnapALLData SerMRSnapDataReaderRPC;

        //=========================================缓存=================================

        //新数据只有弱权力驱动
        //新数据的结果
        /* 处理结束*/
        //内部循环调用这个ExecuteTransformChange，当没执行标识占位符（SerMoveRotSnapDataReader == null）提出新的[内循环]
        //执行外部分调用本函数ExecuteTransformChange也驱动循环[外循环]
        //都根据询问是否启动，是否在干活哦
        //执行结束后会清理
        private void InvokeFrameTHDDataCache()
        {
            int DealCount = GetDealCountByAddSpeed<SerNttAOIOneFrameSnapALLDataTHD>();

            for (int c = 0; c < DealCount; c++)
            {
                //不用阻塞，那怎么处理，直接推送就行，（（人有二级缓存，怪物不管他），不管直接还是过程化）
                SerMRSnapDataReader = DynamicDataFactory.PopEarliestDataByRecorde<SerNttAOIOneFrameSnapALLDataTHD>();
                if (SerMRSnapDataReader != null && SerMRSnapDataReader._aOIMsgList != null)
                {
                    //SGF.Debuger.Log($"属性同步 处理数据  _frameIndex={SerMRSnapDataReader._frameIndex},curIndex={FrameIndex}");
                    for (int i = 0; i < SerMRSnapDataReader._aOIMsgList.Count; i++)
                    {
                        HandleAoiMsg(SerMRSnapDataReader._aOIMsgList[i]);
                    }
                    //有没有异步数据，
                    //移动过程中可能有属性变更：比如我A发送到B，中间碰到火墙掉血了；肯定不以移动到结尾才处理
                    //    1，所以压根没有【异步等待分割】数据：如移动，那就是同级；
                    //    2，同级分发属性做不同的事情
                    //    3，延迟式属性依赖平均网速
                    //    4，延迟必然停下
                    //    5，延迟后的拥堵这里来处理：处理的都是异步：基础就是对照移动和转向需要一组；【快速播放完毕过程化快速推到0Count】
                    //    核心目的是过去的过程表达：类似小小的5连击；赶紧告诉玩家之前发生了什么事情，但是不重要
                    //    6.1所以你移动结束一定告诉我--不必
                    //    6.2我自己也要有结束推送--结束不推送
                    //    6.3我需要知道是否具有异步数据---并非如此所有属性平均，只不过异步数据加速，同步直接覆盖，同步异步之间的过程化由于网络拥堵导致的表现可以忽略
                    //    没有数据服务器不会发这一帧，没有变更不会发这一帧

                    //有些有反馈有些没反馈,查看是否有移动，转角，和路点决定是否要等待---不不需要等待
                    //服务器会标记脏数据，所以可能有一帧不发数据的么，极限情况很有可能1秒只有一个数据其他29是不发，比如血量没变（不发）【所以距离，速度，我知道时间】*加速，【其他数据都是直接覆盖】；不可能每一帧去对去推这是状态同步
                }
                else
                {
                    //SGF.Debuger.LogError($"属性同步 处理数据 没拿到数据  curIndex={FrameIndex}");
                }
            }
        }

        private void InvokeFrameMainPlayerDataCache()
        {
            int DealCount = GetDealCountByAddSpeed<SerNttAOIOneFrameSnapALLDataMainPlayer>();
            for (int c = 0; c < DealCount; c++)
            {
                //不用阻塞，那怎么处理，直接推送就行，（（人有二级缓存，怪物不管他），不管直接还是过程化）
                SerMRSnapDataReaderMainPlayer =
                    DynamicDataFactory.PopEarliestDataByRecorde<SerNttAOIOneFrameSnapALLDataMainPlayer>();
                if (SerMRSnapDataReaderMainPlayer != null)
                {
                    HandlePropSync(SerMRSnapDataReaderMainPlayer._propSyncList);
                }
            }
        }


        MessageHandleData rpcMsg = null;
        long now = 0;

        private void InvokeFrameRPCDataCache()
        {
            int DealCount = GetDealCountByAddSpeed<SerRPCOneFrameSnapALLData>();
            if (DealCount == 0)
            {
                return;
            }

            now = TimeUtils.ClientNowStampMilli;
            // LogUtils.LogWarning(LogUtils.LogEnum.GameManagerTest, $"[GameManager] 准备 处理消息 count: {DealCount}-----------", DealCount > 10);
            for (int c = 0; c < DealCount; c++)
            {
                SerMRSnapDataReaderRPC = DynamicDataFactory.PopEarliestDataByRecorde<SerRPCOneFrameSnapALLData>();
                if (SerMRSnapDataReaderRPC != null && SerMRSnapDataReaderRPC._rPCMsgList != null)
                {
                    // LogUtils.LogWarning(LogUtils.LogEnum.GameManagerTest, $"[GameManager] _rPCMsgList count: {SerMRSnapDataReaderRPC._rPCMsgList.Count}", SerMRSnapDataReaderRPC._rPCMsgList.Count > 20);

                    for (int i = 0; i < SerMRSnapDataReaderRPC._rPCMsgList.Count; i++)
                    {
                        rpcMsg = SerMRSnapDataReaderRPC._rPCMsgList[i];

                        if (rpcMsg.messageCmd == (int)MsgIDEnum.AOIMsgID)
                        {
                            HandleAoiMsg((AOIMsg)rpcMsg.data);
                        }
                        else if (rpcMsg.messageCmd == (int)MsgIDEnum.PropSyncListID)
                        {
                            HandlePropSync((PropSyncList)rpcMsg.data);
                        }
                        else
                        {
                            HandleRPCMsg(SerMRSnapDataReaderRPC._rPCMsgList[i]);
                        }
                    }
                    // LogUtils.LogWarning(LogUtils.LogEnum.GameManagerTest, $"[GameManager] _rPCMsgList 处理完成 cost: {TimeUtils.ClientNowStampMilli - now}", SerMRSnapDataReaderRPC._rPCMsgList.Count > 20);
                }
            }
            //SGF.Debuger.Log($"属性同步 网络层下发 -===-=-=-=-=-=--=-=-=-=-=-=-=-=-=-=-=-==-=-=-= 处理数据  count={DealCount}");
        }

        private void ClearFrameRPCDataCache()
        {
            rpcOneFrameSnapALLData = null;
        }


        /// <summary>
        ///// //1策略，小于3个不处理服务器可能有空帧，客户端是每帧都在处理
        ////2，大于6~10按照固定速率处理 20
        ////3，大于50~按照 20 + 百分比处理
        ////客户端处理能力固定所以有上限100，确实要加速
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int GetDealCountByAddSpeed<T>() where T : DynamicDataObject
        {
            int count = DynamicDataFactory.GetCount<T>();
            if (count == 0)
            {
                return 0;
            }
            else if (count > 0 && count <= 3) //正常的缓存环境，不加速起码服务器还是有点帧的概念的（比如合包，一个包一起发）尽量不加速的区域
            {
                return 1; //如果一层比一层高，你永远是在处理缓存增加；实际情况一个是增加一个是消耗
            }
            else if (count > 3 && count <= 10) //需要加速
            {
                return (int)(count / 2f);
            }
            else if (count > 10 && count <= 30) //保护层
            {
                return count;
            }
            else if (count > 10 && count <= 100) //需要加速
            {
                //最小处理，动态值处理；上限是3分支1；肯定取大的但是大的我限定你了
                int maxValue = (int)(count / 1.5f);

                int adderValue = (int)(count / 2f) +
                                 Mathf.Clamp((int)MathF.Pow(GameConfig.AOI_CLIENT_SIM_SPEED_PARA, count), 1,
                                     int.MaxValue); //保证不跟65535出问题
                //两个都是递增  1,用min快速递增；2,用max浮动值主导了；3，应该用限制值
                int para = Mathf.Clamp(adderValue, 1, maxValue); //最大也不是cunt的0.25倍
                return para;
            }
            else //保护层
            {
                return count;
            }
            //除了保护层之外都是为了平滑，保护层是处理延迟和拥堵，拥堵分成两种情况增量大，卡顿突然增加很多，全部针对移动，消息的处理
        }


        private int EnityChangeTime = 1;

        private Type typeOfEntityType = typeof(E_EntityType);

        /// <summary>
        /// 属性同步基于AOI，看得见的才同步其他人，同步进入出和属性，基于响应
        /// 一帧之内发过来，数据集合，会包括所有变更的实体数据集合
        /// 【实际情况是每次网络层可能由于拥堵给多条数据】
        /// [NetMgr：网络多条]【协议层次：RPC/AOI】[这里：通过组合拥堵录入以后（这里只管存）]【SerMoveRotSnapDataSaver：存到缓存中】【（取）Update：分帧还原】
        /// </summary>
        /// <param name="data"></param>
        ///新现象4条数据直接发过来 s-s-s-E ；s-s-s-s-s-s-s-s-s-E（也不是分帧执行）
        private void OnAOIMsg(MessageHandleData data)
        {
            //{
            //    ProtoMsg.AOIMsg aoiMsg = (ProtoMsg.AOIMsg)data.data;
            //    SGF.Debuger.Log($"属性同步 网络层下发------------------------------------------------------------------------------------------");


            //    SGF.Debuger.Log($"属性同步 网络层下发---------------------------------- EnterAOIs IsEnd={aoiMsg.IsEnd},count={aoiMsg.EnterAOIs.Count}");
            //    for (int i = 0; i < aoiMsg.EnterAOIs.Count; i++)
            //    {
            //        {
            //            ProtoMsg.PropSyncList psl = aoiMsg.EnterAOIs[i].Prop;
            //            E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
            //            //if (entityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
            //            {
            //                SGF.Debuger.Log($"属性同步 网络层下发 --[进入]-- AOI缓存 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityType}");
            //            }
            //        }
            //    }
            //    SGF.Debuger.Log($"属性同步 网络层下发---------------------------------- UpdateAOIs  count={aoiMsg.UpdateAOIs.Count}");
            //    for (int i = 0; i < aoiMsg.UpdateAOIs.Count; i++)
            //    {
            //        {
            //            ProtoMsg.PropSyncList psl = aoiMsg.UpdateAOIs[i].Prop;
            //            EntityCtrlBase entityCtrlBase = GetEntityCtr(psl.EntityID);
            //            if (entityCtrlBase != null && entityCtrlBase.M_Curr != null)
            //            {
            //                //if (entityCtrlBase.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
            //                {
            //                    SGF.Debuger.Log($"属性同步 网络层下发 --[更新]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityCtrlBase.M_Curr.EntityType}");
            //                }
            //            }
            //        }
            //    }
            //    SGF.Debuger.Log($"属性同步 网络层下发---------------------------------- LeaveAOIs  count={aoiMsg.LeaveAOIs.Count}");
            //    for (int i = 0; i < aoiMsg.LeaveAOIs.Count; i++)
            //    {
            //        {
            //            var entity = GetEntityCtr(aoiMsg.LeaveAOIs[i].EntityID);
            //            if (entity != null && entity.M_Curr != null)
            //            {
            //                //if (entity.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != aoiMsg.LeaveAOIs[i].EntityID)
            //                {
            //                    SGF.Debuger.Log($"属性同步 网络层下发 --[删除]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={aoiMsg.LeaveAOIs[i].EntityID},type={entity.M_Curr.EntityType}");
            //                }
            //            }
            //        }
            //    }
            //    SGF.Debuger.Log($"属性同步 网络层下发=============================================================================================");
            //}
            if (IsLockGameMsgHandle)
            {
                SGF.Debuger.LogWarning("初始登录 登录流程没走完 不处理 AOIMsg");
                return;
            }

            ProtoMsg.AOIMsg aoiMsg = (ProtoMsg.AOIMsg)data.data;
            HandleAoiMsg(aoiMsg);


            //return;


            //return;

            //{
            //    aoiMsg = (ProtoMsg.AOIMsg)data.data;
            //    ///【网络拆开包裹 组合成 整体包裹】
            //    /// 【一次数据可能分包由isEnd和（Start）拼接起来】
            //    /// 【拼接起来才能执行】
            //    /// [IsEnd == true，就是合并存储标签]
            //    /// 本条数据不论是否End都要录入
            //    /// RPC别忘记处理
            //    SerMoveRotSnapDataSaver._aOIMsgList.Add(aoiMsg);
            //    //SerMoveRotSnapDataSaver._frameIndex = FrameIndex;
            //    if (aoiMsg.IsEnd)
            //    {
            //        serMoveRotSnapDataSaver.IsFullMessage =
            //            true; //写和读分离了，那边不会处理了。单条数据没问题（直接end，或者Start-End组没问题），拥堵数据（不是-是-可用）按照（非帧）响应拼接多个（来多少录入多少）；
            //                  //分离数据（来了一个，长时间不能用，等待第二个）
            //                  //是结尾 和 默认情况给我结尾
            //        serMoveRotSnapDataSaver = null; //接触下次启新的数据
            //    }
            //    else
            //    {
            //        // 一帧数据太大，分俩次发送
            //        // 这里不结尾
            //        // Start(非end)就持续堆积到一帧
            //    }
            //    //SGF.Debuger.LogWarning($"属性同步 ++++++++++++++++++++++++++++++++++++ 【缓存】AOI数据 ++++++++++++++++++++++++++++++++++++ ");

            //    //SGF.Debuger.Log($"属性同步 网络层下发---------------------------------- EnterAOIs IsEnd={aoiMsg.IsEnd},count={aoiMsg.EnterAOIs.Count},FrameIndex={FrameIndex}");
            //    //for (int i = 0; i < aoiMsg.EnterAOIs.Count; i++)
            //    //{
            //    //    {
            //    //        ProtoMsg.PropSyncList psl = aoiMsg.EnterAOIs[i].Prop;
            //    //        E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
            //    //        if (entityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
            //    //        {
            //    //            SGF.Debuger.Log($"属性同步 网络层下发 --[进入]-- AOI缓存 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityType}");
            //    //        }
            //    //    }
            //    //}
            //    //SGF.Debuger.LogError($"属性同步 网络层下发---------------------------------- UpdateAOIs  count={aoiMsg.UpdateAOIs.Count}");
            //    //for (int i = 0; i < aoiMsg.UpdateAOIs.Count; i++)
            //    //{
            //    //    {
            //    //        ProtoMsg.PropSyncList psl = aoiMsg.UpdateAOIs[i].Prop;
            //    //        EntityCtrlBase entityCtrlBase = GetEntityCtr(psl.EntityID);
            //    //        if (entityCtrlBase != null && entityCtrlBase.M_Curr != null)
            //    //        {
            //    //            if (entityCtrlBase.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
            //    //            {
            //    //                SGF.Debuger.Log($"属性同步 网络层下发 --[更新]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityCtrlBase.M_Curr.EntityType},data={psl.Prop.Prop}");
            //    //            }
            //    //        }
            //    //    }
            //    //}
            //    //SGF.Debuger.LogError($"属性同步 网络层下发---------------------------------- LeaveAOIs  count={aoiMsg.LeaveAOIs.Count}");
            //    //for (int i = 0; i < aoiMsg.LeaveAOIs.Count; i++)
            //    //{
            //    //    {
            //    //        var entity = GetEntityCtr(aoiMsg.LeaveAOIs[i].EntityID);
            //    //        if (entity != null && entity.M_Curr != null)
            //    //        {
            //    //            if (entity.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != aoiMsg.LeaveAOIs[i].EntityID)
            //    //            {
            //    //                SGF.Debuger.Log($"属性同步 网络层下发 --[删除]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={aoiMsg.LeaveAOIs[i].EntityID},type={entity.M_Curr.EntityType}");
            //    //            }
            //    //        }
            //    //    }

            //    //    for (int j = 0; j < aoiMsg.EnterAOIs.Count; j++)
            //    //    {
            //    //        {
            //    //            ProtoMsg.PropSyncList psl = aoiMsg.EnterAOIs[j].Prop;
            //    //            if (psl.EntityID == aoiMsg.LeaveAOIs[i].EntityID)
            //    //            {
            //    //                SGF.Debuger.LogError($"属性同步 网络层下发 --[删除][又进又出了]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={aoiMsg.LeaveAOIs[i].EntityID}");
            //    //            }
            //    //        }
            //    //    }

            //    //}
            //    //SGF.Debuger.LogWarning($"属性同步 ------------------------------- 【缓存】AOI数据 ------------------------------- ");
            //}
        }

        private void HandleAoiMsg(ProtoMsg.AOIMsg aoiMsg)
        {
            if (aoiMsg == null)
            {
                return;
            }

            //SGF.Debuger.Log($"属性同步 ----------------------------------- 处理数据进入AOI的数据 Count={aoiMsg.EnterAOIs.Count},curIndex={FrameIndex}");
            //SGF.Debuger.Log($"属性同步 ++++++++++++++++++++++++++++++++++++ 【处理】AOI数据 ++++++++++++++++++++++++++++++++++++ ");

            /// 2024/1/9
            /// AOIMsg 目前服务器 没办法告诉客户端 是否是第一次全同步过来的数据(实体刚进场景, aoi 不是理解同步给客户端)。
            /// 此处 客户端 没办法 根据AOIMsg 判定 是否要全部删除 本地的实体数据.
            /// 
            /// 之前的做法 是 切副本/切地图 等会 删除所有的 aoi实体.
            /// 
            /// 所以 AOI实体 在断线重连的时候 没办法 做不删本地实体 去增量同步
            /// 
            OnEnterAOIs(aoiMsg.EnterAOIs);

            OnUpdateAOIs(aoiMsg.UpdateAOIs);

            OnLeaveAOIs(aoiMsg.LeaveAOIs);


            //SGF.Debuger.LogWarning($"属性同步 网络层下发------------------------------------------------------------------------------------------");


            //SGF.Debuger.LogWarning($"属性同步 网络层下发---------------------------------- EnterAOIs IsEnd={aoiMsg.IsEnd},count={aoiMsg.EnterAOIs.Count}");
            //for (int i = 0; i < aoiMsg.EnterAOIs.Count; i++)
            //{
            //    {
            //        ProtoMsg.PropSyncList psl = aoiMsg.EnterAOIs[i].Prop;
            //        E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
            //        //if (entityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
            //        {
            //            SGF.Debuger.LogWarning($"属性同步 网络层下发 --[进入]-- AOI缓存 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityType}");
            //        }
            //    }
            //}
            //SGF.Debuger.LogWarning($"属性同步 网络层下发---------------------------------- UpdateAOIs  count={aoiMsg.UpdateAOIs.Count}");
            //for (int i = 0; i < aoiMsg.UpdateAOIs.Count; i++)
            //{
            //    {
            //        ProtoMsg.PropSyncList psl = aoiMsg.UpdateAOIs[i].Prop;
            //        EntityCtrlBase entityCtrlBase = GetEntityCtr(psl.EntityID);
            //        if (entityCtrlBase != null && entityCtrlBase.M_Curr != null)
            //        {
            //            //if (entityCtrlBase.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
            //            {
            //                SGF.Debuger.LogWarning($"属性同步 网络层下发 --[更新]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityCtrlBase.M_Curr.EntityType}");
            //            }
            //        }
            //    }
            //}
            //SGF.Debuger.LogWarning($"属性同步 网络层下发---------------------------------- LeaveAOIs  count={aoiMsg.LeaveAOIs.Count}");
            //for (int i = 0; i < aoiMsg.LeaveAOIs.Count; i++)
            //{
            //    {
            //        var entity = GetEntityCtr(aoiMsg.LeaveAOIs[i].EntityID);
            //        if (entity != null && entity.M_Curr != null)
            //        {
            //            //if (entity.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != aoiMsg.LeaveAOIs[i].EntityID)
            //            {
            //                SGF.Debuger.LogWarning($"属性同步 网络层下发 --[删除]--AOI缓存 第{EnityChangeTime}次AOI 实体ID={aoiMsg.LeaveAOIs[i].EntityID},type={entity.M_Curr.EntityType}");
            //            }
            //        }
            //    }
            //}
            //SGF.Debuger.LogWarning($"属性同步 网络层下发=============================================================================================");

            //SGF.Debuger.Log($"属性同步 ----------------------------------- 【处理】AOI数据 ----------------------------------- ");
        }

        private void OnEnterAOIs(RepeatedField<ProtoMsg.EnterAOI> enterAOIs)
        {
            for (int i = 0; i < enterAOIs.Count; i++)
            {
                //if (GameManager.Instance.m_listEntityCtrl.Count < 2)
                {
                    ProtoMsg.PropSyncList psl = enterAOIs[i].Prop;
                    //{
                    //    E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
                    //    if (entityType == E_EntityType.Player && mainPlayerId != psl.EntityID)
                    //    {
                    //        SGF.Debuger.LogWarning($"属性同步 【进入】--AOI执行 第{EnityChangeTime}次AOI 实体ID={psl.EntityID},type={entityType}");
                    //    }
                    //}
                    {
                        E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
                        if (entityType == E_EntityType.Npc || entityType == E_EntityType.Interact)
                        {
                            var entity = GetEntityCtr(psl.EntityID);
                            if (entity != null)
                            {
                                //SGF.Debuger.LogWarning($"属性同步 【进入】--AOI执行 实体ID={psl.EntityID},type={entityType},NPC和本地交互物件，我自己创建");
                                continue;
                            }
                        }
                    }
                    CreateEntity(psl);
                }
            }
        }

        private void OnUpdateAOIs(RepeatedField<ProtoMsg.UpdateAOI> updateAOIs)
        {
            // AOI 更新实体信息 更新 更新不发送 EntityType
            for (int i = 0; i < updateAOIs.Count; i++)
            {
                // TIPS：曲 之前是因为客户端少收到一次服务器数据
                // 新进入范围的人必走EnterAOIs，UpdateAOIs进来的不合法
                //SGF.Debuger.LogError($"死亡测试 updateAOIs Prop={updateAOIs[i].Prop}");
                //这里以【服务器】【世界】设定为主，如果我先加入，其他人后加入则服务器判定为（其他玩家是创建服务器才认定为enter）
                //服务器逻辑是以【时间轴】为设定的，而客户端并非如此，切不可以客户端为设计修改协议
                //如别人为先，则别人update过来；若我先，则别人Create进来；在上层处理中转下
                global::ProtoMsg.PropSyncList child = updateAOIs[i].Prop;
                var entityBase = GetEntityCtr(child.EntityID);
                if (entityBase == null)
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} 属性同步 AOI 和服务器说得不一样，这人=>{child.EntityID} 没走EnterAOIs 实体类型={child.EntityType}"); //{TimeUtils.GetServerTimeNow().ToString()}
                    continue;
                }

                //{
                //    if (entityBase != null && entityBase.M_Curr != null)
                //    {
                //        if (entityBase.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != updateAOIs[i].Prop.EntityID)
                //        {
                //            SGF.Debuger.LogWarning($"属性同步 【更新】--AOI执行 第{EnityChangeTime}次AOI 实体ID={updateAOIs[i].Prop.EntityID},type={entityBase.M_Curr.EntityType}");
                //        }
                //    }
                //}
                UpdateClientNttDataByEntityID(child);
            }
        }

        private void OnLeaveAOIs(RepeatedField<ProtoMsg.LeaveAOI> leaveAOIs)
        {
            // AOI 离开的实体 删除
            for (int i = 0; i < leaveAOIs.Count; i++)
            {
                if (leaveAOIs[i].EntityID == mainPlayerId)
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} AOI 删除 不能删除主角 ");
                    continue;
                }
                //SGF.Debuger.LogError($"死亡测试 LeaveAOI Prop={leaveAOIs[i]}");

                // 服务器讨论的
                // 子弹：判断运行时结束 删除 。。AOI离开标记(目前是直接删除)
                // 其他实体 活着、AOI离开了，删除
                // 其他实体： 死了，死亡状态播放完了、AOI离开了，删除

                // 博哥意思：
                // 人：直接离开(服务器控制类的实体客户端模拟死亡，其他都是收到离开就删除)
                //指令处理
                //var entity = GetEntityCtr(leaveAOIs[i].EntityID);
                ////{
                ////    {
                ////        if (entity != null && entity.M_Curr != null)
                ////        {
                ////            if (entity.M_Curr.EntityType == E_EntityType.Player && mainPlayerId != leaveAOIs[i].EntityID)
                ////            {
                ////                SGF.Debuger.LogWarning($"属性同步 【删除】--AOI执行 第{EnityChangeTime}次AOI 实体ID={leaveAOIs[i].EntityID},type={entity.M_Curr.EntityType}");
                ////            }
                ////        }
                ////    }
                ////}
                //if (entity != null && entity.EntityType != E_EntityType.Player.ToString() && !entity.M_Curr.M_IsAlive)
                //{
                //    continue;
                //}
                //SGF.Debuger.LogError($"死亡测试 LeaveAOI2222 Prop={leaveAOIs[i]}");
                {
                    var entity = GetEntityCtr(leaveAOIs[i].EntityID);
                    if (entity != null && (entity.EntityType == E_EntityType.Npc || entity.EntityType == E_EntityType.Interact) && !entity.IsServerAOI)
                    {
                        //SGF.Debuger.Log($"属性同步 【进入】--AOI执行 实体ID={leaveAOIs[i].EntityID},type={entity.EntityType},NPC和本地交互物件，我自己 删除");
                        continue;
                    }
                }
                GameCommand _gameCommand = gameAoiCommand.Init(E_Command.Destroy, leaveAOIs[i].EntityID, null);
                _gameCommand.isServerAOI = true;
                EntityDataCommand(_gameCommand);
            }
        }

        private void CreateEntity(ProtoMsg.PropSyncList psl, bool isServerAOI = true)
        {
            if (!CheckNeedUpdateNttData(psl))
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} 属性同步 AOI 数据有问题 EntityID={psl.EntityID},type={psl.EntityType},time={TimeUtils.ServerNowStampMilli} ");
                return;
            }

            if (GetEntityCtr(psl.EntityID) != null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} 属性同步 AOI 重复创建了相同的id EntityID={psl.EntityID},type={psl.EntityType},time={TimeUtils.ServerNowStampMilli} ");
                return;
            }

            if (psl.EntityID == mainPlayerId)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} 属性同步 AOI 不能创建主角ID EntityID={psl.EntityID},type={psl.EntityType},time={TimeUtils.ServerNowStampMilli} ");
                return;
            }

            EntityBaseData vsd = CreateEntityData(psl, isServerAOI);
            if (vsd == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} 属性同步 AOI 创建获取 实体数据失败 EntityID={psl.EntityID},type={psl.EntityType},vsd={vsd},time={TimeUtils.ServerNowStampMilli}");
            }
            else
            {
                //E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
                //SGF.Debuger.Log($"{LOG_TAG} 属性同步 进入AOI id={psl.EntityID},type={entityType}"); //{TimeUtils.GetServerTimeNow().ToString()}
            }
        }

        private bool CheckNeedUpdateNttData(ProtoMsg.PropSyncList psl)
        {
            //if (!psl.HasEntityID || !psl.HasEntityType)
            //{
            //    return false;
            //}

            // 服务器现在会给本人的数据，直接过滤
            if (mainPlayerId == psl.EntityID)
            {
                return false;
            }

            return true;
        }

        private EntityBaseData CreateEntityData(ProtoMsg.PropSyncList psl, bool isServerAOI = true)
        {
            EntityBaseData vsd = null;

            E_EntityType entityType = (E_EntityType)Enum.Parse(typeOfEntityType, psl.EntityType);
            E_EntityType dataType = entityType;
            switch (entityType)
            {
                case E_EntityType.Player:
                    {
                        vsd = new VitalSignData();
                        (vsd as VitalSignData).isMainPlayer = mainPlayerId == psl.EntityID;
                    }
                    break;
                case E_EntityType.Npc:
                case E_EntityType.Summon:
                case E_EntityType.Partner:
                case E_EntityType.BulletEntity:
                    {
                        // 策划和 服务器定义的子弹,其实是我们客户端 定义的召唤物,它基本具备被动的所有能力
                        // vsd = DynamicDataFactory.InstanceData<NoneVitalSignData>(psl.EntityID);
                        // vsd = DynamicDataFactory.EarliestData<NoneVitalSignData>();
                        // vsd = new NoneVitalSignData();
                        // (vsd as NoneVitalSignData).dataType = E_EntityDataType.Bullet;
                        vsd = new VitalSignData();
                    }
                    break;
                case E_EntityType.Monster:
                case E_EntityType.GVEBoss:
                case E_EntityType.Robot:
                    {
                        vsd = new VitalSignData();
                        dataType = E_EntityType.Monster;
                        (vsd as VitalSignData).IsGVEBoss = entityType == E_EntityType.GVEBoss;
                        (vsd as VitalSignData).IsRobot = entityType == E_EntityType.Robot;
                    }
                    break;
                case E_EntityType.Interact:
                    {
                        vsd = new NoneVitalSignData();
                    }
                    break;
            }

            if (vsd == null)
            {
                return vsd;
            }
            vsd.IsFullMessage = true;
            //SGF.Debuger.LogError($"属性同步创建 --开始-- 时间={TimeUtils.TimeLogString()}");
            //先创建等待数据你不知道什么类型的物件
            //【先创建数据，后续再推一次流程不正常，但是创建请求少了两次回调用】，不如全中断解耦，异步gob创建好以后来取；用数据层，创建类型，类型创建好gob后然后取得data
            //应该是集合在一起，数据层推送，逻辑层次创建（表现层异步gob加载）表现回调逻辑层，创建完毕后回调Data设置赋值，两次请求两次回调
            vsd.Create(psl.EntityID, dataType, isServerAOI);
            //先别删，子弹这块 crate需要直接用到attr
            UpdateEntityData(psl, vsd, true);
            //SGF.Debuger.LogError($"属性同步创建 --结束-- 时间={TimeUtils.TimeLogString()}");
            //SGF.Debuger.LogError($"属性同步创建 实体--开始-- 时间={TimeUtils.TimeLogString()}");
            GameCommand _gameCommand = gameAoiCommand.Init(E_Command.Create, 0, vsd);
            _gameCommand.entityBaseData.Pos = GetAOIBornPos(psl);
            _gameCommand.isServerAOI = true;
            EntityDataCommand(_gameCommand); //todo:数据有必要生成吗
            //SGF.Debuger.LogError($"属性同步创建 实体--结束-- 时间={TimeUtils.TimeLogString()}");
            UpdateEntityData(psl, vsd, false);

            return vsd;
        }

        private UnityEngine.Vector3 GetAOIBornPos(ProtoMsg.PropSyncList pbsAttrList)
        {
            UnityEngine.Vector3 tempV3 = UnityEngine.Vector3.zero;
            ProtoMsg.PropBaseSyncList prop = pbsAttrList.Prop;
            for (int i = 0; i < prop.Prop.Count; i++)
            {
                ProtoMsg.SyncBaseInfo syncBaseInfo = prop.Prop[i];
                try
                {
                    VitalSignAOIClientAttrs vitalSignAOIClientAttrs =
                        LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)syncBaseInfo.Index);

                    if (vitalSignAOIClientAttrs == null)
                    {
                        continue;
                    }

                    if (vitalSignAOIClientAttrs.Name == AOIAttrDefine.Position)
                    {
                        ByteString vector3Msg = syncBaseInfo.MsgValue;
                        byte[] msgData = vector3Msg.ToByteArray();
                        IMessage pbMessage = ProtoUtils.Deserialize(
                             (int)MsgIDEnum.Vector3ID, msgData);
                        ProtoMsg.Vector3 protoPos = (ProtoMsg.Vector3)pbMessage;
                        tempV3.x = protoPos.X;
                        tempV3.y = protoPos.Y;
                        tempV3.z = protoPos.Z;

                        return tempV3;
                    }
                }
                catch (System.Exception)
                {
                    SGF.Debuger.LogWarning($"[EntityBaseData] sync index : {(ushort)syncBaseInfo.Index} error!!!!");
                }
            }

            return tempV3;
        }

        private void UpdateEntityData(ProtoMsg.PropSyncList psl, EntityBaseData vsd, bool isContrast)
        {
            vsd.UpdateWithAttr(psl.Prop, isContrast);
        }

        private void UpdateClientNttDataByEntityID(ProtoMsg.PropSyncList psl)
        {
            //SGF.Debuger.Log($" AOI 更新 英雄id=>{psl.EntityID} 时间=》{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}");

            if (!CheckNeedUpdateNttData(psl))
            {
                return;
            }

            //指令处理
            GameCommand _gameCommand = gameAoiCommand.Init(E_Command.Update, psl.EntityID, null);
            EntityBaseData vsd = EntityDataCommand(_gameCommand);
            if (vsd == null)
            {
                return;
            }

            //更新对应实体数据
            UpdateEntityData(psl, vsd, true);
        }

        #region 主角属性同步

        public HeroMD HeroMD
        {
            get
            {
                var mgr = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Hero);
                if (mgr != null)
                {
                    var heroMDMgr = mgr as HeroMDMgr;
                    {
                        if (heroMDMgr != null)
                            return heroMDMgr.GetHeroMD();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 获得主角的总技能等级
        /// </summary>
        public int HeroSkillTotalLevel()
        {
            if (HeroMD == null)
            {
                return 0;
            }

            int level = 0;
            HeroMD.JobSkillModel.AllSkill.ForEach(skillinfo => { level += skillinfo.SkillLevel; });

            return level;
        }

        /// <summary>
        /// 同步主角自己的属性
        /// </summary>
        /// <param name="data"></param>
        private void OnPropSyncList(MessageHandleData data)
        {
            // TODO：夏哥说，aoi数据是aoi，主角的属性同步预加载地图的时候不应该丢
            //if (IsLockGameMsgHandle)
            //{
            //    SGF.Debuger.LogWarning("初始登录 登录流程没走完 不处理 OnPropSyncList");
            //    return;
            //}

            ProtoMsg.PropSyncList propSyncList = (ProtoMsg.PropSyncList)data.data;
            HandlePropSync(propSyncList);
            // CacheRPCMsg(data);
            return;
            propSyncList = (ProtoMsg.PropSyncList)data.data;
            SerMoveRotSnapDataSaverMainPlayer._propSyncList = propSyncList;
            SerMoveRotSnapDataSaverMainPlayer.IsFullMessage = true;
            serMoveRotSnapDataSaverMainPlayer = null;
            //SGF.Debuger.LogError($"属性同步 22222222222222222222222222222222222==={FrameIndex}");

            #region 直接显示主角AOI数据中的原子锁

            //SGF.Debuger.LogError($"主角属性同步 updateAOIs Prop={propBaseSyncList}");
            for (int i = 0; i < propSyncList.Prop.Prop.Count; i++)
            {
                ProtoMsg.SyncBaseInfo syncBaseInfo = propSyncList.Prop.Prop[i];
                try
                {
                    VitalSignAOIClientAttrs vitalSignAOIClientAttrs =
                        LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)syncBaseInfo.Index);

                    if (vitalSignAOIClientAttrs == null)
                    {
                        continue;
                    }

                    if (vitalSignAOIClientAttrs.Name == AOIAttrDefine.State)
                    {
                        object val = syncBaseInfo.GetType().GetProperty(syncBaseInfo.PropValueCase.ToString())
                            .GetValue(syncBaseInfo, null);

                        HandleOnStateChange((E_BattleStateType)Convert.ToUInt32(val));
                    }
                }
                catch (System.Exception)
                {
                    SGF.Debuger.LogError($"[EntityBaseData] sync index : {(ushort)syncBaseInfo.Index} error!!!!");
                }
            }

            #endregion
        }

        #region 直接显示主角AOI数据中的原子锁

        private bool isServerForbidMove = false;
        private bool isServerForbidDir = false;
        private E_BattleStateType serverBattleStateType = E_BattleStateType.BattleStateType;

        /// <summary>
        /// 更新状态【原子锁】
        /// </summary>
        /// <param name="state"></param>
        private void HandleOnStateChange(E_BattleStateType state)
        {
            E_BattleStateType changeState = serverBattleStateType ^ state; //异或，不同位置是：1
            if (changeState == E_BattleStateType.BattleStateType)
            {
                return;
            }

            serverBattleStateType = state;

            CheckServerStateTest();
        }

        private void CheckServerStateTest()
        {
            bool isForbidMove = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidMove);
            if (isServerForbidMove != isForbidMove)
            {
                isServerForbidMove = isForbidMove;
                M_MainPlayerCtrlBase.Data.ActionStateChange?.Invoke(E_BattleStateType.BattleState_ForbidMove,
                    isServerForbidMove, 2);
            }

            bool isForbidDir = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidDir);
            if (isServerForbidDir != isForbidDir)
            {
                isServerForbidDir = isForbidDir;
                M_MainPlayerCtrlBase.Data.ActionStateChange?.Invoke(E_BattleStateType.BattleState_ForbidDir,
                    isServerForbidDir, 2);
            }
        }

        public bool CheckStateIsServerForbid(E_BattleStateType state)
        {
            bool isForbid = false;
            isForbid = CheckState(serverBattleStateType, state, true);
            ////SGF.Debuger.Log($" {TagFlag} CheckStateIsServerForbid id => {M_EntityID}  battleStateType ===>{serverBattleStateType} , checkState ==> {state}  isForbid {isForbid}");

            return isForbid;
        }

        private bool CheckState(E_BattleStateType state, E_BattleStateType checkState, bool equal)
        {
            if (equal)
            {
                return (state & checkState) == checkState;
            }

            return (state & checkState) != checkState;
        }

        #endregion


        // 临时解决
        public List<ProtoMsg.PropSyncList> CacheMainPlayerPropSyncList = new();

        private void HandlePropSync(ProtoMsg.PropSyncList propSyncList)
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                CacheMainPlayerPropSyncList.Add(propSyncList);
                return;
            }

            if (propSyncList == null)
            {
                return;
            }
            //SGF.Debuger.LogError($"主1角属性同步 ---------------------------------------------------- ");

            //foreach (var item in propSyncList.Prop.Prop)
            //{
            //    VitalSignAOIClientAttrs attrType = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)item.Index);
            //    if (attrType != null)
            //    {
            //        object val = item.GetType().GetProperty(item.PropValueCase.ToString()).GetValue(item, null);
            //        SGF.Debuger.LogWarning($"主1角属性同步 属性={attrType.Name},值={val}");
            //    }
            //}
            //SGF.Debuger.LogError($"主1角属性同步 ======================================================== ");

            UpdateEntityData(propSyncList, M_MainPlayerCtrlBase.Data, true);
        }

        public void UseCacheMainPlayerPropSyncList()
        {
            foreach (var item in CacheMainPlayerPropSyncList)
            {
                HandlePropSync(item);
            }
            SGF.Debuger.LogWarning($"{LOG_TAG} 初始登录 UseCacheMainPlayerPropSyncList 缓存处理主角属性 count={CacheMainPlayerPropSyncList.Count}");

            CacheMainPlayerPropSyncList.Clear();
        }

        #endregion

        #endregion

        #region RPC协议

        private List<MessageHandleData> CacheRPCMsgList = new();

        public void CacheRPCMsg(MessageHandleData messageHandleData)
        {
            //SGF.Debuger.LogError($"[GameManager] CacheRPCMsg : {messageHandleData.messageName}");
            RPCOneFrameSnapALLData._rPCMsgList.Add(messageHandleData);
            RPCOneFrameSnapALLData.IsFullMessage = true;

            //EntityCtrlBase entityCtrlBase = GetEntityCtr(messageHandleData.enityId);
            //if (entityCtrlBase != null)
            //{
            //    if (entityCtrlBase.M_Curr.EntityType == E_EntityType.Monster)
            //    {
            //        entityCtrlBase.HandleRpcMsg2(messageHandleData);
            //    }
            //}
        }

        public void HandleRPCMsg(MessageHandleData messageHandleData, bool isCacheData = false)
        {
            // TODO：夏哥说，aoi数据是aoi，主角的属性同步预加载地图的时候不应该丢

            //if (IsLockGameMsgHandle)
            //{
            //    SGF.Debuger.LogWarning("初始登录 登录流程没走完 不处理 HandleRPCMsg");
            //    return;
            //}

            if (messageHandleData == null)
            {
                return;
            }

            EntityCtrlBase entityCtrlBase = GetEntityCtr(messageHandleData.enityId);
            if (entityCtrlBase != null)
            {
                entityCtrlBase.HandleRpcMsg(messageHandleData);
            }
            else
            {
                if (!isCacheData)
                {
                    CacheRPCMsgList.Add(messageHandleData);
                }
                SGF.Debuger.LogWarning($"[MessageCtr] HandleRPCMsg: cmd: {messageHandleData.messageCmd} , messageName: {messageHandleData.messageName} 没有主角!!!!");
            }
        }

        public void UseCacheRPCMsgList()
        {
            foreach (var item in CacheRPCMsgList)
            {
                HandleRPCMsg(item, true);
            }
            SGF.Debuger.LogWarning($"{LOG_TAG} 初始登录 UseCacheRPCMsgList 缓存处理RPC数据 count={CacheRPCMsgList.Count}");

            CacheRPCMsgList.Clear();
        }

        #endregion

        /// <summary>
        /// 游戏的上下文
        /// 如一局竞技场客户端需要记录的角色移动步数，并且由客户端自己控制胜负成果，本局结束后即释放
        /// 在比如到50步需要做某些，进行计数功能，只在本【局】模式下临时的【上下文】影响数据
        /// </summary>
        private GameContext m_context;

        /// <summary>
        /// 地图
        /// </summary>
        private GameMap m_map;

        public string OldAreaName = "";

        public GameMap M_Map
        {
            get { return m_map; }
        }

        private List<EntityCtrlBase> m_listEntityCtrl = new();

        public List<EntityCtrlBase> M_listEntityCtrl
        {
            get { return m_listEntityCtrl; }
        }

        /// <summary>
        /// 临时在来一个dic,方便查找,只是m_listEntityCtrl 的一份拷贝用map存储
        /// note:
        /// 原来是，组，人，nttid，原来是，组，人，nttid
        /// 一个是用来存储，一个是用来查找;
        /// 【所有实体】
        /// </summary>
        public DictionaryEx<ulong, EntityCtrlBase> m_mapEntityCtrlCopy = new();

        public DictionaryEx<ulong, EntityBaseData> m_mapEntityData = new();

        public event PlayerDieEvent onPlayerDie;

        ////======================================================================
        public bool IsRunning
        {
            get { return m_isRunning; }
        }

        public GameContext Context
        {
            get { return m_context; }
        }
        //public GameMode GameMode { get { return m_context.param.mode; } }
        ////======================================================================

        public GameCommand gameCommand = new();
        public GameCommand gameAoiCommand = new();

        // 是否打开了道具tips 标识
        public bool IsOpenItemTips = false;

        // 是否打开了功能tips 标识
        public bool IsOpenFuncTips = false;

        // 是否打开了对话 标识
        public bool IsOpenTalkBox = false;

        private LoadingView m_LoadingView;

        public LoadingView Loading
        {
            get

            {
                if (m_LoadingView == null)
                {
                    m_LoadingView = UIManager.Instance.GetLoadingView();
                }

                return m_LoadingView;
            }
        }

        public void CreateGame(GameParam param)
        {
            if (m_isRunning)
            {
                Debuger.LogError(LOG_TAG, "Create() Game Is Runing Already!");
                return;
            }

            //Debuger.Log(LOG_TAG, "Create() param:{0}", param);

            /* ////创建上下文，保存战斗全局参数
             m_context = new GameContext();
             //m_context.param = param;
             m_context.random.Seed = param.randSeed;
             m_context.currentFrameIndex = 0;*/

            ////初始化工厂
            EntityFactory.Init();

            ViewFactory.Init(EntityRoot.Instance.RemoveRoot.transform);
            //因为游戏主流程开启，不会因为模式开启：AOI接收器（虽然服务器这块也应该调整）
            //DynamicDataFactory.Init();

            //SerSnapshotSeqManager.Instance.Init();

            ////初始化摄像机
            M_GameCamera = CameraManager.Instance.SwitchCamera(E_CameraType.StarWorldCam);
            //GameCamera.Create();

            m_isRunning = true;

            SetReportNewPlayerLog(false);
            m_ReportNewPlayerLogCache.Clear();
            m_SpecialFlagIndex = 1;
            ///////测试
            ///场景陨石实体
            //TriggerEntityManager.Instance.AddEnvTriggerNtt(EnumEnityListKey.Env_EntityFx);
            ///场景UI小地图实体
            //TriggerManager.Instance.AddUITriggerNtt(EnumEnityListKey.UI_Fx); 
        }

        public void ReleaseGame()
        {
            if (!m_isRunning)
            {
                return;
            }

            // BattleManager.Instance.SetCurAtkEntity(null);
            // ReleaseGame 清空 索敌队列
            BattleManager.Instance.ReleaseGame();
            M_GameCamera = null;
            onPlayerDie = null;
            MainPlayerEnityId = 0;
            DynamicUIRoot.EntityUIRoot.GetComponent<CanvasGroup>().alpha = 1f;
            DynamicUIRoot.DamageUIRoot.GetComponent<CanvasGroup>().alpha = 1f;

            m_isRunning = false;
            GameCamera.Release();

            ReleaseAllEntity();
            ReleaseAllLocalEntity();
            AllWantedInfo.Clear();

            //m_listFood.Clear();

            ViewFactory.Release();
            EntityFactory.Release();
            //模式结束也不会释放
            //DynamicDataFactory.Release();

            ReleaseMap();
            NetworkManager.Instance.Close();
            FixMessageManager.Instance.Close();
            CameraManager.Instance.ReleasePlayerFlowTarget();

            SetReportNewPlayerLog(false);
            RedPointManager.Instance.Clear();
            m_SpecialFlagIndex = 1;

            m_ReportNewPlayerLogCache.Clear();
            //OffEventMessage();    // 重启login时，APPMain只会执行一次，卸载收不到消息
        }

        #region 地图Map逻辑

        public void ChangeMap(int mapID)
        {
            var battleSocket = NetworkManager.Instance.gameSocket;
            MapChangeReq mapChangeReqMsg = new();
            mapChangeReqMsg.MapID = mapID;
            mapChangeReqMsg.ServerID = ServerIDCache;
            mapChangeReqMsg.Reason = ChangeReason.SameServer;
            battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, mapChangeReqMsg, false);
            SGF.Debuger.Log($"切换地图至 mapID={mapID} ServerID={ServerIDCache}");
        }

        public void BackToWorld()
        {
            var battleSocket = NetworkManager.Instance.gameSocket;
            LeaveInstanceReq mapChangeReqMsg = new();
            battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, mapChangeReqMsg, false);
        }

        /// <summary>
        /// 这里都是副本
        /// </summary>
        /// <param name="param"></param>
        /// <param name="newMapId"></param>
        public void CommonChangeMap(GameParam param, int newMapId, ProtoMsg.SpaceType spaceType)
        {
            //首次或者空           
            if (m_map == null)
            {
                param.mapData.MapID = newMapId;
                param.mapData.SpaceType = spaceType;
                CreateMap(param);
            }
            else
            {
                //数据不同，
                switch (m_map.CheckIsSameData(param, newMapId, spaceType))
                {
                    case E_DealMapType.Default:
                        break;
                    case E_DealMapType.SameId:
                        // 地图相同，不做改变
                        UIManager.Instance.ShowLoadingAsync(1f);
                        break;
                    case E_DealMapType.ChangeScene:
                        {
                            // 切场景
                            // 赋值
                            {
                                GlobalEvent.OnMapChange?.Invoke(param.mapData.MapID, newMapId);
                            }
                            param.mapData.MapID = newMapId;
                            param.mapData.SpaceType = spaceType;
                            CreateMap(param);
                        }
                        break;
                    case E_DealMapType.CommonChangeWaveFB:
                        {
                            // 赋值
                            {
                                GlobalEvent.OnMapChange?.Invoke(param.mapData.MapID, newMapId);
                            }
                            param.mapData.MapID = newMapId;
                            param.mapData.SpaceType = spaceType;
                            SetMap(param);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void ChanageLineSetMapData(ulong ServerID)
        {
            if (m_map != null)
            {
                m_map.ChanageLineSetMapData(ServerID);
            }
        }

        public void ChanageLineSetMapServerID(ulong ServerID, int ServerIDShow)
        {
            if (m_map != null)
            {
                m_map.ChanageLineSetMapServerID(ServerID, ServerIDShow);
            }
        }

        #region navmesh

        NavMeshQuery navMeshQuery;

        // 假设使用 Humanoid 代理类型，其 ID 为 0
        int agentTypeID = 0;

        // 假设搜索区域为一个2x2x2的立方体
        public Vector3 extents = new(GameConfig.NAVMESH_EXTEND, GameConfig.NAVMESH_STEPHEIGHT,
            GameConfig.NAVMESH_EXTEND); //容忍度最大永远合理，当成没有容忍度

        // 假设检查所有区域
        int areaMask = NavMesh.AllAreas;

        void CreateNavMeshQuery()
        {
            NavMeshWorld navMeshWorld = NavMeshWorld.GetDefaultWorld();
            Allocator allocator = Allocator.Persistent;
            int pathNodePoolSize = 0;
            //if(System.Object.Equals(navMeshQuery, null))//换场景必须换跟是否是空没关系，leak就leak
            {
                navMeshQuery = new NavMeshQuery(navMeshWorld, allocator, pathNodePoolSize);
            }
        }

        void OnSceneChanged(int sceneId)
        {
            // 切换场景后重新创建 NavMeshQuery 对象
            DestroyNavMeshQuery();
            CreateNavMeshQuery();
        }

        void DestroyNavMeshQuery()
        {
            // 销毁 NavMeshQuery 对象
            //navMeshQuery.Dispose();
        }

        public bool IsPointOnNavMesh(Vector3 testPoint)
        {
            // 判断目标坐标是否在 NavMesh 上
            NavMeshLocation location = navMeshQuery.MapLocation(testPoint, extents, agentTypeID, areaMask);
            if (navMeshQuery.IsValid(location))
            {
                // 找到了离点最近的有效多边形，点在NavMesh上
                /* Debug.Log("目标坐标在NavMesh上");
                 Vector3 positionOnNavMesh = location.position; // 获取点在NavMesh上的位置坐标*/
                // 继续执行其他操作...
                return true; // 返回 true 表示在 NavMesh 上
            }
            else
            {
                /*   // 在搜索区域内找不到包含该点的有效多边形，点不在NavMesh上
                   Debug.Log("目标坐标不在NavMesh上");
                   // 执行其他处理...*/
                return false; // 返回 false 表示不在 NavMesh 上
            }
        }

        /*    public bool IsMovePointOnNavMesh(Vector3 testPoint)
            {
                NavMeshLocation startLocation = ...;  // 当前位置的 NavMeshLocation
                Vector3 endPosition = ...;  // 目标位置的三维坐标

                // 调用 MoveLocation 方法来计算移动后的位置
                NavMeshLocation newLocation = navMeshQuery.MoveLocation(startLocation, endPosition, areaMask);

                // 检查返回的位置是否有效
                if (navMeshQuery.IsValid(newLocation))
                {
                    // 移动到目标位置成功
                    Vector3 newPosition = newLocation.position;  // 移动到的位置的三维坐标
                                                                 // 继续执行其他操作...
                }
                else
                {
                    // 移动到目标位置失败
                    Debug.Log("无法移动到目标位置");
                    // 处理移动失败的情况...
                }
            }*/

        #endregion

        //===========================================================
        private void CreateMap(GameParam param)
        {
            if (m_map != null)
            {
                // Bug: 曲
                // 曲爷 离谱的在 map.CheckIsSameData 的内部设置了 m_map.mapId = newMapID。
                // 导致 此处 CreateMap 中 mapId 已经变成了 新的 mapID.
                // 此处如果 需要在 m_map.Unload() 的时候 释放map 相关的资源, mapID 就不对了.
                // GlobalEvent.OnMapChange?.Invoke(m_map.GetMapId(), GetCurMapId());
                m_map.Unload();
            }
            else
            {
                m_map = new GameMap();
            }

            m_map.Load(param.mapData, true);
            if (m_context == null)
            {
                m_context = new GameContext();
            }

            m_context.random.Seed = param.randSeed;
            m_context.currentFrameIndex = 0;
            m_context.mapSize = m_map.Size;
        }

        private void ReleaseMap()
        {
            if (m_map != null)
            {
                m_map.Unload();
                m_map = null;
            }
        }

        public int GetCurMapId()
        {
            int mapId = -1;
            if (m_map != null)
            {
                mapId = m_map.GetMapId();
            }

            return mapId;
        }

        public ulong GetSpaceID()
        {
            ulong SpaceId = 0;
            if (m_map != null)
            {
                SpaceId = m_map.GetSpaceID();
            }

            return SpaceId;
        }

        public ulong GetCurServerID()
        {
            ulong ServerID = 0;
            if (m_map != null)
            {
                ServerID = m_map.GetServerID();
            }

            return ServerID;
        }

        public int GetCurServerIDShow()
        {
            int ServerID = 0;
            if (m_map != null)
            {
                ServerID = m_map.GetServerIDShow();
            }

            return ServerID;
        }

        public string GetCurSceneLevelName()
        {
            string name = string.Empty;
            if (m_map != null)
            {
                name = m_map.GetSceneLevelName();
            }

            return name;
        }

        public int GetBaseMapId()
        {
            int mapId = -1;

            if (m_map != null)
            {
                mapId = m_map.GetBaseMapId();
            }

            return mapId;
        }

        public ProtoMsg.SpaceType GetCurMapType()
        {
            ProtoMsg.SpaceType spaceType = ProtoMsg.SpaceType.SpaceScene;
            if (m_map != null)
            {
                spaceType = m_map.GetMapType();
            }

            return spaceType;
        }

        public string GetMapSubType()
        {
            string subType = GameConfig.INSTANCE_NORMAL;
            if (m_map != null)
            {
                subType = m_map.GetMapSubType();
            }

            return subType;
        }

        public bool GetMapIsHidePartner()
        {
            bool isHidePartner = false;
            if (m_map != null)
            {
                isHidePartner = m_map.GetMapIsHidePartner();
            }

            return isHidePartner;
        }

        public bool GetMapIsCameraRotate()
        {
            bool isCameraRotate = false;
            if (m_map != null)
            {
                isCameraRotate = m_map.GetMapIsCameraRotate();
            }

            return isCameraRotate;
        }

        public bool GetMapIsCameraTeleport()
        {
            bool isCameraTeleport = false;
            if (m_map != null)
            {
                isCameraTeleport = m_map.GetMapIsCameraTeleport();
            }

            return isCameraTeleport;
        }

        // 设置场景的【动态阻挡】信息
        public void SetObstacleInfo(Google.Protobuf.Collections.MapField<int, bool> ObstacleInfo)
        {
            if (m_map != null)
            {
                m_map.SetObstacleInfo(ObstacleInfo);
            }
        }

        /// <summary>
        /// 根据实体的配置表ID获取地图配置里面的实体是否忽略重力
        /// </summary>
        /// <param name="entityCfgID"></param>
        /// <returns></returns>
        public bool GetEntityIDIsIgnoreGravity(E_EntityType e_EntityType, long entityCfgID)
        {
            if (GameMap.sceneJsonData != null)
            {
                switch (e_EntityType)
                {
                    case E_EntityType.None:
                        break;
                    case E_EntityType.RoomSpace:
                        break;
                    case E_EntityType.Player:
                        break;
                    case E_EntityType.Npc:
                        {
                            if (GameMap.sceneJsonData.Npcs != null && GameMap.sceneJsonData.Npcs.Count > 0)
                            {
                                foreach (var areaJson in GameMap.sceneJsonData.Npcs)
                                {
                                    if (areaJson.Value.NpcID == entityCfgID)
                                    {
                                        return areaJson.Value.IgnoreGravity;
                                    }
                                }
                            }
                        }
                        break;
                    case E_EntityType.Monster:
                        {
                            if (GameMap.sceneJsonData.Monsters != null && GameMap.sceneJsonData.Monsters.Count > 0)
                            {
                                foreach (var areaJson in GameMap.sceneJsonData.Monsters)
                                {
                                    if (areaJson.Value.MonsterID == entityCfgID)
                                    {
                                        return areaJson.Value.IgnoreGravity;
                                    }
                                }
                            }
                        }
                        break;
                    case E_EntityType.BulletEntity:
                        break;
                    case E_EntityType.Interact:
                        break;
                    case E_EntityType.Summon:
                        break;
                    case E_EntityType.Partner:
                        break;
                    case E_EntityType.GVEBoss:
                        break;
                    case E_EntityType.Robot:
                        break;
                    case E_EntityType.ClientSummon:
                        break;
                    default:
                        break;
                }
            }
            //加载配置是异步的，有快有慢，默认是不浮空的绝大多数：所以通常返回是false都享受重力！！
            return false;
        }

        /// <summary>
        /// 服务器推送加载地图
        /// TODO:POS也要告诉我：当然了，那个肯定是放在人的同步信息上而不在这里
        /// </summary>
        /// <param name="param"></param>
        private void SetMap(GameParam param)
        {
            m_map.Load(param.mapData, false);
            m_context.mapSize = m_map.Size;
        }

        // -------- 主角属性全量同步是否成功
        private bool m_UserMainDataNotifyIsSuccess = true;

        public bool M_UserMainDataNotifyIsSuccess
        {
            get { return m_UserMainDataNotifyIsSuccess; }
            set
            {
                if (m_UserMainDataNotifyIsSuccess != value)
                {
                    m_UserMainDataNotifyIsSuccess = value;
                }

                if (m_UserMainDataNotifyIsSuccess)
                {
                    SetMainPlayerState();
                }
            }
        }

        // -------- 地图是否切换成功
        private bool m_ChanageMapIsSuccess = true;

        public bool M_ChanageMapIsSuccess
        {
            get { return m_ChanageMapIsSuccess; }
            set
            {
                if (m_ChanageMapIsSuccess != value)
                {
                    m_ChanageMapIsSuccess = value;
                }

                if (!m_ChanageMapIsSuccess)
                {
                    //RegisterMainPlayerClientBattleStates();
                }
                else
                {
                    SetMainPlayerState(); // 临时加的
                }
            }
        }

        // 获得地图是否切换成功
        public bool GetMapChanageIsSuccess()
        {
            bool res = m_UserMainDataNotifyIsSuccess && m_ChanageMapIsSuccess;
            return res;
        }

        private void SetMainPlayerState()
        {
            //if (m_UserMainDataNotifyIsSuccess == false)
            //{
            //    SGF.Debuger.LogWarning($"{LOG_TAG} 解开 s失败 主角移动 朝向攻击技能 m_UserMainDataNotifyIsSuccess");
            //}
            //if (m_ChanageMapIsSuccess == false)
            //{
            //    SGF.Debuger.LogWarning($"{LOG_TAG} 解开 s失败 主角移动 朝向攻击技能 m_ChanageMapIsSuccess");
            //}
            //if (GetMapChanageIsSuccess())
            //{
            //    UnRegisterMainPlayerClientBattleStates();
            //}
        }

        // 地图是否解锁
        public bool CheckMapIsOpen(int mapID)
        {
            bool isOpend = false;
            var sceneMapDataCell = LocalDataManager.Instance.GetMapCfgData(mapID);
            if (sceneMapDataCell != null)
            {
                isOpend = true;
                // 判断等级条件
                if (sceneMapDataCell.OpenRoleLv > GetPlayerLevel())
                {
                    isOpend = false;
                }

                if (isOpend)
                {
                    // 后判断任务完成条件
                    int taskID = sceneMapDataCell.OpenComTask;
                    if (taskID > 0)
                    {
                        bool isFinish = TaskHelper.IsTaskFinsh((uint)taskID);
                        if (!isFinish)
                        {
                            isOpend = false;
                        }
                    }
                }
            }

            return isOpend;
        }

        /// <summary>
        /// 获取地图区域
        /// </summary>
        /// <returns></returns>
        public Vector3 GetGameMapAreaPos(int areaid)
        {

            if (GameMap.sceneJsonData.Areas != null && GameMap.sceneJsonData.Areas.Count > 0)
            {
                var pos = GameMap.sceneJsonData.Areas[areaid].Position;
                return new Vector3(pos.x, pos.y, pos.z);
            }

            return Vector3.zero;
        }

        public Vector3 GetMapBossPos()
        {
            if (GameMap.sceneJsonData == null)
            {
                return Vector3.zero;
            }

            if (GameMap.sceneJsonData.RandomMonsters == null)
            {
                return Vector3.zero;
            }

            if (GameMap.sceneJsonData.RandomMonsters.Count <= 0)
            {
                return Vector3.zero;
            }

            foreach (var item in GameMap.sceneJsonData.RandomMonsters)
            {
                if (item.Value.MonsterType == 3)
                {
                    return item.Value.Position.Convert();
                }
            }

            return Vector3.zero;
        }

        #endregion

        public void onPlayerDie2(ulong playerId)
        {
            onPlayerDie(playerId);
        }

        #region 物理按键

        public void InputVKey(int vkey, float arg, ulong playerId)
        {
            /*  AkSoundEngine.PostEvent("Loop_Amb_forest_birds", */
            /*CameraManager.Instance.GetCamera( E_CameraType.StarWorldCam).gameObject*/ /*new GameObject());*/
            if (playerId == 0)
            {
                //处理其它VKey，全局性的VKey
                //Client辅助选项
                //HandleOtherVKey(vkey, arg, playerId);
            }
            else
            {
                PlayerCtrlGroup player = (PlayerCtrlGroup)GetEntityCtr(playerId);
                if (player != null)
                {
                    player.InputVKey(vkey, arg);
                }
                else
                {
                    //处理其它Vkey
                    //HandleOtherVKey(vkey, arg, playerId);
                }
            }
        }

        //private void HandleOtherVKey(int vkey, float arg, uint playerId)
        //{
        //    //全局的VKey处理
        //    bool hasHandled = false;
        //    hasHandled = hasHandled || DoVKey_CreatePlayer(vkey, arg, playerId);
        //    hasHandled = hasHandled || DoVKey_ReleasePlayer(vkey, arg, playerId);
        //}


        ////===========================================================================
        //private bool DoVKey_CreatePlayer(int vkey, float arg, uint playerId)
        //{
        //    if (vkey == GameVKey.CreatePlayer)
        //    {
        //        CreatePlayer(playerId);
        //        return true;
        //    }

        //    return false;
        //}

        //private bool DoVKey_ReleasePlayer(int vkey, float arg, uint playerId)
        //{
        //    if (vkey == FSPVKeyBase.GAME_EXIT)
        //    {
        //        ReleasePlayer(playerId);
        //        return true;
        //    }

        //    return false;
        //}

        #endregion

        #region 根据实体ID获取信息

        public Vector3 GetEntityUpPosById(ulong enityId)
        {
            Vector3 pos = Vector3.zero;
            if (m_mapEntityCtrlCopy.ContainsKey(enityId))
            {
                ModelOffLineData modelOffLineData = m_mapEntityCtrlCopy[enityId].Container.GetComponentInChildren<ModelOffLineData>();

                if (modelOffLineData != null)
                {
                    Transform UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.UnitInfo_D.ToString());
                    if (UnitInfoPoint != null)
                    {
                        pos = UnitInfoPoint.position;
                    }
                    else
                    {
                        UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                        if (UnitInfoPoint != null)
                        {
                            pos = UnitInfoPoint.position;
                        }
                    }
                }
                else
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} GetEntityUpPosById() enityId={enityId},modelOffLineData=null,err!!!");
                }
            }

            return pos;
        }

        public Vector3 GetEntityHangPointPos(ulong enityId, HangPoint hangPoint, bool defaultRoot, out bool findResult)
        {
            Vector3 pos = Vector3.zero;
            findResult = false;
            if (m_mapEntityCtrlCopy.ContainsKey(enityId))
            {
                ModelOffLineData modelOffLineData = m_mapEntityCtrlCopy[enityId].Container.GetComponentInChildren<ModelOffLineData>();
                Transform UnitInfoPoint = modelOffLineData.GetTransformByKey(hangPoint.ToString());
                if (UnitInfoPoint != null)
                {
                    pos = UnitInfoPoint.position;
                }
                else
                {
                    // 如果找不到 目标挂点, 并且设置了 找不到 默认挂点 就找root 的话, 就用玩家的坐标
                    if (defaultRoot)
                    {
                        pos = m_mapEntityCtrlCopy[enityId].M_Curr.Position();
                    }
                }
                findResult = true;
            }

            return pos;
        }

        public Vector3 GetEntityPosById(ulong enityId)
        {
            Vector3 pos = Vector3.zero;
            if (m_mapEntityCtrlCopy.ContainsKey(enityId))
            {
                pos = m_mapEntityCtrlCopy[enityId].M_Curr.Position(); //不是数据，是逻辑层数据。
            }

            return pos;
        }

        public Vector3 GetEntityPosByCfgID(long cfgID)
        {
            Vector3 pos = Vector3.zero;
            foreach (var item in m_mapEntityCtrlCopy)
            {
                if (item.Value != null && item.Value.M_Curr != null && item.Value.M_Curr.ConfigIndex == cfgID)
                {
                    return item.Value.M_Curr.Position();
                }
            }
            return pos;
        }

        public ProtoMsg.Vector3 GetEntityProtoPosById(ulong enityId)
        {
            ProtoMsg.Vector3 pos = new();
            if (m_mapEntityCtrlCopy.ContainsKey(enityId))
            {
                pos = m_mapEntityCtrlCopy[enityId].M_Curr.PbPosition();
            }

            return pos;
        }

        public NPCEntityBase GetEntityByEntityID(ulong enityId)
        {
            if (m_mapEntityCtrlCopy.ContainsKey(enityId))
            {
                if (m_mapEntityCtrlCopy[enityId].M_Curr != null)
                {
                    return m_mapEntityCtrlCopy[enityId].M_Curr as NPCEntityBase;
                }
            }

            return null;
        }


        private HashSet<ulong> tempSet = new();
        public NPCEntityBase GetFinalSummonHostEntity(ulong summonHostID)
        {
            NPCEntityBase summonHost = null;
            NPCEntityBase cur_summonHost = null;

            ulong nextSummonHostID = 0;

            tempSet.Clear();

            do
            {
                tempSet.Add(summonHostID);

                cur_summonHost = GetEntityByEntityID(summonHostID);

                if (cur_summonHost == null)
                {
                    return summonHost;
                }

                summonHost = cur_summonHost;

                nextSummonHostID = cur_summonHost.SummonHostID;

                // 防止 相互为主人的情况下, 死循环
                if (summonHostID == nextSummonHostID)
                {
                    return summonHost;
                }

                // 防止 summonHost 嵌套.
                if (tempSet.Contains(nextSummonHostID))
                {
                    return summonHost;
                }

                // 继续往上一层主人上面找
                summonHostID = nextSummonHostID;

            } while (true);

            tempSet.Clear();
        }


        public string GetEntityNameById(ulong enityId)
        {
            string name = "";
            if (m_mapEntityCtrlCopy.ContainsKey(enityId))
            {
                EntityCtrlBase entityCtrlBase = m_mapEntityCtrlCopy[enityId];
                name = entityCtrlBase.M_Curr.M_Name;
            }

            return name;
        }

        public ModelDataCell GetEntityModelById(ulong enityId)
        {
            EntityCtrlBase entityBase = GetEntityCtr(enityId);
            if (entityBase != null)
            {
                AOIEntityObject aOIEntityObject = entityBase.M_Curr;
                return aOIEntityObject.modelDataCell;
            }

            return null;
        }

        public AvatarDataCell GetEntityAvatarById(ulong enityId)
        {
            EntityCtrlBase entityBase = GetEntityCtr(enityId);
            if (entityBase != null)
            {
                AOIEntityObject aOIEntityObject = entityBase.M_Curr;
                return aOIEntityObject.avatarDataCell;
            }

            return null;
        }

        public void SetNpcDialogLookAtMainPlayer(ulong enityId)
        {
            EntityCtrlBase entityBase = GetEntityCtr(enityId);
            if (entityBase == null || entityBase.M_Curr == null || entityBase.Data == null)
            {
                return;
            }
            if (entityBase.Data.EntityType != E_EntityType.Npc)
            {
                return;
            }

            GameNPCCtrlGroup ctrlGroup = entityBase as GameNPCCtrlGroup;
            if (ctrlGroup == null)
            {
                return;
            }
            ctrlGroup.LookAtMainPlayer();
        }

        #endregion

        #region 切换地图时，客户端设置的原子锁

        private List<E_BattleStateType> M_ChanageMapStates = new()
        {
            E_BattleStateType.BattleState_ForbidMove,
            E_BattleStateType.BattleState_ForbidDir,
            E_BattleStateType.BattleState_ForbidAttack,
            E_BattleStateType.BattleState_ForbidSkill
        };

        public void RegisterMainPlayerClientBattleStates()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return;
            }

            M_MainPlayerCtrlBase.Data.HandleClientBattleStates(M_ChanageMapStates, true);
            M_MainPlayerCtrlBase.BreakFindPath();
            SGF.Debuger.LogError($"{LOG_TAG} 禁止 主角移动 朝向攻击技能 11111111111");
        }

        public void UnRegisterMainPlayerClientBattleStates()
        {
            if (M_MainPlayerCtrlBase == null)
            {
                return;
            }

            M_MainPlayerCtrlBase.Data.HandleClientBattleStates(M_ChanageMapStates, false);
            SGF.Debuger.LogError($"{LOG_TAG} 解开 主角移动 朝向攻击技能 22222222222222222");
        }

        public void ClearClientBattleStates()
        {
            if (M_MainPlayerCtrlBase == null)
            {
                return;
            }

            M_MainPlayerCtrlBase.Data.ClearClientBattleStates(M_ChanageMapStates);
        }

        #endregion

        public void EnterFrame(int frameIndex)
        {
            {
                if (!m_isRunning)
                {
                    return;
                }

                if (frameIndex < 0)
                {
                    m_context.currentFrameIndex++;
                }
                else
                {
                    m_context.currentFrameIndex = frameIndex;
                }

                // ClientAddSpeedInvoke();

                // ClearFrameRPCDataCache();

                /*EntityFactory.ClearReleasedObjects();*/

                try
                {
                    //DynamicDataFactory.ClearReleasedObjects();
                    for (int i = 0; i < m_listEntityCtrl.Count; ++i)
                    {
                        m_listEntityCtrl[i].EnterFrame(frameIndex);
                    }

                    for (int j = 0; j < m_LocalEntitys.Count; ++j)
                    {
                        m_LocalEntitys[j].EnterFrame(frameIndex);
                    }

                    //List<ulong> listDiePlayerId = new List<ulong>();
                    //for (int i = 0; i < m_listPlayer.Count; i++)
                    //{
                    //    PlayerCtrlGroup player = m_listPlayer[i];

                    //    //碰撞吃鸡边缘导致死亡
                    //    //if (player.TryHitBound(m_context))
                    //    //{
                    //    //    listDiePlayerId.Add(player.Id);
                    //    //    ReleasePlayerAt(i);
                    //    //    i--;

                    //    //    continue;
                    //    //}
                    //    ////和其他玩家碰撞导致死亡
                    //    //if (player.TryHitEnemies(m_listPlayer))
                    //    //{
                    //    //    listDiePlayerId.Add(player.Id);
                    //    //    ReleasePlayerAt(i);
                    //    //    i--;
                    //    //    continue;
                    //    //}
                    //    //场景buff和碰撞触发器
                    //    //for (int j = 0; j < m_listFood.Count; j++)
                    //    //{
                    //    //    EntityObject food = m_listFood[j];
                    //    //    if (player.TryEatFood(food))
                    //    //    {
                    //    //        RemoveFoodAt(j);
                    //    //        j--;
                    //    //    }
                    //    //}
                    //}
                    if (m_map != null)
                    {
                        m_map.EnterFrame(frameIndex);
                    }

                    // 处理超时监听
                    HandleOverTimeMsgRet();
                }
                catch (System.Exception e)
                {
                    SGF.Debuger.LogError($"[GameManager]: enterFrame 报错了!!! e: {e.Message} //n {e.StackTrace}");
                }

                ////挂掉注册
                //if (onPlayerDie != null)
                //{
                //    //非空事件，全体剔除，（踢出）玩家本次生命。，
                //    for (int i = 0; i < listDiePlayerId.Count; ++i)
                //    {
                //        //判定，死亡通知，复活或者祭坛
                //        onPlayerDie(listDiePlayerId[i]);
                //    }
                //}

                /*SimpleDataFactory.ClearReleasedObjects();*/
            }
        }
        public void EnterFixLaterFrame()
        {
            //先逻辑，后数据，因为逻辑调数据
            EntityFactory.ClearReleasedObjects();
            SimpleDataFactory.ClearReleasedObjects();
        }

        /// <summary>
        /// 1，玩家能接受
        ////2，自己衡量度
        ////3，可能出现一帧多个移动，或者加载显示过慢
        ////4，不会过程帧还原，血量过程还原实际不准确，不是帧数同步的科学准确，原因来源于加载数据缓慢，
        ////5，服务器打包麻烦
        ////6，在乎还原即可，在乎顺序即可，不必在乎帧
        ////7，拆开包确实纯在
        ////uuu8，同一个数据可能存在拆包的可能未必一帧-----合并过====
        ////uuu9，不同的属于也是一帧到来
        ////10，融合性的数据统一处理客户端mmo此时不必加帧同步那么准确（暂时服务器和我们的策略），之后可以加帧概念
        ////11，融合数据快速处理之前拥堵的帧，和加速处理之前的帧
        ////12，之前加缓存队列的原因是客户端先有表现层次再有逻辑处理这个没错
        ////13，锁定是也是没有表现先做逻辑的原因
        ////1，合并了分离的数据（aoi）；2，一帧处理多个数据
        ////2，因为分包的概念合并过，我们加速处理的原因来源于，1网络拥堵，2客户端加载资源，3客户端卡
        ////3，服务器设计理念是保证顺序，不保证帧，客户端就管还原就行了；服务器也保证了Enter和leave和update的顺序以处理顺序问题
        ////1，顺序有顺序问题，同步概念来符合这个东西，好处和方便
        /// </summary>
        private void ClientAddSpeedInvoke()
        {
            //1，空了逻辑不会有问题；2，处理多了表现也没问题;3,取多了不会有问题没有一个固定缓存
            // 每帧取【除主角】属性同步的数据进行分发处理
            // InvokeFrameTHDDataCache();
            // 每帧取【主角】属性同步的数据进行分发处理
            // InvokeFrameMainPlayerDataCache();
            // 每帧取【RPC】同步的数据进行分发处理
            InvokeFrameRPCDataCache();
        }

        // 删除所有实体
        public void ReleaseAllEntity()
        {
            // 删除主角
            if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
            {
                (M_MainPlayerCtrlBase.M_Curr as NPCEntityBase).ActionOnCheckTargetMove -= OnActionOnCheckTargetMove;
            }

            // 删除所有实体缓存
            for (int i = 0; i < m_listEntityCtrl.Count; ++i)
            {
                if (m_listEntityCtrl[i] != null)
                {
                    m_listEntityCtrl[i].Release();
                }
            }

            m_listEntityCtrl.Clear();
            m_mapEntityCtrlCopy.Clear();
            m_mapEntityData.Clear();

            M_MainPlayerCtrlBase = null;
        }

        // 主角的伙伴和召唤物，目前切地图不删除实体
        public void ReleaseOtherEntity()
        {
            for (int i = m_listEntityCtrl.Count - 1; i >= 0; i--)
            {
                // 不删除主角
                if (m_listEntityCtrl[i].M_Curr != null && m_listEntityCtrl[i].M_Curr.EntityId != mainPlayerId)
                {
                    ulong entityId = m_listEntityCtrl[i].M_Curr.EntityId;
                    switch (m_listEntityCtrl[i].M_Curr.EntityType)
                    {
                        case E_EntityType.None:
                            break;
                        case E_EntityType.RoomSpace:
                            break;
                        case E_EntityType.Player:
                            break;
                        case E_EntityType.Npc:
                            break;
                        case E_EntityType.Monster:
                        case E_EntityType.GVEBoss:
                        case E_EntityType.Robot:
                            {
                                // 怪物删除的时候，通知小地图
                                RemoveMonster(m_listEntityCtrl[i].Data);
                            }
                            break;
                        case E_EntityType.BulletEntity:
                            break;
                        case E_EntityType.Interact:
                            break;
                        case E_EntityType.Partner:
                        case E_EntityType.Summon:
                            {
                                // TODO:曲 服务器逻辑是切地图除主角外都删
                                //// 主角的伙伴和召唤物，目前切地图不删除实体
                                //if (m_listEntityCtrl[i].M_Curr.M_IsMainPlayerSummon)
                                //{
                                //    continue;
                                //}
                            }
                            break;
                        default:
                            break;
                    }

                    m_mapEntityCtrlCopy.Remove(entityId);
                    m_mapEntityData.Remove(entityId);
                    m_listEntityCtrl[i].Release();
                    m_listEntityCtrl.RemoveAt(i);
                }
            }
        }

        #region AOI实体【增删改查】逻辑

        // 小地图使用 怪物改变委托
        [XLua.BlackList]
        public List<ulong> MonsterList = new();

        public Action<ulong, bool> OnMonsterDataChange;
        public Action<ulong, bool, Vector3> OnSpaceArenaDataChange; // 异步竞技场的增删

        public Action<ulong> OnPvpAddPlayerAction;

        public Action<ulong> OnPvpRemovePlayerAction;
        // 小地图使用 怪物改变委托

        /// <summary>
        /// TODO：小虫说，播放剧情有隐藏AOI的设定
        /// 首次通知会全部数据，变更给的是差量数据
        /// 给任意一个数据都需要注册他的Attr
        /// 服务器也有两层AOI，视野注册层，和显示层
        /// 【这里就是客户端的创建，或，更新逻辑】
        /// </summary>
        public VitalSignData EntityDataCommand(GameCommand gameCommand)
        {
            switch (gameCommand.command)
            {
                case E_Command.Create:
                    {
                        TriggerEvent(TriggerEventType.AOIEnter, gameCommand.entityID);

                        //SGF.Debuger.Log($"{LOG_TAG} EntityDataCommand Create {gameCommand.entityBaseData.M_EntityID}");
                        CreateEntity(gameCommand);
                    }
                    break;
                case E_Command.Destroy:
                    {
                        TriggerEvent(TriggerEventType.AOILeave, gameCommand.entityID);
                        //SGF.Debuger.Log($"{LOG_TAG} EntityDataCommand Destroy {gameCommand.entityID}");
                        ReleaseEntity(gameCommand.entityID, gameCommand.isServerAOI);
                    }
                    break;
                case E_Command.Update:
                    {
                        // SGF.Debuger.Log($"{LOG_TAG} EntityDataCommand Update {gameCommand.entityID}");
                        EntityCtrlBase AOIEntity = GetEntityCtr(gameCommand.entityID);
                        if (AOIEntity != null)
                        {
                            return AOIEntity.Data;
                        }
                    }
                    break;
                case E_Command.Reg:
                    {
                        //SGF.Debuger.Log($"{LOG_TAG} EntityDataCommand Reg {gameCommand.entityBaseData.M_EntityID}");
                        //数据先缓存起来。
                        if (!m_mapEntityData.ContainsKey(gameCommand.entityBaseData.M_EntityID))
                        {
                            m_mapEntityData[gameCommand.entityBaseData.M_EntityID] = gameCommand.entityBaseData;
                        }
                    }
                    break;
                default:
                    break;
            }

            return null;
        }

        #region 增

        private void CreateEntity(GameCommand gameCommand)
        {
            //SGF.Debuger.Log($"{LOG_TAG}  CreateEntity {data.M_EntityID} , entityType {data.EntityType} isServer {isServer} time {TimeUtils.ServerNowStampMilli}");

            switch (gameCommand.entityBaseData.EntityType)
            {
                case E_EntityType.Player:
                    {
                        CreatePlayer(gameCommand);
                    }
                    break;
                case E_EntityType.Npc:
                    {
                        CreateNpc(gameCommand);
                    }
                    break;
                case E_EntityType.Monster:
                case E_EntityType.GVEBoss:
                case E_EntityType.Robot:
                    {
                        CreateMonster(gameCommand);
                    }
                    break;
                case E_EntityType.BulletEntity:
                    {
                        // CreateBullet(data, pos, isServer);
                        CreateBulletSummon(gameCommand);
                    }
                    break;
                case E_EntityType.Interact:
                    {
                        CreateInterActionObject(gameCommand);
                    }
                    break;
                case E_EntityType.Summon:
                    {
                        CreateSummon(gameCommand);
                    }
                    break;
                case E_EntityType.Partner:
                    {
                        CreatePartner(gameCommand);
                    }
                    break;
                case E_EntityType.ClientSummon:
                    {
                        // 创建 客户端的召唤物
                        CreateSummon(gameCommand);
                    }
                    break;
                default: break;
            }
            //通过缓存创建。
            if (!m_mapEntityData.ContainsKey(gameCommand.entityID))
            {
                //SGF.Debuger.LogWarning($"{LOG_TAG}  CreateEntity {data.M_EntityID} , entityType {data.EntityType} isServer {isServer} m_mapEntityData 找不到");

                m_mapEntityData[gameCommand.entityID] = gameCommand.entityBaseData;
            }
        }

        internal PlayerCtrlGroup CreatePlayer(GameCommand gameCommand)
        {
            VitalSignData data = (VitalSignData)gameCommand.entityBaseData;
            PlayerCtrlGroup player = new();
            player.Create(data, gameCommand.entityBaseData.Pos);
            AddEntityCtr(player, data.M_EntityID);
            //人身跟随ntt（注意这里需要判断是实体例如精灵）

            //AddFxNttToLifeNtt(playerId,null);

            OnPvpAddPlayerAction?.Invoke(data.M_EntityID);

            if (GetCurMapType() == SpaceType.SpaceArena)
            {
                OnSpaceArenaDataChange?.Invoke(data.M_EntityID, true, gameCommand.entityBaseData.Pos);
            }

            return player;
        }

        internal MonsterCtrlGroup CreateMonster(GameCommand gameCommand)
        {
            VitalSignData data = (VitalSignData)gameCommand.entityBaseData;
            MonsterCtrlGroup monster = new();
            monster.Create(data, gameCommand.entityBaseData.Pos);
            AddEntityCtr(monster, data.M_EntityID);
            AddMonster(data);
            //人身跟随ntt（注意这里需要判断是实体例如精灵）
            //AddFxNttToLifeNtt(playerId,null);
            return monster;
        }

        // 小地图通知
        private void AddMonster(VitalSignData data)
        {
            if (!MonsterList.Contains(data.M_EntityID))
            {
                MonsterList.Add(data.M_EntityID);
            }

            OnMonsterDataChange?.Invoke(data.M_EntityID, true);

            OnPvpAddPlayerAction?.Invoke(data.M_EntityID);

            if (data.IsRobot && GetCurMapType() == SpaceType.SpaceArena)
            {
                OnSpaceArenaDataChange?.Invoke(data.M_EntityID, true, data.Pos);
            }
        }

        internal PartnerCtrlGroup CreatePartner(GameCommand gameCommand)
        {
            VitalSignData data = (VitalSignData)gameCommand.entityBaseData;
            //SGF.Debuger.LogError($"属性同步创建 CreatePartner--开始-- 时间={SGF.Time.TimeUtils.TimeLogString()}");

            PartnerCtrlGroup partner = new();
            partner.Create(data, gameCommand.entityBaseData.Pos);
            AddEntityCtr(partner, data.M_EntityID);
            //SGF.Debuger.LogError($"属性同步创建 CreatePartner--结束-- 时间={SGF.Time.TimeUtils.TimeLogString()}");

            //AddMonster(data);

            return partner;
        }

        internal ObjectCtrlGroup CreateInterActionObject(GameCommand gameCommand)
        {
            NoneVitalSignData data = (NoneVitalSignData)gameCommand.entityBaseData;
            ObjectCtrlGroup obj = new();
            obj.Create(data, gameCommand.entityBaseData.Pos);
            AddEntityCtr(obj, data.M_EntityID);
            return obj;
        }

        internal SummonCtrlGroup CreateSummon(GameCommand gameCommand)
        {
            VitalSignData data = (VitalSignData)gameCommand.entityBaseData;
            SummonCtrlGroup summon = new();
            summon.Create(data, gameCommand.entityBaseData.Pos);
            AddEntityCtr(summon, data.M_EntityID);
            return summon;
        }

        /// <summary>
        /// 创建子弹召唤物,目前 策划 们说的 子弹,其实是我们的召唤物,不是我们程序定义的非生命体的纯子弹
        /// </summary>
        /// <param name="entityBaseData"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        internal SummonCtrlGroup CreateBulletSummon(GameCommand gameCommand)
        {
            //SGF.Debuger.LogError($"[Bullet] 子弹 创建: {entityBaseData.M_EntityID}");
            return CreateSummon(gameCommand);
        }

        internal GameNPCCtrlGroup CreateNpc(GameCommand gameCommand)
        {
            VitalSignData data = (VitalSignData)gameCommand.entityBaseData;
            GameNPCCtrlGroup nPCCtrlGroup = new();
            nPCCtrlGroup.Create(data, gameCommand.entityBaseData.Pos);
            AddEntityCtr(nPCCtrlGroup, data.M_EntityID);
            return nPCCtrlGroup;
        }

        private void AddEntityCtr(EntityCtrlBase entityCtrlBase, ulong entityID)
        {
            int index = GetEntityIndex(entityID);
            if (index == -1)
            {
                m_listEntityCtrl.Add(entityCtrlBase);
            }
            else
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} AddEntityCtr add err EnityId={entityID}");
            }

            m_mapEntityCtrlCopy[entityID] = entityCtrlBase;
            //SGF.Debuger.Log($"{LOG_TAG} AOI AddEntityCtr id = {entityID} ");
        }

        #endregion

        #region 删

        private void ReleaseEntity(ulong entityID, bool isServer = true)
        {
            // 如果离开的实体是目标实体的话，清空
            if (BattleManager.Instance.CurAtkEntity != null && BattleManager.Instance.CurAtkEntity.EntityId == entityID)
            {
                // BattleManager.Instance.SetCurAtkEntity(null);
                // 怪物死亡的时候, 切换 当前的 索敌队列
                BattleManager.Instance.ClearSerarchTargets();
            }

            if (BattleManager.Instance.ShowBossPanelEntity != null &&
                BattleManager.Instance.ShowBossPanelEntity.EntityId == entityID)
            {
                BattleManager.Instance.SetShowBossPanelEntity(null);
            }

            // 如果实体离开，先判断下是不是死亡了
            // 如果是死亡，就播放完死亡动作再销毁
            EntityCtrlBase entityCtr = GetEntityCtr(entityID);
            if (isServer)
            {
                if (entityCtr != null)
                {
                    if (entityCtr.entityBaseData == null)
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} AOI isServer {isServer} ReleaseEntity id = {entityID} entityBaseData = {null} error!!!!!");
                    }
                    else
                    {
                        // 怪物删除的时候，通知小地图
                        if (entityCtr.entityBaseData.EntityType == E_EntityType.Monster || entityCtr.entityBaseData.EntityType == E_EntityType.GVEBoss ||
                            entityCtr.entityBaseData.EntityType == E_EntityType.Robot)
                        {
                            RemoveMonster((VitalSignData)entityCtr.entityBaseData);
                            //SGF.Debuger.LogError($"死亡测试 {LOG_TAG}  AOI删除 --------不删除--{entityID}");
                        }
                        else if (entityCtr.entityBaseData.EntityType == E_EntityType.Monster)
                        {
                            OnPvpRemovePlayerAction?.Invoke(entityID);
                        }
                        else if (entityCtr.entityBaseData.EntityType == E_EntityType.Player && GetCurMapType() == SpaceType.SpaceArena)
                        {
                            OnSpaceArenaDataChange?.Invoke(entityID, false, Vector3.zero);
                        }

                        bool isDead = entityCtr.entityBaseData.IsDead;
                        // 如果{实体}【类型不是人】【 状态是死亡】那就等死亡动作播放完毕在销毁实体
                        // 如果{实体}【类型是人】【 状态是死亡】实体不销毁
                        if (isDead && entityCtr.entityBaseData.EntityType != E_EntityType.Player)
                        {
                            //SGF.Debuger.LogError($"{LOG_TAG} AOI isServer {isServer} ReleaseEntity id = {entityID} isDead = {isDead} ");
                            return;
                        }
                    }
                }
            }
            //else
            //{
            //    if (entityCtr.M_Curr.EntityType == E_EntityType.Partner)
            //    {
            //     //   SGF.Debuger.LogError($"死亡测试 {LOG_TAG} AOI删除 ----------删除--{entityID}");
            //    }
            // //   SGF.Debuger.LogError($"{LOG_TAG} AOI isServer {isServer} ReleaseEntity id = {entityID}");
            //}

            int index = GetEntityIndex(entityID);
            if (-1 != index)
            {
                m_mapEntityCtrlCopy.Remove(entityID);
                m_mapEntityData.Remove(entityID);
                ReleaseEntityAt(index);
            }

            //SGF.Debuger.Log($"{LOG_TAG} AOI isServer {isServer} ReleaseEntity id = {entityID} ");
        }

        private void RemoveMonster(VitalSignData data)
        {
            if (MonsterList.Contains(data.M_EntityID))
            {
                MonsterList.Remove(data.M_EntityID);
            }

            OnMonsterDataChange?.Invoke(data.M_EntityID, false);

            OnPvpRemovePlayerAction?.Invoke(data.M_EntityID);

            if (data.IsRobot && GetCurMapType() == SpaceType.SpaceArena)
            {
                OnSpaceArenaDataChange?.Invoke(data.M_EntityID, false, data.Pos);
            }
        }

        private void ReleaseEntityAt(int index)
        {
            if (index >= 0)
            {
                EntityCtrlBase entityCtrlBase = m_listEntityCtrl[index];
                m_listEntityCtrl.RemoveAt(index);

                //if (entityCtrlBase.entityBaseData.EntityType == E_EntityType.Partner.ToString())
                //{
                // //   SGF.Debuger.LogError($"{LOG_TAG} ReleaseEntity Bullet1111 id = {entityCtrlBase.entityBaseData.M_EntityID} ");
                //}

                entityCtrlBase.Release();
            }
        }

        #endregion

        #region 查

        /// <summary>
        ///  当3位1体的时候，就是在玩家控制的多个NttId中寻找
        ///  先寻找nttid组，然后找玩家控组，找到玩家
        ///  每个ntt 有个玩家id就行了，找到ntt就找到玩家了
        /// </summary>
        /// <param name="enityId"></param>
        /// <returns></returns>
        public EntityCtrlBase GetEntityCtr(ulong enityId)
        {
            EntityCtrlBase entityCtrlBase;
            if (m_mapEntityCtrlCopy.TryGetValue(enityId, out entityCtrlBase))
            {
                return entityCtrlBase;
            }

            return null;
        }

        public ulong GetInterEntityID(int index)
        {
            return (ulong)((1000000 * (int)ProtoMsg.ServiceObjEnum.ObjInteract) + index);
        }

        /// <summary>
        /// 更具配置ConfigID  获取实体ID
        /// </summary>
        /// <param name="configID"></param>
        /// <returns></returns>
        public ulong GetNPCEntityIDByConfig(long configID)
        {
            foreach (var item in m_mapEntityData)
            {
                if (item.Value.EntityType == E_EntityType.Npc)
                {
                    uint ConfigIndex = item.Value.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
                    if (ConfigIndex == configID)
                    {
                        return item.Key;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 通过配置获取 GameNPCCtrlGroup
        /// </summary>
        /// <param name="configID"></param>
        /// <returns></returns>
        public GameNPCCtrlGroup GetNPCCtrlGroupByConfig(long configID)
        {
            foreach (var item in m_mapEntityCtrlCopy)
            {
                if (item.Value.EntityType == E_EntityType.Npc)
                {
                    uint ConfigIndex = item.Value.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
                    if (ConfigIndex == configID)
                    {
                        return item.Value as GameNPCCtrlGroup;
                    }
                }
            }

            return null;
        }

        public ulong GetObjectEntityIDByConfig(int configID)
        {
            foreach (var item in m_mapEntityData)
            {
                if (item.Value.EntityType == E_EntityType.Interact)
                {
                    uint ConfigIndex = item.Value.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
                    if (ConfigIndex == configID)
                    {
                        return item.Key;
                    }
                }
            }

            return 0;
        }

        private int GetEntityIndex(ulong entityID)
        {
            for (int i = 0; i < m_listEntityCtrl.Count; i++)
            {
                if (m_listEntityCtrl[i].M_Curr != null && m_listEntityCtrl[i].M_Curr.EntityId == entityID)
                {
                    return i;
                }
            }

            return -1;
        }

        internal List<EntityCtrlBase> GetEntityList()
        {
            return m_listEntityCtrl;
        }

        /// <summary>
        /// 通过 伙伴的配置表 index（伙伴的index 唯一） 获取对应的 EntityCtrl
        /// </summary>
        /// <param name="index"></param>
        public EntityCtrlBase GetMainPlayerParterByIndex(int index)
        {
            for (int i = 0; i < m_listEntityCtrl.Count; i++)
            {
                EntityCtrlBase entityCtr = m_listEntityCtrl[i];
                if (entityCtr == null)
                {
                    continue;
                }

                if (entityCtr.M_Curr == null)
                {
                    continue;
                }

                if (entityCtr.EntityType != E_EntityType.Partner)
                {
                    continue;
                }

                if (entityCtr.M_Curr.ConfigIndex == index)
                {
                    return entityCtr;
                }
            }

            return null;
        }

        /// <summary>
        /// 得到其它阵营的 实体,包括中立阵营
        /// </summary>
        public void GetOterFactionEntitys(EntityCtrlBase player, List<EntityCtrlBase> entityLists)
        {
            int faction = player.M_Curr.Faction;

            // 中立阵营
            List<int> NeutralFaction = player.M_Curr.NeutralFaction;
            // 敌对阵营
            List<int> OpposingFaction = player.M_Curr.OpposingFaction;

            m_listEntityCtrl.ForEach((EntityCtrlBase entity) =>
            {
                if (NeutralFaction.Contains(entity.M_Curr.Faction) || OpposingFaction.Contains(entity.M_Curr.Faction))
                {
                    entityLists.Add(entity);
                    return;
                }
            });
        }

        public int GetMainPlayerFaction()
        {
            return GameManager.Instance.M_MainPlayerCtrlBase.Data.myOwnerNtt.Faction;
        }

        /// <summary>
        /// 获取伙伴的技能CD
        /// </summary>
        /// <param name="skillId">伙伴技能id</param>
        /// <returns></returns>
        public void GetPartnerSkillCDBySkillID(int skillId, out float curCD, out float sumCD)
        {
            curCD = 0;
            sumCD = 0;
            if (M_MainPlayerCtrlBase != null)
            {
                NPCEntityBase nPCEntityBase = M_MainPlayerCtrlBase.M_Curr as NPCEntityBase;
                Skill.SkillContainer mainPlayerSkillContainer =
                    nPCEntityBase.skillDispatcher.SkillUnitController.GetSkillUnit(skillId);
                if (mainPlayerSkillContainer != null)
                {
                    curCD = mainPlayerSkillContainer.LeastCD;
                    if (mainPlayerSkillContainer.CurShowSkillInfo != null)
                    {
                        sumCD = (float)mainPlayerSkillContainer.CurShowSkillInfo.GetRellyCD();
                    }
                }
            }
        }

        #endregion

        #endregion

        #region 本地实体

        private void OnCreateLocalEntity(object[] arg0)
        {
            E_LocalEntityType type = (E_LocalEntityType)arg0[0];
            int id = (int)arg0[1];
            string triggerKey = (string)arg0[2];
            Vector3 pos = (Vector3)arg0[3];
            LocalDropList rewardList = arg0[4] as LocalDropList;
            bool isAllOpen = (bool)arg0[5];
            float scale = (float)arg0[6];

            string localEntityKey = Fire.Utils.GenerateCheckCode(16);
            switch (type)
            {
                case E_LocalEntityType.None:
                    break;
                case E_LocalEntityType.TreasureBox:
                    {
                        TreasureBoxEntity treasureBoxEntity = new();
                        treasureBoxEntity.Create(localEntityKey, type, id, triggerKey, pos, rewardList, isAllOpen, scale);
                        AddLocalEntity(localEntityKey, treasureBoxEntity);
                    }
                    break;
                case E_LocalEntityType.Gateway:
                    {
                        GatewayEntity gatewayEntity = new();
                        gatewayEntity.Create(localEntityKey, type, id, triggerKey, pos, rewardList, isAllOpen, scale);
                        AddLocalEntity(localEntityKey, gatewayEntity);
                    }
                    break;
                default:
                    break;
            }
            // 实体类型/点位id/点阶位类型
        }

        private void OnCreateLocalEntity(E_LocalEntityType localEntityType, int pointType, ulong srvID, ulong spaceID, WTaskPointTarInfo wTaskPointTarInfo)
        {
            if (GameMap.SceneReady && GameMap.DataReady)
            {
                string localEntityKey = Fire.Utils.GenerateCheckCode(16);
                switch (localEntityType)
                {
                    case E_LocalEntityType.None:
                    case E_LocalEntityType.TreasureBox:
                    case E_LocalEntityType.Gateway:
                        break;
                    case E_LocalEntityType.WantedEnity:
                        {
                            //SGF.Debuger.LogWarning($"通缉任务 单个 创建 pointType={pointType},srvID={srvID},spaceID={spaceID},PointID={wTaskPointTarInfo.PointID}");
                            WantedEntity wantedNpcEntity = new();
                            wantedNpcEntity.Create(localEntityKey, localEntityType, pointType, srvID, spaceID, wTaskPointTarInfo);
                            AddLocalEntity(localEntityKey, wantedNpcEntity);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private ulong m_lastWantedSpaceID = 0;
        private ulong m_lastWantedServerID = 0;

        /// <summary>
        /// 切地图完后创建本地的通缉实体
        /// </summary>
        public void InitCreateWantedEntity()
        {
            ulong SpaceID = GetSpaceID();   // 当前服务器地图唯一ID
            ulong ServerID = GetCurServerID(); /// 分线ID
            if (m_lastWantedSpaceID == SpaceID && m_lastWantedServerID == ServerID)
            {
                SGF.Debuger.LogWarning($"通缉 实体创建 不创建了，跟上一个地图一模一样");
                return;
            }

            // 先清空所有的通缉实体
            RemoveAllWantedEntity();
            // 创建本分线和本地图的通缉实体
            Dictionary<int, Dictionary<int, WTaskPointTarInfo>> curServerMapAllWantedData = GetCurMapWantedData();
            if (curServerMapAllWantedData == null)
            {
                return;
            }

            foreach (var item in curServerMapAllWantedData)
            {
                int pointType = item.Key;
                foreach (var item1 in item.Value)
                {
                    WTaskPointTarInfo data = item1.Value;
                    OnCreateLocalEntity(E_LocalEntityType.WantedEnity, pointType, ServerID, SpaceID, data);
                }
            }
            m_lastWantedSpaceID = SpaceID;
            m_lastWantedServerID = ServerID;
        }

        /// <summary>
        /// 加载配置表资源
        /// </summary>
        public void InitCreateSceneNPCEntity()
        {
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Npcs != null && GameMap.sceneJsonData.Npcs.Count > 0)
            {
                foreach (var areaJson in GameMap.sceneJsonData.Npcs)
                {
                    if (areaJson.Value.WaveID != 0)
                    {
                        continue;
                    }
                    ProtoMsg.PropSyncList psl = new();
                    psl.EntityType = E_EntityType.Npc.ToString();
                    psl.Prop = new PropBaseSyncList();
                    //配置表id属性id
                    var syncBaseInfo = new SyncBaseInfo();
                    syncBaseInfo.Uint32Value = (uint)areaJson.Value.NpcID;
                    syncBaseInfo.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Index);
                    psl.Prop.Prop.Add(syncBaseInfo);
                    //坐标
                    var p = new ProtoMsg.Vector3();
                    p.X = areaJson.Value.Position.x;//读取配置表
                    p.Y = areaJson.Value.Position.y;
                    p.Z = areaJson.Value.Position.z;
                    var pos = new SyncBaseInfo();
                    pos.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                    pos.MsgValue = ByteString.CopyFrom(p.ToByteArray());
                    psl.Prop.Prop.Add(pos);
                    //朝向
                    var dir = new SyncBaseInfo();
                    UnityEngine.Vector3 dir2 = Fire.Utils.ServerRota2Vector(areaJson.Value.Rotation);
                    float clientRot = (float)(Math.Atan2(dir2.x, dir2.z) * Mathf.Rad2Deg) % 360;
                    //float ServerRot = (int)(Math.Atan2(dir2.z, dir2.x) * Mathf.Rad2Deg % 360);
                    dir.Int32Value = (int)clientRot;//配置表朝向
                    //dir.Int32Value = areaJson.Value.Rotation;//配置表朝向
                    dir.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                    psl.Prop.Prop.Add(dir);
                    // SpaceIndex
                    var SpaceIndex = new SyncBaseInfo();
                    SpaceIndex.Int32Value = areaJson.Value.Index;
                    SpaceIndex.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.SpaceIndex);
                    psl.Prop.Prop.Add(SpaceIndex);
                    //// 阵营
                    //var faction = new SyncBaseInfo();
                    //faction.Uint32Value = 0;
                    //faction.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Faction);
                    //psl.Prop.Prop.Add(faction);
                    // 实体ID
                    psl.EntityID = (ulong)(1000000 + areaJson.Value.Index);
                    CreateEntity(psl, false);
                }
            }
        }

        /// <summary>
        /// 加载交互物资源
        /// </summary>
        public void InitCreateSceneObjectEntity()
        {
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Mines != null && GameMap.sceneJsonData.Mines.Count > 0)
            {
                foreach (var areaJson in GameMap.sceneJsonData.Mines)
                {
                    if (areaJson.Value.WaveID != 0)
                    {
                        continue;
                    }
                    ProtoMsg.PropSyncList psl = new();
                    psl.EntityType = E_EntityType.Interact.ToString();
                    psl.Prop = new PropBaseSyncList();
                    //配置表id属性id
                    var syncBaseInfo = new SyncBaseInfo();
                    syncBaseInfo.Uint32Value = (uint)areaJson.Value.MineID;
                    syncBaseInfo.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Index);
                    psl.Prop.Prop.Add(syncBaseInfo);
                    //坐标
                    var p = new ProtoMsg.Vector3();
                    p.X = areaJson.Value.Position.x;//读取配置表
                    p.Y = areaJson.Value.Position.y;
                    p.Z = areaJson.Value.Position.z;
                    var pos = new SyncBaseInfo();
                    pos.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                    pos.MsgValue = ByteString.CopyFrom(p.ToByteArray());
                    psl.Prop.Prop.Add(pos);
                    //朝向
                    var dir = new SyncBaseInfo();
                    UnityEngine.Vector3 dir2 = Fire.Utils.ServerRota2Vector(areaJson.Value.Rotation);
                    float clientRot = (float)(Math.Atan2(dir2.x, dir2.z) * Mathf.Rad2Deg) % 360;
                    //float ServerRot = (int)(Math.Atan2(dir2.z, dir2.x) * Mathf.Rad2Deg % 360);
                    dir.Int32Value = (int)clientRot;//配置表朝向
                    //dir.Int32Value = areaJson.Value.Rotation;//配置表朝向
                    dir.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                    psl.Prop.Prop.Add(dir);
                    var SpaceIndex = new SyncBaseInfo();
                    SpaceIndex.Int32Value = areaJson.Value.Index;
                    SpaceIndex.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.SpaceIndex);
                    psl.Prop.Prop.Add(SpaceIndex);
                    //// 阵营
                    //var faction = new SyncBaseInfo();
                    //faction.Uint32Value = 0;
                    //faction.Index = (uint)LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Faction);
                    //psl.Prop.Prop.Add(faction);
                    psl.EntityID = (ulong)((1000000 * 2) + areaJson.Value.Index);
                    CreateEntity(psl, false);
                }
            }
        }

        private WantedEntity GetWantedEnityByPointID(ulong srvID, ulong spaceID, int pointID, int pointType)
        {
            for (int i = 0; i < m_LocalEntitys.Count; i++)
            {
                if (m_LocalEntitys[i] != null && m_LocalEntitys[i].Type == E_LocalEntityType.WantedEnity)
                {
                    WantedEntity item = (WantedEntity)m_LocalEntitys[i];
                    if (item != null)
                    {
                        if (item.PointType == pointType && item.PointID == pointID)
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        private void AddLocalEntity(string entityKey, EntityLocalDynamic localDynamic)
        {
            if (!m_LocalEntityDic.ContainsKey(entityKey))
            {
                m_LocalEntityDic.Add(entityKey, localDynamic);
                m_LocalEntitys.Add(localDynamic);
            }
            else
            {
                SGF.Debuger.LogWarning($"本地实体 key已有 key={entityKey} err!!!");
            }
        }

        public EntityLocalDynamic GetLocakEntity(string entityKey)
        {
            EntityLocalDynamic entityLocalDynamic;
            if (m_LocalEntityDic.TryGetValue(entityKey, out entityLocalDynamic))
            {
                return entityLocalDynamic;
            }

            return null;
        }

        private int GetLocalEntityIndex(string entityKey)
        {
            for (int i = 0; i < m_LocalEntitys.Count; i++)
            {
                if (m_LocalEntitys[i] != null && m_LocalEntitys[i].EntityKey == entityKey)
                {
                    return i;
                }
            }

            return -1;
        }

        public void RemoveLocalEntity(string entityKey)
        {
            int index = GetLocalEntityIndex(entityKey);
            if (index != -1)
            {
                m_LocalEntitys.RemoveAt(index);
            }

            EntityLocalDynamic entityLocalDynamic = GetLocakEntity(entityKey);
            if (entityLocalDynamic != null)
            {
                m_LocalEntityDic.Remove(entityKey);
                EntityFactory.ReleaseEntity(entityLocalDynamic);
            }
        }

        private void RemoveAllWantedEntity()
        {
            for (int i = m_LocalEntitys.Count - 1; i >= 0; i--)
            {
                if (m_LocalEntitys[i] != null && m_LocalEntitys[i].Type == E_LocalEntityType.WantedEnity)
                {
                    EntityLocalDynamic item = m_LocalEntitys[i];
                    EntityFactory.ReleaseEntity(item);
                }
            }
        }

        public void ReleaseAllLocalEntity()
        {
            foreach (var item in m_LocalEntitys)
            {
                EntityFactory.ReleaseEntity(item);
            }

            m_LocalEntitys.Clear();
            m_LocalEntityDic.Clear();

            m_lastWantedSpaceID = 0;
            m_lastWantedServerID = 0;
        }

        /// <summary>
        /// 是否超过了地图格子范围
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsExceedMapGridSize(Vector3 pos)
        {
            if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
            {
                var dis = Vector3.Distance(pos, M_MainPlayerCtrlBase.M_Curr.Position());
                return dis > GameMap.SceneJsonGridSize;
            }
            return false;
        }

        private ulong nextID = 300000;

        public ulong GetNextLocalID()
        {
            return nextID++;
        }

        /// <summary>
        /// 创建本地实体的接口
        /// </summary>
        /// <param name="localEntityType"></param>
        /// <param name="args">创建特定类型实体需要的 [剩余参数] , 每种类型实体的 参数个数应该提前约定好</param>
        public ulong CreateLocalEntity(E_LocalEntityType localEntityType, object[] args)
        {
            ulong entityID = GetNextLocalID();

            switch (localEntityType)
            {
                case E_LocalEntityType.None:
                case E_LocalEntityType.TreasureBox:
                case E_LocalEntityType.Gateway:
                case E_LocalEntityType.WantedEnity:
                    break;
                case E_LocalEntityType.Summon:
                    {
                        // 创建本地召唤物
                        LocalSummonEntity localSummonEntity = EntityFactory.InstanceEntity<LocalSummonEntity>();

                        {
                            int avatarID = (int)args[0];
                            Vector3 pos = (Vector3)args[1];
                            Transform followNode = (Transform)args[2];
                            bool isParent = (bool)args[3];
                            ulong summonHostID = (ulong)args[4];

                            localSummonEntity.Create(entityId: entityID, avatarID: avatarID, pos: pos, follow: followNode, isParent: isParent, summonHostID);

                            AddLocalEntity(entityID, localSummonEntity);
                        }
                    }
                    break;
                default:
                    break;
            }

            return entityID;
        }

        private void AddLocalEntity(ulong entityID, EntityLocalStatic localStatic)
        {
            // 1.本地创建的实体 entityID 由客户端统一生成, 所以理论上不会产生相同的 entityID
            // 2.本地同时存在的实体  不会很多，  没必要使用 一份copy 实体list
            m_LocalStaticEntityDic.Add(entityID, localStatic);
        }

        public EntityLocalStatic GetLocalEntity(ulong entityID)
        {
            EntityLocalStatic entityLocalStatic;
            if (m_LocalStaticEntityDic.TryGetValue(entityID, out entityLocalStatic))
            {
                return entityLocalStatic;
            }

            return null;
        }

        public void RemoveLocalEntity(ulong entityID)
        {
            if (!m_LocalStaticEntityDic.ContainsKey(entityID))
            {
                return;
            }

            var entity = m_LocalStaticEntityDic[entityID];

            m_LocalStaticEntityDic.Remove(entityID);

            EntityFactory.ReleaseEntity(entity);
        }

        public void RemoveAllLocalEntity()
        {
            foreach (var item in m_LocalStaticEntityDic)
            {
                var entity = item.Value;

                EntityFactory.ReleaseEntity(entity);
            }

            m_LocalStaticEntityDic.Clear();
        }

        /// <summary>
        /// 记录一个 本地实体的 标记key 对应的本地实体 id
        /// note:
        ///     1.实体id 是本地自动生成的;
        ///     2.标记key 根据对应的表现标签来生成. 如 模拟子弹的表现标签, key = runtimeID_子弹位置index 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entityID"></param> <summary>
        public void RecordFormateKey2EntityID(string key, ulong entityID)
        {
            m_key2EntityIDDic[key] = entityID;
        }

        public ulong GetFormateKey2EntityID(string key)
        {
            if (m_key2EntityIDDic.ContainsKey(key))
            {
                return m_key2EntityIDDic[key];
            }
            return 0;
        }

        public void RemoveFormateKey2EntityID(string key)
        {
            if (!m_key2EntityIDDic.ContainsKey(key))
            {
                return;
            }
            m_key2EntityIDDic.Remove(key);
        }

        public string FormateLocalEntityKey(ulong runtimeID, int otherKey)
        {
            return $"{runtimeID}_{otherKey}";
        }

        public void RemoveFormateKey2Entity(string key)
        {
            ulong entityID = GetFormateKey2EntityID(key);
            if (entityID != 0)
            {
                RemoveLocalEntity(entityID);
            }

            RemoveFormateKey2EntityID(key);
        }

        #endregion

        #region Timeline


        /// <summary>
        /// 实体隐藏
        /// </summary>
        /// <param name="hide">隐藏/显示</param>
        /// <param name="entityTypes">类型，不包括玩家本身</param>
        public void SetEntityHide(bool hide, List<E_EntityType> entityTypes)
        {
            for (int i = 0; i < m_listEntityCtrl.Count; ++i)
            {
                var entityCtrl = m_listEntityCtrl[i];
                if (entityCtrl != null && entityCtrl.M_Curr != null)
                {
                    if (entityCtrl.M_Curr.EntityId == GameManager.Instance.mainPlayerId)
                    {
                        continue;
                    }
                    if (entityTypes.Contains(entityCtrl.M_Curr.EntityType))
                    {
                        if (hide)
                        {

                            entityCtrl.M_Curr.ActionOnStartHidden?.Invoke(false);
                        }
                        else
                        {
                            entityCtrl.M_Curr.ActionOnStopHidden?.Invoke(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 隐藏空气墙
        /// </summary>
        /// <param name="hide"></param>
        public void SetHideAirWallEffect(bool hide)
        {
            if (m_map != null)
            {
                m_map.SetHideEffect(hide);
            }
        }
        #endregion

        #region 添加特效

        public void AddFxNttToLifeNtt(ulong playerId, MessageHandleData data)
        {
            PlayerCtrlGroup playerCtrlGroup = (PlayerCtrlGroup)GetEntityCtr(playerId);

            if (playerCtrlGroup != null)
            {
                playerCtrlGroup.OnFxCreateRet(data);
            }
            // 初始位置,===
            //     简单池
        }

        /// <summary>
        /// 给实体播放特效
        /// </summary>
        /// <param name="entityID">实体ID</param>
        /// <param name="effectKey">特效Key</param>
        /// <param name="effectName">特效名</param>
        /// <param name="effectPath">特效路径</param>
        /// <param name="effectPath">绑点【SkillEditor.HangPoint】</param>
        public void PlayEntityEffect(ulong entityID, string effectKey, string effectName, string effectPath, int hangPoint = (int)HangPoint.Root)
        {
            EntityCtrlBase entityCtrlGroup = GetEntityCtr(entityID);
            if (entityCtrlGroup == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} AddMainPlayerEffect() entityID={entityID},entityCtrlGroup=null err!!!");
                return;
            }

            FxParam fxParam = new();
            fxParam.Reset();
            fxParam.InitWithLogic(effectName, (HangPoint)hangPoint);
            entityCtrlGroup.M_Curr.ActionOnPlaySpecialEffects?.Invoke(fxParam, effectKey, effectPath);
        }

        //internal void AddFoodRandom()
        //{
        //    Vector3 pos;
        //    pos.x = m_context.random.Range(0, m_context.mapSize.x);
        //    pos.y = m_context.random.Range(0, m_context.mapSize.y);
        //    pos.z = m_context.random.Range(0, m_context.mapSize.z);

        //    pos.z = 0;
        //    int color = m_context.random.Range(1, m_mapPlayerData.Count);
        //    AddFood(pos, color);
        //}

        //internal void AddFood(Vector3 pos, int color)
        //{
        //    NormalFood food = EntityFactory.InstanceEntity<NormalFood>();
        //    food.Create(0, color, pos);

        //    m_listFood.Add(food);
        //}

        //private void RemoveFoodAt(int i)
        //{
        //    EntityObject food = m_listFood[i];
        //    m_listFood.RemoveAt(i);
        //    EntityFactory.ReleaseEntity(food);
        //}

        //public List<EntityObject> GetFoodList()
        //{
        //    return m_listFood;
        //}

        #endregion

        #region 治疗飘字

        public void OnCureFloatingText(long hp, ulong ownerId)
        {
            BattleManager.Instance.OnCureFloatingText(hp, ownerId);
        }

        #endregion

        #region 当前攻击的实体

        /// <summary>
        /// 目标与主角移动回调
        /// </summary>
        public void OnActionOnCheckTargetMove()
        {
            BattleManager.Instance.OnActionOnCheckTargetMove();
            BattleManager.Instance.OnMainPlayMove();
        }

        #endregion

        #region 通用次数

        public int GetRemainFreeRelieveCount()
        {
            ComCountMDMgr comCountMDMgr =
                (ComCountMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.ComCount);
            return comCountMDMgr.GetRemainCount(GameConfig.ComCount_Revive);
        }

        /// <summary>
        /// Boss挑战次数
        /// </summary>
        /// <returns></returns>
        public int GetGNActivBoss_CommonCount()
        {
            ComCountMDMgr comCountMDMgr =
                (ComCountMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.ComCount);
            return comCountMDMgr.GetRemainCount(SystemConstConfigs.GNActivBoss_CommonCount);
        }

        /// <summary>
        /// 获得主角今日获得的总经验
        /// </summary>
        /// <returns></returns>
        public int GetMainPlayerAllExp_GetAddCount()
        {
            ComCountMDMgr comCountMDMgr = (ComCountMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.ComCount);
            return comCountMDMgr.GetAddCount(GameConfig.EComCount_DailyPlayerGetExp_24);
        }

        /// <summary>
        /// 获得世界等级的开放配置ID
        /// </summary>
        /// <returns></returns>
        public int GetLevelLimitID_CommonCount()
        {
            ComCountMDMgr comCountMDMgr = (ComCountMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.ComCount);
            return comCountMDMgr.GetRemainCount(GameConfig.EComCount_DailyPlayerGetExp_24);
        }

        #endregion

        #region 后处理以及摄像机效果

        /// <summary>
        /// 设置挂在摄像机上的效果
        /// </summary>
        /// <param name="b"></param>
        /// <param name="path"></param>
        public void SetBattleCameraEffect(bool b, string path = "")
        {
            //GameManager.Instance.SetBattleCameraEffect(true, "Effects/BattleSceen/Fx_FB_MengJKJ_camera_002");
            Camera camera = GetGameCamera();
            if (camera != null)
            {
                Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetCameraEffect(b, camera.transform, path);
            }
        }

        public bool IsOpenFog = false;
        public bool IsOpenFogOld = false;

        public void SetUIFog(bool isOpenUI)
        {
            if (isOpenUI)
            {
                IsOpenFogOld = IsOpenFog;
                SetFog(false);
            }
            else
            {
                SetFog(IsOpenFogOld);
            }
        }

        public void SetFog(bool b)
        {
            IsOpenFog = b;
            Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetFogRenderPassFeature(b);
        }

        public void SetSGSR(bool b)
        {
            IsOpenFog = b;
            Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetSGSRRenderPassFeature(b);
        }

        #endregion

        #region SceneFlag

        private uint oldSceneFlag;

        public void ChangeSceneFlag(uint newSceneFlag)
        {
            //id 1000是每日免费复活次数

            //打印出来
            Debug.Log("old:  " + Convert.ToString(oldSceneFlag, 2) + " new:" + Convert.ToString(newSceneFlag, 2));

            //1位是迷雾
            uint f1 = oldSceneFlag & 0b10;
            uint f2 = newSceneFlag & 0b10;
            if (f1 != f2)
            {
                if (f2 == 0)
                {
                    //关迷雾
                    SetFog(false);
                }
                else
                {
                    //开迷雾
                    SetFog(true);
                }
            }

            f2 = newSceneFlag & 0b100;
            uint f3 = newSceneFlag & 0b1000;

            uint tt = f2 >> 2;
            uint tt2 = f3 >> 3;

            UIManager.Instance.SetScreenUIEffect(UIDef.ScreenUIEffects_FX_XinSC_Global3, tt == 1);
            UIManager.Instance.SetScreenUIEffect(UIDef.ScreenUIEffects_Dream, tt2 == 1);

            uint f4 = newSceneFlag & 0b10000;
            uint tt4 = f4 >> 4;

            uint f5 = newSceneFlag & 0b100000;
            uint tt5 = f5 >> 5;

            uint f6 = newSceneFlag & 0b1000000;
            uint tt6 = f6 >> 6;

            uint f7 = newSceneFlag & 0b10000000;
            uint tt7 = f7 >> 7;
            if (tt7 == 1)
            {
                StarScenesManager.Instance.SwitchFogAndCloud(4, true);
            }
            else if (tt6 == 1)
            {
                StarScenesManager.Instance.SwitchFogAndCloud(3, true);
            }
            else if (tt5 == 1)
            {
                StarScenesManager.Instance.SwitchFogAndCloud(2, true);
            }
            else if (tt4 == 1)
            {
                StarScenesManager.Instance.SwitchFogAndCloud(1, true);
            }
            else
            {
                StarScenesManager.Instance.SwitchFogAndCloud(5, false);
            }

            uint f8 = newSceneFlag & 0b100000000;
            uint tt8 = f8 >> 8;
            UIManager.Instance.SetScreenUIEffect(UIDef.ScreenUIEffects_FX_XinSC_Global4, tt8 == 1);
            oldSceneFlag = newSceneFlag;
        }

        #endregion

        #region 服务器全局设置（包括开服时间啥的

        public GloInfoSrvRet GloInfoSrv;

        private void OnGloInfoSrvRetMsg(MessageHandleData data)
        {
            //id 1000是每日免费复活次数
            GloInfoSrv = (ProtoMsg.GloInfoSrvRet)data.data;
        }

        /// <summary>
        /// 真实的开服天数， 开服天数未来时间,服务器会给个 负数
        /// </summary>
        public int OpenServerDay()
        {
            if (GloInfoSrv != null)
            {
                return GloInfoSrv.GRisk.OffDay;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 显示的开服天数, 未来时间 也会返回 0
        /// </summary>
        /// <returns></returns>
        public int OpenShowServerDay()
        {
            var serverDay = OpenServerDay();
            if (serverDay < 0)
            {
                return 0;
            }
            return serverDay;
        }

        #endregion

        #region 服务器Flag

        public void SetScreenUIEffect(int effectId, int kg)
        {
            //Dream滤镜
            if (effectId == 2)
            {
                UIManager.Instance.SetScreenUIEffect(UIDef.ScreenUIEffects_Dream, kg == 1);
            }
        }


        private uint oldFlag;

        private void OnInsFlagNtfMsg(MessageHandleData data)
        {
            //id 1000是每日免费复活次数
            ProtoMsg.InsFlagNtf insFlagNtf = (ProtoMsg.InsFlagNtf)data.data;
            uint newFlag = insFlagNtf.Flag;
            //打印出来
            Debug.Log("old:  " + Convert.ToString(oldFlag, 2) + " new:" + Convert.ToString(newFlag, 2));

            //0位是迷雾(废弃)
            uint f = newFlag & 1;
            if (f == 0)
            {
                //关迷雾
                SetFog(false);
            }
            else
            {
                //开迷雾
                SetFog(true);
            }


            //1位是切进里场景(废弃)
            f = newFlag & 0b10;
            if (f == 0b10)
            {
                //切进里场景
                if (!StarScenesManager.Instance.CurrentInnerTag)
                {
                    StarScenesManager.Instance.SwitchInnerOuterWorld();
                }
            }
            else
            {
                //切出里场景
                if (StarScenesManager.Instance.CurrentInnerTag)
                {
                    StarScenesManager.Instance.SwitchInnerOuterWorld();
                }
            }

            //2 3位是Dream滤镜
            f = newFlag & 0b100;
            uint f2 = newFlag & 0b1000;

            if (f == 0b100 || f2 == 0b1000)
            {
                //打开
                UIManager.Instance.SetScreenUIEffect(UIDef.ScreenUIEffects_Dream, true);
            }
            else
            {
                //关闭
                UIManager.Instance.SetScreenUIEffect(UIDef.ScreenUIEffects_Dream, false);
            }



            oldFlag = newFlag;
        }

        #endregion



        #region 环任务相关
        public RingTaskModule GetRingTaskModule()
        {


            return ModuleManager.Instance.GetModule(ModuleDef.Name.RingTaskModule) as RingTaskModule;
        }

        public void CheckRingTaskGetInfo()
        {
            GetRingTaskModule().CheckRingTaskGetInfo();
        }

        #endregion


        #region 装备槽位相关

        public EquipSlotControllerModule GetEquipSlotModule()
        {
            //if (equipSlotModule == null)
            //{
            //    equipSlotModule = ModuleManager.Instance.GetModule(ModuleDef.Name.EquipSlotControllerModule) as Module.EquipSlotControllerModule;
            //}

            //return equipSlotModule;

            return ModuleManager.Instance.GetModule(ModuleDef.Name.EquipSlotControllerModule) as EquipSlotControllerModule;
        }

        public void ClearEquipSlotDatas()
        {
            GetEquipSlotModule().ClearEquipSlotDatas();
        }

        /// <summary>
        /// 获取装备槽位
        /// </summary>
        /// <param name="slot_id"></param>
        /// <returns></returns>
        public ProtoMsg.EquipSlotData GetEquipSlotData(int slot_id)
        {
            return GetEquipSlotModule().GetEquipSlotData(slot_id);
        }

        /// <summary>
        /// 获取全身最低的槽位等级
        /// </summary>
        /// <returns></returns>
        public int GetEquipSlotResonateLv()
        {
            return GetEquipSlotModule().GetEquipSlotResonateLv();
        }

        /// <summary>
        /// 获取平均共鸣等级
        /// </summary>
        /// <returns></returns>
        public int GetEquipSlotAvgResonateLv()
        {
            int SlotResonateLv = 0;

            var minLv = GetEquipSlotResonateLv();
            int jobID = (int)GameManager.Instance.GetPlayerJob(); ;
            int maxResonanceLv = LocalDataManager.Instance.GetJobMaxResonanceLv(jobID);

            for (int i = 1; i <= maxResonanceLv; i++)
            {
                // 取的 共鸣等级对应的 共鸣配置
                var esr = LocalDataManager.Instance.GetEquipSlotResonanceDataCell(i, jobID);
                if (esr != null)
                {
                    if (minLv >= esr.Level)
                        SlotResonateLv = i;
                }
            }


            return SlotResonateLv;
        }


        #endregion

        #region 道具相关

        public int GetPlayerLevel()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);
        }

        public int GetPlayerEXP()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerExp);
        }

        public int GetSkillPoint()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SkillPoint);
        }

        public int GetSkillPointUsed()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SkillPointUsed);
        }

        public string GetPlayerName()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return string.Empty;
            }

            return M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.Name);
        }

        public uint GetPlayerJob()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return GetMDJobID();
            }

            var jobId = M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            if (jobId == 0)
            {
                jobId = GetMDJobID();
            }

            return jobId;
        }

        public int GetPlayJobBaseID()
        {
            uint jobID = GetMDJobID();
            if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.Data != null)
            {
                jobID = M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            }
            var jobCfg = LocalDataManager.Instance.GetJobDataCell((int)jobID);
            if (jobCfg != null)
            {
                return jobCfg.GetBaseJob();
            }
            return (int)jobID;
        }

        /// <summary>
        /// 采集活力值
        /// </summary>
        /// <returns></returns>
        public int GetPlayerCollectEnergy()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return 0;
            }

            var energy = M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.CollectEnergy);
            return energy;
        }

        private uint GetMDJobID()
        {
            if (HeroMD != null && HeroMD.Job > 0)
            {
                return (uint)HeroMD.Job;
            }

            return 0;
        }


        public void DoFunction(int id, ulong entityID)
        {
            StarProject.Service.Function.GlobalFunctionManager.Instance.DoFunction(id, entityID);
        }

        #endregion

        #region 通缉任务属性

        #region 通缉协议同步

        //////// 所有分线的所有大地图的各个阶级的NPC状态信息
        /////// {{分线id,{a}},{分线id,{a}}}
        ////// {a}->{{大地图id,{b}},{大地图id,{b}}}
        ///// {b}->{{阶位类型,{c}},{阶位类型,{c}}}
        //// {c}->{{点位ID,详细信息},{点位ID,详细信息}}[点位ID,当前点位状态，当前进入副本队伍数量]
        public Dictionary<ulong, Dictionary<ulong, Dictionary<int, Dictionary<int, WTaskPointTarInfo>>>> AllWantedInfo =
            new();

        //缓存点阶位开始刷新时间
        ///{{阶位类型,开始时间},{阶位类型,开始时间},{阶位类型,开始时间}}
        public Dictionary<int, long> TarInfoStartShowTime = new();

        // 全量同步通缉信息
        private void OnWTaskGetAllInfoRet(MessageHandleData data)
        {

            WTaskGetAllInfoRet wTaskGetAllInfoRet = (WTaskGetAllInfoRet)data.data;
            //SGF.Debuger.LogWarning($"通缉任务 全量同步 wTaskGetAllInfoRet={wTaskGetAllInfoRet}");
            if (wTaskGetAllInfoRet.AllInfo == null)
            {
                return;
            }

            var AllTarInfo = wTaskGetAllInfoRet.AllInfo.AllTarInfo;
            // 格式化全地图点位NPC状态信息
            if (AllTarInfo != null)
            {
                AllWantedInfo.Clear();
                // 遍历分线
                foreach (var item in AllTarInfo)
                {
                    if (!AllWantedInfo.ContainsKey(item.Key))
                    {
                        AllWantedInfo[item.Key] = new();
                    }

                    var dic = AllWantedInfo[item.Key];
                    // 遍历地图
                    foreach (var item1 in item.Value.SrvTarInfo)
                    {
                        if (!dic.ContainsKey(item1.Key))
                        {
                            dic[item1.Key] = new();
                        }

                        var dic1 = dic[item1.Key];
                        // 遍历点阶位类型信息
                        if (item1.Value != null)
                        {
                            // 一阶
                            {
                                dic1[1] = new();
                                var dic2 = dic1[1];
                                foreach (var item2 in item1.Value.OneInfo)
                                {
                                    dic2.Add(item2.Key, item2.Value);
                                }
                            }
                            // 二阶
                            {
                                dic1[2] = new();
                                var dic2 = dic1[2];
                                foreach (var item2 in item1.Value.TwoInfo)
                                {
                                    dic2.Add(item2.Key, item2.Value);
                                }
                            }
                            // 三阶
                            {
                                dic1[3] = new();
                                var dic2 = dic1[3];
                                foreach (var item2 in item1.Value.ThreeInfo)
                                {
                                    dic2.Add(item2.Key, item2.Value);
                                }
                            }
                        }
                    }
                }
            }

            // 缓存点阶位开始刷新时间
            TarInfoStartShowTime.Clear();
            TarInfoStartShowTime.Add(1, wTaskGetAllInfoRet.AllInfo.OneNextRefTime);
            TarInfoStartShowTime.Add(2, wTaskGetAllInfoRet.AllInfo.TwoNextRefTime);
            TarInfoStartShowTime.Add(3, wTaskGetAllInfoRet.AllInfo.ThreeNextRefTime);
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "InformWantedWindow", new object[] { 1 });

            //SGF.Debuger.LogWarning($"通缉任务 全量同步 一阶 刷新时间={wTaskGetAllInfoRet.AllInfo.OneNextRefTime}");
            //SGF.Debuger.LogWarning($"通缉任务 全量同步 二阶 刷新时间={wTaskGetAllInfoRet.AllInfo.TwoNextRefTime}");
            //SGF.Debuger.LogWarning($"通缉任务 全量同步 三阶 刷新时间={wTaskGetAllInfoRet.AllInfo.ThreeNextRefTime}");

            InitCreateWantedEntity();
        }

        // 单个点位信息发生改变同步
        private void OnWTaskPointNtf(MessageHandleData data)
        {
            WTaskPointNtf wTaskPointNtf = (WTaskPointNtf)data.data;
            if (wTaskPointNtf.PInfo == null)
            {
                return;
            }

            if (AllWantedInfo == null)
            {
                AllWantedInfo = new();
            }

            // 分线数据
            if (!AllWantedInfo.ContainsKey(wTaskPointNtf.SrvID))
            {
                AllWantedInfo[wTaskPointNtf.SrvID] = new();
            }

            var srvIDInfo = AllWantedInfo[wTaskPointNtf.SrvID];
            // 地图数据
            if (!srvIDInfo.ContainsKey(wTaskPointNtf.SpaceID))
            {
                srvIDInfo[wTaskPointNtf.SpaceID] = new();
            }

            var spaceIDInfo = srvIDInfo[wTaskPointNtf.SpaceID];
            // 点阶位类型数据
            if (!spaceIDInfo.ContainsKey(wTaskPointNtf.WTaskType))
            {
                spaceIDInfo[wTaskPointNtf.WTaskType] = new();
            }

            var wTaskTypeInfo = spaceIDInfo[wTaskPointNtf.WTaskType];
            // 单个点阶位数据
            if (!wTaskTypeInfo.ContainsKey(wTaskPointNtf.PointID))
            {
                wTaskTypeInfo[wTaskPointNtf.PointID] = new();
            }

            wTaskTypeInfo[wTaskPointNtf.PointID] = wTaskPointNtf.PInfo;

            // 通知实体改变
            InformRefreshWantedEntity(wTaskPointNtf.SrvID, wTaskPointNtf.SpaceID, wTaskPointNtf.WTaskType, wTaskPointNtf.PInfo);
            //SGF.Debuger.LogWarning($"通缉任务 单个点位信息发生改变同步 分线ID={wTaskPointNtf.SpaceID},地图ID={wTaskPointNtf.SrvID},类型={wTaskPointNtf.WTaskType},点位信息={wTaskPointNtf.PInfo}");

            // 通知界面数据改变了
            // 0全刷1刷新阶位信息2刷新次数
            ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "InformWantedWindow", new object[] { 1 });
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "InformWantedWindow", new object[] { 1 });
        }

        // 某阶刷新同步
        private void OnWTaskStageNtf(MessageHandleData data)
        {
            WTaskStageNtf wTaskStageNtf = (WTaskStageNtf)data.data;
            if (wTaskStageNtf.STageInfo == null)
            {
                return;
            }

            if (wTaskStageNtf.STageInfo != null && wTaskStageNtf.STageInfo.AllTarInfo != null)
            {
                // 统一改变的点阶位类型
                int chanageType = wTaskStageNtf.WTaskType;
                var AllTarInfo = wTaskStageNtf.STageInfo.AllTarInfo;
                // 遍历分线
                foreach (var item in AllTarInfo)
                {
                    if (!AllWantedInfo.ContainsKey(item.Key))
                    {
                        AllWantedInfo[item.Key] = new();
                    }

                    var dic = AllWantedInfo[item.Key];
                    // 遍历地图
                    foreach (var item1 in item.Value.SrvTarInfo)
                    {
                        if (!dic.ContainsKey(item1.Key))
                        {
                            dic[item1.Key] = new();
                        }

                        var dic1 = dic[item1.Key];
                        // 遍历点阶位类型信息
                        if (item1.Value != null)
                        {
                            // 先清空原来的点阶位类型信息
                            if (chanageType == 1)
                            {
                                // 一阶
                                dic1[1] = new();
                                var dic2 = dic1[1];
                                foreach (var item2 in item1.Value.OneInfo)
                                {
                                    dic2.Add(item2.Key, item2.Value);
                                    InformRefreshWantedEntity(item.Key, item1.Key, chanageType, item2.Value);
                                }
                            }
                            else if (chanageType == 2)
                            {
                                // 二阶
                                dic1[2] = new();
                                var dic2 = dic1[2];
                                foreach (var item2 in item1.Value.TwoInfo)
                                {
                                    dic2.Add(item2.Key, item2.Value);
                                    InformRefreshWantedEntity(item.Key, item1.Key, chanageType, item2.Value);
                                }
                            }
                            else if (chanageType == 3)
                            {
                                // 三阶
                                dic1[3] = new();
                                var dic2 = dic1[3];
                                foreach (var item2 in item1.Value.ThreeInfo)
                                {
                                    dic2.Add(item2.Key, item2.Value);
                                    InformRefreshWantedEntity(item.Key, item1.Key, chanageType, item2.Value);
                                }
                            }
                        }
                    }
                }

                // 缓存点阶位开始刷新时间
                TarInfoStartShowTime.Clear();
                TarInfoStartShowTime.Add(1, wTaskStageNtf.STageInfo.OneNextRefTime);
                TarInfoStartShowTime.Add(2, wTaskStageNtf.STageInfo.TwoNextRefTime);
                TarInfoStartShowTime.Add(3, wTaskStageNtf.STageInfo.ThreeNextRefTime);
                //SGF.Debuger.LogWarning($"通缉任务 某阶刷新同步 一阶 刷新时间={wTaskStageNtf.STageInfo.OneNextRefTime}");
                //SGF.Debuger.LogWarning($"通缉任务 某阶刷新同步 二阶 刷新时间={wTaskStageNtf.STageInfo.TwoNextRefTime}");
                //SGF.Debuger.LogWarning($"通缉任务 某阶刷新同步 三阶 刷新时间={wTaskStageNtf.STageInfo.ThreeNextRefTime}");
                // 通知界面数据改变了
                // 0全刷1刷新阶位信息2刷新次数
                ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "InformWantedWindow",
                    new object[] { 1 });
                ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "InformWantedWindow", new object[] { 1 });
            }
        }

        #endregion

        // 获取当前地图的所有的通缉实体
        public Dictionary<int, Dictionary<int, WTaskPointTarInfo>> GetCurMapWantedData()
        {
            //int MapID = GetCurMapId(); /// 地图ID
            ulong SpaceID = GetSpaceID();   // 当前服务器地图唯一ID

            ulong ServerID = GetCurServerID(); /// 分线ID
            Dictionary<int, Dictionary<int, WTaskPointTarInfo>> curServerMapAllWantedData = new();
            if (AllWantedInfo != null)
            {
                if (AllWantedInfo.TryGetValue(ServerID, out var ServerData))
                {
                    if (ServerData != null)
                    {
                        if (ServerData.TryGetValue(SpaceID, out curServerMapAllWantedData))
                        {
                        }
                    }
                }
            }

            return curServerMapAllWantedData;
        }

        // 获取当前地图可挑战的通缉信息
        private WTaskPointTarInfo GetCurMapChallengeWantedData(int pointType)
        {
            WTaskPointTarInfo wTaskPointTarInfo = null;
            float minDistance = 0;
            Dictionary<int, Dictionary<int, WTaskPointTarInfo>> curServerMapAllWantedData = GetCurMapWantedData();
            if (curServerMapAllWantedData != null)
            {
                if (curServerMapAllWantedData.TryGetValue(pointType, out var curTypeAllWantedData))
                {
                    foreach (var item in curTypeAllWantedData)
                    {
                        if (item.Value != null && item.Value.CurState == 0)
                        {
                            int mapID = GetCurMapId(); /// 地图ID
                            bool isUnlockMap = CheckTeamUnlockMap(mapID);
                            if (isUnlockMap)
                            {
                                Vector3 wantedEntityPos = GetWantedEntityPos(pointType, item.Value.PointID);
                                float dis = Vector3.Distance(wantedEntityPos, GetEntityPosById(mainPlayerId));
                                if (wTaskPointTarInfo == null)
                                {
                                    wTaskPointTarInfo = item.Value;
                                    minDistance = dis;
                                }
                                else if (dis < minDistance)
                                {
                                    wTaskPointTarInfo = item.Value;
                                }
                            }
                        }
                    }
                }
            }

            return wTaskPointTarInfo;
        }

        // 获取当前地图的指定类型指定点位的通缉实体坐标
        private Vector3 GetWantedEntityPos(int pointType, int pointID)
        {
            Vector3 pos = Vector3.zero;
            if (GameMap.sceneJsonData.WantedTasks != null && GameMap.sceneJsonData.WantedTasks.Count > 0)
            {
                foreach (var areaJson in GameMap.sceneJsonData.WantedTasks)
                {
                    if (areaJson.Value != null && areaJson.Value.Index == pointID && areaJson.Value.Type == pointType)
                    {
                        pos = areaJson.Value.Position.Convert();
                        break;
                    }
                }
            }

            return pos;
        }

        // 根据类型获取可以跳转的通缉点位信息
        private WTaskPointTarInfo GetChallengeWantedDataByType(int pointType, out ulong sercerID, out ulong mapID)
        {
            //ulong MapID = (ulong)GetCurMapId(); /// 地图ID
            ulong SpaceID = GetSpaceID();   // 当前服务器地图唯一ID

            ulong ServerID = GetCurServerID(); /// 分线ID

            // 遍历分线
            foreach (var item in AllWantedInfo)
            {
                //if (item.Key != ServerID)
                {
                    // 遍历地图
                    foreach (var item1 in item.Value)
                    {
                        if (item1.Key != SpaceID)
                        {
                            // 遍历点阶位类型信息
                            if (item1.Value != null)
                            {
                                if (!item1.Value.ContainsKey(pointType))
                                {
                                    item1.Value[pointType] = new();
                                }

                                var item2 = item1.Value[pointType];
                                foreach (var item3 in item2)
                                {
                                    var child = item3.Value;
                                    if (child != null && child.CurState == 0)
                                    {
                                        sercerID = item.Key;
                                        // 服务器说：规则改了，地图id要/1000，超过1000条线，可能会有除不尽的问题，但是应该不会
                                        mapID = item1.Key / 1000;

                                        //判断地图是否解锁
                                        //bool isUnlockMap = CheckMapIsOpen((int)mapID);
                                        bool isUnlockMap = CheckTeamUnlockMap((int)mapID);
                                        if (isUnlockMap)
                                        {
                                            return child;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            sercerID = ServerID;
            mapID = SpaceID / 1000;
            return null;
        }

        private bool CheckTeamUnlockMap(int mapID)
        {
            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TeamModule) as LuaModule;
            var sceneMapDataCell = LocalDataManager.Instance.GetMapCfgData(mapID);
            if (module != null && sceneMapDataCell != null)
            {
                var luatable = module.GetLuaTable();
                if (luatable != null)
                {
                    var teamInfo = luatable.Get<TeamInfo>("tinfo");
                    if (teamInfo != null)
                    {
                        bool result = true;

                        foreach (var user in teamInfo.UserInfos)
                        {
                            if (user.Level < sceneMapDataCell.OpenRoleLv)
                            {
                                result = false;
                                break;
                            }
                        }
                        return result;
                    }
                }
            }
            return false;
        }

        // 通知通缉实体刷新
        private void InformRefreshWantedEntity(ulong srvID, ulong spaceID, int pointType, WTaskPointTarInfo wTaskPointTarInfo)
        {
            WantedEntity wantedEntity = GetWantedEnityByPointID(srvID, spaceID, wTaskPointTarInfo.PointID, pointType);
            if (wantedEntity == null)
            {
                if (GameMap.SceneReady && GameMap.DataReady)
                {
                    //int MapID = GetCurMapId(); /// 地图ID
                    ulong SpaceID = GetSpaceID(); /// 地图ID
                    ulong ServerID = GetCurServerID(); /// 分线ID
                    if (ServerID == srvID && SpaceID == spaceID)
                    {
                        OnCreateLocalEntity(E_LocalEntityType.WantedEnity, pointType, srvID, spaceID, wTaskPointTarInfo);
                    }
                }
            }
            else
            {
                wantedEntity.RefreshWTaskPointTarInfo(wTaskPointTarInfo);
            }

            GlobalEvent.onRefreshWTaskPointTarInfo?.Invoke(pointType, wTaskPointTarInfo);
        }

        // 获取主角的通缉属性
        public WantTaskMD GetMainPlayerWantTaskMD()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return null;
            }

            object info = M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.WantTaskInfo);
            if (info == null)
            {
                return null;
            }
            else
            {
                ProtoMsg.WantTaskMD wantTaskMD = (ProtoMsg.WantTaskMD)info;
                return wantTaskMD;
            }
        }

        /// <summary>
        /// 自动寻路到通缉实体位置
        /// </summary>
        /// <param name="pointType">-1：按优先级找；其他跟指定类型找</param>
        /// <param name="action"></param>
        public void FindGoWantedEntity(int pointType, System.Action<bool> action = null)
        {
            WTaskPointTarInfo wTaskPointTar = null;
            ulong MapID = (ulong)GetCurMapId(); /// 地图ID
            ulong ServerID = GetCurServerID(); /// 分线ID
            //相同条件时，优先寻路至距离玩家同地图且距离更近的目标
            if (pointType != -1)
            {
                // 寻找最近的指定类型的通缉实体
                wTaskPointTar = GetCurMapChallengeWantedData(pointType);
                if (wTaskPointTar == null)
                {
                    wTaskPointTar = GetChallengeWantedDataByType(pointType, out ServerID, out MapID);
                }
            }
            else
            {
                // 1.未满员的3阶目标
                {
                    wTaskPointTar = GetCurMapChallengeWantedData(3);
                    if (wTaskPointTar == null)
                    {
                        wTaskPointTar = GetChallengeWantedDataByType(3, out ServerID, out MapID);
                    }
                }
                // 2.2阶目标
                {
                    if (wTaskPointTar == null)
                    {
                        wTaskPointTar = GetCurMapChallengeWantedData(2);
                        if (wTaskPointTar == null)
                        {
                            wTaskPointTar = GetChallengeWantedDataByType(2, out ServerID, out MapID);
                        }
                    }
                }
                // 3.1阶目标
                {
                    if (wTaskPointTar == null)
                    {
                        wTaskPointTar = GetCurMapChallengeWantedData(1);
                        if (wTaskPointTar == null)
                        {
                            wTaskPointTar = GetChallengeWantedDataByType(1, out ServerID, out MapID);
                        }
                    }
                }
            }

            if (wTaskPointTar == null)
            {
                return;
            }

            TaskHelper.FindPathByWanted(ServerID, MapID, wTaskPointTar.PointID, 1f, action);
        }

        #endregion

        /// <summary>
        /// 是否解锁了支付系统
        /// </summary>
        public bool IsUnLockPayFunction()
        {
            var dc = LocalDataManager.Instance.GetSystemDataCell(319);

            return dc.Value == 1;
        }

        /// <summary>
        /// 获取服务器时间戳
        /// </summary>
        /// <returns></returns>
        public long GetServerTimeStamp()
        {
            return Service.Time.TimeManager.Instance.GetServerTimeStamp();
        }

        public List<ulong> GetAllInteractiveList()
        {
            //UpdateAllNpcServiceState();

            List<ulong> list = new();

            foreach (var entity in m_mapEntityCtrlCopy)
            {
                if (entity.Value == null)
                {
                    continue;
                }

                if (entity.Value.Data == null)
                {
                    continue;
                }

                GameNPCCtrlGroup ctrlGroup = entity.Value as GameNPCCtrlGroup;
                if (ctrlGroup == null)
                {
                    continue;
                }

                if (
                    entity.Value.Data.EntityType == E_EntityType.Npc && ctrlGroup.IsShow && //NPC
                    Vector3.Distance(GetEntityPosById(entity.Key), GetEntityPosById(mainPlayerId)) <
                    ctrlGroup.Range //距离判断
                )
                {
                    uint npcBaseID = GetEntityCfgID(entity.Key);
                    var npc = LocalDataManager.Instance.GetNPCDataCell(npcBaseID);
                    if (npc.GetIsInteractive())
                    {
                        list.Add(entity.Key);
                    }
                }
            }

            if (m_InterActionModule != null)
            {
                var _ls = m_InterActionModule.InRangeInterActionObjects();
                if (_ls != null && _ls.Count > 0)
                {
                    foreach (var item in _ls)
                    {
                        list.Add(item);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取离得最近的交互物件实体id
        /// </summary>
        /// <returns></returns>
        public ulong GetCloestInteractive()
        {
            float min_dis = 10000.0f;
            ulong aim_entityId = 0;

            bool isNpcRangeInside = false; // 是否NPC范围内
            foreach (var entity in m_mapEntityCtrlCopy)
            {
                if (entity.Value == null)
                {
                    continue;
                }

                if (entity.Value.Data == null)
                {
                    continue;
                }

                GameNPCCtrlGroup ctrlGroup = entity.Value as GameNPCCtrlGroup;
                if (ctrlGroup == null)
                {
                    continue;
                }

                if (!ctrlGroup.IsShow)
                {
                    continue;
                }

                float dis = Vector3.Distance(GetEntityPosById(entity.Key), GetEntityPosById(mainPlayerId));
                if (
                    entity.Value.Data.EntityType == E_EntityType.Npc && ctrlGroup.IsShow && //NPC
                    dis < ctrlGroup.Range && //距离判断
                    dis < min_dis //拿最小的距离
                )
                {
                    uint npcBaseID = GetEntityCfgID(entity.Key);
                    var npc = LocalDataManager.Instance.GetNPCDataCell(npcBaseID);
                    if (npc.GetIsInteractive() && (BusinessManager.Instance.IsNpcCanService(npcBaseID) ||
                                                   (npc.CommonCondition.Count > 0 && npc.CommonEffect.Count > 0 && npc.CommonEffect[0] > 0) ||
                                                   (npc.Dialog.Count > 0 && npc.Dialog[0] > 0)))
                    {
                        min_dis = dis;
                        aim_entityId = entity.Key;

                        var npcE = entity.Value.M_Curr as NPCEntityBase;
                        if (npcE != BattleManager.Instance.CurCheckNPC)
                        {
                            BattleManager.Instance.CurCheckNPC = npcE;
                        }

                        isNpcRangeInside = true;
                    }
                }
            }

            // 如果不在NPC范围内了就清空选中的NPC信息
            if (!isNpcRangeInside)
            {
                BattleManager.Instance.CurCheckNPC = null;
            }

            if (m_InterActionModule != null)
            {
                var _ls = m_InterActionModule.InRangeInterActionObjects();
                if (_ls != null && _ls.Count > 0)
                {
                    foreach (var item in _ls)
                    {
                        var entity = GetEntityCtr(item);
                        ObjectCtrlGroup ctrlGroup = entity as ObjectCtrlGroup;
                        if (ctrlGroup != null)
                        {
                            if (!ctrlGroup.IsShow)
                            {
                                continue;
                            }

                            var pos = ctrlGroup.GetPosition();
                            float dis = Vector3.Distance(pos, GetEntityPosById(mainPlayerId));

                            if (dis < min_dis)
                            {
                                min_dis = dis;

                                aim_entityId = item;
                            }
                        }
                    }
                }
            }

            return aim_entityId;
        }

        /// <summary>
        /// 获取离得最近的本地实体
        /// </summary>
        /// <returns></returns>
        public EntityLocalDynamic GetCloestLocalEntity()
        {
            EntityLocalDynamic entity = null;
            float min_dis = 10000.0f;
            Vector3 mainPlayerPos = GetEntityPosById(mainPlayerId);
            foreach (var item in m_LocalEntitys)
            {
                if (item.IsEntryTrigger && item.IsHide == false)
                {
                    Vector3 pos = item.Position();
                    float dis = Vector3.Distance(pos, mainPlayerPos);
                    if (dis < min_dis)
                    {
                        min_dis = dis;
                        entity = item;
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// 获取离得最近的Boss
        /// </summary>
        /// <returns></returns>
        public NPCEntityBase GetCloestBoss()
        {
            float min_dis = 10000.0f;
            NPCEntityBase nPCEntityBase = null;

            Vector3 mainPlayerPos = M_MainPlayerCtrlBase.M_Curr.Position();
            foreach (var entity in m_mapEntityCtrlCopy)
            {
                if (entity.Value == null)
                {
                    continue;
                }

                if (entity.Value.Data == null)
                {
                    continue;
                }

                if (entity.Value.Data.IsDead)
                {
                    continue;
                }

                MonsterEntityBase entityBase = entity.Value.M_Curr as MonsterEntityBase;
                if (entityBase == null)
                {
                    continue;
                }

                if (!entityBase.IsBoss)
                {
                    continue;
                }

                float dis = Vector3.Distance(entityBase.Position(), mainPlayerPos);
                if (dis < min_dis)
                {
                    nPCEntityBase = entityBase;
                }
            }

            return nPCEntityBase;
        }


        /// <summary>
        /// 获取Entity配置ID
        /// </summary>
        /// <returns></returns>
        public uint GetEntityCfgID(ulong entityID)
        {
            var entity = GetEntityCtr(entityID);
            if (entity == null)
            {
                return 0;
            }

            uint ConfigIndex = 0;

            if (entity.EntityType == E_EntityType.Interact)
            {
                ObjectCtrlGroup ctrlGroup = entity as ObjectCtrlGroup;
                if (ctrlGroup != null)
                {
                    ConfigIndex = (uint)ctrlGroup.ConfigID;
                }
            }
            else
            {
                if (entity == null || entity.Data == null || entity.Data.Attrs == null)
                {
                    return 0;
                }

                ConfigIndex = entity.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
            }

            return ConfigIndex;
        }

        public E_EntityType GetEntityType(ulong entityID)
        {
            var entity = GetEntityCtr(entityID);
            if (entity == null)
            {
                return E_EntityType.None;
            }

            if (entity.EntityType == E_EntityType.Interact)
            {
                return E_EntityType.Interact;
            }
            else
            {
                if (entity == null || entity.Data == null || entity.Data.Attrs == null)
                {
                    return E_EntityType.None;
                }

                return entity.Data.EntityType;
            }
        }

        public void PlayMvpAnimByEntityID(ulong entityID)
        {
            EntityCtrlBase entityCtrlGroup = GetEntityCtr(entityID);
            if (entityCtrlGroup == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} PlayMvpAnimByEntityID() entityID={entityID},entityCtrlGroup=null err!!!");
                return;
            }
            entityCtrlGroup.ActionPlayMvpAnim?.Invoke();
        }

        /// <summary>
        /// 玩家是否处于交战状态
        /// </summary>
        /// <returns></returns>
        public bool IsPlayerInBattle()
        {
            return BattleManager.Instance.IsPlayerInBattle();
        }

        #region NPC,物件交互相关

        /// <summary>
        /// 判断组条件是否满足
        /// </summary>
        /// <param name="gourpid"></param>
        /// <returns></returns>
        public bool IsConditionMete(int gourpid, params object[] args)
        {
            //任何条件都满足
            if (gourpid == 0)
            {
                return true;
            }

            if (gourpid == -1)
            {
                return false;
            }

            return Service.Function.GlobalFunctionManager.Instance.ConditionGroupMete(mainPlayerId, gourpid, args);
        }


        /// <summary>
        /// 刷新选定NPC状态
        /// </summary>
        /// <param name="configIDs"></param>
        public void UpdateNpcServiceState(ServerServiceType serviceType, List<uint> configIDs)
        {
            ServerServiceManager.Instance.UpdateNpcServiceState(serviceType, configIDs);
            SetAllNpcIsVisiable();
        }

        /// <summary>
        /// 刷新所有NPC状态
        /// </summary>
        public void UpdateAllNpcServiceState()
        {
            ServerServiceManager.Instance.UpdateAllNpcServiceState();
            SetAllNpcIsVisiable();
        }

        /// <summary>
        /// 设置NPC显隐
        /// </summary>
        public void SetAllNpcIsVisiable()
        {
            foreach (var entity in m_mapEntityCtrlCopy)
            {
                if (entity.Value == null)
                {

                }
                if (entity.Value.EntityType == E_EntityType.Npc)
                {
                    uint npcBaseID = (entity.Value.M_Curr as GameNPCEntityBase).ConfigIndex;
                    //uint npcBaseID = GetNpcBaseID(entity.Key);
                    var npcCfg = LocalDataManager.Instance.GetNPCDataCell(npcBaseID);
                    if (npcCfg == null)
                    {
                        continue;
                    }

                    int showType = npcCfg.GetShowType();

                    //条件显示
                    if (showType == 0)
                    {
                        (entity.Value as GameNPCCtrlGroup).SetVisiable(IsConditionMete(npcCfg.GetShowCondition()));
                    }
                    //服务显示
                    else if (showType == 1)
                    {
                        (entity.Value as GameNPCCtrlGroup).SetVisiable(BusinessManager.Instance.IsNpcCanService(npcBaseID));
                    }
                    //条件或服务显示
                    else if (showType == 2)
                    {
                        bool isMete = IsConditionMete(npcCfg.GetShowCondition());
                        bool canSer = BusinessManager.Instance.IsNpcCanService(npcBaseID);
                        (entity.Value as GameNPCCtrlGroup).SetVisiable(isMete || canSer);
                    }
                }
                else if (entity.Value.EntityType == E_EntityType.Interact)
                {
                    int npcBaseID = (entity.Value as ObjectCtrlGroup).ConfigID;
                    //uint npcBaseID = GetNpcBaseID(entity.Key);
                    var npcCfg = LocalDataManager.Instance.GetInteractDataCell(npcBaseID);
                    if (npcCfg == null)
                    {
                        continue;
                    }

                    int showType = npcCfg.GetShowType();

                    //     (entity.Value as ObjectCtrlGroup).SetVisiable(IsConditionMete(npcCfg.GetShowCondition()));
                    //     (entity.Value as ObjectCtrlGroup).CheckEffect();

                    ObjectCtrlGroup objectCtrlGroup = entity.Value as ObjectCtrlGroup;
                    if (objectCtrlGroup != null)
                    {
                        bool hasservice = BusinessManager.Instance.IsObjectCanService((uint)npcBaseID);
                        //条件显示
                        if (showType == 0)
                        {
                            objectCtrlGroup.SetVisiable(IsConditionMete(npcCfg.GetShowCondition(), objectCtrlGroup.Index));
                        }
                        //服务显示
                        else if (showType == 1)
                        {
                            objectCtrlGroup.SetVisiable(hasservice);
                        }
                        //条件或服务显示
                        else if (showType == 2)
                        {
                            objectCtrlGroup.SetVisiable(IsConditionMete(npcCfg.GetShowCondition(), objectCtrlGroup.Index) || hasservice);
                        }

                        objectCtrlGroup.OnServiceChange(hasservice);
                        //
                    }
                }
            }
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
            GlobalEvent.RefreshAllNPCVisiableRet?.Invoke(null);
        }

        #endregion

        public void TriggerEvent(string eventType, object v)
        {
            GlobalEvent.AutoBattleEvent.Invoke(eventType, v);
        }

        public string GetTriggerEventStr(TriggerEventType eventType)
        {
            string key = "";
            switch (eventType)
            {
                case TriggerEventType.HeroDie:
                    {
                        key = "HeroDie";
                    }
                    break;
                case TriggerEventType.AOILeave:
                    {
                        key = "AOILeave";
                    }
                    break;
                case TriggerEventType.AOIEnter:
                    {
                        key = "AOIEnter";
                    }
                    break;
                case TriggerEventType.Ectype:
                    {
                        key = "Ectype";
                    }
                    break;
                case TriggerEventType.MapPreloadNotice:
                    {
                        key = "MapPreloadNotice";
                    }
                    break;
                case TriggerEventType.LeaveSpace:
                    {
                        key = "LeaveSpace";
                    }
                    break;
                case TriggerEventType.MapChangeRet:
                    {
                        key = "MapChangeRet";
                    }
                    break;
                case TriggerEventType.EctypeModuleGameEnd:
                    {
                        key = "EctypeModuleGameEnd";
                    }
                    break;
                case TriggerEventType.M_NoneActiveSkill:
                    {
                        key = "M_NoneActiveSkill";
                    }
                    break;
                case TriggerEventType.M_SkillEndCD:
                    {
                        key = "M_SkillEndCD";
                    }
                    break;
                case TriggerEventType.StartUserInput:
                    {
                        key = "StartUserInput";
                    }
                    break;
                case TriggerEventType.EndButtonCD:
                    {
                        key = "EndButtonCD";
                    }
                    break;

                default:
                    {
                        key = eventType.ToString();
                        SGF.Debuger.LogWarning($"[GameManager] TriggerEvent 缺少 {eventType} 的 toString 处理");
                    }
                    break;
            }

            return key;
        }

        public void TriggerEvent(TriggerEventType eventType, object v)
        {
            TriggerEvent(GetTriggerEventStr(eventType), v);
        }

        public void OpenAutoBattle()
        {
            if (!BattleManager.Instance.IsAutoBattling)
            {
                BattleManager.Instance.Start();
            }
        }

        public void OnLocalServerEvent(LocalServerEventRsp localServerEvent, object data)
        {
            switch (localServerEvent)
            {
                case LocalServerEventRsp.AOIMsg:
                    {
                        HandleAoiMsg((AOIMsg)data);
                    }
                    break;
                case LocalServerEventRsp.PropSyncList:
                    {
                        HandlePropSync((PropSyncList)data);
                    }
                    break;
                case LocalServerEventRsp.BulletCreateRet:
                case LocalServerEventRsp.BulletEndRet:
                case LocalServerEventRsp.RunStageRet:
                case LocalServerEventRsp.RunStageForceEndRet:
                    {
                        KeyValuePair<ulong, object> msgData = (KeyValuePair<ulong, object>)data;
                        MessageHandleData messageHandleData = new();
                        messageHandleData.messageName = Enum.GetName(typeof(LocalServerEventRsp), localServerEvent);

                        {
                            messageHandleData.enityId = msgData.Key;
                            messageHandleData.data = msgData.Value;
                            HandleRPCMsg(messageHandleData);
                        }
                    }
                    break;
                case LocalServerEventRsp.UpdateCusBlackBoard:
                    {
                        KeyValuePair<ulong, object> msgData = (KeyValuePair<ulong, object>)data;
                        // 处理自定义的 黑板数据同步
                        ulong enityId = msgData.Key;
                        EditorModeTest.CusBlackBoardData cusBlackBoardData = (EditorModeTest.CusBlackBoardData)msgData.Value;
                        OnUpdateCusBlackBoard(enityId, cusBlackBoardData);
                    }
                    break;
                default: break;
            }
        }

        public void OnUpdateCusBlackBoard(ulong enityId, EditorModeTest.CusBlackBoardData cusBlackBoardData)
        {
            EntityCtrlBase entityCtrlBase = GetEntityCtr(enityId);
            if (entityCtrlBase != null)
            {
                entityCtrlBase.ActionOnUpdateCusBlackBoard?.Invoke(cusBlackBoardData);
            }
        }


        #region 公告获取的接口

        public string GetAnnouncementID()
        {
            // int id = 999;
            // E_LoginCfgEnum e_LoginCfgEnum = SDKManager.Instance.GetLoginCfgEnum();
            // var cfg = LocalDataManager.Instance.GetNoticeDataCell((int)e_LoginCfgEnum);
            // if (cfg != null)
            // {
            //     id = cfg.NoticeSign;
            // }

            // return id;

            var id = SDKManager.Instance.GetChannelID();
            if (id != null)
            {
                return id;
            }
            return "999";
        }

        public void GetHttpAnnouncement(Action<Announcements> cb)
        {
            // #if STAR_DEV
            //             string url = $"https://xinghai.sanguosha.cn/static/{GetAnnouncementID()}.json";
            // #else
            //             string url = $"https://notice.zhoushanlianqing.com/static/{GetAnnouncementID()}.json";
            //             // string url = $"https://notice-sea.7eventya.com/static/{GetAnnouncementID()}.json";
            // #endif
            string url = $"https://notice.zhoushanlianqing.com/static/{GetAnnouncementID()}.json";

            // string url = $"https://xinghai.sanguosha.cn/static/11.json";

            HttpUtils.SendGetHttp<System.Object, Announcements>(url, null, cb);
        }

        #endregion

        public long GetEctypeLeftTime()
        {
            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.EctypeModule) as EctypeModule;
            if (module != null)
            {
                return (long)((module.EctypeEndTime - TimeUtils.ServerNowStampMilli) / 1000);
            }

            return 0;
        }

        public void SetPvpStatus(int status)
        {
            if (UIManager.Instance.M_Current_UIPage is StarProject.UI.StarWorld.StarWorldPage)
            {
                UI.StarWorld.StarWorldPage starWorldPage = (UI.StarWorld.StarWorldPage)UIManager.Instance.M_Current_UIPage;
                if (starWorldPage != null)
                {
                    starWorldPage.UpdatePvpSignStatus(status);
                }
            }
        }


        #region loading 压花之类的逻辑

        private bool isShowLoading = false;
        private LoadingWidgetTypeEnum curShowLoadingType;
        private bool isShowMsgBox = false;

        public void ShowMsgLoading(LoadingWidgetTypeEnum loadingWidgetTypeEnum)
        {
            isShowLoading = true;
            curShowLoadingType = loadingWidgetTypeEnum;
            SGF.Debuger.Log($"[MSG] [loading] 显示loading : {loadingWidgetTypeEnum}");
            UIAPI.ShowRequestLoading(40000, () =>
            {
                isShowLoading = false;
                curShowLoadingType = LoadingWidgetTypeEnum.Default;
            }, loadingWidgetTypeEnum);
        }

        /// <summary>
        /// 显示断网重连弹窗
        /// </summary>
        /// <param name="reConnect"></param>
        /// <param name="CallBack"></param> 
        /// <param name="isPingMsgBox">是否是ping 显示的 假断线重连弹窗, 如果是, 那就不做网络操作 ，只做个假的弹窗显示</param> 
        public void ShowReLoginMsgBox(bool reConnect, Action<string> CallBack, bool isPingMsgBox = false)
        {
            // 快速重连成功先不处理
            // 重连失败,删除其它的 所有实体

            isShowMsgBox = true;


            if (reConnect)
            {
                UIAPI.ShowMsgBoxNewAsync(48, (string v) =>
                {
                    isShowMsgBox = false;
                    CallBack(v);
                });
            }
            else
            {
                // TODO: DL
                // 等策划配置, 先用之前的 弹窗
                UIAPI.ShowMsgBoxNewAsync(55, (string v) =>
                {
                    isShowMsgBox = false;
                    CallBack(v);
                });
            }

            // 如果是 ping 显示的 假弹窗, 不关闭网络 和释放实体
            if (isPingMsgBox)
            {
                return;
            }
            // 强制关闭网络
            NetworkManager.Instance.Close();
            GameManager.Instance.ReleaseOtherEntity();
        }

        public void CloseNsgLoading()
        {
            if (!isShowLoading)
            {
                return;
            }

            isShowLoading = false;
            curShowLoadingType = LoadingWidgetTypeEnum.Default;
            UIAPI.CloseRequestLoading();
        }

        /// <summary>
        /// 关闭指定类型 的 widgetType
        /// </summary>
        /// <param name="loadingWidgetTypeEnum"></param>
        public void CloseWidgetTypeLoading(LoadingWidgetTypeEnum loadingWidgetTypeEnum)
        {
            if (!isShowLoading)
            {
                return;
            }

            bool result = UIAPI.CloseRequestLoading(loadingWidgetTypeEnum);
            if (result)
            {
                isShowLoading = false;
                curShowLoadingType = LoadingWidgetTypeEnum.Default;
            }
        }

        public void CloseMsgBox()
        {
            if (!isShowMsgBox)
            {
                return;
            }

            UIAPI.CloseMsgBoxNew(48);
        }

        #endregion

        #region 相机旋转

        private bool CfgIsCanRotateCamera = true;
        private bool SettingIsCanRotateCamera = true;

        public bool GetIsCanRotateCamera()
        {
            return CfgIsCanRotateCamera && SettingIsCanRotateCamera;
        }

        public void SetCfgRotateCameraState(bool isCan)
        {
            CfgIsCanRotateCamera = isCan;
            if (GetIsCanRotateCamera() == false)
            {
                CameraManager.Instance.CloseCameraRotate();
            }
        }

        public void SetRotateCameraState(bool isCan, bool isCache = true)
        {
            SettingIsCanRotateCamera = isCan;
            string key = $"{GameConfig.SETTING_CAMERA}{UserManager.Instance.MainUserData.playerRoleId}";
            SaveManager.Instance.Save(key, SettingIsCanRotateCamera, "Setting");
        }

        #endregion

        #region 注册 消息回复的监听

        /// <summary>
        /// 注册 的发送的 cmds. 一个sendCMD 可以对应多个 回复监听retCMDs: 1--->[1,1000];
        /// 如上, 发送 1， 成功回复1， 失败 回复 1000;
        /// 收到 1/1000 , 都需要 取消 1;
        /// 当 regSendCMDS 数量为0 时,取消 所有的 回复监听
        /// </summary> 
        Dictionary<int, long> regSendCMDs = new();

        /// <summary>
        /// 协议返回的监听.  一个协议返回的监听, 可能由多个 发送消息 来注册;
        /// 2 -----> [1002],  3---->[1002,3]
        /// 如上: 2 3 均返回 1002. 所以 回复 1002 由 [2,3] 注册. (这种情况一般不存在)
        /// 
        /// </summary>
        Dictionary<int, HashSet<int>> retListenCMDs = new();

        /// <summary>
        /// 取消所有的 msg 回复通知
        /// note:
        ///     不管是tcp还是websocket长连接, socket.send 只是将msg 推送到本地缓存区。
        ///     所以 对于客户端而言， 其实没办法确定 一个消息是否真的发送给服务器成功.
        ///     所以 对于一个消息如果设置了超时未回复 压花， 那再断线的时候， 就应该取消。
        ///     重连之后， 让这个按钮能够重新点击
        /// </summary>
        public void ClearMsgRetListen()
        {
            regSendCMDs.Clear();
            retListenCMDs.Clear();
        }

        /// <summary>
        /// 注册消息回复监听接口
        /// </summary>
        /// <param name="sendCMD"></param>
        /// <param name="retCMDS"></param>
        public void RegisterMsgRet(int sendCMD, List<int> retCMDS)
        {
            // 添加 对应的 消息注册记录
            if (!regSendCMDs.ContainsKey(sendCMD))
            {
                regSendCMDs.Add(sendCMD, TimeUtils.ClientNowStampMilli);
            }

            // 注册 返回的 消息的监听
            retCMDS.ForEach((retCMD) =>
            {
                if (!retListenCMDs.ContainsKey(retCMD))
                {
                    retListenCMDs.Add(retCMD, new HashSet<int>());
                }

                var sendCMDs = retListenCMDs[retCMD];
                if (!sendCMDs.Contains(sendCMD))
                {
                    sendCMDs.Add(sendCMD);
                }
            });
            SGF.Debuger.Log(
                $"[MSG] reg sendCMD: {sendCMD}, retCMDS: {retCMDS}, regSendCMDs: {regSendCMDs.Count}, retListenCMDs: {retListenCMDs.Count}");
        }

        /// <summary>
        /// 发送消息 发送异常后， 取消这个 sendCMD 对应的 注册记录.
        /// </summary>
        /// <param name="sendCMD"></param>
        public void OnSendMsgError(int sendCMD)
        {
            if (regSendCMDs.ContainsKey(sendCMD))
            {
                regSendCMDs.Remove(sendCMD);
            }
        }

        /// <summary>
        /// 收到 服务器返回的 retcmd后 去 解除 之前注册的消息监听
        /// </summary>
        /// <param name="retCmd"></param> 
        public void OnRetMsg(int retCmd)
        {
            if (regSendCMDs.Count == 0)
            {
                return;
            }

            if (retListenCMDs.Count == 0)
            {
                return;
            }

            if (!retListenCMDs.ContainsKey(retCmd))
            {
                return;
            }

            // 监听的 retCmd 对应的注册的 sendCMD
            var sendCMDs = retListenCMDs[retCmd];

            if (sendCMDs.Count == 0)
            {
                return;
            }

            // 发送的 消息，如果 在 regSendCMDs 中找到,就删除 监听
            sendCMDs.ForEach((sendCMD) =>
            {
                if (regSendCMDs.ContainsKey(sendCMD))
                {
                    regSendCMDs.Remove(sendCMD);
                }
            });

            sendCMDs.Clear();
            // 移除 retCmd 对应的 返回消息监听
            retListenCMDs.Remove(retCmd);

            // 如果 注册的 regSendCMDs 监听 被全部取消, 那 对返回cmd 的 retListenCMDs 也可以清空
            // note:
            //     由于 sendCMD 和 retCMD 构建的是 1-->N 的关系. 所以只要有一个 retCMD 返回, 就取消 sendCMD.
            //     所以会 存在 sendCMDs 清空, 但 regSendCMDs 还存在的清空.
            //     对于 是否 还存在 返回消息监听, 采用的是 regSendCMDs 是否还存在 进行判定
            if (regSendCMDs.Count == 0)
            {
                retListenCMDs.Clear();
            }

            SGF.Debuger.Log(
                $"[MSG] unreg retCmd: {retCmd}, regSendCMDs: {regSendCMDs}, retListenCMDs: {retListenCMDs.Count}");
        }


        /// <summary>
        /// 检查是否有 超时 未回复的 消息
        /// </summary>
        /// <returns></returns>
        public bool HasOverTimeMsgRet()
        {
            if (regSendCMDs.Count == 0)
            {
                return false;
            }

            var nowTime = TimeUtils.ClientNowStampMilli;
            foreach (var item in regSendCMDs)
            {
                // 超过 1s  这个消息都没返回
                if ((nowTime - item.Value) >= 1000f)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否需要显示 超时为回复 的 loading
        /// </summary>
        private bool IsShowOverTimeMsgRet()
        {
            // 如果已经显示了 laoding弹窗 或者 断线重连弹窗, 那就不用显示
            if (isShowLoading || isShowMsgBox)
            {
                return false;
            }

            if (HasOverTimeMsgRet())
            {
                return true;
            }

            return false;
        }

        private bool IsCloseOverTimeMsgRet()
        {
            if (isShowMsgBox || !isShowLoading)
            {
                return false;
            }

            // 如果当前显示的是 超时loading, 那就关闭
            if (isShowLoading && curShowLoadingType == LoadingWidgetTypeEnum.MsgOverTime && !HasOverTimeMsgRet())
            {
                return true;
            }

            return false;
        }

        private void HandleOverTimeMsgRet()
        {
            if (IsShowOverTimeMsgRet())
            {
                ShowMsgLoading(LoadingWidgetTypeEnum.MsgOverTime);
            }
            else if (IsCloseOverTimeMsgRet())
            {
                CloseWidgetTypeLoading(LoadingWidgetTypeEnum.MsgOverTime);
            }
        }


        public void TestRegMsgRet()
        {
            RegisterMsgRet(1, new List<int>() { 1, 1001 });
            RegisterMsgRet(2, new List<int>() { 2 });
            RegisterMsgRet(3, new List<int>() { 2 });
        }

        int i = -1;

        public void TestUnRegMsg()
        {
            i++;
            SGF.Debuger.Log($"[MSG] i: {i}");
            switch (i)
            {
                case 0:
                    {
                        TestRegMsgRet();
                    }
                    break;
                case 1:
                    {
                        OnRetMsg(1);
                    }
                    break;
                case 2:
                    {
                        OnRetMsg(1001);
                    }
                    break;
                case 3:
                    {
                        OnRetMsg(2);
                    }
                    break;
                case 4:
                    {
                        OnRetMsg(2);
                    }
                    break;
                case 5:
                    {
                        RegisterMsgRet(5, new List<int>() { 2 });
                    }
                    break;
                case 6:
                    {
                        OnRetMsg(2);
                    }
                    break;
                case 7:
                    {
                        TestRegMsgRet();
                        RegisterMsgRet(5, new List<int>() { 2 });
                    }
                    break;
                case 8:
                    {
                        OnRetMsg(2);
                    }
                    break;
                case 9:
                    {
                        OnRetMsg(1);
                    }
                    break;

                default:
                    i = -1;
                    break;
            }
        }

        #endregion

        #region act

        public void SetActRedpoint(bool show)
        {
            if (UIManager.Instance != null)
            {
                if (UIManager.Instance.M_Current_UIPage is StarProject.UI.StarWorld.StarWorldPage)
                {
                    StarProject.UI.StarWorld.StarWorldPage starWorldPage =
                        (StarProject.UI.StarWorld.StarWorldPage)UIManager.Instance.M_Current_UIPage;
                    if (starWorldPage != null)
                    {
                        starWorldPage.SetActRedpoint(show);
                    }
                }
            }
        }

        #endregion

        public bool IsPlayerInTeam(ulong entityID)
        {
            LuaModule luaModule = ModuleManager.Instance.GetModule(ModuleDef.Name.TeamModule) as LuaModule;
            if (luaModule != null)
            {
                TeamInfo teamInfo = luaModule.GetLuaTable().GetInPath<TeamInfo>("tinfo");
                if (teamInfo != null)
                {
                    var userInfos = teamInfo.UserInfos;
                    foreach (var item in userInfos)
                    {
                        if (item.Eid == entityID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsDailyCopyPassed(ulong id, ulong diff)
        {
            ulong GetFackerGenerateID(ulong high16, ulong low16)
            {
                return (high16 << 48) | (low16 << 16 >> 16);
            }

            var mgr = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.PersonDaily);
            if (mgr != null)
            {
                var pdinfoMDMgr = mgr as PDinfoMDMgr;
                if (pdinfoMDMgr != null)
                {
                    var pdInfo = pdinfoMDMgr.GetPDinfoMD();
                    if (pdInfo != null)
                    {
                        foreach (var item in pdInfo.InsData.DailyDatas)
                        {
                            if (item.Key == GetFackerGenerateID(id, diff))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        public bool IsBlackBoard(string key, int value)
        {
            EctypeModule luaModule = ModuleManager.Instance.GetModule(ModuleDef.Name.EctypeModule) as EctypeModule;
            if (luaModule != null)
            {
                Dictionary<string, int> blackBoardList = luaModule.EctypeTargetData.GetBlackBoardData(GameManager.Instance.mainPlayerId);
                if (blackBoardList != null)
                {
                    foreach (var item in blackBoardList)
                    {
                        if (item.Key == key && item.Value == value)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsTeamCopyPassed(int id)
        {
            var mgr = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.PersonDaily);
            if (mgr != null)
            {
                var pdinfoMDMgr = mgr as PDinfoMDMgr;
                if (pdinfoMDMgr != null)
                {
                    var pdInfo = pdinfoMDMgr.GetPDinfoMD();
                    if (pdInfo != null)
                    {
                        if (pdInfo.TeamDailyData.PassList.ContainsKey(id))
                        {
                            return pdInfo.TeamDailyData.PassList[id];
                        }
                    }
                }
            }

            return false;
        }

        public int GetSevenDayTaskMaxNum(int taskID)
        {
            if (TaskManager.TaskConfigs != null && TaskManager.TaskConfigs.list != null &&
            TaskManager.TaskConfigs.list.TryGetValue((uint)taskID, out Task.TaskConfigInfo info))
            {
                if (info != null)
                {
                    return info.TaskFinishInfo.Targets[0].Target.MaxNum;
                }
            }
            return 0;
        }

        #region 新手打点

        private List<string> m_ReportNewPlayerLogCache = new();
        private bool IsReportNewPlayerLog = false;
        private int m_SpecialFlagIndex = 1;
        public void SetReportNewPlayerLog(bool isReport)
        {
            IsReportNewPlayerLog = isReport;
        }

        public void EventPreNewPlayerEvent(string stepId)
        {
            if (!IsReportNewPlayerLog)
            {
                return;
            }

            if (stepId == "14_1000512_102")
            {
                stepId += $"_{m_SpecialFlagIndex}";
                m_SpecialFlagIndex++;
            }

            if (m_ReportNewPlayerLogCache.Contains(stepId))
            {
                SGF.Debuger.LogWarning($"[SDK] 新手上报 预处理事件打点 重复打点了 EventPreEvent: {stepId} ");
                return;
            }
            else
            {
                m_ReportNewPlayerLogCache.Add(stepId);
            }

            SGF.Debuger.Log($"[SDK] 新手上报 预处理事件打点 EventPreEvent: {stepId} ");

            SDKManager.Instance.EventPreNewPlayerEvent(stepId, 2);
        }

        #endregion

    }


    public interface IEffectDispatcher
    {
        ulong TargetID { get; set; }
    }
}
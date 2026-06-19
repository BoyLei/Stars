using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Module.StarWordGame;
using StarProject.Module.StarWordGame.Data;
using StarProject.Service.Battle;
using StarProject.Service.Business;
using StarProject.Service.Cam;
using StarProject.Service.Input;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    /// <summary>
    /// 这里是通知，GameManager以何种方式沟通大家干活的，这里是独特模式，当然还有其他的战场模式，副本模式
    /// 世界模块，这里作为Business，其实是最大的Business了
    /// </summary>
	public class StarWorldModule : BusinessModule
    {
        private string LOG_TAG = "[StarWorldModule]";

        private readonly string DelayCreateLoad = "DelayCreateLoad";

        //===================================
        /// <summary>
        /// 不设计成单例，希望切换战斗模式，切换副本，切换线路时可以重新生成战斗相关
        /// </summary>
        private VitalSignData mainPlayerVitalSignData = new();//Module一次
        private StarWorldGame m_game;
        private Action<StarWorldStartParam> onNotifyGameStart;
        public StarWorldStartParam m_StarWorldStartParam;
        private SocketBase M_GameSocket;
        public ulong ServerIDCache { get; private set; }
        public ulong SpaceIDCache { get; private set; }

        public ulong MapIDCache { get; private set; }

        private bool IsEnterSpace = false;
        private string OldMapName = "";
        public int OldMapID
        {
            get
            {
                if (oldenterSpaceNtf != null)
                {
                    return oldenterSpaceNtf.MapID;
                }
                return 0;
            }
        }
        ////////------------------------- 管理副本模块
        private Dictionary<SpaceType/*场景名称*/, ModuleDef.Name/*moduleName*/> Modules = new()
        {
            { SpaceType.SpaceScene, ModuleDef.Name.StarWorldModule },   //大世界
            { SpaceType.SpaceSercet, ModuleDef.Name.SecretAreaModule }, //个人秘境
            { SpaceType.SpaceMirror, ModuleDef.Name.EctypeModule },     //镜像副本
            { SpaceType.SpacePlot, ModuleDef.Name.EctypeModule },       //剧情副本
            { SpaceType.SpaceDaily, ModuleDef.Name.EctypeEntranceModule },
            { SpaceType.SpaceTeamDaily, ModuleDef.Name.DailyTeamModule },       //组队日常本
            { SpaceType.SpaceMirrorTreasure, ModuleDef.Name.TreasureModule },       //藏宝图
            { SpaceType.SpacePlotTreasure, ModuleDef.Name.TreasureModule },       //
            { SpaceType.SpacePersonTower, ModuleDef.Name.PersonalTowerModule },       //藏宝图                                                                     //
            { SpaceType.SpaceGng, ModuleDef.Name.AfternoonGveModule },       //午间GVE
            { SpaceType.SpaceWtask, ModuleDef.Name.WantedModule },       //通缉玩法
            { SpaceType.Space10V10, ModuleDef.Name.PvpModule },       //10v10
            { SpaceType.SpaceArena, ModuleDef.Name.ArenaModule },       //异步竞技场
            { SpaceType.SpaceGuildTerritory, ModuleDef.Name.EctypeModule },       //剧情副本
            //{ SpaceType.SpaceGuildTerritory, ModuleDef.Name.PartyTimeModule },       //公会篝火
        };
        private EnterSpaceNtf oldenterSpaceNtf;
        ////////------------------------- 管理副本模块

        public bool IsCurrentInFB
        {
            get
            {
                //服务器是时间戳一定大于100000：//服务器 已经处理了 没问题//是发送什么服务器的问题
                //SpaceID 是很小的（就等于Mapid），和很大的副本（时间戳）
                //Mapid 表格的定义 场景1~999，1000以上
                //Map_Name,PageName，是场景和ui
                if (m_StarWorldStartParam != null && m_StarWorldStartParam.gameParam != null && m_StarWorldStartParam.gameParam.mapData != null)
                {
                    return m_StarWorldStartParam.gameParam.mapData.SpaceId > 100000;
                }
                return false;
            }
        }

        // 当前地图分线服务器信息
        private ServerMapLoadInfo m_ServerMapLoadInfo = null;
        private ServerMapLoadInfo M_ServerMapLoadInfo
        {
            get
            {
                return m_ServerMapLoadInfo;
            }
            set
            {
                if (value != null)
                {
                    m_StarWorldStartParam.gameParam.mapData.ServerIDShow = value.LineID + 1;
                    GlobalEvent.onEnterSpaceEvent.Invoke(m_StarWorldStartParam.gameParam.mapData.ServerIDShow);
                }
                m_ServerMapLoadInfo = value;
                if (m_ServerMapLoadInfo != null)
                {
                    GlobalEvent.OnServerIDChange?.Invoke(GameManager.Instance.GetCurServerID(), m_ServerMapLoadInfo.ServerID);
                    GameManager.Instance.ChanageLineSetMapServerID(m_ServerMapLoadInfo.ServerID, m_ServerMapLoadInfo.LineID + 1);
                }
            }
        }

        #region 场景 和 角色 加载 检测 : 主动式

        private bool isMainUsrServerClientSuccess;
        public bool IsMainUsrServerClientSuccess
        {
            get { return isMainUsrServerClientSuccess; }
            private set
            {
                if (isMainUsrServerClientSuccess != value)
                {
                    isMainUsrServerClientSuccess = value;
                    if (isMainUsrServerClientSuccess)
                    {
                        CheckRoleNMApLoadState();
                    }
                }
            }
        }

        private bool isMapServerClientSuccess;
        public bool IsMapServerClientSuccess
        {
            get { return isMapServerClientSuccess; }
            private set
            {
                // //不同才会给，是真才通知
                if (isMapServerClientSuccess != value)
                {
                    isMapServerClientSuccess = value;
                    if (isMapServerClientSuccess)
                    {
                        CheckRoleNMApLoadState();
                    }
                }
            }
        }
        // 预加载地图
        private bool isMapPreloadNoticeSuccess;
        public bool IsMapPreloadNoticeSuccess
        {
            get { return isMapPreloadNoticeSuccess; }
            private set
            {
                // //不同才会给，是真才通知
                if (isMapPreloadNoticeSuccess != value)
                {
                    isMapPreloadNoticeSuccess = value;
                    if (isMapPreloadNoticeSuccess)
                    {
                        CheckRoleNMApLoadState();
                    }
                }
            }
        }

        // 预加载地图
        private bool isMapLoadSuccess;
        public bool IsMapLoadSuccess
        {
            get { return isMapLoadSuccess; }
            private set
            {
                // //不同才会给，是真才通知
                if (isMapLoadSuccess != value)
                {
                    isMapLoadSuccess = value;
                    if (isMapLoadSuccess)
                    {
                        CheckRoleNMApLoadState();
                    }
                }
            }
        }

        /// <summary>
        /// 读收到了就启动游戏了
        /// </summary>
        public void CheckRoleNMApLoadState()
        {
            if (isMapPreloadNoticeSuccess && isMainUsrServerClientSuccess && isMapServerClientSuccess && isMapLoadSuccess)
            {
                ////都成功后取缓存肯定拿得到
                onNotifyGameStart.Invoke(m_StarWorldStartParam);
            }
            if (AppConfig.IsDev())
            {
                if (!isMapPreloadNoticeSuccess)
                {
                    //DisplayProcessDispenser.Instance.AddSpecialMessage("进入场景-预加载地图消息-地图ID");
                    SGF.Debuger.LogWarning($"{LOG_TAG} CheckRoleNMApLoadState isMapPreloadNoticeSuccess is false");
                }
                if (!isMainUsrServerClientSuccess)
                {
                    //DisplayProcessDispenser.Instance.AddSpecialMessage("进入场景-等待主角信息");
                    SGF.Debuger.LogWarning($"{LOG_TAG} CheckRoleNMApLoadState isMainUsrServerClientSuccess is false");
                }
                if (!isMapServerClientSuccess)
                {
                    //DisplayProcessDispenser.Instance.AddSpecialMessage("进入场景-等待场景消息");
                    SGF.Debuger.LogWarning($"{LOG_TAG} CheckRoleNMApLoadState isMapServerClientSuccess is false");
                }
                if (!isMapLoadSuccess)
                {
                    //DisplayProcessDispenser.Instance.AddSpecialMessage("进入场景-加载场景消息");
                    SGF.Debuger.LogWarning($"{LOG_TAG} CheckRoleNMApLoadState isMapLoadSuccess is false");
                }
            }
        }
        #endregion

        private List<ModuleDef.Name/*ModuleName*/> NotifyModules = new()
        {
            ModuleDef.Name.TaskModule,
            ModuleDef.Name.SecretAreaModule,
            ModuleDef.Name.DailyTeamModule,
            ModuleDef.Name.AfternoonGveModule,
            ModuleDef.Name.WantedModule,
            ModuleDef.Name.ExchangeModule,
            ModuleDef.Name.FriendModule,
            ModuleDef.Name.PvpModule,
            ModuleDef.Name.ArenaModule,
            ModuleDef.Name.PersonalTowerModule,
            ModuleDef.Name.ActivityModule,
            ModuleDef.Name.PartyTimeModule,
            ModuleDef.Name.BeginnerTargetModule,
            ModuleDef.Name.SkillUnlockTipsModule,
            ModuleDef.Name.EctypeEntranceModule,
            ModuleDef.Name.TeamModule,
        };

        //========================================================
        public override void Create(object args = null)
        {
            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
            base.Create(args);

            SGF.Debuger.Log($"{LOG_TAG} Create------------------------------------start");

            M_GameSocket = NetworkManager.Instance.gameSocket;
            GlobalEvent.OnSceneLoadedBinded.AddListener(OnSceneLoadSuccess);
            //Reg Wait Start
            WaitOrLoadPanelToBegin((StarWorldStartParam)args);

            // 设置HUD中battleCamera的角度和坐标
            //UnityEngine.Vector3 localRotate = new(43f, 0, 0);
            //UnityEngine.Vector3 localPos = UnityEngine.Vector3.zero;
            //CameraManager.Instance.SetBattlePos(localRotate, localPos);

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                ulong entityID = GameLoginInfo.M_ChooseHeroReq.PID;
                // 测试 的 entityID  其实就等于 jobID
                uint jobId = (uint)entityID;

                {
                    MessageHandleData messageData = new();
                    messageData.data = EditorModeTest.EditorMode.Instance.localServer.mapPreloadNotice;

                    OnMapPreloadNoticeMsg(messageData);
                }

                {
                    MessageHandleData messageData = new();
                    messageData.data = EditorModeTest.EditorMode.Instance.localServer.enterSpaceNtf;

                    OnEnterSpaceDataNotify(messageData);
                }

                {
                    MessageHandleData messageData = new();

                    messageData.data = EditorModeTest.EditorMode.Instance.localServer.GetUserMainDataNotify(entityID, jobId);
                    GlobalEvent.OnClientReqLocalServerEvent?.Invoke(ClientEventReq.CreateMainPlayer, new object[] { messageData.data });
                    OnUserMainDataNotify(messageData);
                }

                // 刷新所有的 技能
                {
                    var allSkillNotice = EditorModeTest.EditorMode.Instance.localServer.GetAllSkillNotice((int)jobId);
                    GlobalEvent.OnRefreshAllSkillNoticeEvent.Invoke(allSkillNotice);
                }
                return;
            }
#endif


            //TODO：時效性問題，消息緩存，執行順序
            //先Bind，Custom，PB
            BindStarWorldMsg();

            CameraManager.Instance.SetPlayerCameraAngleY(0, true);
            CameraManager.Instance.SetPlayerCameraAngleX(GameConfig.MAX_ANGLE_OF_PITCH, true);

            SGF.Debuger.Log($"{LOG_TAG} Create------------------------------------end");
        }

        public void TestBackStarWorld()
        {
            ModuleManager.Instance.ReleaseModule(this);
        }

        private void Reset()
        {
            m_StarWorldStartParam = null;

            IsMainUsrServerClientSuccess = false;
            IsMapServerClientSuccess = false;
            IsMapPreloadNoticeSuccess = false;
            IsMapLoadSuccess = false;

            m_ServerMapLoadInfo = null;
            oldenterSpaceNtf = null;
            IsEnterSpace = false;
            ServerIDCache = 0;
            SpaceIDCache = 0;

            if (DelayInvoker.ContainInvoke(DelayCreateLoad))
            {
                DelayInvoker.CancelInvoke(DelayCreateLoad);
            }

            GlobalEvent.OnSceneLoadedBinded.RemoveListener(OnSceneLoadSuccess);
            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);
            onNotifyGameStart -= OnNotifyGameStartAction;

            if (M_GameSocket != null)
            {
                M_GameSocket.ReSetPing();
            }
        }

        public override void Release()
        {
            Reset();

            OffStarWorldMsg();

            if (m_game != null)
            {
                m_game.onGameEnd -= StopGame;
                //删除APP我卸载没意义，通常就是重启有意义
                m_game.GameRestart(); //GameExit();
            }

            //if (M_GameSocket != null)
            //{
            //    M_GameSocket.OnSocketStateChanged -= OnSocketStateChanged;
            //    M_GameSocket.OnSocketClose -= OnSocketClose;
            //    M_GameSocket.OnSendMsgAction -= OnSendMsgAction;
            //}

            base.Release();
        }

        /// <summary>
        /// 显示游戏世界模块的主UI
        /// </summary>=
        /// <param name="arg"></param>
        protected override void Show(object arg)
        {
            SGF.Debuger.Log($"{LOG_TAG} Show------------------------------------start");
            base.Show(arg);
            SGF.Debuger.Log($"{LOG_TAG} Show------------------------------------end");
        }

        private void OnSceneMapLoadComplete(int arg0)
        {
            //静态数据，之后要跟着任务改成动态数据|
            //静态的才在小地图里面显示----Doing
            UpdateCurrentMapStaticNPCData();

            //动态变化的NPC不会显示//
            //AOI会显示---（【不显示*其他人】/怪物） 会显示---------暂时先不做
            //怪物是静态配置，不过要根据aoi显示基于服务器数据，可能被干死了或者被别人引走了。
        }

        private void BindStarWorldMsg()
        {
            //PB------------------
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.BattleServerInfoAckID, onBattleServerInfoAck, this);.
            // 告知客户端预加载地图ID 提前加载场景22222
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.MapPreloadNoticeID, OnMapPreloadNoticeMsg, this);
            //场景注册！！！2 进入场景111111
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.EnterSpaceNtfID, OnEnterSpaceDataNotify, this);
            //客户端加载场景~~很快或者很慢：是不能缓存因为AOI在数据已经缓存了加载很快推出上一条不对了
            //人物注册！！！3       自己请求获取场景数据REq  ------------俩个服务器都发
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.UserMainDataNotifyID, OnUserMainDataNotify, this); // 全量同步主角的属性同步
            // 4.场景加载结束，可以开始接收处理其他场景的消息
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SpaceLoadEndNtfID, OnSpaceLoadEndNtf, this);
            // 5.服务器正常会主动发，如果没发，就在【SpaceLoadEndNtf】之后主动请求一下
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.ServerMapLoadInfoID, OnServerMapLoadInfo, this);
            // 监听服务器返回的所有分线信息
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.MapLinesRetID, OnMapLinesMsg, this);

            //对话通知
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.DialogNotifyID, OnDialogNotifyMsg, this);
            //服务器错误码
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.MsgRetID, MsgRetManager.Instance.OnMsgRet, this);
            // 离开场景
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.LeaveSpaceID, OnLeaveSpace, this);
            // 场景切换
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.MapChangeRetID, OnMapChangeMsg, this);
        }

        private void OffStarWorldMsg()
        {
            // 告知客户端预加载地图ID 提前加载场景22222
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.MapPreloadNoticeID, OnMapPreloadNoticeMsg, this);
            //场景注册！！！2 进入场景111111
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.EnterSpaceNtfID, OnEnterSpaceDataNotify, this);
            //客户端加载场景~~很快或者很慢：是不能缓存因为AOI在数据已经缓存了加载很快推出上一条不对了
            //人物注册！！！3       自己请求获取场景数据REq  ------------俩个服务器都发
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.UserMainDataNotifyID, OnUserMainDataNotify, this); // 全量同步主角的属性同步
            // 场景加载结束，可以开始接收处理其他场景的消息
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.SpaceLoadEndNtfID, OnSpaceLoadEndNtf, this);
            // 5.服务器正常会主动发，如果没发，就在【SpaceLoadEndNtf】之后主动请求一下
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.ServerMapLoadInfoID, OnServerMapLoadInfo, this);
            // 监听服务器返回的所有分线信息
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.MapLinesRetID, OnMapLinesMsg, this);
            //对话通知
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.DialogNotifyID, OnDialogNotifyMsg, this);
            //服务器错误码
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.MsgRetID, MsgRetManager.Instance.OnMsgRet, this);
            // 离开场景
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.LeaveSpaceID, OnLeaveSpace, this);
            // 场景切换
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.MapChangeRetID, OnMapChangeMsg, this);
        }

        private void OnDialogNotifyMsg(MessageHandleData data)
        {
            DialogNotify dialogNotify = (DialogNotify)data.data;

            ulong entityID = dialogNotify.EntityID;
            int dialogID = dialogNotify.DialogID;
            var cfg = LocalDataManager.Instance.GetCommonDialogDataCell(dialogID);
            if (cfg != null)
            {
                GameManager.Instance.EventPreNewPlayerEvent($"1_{cfg.GetDialog_type()}_{dialogID}_1");
            }

            ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnOpenAvgFromServer", new object[] { dialogID, entityID });
        }

        #region 管理副本模块

        private void OnCustomLevelSpace(EnterSpaceNtf newenterSpaceNtf)
        {
            if (oldenterSpaceNtf != null)
            {
                SGF.Debuger.Log($"切地图 旧的=>{oldenterSpaceNtf.SpaceID} 新的=>{newenterSpaceNtf.SpaceID}");
                if (oldenterSpaceNtf.SpaceID != newenterSpaceNtf.SpaceID || oldenterSpaceNtf.ServerID != newenterSpaceNtf.ServerID)
                {
                    if (Modules.TryGetValue(oldenterSpaceNtf.SpType, out var moduleName))
                    {
                        ModuleManager.Instance.SendMessage(moduleName, "OnCustomLevelEctype", null);
                    }
                }

                GlobalEvent.OnExitScene?.Invoke(oldenterSpaceNtf.MapID);
            }
            /*else
            {
                foreach (var module in Modules)
                {
                    ModuleManager.Instance.SendMessage(module.Value, "OnCustomLevelEctype", null);
                } 
            }*/
        }

        private void OnCustomEnterSpace(EnterSpaceNtf enterSpaceNtf)
        {
            if (Modules.TryGetValue(enterSpaceNtf.SpType, out var moduleName))
            {
                ModuleManager.Instance.SendMessage(moduleName, "OnCustomEnterEctype", null);
            }

            GlobalEvent.OnEntryScene?.Invoke(enterSpaceNtf.MapID);
            oldenterSpaceNtf = enterSpaceNtf;
        }

        #endregion

        public void OnCustomLevelEctype()
        {
            var mapType = GameManager.Instance.GetCurMapType();
            //公会领地不处理任务面板显影
            if (mapType != SpaceType.SpaceGuildTerritory)
            {
                GlobalEvent.OnSetTaskWidgetShow?.Invoke(false);
            }



            // 公会领地不处理任务面板显影
        }

        public void OnCustomEnterEctype()
        {
            var mapType = GameManager.Instance.GetCurMapType();
            //公会领地不处理任务面板显影
            if (mapType != SpaceType.SpaceGuildTerritory)
            {
                GlobalEvent.OnSetTaskWidgetShow?.Invoke(true);
            }
            else
            {
                GlobalEvent.OnSetTaskWidgetShow?.Invoke(false);
            }

            if (mapType == SpaceType.SpaceScene && BusinessManager.Instance.DigTreasureEndNtfCacheType > -1)
            {
                GlobalEvent.OnEndTreasure.Invoke(BusinessManager.Instance.DigTreasureEndNtfCacheType);
                BusinessManager.Instance.DigTreasureEndNtfCacheType = -1;
            }
        }
        /// <summary>
        ///  玩家进入场景回调
        /// </summary>
        /// <param name="data"></param>
        private void OnEnterSpaceDataNotify(MessageHandleData data)
        {
            SGF.Debuger.LogWarning($"初始登录 玩家进入场景回调----------------------------------------------------- 开始");
            //// 进入场景时，要删除其他实体
            //GameManager.Instance.ReleaseOtherEntity();
            //GameManager.Instance.ReleaseAllLocalEntity();

            EnterSpaceNtf enterSpace = (EnterSpaceNtf)data.data;
            // 管理副本模块的开启和关闭
            OnCustomLevelSpace(enterSpace);
            OnCustomEnterSpace(enterSpace);
            // BattleManager.Instance.SetCurAtkEntity(null);
            // 清空当前索敌队列
            BattleManager.Instance.ClearSerarchTargets();
            BattleManager.Instance.SetShowBossPanelEntity(null);
            BattleManager.Instance.CurCheckNPC = null;

            GameManager.Instance.TriggerEvent(TriggerEventType.Ectype, enterSpace);

            //副本还好 比如出生在比奇城，或者记录在沙巴克 或者魔兽世界的主城信息切换的情况
            //Server：1主城信息（看是几个），2坐标，3副本（客户端知道）							
            //服务器是一个地图，负载均衡，动态分线了
            m_StarWorldStartParam ??= new StarWorldStartParam(); //地图数据接入确保
            m_StarWorldStartParam.gameParam ??= new Game.Data.GameParam();//服务器数据应该是
            m_StarWorldStartParam.gameParam.mapData ??= new Game.Data.MapData();
            //场景SpaceId和SpaceId在大场景时一致；副本时候不一致
            //if (m_StarWorldStartParam.gameParam.mapData.MapID == enterSpace.MapID 
            //    || m_StarWorldStartParam.gameParam.mapData.SpaceType == enterSpace.SpType 
            //    || m_StarWorldStartParam.gameParam.mapData.SpaceId == enterSpace.SpaceID
            //    || m_StarWorldStartParam.gameParam.mapData.ServerID != enterSpace.ServerID
            //    )
            //{
            //    // 分线切换
            //    //ChanageLineSetMapData();
            //}
            m_StarWorldStartParam.gameParam.mapData.SpaceId = enterSpace.SpaceID;
            m_StarWorldStartParam.gameParam.mapData.MapID = enterSpace.MapID;// 地图ID
            m_StarWorldStartParam.gameParam.mapData.ServerID = enterSpace.ServerID; // 分线ID
            // 分线ID(2024/3/8 跟服务器优化分线逻辑，这里以后就不用了，但是协议里的字段先不删)
            //m_StarWorldStartParam.gameParam.mapData.ServerIDShow = enterSpace.Number + 1; 
            m_StarWorldStartParam.gameParam.mapData.SpaceType = enterSpace.SpType;
            m_StarWorldStartParam.gameParam.serverID = enterSpace.ServerID; // 分线ID

            ServerIDCache = enterSpace.ServerID;
            SpaceIDCache = enterSpace.SpaceID;

            GameManager.Instance.SetObstacleInfo(enterSpace.ObstacleInfo);

            //大场景弹出UI
            if (enterSpace.SpType == SpaceType.SpaceScene)
            {
                DelayInvoker.DelayInvoke(GameConfig.CHANAGE_MAP_DELAY_BLACKPANEL + 0.1f, OnDelayCreateShowAreaName);
            }

            //处理摄像机下的特效
            if (enterSpace.MapID == 10002 || enterSpace.MapID == 10003)
            {
                GameManager.Instance.SetBattleCameraEffect(true, "Effects/BattleSceen/Fx_FB_MengJKJ_camera_002");
            }
            else
            {
                GameManager.Instance.SetBattleCameraEffect(false);
            }

            mainPlayerVitalSignData.Pos = new UnityEngine.Vector3(enterSpace.StartPos.X, enterSpace.StartPos.Y, enterSpace.StartPos.Z);

            //SGF.Debuger.Log($"相机位置 切换地图 1111111111111 Pos={mainPlayerVitalSignData.Pos},Rot={enterSpace.StartRot}");

            // 无主角，等主角创建刷；有主角等场景信息额外刷一次
            if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            {
                //Debug.Break();
                (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup).EnterSpace(mainPlayerVitalSignData.Pos, enterSpace.StartRot);
            }

            IsEnterSpace = true;
            IsMapServerClientSuccess = true;
            OnClientReady();

            //GlobalEvent.onEnterSpaceEvent.Invoke(enterSpace.Number + 1);
            SGF.Debuger.LogWarning($"初始登录 玩家进入场景回调-----------------------------------{mainPlayerVitalSignData.Pos}------{enterSpace.StartRot}------------ 结束");

            //SGF.Debuger.Log($"相机位置 切换地图 222222222 Pos={mainPlayerVitalSignData.Pos},Rot={enterSpace.StartRot}");
        }

        // 延迟播放地图名
        private void OnDelayCreateShowAreaName(object[] args)
        {
            if (oldenterSpaceNtf == null)
            {
                return;
            }
            var mapCfgData = LocalDataManager.Instance.GetMapCfgData(oldenterSpaceNtf.MapID);
            if (mapCfgData != null)
            {
                if (OldMapName != mapCfgData.MapName)
                {
                    OldMapName = mapCfgData.MapName;
                    UIManager.Instance.OpenWidgetAsync(UIDef.ShowAreaNameWidget, null, false, new object[] { OldMapName, 1, mapCfgData.EnterTip }, null, StarProjectDef.MainPageCommond.HideNone, true, false);
                }
            }
        }

        private void OnLeaveSpace(MessageHandleData data)
        {
            IsEnterSpace = false;
            SGF.Debuger.LogWarning($"{LOG_TAG} OnLeaveSpace");
            // 无主角，等主角创建刷；有主角等场景信息额外刷一次
            if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            {
                (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup).LeaveSpace();
            }

            GlobalEvent.OnLeaveSpace?.Invoke(true);

            GameManager.Instance.TriggerEvent(TriggerEventType.LeaveSpace, true);

            BusinessManager.Instance.ClearTempBagItems();

            UIManager.Instance.CloseAllLoadedNoUIQueueType();
            UIQueueManager.Instance.DelAllSpecialUIQueue("Camera_", UISeatType.Full);
        }

        private void OnMapChangeMsg(MessageHandleData data)
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EventBossRankModule, "OnCloseBossRank", new object[] { });

            MapChangeRet mapChangeRetMsg = (MapChangeRet)data.data;
            SGF.Debuger.Log($"{LOG_TAG} OnMapLinesMsg  mapLinesRetMsg={mapChangeRetMsg}");

            if ((int)mapChangeRetMsg.RetValue < (int)ProtoMsg.MapChangeRet.Types.ChangeRet.FailLeaveNttNoExist)
            {
                if (mapChangeRetMsg.Reason == ChangeReason.ChangeLine)
                {
                    UIManager.Instance.CloseWidget(UIDef.ChanageLineListWidget, null, true);
                    ////isMapLoadSuccess = false;   // 切换地图的时候，吧开发设置为false
                    //GlobalEvent.OnServerIDChange?.Invoke(GameManager.Instance.GetCurServerID(), mapChangeRetMsg.ServerID);
                    //GameManager.Instance.ChanageLineSetMapData(mapChangeRetMsg.ServerID);

                    //Frame.Util.ShowSystemMessage(GameConfig.LocalStr["ChanageLineFinish"]);
                    Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("ChanageLineFinish"));
                }

                GameManager.Instance.TriggerEvent(TriggerEventType.MapChangeRet, mapChangeRetMsg);
                // 切换成功 --- 关闭界面
                //OnMapChangeMsg(mapChangeType, mapChangeRetMsg.MapID, mapChangeRetMsg.ServerID);
            }
            else
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} OnMapLinesMsg  mapLinesRetMsg={mapChangeRetMsg} chanage err");
                //DisplayProcessDispenser.Instance.AddSpecialMessage("进入副本返回类型" + mapChangeRetMsg.RetValue.ToString());
                //曲，错误码实现下
                //              const (
                ////进入成功，服务器和地图都成功切换到目标ID
                //MapChangeRet_OK_Common MapChangeRet_ChangeRet = 0
                ////进入成功，但是选择的线满员/有问题之类的，管理器自动分配另一条可进入的线
                //MapChangeRet_OK_ChangeLine MapChangeRet_ChangeRet = 1
                ////进入成功，其他
                //MapChangeRet_OK_Other MapChangeRet_ChangeRet = 10
                ////离开当前图失败，ntt不存在当前地图
                //MapChangeRet_Fail_Leave_NttNoExist MapChangeRet_ChangeRet = 11
                ////离开当前地图失败，目标服不存在;
                //MapChangeRet_Fail_Leave_SrcSrvNoExist MapChangeRet_ChangeRet = 12
                ////离开当前地图失败，目标地图不存在;
                //MapChangeRet_Fail_Leave_SrcMapNoExist MapChangeRet_ChangeRet = 13
                ////离开当前地图失败，目标服上不存在目标地图;
                //MapChangeRet_Fail_Leave_SrcSrvNoExistMap MapChangeRet_ChangeRet = 14
                ////离开当前地图失败，未知原因;
                //MapChangeRet_Fail_Leave_Unknown MapChangeRet_ChangeRet = 20
                ////进入目标地图失败，服务器满员
                //MapChangeRet_Fail_Enter_Full MapChangeRet_ChangeRet = 21
                ////进入失败，未知原因
                //MapChangeRet_Fail_Enter_MapNoExist MapChangeRet_ChangeRet = 30
            }
        }

        public void OnClientReady()
        {
            if (GameMap.SceneReady && GameMap.DataReady && IsEnterSpace)
            {
                ProtoMsg.ClientInsReadyReq client = new();
                SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, client, isAutoChangeMsgTarget: false);
                SGF.Debuger.LogWarning("OnClientReady" + System.DateTime.Now.ToString("yy-MM-dd hh:mm:ss:f"));

                DelayInvoker.DelayInvoke(DelayCreateLoad, 0, OnDelayCreateLoad);
            }
        }

        // 初始化场景通缉任务实体
        private void OnDelayCreateLoad(object[] args)
        {
            if (m_StarWorldStartParam == null)
            {
                return;
            }
            if (m_StarWorldStartParam.gameParam.mapData.SpaceType == SpaceType.SpaceScene)
            {
                GameManager.Instance.InitCreateWantedEntity();
            }
            GameManager.Instance.InitCreateSceneNPCEntity();
            GameManager.Instance.InitCreateSceneObjectEntity();
        }

        // 全量同步主角的属性信息
        private void OnUserMainDataNotify(MessageHandleData data)
        {
            SGF.Debuger.LogWarning($"初始登录 全量同步主角的属性信息----------------------------------------------------- 开始");
            GameManager.Instance.M_UserMainDataNotifyIsSuccess = false;

            // 主角全量同步属性
            UserMainDataNotify userMainDataNotify = (UserMainDataNotify)data.data;

            if (m_StarWorldStartParam == null)
            {
                m_StarWorldStartParam = new StarWorldStartParam();  //唯一一份，玩家数据，接入确保
            }
            if (m_StarWorldStartParam.gameParam == null)
            {
                m_StarWorldStartParam.gameParam = new Game.Data.GameParam();//服务器数据应该是
            }
            m_StarWorldStartParam.players.Clear();
            //自己模拟下主角信息：这里正常是开始游戏就会提前告诉我的玩家列表（目前只有玩家自己）：然后在通过第一次AOI
            //TODO这里要创建主角信息UserMainDataNotify
            mainPlayerVitalSignData.M_EntityID = userMainDataNotify.EntityID;
            // TODO：曲 这里主角信息里不给EntityType  默认给个player
            mainPlayerVitalSignData.EntityType = E_EntityType.Player;//Enum.GetName(E_EntityType.Player.GetType(), E_EntityType.Player);
            m_StarWorldStartParam.players.Add(mainPlayerVitalSignData);//主角也要加在里面
            m_StarWorldStartParam.MainPlayerEnityId = userMainDataNotify.EntityID;
            GameManager.Instance.MainPlayerEnityId = userMainDataNotify.EntityID;
            //mainPlayerVitalSignData.InitWithAttr(userMainDataNotify.Prop);
            //mainPlayerVitalSignData.InitWithAttr(userMainDataNotify);

            // 测试代码：发送重复属性同步，压力测试
            // int count = 1;
            // for (int i = 0; i < count; i++)
            // {
            // }
            mainPlayerVitalSignData.UpdateWithAttr(userMainDataNotify.Prop);

            // 副本里是死亡状态-》切换到大场景的时候，服务器会全量同步主角的状态是正常状态
            // 要刷新一次主角现在的公话状态
            if (!mainPlayerVitalSignData.IsDead)
            {
                if (GameManager.Instance.M_MainPlayerCtrlBase != null)
                {
                    (GameManager.Instance.M_MainPlayerCtrlBase.M_Curr as NPCEntityBase).SetMovmentForceState();
                }
            }

            mainPlayerVitalSignData.EntityType = E_EntityType.Player;
            mainPlayerVitalSignData.isMainPlayer = true;
            //功能属性
            //IsMainUsrServerClientSuccess = true;
            // TODO: 曲
            // 现在服务器会给发俩次UserMainDataNotify
            // 第一次会给player.json里面的属性，
            // 第二次会给battle.json里面的属性
            // 服务器会在俩个Server里面发协议，做不到在一个里面发
            // 目前客户端判断俩个json的关键属性做，是否切场景
            string name = mainPlayerVitalSignData.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.Name);
            uint jobId = mainPlayerVitalSignData.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            IsMainUsrServerClientSuccess = !string.IsNullOrEmpty(name) && jobId > 0;
            //IsMainUsrServerClientSuccess = !string.IsNullOrEmpty(name);
            SGF.Debuger.LogWarning($"{LOG_TAG} OnUserMainDataNotify name={name},jobId={jobId},IsMainUsrServerClientSuccess={IsMainUsrServerClientSuccess}");

            ulong sceneFlag = mainPlayerVitalSignData.Attrs.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.SceneFlag);
            GameManager.Instance.ChangeSceneFlag((uint)sceneFlag);

            GameManager.Instance.M_UserMainDataNotifyIsSuccess = true;

            //主角背包New全部去掉
            BusinessManager.Instance.UnNewAllItems();
            TaskHelper.DoCacheFindPath();
            GlobalEvent.OnMainPlayerIsReady?.Invoke(null);
            SGF.Debuger.LogWarning($"初始登录 全量同步主角的属性信息----------------------------------------------------- 结束");
        }

        private void OnSpaceLoadEndNtf(MessageHandleData data)
        {
            SGF.Debuger.LogWarning($"初始登录 登录流程 OnSpaceLoadEndNtf 结束-----------------------------------------------------");
            GameManager.Instance.IsLockGameMsgHandle = false;
            if (m_StarWorldStartParam != null && m_StarWorldStartParam.gameParam != null && m_StarWorldStartParam.gameParam.mapData != null)
            {
                if (m_StarWorldStartParam.gameParam.mapData.SpaceType == SpaceType.SpaceScene)
                {
                    if (m_ServerMapLoadInfo == null)
                    {
                        GetMapLinesReq();
                    }
                }
            }
            if (oldenterSpaceNtf != null && oldenterSpaceNtf.SpType != SpaceType.SpaceMirror && oldenterSpaceNtf.SpType != SpaceType.SpaceScene)
            {
                CameraManager.Instance.SetPlayerCameraAngleY(0);
                CameraManager.Instance.SetPlayerCameraAngleX(GameConfig.MAX_ANGLE_OF_PITCH);
            }
            GameManager.Instance.ClearClientBattleStates();

            //延迟刷交互物
            DG.Tweening.DOVirtual.DelayedCall(0.2f, () =>
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
            });

            //GameManager.Instance.Loading.OnProcessEnd();
        }

        private void OnServerMapLoadInfo(MessageHandleData data)
        {
            SGF.Debuger.LogWarning($"初始登录 登录流程 ServerMapLoadInfo 分线-----------------------------------------------------");
            M_ServerMapLoadInfo = (ServerMapLoadInfo)data.data;
        }

        private bool m_IsMapLinesReq = false;

        private void GetMapLinesReq()
        {
            if (m_IsMapLinesReq)
            {
                return;
            }
            /// 获取地图的全部分线信息
            MapLinesReq mapLinesMsg = new();
            mapLinesMsg.MapID = GameManager.Instance.GetCurMapId();
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, mapLinesMsg, false);
            m_IsMapLinesReq = true;
        }

        private void OnMapLinesMsg(MessageHandleData data)
        {
            if (!m_IsMapLinesReq)
            {
                return;
            }
            m_IsMapLinesReq = false;
            MapLinesRet mapLinesRetMsg = (MapLinesRet)data.data;
            //SGF.Debuger.LogWarning($"OnMapLinesMsg  mapLinesRetMsg={mapLinesRetMsg}");
            if (mapLinesRetMsg.ServerLines == null && mapLinesRetMsg.ServerLines.Count <= 0)
            {
                return;
            }

            if (m_StarWorldStartParam == null || m_StarWorldStartParam.gameParam == null || m_StarWorldStartParam.gameParam.mapData == null)
            {
                SGF.Debuger.LogWarning($"初始登录 OnMapLinesMsg() 缓存的地图信息都是空的err!!!");
                return;
            }

            for (int i = 0; i < mapLinesRetMsg.ServerLines.Count; i++)
            {
                ServerMapLoadInfo child = mapLinesRetMsg.ServerLines[i];
                if (child != null)
                {
                    if (child.ServerID == m_StarWorldStartParam.gameParam.mapData.ServerID && child.SpaceID == m_StarWorldStartParam.gameParam.mapData.SpaceId)
                    {
                        M_ServerMapLoadInfo = child;
                        return;
                    }
                }
            }
        }


        #region 小地图相关

        public Action<UnityEngine.Vector3> OnMainPlayerMoveAction;
        public Action<ulong, UnityEngine.Vector3> OnPvpPlayerMoveAction;
        public Action<ulong, int> OnPvpSetFaction;


        public Action<float> OnMainPlayerRotAction;
        public Action<ulong, UnityEngine.Vector3> OnMonsterMoveAction;  // 旧的怪物实体地图通知移动
        public Action<Dictionary<int, NPCJsonData>> OnNpcMiniShowDataChange;
        public Action<ulong, UnityEngine.Vector3> OnSpaceArenaMoveAction;  // 竞技场其他人的移动

        public void OnMainPlayerMove(UnityEngine.Vector3 Pos)
        {
            OnMainPlayerMoveAction?.Invoke(Pos);
        }

        public void OnSetPvpFaction(ulong eid, int faction)
        {
            OnPvpSetFaction?.Invoke(eid, faction);
        }

        public void OnPvpPlayerMove(ulong eid, UnityEngine.Vector3 Pos)
        {
            OnPvpPlayerMoveAction?.Invoke(eid, Pos);
        }

        public void OnMainPlayerRot(float rot)
        {
            OnMainPlayerRotAction?.Invoke(rot);
        }

        public void OnSpaceArenaMove(ulong eid, UnityEngine.Vector3 Pos)
        {
            OnSpaceArenaMoveAction?.Invoke(eid, Pos);
        }

        public void OnMonsterMove(ulong entityId, UnityEngine.Vector3 Pos)
        {
            // 旧的怪物实体地图通知移动
            OnMonsterMoveAction?.Invoke(entityId, Pos);
        }

        public void UpdateCurrentMapStaticNPCData()
        {
            ///当前场景的所有NPC数据
            // 小地图绑定的委托。直接绑定OnSceneMapConfigLoad这个回调就好了
            //OnNpcMiniShowDataChange?.Invoke(GameMap.sceneJsonData.Npcs);
        }

        #endregion

        /// <summary>
        /// 等待/可直接进入开始游戏
        /// 必备等待逻辑，房间逻辑，加载，匹配逻辑；这里直接自己调用
        /// 打开房间，显示等待UI，比如匹配，比如载入，加载，房间准备
        /// 但是这里直接开始：必要系统，和ui就不说了
        /// 也就是说，一种是按照 登录创建角色--主城镇---战斗---【各个模式+ui】
        /// 一种是               登录创建角色--世界---【各个模式+ui】：也要补充各种战斗模式
        /// 一种是               &&登录创建角色--大厅（分摊模式）---战斗---【各个模式+ui】
        /// </summary>
	    private void WaitOrLoadPanelToBegin(StarWorldStartParam arg)
        {
            //被通知游戏开始
            onNotifyGameStart += OnNotifyGameStartAction;

            //1，这里并不需要前置界面，前置逻辑操作逻辑：流程的分割需要前置步骤，是等待同步，通知的分段
            //2，也不需要其他功能的逻辑的等待，例其他玩家。所以不会有步骤分割
            //3，没有逻辑中断/分割的情况，客户端界面要快速加载新界面，后台进行Socket的链接
            ////[等待SocketSuccess完毕]
            //onNotifyGameStart.Invoke(arg);//自己模块组之内，用action是可以的，这里不等场景回调
            //显示等待UI，比如匹配，比如载入，加载，房间准备
        }

        private void OnNotifyGameStartAction(StarWorldStartParam param)
        {
            StartGame(param);
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// 开始游戏
        /// [StarWorldModule],[StarWorldGame],[GM.Inst.CreateGame]
        /// </summary>
        /// <param name="param"></param>
        private void StartGame(StarWorldStartParam param)
        {
            //新的场景，界面要清理下，并且设置主界面
            //流程解释：界面这里开V 对应 P
            //虽然手柄也是V层，但他属于 StartWorldGame，串联中转 * N *【所有Mgr和插件进行联通】：所以11界面这里开启，2手柄StartWorldGame.GameInput，3StartWorld是主动前置插件，GameManager理解为服务器响应后的后置处理
            //创建游戏逻辑
            m_game = new StarWorldGame();
            m_game.Start(param);

            //当游戏结束时
            m_game.onGameEnd += StopGame;
        }

        /// <summary>
        /// 停止游戏
        /// </summary>
        private void StopGame()
        {
            if (m_game != null)
            {
                m_game.Stop();
                m_game = null;
            }


            //返回上一个UI
            //UIManager.Instance.GoBackPage();
            //ModuleManager.Instance.SendMessage (ModuleDef.HostModule, "ReStart");
        }

        public StarWorldGame GetGame()
        {
            return m_game;
        }

        /// <summary>
        ///提前加载地图：这时候给不到我
        ///进入波纹副本，和提前进入场景都会给我
        ///先PerLoad（Mapid），后EnterSpace（SpaceId，MapID）
        /// </summary>
        /// <param name="data"></param>
        private void OnMapPreloadNoticeMsg(MessageHandleData data)
        {
            MapPreloadNotice MapPreloadNoticeMsg = (MapPreloadNotice)data.data;
            if (MapPreloadNoticeMsg == null)
            {
                return;
            }
            SGF.Debuger.LogWarning($"{LOG_TAG} 初始登录 OnMapPreloadNoticeMsg 【开始】  MapPreloadNoticeMsg={MapPreloadNoticeMsg}");
            if (MapPreloadNoticeMsg.MapID == 0)
            {
                return;
            }
            SGF.Debuger.LogWarning("初始登录 登录流程 不处理 HandleRPCMsg");
            GameManager.Instance.IsLockGameMsgHandle = true;
            GameManager.Instance.CacheMainPlayerPropSyncList.Clear();
            if (m_StarWorldStartParam == null)
            {
                m_StarWorldStartParam = new StarWorldStartParam();
            }
            if (m_StarWorldStartParam.gameParam == null)
            {
                m_StarWorldStartParam.gameParam = new Game.Data.GameParam();//服务器数据应该是
            }
            if (m_StarWorldStartParam.gameParam.mapData == null)
            {
                m_StarWorldStartParam.gameParam.mapData = new Game.Data.MapData();//地图数据接入确保
            }
            // 这里不能直接赋值，如果跟上一次的地图一样的话，就不切地图了
            //m_StarWorldStartParam.gameParam.mapData.MapID = MapPreloadNoticeMsg.MapID;
            //m_StarWorldStartParam.gameParam.mapData.SpaceType = MapPreloadNoticeMsg.SpType;
            //m_StarWorldStartParam.gameParam.mapData.SpaceId = (int)MapPreloadNoticeMsg.SpaceID;
            //GameManager.Instance.Loading.OnProcessStart();
            StarScenesManager.Instance.CurPartnerID = MapPreloadNoticeMsg.PartnerID;
            // 切地图前 通知 外面 地图变化
            GameManager.Instance.TriggerEvent(TriggerEventType.MapPreloadNotice, MapPreloadNoticeMsg);
            // 加载场景的时候吧移动按键都抬起来
            InputManager.Instance.ClearMoveCommand();
            //全部是Meek，所有切换场景流程不会导致ui强制切换，重复切换
            GameManager.Instance.CommonChangeMap(m_StarWorldStartParam.gameParam, MapPreloadNoticeMsg.MapID, MapPreloadNoticeMsg.SpType);
            //LoadMapJson(MapPreloadNoticeMsg.MapID);
            SGF.Debuger.LogWarning($"{LOG_TAG} 初始登录 OnMapPreloadNoticeMsg 【结束】  MapPreloadNoticeMsg={MapPreloadNoticeMsg}");
            M_ServerMapLoadInfo = null;
            IsMapPreloadNoticeSuccess = true;
            // 进入场景时，要删除其他实体
            GameManager.Instance.ReleaseOtherEntity();
            GameManager.Instance.ReleaseAllLocalEntity();
        }

        private void LoadMapJson(int mapID)
        {
            SpaceType curMapType = GameManager.Instance.GetCurMapType();
            if (curMapType == SpaceType.SpaceDefault || curMapType == SpaceType.SpaceScene || curMapType == SpaceType.SpaceMirror || curMapType == SpaceType.SpaceMirrorTreasure)
            {
                return;
            }
            string path = $"MapData/{mapID}/data";
            var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            SceneJsonData _sceneJsonData = null;
            if (jsonAsset != null)
            {
                _sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(jsonAsset.text);
            }
            if (_sceneJsonData == null)
            {
                StarDebug.LogError($" LoadMapJson:{path}  _sceneJsonData==null");
                return;
            }
            if (_sceneJsonData.Areas != null && _sceneJsonData.Areas.Count > 0)
            {
                foreach (var areaJson in _sceneJsonData.Areas)
                {
                    // 1 出生点
                    if (areaJson.Value.areaType == 2)
                    {

                        if (GameManager.Instance.M_MainPlayerCtrlBase != null)
                        {
                            //SGF.Debuger.Log($"相机位置 切换地图 333333333333333 Pos={areaJson.Value.Position.Convert()},Rot={GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ServerAngles}");
                            //Debug.Break();
                            (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup).EnterSpace(areaJson.Value.Position.Convert(), GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ServerAngles);
                            //SGF.Debuger.Log($"相机位置 切换地图 4444444444444 Pos={areaJson.Value.Position.Convert()},Rot={GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ServerAngles}");
                        }
                        break;
                    }
                }
            }
        }

        private void OnSceneLoadSuccess(string loadedName)
        {
            IsMapLoadSuccess = true;
        }

        /// <summary>
        /// 返回大场景
        /// </summary>
        public void BackToWorld()
        {
            LeaveInstanceReq mapChangeReqMsg = new();
            M_GameSocket.SendRPCMsg(ServerType.ServerTypeSpace, mapChangeReqMsg, false);
        }

        /// <summary>
        /// 换地图(大地图；副本，镜像副本；）换地图不会因为人多就换线
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mapID"></param>
        /// <param name="serverID"></param>
        public void SendFBChangeReq(ChangeReason type, int mapID, SpaceType spType, ulong serverID = 0, ulong spaceID = 0, bool isEngerGuildSpace = false)
        {
            MapChangeReq mapChangeReqMsg = new();
            mapChangeReqMsg.MapID = mapID;
            ulong ServerID = ServerIDCache;
            if (serverID != 0)
            {
                ServerID = serverID;
            }
            ulong SpaceID = SpaceIDCache;
            // 只切换地图，这里是不需要给服务器发spaceid，因为服务器也是不知道去那个space服的，只有请求的时候他才会分配，不考客户端传            
            // 只有原图切分线，这个值才有用
            if (spaceID != 0)
            {
                SpaceID = spaceID;
            }

            if (isEngerGuildSpace)
            {
                SpaceID = 0;
            }

            mapChangeReqMsg.ServerID = ServerID;
            mapChangeReqMsg.Reason = type;
            mapChangeReqMsg.SpType = spType;
            mapChangeReqMsg.SpaceID = SpaceID;
            M_GameSocket.SendRPCMsg(ServerType.ServerTypeLobby, mapChangeReqMsg, false);
        }

        public void OnMapChangeMsg(ChangeReason type, int mapID, ulong serverID)
        {
            m_StarWorldStartParam.gameParam.mapData.MapID = mapID;
            m_StarWorldStartParam.gameParam.serverID = serverID;

            //StarWorldPage starWorldPage = (StarWorldPage)UIManager.Instance.M_Current_UIPage;
            //starWorldPage.SetMapLineText(mapID, serverID);

            if (type == ChangeReason.ChangeLine)
            {
                // 原服务器切换分线，不切换地图
            }
            else
            {
                isMapLoadSuccess = false;   // 切换地图的时候，吧开发设置为false
                // 切换地图
                //UnityEngine.Vector3 startPos = UnityEngine.Vector3.zero;
                //string[] vec = mainPlayerVitalSignData.Pos.Split(',');
                //startPos.x = float.Parse(vec[0]);
                //startPos.y = float.Parse(vec[1]);
                //startPos.z = float.Parse(vec[2]);
                //// 切地图后，主角位置信息要更新
                //(GameManager.Instance.mainPlayerCtrlBase as PlayerCtrlGroup).SyncServerMoveTo(startPos, true); //切换地图强拉
            }
            // 目标ID要删除
            // BattleManager.Instance.SetCurAtkEntity(null);
            // 清空当前索敌队列
            BattleManager.Instance.ClearSerarchTargets();

            BattleManager.Instance.SetShowBossPanelEntity(null);
            BattleManager.Instance.CurCheckNPC = null;

            // AOI重新刷新 -- 服务器会发送AOI的消息，客户端不需要自己处理了
            GameManager.Instance.ReleaseOtherEntity();
            GameManager.Instance.ReleaseAllLocalEntity();
        }

        public void OnOpenPage(System.Func<ModuleDef.Name, Transform> func)
        {
            foreach (var item in NotifyModules)
            {
                ModuleManager.Instance.ShowModule(item, func(item));
            }
        }

        public void OnClosePage()
        {
            foreach (var item in NotifyModules)
            {
                //ModuleManager.Instance.ReleaseModule(item);
                ModuleManager.Instance.SendMessage(item, "OnDisable", null);
            }
        }

        // 打开小地图窗口
        internal void OpenFullMapWindow()
        {
            ModuleManager.Instance.ShowModule(ModuleDef.Name.WorldMapModule);
        }

#if UNITY_EDITOR
        public void ChangeModel()
        {
            m_game?.ChangeModel();
        }
#endif
    }


}

using SGF;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Game.StarsCamera;
using StarProject.Module.StarWordGame.Data;
using StarProject.Service.Battle;
using StarProject.Service.Input;
using StarProject.Service.Resource;
using StarProject.Service.Sound;
using StarProject.Service.Time;
using StarProject.UI.StarWorld;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Module.StarWordGame
{
    /// <summary>
    ///  游戏逻辑
    /// </summary>
	public class StarWorldGame
    {
        public string LOG_TAG = "StarWorldGame";
        //private FSPManager m_mgrFSP;
        //private SSPManager m_mgrSSP;

        private List<VitalSignData> m_listPlayerData;

        public event Action onMainPlayerDie;//true是死亡，
        public event Action onGameEnd;//退出游戏
        private GameContext m_context;

        //--------------------------------------------------
        private ulong mainPlayerId;
        private PlayerCtrlGroup m_MainPlayerCtrl;
        public byte FrameIndex = 0;//自己定义的（本秒数的帧）

        //------------------------------------


#if UNITY_EDITOR
        public static EntityBaseData g_vsd = null;
#endif

        /// <summary>
        /// 开始游戏：常规主城模式
        /// </summary>
        /// <param name="param"></param>
        public void Start(StarWorldStartParam param)
        {
            //Debuger.Log(LOG_TAG, "StartGame() param:{0}", param);
            //这里设计，服务器开始也可以传递一堆人进来，目前只用到一个人（主角）
            mainPlayerId = param.MainPlayerEnityId;
            VitalSignData csdCache = new();//缓存1次
            m_listPlayerData = param.players;
            for (int i = 0; i < m_listPlayerData.Count; i++)
            {
                VitalSignData playData = m_listPlayerData[i];

                if (playData.M_EntityID == mainPlayerId)
                {
                    playData.isMainPlayer = true;
                    playData.EntityType = E_EntityType.Player;
                    GameCamera.FocusPlayerId = mainPlayerId;
                    SoundManager.Instance.FocusPlayerId = mainPlayerId;
                    csdCache = playData;
                }
            }
            csdCache.IsFullMessage = true;
            //wwise曲
            //SoundManager.Instance.PlayEvent(AudioMethodEvent.MAIN_CITY_BGM);
            SoundManager.Instance.WWiseBgmStar();

            //启动游戏逻辑
            GameManager.Instance.CreateGame(param.gameParam);
            GameManager.Instance.onPlayerDie += OnPlayerDie;//有玩家死亡
            m_context = GameManager.Instance.Context;

            ////启动状态同步逻辑{FrameSyncP}+{*Lock锁帧驱动版}/{也可以有不锁帧}
            //m_mgrFSP = new FSPManager();
            //m_mgrFSP.Start(param.fspParam, m_mainPlayerId);
            //m_mgrFSP.SetFrameListener(OnEnterFrame);//中转，监听
            //m_mgrFSP.onGameBegin += OnGameBegin;//游戏开始
            //m_mgrFSP.onGameExit += OnGameExit;//有玩家退出
            //m_mgrFSP.onRoundEnd += OnRoundEnd;//有玩家退出
            //m_mgrFSP.onGameEnd += OnGameEnd;//游戏结束

            //Struct Sync
            //m_mgrSSP = new SSPManager();
            //m_mgrSSP.Start(param.sspParam, m_mainPlayerId);
            //m_mgrSSP.SetFrameListener(OnEnterFrame);//中转，监听

            ////初始化输入
            GameInput.Create();
            InputManager.Instance.OnVirtualInput += OnVKey;
            ////监听EnterFrame
            MonoHelper.AddFixedUpdateListener(FixedUpdate, MonoHelper.E_ModuleType.CommonService);
            MonoHelper.AddFixedUpdateListener(LaterFixUpdate, MonoHelper.E_ModuleType.CommonService, MonoHelper.E_Priority.Last);
            //TODO渲染和显示分离：帧
            ////////////---------- 主角操作
            // 创建主角
#if UNITY_EDITOR
            g_vsd = csdCache;
#endif
            GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Create, 0, csdCache);
            gameCommand.isServerAOI = false;
            gameCommand.isMainPlayer = true;
            GameManager.Instance.EntityDataCommand(gameCommand);
            SGF.Debuger.Log($"主角 创建成功!!!");
            // 缓存 【主角自己】 的 【玩家控制组】
            m_MainPlayerCtrl = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(mainPlayerId);
            // GameManager 缓存主角
            GameManager.Instance.M_MainPlayerCtrlBase = m_MainPlayerCtrl;
            // 临时处理
            GameManager.Instance.UseCacheMainPlayerPropSyncList();
            GameManager.Instance.UseCacheRPCMsgList();
            // 创建主角的UI信息挂件
            StarWorldPage starWorldPage = (StarWorldPage)UIManager.Instance.M_Current_UIPage;
            starWorldPage.CreateMainPlayerPanel();
            ////////////---------- 主角操作
            GlobalEvent.OnRoleCreateComplete?.Invoke(mainPlayerId);

            // 请求服务器时间
            TimeManager.Instance.RegReqServerTime();


            GameBegin();

            //Map加载成功->【Map配置加载成功】
            GlobalEvent.OnStarWorldSetUp?.Invoke(true);

            //通知药品绑定回调事件
            ModuleManager.Instance.SendMessage(ModuleDef.Name.MedicineModule, "OnRegisterAttribute", new object[] { m_MainPlayerCtrl });
            ModuleManager.Instance.SendMessage(ModuleDef.Name.SkillWindowModule, "OnRegisterAttribute", new object[] { m_MainPlayerCtrl });

            ModuleManager.Instance.SendMessage(ModuleDef.Name.SkillUnlockTipsModule, "OnStarWorldPageOpened", null);
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ActivityModule, "RefreshRedPoint", new object[] { });
        }

        /// <summary>
        /// 停止游戏 ，主动结束游戏
        /// 游戏流程的关闭主动关闭
        //按钮调用,或者消息主动调用
        /// </summary>
        public void Stop()
        {
            //Debuger.Log(LOG_TAG, "StopGame()");

            //GameManager.Instance.ReleaseGame();

            //MonoHelper.RemoveFixedUpdateListener(FixedUpdate);
            //GameInput.Release();

            //if (m_mgrFSP != null)
            //{
            //	m_mgrFSP.Stop();
            //	m_mgrFSP = null;
            //}

            //onMainPlayerDie = null;
            //onGameEnd = null;
            //         m_context = null;

            //TriggerEntityManager.Instance.OnEndGame();// 重启login时，APPMain只会执行一次，卸载收不到消息
            //LocalFxManager.Instance.OnEndGame();// 重启login时，APPMain只会执行一次，卸载收不到消息

            //关迷雾
            var rf = Service.UniversalRenderPipeline.UniRenderPipline.Instance.GetRenderFeature<ScreenCloudRenderPassFeature>();
            rf.SetActive(false);
        }

        /// <summary>
        /// 中止游戏,系统让你结束GameOver，App关闭这种
        /// 异常，或者被动
        /// </summary>
        public void GameExit()
        {
            Debuger.Log(LOG_TAG, "GameExit()");
            //因为PVP 模式中，还有其它玩家在玩，
            //所以应该只是让自己退出游戏
            //m_mgrFSP.SendGameExit();

            //TriggerEntityManager.Instance.OnEndGame();// 重启login时，APPMain只会执行一次，卸载收不到消息
            //LocalFxManager.Instance.OnEndGame();// 重启login时，APPMain只会执行一次，卸载收不到消息

            //关迷雾
            GameManager.Instance.SetFog(false); ;
        }

        /// <summary>
        /// 中止游戏,系统让你结束GameOver
        /// 异常，或者被动
        /// </summary>
        public void GameRestart()
        {
            Debuger.Log(LOG_TAG, "GameExit()");
            //因为PVP 模式中，还有其它玩家在玩，
            //所以应该只是让自己退出游戏
            //if (m_mgrFSP != null)
            //{
            //    m_mgrFSP.SendGameExit();
            //    m_mgrFSP.Stop();
            //    m_mgrFSP = null;
            //}
            // 请求服务器时间
            TimeManager.Instance.StopRegReqServerTime();

            MonoHelper.RemoveFixedUpdateListener(FixedUpdate, MonoHelper.E_ModuleType.CommonService);
            MonoHelper.RemoveFixedUpdateListener(LaterFixUpdate, MonoHelper.E_ModuleType.CommonService, MonoHelper.E_Priority.Last);
            InputManager.Instance.OnVirtualInput -= OnVKey;
            InputManager.Instance.M_MainPlayerCtrlBase = null;

            //TriggerEntityManager.Instance.OnEndGame();// 重启login时，APPMain只会执行一次，卸载收不到消息
            //LocalFxManager.Instance.OnEndGame();// 重启login时，APPMain只会执行一次，卸载收不到消息
            GameInput.Release();

            //关迷雾
            GameManager.Instance.SetFog(false);
            GameManager.Instance.ReleaseGame();
            // 清空伤害飘字节点
            DynamicUIRoot.ReleaseDamageRoot();

            onMainPlayerDie = null;
            onGameEnd = null;
            m_context = null;
            m_listPlayerData.Clear();
            mainPlayerId = 0;
            m_MainPlayerCtrl = null;

            //Release Init成对出现的， 这里释放但是appmain只有一次启动，不成对了
            //ResourceManager.Instance.Release();//Service的释放都放在这里，重启资源确实需要去掉很多资源都未必有效了
            StarProject.Service.Resource.ResourceFormalManager.Instance.GCResourceCache(ResourcePriority.Low);//都加载过了何必重新删除再加载：因为必然是高级的资源配置-->中级的场景-->低级需要释放就刷新一下不疼不养的释放就好了，就跟切换场景是一样的，游戏都是到主城，各个scene切换都要清理属于小的，这个到maintown也是清理，不必走大的卸载逻辑，1很可能没有二次启动时机，2重新加载的不必清理，3既然每次切换场景都要小刷一次这个也是一个【刷本质】也是清理场景，除非游戏退出那就全部卸载掉，那时候走大的真正的卸载逻辑【release】。

        }

        /// <summary>
        /// Start后续处理
        /// </summary>
        public void GameBegin()
        {
            Debuger.Log(LOG_TAG, "GameBegin()");
            //m_mgrFSP.SendGameBegin();
        }

        /// <summary>
        /// 被通知游戏开始，要回调要处理的
        /// </summary>
        /// <param name="arg"></param>
        private void OnGameBegin(int arg)
        {
            Debuger.Log(LOG_TAG, "OnGameBegin()");
        }

        /// <summary>
        /// 游戏时间结束-本地记录时间，或有玩家退出导致游戏结束
        /// </summary>
        //public void GameEnd()
        //{
        //	Debuger.Log(LOG_TAG, "GameEnd()");
        //	//m_mgrFSP.SendGameEnd();
        //}

        /// <summary>
        /// 创建玩家
        /// </summary>
        public void CreatePlayer()
        {
            Debuger.Log(LOG_TAG, "CreatePlayer()");
            //m_mgrSSP.SendFSP(GameVKey.CreatePlayer);
        }

        /// <summary>
        /// 重生玩家
        /// </summary>
        public void RebornPlayer()
        {
            Debuger.Log(LOG_TAG, "RebornPlayer()");

            //m_mgrFSP.SendFSP(GameVKey.CreatePlayer);
        }

        //--------------------------------------------------

        /// <summary>
        /// 来自GameInput的输入
        /// </summary>
        /// <param name="vkey"></param>
        /// <param name="arg"></param>
        private void OnVKey(int vkey, float arg)
        {
            //m_mgrFSP.SendFSP(vkey, (int)(arg * 10000));
            GameManager.Instance.InputVKey(vkey, arg, mainPlayerId);
        }

        /// <summary>
        /// 驱动游戏循环
        /// 逻辑帧是30FPS
        /// </summary>
        private void FixedUpdate()
        {
            //m_mgrFSP.EnterFrame();
            // -----TODO:自定义帧，应该放在守望帧同步中------------
            if (FrameIndex >= 31)
            {
                FrameIndex = 0;
            }
            else
            {
                FrameIndex++;
            }
            //SGF.Debuger.LogError($"死亡测试 FixedUpdate ------------------------------------------------------------ {FrameIndex}");

            OnEnterFrame(FrameIndex);
            // -----TODO:自定义帧，应该放在守望帧同步中------------
        }
        /// <summary>
        /// 驱动游戏循环
        /// 逻辑帧是30FPS
        /// </summary>
        private void LaterFixUpdate()
        {


            GameManager.Instance.EnterFixLaterFrame();

        }

        ///守望先锋利用帧同步，还原状态中时间的准确性
        private void OnEnterFrame(int frameId/*, FSPFrame frame*/)
        {
            GameManager.Instance.EnterFrame(frameId);
            BattleManager.Instance.EnterFrame(frameId);

            ClientNpc.ClientNpcManager.Instance.EnterFrame(frameId);
            //CheckTimeEnd();
        }
        private void OnLaterFrame(int frameId/*, FSPFrame frame*/)
        {

            //CheckTimeEnd();
        }
        /// <summary>
        /// 游戏已经经过多少时间
        /// </summary>
        /// <returns></returns>
        public int GetElapsedTime()
        {
            return (int)(m_context.currentFrameIndex * 0.033333333f);
            //return 0;//后加的
        }

        //--------------------------------------------------

        /// <summary>
        /// 有玩家死了
        /// </summary>
        /// <param name="playerId"></param>
        private void OnPlayerDie(ulong playerId)
        {
            //当死亡的自己时
            if (mainPlayerId == playerId)
            {
                if (onMainPlayerDie != null)
                {
                    onMainPlayerDie();

                }
                else
                {
                    this.LogError("OnPlayerDie() onMainPlayerDie == null!");
                }
                GlobalEvent.onMainPlayerDie.Invoke(true);//不用了-我自己写的预留的
            }
        }

        /// <summary>
        /// 当有玩家退出时
        /// </summary>
        /// <param name="playerId"></param>
        private void OnGameExit(uint playerId)
        {
            //当退出的是自己时
            if (mainPlayerId == playerId)
            {
                if (onGameEnd != null)
                {
                    onGameEnd();
                }
            }
        }

        /// <summary>
        ///  游戏结束回调
        /// </summary>
        /// <param name="arg"></param>
        private void OnGameEnd(int arg)
        {
            if (onGameEnd != null)
            {
                onGameEnd();
            }
        }

#if UNITY_EDITOR
        //切换模型
        public void ChangeModel()
        {
            m_MainPlayerCtrl = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(StarWorldGame.g_vsd.M_EntityID);
            (m_MainPlayerCtrl.M_Curr as NPCEntityBase).ActionOnCheckTargetMove += GameManager.Instance.OnActionOnCheckTargetMove;
            GameManager.Instance.M_MainPlayerCtrlBase = m_MainPlayerCtrl;
            GlobalEvent.OnRoleCreateComplete?.Invoke(mainPlayerId);

            m_MainPlayerCtrl?.RefreshSkill();
        }
#endif
    }
}

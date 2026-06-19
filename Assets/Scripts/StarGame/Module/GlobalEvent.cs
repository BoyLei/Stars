using ProtoMsg;
using SGF.Module.Framework;
using SkillEditor;
using StarProject.Game.Entity.VitalSigns;
using StarProjectDef;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using XLua;

namespace StarProject
{

    /// <summary>
    /// [全局事件]
    /// 使用原则：如只局限于Business就用Busi面的事件
    /// 如多个模块都需要监听就用GlobalEvent
    /// 另外核心逻辑在Service时只能用GlobalEvent
    /// [使用原则说明]
    /// --如果参数是值的话就用固定的ModuleEvent<int>
    /// 如果是类就用Class实现映射ModuleEvent<Object> 别强用结构体
    /// 因为接口一点不消耗，但是装拆箱会涉及到堆内存拷贝
    /// --多注意别人已经定义过的事件看是否能共用
    ///    如果别人需要的参数少而你多需要一个参数你就给他多增加一个参以形成公用
    /// [原则]
    /// 当难以衡量传参消耗，和事件公用的情况时；就如同审核代码优雅和性能，请教主程
    /// 
    /// 有些事件不确定应该是由谁发出
    /// 就可以通过全局事件来收和发
    /// 事件挂了后面要有保护，上线的时候，Action改成自己的指针list的TryCache；另外线程报错也要拦截主界面的报错
    /// </summary>
    [LuaCallCSharp]
    [CSharpCallLua]
    public static class GlobalEvent
    {
        #region SceneManager_Event
        /// <summary>
        /// 场景切换开始
        /// </summary>
        public static ModuleEvent onSceneBeginChange = new ModuleEvent();

        /// <summary>
        /// 场景切换完成:
        /// </summary>
        public static ModuleEvent<string, bool> onSceneLoaded = new ModuleEvent<string, bool>();

        /// <summary>
        /// 场景绑定成功
        /// </summary>
        public static ModuleEvent<string> OnSceneLoadedBinded = new ModuleEvent<string>();

        #endregion

        /// <summary>
        /// true:登录成功,false：登录失败,或者掉线
        /// </summary>
        public static ModuleEvent<bool> onLogin = new ModuleEvent<bool>();

        /// <summary>
        /// EnterSpace 事件 int:分线显示ID
        /// </summary>
        public static ModuleEvent<int> onEnterSpaceEvent = new ModuleEvent<int>();

        /// <summary>
        /// 伤害的黑板数据发出
        /// </summary>
        public static ModuleEvent<object> onBlackBoardInfo = new ModuleEvent<object>();

        /// <summary>
        /// buff伤害的黑板数据发出
        /// </summary>
        public static ModuleEvent<object> onBuffBlackBoardInfo = new ModuleEvent<object>();

        /// <summary>
        /// skill伤害的黑板数据发出
        /// </summary>
        public static ModuleEvent<object> onSkillBlackBoardInfo = new ModuleEvent<object>();

        /// <summary>
        /// Bullet黑板数据发出
        /// </summary>
        public static ModuleEvent<object> onBulletBoardInfo = new ModuleEvent<object>();

        /// <summary>
        /// 被动技能黑板数据发出
        /// </summary>
        public static ModuleEvent<ProtoMsg.RunStageRet> onPassiveSkillBoardInfo = new ModuleEvent<ProtoMsg.RunStageRet>();

        /// <summary>
        /// 没有本地记录运行时黑板数据发出
        /// </summary>
        public static ModuleEvent<ProtoMsg.RunStageRet> onNoRecordRunBlack = new ModuleEvent<ProtoMsg.RunStageRet>();


        /// <summary>
        /// 敌人信息发出
        /// </summary>
        public static ModuleEvent<NPCEntityBase> onEnemyInfo = new ModuleEvent<NPCEntityBase>();

        /// <summary>
        /// 主角死亡消息
        /// </summary>
        public static ModuleEvent<bool> onMainPlayerDie = new ModuleEvent<bool>();

        public static ModuleEvent<ulong> onPlayerShowHPUI = new ModuleEvent<ulong>();

        public static ModuleEvent<bool> onPlayerInfoWidgetShow = new ModuleEvent<bool>();

        public static ModuleEvent<bool> onSetRotateCamera = new ModuleEvent<bool>();

        /// <summary>
        /// 主角的战斗状态切换事件
        /// </summary>
        public static ModuleEvent<E_PlayerStateForMusic> onMainPlayerChanageBattleState = new ModuleEvent<E_PlayerStateForMusic>();


        #region 不确定,也可能来源于模块,管理器,特殊类型；确定时会具体到Mgr就直接调用了
        //游戏重新开始
        public static ModuleEvent<bool> OnRestar = new ModuleEvent<bool>();


        //游戏UI清理
        public static ModuleEvent<bool> OnUIClear = new ModuleEvent<bool>();

        //游戏场景清理,切换,开启本地地图但是进入副本
        public static ModuleEvent<bool> OnFbOrSceneChange = new ModuleEvent<bool>();
        #endregion

        #region 客户端模拟创建实体

        // 创建本地实体[类型（1宝箱2传送门）、交互物件ID、实体来源key、坐标、奖励、是否同类型全部一起打开、缩放值]
        public static ModuleEvent<int, int, string, UnityEngine.Vector3, LocalDropList, bool, float> onCreateLocalEntity = new ModuleEvent<int, int, string, UnityEngine.Vector3, LocalDropList, bool, float>();
        //public static ModuleEvent<> onCreateLocalEntity = new ModuleEvent<int, UnityEngine.Vector3, int, string>();
        // 点击本地实体[类型、交互物件ID、实体来源key、交互物件特殊key]
        public static ModuleEvent<int, long, string, string> onClickLocalEntity = new ModuleEvent<int, long, string, string>();


        // 通知挑战通缉实体 分线id、地图id、点位id、点位类型
        public static ModuleEvent<ulong, ulong, int, int> onChallengeWantedEntity = new ModuleEvent<ulong, ulong, int, int>();

        // 刷新单个通缉实体信息
        public static ModuleEvent<int, WTaskPointTarInfo> onRefreshWTaskPointTarInfo = new ModuleEvent<int, WTaskPointTarInfo>();

        #endregion

        #region 交互物件
        public static ModuleEvent<int, ulong> OnTriggerEvent = new ModuleEvent<int, ulong>();

        public static ModuleEvent<int, ulong> OnExitEvent = new ModuleEvent<int, ulong>();

        public static ModuleEvent OnRoleCreateComplete = new ModuleEvent();
        /// <summary>
        /// 交互事件   物件UID,到期时间
        /// </summary>
        public static ModuleEvent<ulong, long> InterEvent = new ModuleEvent<ulong, long>();
        /// <summary>
        /// 交互事件   显示的文字,到期时间
        /// </summary>
        public static ModuleEvent<string, int, UnityAction, UnityAction> InterEventByStr = new ModuleEvent<string, int, UnityAction, UnityAction>();


        /// <summary>
        /// 采集和制造停止
        /// </summary>
        public static ModuleEvent<int/*1 采集  2制造*/, long/*交互物ID 或 配方ID*/, bool/* true 开始 false 结束*/> OnStopIner = new ModuleEvent<int, long, bool>();

        public static ModuleEvent<int, long, bool> OnGratherEnergy = new();

        /// <summary>
        /// 采集选中事件
        /// </summary>
        public static ModuleEvent<int, int> OnGratherSelectChange = new();
        #endregion

        #region 常驻提示信息

        public static ModuleEvent<string> OnHitStarEvent = new ModuleEvent<string>();

        public static ModuleEvent OnHitEndEvent = new ModuleEvent();



        #endregion

        #region 摄像机效果 < cameraEffectID , cameraType, pos>
        public static ModuleEvent<int, int, E_CameraEffectType, UnityEngine.Vector3> OnCameraEvent = new ModuleEvent<int, int, E_CameraEffectType, UnityEngine.Vector3>();
        public static ModuleEvent<int, CameraEvent, E_CameraType> OnCameraStopEvent = new ModuleEvent<int, CameraEvent, E_CameraType>();
        public static ModuleEvent<E_CameraType> OnCameraAwake = new ModuleEvent<E_CameraType>();
        /// <summary>
        /// <duration ,itemTime, vector3(x,y,z) >
        /// </summary>
        public static ModuleEvent<int, float, float, UnityEngine.Vector3> OnCameraShakeEvent = new ModuleEvent<int, float, float, UnityEngine.Vector3>();

        public static ModuleEvent<int> OnCameraStopShakeEvent = new ModuleEvent<int>();


        #endregion

        #region 屏幕效果事件
        public static ModuleEvent<string, bool> PreLoadAVEvent = new ModuleEvent<string, bool>();
        public static ModuleEvent<E_AVType, string> PlayAVEvent = new ModuleEvent<E_AVType, string>();
        public static ModuleEvent<E_AVType> PauseAVEvent = new ModuleEvent<E_AVType>();

        public static ModuleEvent<E_AVType> StopAVEvent = new ModuleEvent<E_AVType>();

        #endregion

        #region 技能相关事件
        public static ModuleEvent<ProtoMsg.AllSkillNotice> OnAllSkillNoticeEvent = new ModuleEvent<ProtoMsg.AllSkillNotice>();

        public static ModuleEvent<ProtoMsg.SwitchSkillPosRet> OnSwitchSkillPosRetEvent = new ModuleEvent<ProtoMsg.SwitchSkillPosRet>();

        public static ModuleEvent<ProtoMsg.SwitchTalentRet> OnSwitchTalentRetEvent = new ModuleEvent<ProtoMsg.SwitchTalentRet>();

        public static ModuleEvent<ProtoMsg.CDUpdateNotice> OnCDUpdateNotice = new ModuleEvent<ProtoMsg.CDUpdateNotice>();

        #endregion


        #region 怪物超过1个屏幕在俩个屏幕之间
        /// <summary> 目标箭头显示 </summary>
        public static ModuleEvent<bool> onTargetArrowShow = new ModuleEvent<bool>();
        /// <summary> 目标箭头显示 </summary>
        public static ModuleEvent<UnityEngine.Vector3> onTargetArrowPos = new ModuleEvent<UnityEngine.Vector3>();

        #endregion

        public static ModuleEvent<object> onLuaUIShow = new ModuleEvent<object>();
        public static ModuleEvent<object> onLuaUIHide = new ModuleEvent<object>();

        public static ModuleEvent<bool, CameraShakeJson> OnVirtualCameraShakeEvent = new ModuleEvent<bool, CameraShakeJson>();

        /// <summary>
        /// 摄像机震动
        /// </summary>
        public static ModuleEvent<ulong/*对象ID*/, string/*名称*/, bool> OnCameraImpluseEvent = new ModuleEvent<ulong, string, bool>();

        /// <summary>
        /// 摄像机偏移
        /// </summary>
        public static ModuleEvent<ulong/*对象ID*/, string/*名称*/> OnCameraOffsetEvent = new ModuleEvent<ulong, string>();

        public static ModuleEvent<ulong/*对象ID*/, GlobalShowGlobal_CameraMove /*摄像机移动配置*/, bool /*是进入还是退出*/> OnCameraMoveEvent = new ModuleEvent<ulong, GlobalShowGlobal_CameraMove, bool>();

        /// <summary>
        /// 虚拟相机创建完成
        /// </summary>
        public static ModuleEvent OnVirtualCameraCreate = new ModuleEvent();
        /// <summary>
        /// 修改摄像机高度
        /// </summary>
        public static ModuleEvent<float> OnModifyCameraHeight = new ModuleEvent<float>();

        /// <summary>
        /// 选中任务目标
        /// </summary>
        public static ModuleEvent<int> SelectTaskItem = new ModuleEvent<int>();

        //任务状态变化
        public static ModuleEvent<int, int> TaskStageChange = new ModuleEvent<int, int>();

        public static ModuleEvent<int> OnClickTaskItem = new ModuleEvent<int>();

        public static ModuleEvent<int> TaskScrollRectChange = new ModuleEvent<int>();
        /// <summary>
        /// 展示任务
        /// </summary>
        public static ModuleEvent<object> ShowTaskInfo = new ModuleEvent<object>();

        /// <summary>
        /// 大转盘结束
        /// </summary>
        public static ModuleEvent OnFinishRingTaskRandom = new ModuleEvent();

        /// <summary>
        /// 显示任务info面板
        /// </summary>
        public static ModuleEvent<bool> ShowTaskInfoPanel = new ModuleEvent<bool>();


        /// <summary>
        /// 场景配置文件加载完毕
        /// </summary>
        public static ModuleEvent<int> OnSceneMapConfigLoad = new ModuleEvent<int>();

        /// <summary>
        /// 切场景主角准备好
        /// </summary>
        public static ModuleEvent OnMainPlayerIsReady = new ModuleEvent();

        //Loading状态
        public static ModuleEvent<bool> IsBeginLoadingOrEnd = new ModuleEvent<bool>();

        /// <summary>
        /// 任务配置重新加载
        /// </summary>
        public static ModuleEvent OnTaskConfigLoad = new ModuleEvent();

        /// <summary>
        /// 效果结束（效果ID）
        /// </summary>
        public static ModuleEvent<int> OnEffectEnd = new ModuleEvent<int>();

        /// <summary>
        /// 选择EffectID 效果 ，如对话选项
        /// </summary>
        public static ModuleEvent<int, int> OnSelectEffect = new ModuleEvent<int, int>();

        #region 伙伴相关的event
        public static ModuleEvent<StarProject.Game.Player.EntityCtrlBase> OnPartnerCreate = new ModuleEvent<StarProject.Game.Player.EntityCtrlBase>();
        public static ModuleEvent<StarProject.Game.Player.EntityCtrlBase> OnPartnerLeave = new ModuleEvent<StarProject.Game.Player.EntityCtrlBase>();
        public static ModuleEvent<StarProject.Game.Player.EntityCtrlBase, int> OnPartnerUpdate = new ModuleEvent<StarProject.Game.Player.EntityCtrlBase, int>();
        /// <summary>
        /// 伙伴 技能cd 通知
        /// </summary>
        public static ModuleEvent<int> OnPartnerCDUpdateNotice = new ModuleEvent<int>();

        /// <summary>
        /// 当伙伴列表发生变化的 同步事件
        /// </summary>
        /// <returns></returns>
        public static ModuleEvent<object> OnPartnerListUpdate = new ModuleEvent<object>();

        public static ModuleEvent<ProtoMsg.PartnerBattleRet> OnMsgPartnerBattleRet = new ModuleEvent<ProtoMsg.PartnerBattleRet>();
        public static ModuleEvent<ProtoMsg.PartnerAssistRet> OnMsgPartnerAssistRet = new ModuleEvent<ProtoMsg.PartnerAssistRet>();

        public static ModuleEvent<ProtoMsg.PartnerFallRet> OnMsgPartnerFallRet = new ModuleEvent<ProtoMsg.PartnerFallRet>();
        /// <summary>
        /// 出战的回复通知
        /// </summary>
        /// <typeparam name="ProtoMsg.PartnerConcretizeRet">出战的协议</typeparam>
        /// <typeparam name="int">下位的伙伴id</typeparam>
        /// <returns></returns>
        public static ModuleEvent<ProtoMsg.PartnerConcretizeRet, long> OnMsgPartnerConcretizeRet = new ModuleEvent<ProtoMsg.PartnerConcretizeRet, long>();
        public static ModuleEvent<ProtoMsg.PartnerSwitchEndTimeNtf> OnPartnerSwitchEndTimeNtf = new();

        public static ModuleEvent<ProtoMsg.PartnerSelectCDRet> OnMsgPartnerSelectCDRet = new ModuleEvent<ProtoMsg.PartnerSelectCDRet>();


        /// <summary>
        /// 伙伴血量刷新
        /// </summary>
        /// <typeparam name="uint">伙伴id</typeparam>
        /// <typeparam name="long">伙伴血量</typeparam>
        /// <typeparam name="long">伙伴满血量</typeparam>
        /// <returns></returns>
        public static ModuleEvent<uint, long, long> OnPartnerBloodUpdate = new ModuleEvent<uint, long, long>();

        /// <summary>
        /// 伙伴实体创建成功
        /// </summary>
        public static ModuleEvent<ulong> OnPartnerEntityCreateFinish = new ModuleEvent<ulong>();


        /// <summary>
        /// 伙伴死亡状态更新
        ///  <typeparam name="uint">伙伴id</typeparam>
        ///  <typeparam name="bool">死亡状态</typeparam>
        /// </summary>
        public static ModuleEvent<ulong, bool> OnPartnerDeadUpdate = new ModuleEvent<ulong, bool>();

        /// <summary>
        /// 伙伴技能CD刷新
        /// </summary>
        /// <typeparam name="uint">伙伴id</typeparam>
        /// <typeparam name="long">剩余时间</typeparam>
        /// <typeparam name="long">最大cd</typeparam>
        /// <returns></returns>
        //public static ModuleEvent<ulong, float, float> OnPartnerSkillCDUpdate = new ModuleEvent<ulong, float, float>();


        #endregion

        //副本任务目标同步
        public static ModuleEvent<EctypeData> OnEctypeTargetDataChange = new ModuleEvent<EctypeData>();


        public static ModuleEvent<bool> OnLeaveSpace = new ModuleEvent<bool>();



        // 副本目标通知 除大场景、秘境、默认类型，其他副本都需要副本目标
        public static ModuleEvent<bool> OnEntityEctype = new ModuleEvent<bool>();

        // 设置任务挂件的显示（只有大场景才打开）
        public static ModuleEvent<bool> OnSetTaskWidgetShow = new ModuleEvent<bool>();

        // 设置活动时间挂件的时间（只有大场景才打开）
        public static ModuleEvent<int, int, long, long, long> OnSetEventTime = new ModuleEvent<int, int, long, long, long>();

        public static ModuleEvent<bool> OnRefeshShopBtn = new ModuleEvent<bool>();

        //进出公会区域回调
        public static ModuleEvent<bool> OnEnterPartyArea = new ModuleEvent<bool>();

        //GVEBoss奖励刷新
        public static ModuleEvent<bool> OnClearGVEBossReward = new ModuleEvent<bool>();
        //GNGBoss奖励刷新
        public static ModuleEvent<bool> OnClearGNGBossReward = new ModuleEvent<bool>();


        public static ModuleEvent<long> OnFreshExitTime = new ModuleEvent<long>();

        // 开启临时背包 //  按副本类型走【个人日常】目前只有日常副本使用临时背包
        public static ModuleEvent<bool> OnOpenEctypeBag = new ModuleEvent<bool>();

        //伙伴升级成功
        public static ModuleEvent<int> OnPartnerUpgrade = new ModuleEvent<int>();

        //是否刷新货币栏
        public static ModuleEvent<bool> OnRefeshCurrencyBar = new ModuleEvent<bool>();

        //任务NPC状态变化
        public static ModuleEvent<long/*NPCID*/, uint /*任务id*/, int/*NPC类型*/, TaskNpc.StateEnum> OnTaskNpcChange = new ModuleEvent<long, uint, int, TaskNpc.StateEnum>();

        // 任务交互物件状态变化
        public static ModuleEvent<long/*NPCID*/, uint /*任务id*/, int/*NPC类型*/, TaskNpc.StateEnum> OnTaskInterChange = new ModuleEvent<long, uint, int, TaskNpc.StateEnum>();

        //Map配置加载成功
        public static ModuleEvent<string> OnMapConfigLoaded = new ModuleEvent<string>();

        //StarWorld启动成功
        public static ModuleEvent<bool> OnStarWorldSetUp = new ModuleEvent<bool>();

        //伙伴刷新
        public static ModuleEvent<int> OnPartnerFresh = new ModuleEvent<int>();



        //伙伴突破成功
        public static ModuleEvent<bool> OnPartnerTopped = new ModuleEvent<bool>();

        //伙伴升星（觉醒）成功
        public static ModuleEvent<bool> OnPartnerUpStar = new ModuleEvent<bool>();

        //伙伴出战 上阵，替换
        public static ModuleEvent<bool> OnPartnerBattleRet = new ModuleEvent<bool>();

        //伙伴出战/助战 下阵
        public static ModuleEvent<bool> OnPartnerFallRet = new ModuleEvent<bool>();

        //伙伴助战 上阵，替换
        public static ModuleEvent<bool> OnPartnerAssistRet = new ModuleEvent<bool>();

        // 通知刷新所有NPC显隐
        public static ModuleEvent RefreshAllNPCVisiableRet = new ModuleEvent();
        // 通知NPC显隐 配置表ID，坐标，显隐
        public static ModuleEvent<int, UnityEngine.Vector3, bool> NPCVisiableRet = new ModuleEvent<int, UnityEngine.Vector3, bool>();


        //伙伴升级
        public static ModuleEvent<int/*ID*/, int/*等级*/> OnPartnerUpLevel = new ModuleEvent<int, int>();

        //MachineQualityLevel Change
        public static ModuleEvent<int> OnQualChangeTinyModuleReflesh = new ModuleEvent<int>();

        #region 角色属性面板与装备栏

        // 点击装备
        public static ModuleEvent<bool> OnEquipClickEvent = new ModuleEvent<bool>();


        #endregion

        #region 聊天

        // 主角遥感控制HUD聊天显示时事件
        public static ModuleEvent<bool> OnChatHudShowEvent = new ModuleEvent<bool>();
        // 主界面点击打开了道具Tips
        public static ModuleEvent OnChatHudShowTipsEvent = new ModuleEvent();
        // 道具Tips关闭回调
        public static ModuleEvent OnTipsCloseEvent = new ModuleEvent();

        //道具变化
        public static ModuleEvent<long/*配置表ID*/, long/*数量*/> OnItemChange = new ModuleEvent<long, long>();
        public static ModuleEvent OnItemFirstInit = new ModuleEvent();
        public static ModuleEvent<ulong/*实体ID*/> OnHaveNewEquip = new ModuleEvent<ulong>();
        public static ModuleEvent OnItemChangeToRefeshBag = new ModuleEvent();//道具更新刷新背包
        #endregion


        #region UI主Page事件

        /// <summary>
        /// 主PAGE加载完成
        /// </summary>
        public static ModuleEvent<bool> OnMainPageLoadedComplete = new ModuleEvent<bool>();

        #endregion

        #region 属性变化广播
        public static ModuleEvent<ulong, string, object> OnPropChange = new ModuleEvent<ulong, string, object>();

        #endregion

        #region 个人秘境

        public static ModuleEvent<int, int> OnPersonSecretAwardRet = new ModuleEvent<int, int>();

        public static ModuleEvent<object> OnPersonSecretDailyTaskRet = new ModuleEvent<object>();


        public static ModuleEvent<object> OnPersonSecretAchieveChange = new ModuleEvent<object>();

        // 秘境复活
        public static ModuleEvent<bool> OnPersonSecretRevive = new ModuleEvent<bool>();

        // 秘境boss出现
        public static ModuleEvent<bool> OnPersonSecretBossAppearRet = new ModuleEvent<bool>();


        #endregion

        #region 邮件
        //刷新邮件
        public static ModuleEvent<object> OnFreshMail = new ModuleEvent<object>();
        public static ModuleEvent<ulong> OnDeleteMail = new ModuleEvent<ulong>();
        #endregion

        #region 组队日常本
        //进入副本入口
        public static ModuleEvent<int> OnEnterEntryScene = new ModuleEvent<int>();

        public static ModuleEvent<object> OnShowDailyTeamItem = new ModuleEvent<object>();

        public static ModuleEvent OnDailyTeamGameEnd = new ModuleEvent();
        #endregion

        #region  战斗需要监听的 event

        public static ModuleEvent<string, object> AutoBattleEvent = new();


        #endregion

        #region  藏宝图

        public static ModuleEvent<int> OnCraftingTreasure = new ModuleEvent<int>();

        public static ModuleEvent<int> OnEndTreasure = new ModuleEvent<int>();
        #endregion

        #region GVEBoss小地图
        public static ModuleEvent<int> OnGVEBossRefesh = new ModuleEvent<int>();
        #endregion

        #region 单人副本小地图
        public static ModuleEvent<int> OnMiniMapShowEctypeTask = new ModuleEvent<int>();
        #endregion

        #region 切换摄像机默认参数的事件
        public static ModuleEvent<object> OnSwitchBattleCameraDefaultParam = new();

        public static ModuleEvent<int, int> OnActiveSceneVirtualCamera = new ModuleEvent<int, int>();

        #endregion

        #region Timeline 事件

        //timeline 开始隐藏HUD界面事件
        public static ModuleEvent<bool> OnPlayTimelineEvent = new ModuleEvent<bool>();

        /// <summary>
        /// Timeline 事件
        /// </summary>
        public static ModuleEvent<string, PlayableDirector, TimelineEventData,float> OnTimelineEvent = new ModuleEvent<string, PlayableDirector, TimelineEventData,float>();

        /// <summary>
        /// bool true 开始  false 结束
        /// </summary>
        public static ModuleEvent<int, bool> OnTimelinePlayStateChangeEvent = new ModuleEvent<int, bool>();
        #endregion

        #region 临时显隐
        public static ModuleEvent<E_EntityType, int, bool> OnEntityTempVisibityChange = new ModuleEvent<E_EntityType, int, bool>();
        #endregion


        // #if UNITY_EDITOR
        #region  离线技能编辑器 的相关协议
        /// <summary>
        /// 刷新所有 技能的 协议
        /// </summary>
        /// <typeparam name="ProtoMsg.AllSkillNotice"></typeparam>
        /// <returns></returns>
        public static ModuleEvent<ProtoMsg.AllSkillNotice> OnRefreshAllSkillNoticeEvent = new ModuleEvent<ProtoMsg.AllSkillNotice>();

        /// <summary>
        /// 本地服 推送的 事件
        /// </summary>
        /// <returns></returns>
        public static ModuleEvent<LocalServerEventRsp, object> OnLocalServerEvent = new();

        /// <summary>
        /// 客户端 请求本地服 做的 一些事
        /// </summary>
        /// <returns></returns>
        public static ModuleEvent<ClientEventReq, object[]> OnClientReqLocalServerEvent = new();

        #endregion
        // #endif

        #region 系统开放 相关的 事件
        /// <summary>
        /// 系统开放 数据初始化
        /// </summary>
        public static ModuleEvent<object> OnSystemOpenInit = new();
        /// <summary>
        /// 单个系统 类型开放的通知. 目前策划要求 多个系统同时开放时 需要一个个 显示.
        /// </summary>
        public static ModuleEvent<SystemOpenType, bool> OnSystemOpen = new();
        public static ModuleEvent<SystemOpenType> OnNewSystemOpen = new();

        /// <summary>
        /// 通知 所有 系统开放组件 刷新的 通知. 玩家数据的 初始化的时候， 节点 可能已经在 OnEnable 中刷新了一次，但是 缺少数据。
        /// 所以需要在 数据 完全初始化后，再触发 刷新一次 
        /// </summary>
        public static ModuleEvent<int> OnRefreshSystemOpen = new();

        /// <summary>
        /// 系统开放 拍脸图 动画结束后，飞行到指定icon 的动画事件
        /// </summary>
        public static ModuleEvent<SystemOpenType, UnityEngine.Vector3> OnSystemOpenTipFly = new();

        #endregion

        #region 主角 相关的 全局事件通知
        /// <summary>
        /// 玩家 身上数据变化 的 通知
        /// </summary>
        public static ModuleEvent<string, object> OnPlayerDataChange = new();

        /// <summary>
        /// 主角创建的 事件通知
        /// </summary>
        public static ModuleEvent<int> OnMainPlayerCreate = new();

        #endregion

        public static ModuleEvent<object> TestEvent = new ModuleEvent<object>();

        /// <summary>
        /// 队伍信息变化
        /// </summary>
        public static ModuleEvent OnTeamInfoChange = new ModuleEvent();

        /// <summary>
        /// 返回登录
        /// </summary>
        public static ModuleEvent<AgainLoginType> OnBackLogin = new();
        #region 引导

        public static ModuleEvent<string> OnOpenUI = new();

        public static ModuleEvent<int> OnFinishTutorial = new();

        /// <summary>
        /// 引导接收事件
        /// </summary>
        public static ModuleEvent<E_EventDefine, string> OnReciveEvent = new();

        public static ModuleEvent<string> OnClickScreen = new();

        public static ModuleEvent<int> OnEntryScene = new();


        public static ModuleEvent<int> OnExitScene = new();

        #endregion

        #region pvp
        public static ModuleEvent<bool> OnShowPvpWidget = new();
        #endregion



        #region  sdk 的通知事件
        public static ModuleEvent<string, object> OnSDKEvent = new();
        #endregion

        /// <summary>
        /// 战斗状态变化
        /// </summary>
        public static ModuleEvent<bool> OnBattleStateChange = new();

        /// <summary>
        /// 停止制造
        /// </summary>
        public static ModuleEvent<int> OnStopCreate = new();

        /// <summary>
        /// 地图矿变化
        /// </summary>
        public static ModuleEvent<int> OnMapMineChange = new();

        public static ModuleEvent<bool> OnGameNoneInputChange = new();

        /// <summary>
        /// socket 验证结果
        /// </summary>
        public static ModuleEvent<bool> OnSocketVerifyResult = new();

        /// <summary>
        /// mapId 切换. <oldMapId , newMapId>
        /// </summary>
        public static ModuleEvent<int, int> OnMapChange = new();
        /// <summary>
        /// scene 切换, <oldScene , newScene>
        /// </summary>
        public static ModuleEvent<string, string> OnDiffMap_SceneChange = new();
        /// <summary>
        /// serverID 切换, <oldServerID , newServerID>
        /// </summary>
        public static ModuleEvent<ulong, ulong> OnServerIDChange = new();

        public static ModuleEvent<GameObject> SettoTutorial = new();

        /// <summary>
        /// 播放波纹特效事件
        /// </summary>
        public static ModuleEvent<bool> OnPlayWaveEffect = new();
        /// <summary>
        /// 个人爬塔
        /// </summary>
        public static ModuleEvent<int> OnPersonTowerReward = new();
        public static ModuleEvent<int> OnPersonTowerTargetChanged = new();

        /// <summary>
        /// 刷新服务器实体显隐
        /// </summary>
        public static ModuleEvent<int> OnFreshServerEntityVisiable = new();

        /// <summary>
        /// 伙伴引导提示
        /// </summary>
        public static ModuleEvent<int> OnShowGuidancePartner = new();

        /// <summary>
        /// 主线任务引导提示
        /// </summary>
        public static ModuleEvent<int> OnShowGuidanceMainTask = new();

        public static ModuleEvent<int> OnShowGuidancePartnerClean = new();
        /// <summary>
        /// 客户端战斗状态变化
        /// </summary>
        public static ModuleEvent<bool> OnClientBattleStateChange = new();

        /// <summary>
        /// 新手关显示UI
        /// </summary>
        public static ModuleEvent OnXinSGUIShow = new();
        /// <summary>
        /// 伙伴喊话
        /// </summary>
        public static ModuleEvent<int, int, int> OnPartnerDialog = new ModuleEvent<int, int, int>();

        /// <summary>
        /// 场景玩法UI显示
        /// </summary>
        public static ModuleEvent<int, int, int> OnShowScenePlay = new();
        /// <summary>
        /// 新手目标领奖
        /// </summary>
        public static ModuleEvent<int> OnBeginnerRefresh = new();
        /// <summary>
        /// 新手目标领奖红点
        /// </summary>
        public static ModuleEvent<int> OnBeginnerRefreshRedDot = new();
        /// <summary>
        /// 初始化任务hud页签
        /// </summary>
        public static ModuleEvent<int> OnRefreshHudTaskIndex = new();
        /// <summary>
        /// 心灵修复小游戏结果事件
        /// </summary>
        public static ModuleEvent<int> OnMindRepairResultEvent = new ModuleEvent<int>();

        /// <summary>
        /// loading 界面的 开始 和 结束事件
        /// </summary>
        /// <typeparam name="bool"></typeparam>
        public static ModuleEvent<bool> OnLoadingViewEvent = new ModuleEvent<bool>();
        /// <summary>
        /// 
        /// </summary>
        public static ModuleEvent<int> OnNpcCreateFinished = new();
        
        
        /// <summary>
        /// 状态变化事件
        /// </summary>
        public static ModuleEvent<int> OnNpcInterStageChange = new();

    }
}
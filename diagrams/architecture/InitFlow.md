# 初始化全链路

> Level 3 — 从 Prepare 场景启动到登录完成的完整初始化流程。

---

## 场景启动时序

```
Prepare.unity 加载
│
├─ GamePrepare.Awake()                    ← 最早执行的 MonoBehaviour
│  ├── LogCollect.Init() (GM模式)
│  ├── InitLog()                          ← SGF.Debuger 日志配置
│  ├── SaveManager.Instance.Init("Stars/")  ← PlayerPrefs 封装
│  ├── LocalDataManager.Instance.Init()   ← 本地数据缓存 (706 文件)
│  ├── LanguageManager.Instance.Init()    ← 多语言初始化
│  ├── GetHardward()                      ← 硬件检测 / 模拟器识别
│  │   ├── 平台检测: WindowsEditor/Android/iPhone
│  │   ├── 模拟器识别: IsRunningOnEmulator() (CPU指令集)
│  │   └── GameConfig.isEmulator = true/false
│  ├── InitLocalSettingCache()            ← 读取本地画质/音效缓存
│  │   ├── SETTING_GRAPH → _graphQualityLevel
│  │   ├── SETTING_SGSR → GameConfig.UseSGSR
│  │   └── SETTING_SGSR_EdgeSharpness
│  ├── SetQuality()                       ← 画质分档设置
│  │   ├── CheckQuality() → GameConfig.MachineQualityLevel
│  │   ├── SetResolution()
│  │   └── GameManager.Instance.SetSGSR()
│  ├── UString.Initialize()               ← 字符串工具
│  └── LuaManager.Instance.Init()         ← XLua 虚拟机
│
├─ GamePrepare.Start()                    ← 播放 Logo 视频
│  ├── 根据语言选择: YoKaLogoH.mp4 / YoKaLogoH_en.mp4
│  └── VideoPlayer.Prepare()
│
├─ 视频播放完成 → UnityVideoPlayOver()
│  └── SDKManager.Instance.SetOnSdkInit(callback)
│
├─ SDK 初始化完成 → PrepareToUpdate()
│  ├── 上报设备信息: CPU/GPU/型号/系统
│  ├── 中文版: PlayAdvice() → 3秒健康公告 → CheckGameUpdate()
│  └── 国际版: 直接 → CheckGameUpdate()
│
├─ GameUpdate.CheckUpdate()               ← Unity Addressables 资源更新
│  ├── 版本号比对: RemoteVersion vs LocalVersion
│  ├── Addressables.InitializeAsync()
│  ├── CheckCatalogUpdate() → 资源目录更新
│  ├── DownloadDependencies() → 下载差量
│  └── 完成 → GameApp.Run(LoadingState.GameUpdate)
│
└─ GameApp.cs → 场景切换 → AppMain.Init() ← ★ 真正游戏入口
```

---

## AppMain.Init() — 游戏主体初始化

**文件**: `Assets/Scripts/StarGame/AppMain.cs:76-103`

```csharp
protected override void Init()
{
    urpAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
    Screen.sleepTimeout = SleepTimeout.NeverSleep;

    Loom.Initialize();                        // 1. 主线程调度器
    ModuleDef.Init();                         // 2. 模块枚举字典

    CheckDriverPlatformRenderType();          // 3. Vulkan/OpenGL ES
    InitEngineSetting();                      // 4. URP + 纹理流式加载
    InitAppSetting();                         // 5. APPHelper
    InitServices();                           // 6. ★ 全部服务初始化
    InitBusiness();                           // 7. ★ 全部业务模块创建
    InitHelper();                             // 8. GM 调试
    InitThd();                                // 9. DOTween(300, 50)
    OnFinishedPlaying();
}
```

---

## InitEngineSetting() — 引擎设置详解

```csharp
private void InitEngineSetting()
{
    QualitySettings.streamingMipmapsActive = true;
    // MipMap 流式加载：仅加载需要的 Mip 级别，节省 GPU 内存
    // 策略：被动式缓存 (textureDiscardUnusedMips = false)
    //       新贴图需要空间时才清理旧贴图池

    GraphicsSettings.useScriptableRenderPipelineBatching = true;
    QualitySettings.maximumLODLevel = 0;
    GameConfig.AllowDynamicResolution = true;

    SetQualityLevel(GameConfig.MachineQualityLevel);
    // 根据机型分4档:
    // TopestLevel → UseHighHighHighLevelSetting()
    // TopLevel    → UseHighLevelSetting()
    // MiddleLevel → UseMiddleLevelSetting()
    // LowerLevel  → UseLowLevelSetting()
    // LowestLevel → UseLowestLevelSetting()
}
```

### 各档关键参数对比

| 参数 | Topest | Top | Middle | Low | Lowest |
|------|--------|-----|--------|-----|--------|
| 帧率 | 60 | 60 | 30 | 30 | 30 |
| VSync | 0 | 0 | 1 | 0 | 0 |
| 渲染比例 | 1.0 | 1.0 | 0.8 | 0.5 | 0.5 |
| 纹理限制 | 0 | 0 | 1(1/2) | 2(1/4) | 3(1/8) |
| 后处理 | ✅ | ✅ | ✅ | ❌ | ❌ |
| AA 模式 | FXAA High | FXAA Med | None | None | None |
| 软阴影 | ✅ | ✅ | ✅ | ❌ | ❌ |
| LOD Bias | 1.0 | 1.0 | 0.6 | 0.5 | 0.3 |
| 特效上限 | 50 | 10 | 10 | 10 | 10 |
| 描边 | ✅ | ✅ | ❌ | ❌ | ❌ |
| Shader LOD | 500 | 500 | 200 | 200 | 200 |

---

## InitServices() — 服务初始化顺序

**关键**: 顺序严格，不可调换！依赖项必须在前创建。

```
ModuleManager.Init("StarProject.Module")     ← 1. 模块系统
MsgRetManager.Init()              ← 2. 消息回调
FixMessageManager.Init()          ← 3. 消息分发
Client2ThirdMsgManager.Init()     ← 4. 第三方消息
NetworkManager.Init()             ← 5. TCP/KCP Socket
UIManager.Init("UI/")             ← 6. UI 框架 (UI 资源根路径)
  + UIManager.MainPage = UIDef.UILoginPage
  + UIManager.MainScene = "MainTown"
UIQueueManager.Init()             ← 7. UI 队列
RenderManager.Init()              ← 8. 渲染
UserManager.Init()                ← 9. 用户数据
LuaManager.OnPostInit()           ← 10. Lua 虚拟机就绪
PreLoading.Init()                 ← 11. 预加载 (角色/纹理)
TimeManager.Init()                ← 12. 时间同步
LocalCache.Init()                 ← 13. 本地缓存(static)
LocalCacheManager.Init()          ← 14. 本地缓存管理
GameManager.Init()                ← 15. 游戏核心
BusinessManager.Init()            ← 16. 业务调度 (65KB)
SlotManager.Init()                ← 17. 技能/装备插槽
CameraManager.Init()              ← 18. 摄像机
InputManager.Init()               ← 19. 输入
FindPathManager.Init()            ← 20. NavMesh 寻路
AtlasManager.Init()               ← 21. 图集
DisplayProcessDispenser.Init()    ← 22. 提示消息队列
SoundManager.Init()               ← 23. Wwise 音频
StarScenesManager.Init()          ← 24. 场景加载
TriggerEntityManager.Init(RemoteDynamicRoot) ← 25. 触发器实体
LocalFxManager.Init(LocalDynamicRoot)        ← 26. 本地特效
GlobalFunctionManager.Init()      ← 27. 全局功能 (112KB)
CameraShakeManager.Init()         ← 28. 摄像机震动
ServerServiceManager.Init()       ← 29. 服务器服务实体
PartnerManager.Init()             ← 30. 伙伴管理
UniRenderPipline.Init()           ← 31. URP 自定义渲染
TimelineManager.Init()            ← 32. Timeline
ClientNpcManager.Init()           ← 33. NPC 客户端
BattleManager.Init()              ← 34. 战斗服务
FightPowerManager.Init()          ← 35. 战力计算 (73KB)
SkillEffectManager.Init()         ← 36. 技能特效
WorldItemChecker.Init()           ← 37. 场景物品检测
DynamicRenderQueueManager.Init()  ← 38. 动态渲染队列
RedPointManager.Init()            ← 39. 红点系统
SystemOpenManager.Init()          ← 40. 系统功能开放
MutiScenesMergeManager.Init()     ← 41. 多场景合并
LogModule.LogReport.Instance.Init() ← 42. 日志上报
```

> InitServices 含 42 步调用，详见上方完整列表。部分子服务在父服务 Init() 中嵌套初始化。

---

## InitBusiness() — 业务模块创建顺序

按功能域分组创建 ~80 个模块：

```csharp
private void InitBusiness()
{
    // ══════ 核心 ══════
    LoginModule
    TutorialModule          (非 Editor 模式)
    TriggerModule           // 触发器
    InterActionModule       // 交互

    // ══════ 道具系统 ══════
    ItemControllerModule    // 道具管理
    BagModule               // 背包
    ItemTipsModule          // Tip 弹窗
    FuncTipsModule
    ItemUseModule           // 使用弹框
    ItemBuyModule           // 购买弹框
    DropControllerModule    // 掉落管理
    DropInfoListModule
    QuickEquipModule        // 快速穿戴
    ItemResolveModule       // 分解

    // ══════ 战斗 ══════
    EctypeModule            // 副本
    BattleTeamModule        // 伙伴出战
    SkillWindowModule       // 技能
    MedicineModule          // 药品
    SecretAreaModule        // 个人秘境
    PersonalTowerModule     // 爬塔
    PvpModule               // PVP
    ArenaModule             // 异步竞技场
    WildBossModule          // 野外 Boss

    // ══════ 装备 ══════
    EquipModule             // 装备
    EquipSlotControllerModule // 槽位强化
    EquipUpgradeModule      // 强化

    // ══════ 社交 ══════
    ChatModule
    FriendModule
    GuildModule
    MailModule
    TeamModule
    DonateModule            // 公会捐献
    PartyTimeModule         // 公会篝火

    // ══════ 经济 ══════
    ShopModule              // 商店
    ExchangeModule          // 交易行
    RechargeModule          // 充值
    BankModule              // 银行

    // ══════ 活动 ══════
    EventModule
    EventBossRankModule
    ActivityModule          // 签到
    DrawCardModule          // 抽卡
    OnlineRewardModule      // 在线奖励
    AfternoonGveModule      // 午间 GVE

    // ══════ 系统 ══════
    GamePlayChoiceModule    // 玩法选择
    EctypeEntranceModule    // 入口
    EctypeBagModule         // 临时背包
    EctypePopModule         // 弹窗
    EctypeSettleModule      // 结算
    WorldMapModule          // 世界地图
    WorldLineModule         // 世界线
    RankModule              // 排行榜
    SystemOpenTipsModule    // 系统解锁提示
    DailyActModule          // 每日活动
    GamePlayCalendarModule  // 玩法日历
    FightPowerModule        // 战力
    PlayerPreviewModule     // 玩家预览

    // ══════ 其他 ══════
    RunHorseModule          // 跑马灯
    TalentModule            // 天赋
    SelectorModule          // 选择器
    LivingSkillsModule      // 生活技能
    AdventureLevelModule    // 冒险等级
    TreasureModule          // 藏宝图
    WantedModule            // 通缉
    RingTaskModule          // 环任务
    RewardsPopModule        // 奖励弹窗
    RayCheckModule          // 遮挡半透
    BeginnerTargetModule    // 新手目标
    SkillUnlockTipsModule   // 技能解锁
    AnnouncementModule      // 公告
    CommercializationModule // 商业化
    MindRepairModule        // 心灵修复小游戏
    AmuletModule            // 符文
    InscriptionModule       // 鸣器
    ObjectInteractiveModule // 物体交互
    AvgLuaModule            // AVG
    ScenePlayModule         // 场景玩法
}
```

### 创建规则

- C# 模块（enum < 5000）：`new BusinessModule()` 反射实例化
- Lua 模块（enum > 5000）：`LuaManager.GetLuaModule() → new LuaModule(bind LuaTable)`
- 可通过 `ModuleManager.GetModule(moduleId)` 获取已创建模块

---

## 热更流程

```
GameUpdate.CheckUpdate(LoadingView)
├── 读取 version.txt (远程 URL)
├── 版本号比对 (RemoteVersion vs LocalVersion)
├── Addressables.InitializeAsync()
│   └── 失败 → 重试/报告错误
├── CheckCatalogUpdate()
│   ├── 加载远程 catalog hash
│   └── 比对本地 catalog
├── 有更新 → DownloadDependencies()
│   ├── 计算下载大小
│   ├── 进度回调 → GameProcess.OnProcess(progress)
│   └── 完成/失败处理
└── 完成 → GameApp.Run() → 场景切换
```

---

## 关键时序总结

```
Prepare.unity
  │
  ├── GamePrepare.Awake       (最早: 日志/硬件/缓存/画质/Lua)
  ├── GamePrepare.Start       (Logo 视频)
  ├── SDK 初始化              (异步回调)
  ├── 健康公告 (3秒)          (仅中文)
  ├── GameUpdate              (Addressables 热更)
  │
  └── GameApp.Run() → 场景切换
      │
      └── AppMain.Init()
          ├── Loom / ModuleDef
          ├── CheckDriverPlatformRenderType
          ├── InitEngineSetting (URP/画质)
├── InitServices
├── InitBusiness
└── TravelToScene(GameEnter_Login) → 登录流程
```

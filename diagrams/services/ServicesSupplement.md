# 常驻服务补充 — 未文档化服务详情

> Level 3 — 补齐 `_Nav__Service.md` 中未覆盖的 14 个常驻服务。

---

## 1. UserManager

**文件**: `Service/UserManager/UserManager.cs` (822 B)  
**命名空间**: `StarProject.Service.User`  
**基类**: `ServiceModule<UserManager>`

```csharp
public class UserManager : ServiceModule<UserManager>
{
    private UserData m_mainUserData;
    public UserData MainUserData { get; }

    public void Init()       // AppMain.InitServices() 中调用
    public void Clear()      // 登出时清理
    public void UpdateMainUserData(UserData data)  // 登录后更新用户数据
}
```

**职责**: 保存当前登录用户核心数据（playerRoleId/accountID/groupID/groupName/areaID），供全局访问。

---

## 2. SDKManager

**文件**: `Service/SDKManager/SDKManager.cs` (47 KB)  
**命名空间**: `StarProject.Service.SDK`  
**类型**: 手动双检锁单例（非 ServiceModule 泛型），因 GamePrepare.Awake 阶段 ServiceModule 体系尚未初始化。

```csharp
public class SDKManager
{
    private static SDKManager _instance;
    public static SDKManager Instance { get; } // 双检锁单例

    public bool IsInit;
    public string LoginResult;                 // SDK 登录结果 JSON
    public SKDLoginResult sdkLoginResult;      // 解析后的登录数据

    public void Init();           // 初始化 SDK（GSSDK）
    public void Login();          // 弹出 SDK 登录窗口
    public void Logout();         // 登出
}
```

**职责**: 封装 GSSDK（第三方登录 SDK），管理登录/登出/支付回调。与 Android/iOS 原生层通过 jar/aar 通信。

**初始化时序**: GamePrepare 视频播放完成 → SDKManager.Instance.SetOnSdkInit(callback) → SDK 初始化完成 → PrepareToUpdate()

---

## 3. BusinessManager

**文件**: `Service/BusinessManager/BusinessManager.cs` (65 KB)  
**命名空间**: `StarProject.Service.Business`

```csharp
public class BusinessManager : ServiceModule<BusinessManager>
{
    public EntityCtrlBase M_MainPlayerCtrlBase; // 主角控制器快捷引用
    public ulong GetUserPID();                  // 玩家角色ID
    public ulong GetAccountID();                // 账号ID
    public uint GetAreaID();                    // 区服ID
    public string GetAreaName();                // 区服名
    public string GetChannelID();               // 渠道ID
}
```

**职责**: 业务层全局访问入口，提供常用数据查询快捷方法（玩家ID/账号/区服/渠道），协调多个业务模块间的数据交互。65KB 大型文件。

---

## 4. SDKManager 补充: SKDLoginResult

SDK 登录成功后的数据结构：

```csharp
public class SKDLoginResult
{
    // 从 JSON 解析:
    // accountID, playerRoleId, groupID, groupName
    // token, serverAddress, serverPort
    // 创建角色时需要的数据
}
```

---

## 5. ServerServiceManager

**文件**: `Service/ServerServiceManager/ServerServiceManager.cs` (10 KB)  
**命名空间**: `StarProject.Service.ServerService`

```csharp
public class ServerServiceManager : ServiceModule<ServerServiceManager> { }

// 服务实体
public class ServerService : EntityRemoteStatic
{
    public string ServiceName;       // 服务名（动态注册时填写）
    public int serviceID;
    public int npcID;
}

public enum ServerServiceType { Default, NPC, InterAction }
public enum RegServiceSubType { Task_Pick, Task_Finish, Task_Event }
public enum ServerServiceState { Default, Open, Close }
```

**职责**: 管理服务器注册的服务实体（NPC 交互服务、任务承接/交付/事件服务等），与 Trigger 系统配合使用。

---

## 6. DisplayProcessDispenser

**文件**: `Service/DisplayProcessDispenser/DisplayProcessDispenser.cs`  
**命名空间**: `StarProject.Service.DisplayProcess`

```csharp
public class DisplayProcessDispenser : ServiceModule<DisplayProcessDispenser>
{
    // 系统提示消息队列
    private Queue<string> MessageQueue;
    private int curMessageDelay;                    // 每N个逻辑帧显示一个
    private const int messageMaxDelay = 5;           // GameConfig.SPECIAL_MESSAGE_FIX_SPACE_DELAY

    // 战斗消息
    private BattleUITips battleTip;
    private Queue<string> SystemMessageQueue;        // 系统消息队列
    private float curSystemMessageDelay;              // GameConfig.SYSTEM_MESSAGE_SPACE_DELAY=2s

    // 成就消息
    private Queue<string> AchievementMessageQueue;
    private float curAchievementMessageDelay;         // GameConfig.ACHIEVEMENT_MESSAGE_SPACE_DELAY=2.1s

    // 伙伴升级消息
    private Queue<string> PartnerLevelTeamMessageQueue;

    // 对象池
    private static Queue<GameObject> UnUseGameTips;
    private static Queue<SystemUITips> UnUseSystemTips;
}
```

**职责**: 管理所有游戏内提示信息的队列显示，包括系统提示、战斗飘字、成就提示、伙伴升级提示。使用对象池复用 UI。

---

## 7. FindPathManager

**文件**: `Service/FindPathManager/FindPathManager.cs` (19.6 KB)  
**命名空间**: `StarProject.Service.FindPath`

**职责**: 寻路服务，封装 Unity NavMesh API，为角色移动提供路径计算。核心参数在 GameConfig:

```csharp
// GameConfig 寻路参数:
public const float NAVMESH_RADIUS = 0.2f;     // 寻路半径
public const float NAVMESH_EXTEND = 0.01f;     // 扩展
public const float NAVMESH_STEPHEIGHT = 2.4f;  // 步高（调大可下更陡的坡）
```

---

## 8. TimeManager

**文件**: `Service/TimeManager/TimeManager.cs` (5.9 KB)  
**命名空间**: `StarProject.Service.Time`

**职责**: 服务器时间同步服务。游戏切回前台时重新请求服务器时间 (`OnApplicationPause` 触发 `TimeManager.Instance.RegReqServerTime()`)。

---

## 9. LocalFxManager

**文件**: `Service/LocalFxManager/LocalFxManager.cs` (47.8 KB)

```csharp
public class LocalFxManager : ServiceModule<LocalFxManager>
{
    public int MaxFxCount;                  // 特效上限 (Top=50, Mid/Low=10)
    public bool OnlyShowMainPlayerFX;       // 只显示主角特效（性能开关）
    public bool NoFxAll;                    // 全部关闭特效

    public void Init(Transform root);       // 传入 LocalDynamicRoot
}
```

**职责**: 管理本地动态特效的创建/回收/上限控制。根据画质档位调整 MaxFxCount。

---

## 10. TriggerEntityManager

**文件**: `Service/TriggerEntityManager/TriggerEntityManager.cs` (17.8 KB)

```csharp
public class TriggerEntityManager : ServiceModule<TriggerEntityManager>
{
    public void Init(Transform root);  // 传入 RemoteDynamicRoot
}
```

**职责**: 管理触发区域实体（碰撞检测），与 TriggerModule 配合实现 NPC 交互检测、任务区域触发等。

---

## 11. MutiScenesMergeManager

**文件**: `Service/MutiScenesMergeManager/MutiScenesMergeManager.cs` (5.2 KB)

**职责**: 多场景合并管理器。支持动态加载/卸载子场景（如 UI 场景与游戏场景分离加载）。

---

## 12. CameraShakeManager

**文件**: `Service/CameraShakeManager/CameraShakeManager.cs` (27.4 KB)

**职责**: 摄像机震动效果管理。用于战斗技能震屏、爆炸效果等。

---

## 13. GlobalFunctionManager

**文件**: `Service/GlobalFunctionManager/GlobalFunctionManager.cs` (112.6 KB)

**职责**: 全局功能管理器。112KB 大文件，包含通用游戏功能（如全局快捷键、全局回调注册等）。

---

## 14. SlotManager

**文件**: `Service/ModuleIntegration/SlotManager.cs` (11.6 KB)

**职责**: 插槽管理器，属于模块集成服务组。管理技能槽/装备槽等 UI 槽位。

---

## 补充服务（轻量）

| 服务 | 文件大小 | 职责 |
|------|----------|------|
| **ShaderManager** | 410 B | Shader 管理（极简） |
| **AvProManager** | 4.6 KB | 视频播放（AvPro 组件包装） |
| **ClientNpcManager** | — | NPC 客户端管理器 |
| **WorldItemChecker** | — | 场景物品检测 |
| **DynamicRenderQueueManager** | — | 动态渲染队列管理（描边/材质质量分） |
| **RedPointManager** | — | 红点系统（各模块解锁/开启提示） |
| **SystemOpenManager** | — | 系统功能开放管理（等级解锁等） |
| **FightPowerManager** | 72.8 KB | 战力计算管理 |
| **PartnerManager** | — | 伙伴系统管理 |
| **UniRenderPipline** | — | URP 自定义渲染管线配置 |

---

## 服务依赖关系

```
AppMain.InitServices() 严格按照序初始化:

ModuleManager        ← 最先，所有模块系统基础
  ↓
NetworkManager       ← 依赖 ModuleManager 的消息分发
  ↓
UIManager            ← 依赖 独立
  ↓
RenderManager        ← 渲染必须早于摄像机
  ↓
UserManager          ← 用户数据全局引用
  ↓
LuaManager           ← Lua 虚拟机必须在业务模块前就绪
  ↓
GameManager          ← 游戏核心
  ↓
BusinessManager      ← 依赖 GameManager
  ↓
CameraManager        ← 摄像系统
  ↓
InputManager         ← 输入
  ↓
FindPathManager      ← 寻路
  ↓
AtlasManager         ← 图集
  ↓
SoundManager         ← 音频 (Wwise)
  ↓
StarScenesManager    ← 场景加载
  ↓
TriggerEntityManager ← 触发器
  ↓
LocalFxManager       ← 本地特效
  ↓
TimelineManager      ← Timeline
  ↓
BattleManager        ← 战斗服务 (接近最后)
  ↓
RedPointManager      ← 红点 (最后)
  ↓
SystemOpenManager    ← 系统开放 (最后)
```

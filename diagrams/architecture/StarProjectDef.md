# StarProjectDef — 共享定义层

> Level 3 — 全局枚举、配置常量、UI 定义、模块定义、数据包装结构体。

---

## 概述

`StarProjectDef` 是项目最底层的共享定义命名空间，被 StarFramework 和 StarGame 共同引用，自身无任何外部依赖。包含全局枚举（91KB GameEnums）、游戏配置常量、模块枚举定义（C#/Lua 分界线 5000）、UI ID 定义以及 350+ 配置包装结构体。

**文件位置**:
- `Assets/Scripts/StarGame/GameEnums.cs` — 全局枚举 (91 KB, ~2000+ 行)
- `Assets/Scripts/StarGame/GameConfig.cs` — 全局配置常量
- `Assets/Scripts/StarGame/Module/ModuleDef.cs` — 模块枚举 (LuaModuleType=5000)
- `Assets/Scripts/StarGame/UIDef.cs` — UI 资源路径枚举常量定义
- `Assets/Scripts/StarGame/WrapData/` — 350+ 配置结构体

**命名空间**: `StarProjectDef`

---

## 1. GameEnums.cs — 全局枚举总集 (91 KB)

所有系统和模块共用的枚举定义，使用 `[XLua.LuaCallCSharp]` 标记供 Lua 侧访问。

### 关键枚举类别

| 类别 | 枚举 | 用途 |
|------|------|------|
| 渲染表现 | `E_ShaderShowType` / `E_OverLayTypeOrderBase` | 角色溶解/Alpha、头顶飘字层级 |
| UI 系统 | `UIAsyncLoadState` / `ThreeDModelPos` / `ChangePageType` | UI 异步加载、3D 模型位置 |
| UI 显示控制 | `MainPageCommond` (Flags) | 位掩码控制页面分区块显隐 |
| UI 队列 | `UISeatType` / `UIQueuePriorityType` | 队列弹窗位置和优先级 |
| AOI 同步 | `EnumAOIType` | Int/Long/String/Proto 等属性同步类型 |
| 战斗状态 | `E_BattleStateType` (Flags) | 禁移动/禁转向/禁伤害/禁治疗/禁技能等 |
| 实体类型 | `E_OutlineEntityType` | 静态/动态物品、主角、英雄、怪物、NPC |
| 摄像机 | `E_CameraType` | StarWorldCam / UICam / SpecialCam |
| 音效 | `E_MusicTriggerType` | State 临时状态 / Event 事件触发 |
| 场景 | `E_SceneType` | PerInit / GameEnter_Login / GameScene_Town / Explorer |

### 关键代码：MainPageCommond 位掩码

```csharp
[Flags]
public enum MainPageCommond
{
    HideNone = 0,
    HideMoveWidgetCommond = 1 << 0,     // 1 - 移动摇杆
    HideSkillWidgetCommond = 1 << 1,    // 2 - 技能按钮
    WidgetHide = (1 << 2) - 1,          // 3 - Widget 全部隐藏
    HidePageTopLeft = 1 << 2,           // 4 - 左上角
    HidePageTopCentre = 1 << 3,         // 8 
    // ... 9 个区块位
    ChatHide = HidePageModdleRight + WidgetHide,
    HudHideRight = 右侧全部 + WidgetHide,
    HideBoth = (1 << 11) - 1,           // 全隐藏
}
```

### 关键代码：UIQueuePriorityType

```csharp
public enum UIQueuePriorityType
{
    None = 0,
    NotBlockingPlot = 10,    // 不阻断剧情
    PermanentTips = 20,      // 常驻提示
    GetTips = 30,            // 获得提示
    ShortMsg = 40,           // 短消息
    PlayGmae = 50,           // 小游戏
    Button = 60,             // 按钮级
    Full = 70,               // 全屏
    BlockingPlot = 80,       // 阻断剧情
}
```

---

## 2. GameConfig.cs — 全局配置常量

**文件**: `Assets/Scripts/StarGame/GameConfig.cs` (545 行)

### 2.1 帧率与时间

```csharp
public const byte UpdatePerFix = 2;
public const byte FIX_TIME_PER_SEC = 30;
public const float FIX_LOGIC_FRAME = 0.03333f;   // 30 FPS 逻辑帧
public const float FIX_RENDER_FRAME_SEC = 0.016666f; // 60 FPS 渲染帧(高配)
```

### 2.2 网络同步容差

```csharp
public const float C2S_MINI_MOVE_SYNC_DISTANCE = 0.1f;  // 客户端→服务器最小同步距离
public const float C2S_MINI_SYNC_ANGLE = 5f;             // 最小角度同步
public const float S2C_MINI_PATH_MOVE_SYNC_DISTANCE = 0.1f; // 服务器路点最小误差
public const float S2C_MINI_MOVE_SYNC_TIME = 1f;         // 路点同步最小时间
```

### 2.3 动画速度基准

```csharp
// 怪物
public const float CLIENT_MONSTER_ANIM_BASE_WALK_SPEED = 2.0f;
public const float CLIENT_MONSTER_ANIM_BASE_RUN_SPEED = 4.0f;
// 英雄
public const float CLIENT_HERO_ANIM_BASE_WALK_SPEED = 2.5f;
public const float CLIENT_HERO_ANIM_BASE_RUN_SPEED = 5.0f;
```

### 2.4 物理碰撞参数

```csharp
public const float PLAYER_SKIN_WIDTH = 0.01f;
public const float MODEL_RADIUS = 0.5f;
public const float MODEL_RADIUS_FACTOR = 3f;  // 胶囊体半径系数 2~N
public const float NAVMESH_RADIUS = 0.2f;
public const float NAVMESH_STEPHEIGHT = 2.4f;

// 移动: 6 m/s, 每帧 0.2m
public const float PLAYER_MOVE_SPEED_PER_SEC = 6;
public const float PLAYER_MOVE_SPEED_PER_FIXFRAME = 0.2f;
public const float PLAYER_ROTATE_SPEED = 1080f; // 1080 度/秒
```

### 2.5 AOI 模拟加速

```csharp
public const float THD_PERSION_CLIENT_SIM_SPEED_PARA = 1.1f; // 第三人称
public const float AOI_CLIENT_SIM_SPEED_PARA = 1.02f;        // AOI
public const bool RotSyncMode = true;   // 转角：立刻完成
public const bool MoveSyncMode = false; // 移动：加速走完
```

### 2.6 技能相关配置

```csharp
public const string RES_CONFIG_JSON_PATH = "Config/Skill/";
public const int SKILL_LAG_TIME = 100;              // 技能网络延迟(ms)
public const int SKILL_STAGE_NET_LAG_TIME = 100;    // 技能阶段延迟(ms)
public const float STATIC_SKILL_LAG_TIME = 15;       // 静态补偿(ms)
public const int SKILL_PARTNER_POS_ID = 100;         // 伙伴技能位
public const float System_Skill_Down_Time = 0.067f;  // 技能按下判断时间
```

### 2.7 画质与机器分档

```csharp
private static MachineQualityLevel machineQualityLevel = MachineQualityLevel.TopLevel;
public static MachineQualityLevel MachineQualityLevel { get; set; } // 触发 GlobalEvent
public static MachineQualityLevel InitQualityLevel;     // 启动时记录的分档
public static byte QualityForCameraClipNeighbor = 0;    // 裁切距离级别
public static bool AllowDynamicResolution = true;
public static bool UseSGSR = false;                      // SGSR 超分
public static ModelQualityLevel modelType = ModelQualityLevel.Low;
```

### 2.8 阵营与寻路

```csharp
// 阵营颜色 (Hex)
public const string COLOR_NAME_TEAM = "#A0FF76";
public const string COLOR_NAME_ENEMY = "#EE2523FF";
public const string COLOR_NAME_NPC = "#3BD1CAFF";
public const string COLOR_NAME_LABOR_UNION = "#ffd35d";
// PVP 蓝红方
public const string COLOR_NAME_BLUE = "#64C3FF";
public const string COLOR_NAME_RED = "#FF6464";
```

### 2.9 本地缓存 Key

```csharp
public const string SETTING_KEY = "SettingKey";
public const string SETTING_GRAPHKEY = "SettingGraphKey";
public const string SETTING_GRAPH = "SettingGraph";          // 画质
public const string SETTING_BATTLE = "SettingBattle";        // 战斗设置
public const string SETTING_BACKGROUND_MUSIC = "SettingBackgroundMusic";
public const string SETTING_SGSR = "SettingSGSR";
public const string SETTING_CAMERA = "Setting_Camera";
```

### 2.10 副本子类型

```csharp
public const string INSTANCE_NORMAL = "Normal";           // 普通副本
public const string INSTANCE_PERSON_DAILY = "PersonDaily"; // 个人日常
public const string INSTANCE_TEAM_DAILY = "TeamDaily";    // 组队日常
public const string INSTANCE_10v10 = "10v10";            // 随机战场
public const string INSTANCE_1v1 = "1v1";                // 异步竞技场
public const string INSTANCE_PERSON_TOWER = "PersonTower"; // 个人爬塔
public const string INSTANCE_PLOT_FIRST = "PlotFirst";   // 新手关1
public const string INSTANCE_GuildTerritory = "GuildTerritory"; // 公会领地
// 共 18 种副本类型（以上仅列出常用）
```

---

## 3. ModuleDef — 模块枚举定义

**文件**: `Assets/Scripts/StarGame/Module/ModuleDef.cs` (215 行)

### 3.1 C#/Lua 分界线

```csharp
public enum Name
{
    None,
    // C# 模块区 (< 5000)
    LoginModule, StarWorldModule, TriggerModule, InterActionModule,
    RayCheckModule, RingTaskModule, ItemControllerModule, WorldMapModule,
    PlayerLocalCache, EctypeModule, TutorialModule, ScenePlayModule,
    // ... (C# 模块)

    LuaModuleType = 5000,  // 🚨 分界线

    // Lua 模块区 (> 5000)
    AvgLuaModule, TaskModule, EquipModule, ShopModule, RunHorseModule,
    ItemUseModule, ItemBuyModule, GuildModule, BagModule, ItemTipsModule,
    // ... (Lua 模块)
}
```

### 3.2 判断逻辑

```csharp
public static ModuleDefType GetModuleDefState(Name E_ModuleName)
{
    if (E_ModuleName == Name.None || E_ModuleName == Name.LuaModuleType)
        return ModuleDefType.None;
    if ((int)E_ModuleName > (int)Name.LuaModuleType)
        return ModuleDefType.LuaModuleDef;  // Lua 模块
    else
        return ModuleDefType.CsModuleDef;   // C# 模块
}
```

### 3.3 模块枚举→名称双向映射

```csharp
private static Dictionary<Name, string> m_ModuleNameToEnumDict;  // enum → "LoginModule"
private static Dictionary<string, Name> m_EnumToModuleNameDict;  // "LoginModule" → enum

public static void Init()  // 在 AppMain.Init() 中调用
{
    foreach (Name moduleEnum in Enum.GetValues(typeof(Name)))
        if (moduleEnum != Name.None && moduleEnum != Name.LuaModuleType)
            // 构建双向映射
}
```

---

## 4. UIDef — UI 资源定义

**文件**: `Assets/Scripts/StarGame/UI/UIDef.cs`

定义所有 UI 页面的资源路径名和类型枚举，作为 `UIManager` 的统一入口：

```csharp
// 示例（具体定义见源码）
UIManager.MainPage = UIDef.UILoginPage;   // 主页面 = 登录页
UIManager.MainScene = "MainTown";          // 主场景
```

UIManager 通过 `Init("UI/")` 指定资源根路径，所有 UI Prefab 从 `UI/` 目录加载。

---

## 5. WrapData — 配置结构体 (~350+)

**目录**: `Assets/Scripts/StarGame/WrapData/`

为表格配置数据提供 C# 结构体定义，与 `ExcelBytes/` 下的配置表一一对应。支持 XLua 访问（`[LuaCallCSharp]`），是配置表加载后反序列化的目标类型。

---

## 依赖关系

```
StarProjectDef (无依赖)
    ↑
    ├── StarFramework (SGF.*)
    └── StarGame (StarProject.*)
```

- StarProjectDef 是唯一无项目内依赖的命名空间
- 被 StarFramework 和 StarGame 同时引用
- `[XLua.LuaCallCSharp]` 标记的枚举和配置可供 Lua 侧使用

# 架构总览

> Level 2 — 项目整体架构信息。修改顶层设计前先读本文件。

## 子模块列表

| 编号 | 子模块 | Level 3 详情 | 职责 |
|------|--------|-------------|------|
| H1 | StarProjectDef | `StarProjectDef.md` | GameEnums(91KB)/GameConfig(545行)/ModuleDef(5000分界线)/UIDef/WrapData(350+) |
| H2 | 初始化全链路 | `InitFlow.md` | Prepare场景→热更→AppMain.Init()→Login 完整时序 |
| H3 | StarFramework 框架 | `../network/_Nav__Network.md` + `../ui/_Nav__UI.md` + `../module/_Nav__Module.md` | 网络/UI/模块三大框架子系统 |
| H4 | StarGame 游戏层 | `../battle/_Nav__Battle.md` + `../entity/_Nav__Entity.md` + `../services/_Nav__Service.md` | 战斗/实体/服务 游戏逻辑子系统 |

## 三层架构

```
StarFramework (SGF)  命名空间 SGF.*
├── Module.Framework    ModuleManager / Module / BusinessModule / ServiceModule
├── Network             NetworkManager / SocketBase / 协议编解码 / 重连
└── UI.Framework        UIManager / UIPage / UIWindow / UIWidget

StarGame  命名空间 StarProject.*
├── AppMain.cs          入口点 (Prepare场景 → InitServices → InitBusiness)
├── Game                Entity / Player / Skill / Buff / Passive / Map / Camera
├── Module              80+ 业务模块 (Login / StarWorld / Ectype / Tutorial...)
└── Service             30+ 常驻服务 (Resource / Lua / Network / Battle / Sound...)

StarProjectDef  命名空间 StarProjectDef
├── GameEnums           全局枚举 (91KB)
├── GameConfig          全局配置 (帧率30FPS/移动参数/网络/技能/画质/阵营)
├── ModuleDef           模块定义 (Name枚举, 5000分界线, 双向映射)
├── UIDef               UI 路径常量定义
└── WrapData            配置结构体 (350+)
```

## 依赖方向

```
StarProjectDef ←── StarFramework (共享引用)
StarProjectDef ←── StarGame     (共享引用)
StarGame ──→ StarFramework      (单向依赖)
```

## 初始化全链路

```
Prepare.unity
├── GamePrepare.Awake  日志/硬件检测/模拟器/画质/Lua
├── Logo 视频播放 → SDK初始化
├── 健康公告 (3秒, 仅中文)
└── GameUpdate (Addressables 热更)
    └── AppMain.Init()
        ├── Loom / ModuleDef
        ├── CheckDriverPlatformRenderType
        ├── InitEngineSetting (URP纹理流式/5档画质)
├── InitServices()
├── InitBusiness()
└── TravelToScene → 登录
```

详见 `InitFlow.md` 完整时序与参数详解。

## 关键设计原则

- **C#/Lua 混合**：模块 enum < 5000 = C# 反射实例化，> 5000 = LuaModule 桥接
- **工厂+对象池**：EntityFactory/ViewFactory 统一创建回收，禁止直接 new
- **逻辑/显示分离**：EntityObject(逻辑) ↔ ViewObject(MonoBehaviour 显示)
- **Timeline 驱动**：技能/Buff 核心由 Timeline 轨道驱动 SkillStage
- **双协议栈**：CustomMsg(自定义二进制+加密) 和 Protobuf 并存
- **30FPS 逻辑帧**：`FIX_TIME_PER_SEC=30`, `FIX_LOGIC_FRAME=0.03333f`
- **5 档画质**：Topest/Top/Middle/Low/Lowest，每档独立参数配置

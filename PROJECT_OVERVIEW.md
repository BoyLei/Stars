# 七王书客户端项目概览

## 项目定位

本仓库是“七王书”的 Unity 客户端工程，应用标识为 `com.Yoka.Stars`。项目以 C# 为主，使用 XLua 承载部分业务脚本，通过模块系统、常驻 Service/Manager、Addressables 资源系统和自定义网络层组织大型游戏客户端功能。

当前工程主要面向移动端，Android 使用 IL2CPP；同时保留 Standalone 和 iOS 配置。启动场景为 `Assets/Res/Map/Prepare.unity`。

## 技术基线

| 项目 | 当前配置 |
| --- | --- |
| Unity | `2022.3.42f1c1` |
| 渲染管线 | URP `12.1.9`，包含自定义 Renderer Feature 和画质分级 |
| 资源系统 | Addressables `1.22.2`、Resources、StreamingAssets |
| 脚本 | C#、XLua；Lua 通过 `LuaScripts/...` 逻辑路径作为 TextAsset 加载 |
| 序列化 | Protobuf、自定义协议、MessagePack |
| UI | UGUI、TextMeshPro、自定义 `UIManager`/`UIQueueManager` |
| 音频 | Wwise |
| 常用第三方 | DOTween、BestHTTP、Cinemachine、UniTask、Newtonsoft.Json |
| 测试 | Unity Test Framework `1.1.33`，另有测试场景和调试脚本 |

部分 Unity 包通过 `LocalPackages` 相对路径引用，私有包通过 Yoka 内部 npm registry 获取。首次打开工程前，需要确保这些本地目录和内部网络访问可用。

## 启动流程

核心入口是 `Assets/Scripts/StarGame/AppMain.cs` 中的 `AppMain`。初始化顺序对运行时依赖关系很重要：

```text
Prepare 场景
  -> AppMain.Init
  -> Loom / ModuleDef
  -> 平台与渲染配置
  -> Service/Manager 初始化
  -> C# 与 Lua 业务模块创建
  -> 辅助系统和第三方组件
  -> 登录模块与后续场景流程
```

`AppMain.InitServices()` 集中启动网络、UI、资源、缓存、时间、输入、场景、战斗、音频、渲染等常驻服务。`AppMain.InitBusiness()` 通过 `ModuleManager` 创建登录、背包、任务、副本、社交、商业化和活动等业务模块。

## 架构分层

### Framework 层

主要位于 `Assets/Scripts/StarFramework`，提供可复用的客户端基础设施：

- `Module`：模块创建、显示、关闭以及 C#/Lua 模块桥接。
- `Network`：长连接、鉴权、心跳、重连、协议编解码和主线程消息派发。
- `UI`：页面、窗口、队列和 UI 生命周期管理。
- `Storage`、`Time`、`Unity`、`Utils`：存储、时间、Unity 适配与通用能力。

### Game 层

主要位于 `Assets/Scripts/StarGame`，承载游戏运行时和业务实现：

- `Service`：资源、Lua、场景、战斗、输入、相机、音频、服务器状态等常驻服务。
- `Module`：业务模块定义和模块注册。
- `Game`：角色、地图、战斗对象、表现和世界运行逻辑。
- `UI`：登录、背包、任务、社交、活动、商业化等具体界面。
- `LocalCache`、`PreLoading`、`RemoteConfig`：本地状态、预加载和远程配置。

### 工具与内容层

- `Assets/Editor`：发包、Addressables、配置表、协议、资源检查和代码生成工具。
- `Assets/DevTools`、`Assets/GTTools`：任务、技能、时间轴、UI 和项目扫描等编辑器工具。
- `Assets/Res`、`Assets/Resources`、`Assets/StreamingAssets`：运行时内容和外部数据。
- `Assets/ThirdParts`、`Assets/Plugins`、`Assets/Wwise`、`Assets/XLua`：第三方 SDK 和原生插件。
- `Packages`、`LocalPackages`：UPM 包及项目内维护的 Unity 包版本。

## 核心系统

| 系统 | 职责 |
| --- | --- |
| `AppMain` | 应用生命周期、初始化顺序、画质和退出流程 |
| `ModuleManager` | C#/Lua 业务模块的注册、创建和切换 |
| `NetworkManager` | 游戏/战斗连接、消息订阅、反序列化和主线程派发 |
| `ResourceFormalManager` | Addressables 资源加载、缓存、对象池和 Lua TextAsset 访问 |
| `LuaManager` | XLua 虚拟机、加载器、Lua 模块及定时更新 |
| `UIManager` | UI 页面和窗口生命周期，入口资源路径为 `UI/` |
| `GameManager` | 世界和游戏状态的核心运行时协调 |
| `BattleManager` | 战斗域管理及相关服务 |
| `StarScenesManager` | 场景加载、切换和场景生命周期 |
| `RenderManager` / `UniRenderPipline` | URP、自定义渲染功能和平台画质适配 |

## 主要运行时关系

1. 业务模块通过 `ModuleManager` 管理生命周期，通过 Service/Manager 获取基础能力。
2. Lua 脚本由 `LuaManager` 从 `ResourceFormalManager` 按 `LuaScripts/...` 路径加载，并与 C# 模块桥接。
3. 网络线程完成 Socket 收发和协议预处理，`NetworkManager` 在 Unity 主线程按帧派发业务消息。
4. UI 由 `UIManager` 和业务模块协同控制，资源统一通过资源服务加载。
5. 场景、战斗、角色和表现系统通过常驻 Manager 共享状态，因此初始化和释放顺序必须保持一致。

## 构建与内容流水线

Unity 菜单 `Build` 下提供主要工程操作：

- 发包与代码编译。
- Addressables 分组刷新、资源构建和清理。
- Excel 配置更新与转换。
- 协议更新与代码转换。
- MessagePack IL2CPP 代码生成。
- 技能配置和其他项目数据导出。

`Assets/Editor/BuildProcess.cs` 和 `Assets/Editor/Build/BuildPack.cs` 包含 Android、Windows 等平台的构建逻辑。执行正式构建前，应先完成配置、协议和 Addressables 产物更新。

## 开发注意事项

- 使用与 `ProjectVersion.txt` 完全一致的 Unity 版本，避免资源和序列化格式被自动升级。
- 不提交 `Library`、`Temp`、`Logs` 等 Unity 生成目录。
- 大部分业务代码没有独立 asmdef，主要落在 `Assembly-CSharp`；修改公共类型会触发较大范围重编译。
- `AppMain` 集中维护初始化顺序。新增 Service/Manager 时，应明确其前置依赖、更新入口和释放时机。
- Lua 代码按资源逻辑路径加载；当前检出中未直接包含 `.lua` 源文件，排查脚本问题时需要同时确认资源产物。
- 网络回调、异步资源回调和 Unity 对象访问存在主线程边界，新增逻辑不得在后台线程直接操作 Unity API。
- 工程包含大量第三方和定制包，升级 Unity、URP、Addressables 或 XLua 前需要做真机回归。
- 当前项目级测试以测试场景和调试脚本为主，核心流程变更至少需要完成 Editor 冒烟、登录流程和目标平台构建验证。

## 快速开始

1. 使用 Unity Hub 以 `2022.3.42f1c1` 打开仓库根目录。
2. 确认 `LocalPackages` 引用可解析，并能访问项目所需的私有包源。
3. 等待脚本、Shader 和 Addressables 导入完成。
4. 打开并运行 `Assets/Res/Map/Prepare.unity`。
5. 修改配置、协议或资源分组后，从 Unity 的 `Build` 菜单执行对应生成步骤。

## 建议阅读顺序

1. `Assets/Scripts/StarGame/AppMain.cs`
2. `Assets/Scripts/StarFramework/Module/Framework/ModuleManager.cs`
3. `Assets/Scripts/StarFramework/Network/NetworkManager.cs`
4. `Assets/Scripts/StarGame/Service/ResourceManager/ResourceManagerExtend.cs`
5. `Assets/Scripts/StarGame/Service/LuaManager/LuaManager.cs`
6. `Assets/Editor/MenuExtends/BuildMenu.cs`

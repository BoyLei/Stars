# 常驻服务导航

> Level 2 — Service 层子模块划分。

## 子模块列表

| 编号 | 服务 | 命名空间 | Level 3 详情 | 职责 |
|------|------|---------|-------------|------|
| G1 | ResourceFormalManager | StarProject.Service | `ResourceFormalManager.md` | Addressables资源加载/缓存/对象池 |
| G2 | LuaManager | StarProject.Service.Lua | `LuaManager.md` | XLua虚拟机/Lua模块加载/定时更新 |
| G3 | NetworkManager | SGF.Network | `NetworkManager.md` | 双Socket/消息订阅/主线程分帧派发 |
| G4 | BattleManager | StarProject.Service | `BattleManager.md` | 战斗域管理 |
| G5 | SoundManager | StarProject.Service | `SoundManager.md` | Wwise音频 |
| G6 | GMManager | StarProject.Service | `GMManager.md` | GM调试指令 |
| G7 | CameraManager | StarProject.Service | `CameraManager.md` | StarsCamera摄像机 |
| G8 | InputManager | StarProject.Service | `InputManager.md` | 输入管理 |
| G9 | AtlasManager | StarProject.Service | `AtlasManager.md` | 图集管理 |
| G10 | LanguageManager | StarProject.Service | `LanguageManager.md` | 多语言 |
| G11 | TimelineManager | StarProject.Service | `TimelineManager.md` | Timeline驱动 |
| G12 | LocalDataManager | StarProject.Service | `LocalDataManager.md` | 本地数据缓存 |
| G13 | StarScenesManager | StarProject.Service | `StarScenesManager.md` | 场景加载切换 |
| G14 | RenderManager | StarProject.Service.UniRender | `RenderManager.md` | URP渲染/画质适配 |

| G15 | SDKManager | `ServicesSupplement.md` | GSSDK 登录/支付回调 (双检锁单例) |
| G16 | UserManager | `ServicesSupplement.md` | 当前用户数据全局引用 |
| G17 | BusinessManager | `ServicesSupplement.md` | 业务层访问入口 (65KB) |
| G18 | ServerServiceManager | `ServicesSupplement.md` | 服务器服务实体管理(NPC交互/任务) |
| G19 | DisplayProcessDispenser | `ServicesSupplement.md` | 提示消息队列(系统/战斗/成就) |
| G20 | FindPathManager | `ServicesSupplement.md` | NavMesh 寻路服务 |
| G21 | TimeManager | `ServicesSupplement.md` | 服务器时间同步 |
| G22 | LocalFxManager | `ServicesSupplement.md` | 本地特效管理(上限/画质) |
| G23 | TriggerEntityManager | `ServicesSupplement.md` | 触发区域实体管理 |
| G24 | CameraShakeManager | `ServicesSupplement.md` | 摄像机震动 |
| G25 | GlobalFunctionManager | `ServicesSupplement.md` | 全局功能(112KB) |
| G26 | MutiScenesMergeManager | `ServicesSupplement.md` | 多场景合并加载 |
| G27 | SlotManager | `ServicesSupplement.md` | 技能/装备插槽管理 |
| G28 | RedPointManager | `ServicesSupplement.md` | 红点系统 |
| G29 | SystemOpenManager | `ServicesSupplement.md` | 系统功能解锁(等级控制) |
| G30 | FightPowerManager | `ServicesSupplement.md` | 战力计算 (73KB) |

## 补充服务（轻量）

详见 `ServicesSupplement.md`：
- ShaderManager (410B) / AvProManager / ClientNpcManager / WorldItemChecker / DynamicRenderQueueManager / PartnerManager / UniRenderPipline

## 关键规则

- 所有服务继承 `ServiceModule<T>` (泛型单例)，SDKManager 除外（双检锁手动单例）
- 初始化顺序由 `AppMain.InitServices()` 严格控制，依赖项必须在前
- 新增服务必须明确前置依赖和释放时机
- 服务间通过 `GlobalEvent` 跨层通信

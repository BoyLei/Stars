# ResourceFormalManager

**继承：** `ServiceModule<ResourceFormalManager>` → `Module`
**文件：** `Assets/Scripts/StarGame/Service/ResourceManager/`

## 职责

Addressables 资源加载/缓存/对象池。游戏资源管理的统一入口。

## 核心功能

- Addressables 资源异步加载
- Lua TextAsset 访问（`LuaScripts/` 路径）
- 资源缓存与引用计数
- 对象池管理
- 场景资源预加载

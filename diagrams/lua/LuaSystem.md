# Lua 层

## 位置

```
Assets/Res/LuaScripts/
├── LuaModule/     业务模块 (77 个)
├── View/          UI 界面
├── Common/        公共库/工具
├── Struct/        数据结构
├── Global.lua.txt         全局环境
└── GlobalEvent.lua.txt    全局事件
```

## 架构

```
C# LuaModule (BusinessModule 子类)
  └── LUA: BaseLuaModule      (LuaModule/Core/BaseLuaModule.lua.txt)
        ├── ChatModule        ─── ChatModule.lua.txt
        ├── ShopModule        ─── ShopModule.lua.txt
        ├── TaskModule        ─── TaskModule.lua.txt
        └── ...
```

## 关键机制

- Lua 文件以 `.lua.txt` 后缀存储，通过 Resources/Addressables 加载
- 模块 identifier > 5000 自动走 Lua 路径
- **桥接详情**：C#↔Lua 生命周期映射 / 创建流程 / 事件绑定 → 见 `LuaModuleBridge.md`
- **模块通信**：`Event`（事件表）和 `GlobalEvent`（全局广播）→ 见 `../module/_Nav__Module.md`

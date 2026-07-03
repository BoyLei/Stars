# AGENTS.md — Star (七王书)

**代码根目录：** `Stars_Project/StarsProject_Client/trunk/Stars/`

## 核心结构

```
Assets/Scripts/StarFramework/  ← [SGF] 框架层
  ├── Module/Framework/        ModuleManager/Module/BusinessModule/ServiceModule
  ├── Network/                 NetworkManager/SocketBase/SocketItem/协议编解码
  └── UI/Framework/            UIManager/UIPage/UIWindow/UIWidget
Assets/Scripts/StarGame/      ← [Game] 游戏逻辑层，依赖 SGF
  ├── AppMain.cs              入口 (Prepare→InitServices→InitBusiness)
  ├── Game/                    Entity/Player/Skill/Buff/Passive
  ├── Module/                  80+ 业务模块 (Login/Bag/Task/Guild/...)
  └── Service/                 Resource/Lua/Battle/Sound/GM Manager
Assets/Editor/                 构建(BuildProcess/BuildPack)
```

## 关键约束

1. **禁止全量扫描** — 读文件前列出最小文件集，只搜 `Assets/Scripts/` 下具体子目录
2. **禁止目录：** `Library/` `Temp/` `Logs/` `Obj/` `Build/` `UserSettings/` `node_modules/` `.codegraph/` `.codex/` `.git/`
3. **主线程边界：** 网络/异步回调不得直接操作 Unity API
4. **公共类型修改触发大面积重编译**（无独立 asmdef）
5. **初始化顺序在 `AppMain.cs` 集中维护**，新增服务必须明确前置依赖
6. **长命令协议：** >30 秒的命令不要轮询，给出命令让用户手动执行
7. **升级权限：** 需要网络/全局安装/写工作区外的命令，先解释原因再提供命令，不要重试

## 修改前必读

| 优先级 | 文件 |
|--------|------|
| 1 | `Assets/Scripts/StarGame/AppMain.cs` |
| 2 | `Assets/Scripts/StarFramework/Module/Framework/ModuleManager.cs` |
| 3 | `Assets/Scripts/StarFramework/Network/NetworkManager.cs` |
| 4 | `Assets/Scripts/StarFramework/UI/Framework/UIManager.cs` |
| 5 | `Assets/Scripts/StarGame/Service/ResourceManager/ResourceManagerExtend.cs` |
| 6 | `Assets/Scripts/StarGame/Service/LuaManager/LuaManager.cs` |

## 大文件规则

- >500 行：只读类签名+核心方法，不要全文读入


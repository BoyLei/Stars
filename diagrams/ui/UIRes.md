# UIRes

**类型：** `static class`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 职责

UI 资源加载工具，统一管理 UI 预制件的加载。

## 核心方法

| 方法 | 说明 |
|------|------|
| `LoadPrefab(name)` | 同步加载 UI 预制件 |
| `LoadPrefabAsync(name, callback)` | 异步加载 UI 预制件 |

## 资源路径

所有 UI 资源统一从 `UI/` 目录加载：
```
UI/Page/xxx      → UIPage 预制件
UI/Window/xxx    → UIWindow 预制件
UI/Widget/xxx    → UIWidget 预制件
```

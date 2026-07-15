# Lua View 层

位置：`Assets/Res/LuaScripts/View/`

> C# UI 框架定义见 `../../ui/_Nav__UI.md`（UIPage/UIWindow/UIWidget 三层体系）

## 结构

```
View/
├── Core/UIPanel.lua.txt    基类
├── UIPage/                 页面 (全局唯一)
│   └── TestPage.lua.txt
├── UIWindow/               窗口 (443 个)
├── UIWidget/               挂件 (103 个)
└── Item/                   道具相关 (46 个)
```

## 对应关系

Lua View 与 C# UI 框架对应：

| C# | Lua View |
|----|----------|
| `UIPage` | `View/UIPage/*` |
| `UIWindow` | `View/UIWindow/*` |
| `UIWidget` | `View/UIWidget/*` |

## 统计

- UIWindow: ~443 个 Lua 文件
- UIWidget: ~103 个 Lua 文件
- UIPage: 1 个 (TestPage)
- Item: 46 个道具相关 UI 文件

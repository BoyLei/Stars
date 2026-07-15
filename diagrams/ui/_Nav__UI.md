# UI 框架导航

> Level 2 — UI 系统子模块划分。修改 UI 代码前先读本文件。

## 子模块列表

| 编号 | 子模块 | Level 3 详情 | 职责 |
|------|--------|-------------|------|
| C1 | UIManager | `UIManager.md` | UI生命周期管理入口，页面堆栈 |
| C2 | UIPanel | `UIPanel.md` | 抽象基类，定义Open/Close生命周期 |
| C3 | UIPage | `UIPage.md` | 页面，全局唯一，PageRoot，堆栈导航 |
| C4 | UIWindow | `UIWindow.md` | 窗口，可多开，WindowRoot，父子关系 |
| C5 | UIWidget | `UIWidget.md` | 挂件，不阻塞交互，WidgetRoot |
| C6 | UIRoot | `UIRoot.md` | 静态根节点管理(Page/Window/Widget等) |
| C7 | UIWindowStack | `UIWindowStack.md` | 窗口压栈弹栈管理 |
| C8 | UIQueueManager | `UIQueueManager.md` | UI优先级队列(Normal/Tips/BlockingPlot) |
| C9 | UIRes | `UIRes.md` | UI资源加载(UI/路径) |

## 层次结构

```
UIRoot (static)
├── PageRoot      → UIPage 挂载
├── WindowRoot    → UIWindow 挂载
└── WidgetRoot    → UIWidget 挂载

UIPanel (abstract)
├── UIPage     页面，全局唯一
├── UIWindow   窗口，可多开，压栈
└── UIWidget   挂件，不阻塞
```

## 相关文档

- **Lua View 实现层**: `../lua/view/LuaView.md` — 443 个 UIWindow + 103 个 UIWidget 的 Lua 实现
- **Lua 业务模块**: `../lua/_Nav__Lua.md` — 各模块通过 UIManager.OpenWindowAsync 打开 View

- 三类UI各有独立挂载根，互不干扰
- Window支持父子关系(parentWin)
- Page支持场景级导航(OpenPage/GoBackPage/EnterMainPage)
- UI资源统一从 `UI/` 路径加载

# UIPanel / UIPage / UIWindow / UIWidget

**命名空间：** `SGF.UI.Framework`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 继承体系

```
UIPanel (abstract)          基类
├── UIPage                  页面
├── UIWindow                窗口
└── UIWidget                挂件
```

## UIPanel (abstract)

| 成员 | 说明 |
|------|------|
| `UIType` (abstract) | 面板类型(Page/Window/Widget) |
| `Open(arg)` (abstract) | 打开面板 |
| `Close(arg)` (abstract) | 关闭面板 |
| `OnOpen(arg)` | 打开回调(子类重写) |
| `OnClose(arg)` | 关闭回调(子类重写) |
| `GetAtlas(atlas)` | 获取图集 |
| `ReleaseAtlas(atlas)` | 释放图集 |

## UIPage

- 全局唯一，通过PageRoot挂载
- 支持堆栈导航：OpenPage → GoBackPage → EnterMainPage
- 场景切换时自动管理页面栈

## UIWindow

- 可多开实例，通过WindowRoot挂载
- `onCloseDestroy = true` (默认关闭时销毁)
- 支持父子窗口关系(`parentWin`)
- `UIWindowStack` 管理窗口压栈弹栈
- `onClose` 回调委托

## UIWidget

- 不阻塞交互，通过WidgetRoot挂载
- `onCloseDestroy = false` (默认关闭时不销毁，可复用)
- 适用于常驻UI组件

## 枚举

- `E_UI_TYPE`: Panel=0, Widget=1, Window=2, Page=3
- `UISeatType`: None/Top/Down/Left/Right/Full
- `UIQueuePriorityType`: None=0, Normal=10, Tips=40, BlockingPlot=80

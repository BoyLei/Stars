# UIManager

**命名空间：** `SGF.UI.Framework`
**继承：** `ServiceModule<UIManager>` → `Module`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/UIManager.cs`

## 职责

UI 框架入口，管理所有 UI 面板的加载/卸载/生命周期。

## 核心方法

| 方法 | 说明 |
|------|------|
| `Init(uiResRoot)` | 初始化，设置UI资源根目录 |
| `Load<T>(name, parent, arg)` | 同步加载 UI 面板 |
| `UnLoad<T>(panel)` | 卸载 UI 面板 |
| `OpenPage(scene, page, arg)` | 打开页面(Page堆栈) |
| `GoBackPage()` | 返回上一页 |
| `EnterMainPage(type)` | 进入主界面 |
| `OnUIPanelClose(panel)` | 面板关闭回调 |

## 三类 UI

| 类型 | 根节点 | 特性 |
|------|--------|------|
| UIPage | UIRoot.PageRoot | 全局唯一，堆栈导航 |
| UIWindow | UIRoot.WindowRoot | 可多开，父子关系，压栈 |
| UIWidget | UIRoot.WidgetRoot | 不阻塞交互 |

## 关键设计

- 面板资源统一从 `UI/` 路径加载
- Page 堆栈支持场景级导航(OpenPage/GoBackPage)
- 3D模型UI支持(GetPlaneNodeFromOfflineData)

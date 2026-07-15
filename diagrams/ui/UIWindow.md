# UIWindow

**继承：** `UIWindow : UIPanel`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 职责

窗口，可多开，挂载在 WindowRoot，支持父子关系。

## 特性

- `UIType = E_UI_TYPE.Window`
- 可同时打开多个实例
- `onCloseDestroy = true`（默认关闭时销毁）
- 支持父子窗口关系（`parentWin`）
- `UIWindowStack` 管理窗口压栈弹栈
- `onClose` 回调委托

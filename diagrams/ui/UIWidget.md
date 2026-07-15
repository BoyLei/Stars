# UIWidget

**继承：** `UIWidget : UIPanel`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 职责

挂件，不阻塞交互，挂载在 WidgetRoot，可复用。

## 特性

- `UIType = E_UI_TYPE.Widget`
- 不独占焦点，不阻塞用户操作
- `onCloseDestroy = false`（默认关闭时不销毁）
- 适用于常驻 UI 组件（如红点、小地图、HUD）

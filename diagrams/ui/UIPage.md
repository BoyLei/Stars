# UIPage

**继承：** `UIPage : UIPanel`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 职责

页面，全局唯一，挂载在 PageRoot，支持堆栈导航。

## 特性

- `UIType = E_UI_TYPE.Page`
- 同一时间只存在一个实例
- 支持堆栈：OpenPage → GoBackPage → EnterMainPage
- 场景切换时自动管理页面栈

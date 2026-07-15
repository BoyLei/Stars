# UIWindowStack

**类型：** `static class`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 职责

窗口栈管理，维护 UIWindow 的打开/关闭顺序。

## 核心方法

| 方法 | 说明 |
|------|------|
| `pushWindow(win)` | 窗口入栈 |
| `popWindow()` | 窗口出栈 |
| `onClose(win)` | 窗口关闭时自动出栈 |

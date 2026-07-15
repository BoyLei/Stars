# UIQueueManager

**继承：** `ServiceModule<UIQueueManager>` → `Module`
**文件：** `Assets/Scripts/StarFramework/UI/Framework/`

## 职责

UI 优先级队列管理器，控制面板的排队打开顺序。

## 优先级

| 枚举值 | 说明 |
|--------|------|
| `None = 0` | 无优先级 |
| `Normal = 10` | 普通 |
| `Tips = 40` | 提示 |
| `BlockingPlot = 80` | 剧情阻塞（最高） |

## 核心方法

| 方法 | 说明 |
|------|------|
| `AddToQueue(panel)` | 加入队列 |
| `RemoveFromQueue(panel)` | 移出队列 |

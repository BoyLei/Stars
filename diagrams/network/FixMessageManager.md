# FixMessageManager

**继承：** `ServiceModule<FixMessageManager>` → `Module`
**文件：** `Assets/Scripts/StarFramework/Network/FixMessageManager.cs`

## 职责

消息分发管理器，将网络消息路由到对应的消息处理器。

## 核心方法

| 方法 | 说明 |
|------|------|
| `ProcessMessage(cmd, data)` | 处理消息，分发给对应的 MDMgr |
| `RegisterMDMgr(mgr)` | 注册消息分发器 |

## 管理的 MDMgr

- `ItemMDMgr` — 道具消息分发
- `HeroMDMgr` — 英雄消息分发
- `PartnerMDMgr` — 伙伴消息分发

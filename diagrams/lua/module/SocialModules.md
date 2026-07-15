# Lua 社交域模块

> Level 3 — 8 个社交模块，覆盖聊天/好友/公会/邮件/组队

---

## 1. ChatModule — 聊天系统
多频道聊天（世界/私聊/公会/队伍/喇叭），含 HUD 挂件和弹幕组件。私聊消息通过 `LuaModuleDispatcher` 转发到 FriendModule。
UI: `Chat/Prefab/ChatChannelWindow` / `Chat/Prefab/ChatInput`
特点: 黑名单过滤 — 发送前检查对方是否已在黑名单中。

**系统关系**:
Module: `LuaModule/ChatModule.lua.txt` (~593行)
  ↓ 同时管理 4 个 View:
View: `View/UIWindow/Chat/ChatChannelWindow.lua.txt` (~398行, 频道窗口)
  ├── `ChatInputWindow.lua.txt` (~240行, 输入窗口)
  ├── `ChatHudWidget.lua.txt` (~140行, HUD挂件)
  └── `ChatBarrageWidget.lua.txt` (~120行, 弹幕挂件)
子面板: 8个（ChatChannelWorld / EmojiSelect / BigEmojiSelect / EquipSelect / HistorySelect / ItemHudChat / ItemChatBarrage / SelectBtn）
Module→View: OnOpenChat → 按需打开4个View / 消息广播 OnChatMsgNoticeS → 所有View:AddMsgItem
View→Module: ChatInputWindow 关闭时 → SendMessage(FriendModule) 转发私聊
跨模块: FriendModule (私聊同步/黑名单检查)

## 2. FriendModule — 好友系统
好友添加/删除/黑名单/在线状态管理。所有操作通过 `Client2Third` 协议中转。每 10 秒定时拉取好友数据，缓存私聊消息和未读红点。
UI: `Friends/Prefab/FriendWindow` / `FriendApplyListWidget`
特点: 与 ChatModule 协作 — 接收私聊消息同步并触发红点更新。

**系统关系**:
Module: `LuaModule/FriendModule.lua.txt` (~1599行)
  ↓
View: `View/UIWindow/Friend/FriendWindow.lua.txt` (~224行)
  ├── `FriendApplyListWidget.lua.txt` (~600行, 好友申请列表)
  └── `FriendTipsWidget.lua.txt` (~230行, 好友提示)
子面板: 5个（FirendPanel ~33KB / AddfriendsPanel ~19KB / BlackListPanel / ItemFriendApply / SelectBtn）
Module→View: OnOpenFriendWindow → BindModule + InitData / RefreshBlackList / OnSyncMsgData
View→Module: 好友操作请求/黑名单/私聊转发
跨模块: ChatModule (私聊消息双向同步)

## 3. GuildModule — 公会（冒险团）
公会创建/加入/管理/公会战入口。根据玩家是否已加入公会，打开加入或信息界面。
UI: `Guild/GuildJoinWindow` / `Guild/GuildInfoWindow`
特点: 模块 Show/Hide 时启动/停止定时器（UpdateFun）。

**系统关系**:
Module: `LuaModule/GuildModule.lua.txt`
  ↓
View: `Guild/GuildJoinWindow` (加入界面) + `Guild/GuildInfoWindow` (信息界面)
Module→View: 根据玩家是否有公会 → 打开 JoinWindow 或 InfoWindow
View→Module: 创建公会/加入申请/退出/管理操作

## 4. MailModule — 邮件系统
系统邮件/玩家邮件查看、附件领取、一键删除已读、邮件排序。
UI: `Mail/Prefab/MailWidget` / `MailDeatailWidget`
特点: 邮件数据变更后自动刷新红点。

**系统关系**:
Module: `LuaModule/MailModule.lua.txt`
  ↓
View: `Mail/Prefab/MailWidget` (邮件列表) + `Mail/Prefab/MailDeatailWidget` (邮件详情)
Module→View: OnMailListRet → 刷新列表 / OnMailDetailRet → 刷新详情
View→Module: 阅读/删除/领取附件

## 5. TeamModule — 组队系统
队伍创建/加入/邀请/踢人/队长转让/跟随切场景。支持自动同意加入。
UI: `TeamWindow/Prefab/TeamWindow` / `TargetWindow` / `ApplyListWindow`
特点: `AtuoAgreeJoinTeam` 开关控制自动同意入队。

**系统关系**:
Module: `LuaModule/TeamModule.lua.txt`
  ↓
View: `TeamWindow/Prefab/TeamWindow` (主窗口) + `TargetWindow` (目标窗口)
  ├── `ApplyListWindow` (申请列表) + `TeamLobbyWindow` (组队大厅)
Module→View: OnTeamDataNotify → 刷新4个View / OnSyncTeamData → 更新队伍状态
View→Module: 邀请/踢人/转让队长/跟随切场景
跨模块: TaskModule (同步队伍数据给任务系统) / WantedModule (共享通缉任务)

## 6. DailyTeamModule — 日常组队
日常副本的自动组队匹配，与 TeamModule 独立解耦。
UI: 日常组队专用窗口
特点: 独立于 TeamModule，不共享队伍状态。

## 7. DonateModule — 公会捐献
公会捐献功能，最小的 Lua 模块。
UI: 捐献界面
特点: 仅 1 个消息 `OnOpenDonateWindow`。

## 8. PartyTimeModule — 公会篝火
公会挂机奖励/篝火派对玩法，挂机奖励定时同步。
UI: 派对时刻界面
特点: 挂机奖励通过定时协议同步。

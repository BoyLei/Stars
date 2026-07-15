# ProtoDic

**文件：** `Assets/Scripts/StarFramework/Network/ProtoDic.cs`

## 职责

Protobuf 协议字典，维护命令码到 Proto 消息类型的映射。

## 核心字段

- `protoMap` — `Dictionary<cmd, ProtoInfo>` 命令码到协议信息的映射

## 核心方法

| 方法 | 说明 |
|------|------|
| `GetProtoInfo(cmd)` | 获取协议信息 |
| `Register(cmd, info)` | 注册协议 |

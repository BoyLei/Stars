# ByteStream

**文件：** `Assets/Scripts/StarFramework/Network/ByteStream.cs`

## 职责

二进制流读写工具，用于 CustomMsg 路径的反射序列化。

## 核心方法

| 方法 | 说明 |
|------|------|
| `WriteInt(val)` | 写入 int |
| `WriteString(val)` | 写入 string |
| `ReadInt()` | 读取 int |
| `ReadString()` | 读取 string |
| `WriteBytes(data)` | 写入 byte[] |
| `ReadBytes(len)` | 读取 byte[] |

# ViewFactory

**类型：** `static class`
**文件：** `Assets/Scripts/StarGame/Game/Entity/Factory/ViewFactory.cs`

## 职责

显示实体工厂，负责 ViewObject 的创建/释放，维护 Entity ↔ View 的映射关系。

## 核心字段

- `m_mapEntityView` — Entity → View 的字典映射
- `m_Recycler` — ViewObject 回收池

## 核心方法

| 方法 | 说明 |
|------|------|
| `Init(viewRoot)` | 初始化工厂 |
| `CreateViewAsync(path, entity, parent, cb)` | 异步创建 View（Resources） |
| `CreateViewAddressables(path, entity, parent, cb)` | 异步创建 View（Addressables） |
| `ReleaseView(entity)` | 释放 View |
| `GetView(entity)` | 获取 Entity 对应的 View |

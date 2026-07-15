# EntityFactory

**命名空间：** `StarProject.Game.Entity.Factory`
**类型：** `public static class`
**文件：** `Assets/Scripts/StarGame/Game/Entity/Factory/EntityFactory.cs`

## 职责

实体对象的创建/释放/回收管理中心，配合 Recycler 对象池。

## 核心字段

- `m_listObject` — 所有存活实体列表
- `m_mapObject` — 实体ID → 实体对象字典
- `m_Recycler` — 实体回收池

## 核心方法

| 方法 | 说明 |
|------|------|
| `Init()` | 初始化工厂 |
| `InstanceEntity<T>()` | 创建实体实例(从池或new) |
| `ReleaseEntity(entity)` | 释放实体(回池) |
| `ClearReleasedObjects()` | 延迟清理已释放对象 |
| `GetEntity(id)` | 通过ID获取实体 |

## 附带工厂

| 工厂 | 说明 |
|------|------|
| ViewFactory | 创建ViewObject，维护Entity↔View字典映射 |
| SimpleDataFactory | 简单数据对象池 |
| DynamicDataFactory | 动态数据对象池 |

## 关键设计

- 泛型工厂 `InstanceEntity<T>()` 自动匹配类型
- Recycler 对象池减少GC
- ViewFactory 异步创建 View(CreateViewAsync/CreateViewAddressables)
- Entity 和 View 通过字典双向查找

# Entity 系统导航

> Level 2 — Entity 系统子模块划分。修改实体代码前先读本文件。

## 子模块列表

| 编号 | 子模块 | Level 3 详情 | 职责 |
|------|--------|-------------|------|
| D1 | EntityFactory | `EntityFactory.md` | 静态工厂 + Recycler对象池 |
| D2 | ViewFactory | `ViewFactory.md` | 显示工厂，Entity↔View映射 |
| D3 | SimpleDataFactory | `DataFactory.md` | 简单数据对象池 |
| D4 | DynamicDataFactory | `DataFactory.md` | 动态数据对象池 |
| D5 | EntityObject | `EntityHierarchy.md` | 逻辑实体基类 |
| D6 | EntityRemoteDynamic | `EntityHierarchy.md` | 网络同步实体(AOI) |
| D7 | EntityLocalDynamic | `EntityHierarchy.md` | 本地动态实体 |
| D8 | EntityLocalStatic | `EntityHierarchy.md` | 本地静态实体 |
| D9 | ViewObject | `ViewHierarchy.md` | 显示实体基类(MonoBehaviour) |
| D10 | ViewAOI | `ViewHierarchy.md` | AOI显示实体(带动画/模型) |
| D11 | ViewLocal | `ViewHierarchy.md` | 本地显示实体 |

## 实体继承链

```
EntityObject → RemoteDynamic → AOIEntityObject → NPCEntityBase
  → HeroEntityBase(含SkillComp/BuffComp/PassiveComp)
  → MonsterEntityBase/PartnerEntityBase/SummonEntityBase
  → BulletEntity/ObstacleBase
EntityObject → LocalDynamic → TreasureBox/Gateway/Wanted
EntityObject → LocalStatic / RemoteStatic

ViewObject → ViewModel → ViewAOI → ViewVitalHeroNormal等
ViewObject → ViewLocal → ViewLocalDynamic/ViewLocalStatic
```

## 关键设计

- 工厂 + 对象池，所有实体通过 EntityFactory 创建/回收
- 逻辑实体(EntityObject)和显示实体(ViewObject)分离，通过Dictionary映射
- 支持异步View创建(CreateViewAsync/CreateViewAddressables)

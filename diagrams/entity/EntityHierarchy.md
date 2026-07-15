# 实体继承体系

## 逻辑实体 (EntityObject)

```
EntityObject (abstract) — IRecyclableObject
├── EntityRemoteDynamic — 网络同步实体
│   └── AOIEntityObject — 带AOI视野管理(faction/IsShown)
│       ├── NPCEntityBase
│       │   ├── HeroEntityBase (含SkillComp/BuffComp/PassiveComp)
│       │   ├── MonsterEntityBase
│       │   ├── PartnerEntityBase
│       │   ├── SummonEntityBase
│       │   └── GameNPCEntityBase
│       ├── BulletEntity
│       ├── ObstacleBase
│       └── AuxiliarySummoner
├── EntityLocalDynamic — 本地动态实体
│   ├── TreasureBoxEntity
│   ├── GatewayEntity
│   ├── WantedEntity
│   └── LocalSummonEntity
├── EntityLocalStatic — 本地静态实体
└── EntityRemoteStatic — 远端静态实体
```

## 显示实体 (ViewObject)

```
ViewObject (abstract, MonoBehaviour) — IRecyclableObject
├── ViewModel
│   └── ViewAOI — AOI显示
│       ├── ViewVitalNPCNormal
│       ├── ViewVitalAnim
│       │   ├── ViewVitalHeroNormal
│       │   ├── ViewVitalMonsterAnim
│       │   ├── ViewVitalPartnerAnim
│       │   ├── ViewVitalSummonAnim
│       │   └── ViewVitalGameNPCAnim
├── ViewLocal
│   ├── ViewLocalDynamic
│   │   ├── TreasureBoxView
│   │   ├── GatewayView
│   │   ├── WantedView
│   │   └── SummonView
│   └── ViewLocalStatic
```

## 关键设计

- 逻辑与显示分离：EntityObject 处理数据/网络，ViewObject 处理表现
- EntityFactory ↔ ViewFactory 通过 Dictionary 映射
- ViewObject 继承 MonoBehaviour，挂载在场景中
- AOIEntityObject 自带视野管理(faction/IsShown/AOIEffect)

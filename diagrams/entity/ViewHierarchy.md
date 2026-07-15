# ViewObject 显示实体继承体系

**文件：** `Assets/Scripts/StarGame/Game/Entity/View/`

## 继承链

```
ViewObject (abstract, MonoBehaviour) — IRecyclableObject
├── ViewModel                    — 带动画/模型的实体显示
│   ├── (base functionality: animancer, modelPath)
│   └── ViewAOI                  — AOI 视野管理
│       ├── ViewVitalNPCNormal   — NPC 普通显示
│       └── ViewVitalAnim        — 带动画显示
│           ├── ViewVitalHeroNormal
│           ├── ViewVitalMonsterAnim
│           ├── ViewVitalPartnerAnim
│           ├── ViewVitalSummonAnim
│           └── ViewVitalGameNPCAnim
│
└── ViewLocal                    — 本地实体显示
    ├── ViewLocalDynamic         — 本地动态实体
    │   ├── TreasureBoxView
    │   ├── GatewayView
    │   ├── WantedView
    │   └── SummonView
    └── ViewLocalStatic          — 本地静态实体
```

## 关键设计

- 所有 ViewObject 继承 MonoBehaviour，挂载在场景中
- ViewFactory 维护 Entity ↔ View 的双向字典映射
- ViewModel 使用 Animancer 组件管理动画
- ViewAOI 支持显示/隐藏视野效果

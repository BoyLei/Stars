# Star 战斗层代码分析计划 (v5)

> **创建时间**: 2026-07-09  
> **状态**: 待执行  
> **v4→v5 变更**: 基于全量 `search_file` 扫描结果重写，修正 v4 的大量虚构文件和错误目录结构

> **代码验证状态（2026-07-11）**: 本文件仍是历史计划草案，不是最终事实报告。已用当前工作区验证，仍有缺失引用：`BlackBoard.cs`、`CollectCtrlGroup.cs`、`GameDebug.cs`、`GameDefine.cs`、`GameHelper.cs`、`GameInputManager.cs`、`HeroCtrlGroup.cs`、`HeroEntity.cs`、`IEntityFactory.cs`、`IEntityManager.cs`、`LocalCollectEntity.cs`、`MainHeroEntity.cs`、`PlayerManager.cs`。另外，后续 review 已确认 `EntityFactory` 不是单一 CreateXxx 分发口，`Recycler` API 是 `Pop`/`Push`/`Release`，L5 Map/TypeEffect/Camera 的若干方法名也已修正。后续架构说明应以已验证的 `BATTLE_*`、`FLOW_*`、`L*` 文档和 `reports/*-review.md` 为准。

---

## ⚠️ v4 → v5 重大修正说明

### v4 的严重错误

#### 🔴 错误1: 目录结构完全错误
| v4 假设 | 实际情况 |
|---------|---------|
| `Game/Buff/` 独立目录 | ❌ 实际是 `Game/Skill/Buff/` |
| `Game/Bullet/` 独立目录 | ❌ 实际是 `Game/Skill/Bullet/` |
| `Game/Passive/` 独立目录 | ❌ 实际是 `Game/Skill/Passive/` |
| `Game/AutoBattle/` 独立目录 | ❌ 实际是 `Game/Skill/AutoBattle/` |
| `Entity/Data/` 目录 (8文件) | ❌ **完全不存在** |
| `Entity/Object/` 目录 (18文件) | ❌ **完全不存在** |
| `Entity/Static/` 12文件 | ❌ 实际只有 1 个文件 |
| `Skill/TimelineBase/` | ❌ 实际是 `Skill/Base/` |
| `Skill/Partial/` | ❌ 实际是 `Skill/SkillPartial/` |

#### 🔴 错误2: 大量虚构文件（v4 列出但实际不存在）
- `BattleManager.cs` 路径归类错误（实际在 `Service/BattleManager/BattleManager.cs`）/ `GameInputManager.cs` ❌(实际是`GameInput.cs`)
- `GameDefine.cs` ❌ / `GameHelper.cs` ❌ / `GameDebug.cs` ❌
- **`EntityBaseData.cs` 路径归类错误**（实际在 `Game/Data/EntityBaseData.cs`，约 67KB）
- `IEntityFactory.cs` ❌ / `IEntityManager.cs` ❌
- `HeroCtrlGroup.cs` ❌ / `CollectCtrlGroup.cs` ❌ / `PartnerManager.cs` ❌ / `PlayerManager.cs` ❌
- `HeroEntity.cs` / `MainHeroEntity.cs` / `LocalCollectEntity.cs` 等多个 LocalDynamic 文件 ❌
- `BlackBoard.cs` ❌ (实际是 `SkillBlackBoard.cs`)

#### 🔴 错误3: 遗漏的真实文件（v4 未列出）
**Game/ 根目录遗漏 3 个**: `DynamicRenderQueueManager.cs`(33KB), `GameCommand.cs`, `GameEvent.cs`  
**Entity/Factory/ 遗漏 10 个**: `DynamicDataFactory.cs`(25KB), `ViewFactory.cs`(29KB), `Recycler.cs` 等  
**Entity/RemoteDynamic/ 遗漏 7 个**: `GameNPCEntityBase.cs`, `MonsterEntityBase.cs`, `SummonEntityBase.cs` 等  
**Entity/View/ 整个目录 38 文件**: v4 几乎没有详细列出  
**Skill/ 根目录遗漏 14 个**: `StageHandle.cs`(38KB), `TimeLineStageInfos.cs`(30KB), `SkillInfo.cs`(29KB) 等  
**Skill/SkillPartial/ 12 文件**: 含 3 个 >70KB 巨型文件，v4 完全未提及截断策略

#### 🔴 错误4: 新发现的巨型文件（v4 完全未标注）
| 文件 | 大小 | 位置 |
|------|------|------|
| `SkillControllerSkillPartial.cs` | **84.89 KB** | Skill/SkillPartial/ ← 比 SkillEntity 还大！ |
| `SkillEntityActionPartial.cs` | **76.07 KB** | Skill/SkillPartial/ |
| `SkillEntityUserInputPartial.cs` | **57.2 KB** | Skill/SkillPartial/ |
| `SkillControllerUserInputPartial.cs` | **42.87 KB** | Skill/SkillPartial/ |
| `ViewObjectNormal.cs` | 38.54 KB | Entity/View/OutLifeEntity/ |
| `ViewVitalAnim.cs` | 36.32 KB | Entity/View/VitalEnity/ |
| `SkillControllerEffectPartial.cs` | 35.81 KB | Skill/SkillPartial/ |
| `SceneJsonData.cs` | 36.26 KB | Map/ |
| `DynamicRenderQueueManager.cs` | 32.84 KB | Game/根 |
| + 20+ 个 20-32KB 文件 | | 各目录 |

---

## 一、实际代码范围（v5 全量扫描确认）

### 实际目录结构（200 个 CS 文件）

```
Assets/Scripts/StarGame/Game/                          # 7 文件
├── GameManager.cs                    [238.55 KB] 🔴★★★★★
├── GameInput.cs                      [28.36 KB]   ★☆
├── DynamicRenderQueueManager.cs      [32.84 KB]   ★☆
├── GameContext.cs                    [1.55 KB]
├── RenderManager.cs                  [7.38 KB]
├── GameCommand.cs                    [772 B]
└── GameEvent.cs                      [186 B]

Entity/                                                # 68 文件
├── Factory/                                           # 13 文件
│   ├── EntityFactory.cs              [10.64 KB]
│   ├── DynamicDataFactory.cs         [25.08 KB]   ★☆
│   ├── DynamicDataObject.cs          [3.86 KB]
│   ├── EntityLocalDynamic.cs         [11.26 KB]
│   ├── EntityLocalStatic.cs          [8.91 KB]
│   ├── EntityObject.cs               [7.93 KB]
│   ├── EntityRemoteDynamic.cs        [18.64 KB]
│   ├── EntityRemoteStatic.cs         [4.5 KB]
│   ├── Recycler.cs                   [4.78 KB]
│   ├── SimpleDataFactory.cs          [5.72 KB]
│   ├── SimpleDataObject.cs           [2.31 KB]
│   ├── ViewFactory.cs                [28.57 KB]   ★☆
│   └── ViewObject.cs                 [3.36 KB]
│
├── RemoteDynamic/                                     # 10 文件 (9根+1Base)
│   ├── NPCEntityBase.cs              [216.02 KB] 🔴★★★★★ (!!)
│   ├── HeroEntityBase.cs             [62.97 KB]   ★★★☆☆
│   ├── AOIEntityObject.cs (Base/)    [75.2 KB]    ★★★☆☆
│   ├── MonsterEntityBase.cs          [19.77 KB]
│   ├── SummonEntityBase.cs           [20.54 KB]
│   ├── PartnerEntityBase.cs          [14.9 KB]
│   ├── GameNPCEntityBase.cs          [12.93 KB]
│   ├── ObstacleBase.cs               [9.4 KB]
│   ├── BulletEntity.cs               [4.59 KB]
│   └── AuxiliarySummoner.cs          [538 B]
│
├── LocalDynamic/                                      # 6 文件 (4根+2Base)
│   ├── TreasureBoxEntity.cs          [16.66 KB]
│   ├── GatewayEntity.cs              [8.42 KB]
│   ├── WantedEntity.cs               [9.57 KB]
│   ├── LocalSummonEntity.cs          [2.86 KB]
│   ├── InteractiveShowEntity.cs (Base/) [775 B]
│   └── LocalSimulateEntity.cs (Base/)   [633 B]
│
├── Static/                                            # 1 文件
│   └── Stone.cs                      [314 B]
│
└── View/                                              # 38 文件
    ├── OutLifeEntity/                                 # 5 文件
    │   ├── ViewObjectNormal.cs       [38.54 KB]   ★☆
    │   ├── ViewVitalObjectAnim.cs    [7.91 KB]
    │   ├── OutLifeEntityViewBase.cs  [5.97 KB]
    │   ├── FxView.cs                 [1.18 KB]
    │   └── ViewInterActionObject.cs  [832 B]
    │
    ├── LocalDynamicEntity/                            # 8 文件 (5根+3Base)
    │   ├── ViewInterctive.cs         [9.78 KB]
    │   ├── SummonView.cs             [7.39 KB]
    │   ├── TreasureBoxView.cs        [1.69 KB]
    │   ├── WantedView.cs             [1.93 KB]
    │   ├── GatewayView.cs            [312 B]
    │   ├── ViewLocal.cs (Base/)      [26.01 KB]   ★☆
    │   ├── ViewLocalDynamic.cs (Base/) [2.32 KB]
    │   └── ViewLocalStatic.cs (Base/)  [2.49 KB]
    │
    └── VitalEnity/                                    # 25 文件
        ├── (根目录 15 文件)
        │   ├── ViewVitalAnim.cs      [36.32 KB]   ★☆
        │   ├── ViewVitalSummonAnim.cs [31.28 KB]
        │   ├── ViewVitalGameNPCAnim.cs [31.72 KB]
        │   ├── ViewVitalMonsterAnim.cs [30.78 KB]
        │   ├── ViewVitalPartnerAnim.cs [30.26 KB]
        │   ├── ViewVitalHeroNormal.cs [28.7 KB]
        │   ├── ViewVitalMonsterNormal.cs [9.52 KB]
        │   ├── ViewVitalSummonNormal.cs [2.43 KB]
        │   ├── ViewVitalGameNPCNormal.cs [1.06 KB]
        │   ├── ViewVitalPartnerNormal.cs [1.09 KB]
        │   ├── VVitalGlare.cs         [1.1 KB]
        │   ├── VVitalInk.cs           [1.7 KB]
        │   ├── 主角配角宝宝.cs         [323 B]
        │   ├── 普通变身时装.cs         [323 B]
        │   └── 真身幻象幻影.cs         [323 B]
        ├── Base/ (4 文件)
        │   ├── ViewAOI.cs             [145.25 KB] 🔴★★★★☆
        │   ├── ViewVitalNPCNormal.cs  [85.5 KB]   ★★★☆☆
        │   ├── ViewModel.cs           [6.36 KB]
        │   └── ClientSimulateMoveParam.cs [1.51 KB]
        ├── Example/ (1 文件)
        │   └── UpdateFrame.cs         [1.46 KB]
        └── ViewState/ (5 文件)
            ├── VitalState.cs          [22.18 KB]
            ├── AnimParam.cs           [7.81 KB]
            ├── AttackState.cs         [2.27 KB]
            ├── AttacState.cs          [2.26 KB]  # 注意：拼写如此
            └── I_VVitalAnim.cs        [405 B]

Player/                                                # 15 文件
├── (根目录 8 文件)
│   ├── PlayerCtrlGroup.cs            [61.92 KB]   ★★★☆☆
│   ├── EntityCtrlBase.cs             [29.84 KB]
│   ├── GameNPCCtrlGroup.cs           [20.44 KB]
│   ├── MonsterCtrlGroup.cs           [18.99 KB]
│   ├── PartnerCtrlGroup.cs           [17.74 KB]
│   ├── SummonCtrlGroup.cs            [15.95 KB]
│   ├── EntityCtrlMsgBase.cs          [9.31 KB]
│   └── BulletCtrl.cs                 [3.63 KB]
└── Component/ (7 文件)
    ├── SkillComponent.cs             [64.89 KB]   ★★★☆☆
    ├── UnitPendant.cs                [15.34 KB]
    ├── SkillIndicator.cs             [6.21 KB]
    ├── CheckIndicator.cs             [5.75 KB]
    ├── PCEnityAI.cs                  [4.58 KB]
    ├── EntityTempVisibility.cs       [1.39 KB]
    └── PlayerComponent.cs            [589 B]

Skill/                                                 # 59 文件
├── (根目录 20 文件)
│   ├── SkillEntity.cs                [83.07 KB]   ★★★★☆
│   ├── SkillStage.cs                 [73.19 KB]   ★★★★☆
│   ├── SkillContainer.cs             [56.53 KB]   ★★★☆☆
│   ├── StageHandle.cs                [37.79 KB]   ★☆
│   ├── TimeLineStageInfos.cs         [30.43 KB]
│   ├── SkillInfo.cs                  [29 KB]
│   ├── SkillController.cs            [26.11 KB]
│   ├── StageInfo.cs                  [25.89 KB]
│   ├── SkillStageFrame.cs            [22.97 KB]
│   ├── SkillUnitController.cs        [21.35 KB]
│   ├── SkillBlackBoard.cs            [19.88 KB]
│   ├── SkillEffectParam.cs           [18.18 KB]
│   ├── EffectUtils.cs                [78.73 KB]   ★★★★☆
│   ├── EffectResultUtils.cs          [12.85 KB]
│   ├── EffectExecuteResult.cs        [7.8 KB]
│   ├── PassiveSkillEntity.cs         [6.3 KB]
│   ├── SkillDispatcher.cs            [5 KB]
│   ├── DamageEntity.cs               [4.89 KB]
│   ├── SkillInputCache.cs            [3.73 KB]
│   └── SkillWheelInfo.cs             [2.43 KB]
│
├── Base/ (7 文件)
│   ├── ServerControlStageEntityBase.cs [26.41 KB]
│   ├── FxParam.cs                    [25.98 KB]
│   ├── TimeLineStage.cs              [3.33 KB]
│   ├── BaseConfigInfo.cs             [4.86 KB]
│   ├── TimeLineFrameEvent.cs         [4.04 KB]
│   ├── AudioParam.cs                 [1 KB]
│   └── StageSkipData.cs              [721 B]
│
├── SkillPartial/ (12 文件) ← v4 完全未标注的大文件集中地！
│   ├── SkillControllerSkillPartial.cs [84.89 KB] 🔴★★★★★ (!比SkillEntity还大!)
│   ├── SkillEntityActionPartial.cs   [76.07 KB] 🔴★★★★☆
│   ├── SkillEntityUserInputPartial.cs [57.2 KB]  ★★★☆☆
│   ├── SkillControllerUserInputPartial.cs [42.87 KB]
│   ├── SkillControllerEffectPartial.cs [35.81 KB]
│   ├── SkillEntityCfgPartial.cs      [13.04 KB]
│   ├── SkillControllerBuffPartial.cs [8.35 KB]
│   ├── SkillControllerMsgPartial.cs  [6.42 KB]
│   ├── SkillEntityDebugDataPartial.cs [5.73 KB]
│   ├── SkillControllerPassivePartial.cs [5.21 KB]
│   ├── SkillControllerBulletPartial.cs [5.38 KB]
│   └── SkillControllerBasePartial.cs [2.47 KB]
│
├── Utils/ (3 文件)
│   ├── SkillUtils.cs                 [28.52 KB]
│   ├── SkillMsgUtils.cs              [1.94 KB]
│   └── FxUtils.cs                    [668 B]
│
├── Power/ (6 文件)
│   ├── ServerPowerData.cs            [8.03 KB]
│   ├── PowerBigDataModule.cs         [4.15 KB]
│   ├── PowerAllData.cs               [3.73 KB]
│   ├── CustomGradient.cs             [3.5 KB]
│   ├── PowerSubDataModule.cs         [3.12 KB]
│   └── PowerDataModule.cs            [2.12 KB]
│
├── ClientEffect/ (1 文件)
│   └── ClientMoveFx.cs               [4.02 KB]
│
├── Buff/ (3 文件)
│   ├── SkillBuff.cs                  [18.42 KB]
│   ├── BuffInfo.cs                   [5 KB]
│   └── BuffStageHandle.cs            [2.18 KB]
│
├── Bullet/ (3 文件)
│   ├── SkillBullet.cs                [24.81 KB]
│   ├── BulletInfo.cs                 [5.5 KB]
│   └── BulletStageHandle.cs          [1.76 KB]
│
├── Passive/ (2 文件)
│   ├── PassiveInfo.cs                [4.29 KB]
│   └── PassiveStageHandle.cs         [2.36 KB]
│
└── AutoBattle/ (2 文件)
    ├── AutoBattleBtn.cs              [3.47 KB]
    └── SwitchEnemyBtn.cs             [626 B]

Map/                                                   # 7 文件
├── SceneJsonData.cs                  [36.26 KB]
├── GameMap.cs                        [21.7 KB]
├── SceneObstacleLogic.cs             [10.97 KB]
├── SceneAreaLogic.cs                 [6.38 KB]
├── ActiveSceneCameraLogic.cs         [4.62 KB]
├── MapScript.cs                      [1.45 KB]
└── IMapLogic.cs                      [753 B]

SnapShot/                                              # 7 文件
├── SerSnapshotSeqManager.cs          [20.91 KB]
├── SnapShotUtils.cs                  [8.81 KB]
├── ServiceSnapshotData.cs            [2.22 KB]
├── SerNttAOIOneFrameSnapALLDataTHD.cs [1.35 KB]
├── SerRPCOneFrameSnapALLData.cs      [539 B]
├── SerNttAOIOneFrameSnapALLDataMainPlayer.cs [672 B]
└── SerMessageSnapData.cs             [500 B]

StarsCamera/                                          # 6 文件
├── GameCamera.cs                     [16.94 KB]
├── GameCameraFeel.cs                 [12.47 KB]
├── GameCameraScale.cs                [10.38 KB]
├── GameCameraRotate.cs               [3.37 KB]
├── UICamera.cs                       [5.63 KB]
└── RawCamera.cs                      [2.7 KB]

TypeEffect/                                           # 26 文件
├── (根目录 7 文件)
│   ├── BaseTypeEffect.cs, CameraBaseTypeEffect.cs, CameraMoveTypeEffect.cs
│   ├── ScreenTouchEffect.cs, Touch2UseSkillEffect.cs
│   ├── TriggerTypeEffectData.cs, TriggleEventUtils.cs
└── TypeEffect/ (19 文件)
    ├── TypeEffectFactory.cs, SimulateSummonEffect.cs [5.49 KB]
    ├── HiddenUIEffect.cs [3.22 KB], FreezeBuffEffect.cs [3.04 KB]
    ├── InvisibleBuffEffect.cs, KnockDownBuffEffect.cs, TranslucentEffect.cs
    ├── ShadowFollowBuffEffect.cs, ChangeAnimsBuffEffect.cs, ParalysisBuffEffect.cs
    ├── BuffPlayLineRenderEffect.cs, HiddenSkillSlotEffect.cs
    ├── AvatarChangeEffect.cs, ChangSkillEffect.cs, SpectralChangeEffect.cs
    ├── SpectralSkillBuffEffect.cs, ShaderChange2.cs, TestBuff.cs, ChangeAnimsData.cs

CustomDataStruct/                                     # 2 文件
├── CusListQueue.cs                   [1.15 KB]
└── CusQueue.cs                       [1.46 KB]

SpecialUtilComp/                                      # 2 文件
├── StarShadowFollow.cs               [23.3 KB]
└── StarShadowMirror.cs               [22.61 KB]

ViewEffect/                                           # 1 文件
└── Footprints.cs                     [1.08 KB]
```

**总计：200 个 CS 文件**（v4 说的 245 是错误的）

---

## 二、Agent 分工方案（v5：8 路并行）

> v4 的 7 路方案中 Agent-4 [skill] 承载 49 文件 + 6 个 >50KB 巨型文件，负载过重。
> v5 将 SkillPartial (12文件含3个>70KB巨型文件) 拆分为独立 Agent-4b。

| Agent ID | 名称 | 层级 | 文件数 | 巨型文件(>50KB) | 核心职责 |
|----------|------|------|--------|----------------|---------|
| **Agent-1** | entry | L0 | 7 | 1 (GameManager 239KB) | 游戏主循环/上下文/输入/渲染管理 |
| **Agent-2a** | entity-factory | L1 | 13+ | 0 | Entity/Factory 13 文件 + Agent 分工中的本地/静态实体口径 |
| **Agent-2b** | entity-runtime | L1 | 48 | 5 (NPC 216KB, ViewAOI 145KB, ViewVitalNPC 85KB, AOIEntity 75KB, Hero 63KB) | 远程实体/AOI/View渲染层38文件 |
| **Agent-3** | player | L2 | 15 | 2 (SkillComp 65KB, PlayerCtrl 62KB) | 控制组8文件/组件模式7文件 |
| **Agent-4a** | skill-core | L3 | 37 | 3 (SkillEntity 83KB, SkillStage 73KB, EffectUtils 79KB) | 引擎核心20文件+Base7+Power6+Utils3+ClientEffect1 |
| **Agent-4b** | skill-partial | L3 | 12 | 4 (ControllerSkill 85KB!!, EntityAction 76KB, EntityUserInput 57KB, ControllerUserInput 43KB) | 分部类扩展12文件 |
| **Agent-5** | effect | L4 | 10 | 1 (SkillBullet 25KB) | Buff3+Bullet3+Passive2+AutoBattle2 |
| **Agent-6** | support | L5 | 51 | 0 | Map7+SnapShot7+Camera6+TypeEffect26+DataStruct2+Special2+ViewEffect1 |

**总计：200 文件，8 路并行**

### Agent 边界精确定义

#### Agent-1 [entry] — 7 文件
```
Game/GameManager.cs              [238.55 KB] 🔴
Game/GameInput.cs                [28.36 KB]
Game/DynamicRenderQueueManager.cs [32.84 KB]
Game/GameContext.cs
Game/RenderManager.cs
Game/GameCommand.cs
Game/GameEvent.cs
```

#### Agent-2a [entity-factory] — 20 文件
```
Entity/Factory/ (13 文件)
  EntityFactory.cs, DynamicDataFactory.cs [25KB], DynamicDataObject.cs,
  EntityLocalDynamic.cs, EntityLocalStatic.cs, EntityObject.cs,
  EntityRemoteDynamic.cs [19KB], EntityRemoteStatic.cs, Recycler.cs,
  SimpleDataFactory.cs, SimpleDataObject.cs, ViewFactory.cs [29KB], ViewObject.cs

Entity/LocalDynamic/ (4 文件)
  TreasureBoxEntity.cs [17KB], GatewayEntity.cs, WantedEntity.cs, LocalSummonEntity.cs

Entity/LocalDynamic/Base/ (2 文件)
  InteractiveShowEntity.cs, LocalSimulateEntity.cs

Entity/Static/ (1 文件)
  Stone.cs
```

#### Agent-2b [entity-runtime] — 48 文件
```
Entity/RemoteDynamic/ (9 文件)
  NPCEntityBase.cs [216KB] 🔴, HeroEntityBase.cs [63KB], MonsterEntityBase.cs [20KB],
  SummonEntityBase.cs [21KB], PartnerEntityBase.cs [15KB], GameNPCEntityBase.cs [13KB],
  ObstacleBase.cs, BulletEntity.cs, AuxiliarySummoner.cs

Entity/RemoteDynamic/Base/ (1 文件)
  AOIEntityObject.cs [75KB]

Entity/View/OutLifeEntity/ (5 文件)
  ViewObjectNormal.cs [39KB], ViewVitalObjectAnim.cs, OutLifeEntityViewBase.cs, FxView.cs, ViewInterActionObject.cs

Entity/View/LocalDynamicEntity/ (5 文件)
  ViewInterctive.cs, SummonView.cs, TreasureBoxView.cs, WantedView.cs, GatewayView.cs

Entity/View/LocalDynamicEntity/Base/ (3 文件)
  ViewLocal.cs [26KB], ViewLocalDynamic.cs, ViewLocalStatic.cs

Entity/View/VitalEnity/ (15 文件)
  ViewVitalAnim.cs [36KB], ViewVitalGameNPCAnim.cs [32KB], ViewVitalSummonAnim.cs [31KB],
  ViewVitalMonsterAnim.cs [31KB], ViewVitalPartnerAnim.cs [30KB], ViewVitalHeroNormal.cs [29KB],
  ViewVitalMonsterNormal.cs, ViewVitalSummonNormal.cs, ViewVitalGameNPCNormal.cs,
  ViewVitalPartnerNormal.cs, VVitalGlare.cs, VVitalInk.cs,
  主角配角宝宝.cs, 普通变身时装.cs, 真身幻象幻影.cs

Entity/View/VitalEnity/Base/ (4 文件)
  ViewAOI.cs [145KB] 🔴, ViewVitalNPCNormal.cs [86KB], ViewModel.cs, ClientSimulateMoveParam.cs

Entity/View/VitalEnity/Example/ (1 文件)
  UpdateFrame.cs

Entity/View/VitalEnity/ViewState/ (5 文件)
  VitalState.cs [22KB], AnimParam.cs, AttackState.cs, AttacState.cs, I_VVitalAnim.cs
```

#### Agent-3 [player] — 15 文件
```
Player/ (8 文件)
  PlayerCtrlGroup.cs [62KB], EntityCtrlBase.cs [30KB], GameNPCCtrlGroup.cs [20KB],
  MonsterCtrlGroup.cs [19KB], PartnerCtrlGroup.cs [18KB], SummonCtrlGroup.cs [16KB],
  EntityCtrlMsgBase.cs, BulletCtrl.cs

Player/Component/ (7 文件)
  SkillComponent.cs [65KB], UnitPendant.cs [15KB], SkillIndicator.cs,
  CheckIndicator.cs, PCEnityAI.cs, EntityTempVisibility.cs, PlayerComponent.cs
```

#### Agent-4a [skill-core] — 37 文件
```
Skill/ (20 文件 - 根目录)
  SkillEntity.cs [83KB] 🔴, EffectUtils.cs [79KB] 🔴, SkillStage.cs [73KB] 🔴,
  SkillContainer.cs [57KB], StageHandle.cs [38KB], TimeLineStageInfos.cs [30KB],
  SkillInfo.cs [29KB], SkillController.cs [26KB], StageInfo.cs [26KB],
  SkillStageFrame.cs [23KB], SkillUnitController.cs [21KB],
  SkillBlackBoard.cs [20KB], SkillEffectParam.cs [18KB],
  EffectResultUtils.cs, EffectExecuteResult.cs, PassiveSkillEntity.cs,
  SkillDispatcher.cs, DamageEntity.cs, SkillInputCache.cs, SkillWheelInfo.cs

Skill/Base/ (7 文件)
  ServerControlStageEntityBase.cs [26KB], FxParam.cs [26KB],
  TimeLineStage.cs, BaseConfigInfo.cs, TimeLineFrameEvent.cs, AudioParam.cs, StageSkipData.cs

Skill/Utils/ (3 文件)
  SkillUtils.cs [29KB], SkillMsgUtils.cs, FxUtils.cs

Skill/Power/ (6 文件)
  ServerPowerData.cs, PowerBigDataModule.cs, PowerAllData.cs,
  CustomGradient.cs, PowerSubDataModule.cs, PowerDataModule.cs

Skill/ClientEffect/ (1 文件)
  ClientMoveFx.cs
```

#### Agent-4b [skill-partial] — 12 文件
```
Skill/SkillPartial/ (12 文件)
  SkillControllerSkillPartial.cs [84.89KB] 🔴★★★★★ (!最大文件!)
  SkillEntityActionPartial.cs [76.07KB] 🔴
  SkillEntityUserInputPartial.cs [57.2KB]
  SkillControllerUserInputPartial.cs [42.87KB]
  SkillControllerEffectPartial.cs [35.81KB]
  SkillEntityCfgPartial.cs [13.04KB]
  SkillControllerBuffPartial.cs [8.35KB]
  SkillControllerMsgPartial.cs [6.42KB]
  SkillControllerBulletPartial.cs [5.38KB]
  SkillEntityDebugDataPartial.cs [5.73KB]
  SkillControllerPassivePartial.cs [5.21KB]
  SkillControllerBasePartial.cs [2.47KB]
```

#### Agent-5 [effect] — 10 文件
```
Skill/Buff/ (3 文件)
  SkillBuff.cs [18KB], BuffInfo.cs, BuffStageHandle.cs

Skill/Bullet/ (3 文件)
  SkillBullet.cs [25KB], BulletInfo.cs, BulletStageHandle.cs

Skill/Passive/ (2 文件)
  PassiveInfo.cs, PassiveStageHandle.cs

Skill/AutoBattle/ (2 文件)
  AutoBattleBtn.cs, SwitchEnemyBtn.cs
```

#### Agent-6 [support] — 51 文件
```
Map/ (7 文件)
  SceneJsonData.cs [36KB], GameMap.cs [22KB], SceneObstacleLogic.cs,
  SceneAreaLogic.cs, ActiveSceneCameraLogic.cs, MapScript.cs, IMapLogic.cs

SnapShot/ (7 文件)
  SerSnapshotSeqManager.cs [21KB], SnapShotUtils.cs, ServiceSnapshotData.cs,
  SerNttAOIOneFrameSnapALLDataTHD.cs, SerRPCOneFrameSnapALLData.cs,
  SerNttAOIOneFrameSnapALLDataMainPlayer.cs, SerMessageSnapData.cs

StarsCamera/ (6 文件)
  GameCamera.cs [17KB], GameCameraFeel.cs [12KB], GameCameraScale.cs,
  GameCameraRotate.cs, UICamera.cs, RawCamera.cs

TypeEffect/ (26 文件)
  (根 7 + TypeEffect/子目录 19)

CustomDataStruct/ (2 文件)
  CusListQueue.cs, CusQueue.cs

SpecialUtilComp/ (2 文件)
  StarShadowFollow.cs [23KB], StarShadowMirror.cs [23KB]

ViewEffect/ (1 文件)
  Footprints.cs
```

---

## 三、大文件截断策略（v5 完整清单）

### 🔴 极端截断（>100KB，只读类名+方法列表）
| 文件 | 大小 | Agent |
|------|------|-------|
| `GameManager.cs` | 238.55 KB | 1 |
| `NPCEntityBase.cs` | 216.02 KB | 2b |
| `ViewAOI.cs` | 145.25 KB | 2b |

### 🟠 重度截断（50-100KB，只读类签名+核心方法+调用关系）
| 文件 | 大小 | Agent |
|------|------|-------|
| `SkillControllerSkillPartial.cs` | 84.89 KB | 4b |
| `SkillEntity.cs` | 83.07 KB | 4a |
| `EffectUtils.cs` | 78.73 KB | 4a |
| `SkillEntityActionPartial.cs` | 76.07 KB | 4b |
| `AOIEntityObject.cs` | 75.2 KB | 2b |
| `SkillStage.cs` | 73.19 KB | 4a |
| `ViewVitalNPCNormal.cs` | 85.5 KB | 2b |
| `SkillComponent.cs` | 64.89 KB | 3 |
| `HeroEntityBase.cs` | 62.97 KB | 2b |
| `PlayerCtrlGroup.cs` | 61.92 KB | 3 |
| `SkillContainer.cs` | 56.53 KB | 4a |
| `SkillEntityUserInputPartial.cs` | 57.2 KB | 4b |

### 🟡 中度截断（30-50KB，读类签名+关键方法实现）
| 文件 | 大小 | Agent |
|------|------|-------|
| `SkillControllerUserInputPartial.cs` | 42.87 KB | 4b |
| `StageHandle.cs` | 37.79 KB | 4a |
| `ViewObjectNormal.cs` | 38.54 KB | 2b |
| `SceneJsonData.cs` | 36.26 KB | 6 |
| `ViewVitalAnim.cs` | 36.32 KB | 2b |
| `SkillControllerEffectPartial.cs` | 35.81 KB | 4b |
| `DynamicRenderQueueManager.cs` | 32.84 KB | 1 |
| `ViewVitalGameNPCAnim.cs` | 31.72 KB | 2b |
| `ViewVitalSummonAnim.cs` | 31.28 KB | 2b |
| `ViewVitalMonsterAnim.cs` | 30.78 KB | 2b |
| `ViewVitalPartnerAnim.cs` | 30.26 KB | 2b |
| `TimeLineStageInfos.cs` | 30.43 KB | 4a |
| `GameInput.cs` | 28.36 KB | 1 |
| `ViewFactory.cs` | 28.57 KB | 2a |
| `ViewVitalHeroNormal.cs` | 28.7 KB | 2b |
| `SkillUtils.cs` | 28.52 KB | 4a |
| `EntityCtrlBase.cs` | 29.84 KB | 3 |
| `SkillInfo.cs` | 29 KB | 4a |
| `DynamicDataFactory.cs` | 25.08 KB | 2a |
| `StageInfo.cs` | 25.89 KB | 4a |
| `ServerControlStageEntityBase.cs` | 26.41 KB | 4a |
| `FxParam.cs` | 25.98 KB | 4a |
| `SkillController.cs` | 26.11 KB | 4a |
| `ViewLocal.cs` | 26.01 KB | 2b |
| `SkillBullet.cs` | 24.81 KB | 5 |
| `StarShadowFollow.cs` | 23.3 KB | 6 |
| `StarShadowMirror.cs` | 22.61 KB | 6 |
| `SkillStageFrame.cs` | 22.97 KB | 4a |
| `GameMap.cs` | 21.7 KB | 6 |
| `SerSnapshotSeqManager.cs` | 20.91 KB | 6 |
| `SummonEntityBase.cs` | 20.54 KB | 2b |
| `SkillBlackBoard.cs` | 19.88 KB | 4a |
| `MonsterEntityBase.cs` | 19.77 KB | 2b |
| `SkillEffectParam.cs` | 18.18 KB | 4a |

**总计：37 个大文件需截断策略**（v4 只标注了 14 个，严重不足！）

---

## 四、输出规格（方案 C：三套图体系）

### 输出文件清单（共 20 个）

```
diagrams/battle/
├── _Nav_Battle.md                    # 导航索引页
├── BATTLE_OVERVIEW.md                # 图1: 全景模块关系图 (flowchart)
├── L0_ENTRY_LAYER.md                 # 图2: L0 入口调度层
├── L1_ENTITY_FACTORY_LAYER.md        # 图3: L1 实体工厂层
├── L1_ENTITY_RUNTIME_LAYER.md        # 图4: L1 实体运行时层
├── L2_CONTROL_LAYER.md               # 图5: L2 角色控制层
├── L3A_SKILL_ENGINE_CORE.md          # 图6: L3 技能引擎核心 [v5拆分]
├── L3B_SKILL_PARTIAL_EXT.md          # 图7: L3 技能分部类扩展 [v5新增]
├── L4_COMBAT_EFFECT.md               # 图8: L4 战斗效果层
├── L5_SUPPORT_SYS.md                 # 图9: L5 支撑系统层
├── FLOW_SKILL_RELEASE.md             # 图10: 技能释放时序图
├── FLOW_DAMAGE_PIPELINE.md           # 图11: 伤害结算管线时序图
├── FLOW_BUFF_LIFECYCLE.md            # 图12: Buff生命周期时序图
└── reports/
    ├── agent-entry.md                # Agent-1
    ├── agent-entity-factory.md       # Agent-2a
    ├── agent-entity-runtime.md       # Agent-2b
    ├── agent-player.md               # Agent-3
    ├── agent-skill-core.md           # Agent-4a [v5新增]
    ├── agent-skill-partial.md        # Agent-4b [v5新增]
    ├── agent-effect.md               # Agent-5
    └── agent-support.md              # Agent-6
```

---

## 五、各 Agent 执行指令

### 通用规则（所有 Agent）

```
【读取规则】
1. 只读取本任务分配的文件，不越界
2. 大文件截断策略：
   - >100KB: 只读 class 继承链 + 方法名列表（不读任何实现体）
   - 50-100KB: 读类签名 + 前10个核心方法签名 + 被外部调用的public方法
   - 30-50KB: 读类签名 + 核心方法实现（限前30行）
   - <30KB: 全文阅读
3. 标记未读内容为 [实现未读]

【输出规则】
输出到 reports/agent-{name}.md，格式：
## 文件清单（文件名 | 行数 | 职责一句话）
## 类图（Mermaid classDiagram）
## 核心调用链（Mermaid sequenceDiagram，3-5条）
## 关键发现（疑问标注 [待确认]）
## 对外接口（public API 列表）

禁止推测！语义不清标注 [语义不清] 并附上下文片段。
```

### 各 Agent 重点问题

#### Agent-1 [entry]
- [ ] GameManager.Update 的战斗调用顺序？
- [ ] GameInput 如何分发输入到 PlayerCtrlGroup？
- [ ] DynamicRenderQueueManager 的职责？（v4完全遗漏的33KB文件）

#### Agent-2a [entity-factory]
- [ ] EntityFactory 的三条创建链（DynamicData/SimpleData/View）如何分发？
- [ ] DynamicDataFactory vs SimpleDataFactory vs ViewFactory 的使用场景？
- [ ] Recycler 的对象池策略？

#### Agent-2b [entity-runtime]
- [ ] NPCEntityBase 216KB 包含什么？（方法数量统计）
- [ ] AOI 三层架构：AOIEntityObject → ViewAOI → ViewVitalNPCNormal 的数据流
- [ ] View 层 38 文件的组织逻辑？（Anim vs Normal vs ViewState）

#### Agent-3 [player]
- [ ] PlayerCtrlGroup 如何组合 7 个 Component？
- [ ] EntityCtrlBase 的多态调度机制？
- [ ] SkillComponent 65KB 是 SkillContainer 的代理还是独立逻辑？

#### Agent-4a [skill-core]
- [ ] Timeline 管线：TimeLineStage → StageHandle → EffectExecuteResult → EffectUtils
- [ ] SkillEntity vs SkillStage vs SkillContainer 三者关系
- [ ] EffectUtils 79KB 是如何分发到 Buff/Bullet/Passive 的？

#### Agent-4b [skill-partial] ← v5 新增
- [ ] SkillControllerSkillPartial 85KB 包含什么？（比 SkillEntity 还大！）
- [ ] SkillEntityActionPartial 76KB 的动作逻辑
- [ ] 12 个 Partial 文件分别扩展哪个类的哪部分功能？

#### Agent-5 [effect]
- [ ] Buff/Bullet/Passive 的统一抽象接口？
- [ ] SkillBuff 18KB vs SkillBullet 25KB 的结构差异
- [ ] AutoBattle 的决策入口

#### Agent-6 [support]
- [ ] TypeEffect 26 文件如何通过 TypeEffectFactory 分发？
- [ ] SnapShot 帧同步的序列化格式
- [ ] StarShadowFollow/Mirror 的阴影实现

---

## 六、汇总阶段

1. **冲突解决**：交叉验证 8 份报告的接口一致性
2. **图表生成**：12 张 Mermaid 图 + 导航页
3. **质量检查**：
   - [ ] 文件覆盖率 = 200/200
   - [ ] 大文件截断执行率 = 37/37
   - [ ] Mermaid 语法检查
   - [ ] 跨层接口一致性

---

## 七、执行时间估算

| Agent | 文件数 | 大文件数 | 预估时间 |
|-------|--------|---------|---------|
| Agent-1 entry | 7 | 3 | ~15 min |
| Agent-2a entity-factory | 20 | 2 | ~20 min |
| Agent-2b entity-runtime | 48 | 12 | ~45 min |
| Agent-3 player | 15 | 4 | ~25 min |
| Agent-4a skill-core | 37 | 12 | ~40 min |
| Agent-4b skill-partial | 12 | 4 | ~30 min |
| Agent-5 effect | 10 | 1 | ~10 min |
| Agent-6 support | 51 | 5 | ~30 min |
| **汇总** | - | - | **~25 min** |
| **总计** | **200** | **37** | **~240 min (4h)** |

---

## 八、版本历史

| 版本 | 日期 | 变更 |
|------|------|------|
| v1 | 2026-07-09 | 初版 6-Agent |
| v2 | 2026-07-09 | 方案 C 三套图 |
| v3 | 2026-07-09 | 补 25 遗漏文件 |
| v4 | 2026-07-09 | 拆分 entity，补 5 文件（但含大量虚构文件和错误目录结构） |
| **v5** | **2026-07-09** | **基于全量 search_file 扫描重写。修正：① 目录结构错误(Buff/Bullet等在Skill下而非Game下) ② 删除22个虚构文件 ③ 补全40+遗漏文件 ④ 新增Agent-4b处理SkillPartial ⑤ 大文件清单从14个修正为37个 ⑥ 总文件数从错误的245修正为200** |

---

**下一步：用户确认 Plan v5 后，调用 8 个 Task tool 并行启动 SubAgent。**

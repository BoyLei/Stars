# Star 战斗层全景模块关系图 (BATTLE_OVERVIEW)

> 基于 8 个 SubAgent 分析报告的汇总。代码根：`Assets/Scripts/StarGame/Game/`，共 200 个 CS 文件，分 6 层。

```mermaid
flowchart TB
    subgraph L0["L0 入口调度层 (Agent-1, 7文件)"]
        GM[GameManager<br/>238KB 帧循环总驱动]
        GIN[GameInput<br/>28KB 输入分发]
        DRQ[DynamicRenderQueueManager<br/>33KB 渲染队列规划]
        GCX[GameContext<br/>共享数据]
        RM[RenderManager]
    end

    subgraph L1F["L1a 实体工厂层 (Agent-2a, 20文件)"]
        EF[EntityFactory]
        DDF[DynamicDataFactory]
        SDF[SimpleDataFactory]
        VF[ViewFactory]
        RC[Recycler 对象池]
    end

    subgraph L1R["L1b 实体运行时层 (Agent-2b, 48文件)"]
        NPC[NPCEntityBase 216KB]
        AOI[AOIEntityObject 75KB]
        HERO[HeroEntityBase 63KB]
        VIEW[ViewAOI 145KB + ViewVitalNPCNormal 86KB]
    end

    subgraph L2["L2 角色控制层 (Agent-3, 15文件)"]
        PCG[PlayerCtrlGroup 62KB]
        ECB[EntityCtrlBase]
        SC[SkillComponent 65KB]
        NPCG[GameNPCCtrlGroup]
        MCG[MonsterCtrlGroup]
    end

    subgraph L3C["L3a 技能引擎核心 (Agent-4a, 37文件)"]
        SD[SkillDispatcher]
        SC2[SkillController]
        SUC[SkillUnitController]
        SE[SkillEntity 83KB]
        SS[SkillStage 73KB]
        SH[StageHandle]
        EU[EffectUtils 79KB]
        SB[SkillBlackBoard]
    end

    subgraph L3P["L3b 技能分部类 (Agent-4b, 12文件)"]
        SCS[SkillControllerSkillPartial 85KB]
        SEA[SkillEntityActionPartial 76KB]
        SUI[SkillEntityUserInputPartial 57KB]
    end

    subgraph L4["L4 战斗效果层 (Agent-5, 10文件)"]
        BUFF[SkillBuff 18KB]
        BULLET[SkillBullet 25KB]
        PASS[PassiveInfo]
        AB[AutoBattle]
    end

    subgraph L5["L5 支撑系统层 (Agent-6, 51文件)"]
        MAP[GameMap+SceneJsonData]
        SNAP[SnapShot 序列化]
        CAM[StarsCamera]
        TE[TypeEffect 26文件]
        CDS[CusQueue/CusListQueue]
        SHADOW[StarShadowFollow/Mirror]
        VE[Footprints]
    end

    GM --> GCX & GIN & DRQ & RM
    GM --> EF
    EF --> DDF & SDF & VF & RC
    DDF --> L1R
    VF --> L1R
    L1R --> L2
    GM --> PCG
    PCG --> ECB & SC
    ECB --> NPCG & MCG
    GIN --> PCG

    GM --> SD
    SD --> SC2 & SUC
    SC2 --> SE & SS & BUFF & BULLET & PASS
    SUC --> SE
    SS --> SH
    SH --> EU
    EU --> BUFF & BULLET & PASS
    SC2 --> SB
    SS --> SB

    SC --> SD
    SC --> SE

    L3P -.->|partial 扩展| SC2
    L3P -.->|partial 扩展| SE

    L4 --> L5
    L2 --> L5
    L1R --> L5
    L3C --> L5
```

## 各层职责速览

| 层 | Agent | 文件数 | 核心职责 | 巨型文件 |
|----|-------|--------|---------|---------|
| L0 入口调度 | entry | 7 | 帧循环总驱动、输入分发、渲染队列规划 | GameManager 239KB |
| L1a 工厂 | entity-factory | 20 | 三条创建链（DynamicData/SimpleData/View）+ 对象池 | ViewFactory 29KB |
| L1b 运行时 | entity-runtime | 48 | 远程实体/AOI/View 渲染表现 | NPCEntityBase 216KB, ViewAOI 145KB |
| L2 控制 | player | 15 | 控制组家族 + 组件模式 | SkillComponent 65KB, PlayerCtrlGroup 62KB |
| L3a 引擎 | skill-core | 37 | Timeline 驱动技能管线 | SkillEntity 83KB, EffectUtils 79KB |
| L3b 分部 | skill-partial | 12 | partial 类功能扩展 | SkillControllerSkillPartial 85KB |
| L4 效果 | effect | 10 | Buff/Bullet/Passive/AutoBattle | SkillBullet 25KB |
| L5 支撑 | support | 51 | 地图/快照/相机/特效/数据结构 | SceneJsonData 36KB |

## 关键架构洞察

1. **Timeline 驱动主轴**：`SkillStage → StageHandle → EffectExecuteResult → EffectUtils` 是整条技能 pipeline 核心，EffectUtils 是 L3→L4 的唯一效果落地入口。
2. **工厂+对象池模式**：EntityFactory 按 m_nType 分派三条创建链，Recycler 实现 IRecyclableObject 复用。
3. **AOI 三层裁剪**：`AOIEntityObject → ViewAOI → ViewVitalNPCNormal` 单向数据流。
4. **StageHandle 范式统一**：L3 技能阶段与 L4 Buff/Bullet/Passive 共用 StageHandle/TimeLineStage 阶段状态机。
5. **partial 物理拆分**：SkillController/SkillEntity 按功能域（输入/特效/动作/Buff等）拆分为 12 个 partial 文件，共享私有字段无接口隔离。

> 详细分层图见 `L0_ENTRY_LAYER.md` ~ `L5_SUPPORT_SYS.md`；跨层时序图见 `FLOW_*.md`。

# Star 战斗层代码分析导航 (Battle Layer Navigation)

> 生成日期：2026-07-09 | 代码根：`Assets/Scripts/StarGame/Game/` | 文件总数：**200** | SubAgent：**8 路**

## 📊 分层架构图（点击查看）

| 图 | 文件 | 层级 | 内容 |
|----|------|------|------|
| 框架图 | [BATTLE_FRAMEWORK_ARCHITECTURE.md](./BATTLE_FRAMEWORK_ARCHITECTURE.md) | All | 运行时主轴 + BattleManager 服务线 + 技能效果链路 |
| 全景图 | [BATTLE_OVERVIEW.md](./BATTLE_OVERVIEW.md) | All | 6 层模块关系总览 |
| L0 入口调度 | [L0_ENTRY_LAYER.md](./L0_ENTRY_LAYER.md) | L0 | GameManager/GameInput/渲染队列 |
| L1a 实体工厂 | [L1_ENTITY_FACTORY_LAYER.md](./L1_ENTITY_FACTORY_LAYER.md) | L1 | 三条创建链 + 对象池 |
| L1b 实体运行时 | [L1_ENTITY_RUNTIME_LAYER.md](./L1_ENTITY_RUNTIME_LAYER.md) | L1 | 远程实体/AOI/View 渲染 |
| L2 角色控制 | [L2_CONTROL_LAYER.md](./L2_CONTROL_LAYER.md) | L2 | 控制组家族 + 组件模式 |
| L3a 技能引擎 | [L3A_SKILL_ENGINE_CORE.md](./L3A_SKILL_ENGINE_CORE.md) | L3 | Timeline 驱动管线 |
| L3b 技能分部 | [L3B_SKILL_PARTIAL_EXT.md](./L3B_SKILL_PARTIAL_EXT.md) | L3 | 12 个 partial 扩展 |
| L4 战斗效果 | [L4_COMBAT_EFFECT.md](./L4_COMBAT_EFFECT.md) | L4 | Buff/Bullet/Passive/AutoBattle |
| L5 支撑系统 | [L5_SUPPORT_SYS.md](./L5_SUPPORT_SYS.md) | L5 | 地图/快照/相机/特效 |

## 🔄 时序图（跨层调用链）

| 图 | 文件 | 描述 |
|----|------|------|
| 自动战斗详细流程 | [FLOW_AUTO_BATTLE_DETAILED.md](./FLOW_AUTO_BATTLE_DETAILED.md) | AutoBattleBtn→BattleManager→FixedUpdate→索敌→移动→施法 |
| 技能施法全流程 | [FLOW_SKILL_CAST_FULL.md](./FLOW_SKILL_CAST_FULL.md) | 技能按钮→SkillComponent→SkillDispatcher→StageHandle→EffectUtils→目标实体 |
| 技能释放 | [FLOW_SKILL_RELEASE.md](./FLOW_SKILL_RELEASE.md) | 输入→SkillDispatcher→SkillStage→EffectUtils |
| 伤害结算 | [FLOW_DAMAGE_PIPELINE.md](./FLOW_DAMAGE_PIPELINE.md) | EffectUtils→Buff/Bullet→命中→伤害 |
| Buff 生命周期 | [FLOW_BUFF_LIFECYCLE.md](./FLOW_BUFF_LIFECYCLE.md) | AddBuff→Tick→到期 |

## 📁 SubAgent 原始报告

| Agent | 报告 | 文件数 | 关键发现 |
|-------|------|--------|---------|
| entry | [agent-entry.md](./reports/agent-entry.md) | 7 | GameManager 非 MonoBehaviour，由外部 EnterFrame 驱动 |
| entity-factory | [agent-entity-factory.md](./reports/agent-entity-factory.md) | 20 | 三条创建链 + Recycler 对象池 |
| entity-runtime | [agent-entity-runtime.md](./reports/agent-entity-runtime.md) | 48 | NPCEntityBase 216KB 仅读法名；AOI 三层裁剪 |
| player | [agent-player.md](./reports/agent-player.md) | 15 | 三段式异步加载 + 组件按需注册 |
| skill-core | [agent-skill-core.md](./reports/agent-skill-core.md) | 37 | Timeline 管线主轴 + EffectUtils 对接 L4 |
| skill-partial | [agent-skill-partial.md](./reports/agent-skill-partial.md) | 12 | 按功能域拆分，非生命周期 |
| effect | [agent-effect.md](./reports/agent-effect.md) | 10 | StageHandle 范式统一 L3/L4 |
| support | [agent-support.md](./reports/agent-support.md) | 51 | TypeEffect 工厂 + SnapShot 帧同步 |

## 📌 阅读顺序建议

```
1. BATTLE_OVERVIEW.md          ← 先看全局
2. L0 → L1a → L1b → L2 → L3a → L3b → L4 → L5   ← 按依赖方向自底向上
3. FLOW_SKILL_RELEASE.md       ← 理解技能主链路
4. FLOW_DAMAGE_PIPELINE.md     ← 理解效果落地
5. FLOW_BUFF_LIFECYCLE.md      ← 理解 Buff 机制
6. reports/agent-*.md          ← 需要细节时查原始报告
```

## ⚠️ 已知局限

1. **大文件仅读法名/签名**：NPCEntityBase(216KB)、ViewAOI(145KB)、SkillControllerSkillPartial(85KB)、SkillEntityActionPartial(76KB)、GameManager(239KB)、EffectUtils(79KB)、SkillEntity(83KB) 等巨型文件按截断策略只读结构/签名，实现细节未读。
2. **跨层接口存疑点**：EffectUtils 对 L4 的具体方法签名、AutoBattle 决策逻辑所在模块、AttacState vs AttackState 冗余等已标注 `[待确认]`/`[语义不清]`。
3. **L0 驱动源未确认**：GameManager.EnterFrame 的外部调用点（应在 Battle/World/MonoHelper 层）不在本次分析范围。
4. **虚构文件已剔除**：v4 计划中的 BattleManager.cs、EntityBaseData.cs 等 22 个文件经全量扫描确认不存在。

## 📊 统计

- 总文件：200（Game 7 / Entity 68 / Player 15 / Skill 59 / Map 7 / SnapShot 7 / StarsCamera 6 / TypeEffect 26 / CustomDataStruct 2 / SpecialUtilComp 2 / ViewEffect 1）
- 大文件（>30KB）：37 个，均按截断策略处理
- Mermaid 图：12 张（1 全景 + 8 分层 + 3 时序）
- 原始报告：8 份

# Star 战斗层代码分析导航 (Battle Layer Navigation)

> 生成日期：2026-07-09 | 代码根：`Assets/Scripts/StarGame/Game/` | 已覆盖文件：**220** | 补充目录已折入：ClientNpc 9 / Data 8 / Object 2 / Partner 1 | SubAgent：**8 路**

## 📊 分层架构图（点击查看）

| 图 | 文件 | 层级 | 内容 |
|----|------|------|------|
| 框架图 | [BATTLE_FRAMEWORK_ARCHITECTURE.md](./BATTLE_FRAMEWORK_ARCHITECTURE.md) | All | 运行时主轴 + BattleManager 服务线 + 技能效果链路 |
| 全景图 | [BATTLE_OVERVIEW.md](./BATTLE_OVERVIEW.md) | All | 6 层模块关系总览 |
| L0 入口调度 | [L0_ENTRY_LAYER.md](./L0_ENTRY_LAYER.md) | L0 | GameManager/GameInput/渲染队列 |
| L1a 实体工厂 | [L1_ENTITY_FACTORY_LAYER.md](./L1_ENTITY_FACTORY_LAYER.md) | L1 | Entity/Data/View 分工 + Recycler 对象池 |
| L1b 实体运行时 | [L1_ENTITY_RUNTIME_LAYER.md](./L1_ENTITY_RUNTIME_LAYER.md) | L1 | 远程实体/AOI/View 渲染 + Data 数据底座 |
| L2 角色控制 | [L2_CONTROL_LAYER.md](./L2_CONTROL_LAYER.md) | L2 | 控制层类 + 组件模式 + Object/PartnerManager |
| L3a 技能引擎 | [L3A_SKILL_ENGINE_CORE.md](./L3A_SKILL_ENGINE_CORE.md) | L3 | Timeline 驱动管线 |
| L3b 技能分部 | [L3B_SKILL_PARTIAL_EXT.md](./L3B_SKILL_PARTIAL_EXT.md) | L3 | 12 个 partial 扩展 |
| L4 战斗效果 | [L4_COMBAT_EFFECT.md](./L4_COMBAT_EFFECT.md) | L4 | Buff/Bullet/Passive/AutoBattle |
| L5 支撑系统 | [L5_SUPPORT_SYS.md](./L5_SUPPORT_SYS.md) | L5 | 地图/ClientNpc/快照现状/相机/特效/音效 |

## 🔄 时序图（跨层调用链）

| 图 | 文件 | 描述 |
|----|------|------|
| 自动战斗详细流程 | [FLOW_AUTO_BATTLE_DETAILED.md](./FLOW_AUTO_BATTLE_DETAILED.md) | AutoBattleBtn→BattleManager→FixedUpdate→索敌→移动→施法 |
| 技能施法全流程 | [FLOW_SKILL_CAST_FULL.md](./FLOW_SKILL_CAST_FULL.md) | 手动按钮→SkillComponent→SkillController.ClientUseSkill；自动战斗经 DispatchVKey(arg=2) 模拟按钮 |
| 技能释放 | [FLOW_SKILL_RELEASE.md](./FLOW_SKILL_RELEASE.md) | SkillComponent.SendUserSkillReq→SkillController.ClientUseSkill→服务器回包/Stage |
| 伤害结算 | [FLOW_DAMAGE_PIPELINE.md](./FLOW_DAMAGE_PIPELINE.md) | EffectUtils.HandleEffectDamage→HandleHurtNodeMsg→BattleManager.OnHurtData→DamageEntity 飘字 |
| Buff 生命周期 | [FLOW_BUFF_LIFECYCLE.md](./FLOW_BUFF_LIFECYCLE.md) | OnBuffCreateRet→SkillBuff.Create/EnterFrame→OnBuffEndRet→Release |

## 📁 SubAgent 历史报告

| Agent | 报告 | 文件数 | 关键发现 |
|-------|------|--------|---------|
| entry | [agent-entry.md](./reports/agent-entry.md) | 7 | 历史原始报告；入口结论以 L0_ENTRY_LAYER.md 和 entry-control review 为准 |
| entity-factory | [agent-entity-factory.md](./reports/agent-entity-factory.md) | 13+ | 历史报告口径；当前以 L1_ENTITY_FACTORY_LAYER.md 的真实 API 为准 |
| entity-runtime | [agent-entity-runtime.md](./reports/agent-entity-runtime.md) | 48 | 历史原始报告；AOI/View 结论以 L1_ENTITY_RUNTIME_LAYER.md 为准 |
| player | [agent-player.md](./reports/agent-player.md) | 15 | 历史原始报告；控制组结论以 L2_CONTROL_LAYER.md 为准 |
| skill-core | [agent-skill-core.md](./reports/agent-skill-core.md) | 37 | 历史原始报告；技能入口以 L3A/FLOW 文档为准 |
| skill-partial | [agent-skill-partial.md](./reports/agent-skill-partial.md) | 12 | 历史原始报告；UserInput/ActionPartial 以 L3B 为准 |
| effect | [agent-effect.md](./reports/agent-effect.md) | 10 | 历史原始报告；Buff/Bullet/Passive 以 L4 和 FLOW 文档为准 |
| support | [agent-support.md](./reports/agent-support.md) | 51 | 历史原始报告；Map/Camera/TypeEffect 以 L5 为准 |

## 📌 阅读顺序建议

```
1. BATTLE_OVERVIEW.md          ← 先看全局
2. L0 → L1a → L1b → L2 → L3a → L3b → L4 → L5   ← 按依赖方向自底向上
3. FLOW_SKILL_RELEASE.md       ← 理解技能主链路
4. FLOW_DAMAGE_PIPELINE.md     ← 理解效果落地
5. FLOW_BUFF_LIFECYCLE.md      ← 理解 Buff 机制
6. VERIFY_BATTLE_MD_AGAINST_CODE.md / reports/*-review.md ← 查校验依据；原始 agent-*.md 只作历史输入
```

## ⚠️ 已知局限

1. **原始报告截断策略**：NPCEntityBase(216KB)、ViewAOI(145KB)、SkillControllerSkillPartial(85KB)、SkillEntityActionPartial(76KB)、GameManager(239KB)、EffectUtils(79KB)、SkillEntity(83KB) 等巨型文件在初版 agent 报告中按截断策略读取；当前根文档的关键链路已通过 CodeGraph / 定向源码复核，依据见 `VERIFY_BATTLE_MD_AGAINST_CODE.md` 与 `reports/*-review.md`，不得从原始报告外推未复核实现。
2. **跨层接口校正**：EffectUtils/L4 方法签名以 `L4_COMBAT_EFFECT.md` 与 `FLOW_DAMAGE_PIPELINE.md` 的已验证链路为准；`AttacState/AttackState` 只保留代码可证的 `VitalState` 继承与 `OnEnable` 差异，不再声明资产 GUID 引用状态。
3. **已补证入口**：AutoBattle 决策主线已确认在 BattleManager；L0 帧驱动源已确认由 StarWorldGame.FixedUpdate/OnEnterFrame 调用 GameManager.EnterFrame / BattleManager.EnterFrame。
4. **历史计划需复核**：v4/v5 计划中的部分路径和文件结论已被当前代码推翻；例如 `BattleManager.cs` 实际位于 `Service/BattleManager/`，`EntityBaseData.cs` 实际位于 `Game/Data/`。
5. **补充目录拆分进度**：`ClientNpc` 已折入 `L5_SUPPORT_SYS.md`；`Data` 已折入 `L1_ENTITY_RUNTIME_LAYER.md`；`Object` 与 `PartnerManager` 已折入 `L2_CONTROL_LAYER.md`。原始归属记录见 `reports/agent-game-extra-review.md`。

## 📊 统计

- 已覆盖文件：220（原 200 + ClientNpc 9 + Data 8 + Object 2 + PartnerManager 1）
- ClientNpc 已折入 L5；Data 已折入 L1；Object/PartnerManager 已折入 L2
- 大文件（>30KB）：37 个；初版原始报告按截断策略处理，当前根文档关键链路以 VERIFY / review 复核记录为准
- Mermaid 图：13 张（1 全景 + 8 分层 + 4 时序）
- 原始报告：8 份（历史输入）+ review 校验报告 4 份 + VERIFY 校验表

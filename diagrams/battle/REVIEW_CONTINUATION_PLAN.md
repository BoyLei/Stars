# Battle Docs Review Continuation Plan

> 用途：在新 Codex 对话中继续 `review战斗模块，逐步完善补齐战斗各模块层级md`，避免依赖旧对话上下文。

## 新对话启动提示

请在项目根目录 `H:\Star\Stars_Project\StarsProject_Client\trunk\Stars` 继续：

```text
继续 review 战斗模块文档。先读 `diagrams/battle/REVIEW_CONTINUATION_PLAN.md`、`diagrams/battle/_Nav_Battle.md`、`diagrams/battle/VERIFY_BATTLE_MD_AGAINST_CODE.md`，再基于当前工作树和代码继续校验，不要依赖旧对话记忆。优先用 codebase-memory-mcp；如果没有暴露工具，用本地 `cmd.exe /c codegraph ...`。只修有源码证据的问题，不猜测。
```

## 当前已完成

- 已把 battle 根文档、流程文档、导航和 VERIFY 表做了一轮代码证据修正。
- 已修正 `diagrams/NAVIGATION.md` 中不存在的 `battle/SkillSystem.md` 断链，改指向现行 `battle/L4_COMBAT_EFFECT.md` 和 `battle/BATTLE_OVERVIEW.md`。
- 已把 L2 相关描述从过宽的“CtrlGroup 家族”收窄为“控制层类”，并同步 `BATTLE_OVERVIEW.md`、`_Nav_Battle.md`、`VERIFY_BATTLE_MD_AGAINST_CODE.md`。
- 三个 SVG 已标为历史渲染版本；当前事实以 Markdown 内 Mermaid 源为准。
- 原始 `reports/agent-*.md` 是历史输入；当前事实以根文档、`VERIFY_BATTLE_MD_AGAINST_CODE.md` 和 `reports/*-review.md` 为准。
- 2026-07-16 续核：四份 `reports/*-review.md` 均已有 Follow-up 说明 `Required Fixes` 是当时清单且根文档已同步；`PLAN_v4.md` / `PLAN_v5.md` 顶部已声明历史草案边界，内部旧 checklist 不逐句重写。
- 2026-07-16 续核：根文档高风险旧链路反查未发现新的正向错误；硬数字已按当前文件树复核为 `220`、`ClientNpc 9`、`Data 8`、`Object 2`、`Partner 1`、`SkillPartial 12`、`L4 11`、`L5 51`、`TypeEffectFactory` 映射 `17`。
- 2026-07-16 续核：Mermaid 图语义已收紧，`SkillComponent` 不再画成直连 `SkillEntity`，框架总图改为实体运行时驱动 `SkillDispatcher`，技能输入参与者从 `UI / GameInput` 改为 `UI / UniversalButton`。
- 2026-07-16 续核：`FLOW_SKILL_CAST_FULL.md` 时序图不再把 `NPCEntityBase` 画成 `ClientUseSkill()` 的主动中转调用者；`FLOW_DAMAGE_PIPELINE.md` 中 Bullet 创建顺序已同步为 `CreateStageHandle()` 后 `CreateBulletInfo()`。
- 2026-07-16 续核：`FLOW_BUFF_LIFECYCLE.md` 补入 `BuffEndRet.BlackList` 在 `SkillBuff.OnBuffEndRet()` 内的处理；`FLOW_SKILL_RELEASE.md` 已把 Buff/Bullet/Passive 的回包创建和每帧推进拆开。
- 2026-07-16 续核：`L3A_SKILL_ENGINE_CORE.md` 已把 `SkillContainer -> SkillEntity`、`SkillController -> SkillStage` 和 `EffectExecuteResult` 调度节点等过直图语义收紧；`L4_COMBAT_EFFECT.md` BuffEnd 时序补入 `BuffEndRet.BlackList` 处理顺序。
- 2026-07-16 续核：`L3B_SKILL_PARTIAL_EXT.md` 补齐 `OnBuffRuntimeSync`、`OnBulletRuntimeSync`、Bullet release/reset helper 和 `JoyStickCancleBuff` 实名；`L4_COMBAT_EFFECT.md` / `FLOW_SKILL_RELEASE.md` 补入 Bullet runtime sync 黑板边界与 Passive 结束/释放顺序。
- 2026-07-16 续核：抽样复核 `_Nav_Battle.md` / `BATTLE_OVERVIEW.md` / `L4_COMBAT_EFFECT.md` 的 L4 入口摘要，把 AutoBattle 收窄为 `AutoBattleBtn` / `SwitchEnemyBtn` 按钮支线，决策主线仍归 `BattleManager`。
- 2026-07-16 续核：抽样复核 `L5_SUPPORT_SYS.md` Snapshot 入口摘要，补清 `SerMessageSnapData` 由 `NetworkManager.CacheMessageSnapData()` 写入、`NetworkManager.OnMyUpdate -> InvokeMessageSnapDataCache()` 弹出处理；RPC 快照主帧驱动仍为非现行链路。
- 2026-07-16 续核：抽样复核 `BattleManager` / `SkillComponent` / 阶段效果路径入口摘要，确认 `BattleManager.EnterFrame(int)` 不驱动完整战斗主轴、`SkillComponent.SendUserSkillReq()` 只是进入 `SkillController.ClientUseSkill(...)`、手动效果路径与 `StageHandle` 路径分离；并把 `BATTLE_OVERVIEW.md` / `FLOW_AUTO_BATTLE_DETAILED.md` 总览句收窄为 Damage 分支才继续进入 `EffectUtils`。
- 2026-07-16 续核：收口审计 `_Nav_Battle.md` 与 L1-L5 入口摘要，唯一新增源码证据修正是 `L1_ENTITY_FACTORY_LAYER.md` 中 `IRecyclableObject` 从“所有实体实现”收窄为“池化对象接口”；其余入口摘要未发现新的正向错误。
- 2026-07-16 最终校验：`git diff --check`、Markdown 链接检查、源码路径检查、UTF-8 无 BOM + CRLF 检查均通过；当前事实文档高风险词扫描未发现新的正向错误。

## 当前变更范围

截至本计划生成时，主要改动集中在：

- `diagrams/NAVIGATION.md`
- `diagrams/battle/_Nav_Battle.md`
- `diagrams/battle/BATTLE_FRAMEWORK_ARCHITECTURE.md`
- `diagrams/battle/BATTLE_OVERVIEW.md`
- `diagrams/battle/FLOW_*.md`
- `diagrams/battle/L1_*.md` 到 `L5_SUPPORT_SYS.md`
- `diagrams/battle/VERIFY_BATTLE_MD_AGAINST_CODE.md`
- `diagrams/battle/reports/agent-effect-review.md`

新对话必须先用 `git status --short -- diagrams/NAVIGATION.md diagrams/battle` 读取当前状态。

## 下一步建议

1. 先跑校验命令，确认当前工作树没有格式和链接问题。
2. 数量断言已在 2026-07-16 复核通过；后续除非文件树变化，不必重复作为首要任务。
3. 原高风险结论已抽样复核：`BattleManager` 服务线、`SkillComponent` UI 协调入口、手动 `SkillEntityActionPartial` 与 Buff/Bullet/Passive `StageHandle` 路径分离、`PassiveInfo`/`PassiveSkillEntity` 边界、RPC snapshot 非主帧缓存链均已同步到当前根文档或 VERIFY。
4. Mermaid 总图、`FLOW_SKILL_CAST_FULL`、`FLOW_DAMAGE_PIPELINE`、`FLOW_AUTO_BATTLE_DETAILED`、`FLOW_BUFF_LIFECYCLE`、`FLOW_SKILL_RELEASE`、`L3A_SKILL_ENGINE_CORE`、`L3B_SKILL_PARTIAL_EXT`、`L4_COMBAT_EFFECT` 已做多轮图语义收紧；`_Nav_Battle.md` 与 L1-L5 入口摘要也已做一轮收口审计，最终交付前校验已通过。后续不建议继续同批 review；如要推进，应转为提交前 diff 整理或新的指定模块问题。
5. 每修一个根文档或流程文档，同时更新 `VERIFY_BATTLE_MD_AGAINST_CODE.md` 的对应记录。
6. 不要重写历史原始报告，除非它缺少顶部“历史草稿/当前事实以 ... 为准”的边界说明。

## 建议校验命令

```powershell
git diff --check -- diagrams/NAVIGATION.md diagrams/battle
```

```powershell
$files = @((Resolve-Path -LiteralPath 'diagrams/NAVIGATION.md').Path) + (Get-ChildItem -LiteralPath 'diagrams/battle' -File -Filter '*.md' | ForEach-Object { $_.FullName })
$missing = @()
foreach ($file in $files) {
  $base = [System.IO.Path]::GetDirectoryName($file)
  $text = Get-Content -LiteralPath $file -Raw
  foreach ($m in [regex]::Matches($text, '\[[^\]]+\]\(([^)]+)\)')) {
    $target = $m.Groups[1].Value
    if ($target -match '^(https?:|mailto:|#)' -or $target.Trim() -eq '') { continue }
    $clean = ($target -split '#')[0]
    if ($clean -eq '') { continue }
    $path = [System.IO.Path]::GetFullPath((Join-Path $base $clean))
    if (-not (Test-Path -LiteralPath $path)) { $missing += $target }
  }
}
if ($missing.Count -eq 0) { 'NO_MISSING_LINKS' } else { $missing }
```

```powershell
$files = @((Resolve-Path -LiteralPath 'diagrams/NAVIGATION.md').Path) + (Get-ChildItem -LiteralPath 'diagrams/battle' -File -Filter '*.md' | ForEach-Object { $_.FullName })
$missing = @()
foreach ($file in $files) {
  $text = Get-Content -LiteralPath $file -Raw
  foreach ($m in [regex]::Matches($text, 'Assets/Scripts/[A-Za-z0-9_./-]+\.(?:cs|lua)')) {
    $rel = $m.Value -replace '/', '\'
    if (-not (Test-Path -LiteralPath $rel)) { $missing += $m.Value }
  }
}
if ($missing.Count -eq 0) { 'NO_MISSING_SOURCE_PATHS' } else { $missing }
```

## 完成条件

- `diagrams/NAVIGATION.md` 和 `diagrams/battle/_Nav_Battle.md` 能正确指向现行 battle 文档。
- 根文档、流程文档、L1-L5 文档中的核心断言都有源码证据或 VERIFY 记录支撑。
- 没有当前事实文档引用不存在的本地 Markdown 或源码路径。
- `git diff --check` 通过。
- 新增/修改 Markdown 为 UTF-8 无 BOM、CRLF。

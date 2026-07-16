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
2. 检查文档里的数量断言是否还和当前文件树一致，例如 L1/L2/L3/L4/L5 的文件数量、`SkillPartial/` 12 个文件、`Object 2`、`ClientNpc 9`、`Data 8` 等。
3. 优先复核仍容易过宽的结论：
   - `BattleManager` 只作为服务侧战斗状态/自动战斗线，不要写成整个战斗核心。
   - `SkillComponent` 是 UI 协调入口，不是技能状态 tick 驱动者。
   - 手动 `SkillEntityActionPartial` 效果路径和 Buff/Bullet/Passive 的 `StageHandle` 路径要分开。
   - `PassiveInfo` 是配置，运行时实体是 `PassiveSkillEntity`。
   - `SerRPCOneFrameSnapALLData` 等 RPC snapshot 类保留但不是当前 `GameManager.EnterFrame()` 主驱动缓存链。
4. 每修一个根文档或流程文档，同时更新 `VERIFY_BATTLE_MD_AGAINST_CODE.md` 的对应记录。
5. 不要重写历史原始报告，除非它缺少顶部“历史草稿/当前事实以 ... 为准”的边界说明。

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

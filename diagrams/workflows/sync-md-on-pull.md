# MD 同步工作流 — AI Agent 使用说明
> 供 Codex、Claude、CodeBuddy 等 AI Agent 调用
> 目标：git pull 后自动检测模块变更，更新 Lua MD 文档

---

## 入口

AI Agent 收到指令 `"根据最新的 git pull 更新 MD"` 后执行以下流程。

---

## Step 1 — 读取变更报告

运行命令获取 hook 保存的变更文件列表：

```bash
python h:\Star\tools\detect_md_changes.py --from-hook --json
```

输出示例：
```json
{
  "meta": { "analyzed_at": "2026-07-05T02:43:02" },
  "modules_changed": [
    {
      "module": "NewModule",
      "type": "lua",
      "domain": null,
      "known": false,
      "is_new": true,
      "is_deleted": false,
      "recommended_action": "add_description",
      "files": [{"path": "Assets/Res/.../NewModule.lua.txt", "status": "A"}]
    },
    {
      "module": "ChatModule",
      "type": "lua",
      "domain": "社交域",
      "known": true,
      "is_new": false,
      "is_deleted": false,
      "recommended_action": "check_description",
      "files": [{"path": "Assets/Res/.../ChatModule.lua.txt", "status": "M"}]
    }
  ],
  "summary": "检测到 2 个模块变更: 新增 1 个模块; 修改 1 个模块"
}
```

**如果报告为空或无变更** → 结束。

---

## Step 2 — 更新 LuaModuleList.md

文件：`h:\Star\diagrams\lua\module\LuaModuleList.md`

对每个 `modules_changed` 中的模块：

| 操作 | 处理 |
|------|------|
| `add_description` | 在对应域表格末尾追加一行 `\| **ModuleName** \| [待补充职责] \|`。如果 domain=null 则追加到"未归类"段 |
| `remove_entry` | 删除对应表格行 |
| `update_reference` | 将旧模块名改为新模块名 |
| `check_description` | 不操作（描述在域 MD 中管理） |

最后更新各域标题的计数 `(N 个)`，统计表格实际行数。

---

## Step 3 — 更新域 MD

根据 domain 确定对应文件：

| domain | 文件 |
|--------|------|
| 社交域 | `SocialModules.md` |
| 经济/交易域 | `EconomyModules.md` |
| 活动/运营域 | `EventModules.md` |
| 战斗/副本域 | `CombatModules.md` |
| 角色/养成域 | `CharacterModules.md` |
| 系统/信息域 | `SystemModules.md` |

### 新增模块
1. 读取源码：拼接完整路径 `h:\Star\Stars_Project\StarsProject_Client\trunk\Stars\{files[0].path}`
2. 分析关键函数：`OnModuleCreate`、`OnShow`、注册的消息、打开的 UI
3. 生成 **3-5 行描述**：
   - 第 1 行：功能概述（一句话）
   - 第 2 行：关键 UI 路径（`UI: ...`）
   - 第 3 行：一个区分性特点（`特点: ...`）
4. 追加到 MD 末尾，编号为当前最大编号 + 1

**约束**：
- 不含代码片段
- 不含文件大小、协议数量、消息表
- 如果不确定域，归入"系统/信息域"

### 删除模块
1. 删除 `## N. ModuleName — 标题` 到下一个 `##` 之间的全部内容
2. 重新编号剩余条目（`## 1.`、`## 2.`、...）

### 修改模块
1. 读取当前源码
2. 对比 MD 中的描述是否仍然准确
3. 如有变化（新增功能/改 UI/逻辑变更），更新描述

---

## Step 4 — 同步 codebase-memory

调用 MCP 工具通知知识图谱变更：

```
codebase-memory-mcp.detect_changes(project="Star", since="HEAD@{1}", depth=2)
```

（可选，建议执行以保持知识图谱最新）

---

## 手动触发（无 hook 时）

如果 hook 未安装，也可以直接对比 git commit：

```bash
python h:\Star\tools\detect_md_changes.py --since HEAD~1 --json
```

将 `HEAD~1` 替换为上次更新 MD 后的起始 commit。

---

## 文件索引

| 文件 | 作用 |
|------|------|
| `h:\Star\tools\detect_md_changes.py` | 检测变更并输出 JSON 报告 |
| `h:\Star\Stars_Project\StarsProject_Client\trunk\Stars\.githooks\post-merge` | git pull 后自动保存 diff 文件 |
| `h:\Star\Stars_Project\StarsProject_Client\trunk\Stars\.githooks\post-checkout` | git pull 快进方式时自动保存 diff 文件 |
| `h:\Star\Stars_Project\StarsProject_Client\trunk\Stars\install-hooks.bat` | 团队成员安装 hooks |
| `%USERPROFILE%\.star_md_sync\diff_files.txt` | hook 保存的变更文件列表 |

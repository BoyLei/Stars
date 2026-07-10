# 自动战斗详细流程

> 生成日期：2026-07-10  
> 代码主线：`Assets/Scripts/StarGame/Service/BattleManager/BattleManager.cs`  
> 可直接打开的 SVG 图：[FLOW_AUTO_BATTLE_DETAILED.svg](./FLOW_AUTO_BATTLE_DETAILED.svg)

## 总览

自动战斗不是独立战斗系统，而是 `BattleManager` 在服务层做的一套“定期思考 + 索敌 + 移动到技能范围 + 复用技能施法入口”的状态机。

核心链路：

```text
AutoBattleBtn / 缓存恢复
  -> BattleManager.SwitchAutoBattle()
  -> Start()
  -> MonoHelper.AddFixedUpdateListener(OnFixedUpdate)
  -> IsExecuteAutoBattle()
  -> MarkDirtyState / OnTick()
  -> SearchEnemyFight()
  -> SearchEnemy()
  -> GotoFightEnemy() 或 GotoTargetPos()
  -> GetAutoBattleCanUseSkillContainer()
  -> CheckIsSkillArea()
  -> AutoBattleUseSkill()
  -> TryUseSkill()
  -> UseSkill()
```

## 详细流程图

```mermaid
flowchart TB
    UI["AutoBattleBtn.OnClick / OnEnable<br/>玩家点击或读取本地缓存"] --> Switch["BattleManager.SwitchAutoBattle()"]
    Switch --> Forbid{"IsForbid?"}
    Forbid -- yes --> Noop["return，不启动"]
    Forbid -- no --> IsOpen{"IsAutoBattling?"}
    IsOpen -- no --> Start["Start()"]
    IsOpen -- yes --> Stop["Stop()"]

    Start --> Open["UpdateBateState(Open, true)"]
    Open --> Listen["OnEventListener()<br/>AutoBattleEvent / LoadingViewEvent"]
    Listen --> Fixed["MonoHelper.AddFixedUpdateListener(OnFixedUpdate, Battle)"]
    Fixed --> Prep["RecordAutoBattleStartPos(true)<br/>BreakFindPath()<br/>BreakFollowDynamicEnity()<br/>RefreshCacheSkillPosContainers()"]
    Prep --> Dirty["MarkDirtyState(true)"]

    Dirty --> FixedTick["OnFixedUpdate()"]
    FixedTick --> GlobalCD{"globalSkillCD > 0?"}
    GlobalCD -- yes --> DecGlobal["递减 globalSkillCD<br/>归零后 MarkDirtyState(true)"]
    GlobalCD -- no --> Cool{"coolTime > 0?"}
    DecGlobal --> Cool
    Cool -- yes --> DecCool["递减 coolTime<br/>归零后 MarkDirtyState(true)<br/>return"]
    Cool -- no --> CanExec{"IsExecuteAutoBattle()?"}

    CanExec -- no --> Wait["等待下一次 FixedUpdate"]
    CanExec -- yes --> DirtySkill{"dirtyAutoSkill?"}
    DirtySkill -- yes --> Tick["OnTick()"]
    DirtySkill -- no --> Think["thinkingTimes++"]
    Think --> ThinkReady{"thinkingTimes >= THINKING_FREQUENCY?"}
    ThinkReady -- yes --> MarkThink["thinkingTimes = 0<br/>MarkDirtyState(true)"]
    ThinkReady -- no --> PartnerCheck{"伙伴自动战斗<br/>到 5s 检查点?"}
    MarkThink --> PartnerCheck
    PartnerCheck -- yes --> Partner["TriggerPartnerSkill()"]
    PartnerCheck -- no --> Wait
    Partner --> Wait

    Tick --> Hold{"IsAutoBattleHoldOn?"}
    Hold -- yes --> Wait
    Hold -- no --> SearchFight["SearchEnemyFight()"]
    SearchFight --> Search["SearchEnemy()"]
    Search --> Found{"找到目标?"}
    Found -- no --> Empty["更新 searchEmptyTime<br/>超过时间后准备回起点"]
    Empty --> Back["GotoTargetPos()"]
    Back --> Wait

    Found -- yes --> Fight["GotoFightEnemy()"]
    Fight --> SkillSlot["GetAutoBattleCanUseSkillContainer()"]
    SkillSlot --> HasSkill{"有可用技能槽?"}
    HasSkill -- no --> Wait
    HasSkill -- yes --> Area{"CheckIsSkillArea<br/>目标在技能范围内?"}
    Area -- yes --> Cast["AutoBattleUseSkill(skillContainer)"]
    Cast --> Try["TryUseSkill() -> UseSkill()"]
    Try --> Success{"释放成功?"}
    Success -- yes --> GCD["StartGlobalSkillCD()<br/>clear ignoreSkillPoss<br/>return"]
    GCD --> Wait
    Success -- no --> Ignore["加入 ignoreSkillPoss<br/>换下一个技能槽"]
    Ignore --> SkillSlot

    Area -- no --> MoveCalc["按 SkillAutoUseDistance<br/>计算释放中心点"]
    MoveCalc --> ValidPoint["FindPathManager.FindDistanceValidPoint()"]
    ValidPoint --> Move["PlayerCtrlGroup.TryFindPath(..., E_FindPathType.AutoBattle)"]
    Move --> MoveOK{"寻路成功?"}
    MoveOK -- yes --> WaitMove["isMovingToEnemy = true<br/>等待移动完成回调再标脏"]
    MoveOK -- no --> MoveFail["isMovingToEnemy = false"]
    WaitMove --> Wait
    MoveFail --> Wait
```

## 时序图

```mermaid
sequenceDiagram
    participant Btn as AutoBattleBtn
    participant BM as BattleManager
    participant Mono as MonoHelper FixedUpdate
    participant GM as GameManager
    participant PCG as PlayerCtrlGroup
    participant Skill as SkillContainer
    participant Find as FindPathManager

    Btn->>BM: SwitchAutoBattle()
    alt 禁止自动战斗
        BM-->>Btn: return
    else 未开启
        BM->>BM: Start()
        BM->>BM: UpdateBateState(Open, true)
        BM->>Mono: AddFixedUpdateListener(OnFixedUpdate)
        BM->>PCG: BreakFindPath / BreakFollowDynamicEnity
        BM->>BM: RefreshCacheSkillPosContainers()
    else 已开启
        BM->>BM: Stop()
    end

    loop FixedUpdate
        Mono->>BM: OnFixedUpdate()
        BM->>BM: 扣 globalSkillCD / coolTime
        BM->>BM: IsExecuteAutoBattle()
        BM->>BM: dirtyAutoSkill 或 THINKING_FREQUENCY 触发
        BM->>BM: OnTick()
        BM->>BM: SearchEnemyFight()
        alt 找到敌人
            BM->>GM: GetEntityCtr(curAutoBattleAtkEntity)
            BM->>Skill: GetAutoBattleCanUseSkillContainer()
            alt 目标在技能范围
                BM->>BM: AutoBattleUseSkill()
                BM->>BM: TryUseSkill() -> UseSkill()
                BM->>Skill: MarkAutoBattleCD()
                BM->>BM: StartGlobalSkillCD()
            else 目标不在范围
                BM->>Find: FindDistanceValidPoint()
                BM->>PCG: TryFindPath(... AutoBattle)
            end
        else 没找到敌人
            BM->>BM: GotoTargetPos()
        end
    end
```

## 关键状态和分支

| 节点 | 代码位置 | 作用 |
|------|----------|------|
| `SwitchAutoBattle()` | `BattleManager.cs:1407` | UI 开关入口；禁止状态直接返回；未开启走 `Start()`，已开启走 `Stop()`。 |
| `Start()` | `BattleManager.cs:1512` | 打开状态、注册事件和 FixedUpdate、打断寻路/跟随、刷新技能槽缓存。 |
| `OnFixedUpdate()` | `BattleManager.cs:1337` | 自动战斗的定期思考循环，处理全局技能 CD、冷却、脏标记、伙伴自动战斗。 |
| `IsExecuteAutoBattle()` | `BattleManager.cs:1305` | 执行保护：主角控制组存在、自动战斗开启、未挂起、未寻路、主角未死亡。 |
| `OnTick()` | `BattleManager.cs:1397` | 非挂起状态下进入 `SearchEnemyFight()`。 |
| `SearchEnemyFight()` | `BattleManager.cs:1651` | 查怪成功进入战斗；失败时回目标点/起点。 |
| `GotoFightEnemy()` | `BattleManager.cs:1986` | 找可用技能槽，若目标在范围内直接施法；否则计算技能释放距离并寻路。 |
| `AutoBattleUseSkill()` | `BattleManager.cs:2180` | 清脏标记、调用 `TryUseSkill()`、给技能槽打自动战斗 CD。 |
| `TryUseSkill()` | `BattleManager.cs:2257` | 调用 `UseSkill()`，再按是否开启预播检查施法是否成功。 |

## 与手动施法的交汇点

自动战斗自身只负责“什么时候该对哪个目标用哪个技能”。真正施法仍复用技能系统入口：

- `TryUseSkill()` 调用 `UseSkill()`。
- `UseSkill()` 进入后续技能发送/客户端预播逻辑。
- 后续流程与手动点击技能一致，进入 `SkillComponent / SkillEntity / StageHandle / EffectUtils` 这条管线。


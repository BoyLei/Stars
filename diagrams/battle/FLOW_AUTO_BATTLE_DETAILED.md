# 自动战斗详细流程

> 生成日期：2026-07-10  
> 代码主线：`Assets/Scripts/StarGame/Service/BattleManager/BattleManager.cs`  
> 历史 SVG 图：[FLOW_AUTO_BATTLE_DETAILED.svg](./FLOW_AUTO_BATTLE_DETAILED.svg)。当前代码事实以本文 Mermaid 源为准；本轮未触网安装 Mermaid CLI 重渲 SVG。

## 总览

自动战斗不是独立战斗系统，而是 `BattleManager` 在服务层做的一套“定期思考 + 索敌 + 移动到技能范围 + 复用技能施法入口”的状态机。

核心链路：

```text
AutoBattleBtn.OnClick
  -> BattleManager.SwitchAutoBattle()
  -> Start() 或 Stop()
AutoBattleBtn.OnEnable 缓存恢复
  -> 读取 AUTOBATTLE_OPEN
  -> Start() 或 Stop()
Start()
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
    UI["AutoBattleBtn.OnClick"] --> Switch["BattleManager.SwitchAutoBattle()"]
    Cache["AutoBattleBtn.OnEnable<br/>读取 AUTOBATTLE_OPEN"] --> CacheOpen{"缓存 isOpen?"}
    CacheOpen -- yes --> Start["Start()"]
    CacheOpen -- no --> Stop["Stop()"]
    Switch --> Forbid{"IsForbid?"}
    Forbid -- yes --> Noop["return，不启动"]
    Forbid -- no --> IsOpen{"IsAutoBattling?"}
    IsOpen -- no --> Start
    IsOpen -- yes --> Stop

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
    Try --> SlotCD["skillContainer.MarkAutoBattleCD()<br/>无论 TryUseSkill 返回值"]
    SlotCD --> Success{"释放成功?"}
    Success -- yes --> GCD["StartGlobalSkillCD()<br/>clear ignoreSkillPoss<br/>return"]
    GCD --> Wait
    Success -- no --> Ignore["加入 ignoreSkillPoss<br/>换下一个技能槽"]
    Ignore --> SkillSlot
    SkillSlot -.循环退出.-> ClearIgnore["ignoreSkillPoss.Clear()<br/>再进入移动到范围分支"]

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

    Note over Btn,BM: OnEnable 读取 AUTOBATTLE_OPEN 后直接 Start/Stop；OnClick 才 SwitchAutoBattle
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
                BM->>Skill: MarkAutoBattleCD()（无论 TryUseSkill 返回值）
                alt AutoBattleUseSkill 返回 true
                    BM->>BM: StartGlobalSkillCD()
                else 返回 false
                    BM->>BM: ignoreSkillPoss.Add / 换下一个技能槽
                end
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
| `AutoBattleUseSkill()` | `BattleManager.cs:2180` | 清脏标记、调用 `TryUseSkill()`，随后无论释放结果都给当前技能槽打自动战斗 CD。 |
| `TryUseSkill()` | `BattleManager.cs:2257` | 调用 `UseSkill()`，再按是否开启预播检查施法是否成功。 |

## 与手动施法的交汇点

自动战斗自身只负责“什么时候该对哪个目标用哪个技能”。真正施法仍复用技能系统入口：

- `TryUseSkill()` 调用 `UseSkill()`。
- `UseSkill()` 不直接发送技能协议，而是通过 `InputManager.DispatchVKey(vkey, 2, true)` 触发虚拟键事件。
- `InputManager.DispatchVKey(..., isForce: true)` 最终触发 `OnVirtualInput`；`UniversalButton.Awake()` 订阅该事件，`Reset()` 取消订阅。
- `UniversalButton.InputVKey(arg == 2)` 模拟一次 `OnPointerDown(null)` + `OnPointerUp(null)`。
- 后续流程与手动点击技能一致，经 `SkillComponent.SendUserSkillReq()` 进入 `SkillController.ClientUseSkill()`；手动 `SkillEntity` 效果走 `SkillEntityActionPartial`，ServerControl/Bullet 等阶段路径才走 `StageHandle`，Damage 分支才继续进入 `EffectUtils` 等伤害落地处理。

## 追加复核证据（2026-07-15）

- `BattleManager.AutoBattleUseSkill()` 当前顺序为 `MarkDirtyState(false)` -> `TryUseSkill()` -> `skillContainer.MarkAutoBattleCD()` -> 返回结果；`MarkAutoBattleCD()` 不依赖释放成功。
- `BattleManager.GotoFightEnemy()` 只在 `AutoBattleUseSkill(...) == true` 分支调用 `StartGlobalSkillCD()`；`ignoreSkillPoss` 在入口、成功分支和循环退出后都会清空，失败分支会先加入当前技能槽再尝试下一个。
- `BattleManager.UseSkill()` 只做技能槽到虚拟按键的映射，并调用 `InputManager.Instance.DispatchVKey(keyCode, 2, true)`；它不直接调用 `SkillController`。
- `UniversalButton.InputVKey(arg == 2)` 负责把自动战斗虚拟按键转换成一次按钮按下/抬起。

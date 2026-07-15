# Scope

Reviewed:
- `diagrams/battle/L3A_SKILL_ENGINE_CORE.md`
- `diagrams/battle/L3B_SKILL_PARTIAL_EXT.md`
- `diagrams/battle/FLOW_SKILL_RELEASE.md`
- `diagrams/battle/FLOW_SKILL_CAST_FULL.md`
- `diagrams/battle/BATTLE_FRAMEWORK_ARCHITECTURE.md`

Focus: skill core, skill partials, skill release/cast flow, and the requested modules: `SkillDispatcher`, `SkillController` partials, `SkillEntity`, `SkillStage`, `StageHandle`, `SkillComponent`, `GameManager/InputVKey`.

# Follow-up 2026-07-13

- 本报告下方 `Required Fixes` 是当时发现的问题清单；当前根文档已同步：`L3A_SKILL_ENGINE_CORE.md` 不再把手动技能入口写成 `GameManager.InputVKey` 链，`FLOW_SKILL_RELEASE.md` / `FLOW_SKILL_CAST_FULL.md` 已改为 `SkillComponent.SendUserSkillReq -> SkillController.ClientUseSkill -> UseSkill -> UseNewSkill -> CreateSkillEntity -> SkillEntity.ClientUseSkill -> OnClientPreEnter -> EnterCurStage`，自动战斗入口已改为 `BattleManager -> InputManager.DispatchVKey -> UniversalButton.InputVKey -> SkillComponent`。
- 后续摘要复核又补充了两点到根文档：技能主轴需要包含 `SkillContainer`，运行时 tick 入口是 `NPCEntityBase.EnterFrame -> SkillDispatcher.EnterFrame`，不是 `SkillComponent` 驱动技能状态 tick。
- 2026-07-15 三次复核补充：`L3A_SKILL_ENGINE_CORE.md` / `FLOW_SKILL_RELEASE.md` 已把手动按钮入口从单一 `OnPointerUp` 收紧为 `OnPointerDown/OnPointerUp/OnEndDrag/onPressing` 委托进入 `SkillComponent.OnPointerDown/OnPointerUp/OnEndDrag/OnPressingAction`；`VKey` 移动链只到 `PlayerCtrlGroup.DoVKey_Move`，不进入 Stage/效果链。
- 2026-07-15 三次复核补充：`SkillStage.Enter/Tick/GetTimeLineStage` 等伪接口已改为 `OnEnter/OnUpdate/ExitStage/ExecuteFrameEvents`，`GetTimeLineStage` 归属 `SkillInfo/BaseConfigInfo`；手动 SkillEntity 效果入口改为 `SkillEntityActionPartial.PlayStageEffect/TryPlayEffect`，`StageHandle` 限定为 ServerControl/Bullet 等路径。
- 2026-07-15 三次复核补充：`L3B_SKILL_PARTIAL_EXT.md` 已把不存在的 `InterruptSkill` 改为 `ClientBreakActiveSkill/BreakSkillEntity`，并明确 `SendPreUseSkillReq()` 定义在 `SkillControllerMsgPartial`，`SkillControllerUserInputPartial` 负责 `SendUserInput()` 决策与预输入缓存。
- 2026-07-15 四次复核补充：`FLOW_SKILL_CAST_FULL.md` / `BATTLE_FRAMEWORK_ARCHITECTURE.md` 已同步手动 `SkillEntityActionPartial` 效果路径；Buff/Bullet/Passive 不再写成 `SkillController*Partial` 是最上游协议入口，而是 `GameManager.HandleRPCMsg -> EntityCtrlMsgBase/CtrlGroup` 后由 partial 处理。
- 2026-07-15 五次复核补充：`SkillEntityActionPartial.PlayStageEffect()` / `TryPlayEffect()` 已通过代码图验证存在；手动阶段效果链为 `OnActionStageTryPlayEffect -> PlayStageEffect -> TryPlayEffect -> RegisterServerEffect/StageTryPlayServerEffect/FuncOnTryPlayClientEffect`。原先同名未核实项已删除。
- 2026-07-15 六次复核补充：`L3B_SKILL_PARTIAL_EXT.md` 已补强 UserInput 边界。代码图确认 `SkillControllerUserInputPartial.SendUserInput()` 会先更新 `_clientInputCache`，在阶段未 `ServerCreateStage` 时写 `_waitSendInputCache` 暂不发协议；技能预输入走 `SkillControllerMsgPartial.SendPreUseSkillReq()`，输入轴预输入走 `SkillMsgUtils.SendPreSkillUseInput()`，取消走 `sendPreSkillUseInputCancelReq()`。`SkillEntityUserInputPartial` 侧负责 `KEY_USER_INPUT` 黑板、客户端/服务器结果合流、`ExecuteSkillUserInput/ExecuteEnergyUserInput` 和蓄力计时事件。
- 2026-07-15 七次复核补充：`FLOW_SKILL_CAST_FULL.md` 业务链路未改；仅把 `FLOW_SKILL_CAST_FULL.svg` 标为历史渲染版本。本地 `mmdc` 不存在，现有 SVG 是 2026-07-10 旧文件，当前事实以 Markdown 内 Mermaid 源为准。
- 2026-07-16 八次复核补充：`L3B_SKILL_PARTIAL_EXT.md` 已补 Energy 延迟输入边界。代码图确认 `ExecuteEnergyUserInput()` 在客户端蓄力不足最小时间时注册 `DelayInvokeExecuteUserInput()`；延迟回调先 `SkillMsgUtils.SendPreSkillUseInput(RuntimeID, inputSkillUseReq, userInput.EffectID)`，再执行 `ExecuteUserInput(...)`，所以输入轴协议发送不只发生在 Controller partial。

# Evidence Table

| md file | claim checked | verdict | code evidence |
|---|---|---|---|
| `L3A_SKILL_ENGINE_CORE.md:48-49` | `SkillComponent.SendUserSkillReq / GameManager.InputVKey -> PlayerCtrlGroup.InputVKey -> SkillController.UseSkill / ClientUseSkill` is one input chain | 需修正 | Manual skill button path: `UniversalButton.OnPointerUp()` invokes `onPointerUp` at `Assets/Scripts/StarGame/UI/SkillBtn/UniversalButton.cs:472,510`; `SkillComponent.OnPointerUp()` calls `SendUserSkillReq()` at `Assets/Scripts/StarGame/Game/Player/Component/SkillComponent.cs:542,553`; `SendUserSkillReq()` calls `SkillController.ClientUseSkill(...)` at `Assets/Scripts/StarGame/Game/Player/Component/SkillComponent.cs:842,1188`. `GameManager.InputVKey()` only forwards to `PlayerCtrlGroup.InputVKey()` at `Assets/Scripts/StarGame/Game/GameManager.cs:2455,2470`, and `PlayerCtrlGroup.InputVKey()` only calls `DoVKey_Move()` at `Assets/Scripts/StarGame/Game/Player/PlayerCtrlGroup.cs:730,734`. No evidence that this path calls `SkillController.UseSkill/ClientUseSkill`. |
| `FLOW_SKILL_RELEASE.md:26-29,46` | skill button release goes through `GameManager.InputVKey -> PlayerCtrlGroup -> SkillDispatcher -> SkillController` | 需修正 | Same evidence as above. `SkillComponent.SendUserSkillReq()` directly reaches `m_NPCEntityBase.skillDispatcher.SkillController.ClientUseSkill(...)` at `Assets/Scripts/StarGame/Game/Player/Component/SkillComponent.cs:1188`; `GameManager.InputVKey()`/`PlayerCtrlGroup.InputVKey()` are VKey/movement dispatch at `GameManager.cs:2455,2470` and `PlayerCtrlGroup.cs:730,734`. |
| `FLOW_SKILL_CAST_FULL.md:13,37-41,127-131,161-162` | local control stage after manual skill request is `GameManager.InputVKey -> PlayerCtrlGroup.InputVKey -> DoVKey_Move -> NPCEntityBase/SkillDispatcher` | 需修正 | `SkillComponent.OnPointerUp()` calls `SendUserSkillReq()` at `SkillComponent.cs:542,553`, then `ClientUseSkill()` at `SkillComponent.cs:1188`. `DoVKey_Move()` is called only from `PlayerCtrlGroup.InputVKey()` at `PlayerCtrlGroup.cs:700,730-734`; it is not the manual skill release continuation. |
| `BATTLE_FRAMEWORK_ARCHITECTURE.md:148-150` | `SkillComp -> GM: SendUserSkillReq / InputVKey`, then `PCG -> SC: SkillController.UseSkill / ClientUseSkill` | 需修正 | `SkillComponent.SendUserSkillReq()` calls `SkillController.ClientUseSkill(...)` directly through `m_NPCEntityBase.skillDispatcher` at `SkillComponent.cs:1188`. `PlayerCtrlGroup.InputVKey()` has no `SkillController` call in its body, only `DoVKey_Move()` at `PlayerCtrlGroup.cs:730-734`. |
| `FLOW_SKILL_CAST_FULL.md:170-180` | auto battle shares the latter skill pipeline through `AutoBattleUseSkill() -> TryUseSkill() -> UseSkill()` | 需修正 | The named methods exist, but `BattleManager.UseSkill()` does not directly call `SkillController.UseSkill`; it simulates a key via `InputManager.Instance.DispatchVKey(keyCodePair.Value, 2, true)` at `Assets/Scripts/StarGame/Service/BattleManager/BattleManager.cs:2180,2189,2257,2268,2524-2536`. `UniversalButton.InputVKey()` treats `arg == 2` as auto battle and calls `OnPointerDown()` then `OnPointerUp()` at `UniversalButton.cs:906,936-942`, which then reaches `SkillComponent.SendUserSkillReq()` through the button callbacks at `UniversalButton.cs:438,510` and `SkillComponent.cs:797-799,842`. |
| `L3A_SKILL_ENGINE_CORE.md:51,78`; `FLOW_SKILL_RELEASE.md:47`; `FLOW_SKILL_CAST_FULL.md:163`; `BATTLE_FRAMEWORK_ARCHITECTURE.md:181` | `SkillDispatcher.EnterFrame()` drives `SkillController` and `SkillUnitController` | 正确 | `SkillDispatcher.Create()` instantiates both controllers at `Assets/Scripts/StarGame/Game/Skill/SkillDispatcher.cs:73-82`; `SkillDispatcher.EnterFrame()` calls `skillController.EnterFrame()` and `skillUnitController.EnterFrame()` at `SkillDispatcher.cs:86-89`. |
| `L3A_SKILL_ENGINE_CORE.md:82`; `L3B_SKILL_PARTIAL_EXT.md:66` | public `SkillController` interface includes `GetSkillStage()` | 需修正 | Public method found is `GetActiveStage()` at `Assets/Scripts/StarGame/Game/Skill/SkillPartial/SkillControllerSkillPartial.cs:1694-1701`. `GetSkillStage(...)` found in `SkillEntity` is private at `Assets/Scripts/StarGame/Game/Skill/SkillEntity.cs:529-539`, not a public `SkillController` method. |
| `L3B_SKILL_PARTIAL_EXT.md:1-45` | `Skill/SkillPartial/` has 12 partial files split between `SkillController` and `SkillEntity` | 正确 | Directory listing shows 12 `.cs` files: 8 `SkillController*Partial.cs` and 4 `SkillEntity*Partial.cs` under `Assets/Scripts/StarGame/Game/Skill/SkillPartial/`. Class declarations confirm partial merge: `SkillController.cs:22`, `SkillControllerBasePartial.cs:17`, `SkillControllerUserInputPartial.cs:9`, `SkillEntity.cs:26`, `SkillEntityActionPartial.cs:23`, `SkillEntityDebugDataPartial.cs:16`. |
| `L3B_SKILL_PARTIAL_EXT.md:43-55,74-79` | Buff/Bullet/Passive partial method distribution | 正确 | Buff: `OnBuffCreateRet()` / `OnBuffRunStage()` / `OnBuffEndRet()` / `EnterFrameBuff()` / `EnterFrameRunnintBuff()` are present in `SkillControllerBuffPartial.cs`; the `Runnint` spelling is the code name. Bullet: `EnterFrameBullet()`, `OnBulletCreateRet()`, `OnBulletRunStage()`, `OnBulletEndRet()` are present in `SkillControllerBulletPartial.cs`. Passive: `EnterFramePassive()`, `OnPassiveSkillUseRet()`, `OnPassiveRunStageRet()`, `OnPassiveSkillEndRet()` are present in `SkillControllerPassivePartial.cs`. |
| `L3B_SKILL_PARTIAL_EXT.md:72-82,91` | UserInput partial boundary | 正确 | Controller side: `UpdateClientInputCache()` writes `_waitSendInputCache` when the active stage has not `ServerCreateStage` at `SkillControllerUserInputPartial.cs:192-219`; `SendUserInput()` dispatches skill pre-use or input pre-use at `SkillControllerUserInputPartial.cs:222-305`; `CancelUserInputReq()` cancels via `sendPreSkillUseInputCancelReq()` at `SkillControllerUserInputPartial.cs:360-380` and `SkillControllerMsgPartial.cs:122-131`; `TriggerServerInput()` / `PrePlayUserInput()` are at `SkillControllerUserInputPartial.cs:597-682`. Entity side: `OnClientUserInputEffect()` / `OnServerInputEffect()` / `ExecuteUserInput()` / `ExecuteServerUserInput()` are at `SkillEntityUserInputPartial.cs:157-828`; Energy methods are `StartEnergy()` / `OnEnergyUpdate()` / `StopEnergy()` at `SkillEntityUserInputPartial.cs:1040-1123`. |
| `L3A_SKILL_ENGINE_CORE.md:102`; `FLOW_SKILL_RELEASE.md:48`; `FLOW_SKILL_CAST_FULL.md:15,51` | `SkillStage` owns stage frame/timeline execution and uses `OnEnter/OnUpdate/OnExit` | 正确 | `SkillStage.Init()` builds `frameEventsQueue`/`frameEventsList` from stage frames at `Assets/Scripts/StarGame/Game/Skill/SkillStage.cs:376-413`; `OnEnter()` executes frame events at `SkillStage.cs:521,630,670`; `OnUpdate()` executes frame events and timeout exit at `SkillStage.cs:790,846,857-860`; `OnExit()` invokes exit handling at `SkillStage.cs:700-715`. |
| `L3A_SKILL_ENGINE_CORE.md:52,104`; `FLOW_SKILL_RELEASE.md:49`; `FLOW_SKILL_CAST_FULL.md:164-165`; `BATTLE_FRAMEWORK_ARCHITECTURE.md:182` | 手动 SkillEntity 效果走 `SkillEntityActionPartial.PlayStageEffect/TryPlayEffect`；ServerControl/Bullet 等阶段路径走 `StageHandle.PlayStageEffect/TryPlayEffect` | 正确 | 手动路径：`SkillEntityActionPartial.OnActionStageTryPlayEffect()` calls `PlayStageEffect()` at `SkillEntityActionPartial.cs:470-473`; `PlayStageEffect()` creates `EffectParam` and calls `TryPlayEffect()` at `SkillEntityActionPartial.cs:505-519`; `TryPlayEffect()` calls `RegisterServerEffect()`, `StageTryPlayServerEffect()`, `FuncOnTryPlayClientEffect.Invoke(...)`, and `EffectResultUtils.UpdateEffectExecuteResult(...)` at `SkillEntityActionPartial.cs:800-849`. StageHandle path remains verified at `StageHandle.cs:406-412,556-584`. |
| `FLOW_SKILL_RELEASE.md:49`; `BATTLE_FRAMEWORK_ARCHITECTURE.md:154` | Buff/Bullet/Passive 运行时实体由上游协议分发后的 SkillController partial 创建，Passive 运行时实体为 `PassiveSkillEntity` | 正确 | `SkillControllerBuffPartial.OnBuffCreateRet()` creates `SkillBuff` and calls `buff.Create(buffCreateRet)` at `SkillControllerBuffPartial.cs:46-59`; `SkillControllerBulletPartial.OnBulletCreateRet()` creates `SkillBullet` and calls `skillBullet.Create(bulletCreateRet)` at `SkillControllerBulletPartial.cs:31-40`; `PassiveSkillEntity.Create()` calls `CreatePassiveInfo()` and creates `PassiveStageHandle` at `Assets/Scripts/StarGame/Game/Skill/PassiveSkillEntity.cs:22,74,88,93`. Protocol entry is upstream of these partials. |
| `FLOW_SKILL_CAST_FULL.md:166,179`; `BATTLE_FRAMEWORK_ARCHITECTURE.md:183` | `EffectUtils.HandleEffectDamage()` lands damage on target `HandleHurtNodeMsg()` | 正确 | `EffectUtils.HandleEffectDamage(...)` is defined at `Assets/Scripts/StarGame/Game/Skill/EffectUtils.cs:1342`; it resolves the target with `GameManager.Instance.GetEntityCtr(hurtData.TargetID)` at `EffectUtils.cs:1349`, then calls `entityCtrlGroup.M_Curr.HandleHurtNodeMsg(...)` at `EffectUtils.cs:1423`. |
| `L3A_SKILL_ENGINE_CORE.md:105`; `BATTLE_FRAMEWORK_ARCHITECTURE.md:167` | skill blackboard is per-skill and passed into stages | 正确 | `SkillEntity.Create()` creates `skillBlackBoard = new SkillBlackBoard()` and writes `KEY_CFG_ID` at `SkillEntity.cs:112-128`; `SkillEntity.CteateSkillStage()` passes `skillBlackBoard` into `SkillStage.Init()` at `SkillEntity.cs:1159-1165`; `SkillEntity.Release()` clears and nulls the blackboard at `SkillEntity.cs:964,1010-1011`. |
| `L3A_SKILL_ENGINE_CORE.md:106` | `SkillDispatcher` is a unified receiver of input and timing | 需修正 | Timing/frame dispatch is correct (`SkillDispatcher.EnterFrame()` at `SkillDispatcher.cs:86-89`). Input receipt is not: manual input is handled by `SkillComponent.SendUserSkillReq()` and calls `SkillController.ClientUseSkill()` at `SkillComponent.cs:842,1188`; VKey movement is handled by `GameManager.InputVKey()`/`PlayerCtrlGroup.InputVKey()` at `GameManager.cs:2455,2470` and `PlayerCtrlGroup.cs:730-734`. No `SkillDispatcher` input-receive method was found. |
| `BATTLE_FRAMEWORK_ARCHITECTURE.md:15,197-198` | core skill chain is `SkillDispatcher -> SkillController / SkillUnitController -> SkillContainer -> SkillEntity / SkillStage -> StageHandle / SkillEntityActionPartial -> EffectUtils` | 正确 | `SkillDispatcher.EnterFrame()` drives controllers at `SkillDispatcher.cs:86-89`; `SkillController.EnterFrame()` updates skill entities and Buff/Bullet/Passive at `Assets/Scripts/StarGame/Game/Skill/SkillController.cs:73-83`; `SkillEntity.EnterFrame()` calls `OnUpdate()` at `SkillEntity.cs:480-484`; `SkillStage` invokes stage effect actions at `SkillStage.cs:1190-1206`; manual `SkillEntityActionPartial.TryPlayEffect()` reaches client/server effect handling at `SkillEntityActionPartial.cs:800-849`; `StageHandle` remains the ServerControl/Bullet-style path at `StageHandle.cs:556-584`. |

# Required Fixes

1. Original: `L3A_SKILL_ENGINE_CORE.md:48-49`

   Problem: combines two different entry paths and incorrectly routes manual skill release through `GameManager.InputVKey` / `PlayerCtrlGroup.InputVKey`.

   Code evidence: `SkillComponent.cs:542,553,842,1188`; `GameManager.cs:2455,2470`; `PlayerCtrlGroup.cs:730-734`.

   Suggested replacement:

   ```text
   Manual skill button:
     UniversalButton.OnPointerDown/OnPointerUp
       -> SkillComponent.OnPointerDown/OnPointerUp
       -> SkillComponent.SendUserSkillReq(...)
       -> m_NPCEntityBase.skillDispatcher.SkillController.ClientUseSkill(...)
       -> SkillController.UseSkill(...) / UseNewSkill(...)

   VKey movement:
     GameManager.InputVKey(...)
       -> PlayerCtrlGroup.InputVKey(...)
       -> DoVKey_Move(...)
   ```

2. Original: `FLOW_SKILL_RELEASE.md:26-29,46`; `FLOW_SKILL_CAST_FULL.md:13,37-41,127-131,161-162`; `BATTLE_FRAMEWORK_ARCHITECTURE.md:148-150`

   Problem: sequence diagrams show `SkillComponent -> GameManager.InputVKey -> PlayerCtrlGroup -> SkillController`, but manual skill release does not follow that chain.

   Code evidence: `UniversalButton.cs:438,510`; `SkillComponent.cs:797-799,842,1188`; `PlayerCtrlGroup.cs:730-734`.

   Suggested replacement:

   ```text
   UI/manual cast chain:
     UniversalButton callbacks
       -> SkillComponent.OnPointerDown/OnPointerUp
     -> SkillComponent.SendUserSkillReq
     -> SkillController.ClientUseSkill
     -> SkillController.UseSkill / UseNewSkill
     -> SkillController.CreateSkillEntity
     -> SkillEntity.ClientUseSkill
     -> OnClientPreEnter -> EnterCurStage
     -> SkillStage.OnEnter/OnUpdate
    -> SkillEntityActionPartial.PlayStageEffect / TryPlayEffect
   ```

3. Original: `FLOW_SKILL_CAST_FULL.md:170-180`

   Problem: auto battle is described as directly reusing the same pipeline through `UseSkill()`, but the actual auto path simulates a virtual key that triggers button down/up.

   Code evidence: `BattleManager.cs:2180,2189,2257,2268,2524-2536`; `UniversalButton.cs:906,936-942`.

   Suggested replacement:

   ```text
   自动战斗入口：
     AutoBattleUseSkill() -> TryUseSkill() -> BattleManager.UseSkill()
       -> InputManager.Instance.DispatchVKey(keyCode, 2, true)
       -> UniversalButton.InputVKey(arg == 2)
       -> OnPointerDown(null) + OnPointerUp(null)
       -> SkillComponent.SendUserSkillReq(...)
   ```

4. Original: `L3A_SKILL_ENGINE_CORE.md:82`; `L3B_SKILL_PARTIAL_EXT.md:66`

   Problem: public interface lists `SkillController.GetSkillStage()`, but verified public method is `GetActiveStage()`.

   Code evidence: `SkillControllerSkillPartial.cs:1694-1701`; private `SkillEntity.GetSkillStage()` at `SkillEntity.cs:529-539`.

   Suggested replacement:

   ```text
   SkillController（施法和状态机）
   - UseSkill(...) / ClientUseSkill(...) / ClientBreakActiveSkill(...) / BreakSkillEntity(...) / GetActiveStage()
   ```

5. Original: `L3A_SKILL_ENGINE_CORE.md:106`

   Problem: says `SkillDispatcher` uniformly receives input and timing. It uniformly handles controller creation/frame/reset/release, but not input receipt.

   Code evidence: `SkillDispatcher.cs:73-89,97-120`; input evidence `SkillComponent.cs:842,1188`.

   Suggested replacement:

   ```text
   SkillDispatcher 是每个实体的技能运行时聚合点：创建并持有 SkillController / SkillUnitController，并在 EnterFrame 中推进两者。技能输入由 SkillComponent 或自动战斗按钮模拟进入 SkillController.ClientUseSkill，不是 SkillDispatcher 直接接收。
   ```

# Unverified Claims

- `GameInput (L0)` as a direct participant in skill release diagrams was not verified in the searched evidence. Verified VKey consumers are `UniversalButton.InputVKey()` / `AnalogStick.InputVKey()` via `InputManager.Instance.OnVirtualInput`, and `GameManager.InputVKey()` for `PlayerCtrlGroup.InputVKey()`.

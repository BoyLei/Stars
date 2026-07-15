# Scope

Reviewed battle docs for effect/damage/Buff/Bullet/Passive accuracy:

- `diagrams/battle/L4_COMBAT_EFFECT.md`
- `diagrams/battle/FLOW_DAMAGE_PIPELINE.md`
- `diagrams/battle/FLOW_BUFF_LIFECYCLE.md`
- effect/Buff/Bullet/Passive sections of `diagrams/battle/BATTLE_FRAMEWORK_ARCHITECTURE.md`

Primary code evidence came from CodeGraph for `SkillBuff`, `SkillBullet`, `PassiveSkillEntity`, `PassiveInfo`, `BuffStageHandle`, `StageHandle`, `ServerControlStageEntityBase`, `BattleManager`, and TypeEffect classes. Shell reads were used only for md files, CodeGraph-truncated ranges, and the CodeGraph-stale `SkillControllerBuffPartial.cs`.

# Follow-up 2026-07-13

- `FLOW_DAMAGE_PIPELINE.md` 追加复核：`FuncOnTryPlayClientEffect` 不直接进入 `EffectUtils`，而是回到 `SkillController.StageTryPlayClientEffect()` / `TryPlayClientEffect()`，再经 `NPCEntityBase.PlayClientSkillEffect()` 分支派发；`Damage` 先到 `NPCEntityBase.HandleClientEffectDamage()`，再按黑板 `HurtNodeMsg` 或目标数组分别进入 `EffectUtils.HandleEffectDamage()` / `HandleEffectDamageTar()`。
- `FLOW_DAMAGE_PIPELINE.md` 追加复核：`TypeEffectFactory.Create(GlobalShowSerialize)` 的已验证调用方是 `SkillBullet.PlayEffects()` / `ServerControlStageEntityBase.PlayEffects()`，不是 `EffectUtils`。
- `FLOW_DAMAGE_PIPELINE.md` 追加复核：`DamageEntity.Create()` 只保存 `DamageData/HurtData`、加载 `DamageViewNew` 并 `BindDamageEntity()`；伤害数值来自服务器 HurtData 经 `BattleManager.OnHurtData()` 组装的 `DamageData`。
- `FLOW_DAMAGE_PIPELINE.md` 二次复核：`SkillBullet.Create(BulletCreateRet)` 用 `hasCreate` 防止服务器运行时创建与 AOI 创建通知重复播放子弹特效；创建路径写 Runtime/Bullet/Owner/Builder 后执行 `CreateStageHandle()` / `CreateBulletInfo()` / `HandleBlackList(...)` / `PlayCreateLoopEffects(...)` / `PlayEffects(...)`。
- `FLOW_DAMAGE_PIPELINE.md` 二次复核：`OnBulletRunStage` 的上游由 `trace_path` 证实为 `NPCEntityBase.OnRunStageRet -> SkillController.OnServerRunStage -> OnBulletRunStage`；`OnBulletEndRet()` 则是 `SkillControllerBulletPartial.OnBulletEndRet -> SkillBullet.OnBulletEndRet -> ReleaseBullet()`。
- `FLOW_DAMAGE_PIPELINE.md` 二次复核：`SkillBullet` 目录定向搜索 `OnHitTarget|HitTarget|OnCollision|OnTrigger|SkillUtils|HandleEffectDamage` 结果为 0；因此本地命中/碰撞/直接伤害仍不能写成 Bullet 的真实 API。
- `FLOW_DAMAGE_PIPELINE.md` 三次复核：`EffectUtils.HandleEffectDamage()` 在 `EffectUtils.cs:1342-1424` 消费 `HurtNodeMsg/HurtData`、播放受击特效/动作并调用 `HandleHurtNodeMsg(...)`；`HandleEffectDamageTar()` 在 `EffectUtils.cs:1434-1476` 只按 `SkillTarData` 播放受击特效/动作，未调用 `HandleHurtNodeMsg()`；`HandleEffectDamageSecond()` 在 `EffectUtils.cs:1488-1506` 只调用 `HandleHurtNodeMsg(...)`。
- `FLOW_DAMAGE_PIPELINE.md` 三次复核：`AOIEntityObject.HandleHurtNodeMsg()` 在 `AOIEntityObject.cs:574-577` 只触发 `ActionOnHurtNodeMsg`；`NPCEntityBase.OnHurtNodeMsg()` 在 `NPCEntityBase.cs:866-918` 调用 `BattleManager.OnHurtData(...)`，并处理主角受击索敌/头顶血条/GM 目标等副作用。
- `FLOW_DAMAGE_PIPELINE.md` 三次复核：`BattleManager.OnHurtData(HurtData,...)` 在 `BattleManager.cs:299-372` 按命中、护盾、吸血、反伤、miss 组装 `DamageData`；`PlayDamageText()` 在 `BattleManager.cs:494-507` 创建 `DamageEntity`；`DamageEntity.Create()/CreateGob()` 在 `DamageEntity.cs:46-96` 保存数据、加载 `Effects/Damage/DamageViewNew` 并绑定 `DamageViewNew.BindDamageEntity()`。
- `BATTLE_FRAMEWORK_ARCHITECTURE.md` 三次复核：总图已把原先合并的 `DamageEntity / HandleHurtNodeMsg` 拆为 `HandleHurtNodeMsg -> BattleManager.OnHurtData` 和 `DamageEntity / DamageViewNew` 两段，避免误读为 `EffectUtils -> DamageEntity -> Entity`。
- `FLOW_DAMAGE_PIPELINE.md` / `FLOW_BUFF_LIFECYCLE.md` 追加复核：`SkillStage.ExitStage(E_SkillStageExitType)` 存在；`BuffStageHandle` 只验证到 `OnCreate()` / `OnActionExitStage()` / `OnActionStageStartCD()` 等覆写点。
- `FLOW_BUFF_LIFECYCLE.md` 追加复核：`SkillBuff.EnterFrame()` 每帧调用 `OnFrameInit()`，但 `PlayBuffEffects()` / `PlayCreateLoopEffects()` 只在 `needExecuteFrameInit == true` 的首次初始化分支执行，随后清除标记。
- `FLOW_BUFF_LIFECYCLE.md` 追加复核：`OnBuffEndRet()` 只标记 ServerClose/ClientClose 并触发 BuffEnd；真正释放由后续 `EnterFrameBuff()` 收集 `!IsRunning`，再走 `ReleaseTempBuffs()` / `ReleaseItemBuff()` / `EntityFactory.ReleaseEntity()`。
- `FLOW_BUFF_LIFECYCLE.md` 追加复核：`SkillStage.OnRunStageRet()` 对 StartTime/LiveTime/StackCount/ShieldVal 等特殊 key 只触发 `ActionOnSpecialServerBlackBoardNode?.Invoke(...)`；定向搜索只发现字段定义、Invoke 和 `ReleaseAction()` 清空，未发现 `+=` 或非清空赋值注册点。
- `FLOW_BUFF_LIFECYCLE.md` 追加复核：`Replace` / `MaxFloor` 在本轮 StarGame 运行时代码范围内未验证为客户端本地合并逻辑；当前可证结论是 `buffDic` 按 `RuntimeID` 管理，`StackCount` 由服务器黑板同步。
- `FLOW_BUFF_LIFECYCLE.md` 追加复核：`AttacState.cs` 与 `AttackState.cs` 都定义为 `VitalState`；`AttacState.OnEnable()` 播放攻击动画，`AttackState.OnEnable()` 的播放逻辑被注释。资产 GUID 无引用不属于本轮代码行证据。
- `FLOW_BUFF_LIFECYCLE.md` 二次复核：codebase-memory `trace_path` 证实 `EnterFrameBuff` 的上游是 `SkillDispatcher.EnterFrame -> SkillController.EnterFrame -> EnterFrameBuff`；`SkillControllerBuffPartial.EnterFrameBuff()` 先释放 `!IsRunning` 的 Buff，再驱动仍运行的 Buff。
- `FLOW_BUFF_LIFECYCLE.md` 二次复核：`SkillBuff.Create(BuffCreateRet, VitalSignData)` 的代码落点是运行时初始化、`CreateStageHandle()`、`InitBuff(...)` 和 `TriggerBuffChange(..., "buffCreate", stackCount)`；不是 AddBuff 方法。
- `FLOW_BUFF_LIFECYCLE.md` 二次复核：`SkillBuff.HandleBuffBlackBoardNode()` 明确处理 `StartTime/LiveTime/StackCount/ShieldVal`；当前代码中 `ShieldVal` 分支写入 `shieldVal`，但通知调用传的是 `stackCount`，该文档只记录代码现状，不推断设计意图。
- `FLOW_BUFF_LIFECYCLE.md` 二次复核：`ActionOnSpecialServerBlackBoardNode\s*(\+=|=)` 只命中 `SkillStage.ReleaseAction()` 清空赋值；`SkillBuff.AddBuff` / `SkillBuff.RemoveBuff` / `PassiveInfo.TriggerPassive` 精确/近似方法名搜索结果为 0。
- `L4_COMBAT_EFFECT.md` 追加同步：当前根文档已改为 `PassiveSkillEntity -> PassiveInfo / PassiveStageHandle`，Buff 结束链已改为 `OnBuffEndRet` 标记状态、后续 `EnterFrameBuff -> ReleaseTempBuffs` 延迟释放。
- 2026-07-15 三次复核补充：Buff 协议入口不在 `SkillControllerBuffPartial`；真实上游是 `GameManager.HandleRPCMsg -> EntityCtrlMsgBase/CtrlGroup -> SkillControllerBuffPartial`。`FLOW_BUFF_LIFECYCLE.md` 和 `L4_COMBAT_EFFECT.md` 已把 `SkillControllerBuffPartial` 收紧为 CtrlGroup 之后的 Buff 运行时处理器。
- `BATTLE_FRAMEWORK_ARCHITECTURE.md` / `BATTLE_OVERVIEW.md` 追加同步：摘要图已把 Passive 运行时实体改为 `PassiveSkillEntity`，把 Buff/Bullet/Passive 运行时处理边界改为上游 CtrlGroup 转交后的 `SkillController*Partial`，把 DamageEntity 改为 `BattleManager.OnHurtData/PlayDamageText` 飘字链路，并把 TypeEffect 示例改为代码中可证的映射口径。
- 2026-07-15 追加复核：`FLOW_DAMAGE_PIPELINE.md` / `L4_COMBAT_EFFECT.md` 已把阶段效果入口拆成手动 `SkillEntityActionPartial.PlayStageEffect/TryPlayEffect` 与 ServerControl/Bullet 等 `StageHandle.PlayStageEffect/TryPlayEffect` 两条，不再把 `StageHandle` 写成全部阶段效果的唯一入口。
- 2026-07-15 四次复核：`NPCEntityBase.HandleClientEffectDamage()` 先检查服务器执行结果，再尝试三参 `NPCEntityBase.HandleEffectDamage(effectParam, blackBoard, true)` 读取黑板 `HurtNodeMsg` 并调用 `EffectUtils.HandleEffectDamage(...)`；失败后才走 `HandleEffectDamageTargetArr()` / `EffectUtils.HandleEffectDamageTar(...)`。`DamageSecond` 是 `HandleClientDamageSecond()` / `EffectUtils.HandleEffectDamageSecond(...)` 的独立分支，`FLOW_DAMAGE_PIPELINE.md` 已同步收紧。
- 2026-07-15 五次复核：`FLOW_BUFF_LIFECYCLE.md` / `FLOW_DAMAGE_PIPELINE.md` 已补 Buff 更新通知边界。`SkillBuff.HandleBuffBlackBoardNode()` 对 `StartTime/LiveTime/StackCount/ShieldVal` 只写本地字段、调用 `playerData.TriggerBuffChange(...)` 并置 `dirtyUpdateBuff`；`SkillBuff.EnterFrame()` 再转 `ActionOnUpdateBuff -> SkillController.ActionOnBuffUpdate`。`EntityBaseData.TriggerBuffChange()` 只是按 `buff_{id}` 派发 `BuffEventData`，本轮未发现它直接改 `Attrs` 或参与伤害数值计算。
- 2026-07-15 六次复核：`FLOW_BUFF_LIFECYCLE.md` 已把“存疑点”改成已验证边界。代码图确认 `buffDic` 以 `RuntimeID` 为 key；`OnBuffCreateRet()` 只处理 `RuntimeID` 重复释放，不按 `BuffID` 合并；`StackCount` 来自服务器黑板并触发 Buff 变化通知。`Replace/MaxFloor` 仍只作为配置侧字段保留边界，不写成客户端本地叠层/替换逻辑。
- 2026-07-16 七次复核：`BATTLE_FRAMEWORK_ARCHITECTURE.md` 的伤害时序显式补出 `BattleManager` 参与者：`EffectUtils.HandleEffectDamage -> AOIEntityObject.HandleHurtNodeMsg -> NPCEntityBase.OnHurtNodeMsg -> BattleManager.OnHurtData -> PlayDamageText -> DamageEntity/DamageViewNew`。`FLOW_DAMAGE_PIPELINE.md` 同步了 Bullet/Buff 方法枚举顺序：创建时先 `CreateStageHandle()`，再 `CreateBulletInfo()` / `InitBuff(...)`。
- 本报告下方 `Required Fixes` 是当时发现的问题清单；当前根文档与 FLOW/L4 文档已按这些证据同步，保留该清单作为审查过程记录。

# Evidence Table

| md file | claim checked | verdict | code evidence |
|---|---|---|---|
| `L4_COMBAT_EFFECT.md:28-31` | `PassiveInfo -> PassiveStageHandle` in module graph | 需修正 | `PassiveSkillEntity.Create()` calls `CreateStageHandle()` at `Assets/Scripts/StarGame/Game/Skill/PassiveSkillEntity.cs:72`; `CreateStageHandle()` instantiates `PassiveStageHandle` and calls `SetStageHandle(handle)` at `PassiveSkillEntity.cs:86-91`. `PassiveInfo` only loads config in `Assets/Scripts/StarGame/Game/Skill/Passive/PassiveInfo.cs:25-64`. |
| `L4_COMBAT_EFFECT.md:35-41` | `StageHandle.PlayStageEffect / TryPlayEffect` directly branches into `SkillBuff.Create`, `SkillBullet.Create`, `PassiveSkillEntity.CreatePassiveInfo` | 需修正 | `StageHandle.PlayStageEffect()` wraps `EffectData` into `EffectParam` then calls `TryPlayEffect()` at `Assets/Scripts/StarGame/Game/Skill/StageHandle.cs:406-412`; `TryPlayEffect()` registers/tries server effects and invokes `FuncOnTryPlayClientEffect` at `StageHandle.cs:556-587`. Runtime creation is driven by controller protocol callbacks: `OnBuffCreateRet()` -> `buff.Create(...)` at `SkillControllerBuffPartial.cs:46-62`, `OnBulletCreateRet()` -> `skillBullet.Create(...)` at `SkillControllerBulletPartial.cs:31-40`, `OnPassiveSkillUseRet()` -> `passiveSkillEntity.Create(...)` at `SkillControllerPassivePartial.cs:50-68`. |
| `L4_COMBAT_EFFECT.md:52-60` | Buff chain says `BSH Tick()` and `BSH -> BI 应用属性/效果` | 需修正 | `BuffStageHandle` has overrides only for create/exit/startCD/active/reset in `Assets/Scripts/StarGame/Game/Skill/Buff/BuffStageHandle.cs:15-66`; no `Tick()` method is present there. Actual per-frame driver is `SkillController.EnterFrameBuff()` -> `buff.EnterFrame()` at `SkillControllerBuffPartial.cs:153-183`, then `SkillBuff.EnterFrame()` -> `OnFrameInit()` at `Assets/Scripts/StarGame/Game/Skill/Buff/SkillBuff.cs:213-223`. |
| `L4_COMBAT_EFFECT.md:71` | Bullet hit callback returns to L3 and EffectUtils executes effect | 未证实 | Verified Bullet stage/network flow: `SkillBullet.EnterFrame()` updates stage lists at `Assets/Scripts/StarGame/Game/Skill/Bullet/SkillBullet.cs:262-280`; `OnRunStageRet()` locates/creates stages and calls `stage.OnServerRunStageRet()` at `SkillBullet.cs:333-348`. Targeted search in `SkillBullet.cs` found no `SkillUtils`, `HandleEffectDamage`, or hit callback method. |
| `L4_COMBAT_EFFECT.md:82-85` | 旧说法把 Buff 写成 Tick、Bullet 写成本地即时命中、Passive 写成条件事件 | 需修正 | Buff is server-created and frame-driven via `OnBuffCreateRet()`/`EnterFrameBuff()` at `SkillControllerBuffPartial.cs:46-62` and `153-183`; Bullet is server-created/stage-driven via `OnBulletCreateRet()`/`OnBulletRunStage()` at `SkillControllerBulletPartial.cs:31-40` and `83-108`; Passive is server-use/stage-driven via `OnPassiveSkillUseRet()`/`OnPassiveRunStageRet()` at `SkillControllerPassivePartial.cs:50-68` and `87-99`. |
| `L4_COMBAT_EFFECT.md:83` | `SkillBuff/PassiveInfo` inherit `ServerControlStageEntityBase` | 需修正 | `SkillBuff : ServerControlStageEntityBase` in CodeGraph source at `Assets/Scripts/StarGame/Game/Skill/Buff/SkillBuff.cs:42`; `PassiveInfo : BaseConfigInfo` at `Assets/Scripts/StarGame/Game/Skill/Passive/PassiveInfo.cs:10`; `PassiveSkillEntity : ServerControlStageEntityBase` at `Assets/Scripts/StarGame/Game/Skill/PassiveSkillEntity.cs:22`. |
| `FLOW_DAMAGE_PIPELINE.md:23-25` | EffectUtils creates `SkillBullet` during stage effect execution | 需修正 | `StageHandle.TryPlayEffect()` calls client/server effect delegates at `Assets/Scripts/StarGame/Game/Skill/StageHandle.cs:556-587`; `SkillBullet` creation is in `SkillController.OnBulletCreateRet()` at `Assets/Scripts/StarGame/Game/Skill/SkillPartial/SkillControllerBulletPartial.cs:31-40`. |
| `FLOW_DAMAGE_PIPELINE.md:27-34` | Bullet does collision detection via `SkillUtils`, hit target then returns to EffectUtils | 未证实 | `SkillBullet.EnterFrame()` only calls `EnterFrameStages(normalStages/otherStages/bulletStages)` at `Assets/Scripts/StarGame/Game/Skill/Bullet/SkillBullet.cs:262-280`; `OnRunStageRet()` handles server stage notification at `SkillBullet.cs:333-348`. Targeted search in `SkillBullet.cs` found no `SkillUtils` or `HandleEffectDamage`. |
| `FLOW_DAMAGE_PIPELINE.md:34-36` | `EffectUtils` calculates damage and creates `DamageEntity`; `DamageEntity` applies damage/attribute modification | 需修正 | `EffectUtils.HandleEffectDamage()` consumes `HurtNodeMsg`, finds target entity, plays hit FX/action, then calls `entityCtrlGroup.M_Curr.HandleHurtNodeMsg(...)` at `Assets/Scripts/StarGame/Game/Skill/EffectUtils.cs:1342-1424`; second damage similarly calls `HandleHurtNodeMsg(...)` at `EffectUtils.cs:1488-1506`. `BattleManager.PlayDamageText()` creates `DamageEntity` for floating text at `Assets/Scripts/StarGame/Service/BattleManager/BattleManager.cs:494-507`, not for applying numeric damage. |
| `FLOW_DAMAGE_PIPELINE.md:37-38` | Hit creates `FreezeBuffEffect/ShadowFollowBuffEffect` through TypeEffect | 需修正 | `TypeEffectFactory.Type2Effects` maps `BUFF_ShadowFollow` to `ShadowFollowBuffEffect`, `BUFF_Hide` to `InvisibleBuffEffect`, `BUFF_Paralysis` to `ParalysisBuffEffect`, etc. at `Assets/Scripts/StarGame/Game/TypeEffect/TypeEffect/TypeEffectFactory.cs:21-42`; no `FreezeBuffEffect` mapping is present in this factory. |
| `FLOW_BUFF_LIFECYCLE.md:16-21` | Buff creation is from EffectUtils, BSH writes state and creates visual TypeEffect | 需修正 | Buff creation is `OnBuffCreateRet()` -> `SkillBuff.Create()` at `SkillControllerBuffPartial.cs:46-62`; blackboard handling is in `SkillBuff.InitBuff()`/`HandleBuffBlackBoardNode()` at `Assets/Scripts/StarGame/Game/Skill/Buff/SkillBuff.cs:146-164` and `343-416`; global visual effects are played by `SkillBuff.PlayBuffEffects()` -> `PlayEffects(...)` at `SkillBuff.cs:312-318`, with actual factory call in `ServerControlStageEntityBase.PlayEffects()` at `Assets/Scripts/StarGame/Game/Skill/Base/ServerControlStageEntityBase.cs:610-630`. |
| `FLOW_BUFF_LIFECYCLE.md:22-32` | Loop says `SB -> BSH: Tick`, BSH applies attributes, clears state, releases visual effects | 需修正 | Per-frame path is `EnterFrameBuff()` -> `buff.EnterFrame()` at `SkillControllerBuffPartial.cs:153-183`; `SkillBuff.EnterFrame()` calls `OnFrameInit()` and update notification at `SkillBuff.cs:213-223`. End path is `SkillController.OnBuffEndRet()` -> `skillBuff.OnBuffEndRet()` at `SkillControllerBuffPartial.cs:130-143`; `SkillBuff.OnBuffEndRet()` marks server/client close states at `SkillBuff.cs:434-474`; visual stop/reset is in `SkillBuff.Reset()` via `StopBuffEffects()`/`PlayCreateLoopEffects(..., false)`/`ExecuteStageStates(..., false)` at `SkillBuff.cs:481-490`. |
| `FLOW_BUFF_LIFECYCLE.md:39` | Buff state marker is written to `SkillBlackBoard` | 未证实 | Code evidence verifies `SkillBuff.HandleBuffBlackBoardNode()` handles `StartTime`, `LiveTime`, `StackCount`, `ShieldVal`, etc. at `Assets/Scripts/StarGame/Game/Skill/Buff/SkillBuff.cs:355-416`; exact `SkillBlackBoard` write target was not proven in this pass. |
| `BATTLE_FRAMEWORK_ARCHITECTURE.md:48-53` and `160-170` | Effect runtime entity is `PassiveInfo`; common runtime is `SkillBuff / SkillBullet / PassiveInfo` | 需修正 | Runtime passive entity is `PassiveSkillEntity : ServerControlStageEntityBase` at `Assets/Scripts/StarGame/Game/Skill/PassiveSkillEntity.cs:22`; `PassiveInfo` is config holder `BaseConfigInfo` at `Assets/Scripts/StarGame/Game/Skill/Passive/PassiveInfo.cs:10` and loads `PassiveSkillConfig` at `PassiveInfo.cs:32-64`. |
| `BATTLE_FRAMEWORK_ARCHITECTURE.md:153-157` | `StageHandle -> EffectUtils -> SkillBuff.Create / SkillBullet.Create / PassiveSkillEntity.CreatePassiveInfo`; EffectUtils handles damage then target | 需修正 | Stage effect dispatch is `PlayStageEffect()`/`TryPlayEffect()` at `Assets/Scripts/StarGame/Game/Skill/StageHandle.cs:406-412` and `556-587`; create events are controller protocol callbacks in `SkillControllerBuffPartial.cs:46-62`, `SkillControllerBulletPartial.cs:31-40`, `SkillControllerPassivePartial.cs:50-68`; damage landing is `EffectUtils.HandleEffectDamage()` -> `HandleHurtNodeMsg(...)` at `Assets/Scripts/StarGame/Game/Skill/EffectUtils.cs:1342-1424`. |
| `BATTLE_FRAMEWORK_ARCHITECTURE.md:182-184` | Code anchors for `StageHandle.cs:406`, `EffectUtils.cs:1342`, `SkillBuff.cs:213` | 正确 | `StageHandle.PlayStageEffect()` starts at `Assets/Scripts/StarGame/Game/Skill/StageHandle.cs:406`; `EffectUtils.HandleEffectDamage()` starts at `Assets/Scripts/StarGame/Game/Skill/EffectUtils.cs:1342`; `SkillBuff.EnterFrame()` starts at `Assets/Scripts/StarGame/Game/Skill/Buff/SkillBuff.cs:213`. |

# Required Fixes

1. `L4_COMBAT_EFFECT.md:28-31`
   - Replace `PI --> PSH` with `PassiveSkillEntity --> PassiveInfo` and `PassiveSkillEntity --> PassiveStageHandle`.

2. `L4_COMBAT_EFFECT.md:35-41`
   - Suggested replacement:
     `手动 SkillEntity 阶段由 SkillEntityActionPartial.PlayStageEffect() -> TryPlayEffect() 派发客户端/服务器效果线；ServerControl/Bullet 等阶段由 StageHandle.PlayStageEffect() -> TryPlayEffect() 派发；Buff/Bullet/Passive 运行时实体由 SkillController 的 OnBuffCreateRet / OnBulletCreateRet / OnPassiveSkillUseRet 根据服务器通知创建。`

3. `L4_COMBAT_EFFECT.md:52-60`
   - Suggested replacement:
     `SkillController.EnterFrameBuff() 每帧调用 SkillBuff.EnterFrame(); SkillBuff.OnFrameInit() 负责延后一帧播放 Buff 全局表现、循环特效和状态；BuffStageHandle 只承接 StageHandle 覆写点，不存在 Tick()。`

4. `L4_COMBAT_EFFECT.md:82-85`
   - Suggested replacement:
     `共同模式是：服务器通知创建/同步/结束 -> SkillController partial 分发 -> SkillBuff/SkillBullet/PassiveSkillEntity 持有 Info + StageHandle -> EnterFrame/RunStageRet 推进阶段。`

5. `L4_COMBAT_EFFECT.md:83`
   - Replace `SkillBuff/PassiveInfo 继承 ServerControlStageEntityBase` with `SkillBuff/PassiveSkillEntity 继承 ServerControlStageEntityBase；PassiveInfo 继承 BaseConfigInfo，仅承载被动配置。`

6. `FLOW_DAMAGE_PIPELINE.md:23-36`
   - Suggested replacement:
     `StageHandle.TryPlayEffect() 通过 FuncOnTryPlayClientEffect / TryPlayRegistedServerEffect 进入效果逻辑；Bullet 运行时由 OnBulletCreateRet 创建，并通过 OnBulletRunStage / OnRunStageRet 推进阶段。伤害落地由 EffectUtils.HandleEffectDamage(服务器 HurtNodeMsg) 调用目标实体 HandleHurtNodeMsg。`

7. `FLOW_DAMAGE_PIPELINE.md:37-38`
   - Replace `FreezeBuffEffect/ShadowFollowBuffEffect` with `TypeEffectFactory 当前映射的 BaseTypeEffect，如 ShadowFollowBuffEffect、InvisibleBuffEffect、ParalysisBuffEffect、KnockDownBuffEffect、ShaderChangeEffect 等。`

8. `FLOW_BUFF_LIFECYCLE.md:16-32`
   - Suggested replacement:
     `OnBuffCreateRet -> SkillBuff.Create -> CreateStageHandle -> InitBuff；配置加载后下一帧 OnFrameInit 播放 Buff 全局表现/循环特效/状态。OnBuffEndRet 先标记 ServerClose，并按 EndBuffStage/Break 情况标记 ClientClose；最终 Release/Reset 停止表现并回收。`

9. `FLOW_BUFF_LIFECYCLE.md:39`
   - Replace hard assertion with cautious text:
     `Buff 会处理服务器同步来的黑板键（StartTime/LiveTime/StackCount/ShieldVal 等）并触发 UI/状态更新；具体跨 Stage 查询使用的黑板归属需另查 ServerControlStageEntityBase/SkillStage 黑板写入链。`

10. `BATTLE_FRAMEWORK_ARCHITECTURE.md:48-53` and `160-170`
    - Replace `PassiveInfo` as runtime entity with `PassiveSkillEntity`; keep `PassiveInfo` as config node under Passive.

11. `BATTLE_FRAMEWORK_ARCHITECTURE.md:153-157`
    - Suggested replacement:
      `SkillEntityActionPartial 负责手动 SkillEntity 阶段的 EffectData -> EffectParam 和客户端/服务器效果线派发；StageHandle 负责 ServerControl/Bullet 等阶段的同类派发；Buff/Bullet/Passive 的协议入口在 GameManager/EntityCtrlMsgBase/CtrlGroup，上游转交后由 SkillController partial 创建运行时实体；伤害表现由 EffectUtils.HandleEffectDamage 消费 HurtNodeMsg 并转交目标实体 HandleHurtNodeMsg。`

# Unverified Claims

- `SkillBullet` 本地碰撞检测（圆/矩形/扇形）和“命中回调回到 EffectUtils”：本次在 `SkillBullet.cs` 的定向搜索未发现 `SkillUtils`、`HandleEffectDamage` 或明确 hit callback。
- Buff 属性修改是直接改 Entity 字段还是经 SkillBlackBoard 中转：已验证 Buff 处理若干黑板键，但未完整追完 `ServerControlStageEntityBase`/`SkillStage`/效果执行到属性修改的链路。
- 伤害数值计算所在地：`EffectUtils.HandleEffectDamage()` 使用服务器 `HurtNodeMsg/HurtData` 并调用 `HandleHurtNodeMsg`，`BattleManager.OnHurtData()` 只组装飘字所需 `DamageData`，未证明客户端在该链路进行数值计算。

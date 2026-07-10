# 时序图：技能释放链路 (Skill Release Flow)

> 跨层调用链：L0 输入 → L2 控制 → L3 引擎 → L3b 分部 → L4 效果

```mermaid
sequenceDiagram
    participant UI as 技能按钮UI
    participant GI as GameInput (L0)
    participant GM as GameManager (L0)
    participant PCG as PlayerCtrlGroup (L2)
    participant SC as SkillComponent (L2)
    participant SD as SkillDispatcher (L3)
    participant SUC as SkillUnitController (L3)
    participant Ctrl as SkillController (L3+L3b)
    participant SE as SkillEntity (L3+L3b)
    participant SS as SkillStage (L3)
    participant SH as StageHandle (L3)
    participant EU as EffectUtils (L3)
    participant L4 as Buff/Bullet/Passive (L4)

    UI->>SC: OnPointerDown(skillUnit)
    SC->>SC: g_SkillComponent 查询 GetSkillWheelInfo
    SC->>GI: 滑动 OnSkillDirChange
    UI->>SC: OnPointerUp
    SC->>GM: SendUserSkillReq(dir, isLongPress, btnType)
    GM->>GM: InputVKey → GetEntityCtr(playerId)
    GM->>PCG: player.InputVKey(vkey, arg)
    PCG->>PCG: M_Curr(EntityBase) → skillDispatcher
    SD->>Ctrl: SkillController.UseSkill / ClientUseSkill
    Ctrl->>SE: SkillEntity.ClientUseSkill / EnterCurStage
    SE->>SE: SkillContainer / SkillInfo 驱动当前技能数据
    SE->>SS: 创建运行时 Stage 实例
    loop 每帧 Tick(dt)
        Ctrl->>SS: Tick(dt)
        SS->>SH: Handle(timeLineFrame)
        SH->>SH: 解析 TimeLineStage 帧事件
        SH->>EU: TryPlayEffect → TryPlayServerEffect / FuncOnTryPlayClientEffect
        EU->>L4: HandleEffectDamage / SkillBuff.Create / SkillBullet.Create / PassiveSkillEntity.CreatePassiveInfo
        L4-->>EU: 阶段结果 / 目标表现回流
    end
    Note over Ctrl,L4: SkillControllerSkillPartial(85KB) 驱动状态机<br/>SkillEntityActionPartial(76KB) 执行动作/位移/打击
```

## 链路要点

1. **输入入口**：技能按钮 → SkillComponent（不继承 PlayerComponent，由 PlayerCtrlGroup.g_SkillComponent 显式持有）→ GameManager.InputVKey → PlayerCtrlGroup。
2. **调度分发**：当前代码中 `SkillDispatcher.EnterFrame()` 每帧驱动 `SkillController.EnterFrame()` 与 `SkillUnitController.EnterFrame()`；施法入口已验证到 `SkillController.UseSkill()` / `ClientUseSkill()`。
3. **Timeline 主轴**：SkillStage 持有 TimeLineStage[] → StageHandle 逐帧解析 → `PlayStageEffect()` / `TryPlayEffect()`。
4. **效果落地**：StageHandle 通过 `RegisterServerEffect()` / `TryPlayServerEffect()` / `FuncOnTryPlayClientEffect` 进入效果逻辑；伤害落地已验证到 `EffectUtils.HandleEffectDamage()`，Buff/Bullet/Passive 分别由 `SkillBuff.Create()`、`SkillBullet.Create()`、`PassiveSkillEntity.CreatePassiveInfo()` 等真实方法承接。
5. **partial 扩展**：SkillControllerSkillPartial 驱动技能状态机；SkillEntityActionPartial 执行底层动作（动画/位移/打击点）。

## 涉及文件（按层）

| 层 | 关键文件 |
|----|---------|
| L0 | GameInput.cs, GameManager.cs |
| L2 | PlayerCtrlGroup.cs, SkillComponent.cs |
| L3 | SkillDispatcher.cs, SkillUnitController.cs, SkillController.cs, SkillEntity.cs, SkillStage.cs, StageHandle.cs, EffectUtils.cs |
| L3b | SkillControllerSkillPartial.cs, SkillEntityActionPartial.cs, SkillEntityUserInputPartial.cs |
| L4 | SkillBuff.cs, SkillBullet.cs, PassiveInfo.cs |

# Scope

Reviewed the Game subdirectories previously listed as outside the 200-file layer statistics:

- `Assets/Scripts/StarGame/Game/ClientNpc/` — 9 files
- `Assets/Scripts/StarGame/Game/Data/` — 8 files
- `Assets/Scripts/StarGame/Game/Object/` — 2 files
- `Assets/Scripts/StarGame/Game/Partner/` — 1 file

# Result

These files are not empty leftovers. All four supplemental areas are now folded into existing layer docs: `ClientNpc` into `L5_SUPPORT_SYS.md`, `Data` into `L1_ENTITY_RUNTIME_LAYER.md`, and `Object` / `Partner` into `L2_CONTROL_LAYER.md`.

| area | classification | evidence |
|---|---|---|
| `ClientNpc` | Folded into L5 Map support: map-config-driven client NPC trigger/state machine with runtime hooks into scene NPC behavior. | `AppMain.InitServices()` calls `ClientNpcManager.Init()` at `AppMain.cs:749-813`; `ClientNpcManager.OnSceneMapLoadComplete()` creates `ClientNpc` from `GameMap.sceneJsonData.Npcs/Triggers` at `ClientNpcManager.cs:44-55`; `GameNPCCtrlGroup.OnTemplateCreateFinifh()` binds AOI through `ClientNpcManager.OnEnterAOI(this)` at `GameNPCCtrlGroup.cs:156-175`; `StarWorldGame.OnEnterFrame()` calls `ClientNpcManager.EnterFrame(frameId)` at `StarWorldGame.cs:334-341`; `ClientNpc.EnterFrame()` runs triggers and current state at `ClientNpc.cs:237-261`. |
| `Data` | Folded into L1 runtime: entity/AOI/battle-state data substrate shared by L1 runtime, L2 control, and L3/L4 skill effects. | `EntityBaseData.RegisterAttribute()` is the C# attr callback registration point at `EntityBaseData.cs:189-200`; `UpdateWithAttr()` / `UpdatePropList()` process AOI sync props at `EntityBaseData.cs:364-488`; `HandleClientBattleStates()` updates client battle-state masks at `EntityBaseData.cs:1173-1204`; `GameManager.UpdateEntityData()` calls `UpdateWithAttr()` at `GameManager.cs:1428-1431`; `TriggerBuffChange()` emits buff change data at `EntityBaseData.cs:1609-1620`. |
| `Object` | Folded into L2 control: non-vital interactable object control branch, closest to L2 control + L1 runtime object view. | `GameManager.CreateEntity()` routes `E_EntityType.Interact` to `CreateInterActionObject()` at `GameManager.cs:3053-3112`; `CreateInterActionObject()` creates `ObjectCtrlGroup` at `GameManager.cs:3179-3186`; `ObjectCtrlGroup.Create()` binds `NoneVitalSignData`, creates `ObstacleBase`, and loads object model at `ObjectCtrlGroup.cs:159-212`; `ObjectUnitPendant` creates AOI/local overhead UI at `ObjectUnitPendant.cs:56-126`. |
| `Partner` | Folded into L2 control: partner roster/concretization manager, adjacent to L2 `PartnerCtrlGroup` rather than a standalone combat loop. | `AppMain.InitServices()` calls `PartnerManager.Init()` at `AppMain.cs:749-813`; `PartnerManager.AddEventListener()` registers partner network/global events at `PartnerManager.cs:41-57`; `PartnerCtrlGroup.CreateSkillComponent()` / `RomveSkillComponent()` raise partner create/leave events at `PartnerCtrlGroup.cs:160-187`; `BattleManager.GetCanUsePartner()` consumes `GetInPlayedPartners()` at `BattleManager.cs:3597-3709`. |

# Follow-up

- Main coverage count is now 220 files: original 200 plus ClientNpc 9, Data 8, Object 2, PartnerManager 1.
- Keep these supplemental areas folded into existing L1/L2/L5 docs unless a later review needs method-level diagrams for one area.

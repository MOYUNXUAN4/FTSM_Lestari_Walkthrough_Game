---
id: kd_943a2f53-a389-4df9-a8a3-2b43e56d7a35
type: memory
path: unity-project-understanding/bk1-room-interactions.md
title: bk1-room-interactions
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779504551774
updatedAt: 1779793403966
---

# bk1-room-interactions

## Summary
BK1 scene interaction, giant-scale movement, door, crosshair, AC behavior, dual-switch row lighting, poster placement, MP1-style ceiling-light setup cache, GAME fixture material fix, GAME root prefab restoration, MP1 independent light switches, and MP1 teacher-to-student computer linking

<!-- locus:maintain-rules:start -->
- Record only Unity project structure knowledge and lookup info that reduce repeated exploration
- Maintain only project-derived engineering understanding, including directory responsibilities, system entry points, asset relationships, runtime entry points, and config mappings
- Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
- Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
- Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
- Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/BK1.unity` now mirrors the MP1 room-interaction pattern using `FTSMRoomInteractable` on scene objects directly.
- BK1 interactables are on layer `Interactable` (layer 15) and are driven by `PC VR Simulator Player` with `FTSMPlayerController` using `interactMask = 32768`.
- BK1 currently has these room interactions:
  - `computer_new` toggles monitor screen off via `FTSMRoomInteractable` with `interactionType = ToggleComputer`, target renderer `New Monitor`, `screenMaterialIndex = 1`.
  - `wall_tv` toggles screen off via `FTSMRoomInteractable` with `interactionType = ToggleComputer`, target renderer `wall_tv`, `screenMaterialIndex = 1`.
  - `LightSwitches01` controls `air_conditioner` via linked `FTSMRoomInteractable`.
  - BK1 AC follows the MP1 setup more closely: `air_conditioner` animates `AC B`, drives only `AC_Status_LED` renderer/light, and leaves the AC shell renderer untouched; `AC_AirGlow` is inactive.
  - `LightSwitches02/LightSwitches02/LightSwitches02_Left` controls front-row ceiling lights `BK1_Rect_Ceiling_Light_01..03`.
  - `LightSwitches02/LightSwitches02/LightSwitches02_Right` controls back-row ceiling lights `BK1_Rect_Ceiling_Light_04..06`.
  - The active BK1 row-light switches also drive the `Fixture` renderers for each controlled row, with off-state multiplier `1.0` and on-state multiplier `1.35` so the fixture visibly brightens when lights are on while preserving the current off color.
  - Legacy single switch `LightSwitches01 (1)` is no longer the active BK1 light control.
  - `door_c` and `door_c (1)` use `FTSMRoomInteractable` door mode with MK1-style swing values.
- `Assets/FTSMRoomInteractable.cs` now supports separate `lightOffRendererMultiplier` and `lightOnRendererMultiplier`, allowing light-controlled renderers to keep their off appearance while brightening above baseline when on.
- BK1 ambient lighting is intentionally darker now so lamp toggling reads more clearly; brightening is primarily from six ceiling fixtures under `BK1_Ceiling_Lights` plus a reduced directional fill.
- `Assets/BlueIndicatorLight.mat` is used as the AC blue status LED material in BK1.
- BK1 player is tuned for giant-scene units: `PC VR Simulator Player` at `(-7000, 0, 12000)`, `CharacterController` height/radius/center = `1700 / 350 / 850`, `moveSpeed = 18000`, `rayDistance = 24000`, `teleportMask = Default(0)`, `interactMask = Interactable(15)`, `Main Camera` local position `(0, 1500, 0)`.
- BK1 wall switches use per-switch colliders on the active visible toggle pieces so left-click ray interaction can hit individual switch buttons.
- BK1 ceiling lighting no longer uses `BK1_LightCluster`; it now uses six generated `BK1_Rect_Ceiling_Light_01..06` fixtures under `BK1_Ceiling_Lights`, each with `Fixture`, `Soft_Light`, `Down_Spot_Light`, and `Ceiling_Fill_Light` children to approximate the large bright MP1 ceiling lights.
- BK1 main camera has `Crosshair Canvas/Crosshair` UI like Staff Office for centered aiming feedback.
- BK1 includes `MP1_Poster_Display` using `Assets/Poster/Materials/MP1_POSTER_Display.mat`, currently placed on the front wall at approximately `(-7200, 3500, -7670)` with large wall-mounted quad scaling.
- In `Assets/Scenes/GAME.unity`, BK1 and MP1 ceiling light `Fixture` MeshRenderers were missing `Materials[0]`; fixed by assigning `Assets/LocusGenerated/Materials/SoftCeilingLight.mat` back onto the `Fixture` MeshRenderers in `Assets/Prefabs/BK1_Room.prefab` and `Assets/Prefabs/MP1_Room.prefab`, which propagated to the GAME scene instances.
- In `Assets/Scenes/GAME.unity`, MP1 light switches under `MP1_Room/LightSwitches03` control columns `C01`, `C02`, and `C03` independently. Each switch has no `linkedInteractables`; the prior all-columns linked setup was reverted after the user confirmed the visual difference and requested separate left/middle/right control again.
- In `Assets/Scenes/GAME.unity`, MP1 teacher computer is `MP1_Room/DrDesk` and student computers are the 33 `FTSMRoomInteractable` objects named `computer_new*` under `MP1_Room/StudentTable`. `DrDesk.linkedInteractables` points to all 33 student computers so toggling the teacher computer propagates the same computer on/off state to student monitors.
- After an accidental overwrite emptied `Assets/Scenes/GAME.unity`, the scene was rebuilt with root prefab instances `BK1_Room` at origin, `MP1_Room` at local position `(30914, -26, 2381)` and local scale `(29.35499, 29.35499, 29.35499)`, plus `Staff_Office_Group` at origin; these match the verified prior GAME scene hierarchy and MP1 transform overrides that were observed before the overwrite.
<!-- locus:body:end -->
